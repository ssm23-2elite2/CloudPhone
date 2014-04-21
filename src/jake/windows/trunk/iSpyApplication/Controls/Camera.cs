using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Video;
using AForge.Vision.Motion;
using iSpyApplication.Video;
using Point = System.Drawing.Point;

namespace iSpyApplication.Controls
{
    /// <summary>
    /// Camera class
    /// </summary>
    public class Camera : IDisposable
    {
        public CameraWindow CW;
        public bool MotionDetected;

        private bool _motionRecentlyDetected;
        public bool MotionRecentlyDetected
        {
            get
            {
                bool b = _motionRecentlyDetected;
                _motionRecentlyDetected = false;
                return MotionDetected || b;
            }
        }
        public float MotionLevel;

        public Rectangle[] MotionZoneRectangles;
        public IVideoSource VideoSource;
        public double Framerate;
        public double RealFramerate;
        private Queue<double> _framerates;
        private HSLFiltering _filter;
        private volatile bool _requestedToStop;
        private readonly object _sync = new object();
        private MotionDetector _motionDetector;
        private int _processFrameCount;
        private DateTime _motionlastdetected = DateTime.MinValue;
        private DateTime _nextFrameTarget = DateTime.MinValue;
        private readonly FishEyeCorrect _feCorrect = new FishEyeCorrect();

        // alarm level
        private double _alarmLevel = 0.0005;
        private double _alarmLevelMax = 1;
        private int _height = -1;
        private DateTime _lastframeProcessed = DateTime.MinValue;
        private DateTime _lastframeEvent = DateTime.MinValue;

        private int _width = -1;
        private bool _pluginTrigger;
        private Brush _foreBrush, _backBrush;
        private Font _drawfont;

        //digital controls
        public float ZFactor = 1;
        public Point ZPoint = Point.Empty;

        public event Delegates.ErrorHandler ErrorHandler;

        public HSLFiltering Filter
        {
            get
            {
                if (CW.Camobject.detector.colourprocessingenabled)
                {
                    if (_filter != null)
                        return _filter;
                    if (!String.IsNullOrEmpty(CW.Camobject.detector.colourprocessing))
                    {
                        string[] config = CW.Camobject.detector.colourprocessing.Split(CW.Camobject.detector.colourprocessing.IndexOf("|", StringComparison.Ordinal) != -1 ? '|' : ',');
                        _filter = new HSLFiltering
                                      {
                                          FillColor =
                                              new HSL(Convert.ToInt32(config[2]), float.Parse(config[5]),
                                                      float.Parse(config[8])),
                                          FillOutsideRange = Convert.ToInt32(config[9])==0,
                                          Hue = new IntRange(Convert.ToInt32(config[0]), Convert.ToInt32(config[1])),
                                          Saturation = new Range(float.Parse(config[3]), float.Parse(config[4])),
                                          Luminance = new Range(float.Parse(config[6]), float.Parse(config[7])),
                                          UpdateHue = Convert.ToBoolean(config[10]),
                                          UpdateSaturation = Convert.ToBoolean(config[11]),
                                          UpdateLuminance = Convert.ToBoolean(config[12])

                        };
                    }
                    
                }

                return null;
            }
        }

        internal Rectangle ViewRectangle
        {
            get
            {
                int newWidth = Convert.ToInt32(Width / ZFactor);
                int newHeight = Convert.ToInt32(Height / ZFactor);

                int left = ZPoint.X - newWidth / 2;
                int top = ZPoint.Y - newHeight / 2;
                int right = ZPoint.X + newWidth / 2;
                int bot = ZPoint.Y + newHeight / 2;

                if (left < 0)
                {
                    right += (0 - left);
                    left = 0;
                }
                if (right > Width)
                {
                    left -= (right - Width);
                    right = Width;
                }
                if (top < 0)
                {
                    bot += (0 - top);
                    top = 0;
                }
                if (bot > Height)
                {
                    top -= (bot - Height);
                    bot = Height;
                }

                ZPoint.X = left + (right - left) / 2;
                ZPoint.Y = top + (bot - top) / 2;

                return new Rectangle(left, top, right - left, bot - top);
            }
        }

        private object _plugin;
        public object Plugin
        {
            get
            {
                if (_plugin == null)
                {
                    foreach (string p in MainForm.Plugins)
                    {
                        if (p.EndsWith("\\" + CW.Camobject.alerts.mode + ".dll", StringComparison.CurrentCultureIgnoreCase))
                        {
                            Assembly ass = Assembly.LoadFrom(p);
                            Plugin = ass.CreateInstance("Plugins.Main", true);
                            if (_plugin != null)
                            {
                                try
                                {
                                    var o = _plugin.GetType();
                                    if (o.GetProperty("WorkingDirectory") != null)
                                        o.GetProperty("WorkingDirectory").SetValue(_plugin, Program.AppDataPath, null);
                                    if (o.GetProperty("VideoSource") != null)
                                        o.GetProperty("VideoSource").SetValue(_plugin, CW.Camobject.settings.videosourcestring, null);
                                    if (o.GetProperty("Configuration") != null)
                                        o.GetProperty("Configuration").SetValue(_plugin,CW.Camobject.alerts.pluginconfig,null);

                                    if (o.GetMethod("LoadConfiguration") != null)
                                        o.GetMethod("LoadConfiguration").Invoke(_plugin, null);
                                    
                                    if (o.GetProperty("DeviceList")!=null)
                                    {
                                        //used for network kinect setting syncing
                                        string dl = "";

                                        //build a pipe and star delimited string of all cameras that are using the kinect plugin
                                        foreach(var oc in MainForm.Cameras)
                                        {
                                            string s = oc.settings.namevaluesettings;
                                            if (!String.IsNullOrEmpty(s))
                                            {
                                                //we're only looking for ispykinect devices
                                                if (s.ToLower().IndexOf("custom=network kinect", StringComparison.Ordinal) != -1)
                                                {
                                                    dl += oc.name.Replace("*","").Replace("|","") + "|" + oc.id + "|" + oc.settings.videosourcestring + "*";
                                                }
                                            }
                                        }
                                        //the ispykinect plugin takes this delimited list and uses it for copying settings
                                        if (dl!="")
                                            o.GetProperty("DeviceList").SetValue(_plugin, dl, null);
                                    }

                                    if (o.GetProperty("CameraName") != null)
                                        o.GetProperty("CameraName").SetValue(_plugin, CW.Camobject.name, null);

                                    var l = o.GetMethod("LogExternal");
                                    if (l != null)
                                    {
                                        l.Invoke(_plugin, new object[] { "Plugin Initialised" });
                                    }
                                }
                                catch (Exception ex)
                                {
                                    //config corrupted
                                    if (ErrorHandler != null)
                                        ErrorHandler("Error configuring plugin - trying with a blank configuration (" + ex.Message + ")");
                    
                                    CW.Camobject.alerts.pluginconfig = "";
                                    _plugin.GetType().GetProperty("Configuration").SetValue(_plugin,
                                                                                            CW.Camobject.alerts.
                                                                                                pluginconfig,
                                                                                            null);
                                }
                            }
                            break;
                        }
                    }
                }
                return _plugin;
            }
            set
            {
                if (_plugin != null)
                {
                    try {_plugin.GetType().GetMethod("Dispose").Invoke(_plugin, null);} catch {}
                }
                _plugin = value;
            }
        }

        public void FilterChanged()
        {
            lock (_sync)
            {
                _filter = null;
            }

        }
        
        public Camera() : this(null, null)
        {
        }

        public Camera(IVideoSource source)
        {
            VideoSource = source;
            _motionDetector = null;
            VideoSource.NewFrame += VideoNewFrame;
        }

        public Camera(IVideoSource source, MotionDetector detector)
        {
            VideoSource = source;
            _motionDetector = detector;
            VideoSource.NewFrame += VideoNewFrame;
        }


        // Running property
        public bool IsRunning
        {
            get { return (VideoSource != null) && VideoSource.IsRunning; }
        }

        public void WaitForStop()
        {
            if (VideoSource!=null)
                VideoSource.WaitForStop();
        }

        //


        // Width property
        public int Width
        {
            get { return _width; }
        }

        // Height property
        public int Height
        {
            get { return _height; }
        }

        // AlarmLevel property
        public double AlarmLevel
        {
            get { return _alarmLevel; }
            set { _alarmLevel = value; }
        }

        // AlarmLevel property
        public double AlarmLevelMax
        {
            get { return _alarmLevelMax; }
            set { _alarmLevelMax = value; }
        }

        // FramesReceived property
        public int FramesReceived
        {
            get { return (VideoSource == null) ? 0 : VideoSource.FramesReceived; }
        }

        // BytesReceived property
        public long BytesReceived
        {
            get { return (VideoSource == null) ? 0 : VideoSource.BytesReceived; }
        }

        // motionDetector property
        public MotionDetector MotionDetector
        {
            get { return _motionDetector; }
            set
            {
                _motionDetector = value;
                if (value != null) _motionDetector.MotionZones = MotionZoneRectangles;
            }
        }

        public Bitmap Mask { get; set; }

        public bool SetMotionZones(objectsCameraDetectorZone[] zones)
        {
            if (zones == null || zones.Length == 0)
            {
                ClearMotionZones();
                return true;
            }
            //rectangles come in as percentages to allow resizing and resolution changes

            if (_width > -1)
            {
                double wmulti = Convert.ToDouble(_width)/Convert.ToDouble(100);
                double hmulti = Convert.ToDouble(_height)/Convert.ToDouble(100);
                MotionZoneRectangles = zones.Select(r => new Rectangle(Convert.ToInt32(r.left*wmulti), Convert.ToInt32(r.top*hmulti), Convert.ToInt32(r.width*wmulti), Convert.ToInt32(r.height*hmulti))).ToArray();
                if (_motionDetector != null)
                    _motionDetector.MotionZones = MotionZoneRectangles;
                return true;
            }
            return false;
        }

        public void ClearMotionZones()
        {
            MotionZoneRectangles = null;
            if (_motionDetector != null)
                _motionDetector.MotionZones = null;
        }

        public event NewFrameEventHandler NewFrame;
        public event EventHandler Alarm;
        public event PlayingFinishedEventHandler PlayingFinished;

        // Constructor

        // Start video source
        public void Start()
        {
            if (VideoSource != null)
            {
                _requestedToStop = false;
                _framerates = new Queue<double>();
                _lastframeProcessed = DateTime.MinValue;
                _lastframeEvent = DateTime.MinValue;
                _motionRecentlyDetected = false;
                if (!CW.IsClone)
                {
                    VideoSource.PlayingFinished -= VideoSourcePlayingFinished;
                    VideoSource.PlayingFinished += VideoSourcePlayingFinished;
                    VideoSource.Start();
                    
                }
            }
        }

        // Signal video source to stop
        public void SignalToStop()
        {
            if (CW.IsClone || _requestedToStop)
                return;
            if (VideoSource != null)
            {
                _requestedToStop = true;
                VideoSource.SignalToStop();
            }
            _motionRecentlyDetected = false;
        }


        private double Mininterval
        {
            get
            {
                return 1000d/MaxFramerate;
            }
        }

        private double MaxFramerate
        {
            get
            {
                var ret = CW.Camobject.settings.maxframerate;
                if (CW.Recording && CW.Camobject.recorder.profile < 3) //use variable rate for mp4 only
                    ret = CW.Camobject.settings.maxframeraterecord;


                if (MainForm.ThrottleFramerate < ret)
                    ret = MainForm.ThrottleFramerate;
                return ret;
            }
        }

        internal RotateFlipType RotateFlipType = RotateFlipType.RotateNoneFlipNone;

        private void VideoNewFrame(object sender, NewFrameEventArgs e)
        {
            if (_requestedToStop || NewFrame==null)
                return;
    
            
            if (_lastframeEvent > DateTime.MinValue)
            {
                if ((Helper.Now<_nextFrameTarget))
                {
                    return;
                }
                CalculateFramerates();
            }
            else
            {
                _lastframeEvent = Helper.Now;
            }
            
            Bitmap bmOrig = null;
            bool bMotion = false;
            lock (_sync)
            {
                _lastframeProcessed = Helper.Now;               
                try
                {
                    if (e.Frame != null)
                    {
                        bmOrig = ResizeBmOrig(e);

                        if (CW.Camobject.settings.FishEyeCorrect)
                            bmOrig = _feCorrect.Correct(bmOrig, CW.Camobject.settings.FishEyeFocalLengthPX,CW.Camobject.settings.FishEyeLimit, CW.Camobject.settings.FishEyeScale);                       

                        if (RotateFlipType != RotateFlipType.RotateNoneFlipNone)
                        {
                            bmOrig.RotateFlip(RotateFlipType);                           
                        }
                         
                        _width = bmOrig.Width;
                        _height = bmOrig.Height;

                        if (CW.NeedMotionZones)
                            CW.NeedMotionZones = !SetMotionZones(CW.Camobject.detector.motionzones);

                        if (Mask != null)
                        {
                            ApplyMask(bmOrig);
                        }

                        if (CW.Camobject.alerts.active && Plugin != null && Alarm!=null)
                        {
                            bmOrig = RunPlugin(bmOrig);
                        }

                        
                        //this converts the image into a windows displayable image so do it regardless
                        using (var lfu = UnmanagedImage.FromManagedImage(bmOrig))
                        {
                            if (_motionDetector != null)
                            {
                                bMotion = ApplyMotionDetector(lfu);
                            }
                            else
                            {
                                MotionDetected = false;
                            }

                            if (ZFactor > 1)
                            {
                                using (var zlfu = ZoomImage(lfu))
                                {
                                    bmOrig = zlfu.ToManagedImage();
                                }
                            }
                            else
                            {
                                bmOrig = lfu.ToManagedImage();
                            }                            
                        }

                        AddTimestamp(bmOrig);                       
                    }
                }
                catch (UnsupportedImageFormatException ex)
                {
                    CW.VideoSourceErrorState = true;
                    CW.VideoSourceErrorMessage = ex.Message;

                    if (bmOrig != null)
                        bmOrig.Dispose();

                    return;
                }
                catch (Exception ex)
                {
                    if (bmOrig != null)
                        bmOrig.Dispose();

                    if (ErrorHandler != null)
                        ErrorHandler(ex.Message);
                    
                    return;
                }


                if (MotionDetector != null && !CW.Calibrating && MotionDetector.MotionProcessingAlgorithm is BlobCountingObjectsProcessing && !CW.PTZNavigate && CW.Camobject.settings.ptzautotrack)
                {
                    try
                    {
                        ProcessAutoTracking();
                    }
                    catch (Exception ex)
                    {
                        if (ErrorHandler != null)
                            ErrorHandler(ex.Message);
                    }
                }
            }

            if (NewFrame != null && !_requestedToStop && bmOrig!=null)
            {
                NewFrame(this, new NewFrameEventArgs(bmOrig));
            }
            if (bMotion)
            {
                TriggerDetect(this);
            }

        }

        [HandleProcessCorruptedStateExceptions]
        private UnmanagedImage ZoomImage(UnmanagedImage lfu)
        {
            if (ZFactor > 1)
            {
                var f1 = new ResizeNearestNeighbor(lfu.Width, lfu.Height);
                var f2 = new Crop(ViewRectangle);
                try
                {
                    lfu = f2.Apply(lfu);
                    lfu = f1.Apply(lfu);
                }
                catch (Exception ex)
                {
                    if (ErrorHandler != null)
                        ErrorHandler(ex.Message);
                }
            }

            return lfu;
        }

        private void AddTimestamp(Bitmap bmp)
        {
            if (CW.Camobject.settings.timestamplocation != 0 &&
                !String.IsNullOrEmpty(CW.Camobject.settings.timestampformatter))
            {
                using (Graphics gCam = Graphics.FromImage(bmp))
                {

                    var ts = CW.Camobject.settings.timestampformatter.Replace("{FPS}",
                        string.Format("{0:F2}", Framerate));
                    ts = ts.Replace("{CAMERA}", CW.Camobject.name);
                    ts = ts.Replace("{REC}", CW.Recording ? "REC" : "");

                    var timestamp = "Invalid Timestamp";
                    try
                    {
                        timestamp = String.Format(ts,
                            DateTime.Now.AddHours(
                                Convert.ToDouble(CW.Camobject.settings.timestampoffset))).Trim();
                    }
                    catch
                    {
                    }

                    var rs = gCam.MeasureString(timestamp, DrawFont).ToSize();
                    rs.Width += 5;
                    var p = new Point(0, 0);
                    switch (CW.Camobject.settings.timestamplocation)
                    {
                        case 2:
                            p.X = _width/2 - (rs.Width/2);
                            break;
                        case 3:
                            p.X = _width - rs.Width;
                            break;
                        case 4:
                            p.Y = _height - rs.Height;
                            break;
                        case 5:
                            p.Y = _height - rs.Height;
                            p.X = _width/2 - (rs.Width/2);
                            break;
                        case 6:
                            p.Y = _height - rs.Height;
                            p.X = _width - rs.Width;
                            break;
                    }
                    if (CW.Camobject.settings.timestampshowback)
                    {
                        var rect = new Rectangle(p, rs);
                        gCam.FillRectangle(BackBrush, rect);
                    }
                    gCam.DrawString(timestamp, DrawFont, ForeBrush, p);
                }
            }
        }

        private void ProcessAutoTracking()
        {
            var blobcounter =
                (BlobCountingObjectsProcessing) MotionDetector.MotionProcessingAlgorithm;

            //tracking

            if (blobcounter.ObjectsCount > 0 && blobcounter.ObjectsCount < 4 && !CW.Ptzneedsstop)
            {
                var pCenter = new Point(Width/2, Height/2);
                Rectangle rec = blobcounter.ObjectRectangles.OrderByDescending(p => p.Width*p.Height).First();
                //get center point
                var prec = new Point(rec.X + rec.Width/2, rec.Y + rec.Height/2);

                double dratiomin = 0.6;
                prec.X = prec.X - pCenter.X;
                prec.Y = prec.Y - pCenter.Y;

                if (CW.Camobject.settings.ptzautotrackmode == 1) //vert only
                {
                    prec.X = 0;
                    dratiomin = 0.3;
                }

                if (CW.Camobject.settings.ptzautotrackmode == 2) //horiz only
                {
                    prec.Y = 0;
                    dratiomin = 0.3;
                }

                double angle = Math.Atan2(-prec.Y, -prec.X);
                if (CW.Camobject.settings.ptzautotrackreverse)
                {
                    angle = angle - Math.PI;
                    if (angle < 0 - Math.PI)
                        angle += 2*Math.PI;
                }
                double dist = Math.Sqrt(Math.Pow(prec.X, 2.0d) + Math.Pow(prec.Y, 2.0d));

                double maxdist = Math.Sqrt(Math.Pow(Width/2d, 2.0d) + Math.Pow(Height/2d, 2.0d));
                double dratio = dist/maxdist;

                if (dratio > dratiomin)
                {
                    CW.PTZ.SendPTZDirection(angle, 1);
                    CW.LastAutoTrackSent = Helper.Now;
                    CW.Ptzneedsstop = true;
                }
            }
        }

        [HandleProcessCorruptedStateExceptions] 
        private bool ApplyMotionDetector(UnmanagedImage lfu)
        {
            if (Alarm != null && lfu!=null)
            {
                _processFrameCount++;
                if (_processFrameCount >= CW.Camobject.detector.processeveryframe || CW.Calibrating)
                {
                    _processFrameCount = 0;
                    
                    try
                    {
                        MotionLevel = _motionDetector.ProcessFrame(Filter != null ? Filter.Apply(lfu) : lfu);
                    }
                    catch(Exception ex)
                    {
                        throw new Exception("Error processing motion: "+ex.Message);
                    }

                    if (MotionLevel >= _alarmLevel)
                    {
                        if (MotionLevel <= _alarmLevelMax || _alarmLevelMax >= 0.1)
                        {
                            return true;
                        }
                    }
                    else
                        MotionDetected = false;
                }
            }
            else
                MotionDetected = false;
            return false;
        }

        internal void TriggerDetect(object sender)
        {
            MotionDetected = true;
            _motionlastdetected = Helper.Now;
            _motionRecentlyDetected = true;
            if (Alarm!=null)
                Alarm(sender, new EventArgs());
        }

        internal void TriggerPlugin()
        {
            _pluginTrigger = true;
        }

        

        private Bitmap ResizeBmOrig(NewFrameEventArgs e)
        {
            if (CW.Camobject.settings.resize &&
                (CW.Camobject.settings.desktopresizewidth != e.Frame.Width ||
                 CW.Camobject.settings.desktopresizeheight != e.Frame.Height))
            {

                var result = new Bitmap(CW.Camobject.settings.desktopresizewidth, CW.Camobject.settings.desktopresizeheight,
                                        PixelFormat.Format24bppRgb);
                try
                {
                    using (Graphics g2 = Graphics.FromImage(result))
                    {
                        g2.CompositingMode = CompositingMode.SourceCopy;
                        g2.CompositingQuality = CompositingQuality.HighSpeed;
                        g2.PixelOffsetMode = PixelOffsetMode.Half;
                        g2.SmoothingMode = SmoothingMode.None;
                        g2.InterpolationMode = InterpolationMode.Default;
                        //g2.GdiDrawImage(e.Frame, 0, 0, result.Width, result.Height);
                        g2.DrawImage(e.Frame, 0, 0, result.Width, result.Height);
                    }
                    return result;
                }
                catch
                {
                    result.Dispose();
                    return e.Frame;
                }
                
            }
            return e.Frame;
        }

        private void CalculateFramerates()
        {
            double dMin = Mininterval;
            _nextFrameTarget = _nextFrameTarget.AddMilliseconds(dMin);
            var d = Helper.Now;
            if (_nextFrameTarget < d)
                _nextFrameTarget = d.AddMilliseconds(dMin);


            TimeSpan tsFr = d - _lastframeProcessed;
            _framerates.Enqueue(1000d/tsFr.TotalMilliseconds);
            if (_framerates.Count >= 30)
                _framerates.Dequeue();
            Framerate = _framerates.Average();
        }

        private void ApplyMask(Bitmap bmOrig)
        {
            Graphics             g = Graphics.FromImage(bmOrig);
            g.CompositingMode = CompositingMode.SourceOver;//.SourceCopy;
            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.SmoothingMode = SmoothingMode.None;
            g.InterpolationMode = InterpolationMode.Default;
            g.DrawImage(Mask, 0, 0, _width, _height);
            //g.GdiDrawImage(Mask, 0, 0, _width, _height);
            g.Dispose();
        }

        public volatile bool PluginRunning = false;

        private Bitmap RunPlugin(Bitmap bmOrig)
        {
            if (!CW.IsEnabled)
                return bmOrig;
            bool runplugin = true;
            switch (CW.Camobject.alerts.processmode)
            {
                case "motion":
                    //only run plugin if motion detected within last 3 seconds
                    runplugin = _motionlastdetected > Helper.Now.AddSeconds(-3);
                    break;
                case "trigger":
                    //only run plugin if triggered and then reset trigger
                    runplugin = _pluginTrigger;
                    _pluginTrigger = false;
                    break;
            }
            
            if (runplugin)
            {
                PluginRunning = true;
                var o = _plugin.GetType();

                try
                {
                    //pass and retrieve the latest bitmap from the plugin
                    bmOrig = (Bitmap) o.GetMethod("ProcessFrame").Invoke(Plugin, new object[] {bmOrig});
                }
                catch (Exception ex)
                {
                    if (ErrorHandler != null)
                        ErrorHandler(ex.Message);
                }
               
                //check the plugin alert flag and alarm if it is set
                var pluginAlert = (String) o.GetField("Alert").GetValue(Plugin);
                if (pluginAlert != "" && Alarm!=null)
                    Alarm(pluginAlert, EventArgs.Empty);
                
                //reset the plugin alert flag if it supports that
                if (o.GetMethod("ResetAlert") != null)
                    o.GetMethod("ResetAlert").Invoke(_plugin, null);

                PluginRunning = false;
            }
            return bmOrig;
        }

        
        public Font DrawFont
        {
            get
            {
                if (_drawfont!=null)
                    return _drawfont;
                _drawfont = FontXmlConverter.ConvertToFont(CW.Camobject.settings.timestampfont);
                return _drawfont;
            }
            set { _drawfont = value; }
        }
        public Brush ForeBrush
        {
            get
            {
                if (_foreBrush!=null)
                    return _foreBrush;
                Color c = CW.Camobject.settings.timestampforecolor.ToColor();
                _foreBrush = new SolidBrush(Color.FromArgb(255,c.R,c.G,c.B));
                return _foreBrush;
            }
            set { _foreBrush = value; }
        }
        public Brush BackBrush
        {
            get
            {
                if (_backBrush != null)
                    return _backBrush;
                Color c = CW.Camobject.settings.timestampbackcolor.ToColor();
                _backBrush = new SolidBrush(Color.FromArgb(128, c.R, c.G, c.B));
                return _backBrush;
            }
            set { _backBrush = value; }
        }

        private bool _disposing;

        public void Dispose()
        {
            if (_disposing)
                return;

            
            _disposing = true;
            lock (_sync)
            {                  
                ClearMotionZones();
                ForeBrush.Dispose();
                BackBrush.Dispose();
                DrawFont.Dispose();
                if (_framerates != null)
                    _framerates.Clear();

                if (Mask != null)
                {
                    Mask.Dispose();
                    Mask = null;
                }
                Alarm = null;
                NewFrame = null;
                PlayingFinished = null;
                Plugin = null;

                VideoSource = null;

                if (MotionDetector != null)
                {
                    try
                    {
                        MotionDetector.Reset();
                    }
                    catch (Exception ex)
                    {
                        if (ErrorHandler != null)
                            ErrorHandler(ex.Message);
                    }
                    MotionDetector = null;
                }
            }
        }

        void VideoSourcePlayingFinished(object sender, ReasonToFinishPlaying reason)
        {
            if (PlayingFinished != null)
                PlayingFinished(sender, reason);
        }
    }
}