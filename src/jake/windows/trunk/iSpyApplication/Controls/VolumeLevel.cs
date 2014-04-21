using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using AForge.Video;
using iSpyApplication.Audio;
using iSpyApplication.Audio.streams;
using iSpyApplication.MP3Stream;
using NAudio.Wave;
using iSpy.Video.FFMPEG;
using utils;
using PictureBox = AForge.Controls.PictureBox;
using WaveFormat = NAudio.Wave.WaveFormat;

namespace iSpyApplication.Controls
{
    public sealed partial class VolumeLevel : PictureBox, ISpyControl
    {
        #region Private

        public MainForm MainClass;
        private AudioFileWriter _writer;
        private DateTime _mouseMove = DateTime.MinValue;
        public event EventHandler AudioDeviceEnabled;
        private long _lastRun = Helper.Now.Ticks;
        private double _secondCountNew;
        private double _recordingTime;
        private Point _mouseLoc;
        private readonly ManualResetEvent _stopWrite = new ManualResetEvent(false);
        private volatile float[] _levels;
        private readonly ToolTip _toolTipMic;
        private int _ttind = -1;
        private DateTime _errorTime = DateTime.MinValue;
        private DateTime _reconnectTime = DateTime.MinValue;

        private Int64 _lastSoundDetected = Helper.Now.Ticks;
        private Int64 _lastAlerted = Helper.Now.Ticks;

        public DateTime LastSoundDetected
        {
            get { return new DateTime(_lastSoundDetected); }
            set { Interlocked.Exchange(ref _lastSoundDetected, value.Ticks); }
        }

        public DateTime LastAlerted
        {
            get { return new DateTime(_lastAlerted); }
            set { Interlocked.Exchange(ref _lastAlerted, value.Ticks); }
        }

        private bool _soundRecentlyDetected;
        public bool SoundRecentlyDetected
        {
            get
            {
                bool b = _soundRecentlyDetected;
                _soundRecentlyDetected = false;
                return SoundDetected || b;
            }
        }

        private readonly DateTime _lastScheduleCheck = DateTime.MinValue;
        private List<FilesFile> _filelist = new List<FilesFile>();
        private readonly AutoResetEvent _newRecordingFrame = new AutoResetEvent(false);
        private Thread _recordingThread;
        private bool _requestRefresh;
        private readonly Pen _vline = new Pen(Color.Green, 2);
        private readonly object _lockobject = new object();
        public bool ShuttingDown;
        public bool IsClone;
        private bool _pairedRecording;
        public volatile bool IsReconnect;
        private readonly StringBuilder _soundData = new StringBuilder(100000);

        //private AudioStreamer _as = null;
        private WaveFormat _audioStreamFormat;
        private Mp3Writer _mp3Writer;
        private readonly MemoryStream _outStream = new MemoryStream();
        private readonly byte[] _bResampled = new byte[22050];

        private const int ButtonCount = 5;
        private Rectangle ButtonPanel
        {
            get
            {
                int w = ButtonCount * 22 + 3;
                int h = 28;
                if (MainForm.Conf.BigButtons)
                {
                    w = ButtonCount * (31) + 3;
                    h = 34;

                }
                return new Rectangle(Width / 2 - w / 2, Height - 10 - h, w, h);

            }
        }

        #endregion

        #region Public
        public string AudioSourceErrorMessage = "";
        private bool _audioSourceErrorState;
        public bool LoadedFiles;
        internal Color BackgroundColor = MainForm.BackgroundColor;
        public event Delegates.ErrorHandler ErrorHandler;

        public bool AudioSourceErrorState
        {
            get { return _audioSourceErrorState; }
            set { _audioSourceErrorState = value;
            _requestRefresh = true;
            }
        }
        public bool Paired
        {
            get { return Micobject != null && MainForm.Cameras.FirstOrDefault(p => p.settings.micpair == Micobject.id) != null; }
        }
        public CameraWindow CameraControl
        {
            get
            {
                if (Micobject != null && Micobject.id>-1)
                {
                    var oc = MainForm.Cameras.FirstOrDefault(p => p.settings.micpair == Micobject.id);
                    if (oc != null && TopLevelControl != null)
                    {
                        return ((MainForm)TopLevelControl).GetCameraWindow(oc.id);
                    }
                }
                return null;
            }
        }


        #region Events
        public event Delegates.RemoteCommandEventHandler RemoteCommand;
        public event Delegates.NotificationEventHandler Notification;
        public event Delegates.NewDataAvailable DataAvailable;
        public event Delegates.FileListUpdatedEventHandler FileListUpdated;
        #endregion

        public List<AudioAction> AudioBuffer = new List<AudioAction>();
        public QueueWithEvents<AudioAction> WriterBuffer = new QueueWithEvents<AudioAction>();
        public bool Alerted;
        public string AudioFileName = "";
        public Enums.AudioStreamMode AudioStreamMode;
       
        public Rectangle RestoreRect = Rectangle.Empty;
        public DateTime FlashCounter = DateTime.MinValue;
        public bool ForcedRecording;
        public double InactiveRecord;
        public bool IsEdit;
        //public bool NoSource;
        public bool ResizeParent;
        public bool SoundDetected;
        public objectsMicrophone Micobject;
        public double ReconnectCount;
        public double SoundCount;
        public IWavePlayer WaveOut;
        public IAudioSource AudioSource;

        public List<HttpRequest> OutSockets = new List<HttpRequest>();

        private Thread _tFiles;
        public void GetFiles()
        {
            if (_tFiles == null || _tFiles.Join(TimeSpan.Zero))
            {
                _tFiles = new Thread(GenerateFileList);
                _tFiles.Start();
            }
        }

        internal void GenerateFileList()
        {
            string dir = Dir.Entry + "audio\\" +Micobject.directory + "\\";
            if (!Directory.Exists(dir))
            {
                try
                {
                    Directory.CreateDirectory(dir);
                }
                catch (Exception ex)
                {
                    if (ErrorHandler!=null)
                        ErrorHandler(ex.Message);
                    _filelist = new List<FilesFile>();
                    if (FileListUpdated != null)
                        FileListUpdated(this);
                    return;
                }
            }
            bool failed = false;
            if (File.Exists(dir + "data.xml"))
            {
                var s = new XmlSerializer(typeof(Files));
                try
                {
                    using (var fs = new FileStream(dir + "data.xml", FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        try
                        {
                            using (TextReader reader = new StreamReader(fs))
                            {
                                fs.Position = 0;
                                lock (_lockobject)
                                {
                                    var t = ((Files)s.Deserialize(reader));
                                    if (t.File == null || !t.File.Any())
                                    {
                                        _filelist = new List<FilesFile>();
                                    }
                                    else
                                        _filelist = t.File.ToList();
                                }
                                reader.Close();
                            }
                            ScanForMissingFiles();
                        }
                        catch (Exception ex)
                        {
                            if (ErrorHandler != null)
                                ErrorHandler(ex.Message);
                            failed = true;
                        }
                        fs.Close();
                    }
                }
                catch (Exception ex)
                {
                    if (ErrorHandler != null)
                        ErrorHandler(ex.Message);
                    failed = true;
                }
                if (!failed)
                {
                    if (FileListUpdated != null)
                        FileListUpdated(this);
                    return;
                }
                    

            }

            //else build from directory contents

            _filelist = new List<FilesFile>();
            lock (_lockobject)
            {
                var dirinfo = new DirectoryInfo(Dir.Entry + "audio\\" + Micobject.directory + "\\");

                var lFi = new List<FileInfo>();
                lFi.AddRange(dirinfo.GetFiles());
                lFi = lFi.FindAll(f => f.Extension.ToLower() == ".mp3");
                lFi = lFi.OrderByDescending(f => f.CreationTime).ToList();

                //sanity check existing data
                foreach (FileInfo fi in lFi)
                {
                    FileInfo fi1 = fi;
                    if (_filelist.Count(p => p.Filename == fi1.Name) == 0)
                    {
                        _filelist.Add(new FilesFile
                                          {
                                              CreatedDateTicks = fi.CreationTime.Ticks,
                                              Filename = fi.Name,
                                              SizeBytes = fi.Length,
                                              MaxAlarm = 0,
                                              AlertData = "0",
                                              DurationSeconds = 0,
                                              IsTimelapse = false,
                                              IsMergeFile = fi.Name.ToLower().IndexOf("merge", StringComparison.Ordinal) != -1
                                          });
                    }
                }
                for (int index = 0; index < _filelist.Count; index++)
                {
                    FilesFile ff = _filelist[index];
                    if (lFi.All(p => p.Name != ff.Filename))
                    {
                        _filelist.Remove(ff);
                        index--;
                    }
                }
                _filelist = _filelist.OrderByDescending(p => p.CreatedDateTicks).ToList();
            }
            if (FileListUpdated != null)
                FileListUpdated(this);
        }
        //public List<FilesFile> FileList
        //{
        //    get { return _filelist ?? (_filelist = new List<FilesFile>()); }
        //}

        public void ClearFileList()
        {
            lock (_lockobject)
            {
                _filelist.Clear();
            }

            MainForm.MasterFileRemoveAll(1,Micobject.id);
        }

        public void RemoveFile(string filename)
        {
            lock (_lockobject)
            {
                _filelist.RemoveAll(p => p.Filename == filename);
            }
            MainForm.MasterFileRemove(filename);
        }

        public void SaveFileList()
        {
            try
            {
                if (_filelist != null)
                {
                    var fl = new Files {File = _filelist.ToArray()};
                    string dir = Dir.Entry + "audio\\" +
                                 Micobject.directory + "\\";
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                    var s = new XmlSerializer(typeof (Files));
                    using (var fs = new FileStream(dir + "data.xml", FileMode.Create))
                    {
                        using (TextWriter writer = new StreamWriter(fs))
                        {
                            fs.Position = 0;
                            s.Serialize(writer, fl);
                            writer.Close();
                        }
                        fs.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                if (ErrorHandler != null)
                    ErrorHandler(ex.Message);
            }
        }

        private Thread _tScan;
        private void ScanForMissingFiles()
        {
            if (_tScan == null || _tScan.Join(TimeSpan.Zero))
            {
                _tScan = new Thread(ScanFiles);
                _tScan.Start();
            }
        }

        private void ScanFiles()
        {
            try
            {
                //check files exist
                var dirinfo = new DirectoryInfo(Dir.Entry + "audio\\" + Micobject.directory + "\\");

                var lFi = new List<FileInfo>();
                lFi.AddRange(dirinfo.GetFiles());
                lFi = lFi.FindAll(f => f.Extension.ToLower() == ".mp3" || f.Extension.ToLower() == ".mp4");
                lFi = lFi.OrderByDescending(f => f.CreationTime).ToList();

                //var farr = _filelist.ToArray();
                lock (_lockobject)
                {
                    for (int j = 0; j < _filelist.Count; j++)
                    {
                        var t = _filelist[j];
                        if (t != null)
                        {
                            var fe = lFi.FirstOrDefault(p => p.Name == t.Filename);
                            if (fe == null)
                            {
                                //file not found
                                _filelist.RemoveAt(j);
                                j--;
                                continue;
                            }
                            lFi.Remove(fe);
                        }
                    }
                    //add missing files
                    foreach (var fi in lFi)
                    {
                        _filelist.Add(new FilesFile
                                            {
                                                CreatedDateTicks = fi.CreationTime.Ticks,
                                                Filename = fi.Name,
                                                SizeBytes = fi.Length,
                                                MaxAlarm = 0,
                                                AlertData = "0",
                                                DurationSeconds = 0,
                                                IsTimelapse = false,
                                                IsMergeFile = fi.Name.ToLower().IndexOf("merge", StringComparison.Ordinal) !=-1
                                            });
                    }
                }
            }
            catch (Exception ex)
            {
                if (ErrorHandler != null)
                    ErrorHandler(ex.Message);
            }
            if (FileListUpdated != null)
                FileListUpdated(this);
        }

        public bool Recording
        {
            get
            {
                try
                {
                    return _recordingThread != null && !_recordingThread.Join(TimeSpan.Zero);
                }
                catch
                {
                    
                }
                return false;
            }
        }

        public string ObjectName
        {
            get { return Micobject.name; }
        }

        public bool CanTalk
        {
            get { return false; }
        }

        public bool CanListen
        {
            get { return true; }
        }

        public bool CanRecord
        {
            get { return true; }
        }

        public bool CanEnable
        {
            get { return true; }
        }

        public bool CanGrab { get { return false; }}

        public bool IsEnabled { get; private set; }
        public bool Talking { get; set; }
        public bool HasFiles { get { return false; } }

        public bool Listening
        {
            get
            {
                if (WaveOut != null && WaveOut.PlaybackState == PlaybackState.Playing)
                    return true;
                return false;
            }
            set
            {
                if (WaveOut != null)
                {
                    if (value && AudioSource!=null)
                    {
                        AudioSource.Listening = true; //(creates the waveoutprovider referenced below)
                        WaveOut.Init(AudioSource.WaveOutProvider);
                        WaveOut.Play();
                        
                    }
                    else
                    {
                        if (AudioSource != null) AudioSource.Listening = false;
                        WaveOut.Stop();

                    }
                }
            }
        }

        public float Gain
        {
            get { return Micobject.settings.gain; }
            set
            {
                //if (AudioSource != null)
                //    AudioSource.Gain = value;
                Micobject.settings.gain = value;
            }
        }
        #endregion

        #region SizingControls

        private MousePos GetMousePos(Point location)
        {
            var result = MousePos.NoWhere;
            int rightSize = Padding.Right;
            int bottomSize = Padding.Bottom;
            var testRect = new Rectangle(Width - rightSize, 0, Width - rightSize, Height - bottomSize);
            if (testRect.Contains(location)) result = MousePos.Right;
            testRect = new Rectangle(0, Height - bottomSize, Width - rightSize, Height);
            if (testRect.Contains(location)) result = MousePos.Bottom;
            testRect = new Rectangle(Width - rightSize, Height - bottomSize, Width, Height);
            if (testRect.Contains(location)) result = MousePos.BottomRight;
            return result;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Select();
            IntPtr hwnd = Handle;
            if ((ResizeParent) && (Parent != null) && (Parent.IsHandleCreated))
            {
                hwnd = Parent.Handle;
            }
            if (e.Button == MouseButtons.Left)
            {
                MousePos mousePos = GetMousePos(e.Location);

                if (mousePos == MousePos.NoWhere)
                {
                    if (MainForm.Conf.ShowOverlayControls)
                    {
                        int bpi = GetButtonIndexByLocation(e.Location);
                        switch (bpi)
                        {
                            case 0:
                                if (IsEnabled)
                                {
                                    Disable();
                                }
                                else
                                {
                                    Enable();
                                }
                                break;
                            case 1:
                                if (IsEnabled)
                                {
                                    RecordSwitch(!Recording);
                                }
                                break;
                            case 2:
                                MainClass.EditMicrophone(Micobject);
                                break;
                            case 3:
                                if (Helper.HasFeature(Enums.Features.Access_Media))
                                {
                                    string url = MainForm.Webserver + "/watch_new.aspx";
                                    if (WsWrapper.WebsiteLive && MainForm.Conf.ServicesEnabled)
                                    {
                                        MainForm.OpenUrl(url);
                                    }
                                    else
                                        MainClass.Connect(url, false);
                                }
                                break;
                            case 4:
                                if (IsEnabled)
                                {
                                    Listen();
                                }
                                break;
                        }
                    }
                    return;
                }
                if (CameraControl!=null ||  MainForm.Conf.LockLayout) return;
                switch (mousePos)
                {
                    case MousePos.Right:
                        {
                            NativeCalls.ReleaseCapture(hwnd);
                            NativeCalls.SendMessage(hwnd, NativeCalls.WmSyscommand, NativeCalls.ScDragsizeE, IntPtr.Zero);
                        }
                        break;
                    case MousePos.Bottom:
                        {
                            NativeCalls.ReleaseCapture(hwnd);
                            NativeCalls.SendMessage(hwnd, NativeCalls.WmSyscommand, NativeCalls.ScDragsizeS, IntPtr.Zero);
                        }
                        break;
                    case MousePos.BottomRight:
                        {
                            NativeCalls.ReleaseCapture(hwnd);
                            NativeCalls.SendMessage(hwnd, NativeCalls.WmSyscommand, NativeCalls.ScDragsizeSe,
                                                    IntPtr.Zero);
                        }
                        break;
                    default:
                        Cursor = Cursors.Hand;
                        break;

                }
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_mouseLoc.X == e.X && _mouseLoc.Y == e.Y)
                return;
            _mouseLoc.X = e.X;
            _mouseLoc.Y = e.Y;

            _mouseMove = Helper.Now;
            MousePos mousePos = GetMousePos(e.Location);
            switch (mousePos)
            {
                case MousePos.Right:
                    Cursor = Cursors.SizeWE;
                    break;
                case MousePos.Bottom:
                    Cursor = Cursors.SizeNS;
                    break;
                case MousePos.BottomRight:
                    Cursor = Cursors.SizeNWSE;
                    break;
                default:
                    Cursor = Cursors.Hand;
                    if (_toolTipMic.Active)
                    {
                        _toolTipMic.Hide(this);
                        _ttind = -1;
                    }
                    if (e.Location.X < 30 && e.Location.Y > Height - 24)
                    {
                        string m = "";
                        if (Micobject.alerts.active)
                            m = "Alerts Active";

                        if (ForcedRecording)
                            m = "Forced Recording, " + m;

                        if (Micobject.detector.recordondetect)
                            m = "Record on Detect, " + m;
                        else
                        {
                            if (Micobject.detector.recordonalert)
                                m = "Record on Alert, " + m;
                            else
                            {
                                m = "No Recording, " + m;
                            }
                        }
                        if (Micobject.schedule.active)
                            m += ", Scheduled";

                        m = m.Trim().Trim(',');
                        var toolTipLocation = new Point(5, Height - 24);
                        _toolTipMic.Show(m, this, toolTipLocation, 1000);
                    }

                    if (MainForm.Conf.ShowOverlayControls)
                    {
                        var rBp = ButtonPanel;
                         var toolTipLocation = new Point(e.Location.X, rBp.Y + rBp.Height + 1);
                        int bpi = GetButtonIndexByLocation(e.Location);
                        if (_ttind != bpi)
                        {
                            switch (bpi)
                            {
                                case 0:
                                    _toolTipMic.Show(
                                        IsEnabled ? LocRm.GetString("switchOff") : LocRm.GetString("Switchon"), this,
                                        toolTipLocation, 1000);
                                    _ttind = 0;
                                    break;
                                case 1:
                                    if (Helper.HasFeature(Enums.Features.Recording))
                                    {
                                        _toolTipMic.Show(LocRm.GetString("RecordNow"), this, toolTipLocation, 1000);
                                        _ttind = 1;
                                    }
                                    break;
                                case 2:
                                    _toolTipMic.Show(LocRm.GetString("Edit"), this, toolTipLocation, 1000);
                                    _ttind = 2;
                                    break;
                                case 3:
                                    if (Helper.HasFeature(Enums.Features.Access_Media))
                                    {
                                        _toolTipMic.Show(LocRm.GetString("MediaoverTheWeb"), this, toolTipLocation, 1000);
                                        _ttind = 3;
                                    }
                                    break;
                                case 4:
                                    _toolTipMic.Show(Listening
                                        ? LocRm.GetString("StopListening")
                                        : LocRm.GetString("Listen"), this, toolTipLocation, 1000);
                                    _ttind = 4;

                                    break;
                            }
                        }
                    }
                    break;
            }
            base.OnMouseMove(e);
        }

        protected override void OnResize(EventArgs eventargs)
        {
            if (CameraControl == null)
            {
                if ((ModifierKeys & Keys.Shift) == Keys.Shift)
                {
                    var ar = Convert.ToDouble(MinimumSize.Width)/Convert.ToDouble(MinimumSize.Height);
                    Width = Convert.ToInt32(ar*Height);
                }

               
                if (Width < MinimumSize.Width) Width = MinimumSize.Width;
                if (Height < MinimumSize.Height) Height = MinimumSize.Height;
            }
            base.OnResize(eventargs);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            Cursor = Cursors.Default;
            _mouseMove = DateTime.MinValue;
            _requestRefresh = true;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (CameraControl==null)
                Cursor = Cursors.Hand;
            _requestRefresh = true;
        }


        #region Nested type: MousePos

        private enum MousePos
        {
            NoWhere,
            Right,
            Bottom,
            BottomRight
        }

        #endregion

        #endregion

        public VolumeLevel(objectsMicrophone om, MainForm mainForm)
        {
            InitializeComponent();

            SetStyle(
                ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint, true);
            Margin = new Padding(0, 0, 0, 0);
            Padding = new Padding(0, 0, 5, 5);
            BorderStyle = BorderStyle.None;
            BackgroundColor = MainForm.BackgroundColor;
            Micobject = om;
            MainClass = mainForm;
            WriterBuffer.Changed += WriterBufferChanged;
            ErrorHandler += VolumeLevel_ErrorHandler;
            _toolTipMic = new ToolTip { AutomaticDelay = 500, AutoPopDelay = 1500 };
        }

        void VolumeLevel_ErrorHandler(string message)
        {
            MainForm.LogErrorToFile(Micobject.name+": "+message);
        }


        [DefaultValue(false)]
        public float[] Levels
        {
            get
            {
                return _levels;
            }
            set
            {
                if (value == null)
                    _levels = null;
                else
                {
                    if (_levels==null || value.Length != _levels.Length)
                        _levels = new float[value.Length];
                    value.CopyTo(_levels, 0);

                    if (_levels.Length > 0 && AudioSourceErrorState)
                    {
                        if (AudioSourceErrorState)
                            UpdateFloorplans(false);
                        AudioSourceErrorState = false;
                    }
                }
                

                Invalidate();
            }
        }

        public string[] ScheduleDetails
        {
            get
            {
                var entries = new List<string>();
                foreach (var sched in Micobject.schedule.entries)
                {
                    string daysofweek = sched.daysofweek;
                    daysofweek = daysofweek.Replace("0", LocRm.GetString("Sun"));
                    daysofweek = daysofweek.Replace("1", LocRm.GetString("Mon"));
                    daysofweek = daysofweek.Replace("2", LocRm.GetString("Tue"));
                    daysofweek = daysofweek.Replace("3", LocRm.GetString("Wed"));
                    daysofweek = daysofweek.Replace("4", LocRm.GetString("Thu"));
                    daysofweek = daysofweek.Replace("5", LocRm.GetString("Fri"));
                    daysofweek = daysofweek.Replace("6", LocRm.GetString("Sat"));

                    string s = sched.start + " -> " + sched.stop + " (" + daysofweek + ")";
                    if (sched.recordonstart)
                        s += " " + LocRm.GetString("Record").ToUpper();
                    if (sched.alerts)
                        s += " " + LocRm.GetString("Alert").ToUpper();
                    if (sched.recordondetect)
                        s += " " + LocRm.GetString("Detect").ToUpper();
                    if (!sched.active)
                        s += " (" + LocRm.GetString("Inactive").ToUpper() + ")";

                    entries.Add(s);
                }
                return entries.ToArray();
            }
        }

        private double _tickThrottle;
        public void Tick()
        {
            //time since last tick
            var ts = new TimeSpan(Helper.Now.Ticks - _lastRun);
            _lastRun = Helper.Now.Ticks;
            _secondCountNew = ts.Milliseconds / 1000.0;

            if (Micobject.schedule.active)
            {
                if (CheckSchedule()) goto skip;
            }

            if (!IsEnabled) goto skip;

            SoundDetected = SoundRecentlyDetected;
            

            if (FlashCounter > DateTime.MinValue)
            {
                double iFc = (FlashCounter - Helper.Now).TotalSeconds;
                if (SoundDetected)
                {
                    InactiveRecord = 0;
                    if (Micobject.alerts.mode != "nosound" &&
                        (Micobject.detector.recordondetect || Micobject.detector.recordonalert))
                    {
                        var cc = CameraControl;
                        if (cc != null)
                            cc.InactiveRecord = 0;
                    }
                }
                if (iFc < 9)
                    SoundDetected = false;

                if (iFc < 1)
                {
                    UpdateFloorplans(false);
                    FlashCounter = DateTime.MinValue;
                }

            }

            if (Recording)
                _recordingTime += Convert.ToDouble(ts.TotalMilliseconds)/1000.0;

            if (IsEnabled)
            {
                FlashBackground();
            }
            
            _tickThrottle += _secondCountNew;

            if (_tickThrottle > 1 && IsEnabled) //every second
            {
                if (CheckReconnect()) goto skip;

                CheckReconnectInterval(_tickThrottle);

                CheckDisconnect();

                if (Recording && !SoundDetected && !ForcedRecording)
                {
                    InactiveRecord += _tickThrottle;
                }
                

                if (_levels!=null)
                {
                    CheckVLCTimeStamp();

                    CheckRecord();
                }
                _tickThrottle = 0;
            }


            CheckAlert(_secondCountNew);

            skip:

            if (_requestRefresh)
            {
                _requestRefresh = false;
                Invalidate();
            }
        }

        private DateTime _lastFlash = DateTime.MinValue;
        private void FlashBackground()
        {
            bool b = true;
            if (Micobject.alerts.active)
            {
                b = BackgroundColor != MainForm.BackgroundColor;
                var dt = Helper.Now;
                if ((dt - _lastFlash).TotalMilliseconds < 500)
                    return;
                _lastFlash = dt;

                if (FlashCounter > Helper.Now)
                {
                    BackgroundColor = (BackgroundColor == MainForm.ActivityColor)
                                          ? MainForm.BackgroundColor
                                          : MainForm.ActivityColor;
                    b = false;
                }
                else
                {
                    switch (Micobject.alerts.mode.ToLower())
                    {
                        case "nosound":
                            if (!SoundDetected)
                            {
                                BackgroundColor = (BackgroundColor == MainForm.NoActivityColor)
                                                      ? MainForm.BackgroundColor
                                                      : MainForm.NoActivityColor;
                                b = false;
                            }

                            break;
                    }
                }
            }
            if (b)
                BackgroundColor = MainForm.BackgroundColor;

        }

        private void CheckReconnectInterval(double since)
        {
            if (IsEnabled && AudioSource != null && !IsClone && !IsReconnect && !(AudioSource is IVideoSource))
            {
                if (Micobject.settings.reconnectinterval > 0)
                {
                    ReconnectCount += since;
                    if (ReconnectCount > Micobject.settings.reconnectinterval)
                    {
                        IsReconnect = true;

                        try
                        {
                            AudioSource.Stop();
                        }
                        catch(Exception ex)
                        {
                            if (ErrorHandler != null)
                                ErrorHandler(ex.Message);
                        }

                        try
                        {
                            AudioSource.Start();
                        }
                        catch (Exception ex)
                        {
                            if (ErrorHandler != null)
                                ErrorHandler(ex.Message);
                        }

                        IsReconnect = false;
                        ReconnectCount = 0;
                    }
                    
                }
            }
        }

        private int GetButtonIndexByLocation(Point xy)
        {
            var rBp = ButtonPanel;
            if (xy.X >= rBp.X && xy.Y > rBp.Y && xy.X <= rBp.X + rBp.Width && xy.Y <= rBp.Y + rBp.Height)
            {
                double x = xy.X - rBp.X;
                return Convert.ToInt32(Math.Ceiling((x / rBp.Width) * ButtonCount)) - 1;
            }
            return -999;//nothing
        }


        private Rectangle GetButtonByIndex(int buttonIndex, out Rectangle destRect)
        {
            Rectangle rSrc = Rectangle.Empty;
            bool b = IsEnabled;
            switch (buttonIndex)
            {
                case 0://power
                    rSrc = b ? MainForm.RPowerOn : MainForm.RPower;
                    break;
                case 1://record
                    if (b && Helper.HasFeature(Enums.Features.Recording))
                        rSrc = Recording ? MainForm.RRecordOn : MainForm.RRecord;
                    else
                    {
                        rSrc = MainForm.RRecordOff;
                    }
                    break;
                case 2://settings
                    rSrc = MainForm.REdit;
                    break;
                case 3://web
                    if (Helper.HasFeature(Enums.Features.Access_Media))
                        rSrc = MainForm.RWeb;
                    else
                        rSrc = MainForm.RWebOff;
                    break;
                case 4://listen
                    if (b)
                        rSrc = Listening ? MainForm.RListenOn : MainForm.RListen;
                    else
                        rSrc = MainForm.RListenOff;
                    break;
            }

            if (MainForm.Conf.BigButtons)
            {
                rSrc.X -= 2;
                rSrc.Width += 8;
                rSrc.Height += 8;
            }
            var bp = ButtonPanel;
            destRect = new Rectangle(bp.X + buttonIndex * (bp.Width / ButtonCount) + 5, bp.Top+5, rSrc.Width, rSrc.Height);
            return rSrc;
        }


        private void DrawButton(Graphics gCam, int buttonIndex)
        {
            Rectangle rDest;
            Rectangle rSrc = GetButtonByIndex(buttonIndex, out rDest);

            gCam.DrawImage(MainForm.Conf.BigButtons ? Properties.Resources.icons_big : Properties.Resources.icons, rDest, rSrc, GraphicsUnit.Pixel);
        }

        private void CheckDisconnect()
        {
            if (_errorTime != DateTime.MinValue)
            {
                int sec = Convert.ToInt32((Helper.Now - _errorTime).TotalSeconds);
                if (sec > MainForm.Conf.DisconnectNotificationDelay)
                {
                    DoAlert("disconnect");
                    _errorTime = DateTime.MinValue;
                }
            }
        }

        private string MailMerge(string s, string mode, bool recorded = false, string pluginMessage = "")
        {
            double offset = 0;
            var oc = CameraControl;
            if (oc != null && oc.Camobject != null)
                offset = Convert.ToDouble(oc.Camobject.settings.timestampoffset);

            s = s.Replace("[OBJECTNAME]", Micobject.name);
            s = s.Replace("[TIME]", DateTime.Now.AddHours(offset).ToLongTimeString());
            s = s.Replace("[DATE]", DateTime.Now.AddHours(offset).ToShortDateString());
            s = s.Replace("[RECORDED]", recorded ? "(recorded)" : "");
            s = s.Replace("[PLUGIN]", pluginMessage);
            s = s.Replace("[EVENT]", mode.ToUpper());
            s = s.Replace("[SERVER]", MainForm.Conf.ServerName);
            
            return s;
        }

        private bool CheckSchedule()
        {
            DateTime dtnow = DateTime.Now;
            foreach (objectsMicrophoneScheduleEntry entry in Micobject.schedule.entries.Where(p => p.active))
            {
                if (
                    entry.daysofweek.IndexOf(
                        ((int) dtnow.DayOfWeek).ToString(CultureInfo.InvariantCulture),
                        StringComparison.Ordinal) != -1)
                {
                    string[] stop = entry.stop.Split(':');
                    if (stop[0] != "-")
                    {
                        if (Convert.ToInt32(stop[0]) == dtnow.Hour)
                        {
                            if (Convert.ToInt32(stop[1]) == dtnow.Minute && dtnow.Second < 2)
                            {
                                Micobject.detector.recordondetect = entry.recordondetect;
                                Micobject.detector.recordonalert = entry.recordonalert;
                                Micobject.alerts.active = entry.alerts;

                                if (IsEnabled)
                                    Disable();
                                return true;
                            }
                        }
                    }

                    string[] start = entry.start.Split(':');
                    if (start[0] != "-")
                    {
                        if (Convert.ToInt32(start[0]) == dtnow.Hour)
                        {
                            if (Convert.ToInt32(start[1]) == dtnow.Minute && dtnow.Second < 3)
                            {
                                if (!IsEnabled)
                                    Enable();
                                if ((dtnow - _lastScheduleCheck).TotalSeconds > 60)
                                {
                                    Micobject.detector.recordondetect = entry.recordondetect;
                                    Micobject.detector.recordonalert = entry.recordonalert;
                                    Micobject.alerts.active = entry.alerts;
                                    if (entry.recordonstart)
                                    {
                                        ForcedRecording = true;
                                        StartSaving();
                                    }
                                }
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private void CheckAlert(double since)
        {
            if (IsEnabled && AudioSource != null)
            {
                if (Alerted)
                {
                    if ((Helper.Now - LastAlerted).TotalSeconds > Micobject.alerts.minimuminterval)
                    {
                        Alerted = false;
                        UpdateFloorplans(false);
                    }
                }
                else
                {
                    if (Micobject.alerts.active && AudioSource != null)
                    {
                        switch (Micobject.alerts.mode)
                        {
                            case "sound":
                                if (SoundDetected)
                                {
                                    SoundCount += since;
                                    if (SoundCount >= Micobject.detector.soundinterval)
                                    {
                                        if (Helper.CanAlert(Micobject.alerts.groupname, Micobject.alerts.resetinterval))
                                        {
                                            DoAlert("alert");
                                            SoundCount = 0;
                                        }
                                    }
                                }
                                else
                                    SoundCount = 0;

                                break;
                            case "nosound":
                                if ((Helper.Now - LastSoundDetected).TotalSeconds >
                                    Micobject.detector.nosoundinterval)
                                {
                                    if (Helper.CanAlert(Micobject.alerts.groupname, Micobject.alerts.resetinterval))
                                    {
                                        DoAlert("alert");
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }

        private void CheckVLCTimeStamp()
        {
            if (Micobject.settings.typeindex == 2)
            {
                var vlc = AudioSource as VLCStream;
                if (vlc != null)
                {
                    vlc.CheckTimestamp();
                }
            }
        }

        private void CheckRecord()
        {
            if (ForcedRecording)
            {
                StartSaving();
            }
            else
            {
                if (Recording && !_pairedRecording)
                {
                    if (_recordingTime > Micobject.recorder.maxrecordtime || 
                        ((!SoundDetected && InactiveRecord > Micobject.recorder.inactiverecord) && !ForcedRecording))
                        StopSaving();
                }
            }
        }

        private bool CheckReconnect()
        {
            if (_reconnectTime != DateTime.MinValue && !IsClone && !IsReconnect && !(AudioSource is IVideoSource))
            {
                if (AudioSource != null)
                {
                    int sec = Convert.ToInt32((Helper.Now - _reconnectTime).TotalSeconds);
                    if (sec > 10)
                    {
                        //try to reconnect every 10 seconds
                        if (!AudioSource.IsRunning)
                        {  
                            AudioSource.Start();
                        }
                        _reconnectTime = Helper.Now;
                        return true;
                    }
                }
            }
            return false;
        }

        protected override void OnLostFocus(EventArgs e)
        {
            _requestRefresh = true;
            base.OnLostFocus(e);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            _requestRefresh = true;
            base.OnGotFocus(e);
        }

        public bool Highlighted;

        public Color BorderColor
        {
            get
            {
                if (Highlighted)
                    return MainForm.FloorPlanHighlightColor;

                if (Focused)
                    return MainForm.BorderHighlightColor;

                return MainForm.BorderDefaultColor;

            }
        }

        public int BorderWidth
        {
            get
            {
                return (Highlighted || Focused) ? 2 : 1;
            }
        }


        protected override void OnPaint(PaintEventArgs pe)
        {
            var gMic = pe.Graphics;
            var rc = ClientRectangle;

            
            var grabBrush = new SolidBrush(BorderColor);
            var borderPen = new Pen(grabBrush,BorderWidth);
            var lgb = new SolidBrush(MainForm.VolumeLevelColor);

            gMic.Clear(BackgroundColor);
            string m = "", txt = Micobject.name;
            if (IsEnabled)
            {
                var l = _levels;
                if (l != null && !AudioSourceErrorState)
                {
                    int bh = (rc.Height - 20)/Micobject.settings.channels - (Micobject.settings.channels - 1)*2;
                    if (bh <= 2)
                        bh = 2;
                    for (int j = 0; j < Micobject.settings.channels; j++)
                    {
                        float f = 0f;
                        if (j < l.Length)
                            f = l[j];
                        int drawW = Convert.ToInt32(Convert.ToDouble(rc.Width - 1.0)*f);
                        if (drawW < 1)
                            drawW = 1;

                        gMic.FillRectangle(lgb, rc.X + 2, rc.Y + 2 + j*bh + (j*2), drawW - 4, bh);

                    }
                    var d = (Convert.ToDouble(rc.Width - 4) / 100.00);
                    var mx1 =(float)(d*Micobject.detector.minsensitivity);
                    var mx2 = (float)(d * Micobject.detector.maxsensitivity);

                    gMic.DrawLine(_vline, mx1, 2, mx1, rc.Height - 20);
                    gMic.DrawLine(_vline, mx2, 2, mx2, rc.Height - 20);

                    if (Listening)
                    {
                        gMic.DrawString("LIVE", MainForm.Drawfont, MainForm.CameraDrawBrush, new PointF(5, 4));
                    }


                    if (Recording)
                    {
                        gMic.FillEllipse(MainForm.RecordBrush, new Rectangle(rc.Width - 14, 2, 8, 8));
                    }


                    lgb.Dispose();
                }
                else
                {
                    var img = Properties.Resources.connecting;
                    gMic.DrawImage(img, Width - img.Width-2, 2, img.Width, img.Height);
                }
            }
            else
            {
                txt += ": " + LocRm.GetString("Offline");
                gMic.DrawString(SourceType + ": " + Micobject.name, MainForm.Drawfont, MainForm.CameraDrawBrush, new PointF(5, 5));
            }

            string flags = "";
            if (Micobject.alerts.active)
                flags += "!";

            if (ForcedRecording)
                flags += "F";
            else
            {
                if (Micobject.detector.recordondetect)
                    flags += "D";
                else
                {
                    if (Micobject.detector.recordonalert)
                        flags += "A";
                    else
                    {
                        flags += "N";
                    }
                }
            }
            if (Micobject.schedule.active)
                flags += "S";

            if (flags != "")
                m = flags + "  " + m;

            gMic.DrawString(m + txt, MainForm.Drawfont, MainForm.CameraDrawBrush, new PointF(5, rc.Height - 18));
            


            if (_mouseMove > Helper.Now.AddSeconds(-3) && MainForm.Conf.ShowOverlayControls)
            {
                DrawOverlay(gMic);
            }


            if (!Paired)
            {
                var grabPoints = new[]
                                 {
                                     new Point(rc.Width - 15, rc.Height), new Point(rc.Width, rc.Height - 15),
                                     new Point(rc.Width, rc.Height)
                                 };
                gMic.FillPolygon(grabBrush, grabPoints);
            }

            if (!Paired)
                gMic.DrawRectangle(borderPen, 0, 0, rc.Width - 1, rc.Height - 1);
            else
            {
                gMic.DrawLine(borderPen, 0, 0, 0, rc.Height - 1);
                gMic.DrawLine(borderPen, 0, rc.Height - 1, rc.Width - 1, rc.Height - 1);
                gMic.DrawLine(borderPen, rc.Width - 1, rc.Height - 1, rc.Width - 1, 0);
            }



            borderPen.Dispose();
            grabBrush.Dispose();

            base.OnPaint(pe);
        }

        private void DrawOverlay(Graphics gMic)
        {
            var rPanel = ButtonPanel;

            gMic.FillRectangle(MainForm.OverlayBackgroundBrush, rPanel);

            for (int i = 0; i < ButtonCount; i++)
                DrawButton(gMic, i);
        }

        public void StopSaving()
        {
            if (Recording)
            {
                _stopWrite.Set();
                if (Recording)
                    _recordingThread.Join();
            }
        }

        internal configurationDirectory Dir
        {
            get
            {
                try
                {
                    return MainForm.Conf.MediaDirectories[Micobject.settings.directoryIndex];
                }
                catch
                {
                    return MainForm.Conf.MediaDirectories[0];
                }
            }
        }

        public void StartSaving()
        {
            if (Recording || Dir.StopSavingFlag || IsEdit)
                return;
            if (!Helper.HasFeature(Enums.Features.Recording))
                return;

            lock (_lockobject)
            {
                _recordingThread = new Thread(Record)
                                   {
                                       Name = "Recording Thread (" + Micobject.id + ")",
                                       IsBackground = false,
                                       Priority = ThreadPriority.Normal
                                   };
                _recordingThread.Start();
            }
        }

        private void Record()
        {
            _stopWrite.Reset();
            DateTime recordingStart = DateTime.MinValue;

            if (!String.IsNullOrEmpty(Micobject.recorder.trigger) && TopLevelControl != null)
            {
                string[] tid = Micobject.recorder.trigger.Split(',');
                switch (tid[0])
                {
                    case "1":
                        VolumeLevel vl = ((MainForm)TopLevelControl).GetVolumeLevel(Convert.ToInt32(tid[1]));
                        if (vl != null && !vl.Recording)
                            vl.RecordSwitch(true);
                        break;
                    case "2":
                        CameraWindow cw = ((MainForm)TopLevelControl).GetCameraWindow(Convert.ToInt32(tid[1]));
                        if (cw != null && !cw.Recording)
                            cw.RecordSwitch(true);
                        break;
                }
            }

            try
            {
                _pairedRecording = false;
                if (CameraControl!=null && CameraControl.Camobject.settings.active)
                {
                    _pairedRecording = true;
                    CameraControl.StartSaving();
                    CameraControl.ForcedRecording = ForcedRecording;
                    while (!_stopWrite.WaitOne(0))
                    {
                        _newRecordingFrame.WaitOne(200);
                    }
                }
                else
                {
                    #region mp3writer

                    DateTime date = Helper.Now;

                    string filename = String.Format("{0}-{1}-{2}_{3}-{4}-{5}",
                                                    date.Year, Helper.ZeroPad(date.Month), Helper.ZeroPad(date.Day),
                                                    Helper.ZeroPad(date.Hour), Helper.ZeroPad(date.Minute),
                                                    Helper.ZeroPad(date.Second));

                    AudioFileName = Micobject.id + "_" + filename;
                    string folder = Dir.Entry + "audio\\" + Micobject.directory + "\\";
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);
                    filename = folder + AudioFileName;

                    
                    
                    _writer = new AudioFileWriter();
                    try
                    {
                        Program.WriterMutex.WaitOne();                       
                        _writer.Open(filename + ".mp3", AudioCodec.MP3, AudioSource.RecordingFormat.BitsPerSample * AudioSource.RecordingFormat.SampleRate * AudioSource.RecordingFormat.Channels, AudioSource.RecordingFormat.SampleRate, AudioSource.RecordingFormat.Channels);
                    }
                    catch (Exception ex)
                    {
                        if (ErrorHandler != null)
                            ErrorHandler(ex.Message);
                    }
                    finally
                    {
                        Program.WriterMutex.ReleaseMutex();
                    }

                    

                    double maxlevel = 0;
                    var l = AudioBuffer.OrderBy(p => p.TimeStamp).ToList();
                    foreach (AudioAction aa in l)
                    {
                        if (recordingStart == DateTime.MinValue)
                        {
                            recordingStart = aa.TimeStamp;
                        }

                        unsafe
                        {
                            fixed (byte* p = aa.Decoded)
                            {
                                _writer.WriteAudio(p, aa.Decoded.Length);
                            }
                        }

                        _soundData.Append(String.Format(CultureInfo.InvariantCulture,
                                                        "{0:0.000}", aa.SoundLevel));
                        _soundData.Append(",");
                        if (aa.SoundLevel > maxlevel)
                            maxlevel = aa.SoundLevel;
                    }
                    
                    ClearAudioBuffer();

                    if (recordingStart == DateTime.MinValue)
                        recordingStart = Helper.Now;

                    try
                    {
                        while (!_stopWrite.WaitOne(0))
                        {
                            while (WriterBuffer.Count > 0)
                            {
                                AudioAction b;
                                lock (_lockobject)
                                {
                                    b = WriterBuffer.Dequeue();
                                }
                                unsafe
                                {
                                    fixed (byte* p = b.Decoded)
                                    {
                                        _writer.WriteAudio(p, b.Decoded.Length);
                                    }
                                }
                                float d = Levels.Max();
                                _soundData.Append(String.Format(CultureInfo.InvariantCulture,
                                                               "{0:0.000}", d));
                                _soundData.Append(",");
                                if (d > maxlevel)
                                    maxlevel = d;
                                b.Nullify();

                            }
                            _newRecordingFrame.WaitOne(200);
                        }


                        FilesFile ff = _filelist.FirstOrDefault(p => p.Filename.EndsWith(AudioFileName + ".mp3"));
                        bool newfile = false;
                        if (ff == null)
                        {
                            ff = new FilesFile();
                            newfile = true;
                        }


                        string[] fnpath = (filename + ".mp3").Split('\\');
                        string fn = fnpath[fnpath.Length - 1];
                        var fi = new FileInfo(filename + ".mp3");
                        var dSeconds = Convert.ToInt32((Helper.Now - recordingStart).TotalSeconds);

                        ff.CreatedDateTicks = DateTime.Now.Ticks;
                        ff.Filename = fnpath[fnpath.Length - 1];
                        ff.MaxAlarm = maxlevel;
                        ff.SizeBytes = fi.Length;
                        ff.DurationSeconds = dSeconds;
                        ff.IsTimelapse = false;
                        ff.IsMergeFile = false;
                        ff.AlertData = Helper.GetMotionDataPoints(_soundData);
                        _soundData.Clear();
                        ff.TriggerLevel = Micobject.detector.minsensitivity;
                        ff.TriggerLevelMax = Micobject.detector.maxsensitivity;

                        if (newfile)
                        {
                            lock (_lockobject)
                            {
                                _filelist.Insert(0, ff);
                            }

                            MainForm.MasterFileAdd(new FilePreview(fn, dSeconds, Micobject.name,DateTime.Now.Ticks, 1, Micobject.id,ff.MaxAlarm,false,false));
                            MainForm.NeedsMediaRefresh = Helper.Now;

                        }


                    }
                    catch (Exception ex)
                    {
                        if (ErrorHandler != null)
                            ErrorHandler(ex.Message);           
                    }


                    if (_writer != null && _writer.IsOpen)
                    {
                        try
                        {
                            Program.WriterMutex.WaitOne();
                            _writer.Dispose();
                        }
                        catch (Exception ex)
                        {
                            if (ErrorHandler != null)
                                ErrorHandler(ex.Message);
                        }
                        finally
                        {
                            Program.WriterMutex.ReleaseMutex();
                        }
                    }

                    _writer = null;
                    #endregion
                }
                ClearWriterBuffer();
                _recordingTime = 0;
                UpdateFloorplans(false);
            }
            catch (Exception ex)
            {
                if (ErrorHandler != null)
                    ErrorHandler(ex.Message);
            }
            
            if (!String.IsNullOrEmpty(Micobject.recorder.trigger) && TopLevelControl != null)
            {
                string[] tid = Micobject.recorder.trigger.Split(',');
                switch (tid[0])
                {
                    case "1":
                        VolumeLevel vl = ((MainForm)TopLevelControl).GetVolumeLevel(Convert.ToInt32(tid[1]));
                        if (vl != null)
                            vl.RecordSwitch(false);
                        break;
                    case "2":
                        CameraWindow cw = ((MainForm)TopLevelControl).GetCameraWindow(Convert.ToInt32(tid[1]));
                        if (cw != null)
                            cw.RecordSwitch(false);
                        break;
                }
            }

            if (!_pairedRecording)
            {
                Micobject.newrecordingcount++;
                if (Notification != null)
                    Notification(this, new NotificationType("NewRecording", Micobject.name, ""));
            }
        }

        private void ClearWriterBuffer()
        {
            while (WriterBuffer.Count > 0)
            {
                WriterBuffer.Dequeue().Nullify();
            }            
        }

        void WriterBufferChanged(object sender, EventArgs e)
        {
            _newRecordingFrame.Set();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Invalidate();              
            }
            if (WaveOut != null)
            {
                WaveOut.Stop();
                WaveOut.Dispose();
                WaveOut = null;
            }
            _toolTipMic.RemoveAll();
            _toolTipMic.Dispose();
            _vline.Dispose();
            base.Dispose(disposing);
        }

        public void ClearAudioBuffer()
        {
            while (AudioBuffer.Count > 0)
            {
                AudioBuffer[0].Nullify();
                AudioBuffer.RemoveAt(0);
            }
        }

        private bool _enabling, _disabling;
        public void Disable(bool stopSource = true)
        {
            if (_disabling)
                return;
            if (InvokeRequired)
            {
                Invoke(new Delegates.DisableDelegate(Disable), stopSource);
                return;
            }

            lock (_lockobject)
            {
                if (!IsEnabled)
                    return;
                IsEnabled = false;
            }
            _disabling = true;

            try
            {
                IsReconnect = false;
                RecordSwitch(false);

                if (AudioSource != null)
                {
                    AudioSource.AudioFinished -= AudioDeviceAudioFinished;
                    AudioSource.DataAvailable -= AudioDeviceDataAvailable;
                    AudioSource.LevelChanged -= AudioDeviceLevelChanged;

                    if (!IsClone)
                    {
                        if (stopSource)
                        {
                            if (!(AudioSource is IVideoSource))
                            {
                                //lock (_lockobject)
                                //{
                                AudioSource.Stop();
                                //allow operations to complete in other threads
                                Thread.Sleep(250);
                                //}
                            }
                        }
                    }

                }

                IsEnabled = false;
                IsReconnect = false;

                StopSaving();

                ClearAudioBuffer();

                Levels = null;
                SoundDetected = false;
                ForcedRecording = false;
                Alerted = false;
                FlashCounter = DateTime.MinValue;
                _recordingTime = 0;
                Listening = false;
                ReconnectCount = 0;
                AudioSourceErrorState = false;
                _soundRecentlyDetected = false;

                UpdateFloorplans(false);
                Micobject.settings.active = false;

                MainForm.NeedsSync = true;
                _errorTime = _reconnectTime = DateTime.MinValue;
                BackgroundColor = MainForm.BackgroundColor;
                if (!ShuttingDown)
                    _requestRefresh = true;
            }
            catch (Exception ex)
            {
                if (ErrorHandler != null)
                    ErrorHandler(ex.Message);
            }
            _disabling = false;
        }

        public void Enable()
        {
            if (_enabling)
                return;
            if (InvokeRequired)
            {
                Invoke(new Delegates.EnableDelegate(Enable));
                return;
            }

            lock (_lockobject)
            {
                if (IsEnabled)
                    return;
                IsEnabled = true;
            }
            _enabling = true;

            try
            {

                if (CameraControl != null)
                {
                    Width = CameraControl.Width;
                    Location = new Point(CameraControl.Location.X, CameraControl.Location.Y + CameraControl.Height);
                    Width = Width;
                    Height = 50;
                    if (!CameraControl.IsEnabled)
                    {
                        CameraControl.Enable();
                    }
                }

                IsEnabled = true;
                IsReconnect = false;

                int sampleRate = Micobject.settings.samples;
                int channels = Micobject.settings.channels;
                const int bitsPerSample = 16;

                if (channels < 1)
                {
                    channels = Micobject.settings.channels = 1;
                }
                if (sampleRate < 8000)
                {
                    sampleRate = Micobject.settings.samples = 8000;
                }
                IsClone = CameraControl != null && CameraControl.Camobject.settings.sourceindex == 10 &&
                          Micobject.settings.typeindex == 4;

                switch (Micobject.settings.typeindex)
                {
                    case 0: //usb
                        AudioSource = new LocalDeviceStream(Micobject.settings.sourcename)
                                      {RecordingFormat = new WaveFormat(sampleRate, bitsPerSample, channels)};
                        break;
                    case 1: //ispy server (fixed waveformat at the moment...)
                        AudioSource = new iSpyServerStream(Micobject.settings.sourcename)
                                      {RecordingFormat = new WaveFormat(8000, 16, 1)};
                        break;
                    case 2: //VLC listener
                        List<String> inargs = Micobject.settings.vlcargs.Split(Environment.NewLine.ToCharArray(),
                            StringSplitOptions.RemoveEmptyEntries).
                            ToList();
                        //switch off video output
                        inargs.Add(":sout=#transcode{vcodec=none}:Display");

                        AudioSource = new VLCStream(Micobject.settings.sourcename, inargs.ToArray())
                                      {
                                          RecordingFormat = new WaveFormat(sampleRate, bitsPerSample, channels),
                                          TimeOut = Micobject.settings.timeout
                                      };
                        break;
                    case 3: //FFMPEG listener
                        AudioSource = new FFMPEGAudioStream(Micobject.settings.sourcename)
                                      {
                                          RecordingFormat = new WaveFormat(sampleRate, bitsPerSample, channels),
                                          AnalyseDuration = Micobject.settings.analyzeduration,
                                          Timeout = Micobject.settings.timeout
                                      };
                        break;
                    case 4: //From Camera Feed
                        AudioSource = null;
                        if (CameraControl != null)
                        {
                            if (CameraControl.Camera != null)
                            {
                                AudioSource = CameraControl.Camera.VideoSource as IAudioSource;
                                if (AudioSource == null)
                                {
                                    if (IsClone)
                                    {
                                        //cloned feed
                                        int icam = Convert.ToInt32(CameraControl.Camobject.settings.videosourcestring);
                                        var topLevelControl = (MainForm) TopLevelControl;
                                        if (topLevelControl != null)
                                        {
                                            var cw = topLevelControl.GetCameraWindow(icam);
                                            if (cw != null)
                                            {
                                                if (CameraControl != null && CameraControl.VolumeControl != null &&
                                                    cw.VolumeControl != null && cw.VolumeControl.AudioSource != null)
                                                {
                                                    AudioSource = cw.VolumeControl.AudioSource;
                                                }
                                            }
                                        }
                                    }
                                }
                                if (AudioSource != null && AudioSource.RecordingFormat != null)
                                {
                                    Micobject.settings.samples = AudioSource.RecordingFormat.SampleRate;
                                    Micobject.settings.channels = AudioSource.RecordingFormat.Channels;

                                }
                            }
                            if (AudioSource == null)
                            {
                                SetErrorState("Mic source offline");
                                AudioSourceErrorState = true;
                                _requestRefresh = true;
                            }
                        }
                        break;
                }

                if (AudioSource != null)
                {
                    WaveOut = !String.IsNullOrEmpty(Micobject.settings.deviceout)
                        ? new DirectSoundOut(new Guid(Micobject.settings.deviceout), 100)
                        : new DirectSoundOut(100);

                    AudioSource.AudioFinished += AudioDeviceAudioFinished;
                    AudioSource.DataAvailable += AudioDeviceDataAvailable;
                    AudioSource.LevelChanged += AudioDeviceLevelChanged;

                    var l = new float[Micobject.settings.channels];
                    for (int i = 0; i < l.Length; i++)
                    {
                        l[i] = 0.0f;
                    }
                    AudioDeviceLevelChanged(this, new LevelChangedEventArgs(l));

                    if (!AudioSource.IsRunning && !IsClone && !(AudioSource is IVideoSource))
                    {
                        lock (_lockobject)
                        {
                            AudioSource.Start();
                        }
                    }
                }

                SoundDetected = false;
                _soundRecentlyDetected = false;
                Alerted = false;
                FlashCounter = DateTime.MinValue;
                _recordingTime = 0;
                ReconnectCount = 0;
                Listening = false;
                LastSoundDetected = Helper.Now;
                UpdateFloorplans(false);
                Micobject.settings.active = true;

                MainForm.NeedsSync = true;
                _requestRefresh = true;

                if (AudioDeviceEnabled != null)
                    AudioDeviceEnabled(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                if (ErrorHandler != null)
                    ErrorHandler(ex.Message);
            }
            _enabling = false;
        }

        internal string SourceType
        {
            get
            {
                switch (Micobject.settings.typeindex)
                {
                    default:
                        return "Local Device";
                    case 1:
                        return "iSpy Server";
                    case 2:
                        return "VLC";
                    case 3:
                        return "FFMPEG";
                    case 4:
                        return "Camera";
                }
            }

        }

        public void AudioDeviceLevelChanged(object sender, LevelChangedEventArgs eventArgs)
        {
            var f = eventArgs.MaxSamples.Max();

            if (Math.Abs(f) < float.Epsilon)
                return;
            Levels = eventArgs.MaxSamples;

            f = f*100;

            if (f >= Micobject.detector.minsensitivity  && f<= Micobject.detector.maxsensitivity)
            {
                TriggerDetect(sender);
            }
        }

        internal void TriggerDetect(object sender)
        {
            SoundDetected = true;
            _soundRecentlyDetected = true;
            InactiveRecord = 0;
            FlashCounter = Helper.Now.AddSeconds(10);
            MicrophoneAlarm(sender, EventArgs.Empty);
        }

        public void AudioDeviceDataAvailable(object sender, DataAvailableEventArgs e)
        {
            if (Levels == null || IsReconnect)
                return;
            try
            {
                if (!Recording)
                {
                    if (Micobject.settings.buffer > 0)
                    {
                        var dt = Helper.Now.AddSeconds(0 - Micobject.settings.buffer);
                        while (AudioBuffer.Count > 0 && AudioBuffer[0].TimeStamp < dt)
                        {
                            AudioBuffer[0].Nullify();
                            AudioBuffer.RemoveAt(0);
                        }
                        AudioBuffer.Add(new AudioAction(e.RawData, Levels.Max(), Helper.Now));
                    }
                }
                else
                {
                    WriterBuffer.Enqueue(new AudioAction(e.RawData, Levels.Max(), Helper.Now));
                }


                if (Micobject.settings.needsupdate)
                {
                    Micobject.settings.samples = AudioSource.RecordingFormat.SampleRate;
                    Micobject.settings.channels = AudioSource.RecordingFormat.Channels;
                    Micobject.settings.needsupdate = false;
                }

                OutSockets.RemoveAll(p => p.TcpClient.Client.Connected == false);
                if (OutSockets.Count>0)
                {
                    if (_mp3Writer == null)
                    {
                        _audioStreamFormat = new WaveFormat(22050, 16, Micobject.settings.channels);
                        var wf = new MP3Stream.WaveFormat(_audioStreamFormat.SampleRate, _audioStreamFormat.BitsPerSample, _audioStreamFormat.Channels);
                        _mp3Writer = new Mp3Writer(_outStream, wf, false);
                    }

                    byte[] bSrc = e.RawData;
                    int totBytes = bSrc.Length;

                    var ws = new TalkHelperStream(bSrc, totBytes, AudioSource.RecordingFormat);
                    var helpStm = new WaveFormatConversionStream(_audioStreamFormat, ws);
                    totBytes = helpStm.Read(_bResampled, 0, 22050);

                    ws.Close();
                    ws.Dispose();
                    helpStm.Close();
                    helpStm.Dispose();

                    _mp3Writer.Write(_bResampled, 0, totBytes);

                    var bterm = Encoding.ASCII.GetBytes("\r\n");

                    if (_outStream.Length > 0)
                    {
                        var bout = new byte[(int) _outStream.Length];

                        _outStream.Seek(0, SeekOrigin.Begin);
                        _outStream.Read(bout, 0, (int) _outStream.Length);

                        _outStream.SetLength(0);
                        _outStream.Seek(0, SeekOrigin.Begin);

                        foreach (var s in OutSockets)
                        {
                            var b = Encoding.ASCII.GetBytes(bout.Length.ToString("X") + "\r\n");
                            s.Stream.Write(b, 0, b.Length);
                            s.Stream.Write(bout, 0, bout.Length);
                            s.Stream.Write(bterm, 0, bterm.Length);
                        }
                    }

                }
                else
                {
                    if (_mp3Writer != null)
                    {
                        _mp3Writer.Close();
                        _mp3Writer = null;
                    }
                }


                if (DataAvailable != null)
                {
                    DataAvailable(this, new NewDataAvailableArgs((byte[])e.RawData.Clone()));
                }

                if (_reconnectTime != DateTime.MinValue)
                {
                    Micobject.settings.active = true;
                    _errorTime = _reconnectTime = DateTime.MinValue;
                    DoAlert("reconnect");
                }
                _errorTime = DateTime.MinValue;

            }
            catch (Exception ex)
            {
                if (ErrorHandler != null)
                    ErrorHandler(ex.Message);
            }
        }

        private void ProcessAlertEvent(string mode, string pluginmessage, string type, string param1, string param2, string param3, string param4)
        {
            string id = Micobject.id.ToString(CultureInfo.InvariantCulture);

            param1 = param1.Replace("{ID}", id).Replace("{NAME}", Micobject.name).Replace("{MSG}", pluginmessage);
            param2 = param2.Replace("{ID}", id).Replace("{NAME}", Micobject.name).Replace("{MSG}", pluginmessage);
            param3 = param3.Replace("{ID}", id).Replace("{NAME}", Micobject.name).Replace("{MSG}", pluginmessage);
            param4 = param4.Replace("{ID}", id).Replace("{NAME}", Micobject.name).Replace("{MSG}", pluginmessage);

            try
            {
                switch (type)
                {
                    case "Exe":
                        {
                            if (param1.ToLower() == "ispy.exe" || param1.ToLower() == "ispy")
                            {
                                var topLevelControl = (MainForm)TopLevelControl;
                                if (topLevelControl != null) topLevelControl.ProcessCommandString(param2);
                            }
                            else
                            {
                                try
                                {

                                    var startInfo = new ProcessStartInfo
                                    {
                                        UseShellExecute = true,
                                        FileName = param1,
                                        Arguments = param2
                                    };
                                    try
                                    {
                                        var fi = new FileInfo(param1);
                                        if (fi.DirectoryName != null)
                                            startInfo.WorkingDirectory = fi.DirectoryName;
                                    }
                                    catch(Exception ex)
                                    {
                                        if (ErrorHandler != null)
                                            ErrorHandler(ex.Message);
                                    }
                                    if (!MainForm.Conf.CreateAlertWindows)
                                    {
                                        startInfo.CreateNoWindow = true;
                                        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                    }

                                    Process.Start(startInfo);
                                }
                                catch (Exception e)
                                {
                                    if (ErrorHandler != null)
                                        ErrorHandler(e.Message);
                                }
                            }
                        }
                        break;
                    case "URL":
                        {
                            var request = (HttpWebRequest)WebRequest.Create(param1);
                            request.Credentials = CredentialCache.DefaultCredentials;
                            var response = (HttpWebResponse)request.GetResponse();

                            // Get the stream associated with the response.
                            Stream receiveStream = response.GetResponseStream();

                            // Pipes the stream to a higher level stream reader with the required encoding format. 
                            if (receiveStream != null)
                            {
                                var readStream = new StreamReader(receiveStream, Encoding.UTF8);
                                readStream.ReadToEnd();
                                response.Close();
                                readStream.Close();
                                receiveStream.Close();
                            }
                            response.Close();
                        }
                        break;
                    case "NM": //network message
                        switch (param1)
                        {
                            case "TCP":
                                {
                                    IPAddress ip;
                                    if (IPAddress.TryParse(param2, out ip))
                                    {
                                        int port;
                                        if (int.TryParse(param3, out port))
                                        {
                                            using (var tcpClient = new TcpClient())
                                            {
                                                try
                                                {
                                                    tcpClient.Connect(ip, port);
                                                    using (var networkStream = tcpClient.GetStream())
                                                    {
                                                        using (var clientStreamWriter = new StreamWriter(networkStream))
                                                        {
                                                            clientStreamWriter.Write(param4);
                                                            clientStreamWriter.Flush();
                                                            clientStreamWriter.Close();
                                                        }
                                                        networkStream.Close();
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    if (ErrorHandler != null)
                                                        ErrorHandler(ex.Message);
                                                }
                                                tcpClient.Close();
                                            }

                                        }
                                    }
                                }

                                break;
                            case "UDP":
                                {
                                    IPAddress ip;
                                    if (IPAddress.TryParse(param2, out ip))
                                    {
                                        int port;
                                        if (int.TryParse(param3, out port))
                                        {
                                            using (var udpClient = new UdpClient())
                                            {
                                                try
                                                {

                                                    udpClient.Connect(ip, port);
                                                    var cmd = Encoding.ASCII.GetBytes(param4);
                                                    udpClient.Send(cmd, cmd.Length);

                                                }
                                                catch (Exception ex)
                                                {
                                                    if (ErrorHandler != null)
                                                        ErrorHandler(ex.Message);
                                                }
                                                finally
                                                {
                                                    udpClient.Close();
                                                }
                                            }

                                        }
                                    }
                                }
                                break;
                        }
                        break;
                    case "S":
                        try
                        {
                            using (var sp = new SoundPlayer(param1))
                            {
                                sp.Play();
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ErrorHandler != null)
                                ErrorHandler(ex.Message);
                        }
                        break;
                    case "SW":
                        RemoteCommand(this, new ThreadSafeCommand("show"));
                        break;
                    case "B":
                        Console.Beep();
                        break;
                    case "M":
                        if (TopLevelControl != null)
                        {
                            var mf = ((MainForm)TopLevelControl);
                            mf.Maximise(this, false);
                        }
                        break;
                    case "TA":
                        {
                            if (TopLevelControl != null)
                            {
                                string[] tid = param1.Split(',');
                                switch (tid[0])
                                {
                                    case "1":
                                        VolumeLevel vl = ((MainForm)TopLevelControl).GetVolumeLevel(Convert.ToInt32(tid[1]));
                                        if (vl != null && vl != this)
                                            vl.MicrophoneAlarm(this, EventArgs.Empty);
                                        break;
                                    case "2":
                                        CameraWindow cw = ((MainForm)TopLevelControl).GetCameraWindow(Convert.ToInt32(tid[1]));
                                        if (cw != null)
                                            cw.CameraAlarm(this, EventArgs.Empty);
                                        break;
                                }
                            }
                        }
                        break;
                    case "SOO":
                        {
                            if (TopLevelControl != null)
                            {
                                string[] tid = param1.Split(',');
                                switch (tid[0])
                                {
                                    case "1":
                                        VolumeLevel vl =
                                            ((MainForm)TopLevelControl).GetVolumeLevel(Convert.ToInt32(tid[1]));
                                        if (vl != null && vl != this)
                                            vl.Enable();
                                        break;
                                    case "2":
                                        CameraWindow cw =
                                            ((MainForm)TopLevelControl).GetCameraWindow(Convert.ToInt32(tid[1]));
                                        if (cw != null)
                                            cw.Enable();
                                        break;
                                }
                            }
                        }
                        break;
                    case "E":
                        {
                            string subject = MailMerge(MainForm.Conf.MailAlertSubject, mode, Recording, pluginmessage);
                            string message = MailMerge(MainForm.Conf.MailAlertBody, mode,Recording, pluginmessage);

                            message += MainForm.Conf.AppendLinkText;

                            WsWrapper.SendAlert(param1, subject, message);
                        }
                        break;
                    case "SMS":
                        {
                            string message = MailMerge(MainForm.Conf.SMSAlert, mode, Recording, pluginmessage);
                            if (message.Length > 160)
                                message = message.Substring(0, 159);

                            WsWrapper.SendSms(param1, message);
                        }
                        break;
                    case "TM":
                        {
                            string message = MailMerge(MainForm.Conf.SMSAlert, mode, Recording, pluginmessage);
                            if (message.Length > 160)
                                message = message.Substring(0, 159);

                            WsWrapper.SendTweet(message + " " + MainForm.Webserver + "/mobile/");
                        }
                        break;

                }
            }
            catch (Exception ex)
            {
                if (ErrorHandler != null)
                    ErrorHandler(ex.Message);
            }
        }

        public void AudioDeviceAudioFinished(object sender, ReasonToFinishPlaying reason)
        {
            if (IsReconnect)
                return;

            if (IsClone)
            {
                SetErrorState("Mic source offline");
                Levels = null;

                if (!ShuttingDown)
                    _requestRefresh = true;

                return;
            }
            

            switch (reason)
            {
                case ReasonToFinishPlaying.DeviceLost:
                    SetErrorState("Device Lost");
                    break;
                case ReasonToFinishPlaying.EndOfStreamReached:
                    SetErrorState("End of Stream");
                    break;
                case ReasonToFinishPlaying.VideoSourceError:
                    SetErrorState("Source Error");
                    break;
                case ReasonToFinishPlaying.StoppedByUser:
                    Disable(false);
                    break;
            }
            
            Levels = null;

            if (!ShuttingDown)
                _requestRefresh = true;
        }

        private void SetErrorState(string reason)
        {
            AudioSourceErrorMessage = reason;
            if (!AudioSourceErrorState)
            {
                AudioSourceErrorState = true;
                if (ErrorHandler != null)
                    ErrorHandler(reason);

                if (_reconnectTime == DateTime.MinValue)
                {
                    _reconnectTime = Helper.Now;
                }
                if (_errorTime == DateTime.MinValue)
                    _errorTime = Helper.Now;
            }
        }

        private void UpdateFloorplans(bool isAlert)
        {
            foreach (
                var ofp in
                    MainForm.FloorPlans.Where(
                        p => p.objects.@object.Count(q => q.type == "microphone" && q.id == Micobject.id) > 0).
                        ToList())
            {
                ofp.needsupdate = true;
                if (isAlert)
                {
                    if (TopLevelControl != null)
                    {
                        FloorPlanControl fpc = ((MainForm) TopLevelControl).GetFloorPlan(ofp.id);
                        fpc.LastAlertTimestamp = Helper.Now.UnixTicks();
                        fpc.LastOid = Micobject.id;
                        fpc.LastOtid = 1;
                    }
                }
            }
        }

        #region Nested type: AudioAction

        public struct AudioAction
        {
            public byte[] Decoded;
            public readonly double SoundLevel;
            public readonly DateTime TimeStamp;

            public AudioAction(byte[] decoded, double soundLevel, DateTime timestamp)
            {
                Decoded = decoded;
                SoundLevel = soundLevel;
                TimeStamp = timestamp;
            }
            public void Nullify()
            {
                Decoded = null;
            }
        }

        #endregion

        public void MicrophoneAlarm(object sender, EventArgs e)
        {
            LastSoundDetected = Helper.Now.AddSeconds(0.3d);

            if (Micobject.detector.recordondetect)
            {
                StartSaving();
            }
            if (sender is IAudioSource)
            {
                FlashCounter = Helper.Now.AddSeconds(10);
                SoundDetected = true;
                return;
            }

            if (sender is LocalServer || sender is VolumeLevel || sender is CameraWindow)
            {
                FlashCounter = Helper.Now.AddSeconds(10);
                DoAlert("alert");
            }
        }


        public ReadOnlyCollection<FilesFile> FileList
        {
            get { return _filelist.AsReadOnly(); }
        }

        public void AddFile(FilesFile f)
        {
            lock (_lockobject)
            {
                _filelist.Add(f);
            }
        }

        private void DoAlert(string type, string msg = "")
        {
            if (IsEdit)
                return;

            if (type == "alert")
            {
                if (Alerted)
                {
                    if ((Helper.Now - LastAlerted).TotalSeconds < Micobject.alerts.minimuminterval)
                    {
                        return;
                    }
                }

                Alerted = true;
                UpdateFloorplans(true);
                LastAlerted = Helper.Now;
                RemoteCommand(this, new ThreadSafeCommand("bringtofrontmic," + Micobject.id));
                if (Micobject.detector.recordonalert)
                {
                    StartSaving();
                }
                
            }
            var t = new Thread(() => AlertThread(type, msg, Micobject.id)) { Name = type + " (" + Micobject.id + ")", IsBackground = false };
            t.Start();           
        }

        private void AlertThread(string mode, string msg, int oid)
        {
            if (Notification != null)
            {
                Notification(this, new NotificationType(mode, Micobject.name, "", ""));
            }

            if (MainForm.Conf.ScreensaverWakeup)
                ScreenSaver.KillScreenSaver();

            int i = 0;
            foreach (var ev in MainForm.Actions.Where(p => p.objectid == oid && p.objecttypeid == 1 && p.mode == mode))
            {
                ProcessAlertEvent(ev.mode, msg, ev.type, ev.param1, ev.param2, ev.param3, ev.param4);
                i++;
            }
            if (i>0)
                MainForm.LastAlert = Helper.Now;
            
        }

        public string RecordSwitch(bool record)
        {
            if (!Helper.HasFeature(Enums.Features.Recording))
                return "notrecording," + LocRm.GetString("RecordingStopped");
            if (record)
            {
                if (!IsEnabled)
                {
                    Enable();
                }
                ForcedRecording = true;
                StartSaving();
                return "recording," + LocRm.GetString("RecordingStarted");
            }

            ForcedRecording = false;
            
            if (_pairedRecording)
            {
                var cc = CameraControl;
                if (cc != null && cc.Recording)
                    cc.RecordSwitch(false);
                StopSaving();
            }
            else
            {
                StopSaving();
            }
            
            return "notrecording," + LocRm.GetString("RecordingStopped");
        }

        public void Talk(IWin32Window f = null)
        {
            //throw new NotImplementedException();
        }

        public void Listen()
        {
            if (CameraControl!=null)
                CameraControl.Listen();
            else
                Listening = !Listening;
        }

        public string SaveFrame(Bitmap bmp=null)
        {
            //throw new NotImplementedException();
            return "";
        }

        public void ApplySchedule()
        {
            if (!Micobject.schedule.active || Micobject.schedule == null || Micobject.schedule.entries == null ||
                !Micobject.schedule.entries.Any())
                return;
            //find most recent schedule entry
            DateTime dNow = DateTime.Now;
            TimeSpan shortest = TimeSpan.MaxValue;
            objectsMicrophoneScheduleEntry mostrecent = null;
            bool isstart = true;

            foreach (objectsMicrophoneScheduleEntry entry in Micobject.schedule.entries)
            {
                if (entry.active)
                {
                    string[] dows = entry.daysofweek.Split(',');
                    foreach (string dayofweek in dows)
                    {
                        int dow = Convert.ToInt32(dayofweek);
                        //when did this last fire?
                        if (entry.start.IndexOf("-", StringComparison.Ordinal) == -1)
                        {
                            string[] start = entry.start.Split(':');
                            var dtstart = new DateTime(dNow.Year, dNow.Month, dNow.Day, Convert.ToInt32(start[0]),
                                                       Convert.ToInt32(start[1]), 0);
                            while ((int) dtstart.DayOfWeek != dow || dtstart > dNow)
                                dtstart = dtstart.AddDays(-1);
                            if (dNow - dtstart < shortest)
                            {
                                shortest = dNow - dtstart;
                                mostrecent = entry;
                                isstart = true;
                            }
                        }
                        if (entry.stop.IndexOf("-", StringComparison.Ordinal) == -1)
                        {
                            string[] stop = entry.stop.Split(':');
                            var dtstop = new DateTime(dNow.Year, dNow.Month, dNow.Day, Convert.ToInt32(stop[0]),
                                                      Convert.ToInt32(stop[1]), 0);
                            while ((int) dtstop.DayOfWeek != dow || dtstop > dNow)
                                dtstop = dtstop.AddDays(-1);
                            if (dNow - dtstop < shortest)
                            {
                                shortest = dNow - dtstop;
                                mostrecent = entry;
                                isstart = false;
                            }
                        }
                    }
                }
            }
            if (mostrecent != null)
            {
                if (isstart)
                {
                    Micobject.detector.recordondetect = mostrecent.recordondetect;
                    Micobject.detector.recordonalert = mostrecent.recordonalert;
                    Micobject.alerts.active = mostrecent.alerts;
                    if (!IsEnabled)
                        Enable();
                    if (mostrecent.recordonstart)
                    {
                        ForcedRecording = true;
                        StartSaving();
                    }
                }
                else
                {
                    if (IsEnabled)
                        Disable();
                }
            }
        }

        #region Nested type: VolumeChangedEventArgs

        public class VolumeChangedEventArgs : EventArgs
        {
            public int NewLevel;

            public VolumeChangedEventArgs(int newLevel)
            {
                NewLevel = newLevel;
            }
        }

        #endregion
    }

    public class NewDataAvailableArgs : EventArgs
    {
        private readonly byte[] _decodedData;

        public NewDataAvailableArgs(byte[] decodedData)
        {
            _decodedData = decodedData;
        }

        public byte[] DecodedData
        {
            get { return _decodedData; }
        }
    }
}