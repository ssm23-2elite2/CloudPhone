﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using AForge.Video.DirectShow;
using iSpyApplication.Controls;
using iSpyApplication.Properties;
using iSpyApplication.Video;

namespace iSpyApplication
{
    public partial class MainForm
    {
        private static configuration _conf;
        private static FilterInfoCollection _videoFilters;
        private static Color _backColor = Color.Empty;
        private static Color _borderDefaultColor = Color.Empty;
        private static Color _borderHighlightColor = Color.Empty;
        private static Color _floorPlanHighlightColor = Color.Empty;
        private static Color _volumeLevelColor = Color.Empty;
        private static Color _activityColor = Color.Empty;
        private static Color _noActivityColor = Color.Empty;
        private static readonly IPAddress[] Ipv6EmptyList = { };

        public static void ReloadColors()
        {
            _backColor =
                _borderDefaultColor =
                _borderHighlightColor =
                _floorPlanHighlightColor = _volumeLevelColor = _activityColor = _noActivityColor = Color.Empty;
        }

        public static Color BackgroundColor
        {
            get
            {
                if (_backColor == Color.Empty)
                    _backColor = Conf.BackColor.ToColor();
                return _backColor;
            }
            set { _backColor = value; }
        }


        public static Color BorderDefaultColor
        {
            get
            {
                if (_borderDefaultColor == Color.Empty)
                    _borderDefaultColor = Conf.BorderDefaultColor.ToColor();
                return _borderDefaultColor;
            }
            set { _borderDefaultColor = value; }
        }


        public static Color BorderHighlightColor
        {
            get
            {
                if (_borderHighlightColor == Color.Empty)
                    _borderHighlightColor = Conf.BorderHighlightColor.ToColor();
                return _borderHighlightColor;
            }
            set { _borderHighlightColor = value; }
        }


        public static Color FloorPlanHighlightColor
        {
            get
            {
                if (_floorPlanHighlightColor == Color.Empty)
                    _floorPlanHighlightColor = Conf.FloorPlanHighlightColor.ToColor();
                return _floorPlanHighlightColor;
            }
            set { _floorPlanHighlightColor = value; }
        }


        public static Color VolumeLevelColor
        {
            get
            {
                if (_volumeLevelColor == Color.Empty)
                    _volumeLevelColor = Conf.VolumeLevelColor.ToColor();
                return _volumeLevelColor;
            }
            set { _volumeLevelColor = value; }
        }


        public static Color ActivityColor
        {
            get
            {
                if (_activityColor == Color.Empty)
                    _activityColor = Conf.ActivityColor.ToColor();
                return _activityColor;
            }
            set { _activityColor = value; }
        }


        public static Color NoActivityColor
        {
            get
            {
                if (_noActivityColor == Color.Empty)
                    _noActivityColor = Conf.NoActivityColor.ToColor();
                return _noActivityColor;
            }
            set { _noActivityColor = value; }
        }




        public static FilterInfoCollection VideoFilters
        {
            get { return _videoFilters ?? (_videoFilters = new FilterInfoCollection(FilterCategory.VideoInputDevice)); }
        }

        public static ImageCodecInfo Encoder
        {
            get
            {
                if (_encoder != null)
                    return _encoder;
                ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

                foreach (ImageCodecInfo codec in codecs)
                {
                    if (codec.FormatID == ImageFormat.Jpeg.Guid)
                    {
                        _encoder = codec;
                        return codec;
                    }
                }
                return null;
            }
        }

        public static configuration Conf
        {
            get
            {
                if (_conf != null)
                    return _conf;
                var s = new XmlSerializer(typeof(configuration));
                bool loaded = false;
                lock (ThreadLock)
                {
                    using (var fs = new FileStream(Program.AppDataPath + @"XML\config.xml", FileMode.Open))
                    {
                        try
                        {
                            using (TextReader reader = new StreamReader(fs))
                            {
                                fs.Position = 0;
                                _conf = (configuration)s.Deserialize(reader);
                                loaded = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogExceptionToFile(ex);
                        }
                    }
                }

                if (!loaded)
                {
                    //copy over new config file
                    try
                    {
                        var didest = new DirectoryInfo(Program.AppDataPath + @"XML\");
                        var disource = new DirectoryInfo(Program.AppPath + @"XML\");
                        File.Copy(disource + @"config.xml", didest + @"config.xml", true);


                        using (var fs = new FileStream(Program.AppDataPath + @"XML\config.xml", FileMode.Open))
                        {
                            fs.Position = 0;
                            using (TextReader reader = new StreamReader(fs))
                            {
                                _conf = (configuration)s.Deserialize(reader);
                                reader.Close();
                            }
                            fs.Close();
                        }
                    }
                    catch (Exception ex2)
                    {
                        string m = LocRm.GetString("CouldNotLoadRestore")+Environment.NewLine+ex2.Message;
                        MessageBox.Show(m);
                        LogMessageToFile(m);
                        throw;
                    }
                }
                if (_conf.CPUMax == 0)
                    _conf.CPUMax = 90;
                if (_conf.MaxRecordingThreads == 0)
                    _conf.MaxRecordingThreads = 20;
                if (_conf.Reseller == null)
                    _conf.Reseller = "";
                if (_conf.AllowedIPList == null)
                {
                    _conf.AllowedIPList = "";
                }
                if (_conf.MaxRedrawRate == 0)
                    _conf.MaxRedrawRate = 10;
                if (_conf.PreviewItems == 0)
                {
                    _conf.PreviewItems = 100;
                    _conf.ShowOverlayControls = true;
                    _conf.ShowMediaPanel = true;
                }
                if (_conf.IPMode != "IPv6")
                    _conf.IPMode = "IPv4";

                if (_conf.Priority == 0)
                {
                    _conf.Priority = 3;
                    _conf.Monitor = true;
                }
                if (_conf.JPEGQuality == 0)
                    _conf.JPEGQuality = 80;

                if (String.IsNullOrEmpty(_conf.FloorPlanHighlightColor))
                    _conf.FloorPlanHighlightColor = "0,217,0";

                if (String.IsNullOrEmpty(_conf.YouTubeCategories))
                {
                    _conf.YouTubeCategories =
                        "Film,Autos,Music,Animals,Sports,Travel,Games,Comedy,People,News,Entertainment,Education,Howto,Nonprofit,Tech";
                }
                if (String.IsNullOrEmpty(_conf.BorderHighlightColor))
                {
                    _conf.BorderHighlightColor = "255,0,0";
                }
                if (!String.IsNullOrEmpty(Resources.Vendor))
                {
                    _conf.Vendor = Resources.Vendor;
                }
                if (String.IsNullOrEmpty(_conf.BorderDefaultColor))
                    _conf.BorderDefaultColor = "0,0,0";

                if (String.IsNullOrEmpty(_conf.StartupForm))
                    _conf.StartupForm = "iSpy";

                if (_conf.GridViews == null)
                    _conf.GridViews = new configurationGrid[] { };

                if (_conf.Joystick == null)
                    _conf.Joystick = new configurationJoystick();

                if (String.IsNullOrEmpty(_conf.AppendLinkText))
                    _conf.AppendLinkText = "<br/>ispyconnect.com";

                if (_conf.FeatureSet < 1)
                    _conf.FeatureSet = 1;

                _conf.IPv6Disabled = true;

                if (_conf.FTPServers == null)
                    _conf.FTPServers = new configurationServer[] {};

                //can fail on windows xp/vista with a very very nasty error
                if (IsWinSevenOrHigher())
                {
                    _conf.IPv6Disabled = !(Socket.OSSupportsIPv6);
                }

                return _conf;
            }
        }

        static bool IsWinSevenOrHigher()
        {
            return (Environment.OSVersion.Platform == PlatformID.Win32NT) && (Environment.OSVersion.Version.Major >= 7);
        }


        public static List<objectsCamera> Cameras
        {
            get
            {
                if (_cameras == null)
                {
                    LoadObjects(Program.AppDataPath + @"XML\objects.xml");
                }
                return _cameras;
            }
            set { _cameras = value; }
        }

        public static List<objectsActionsEntry> Actions
        {
            get
            {
                if (_actions == null)
                {
                    LoadObjects(Program.AppDataPath + @"XML\objects.xml");
                }
                return _actions;
            }
            set { _actions = value; }
        }

        public static List<PTZSettings2Camera> PTZs
        {
            get
            {
                if (_ptzs == null)
                {
                    LoadPTZs(Program.AppDataPath + @"XML\PTZ2.xml");
                }
                return _ptzs;
            }
            set { _ptzs = value; }
        }

        public static List<ManufacturersManufacturer> Sources
        {
            get
            {
                if (_sources == null)
                {
                    LoadSources(Program.AppDataPath + @"XML\Sources.xml");
                }
                return _sources;
            }
            set { _sources = value; }
        }



        public static List<objectsMicrophone> Microphones
        {
            get
            {
                if (_microphones == null)
                {
                    LoadObjects(Program.AppDataPath + @"XML\objects.xml");
                }
                return _microphones;
            }
            set { _microphones = value; }
        }

        public static List<objectsCommand> RemoteCommands
        {
            get
            {
                if (_remotecommands == null)
                {
                    LoadObjects(Program.AppDataPath + @"XML\objects.xml");
                }
                return _remotecommands;
            }
            set { _remotecommands = value; }
        }

        public static List<objectsFloorplan> FloorPlans
        {
            get
            {
                if (_floorplans == null)
                {
                    LoadObjects(Program.AppDataPath + @"XML\objects.xml");
                }
                return _floorplans;
            }
            set { _floorplans = value; }
        }


        public static IPAddress[] AddressListIPv4
        {
            get
            {
                if (_ipv4Addresses != null)
                    return _ipv4Addresses;

                IPAddress[] arr;
                try
                {
                    arr =
                        Dns.GetHostEntry(Dns.GetHostName())
                            .AddressList.Where(p => p.AddressFamily == AddressFamily.InterNetwork)
                            .ToArray();//attempts reverse dns lookup
                }
                catch (Exception ex)
                {
                    LogExceptionToFile(ex);
                    try
                    {
                        arr = Dns.GetHostAddresses(Dns.GetHostName());
                    }
                    catch (Exception ex2)
                    {
                        LogExceptionToFile(ex2);
                        //none in the system - just use the loopback address
                        _ipv4Addresses = new[] { System.Net.IPAddress.Parse("127.0.0.1") };
                        return _ipv4Addresses;
                    }
                    //ignore lookup
                }
                _ipv4Addresses = arr.Where(IsValidIP).ToArray();

                if (!_ipv4Addresses.Any()) //none in the system - just use the loopback address
                    _ipv4Addresses = new[] { System.Net.IPAddress.Parse("127.0.0.1") };
                return _ipv4Addresses;
            }
        }

        //IPv6
        public static IPAddress[] AddressListIPv6
        {
            get
            {
                if (Conf.IPv6Disabled)
                {
                    return Ipv6EmptyList;
                }

                if (_ipv6Addresses != null)
                    return _ipv6Addresses;

                var ipv6Adds = new List<IPAddress>();
                if (Conf.IPv6Disabled)
                {
                    _ipv6Addresses = ipv6Adds.ToArray();
                    return _ipv6Addresses;
                }

                try
                {
                    var addressInfoCollection = IPGlobalProperties.GetIPGlobalProperties().GetUnicastAddresses();

                    foreach (var addressInfo in addressInfoCollection)
                    {
                        if (addressInfo.Address.IsIPv6Teredo ||
                            (addressInfo.Address.AddressFamily == AddressFamily.InterNetworkV6 &&
                            !addressInfo.Address.IsIPv6LinkLocal && !addressInfo.Address.IsIPv6SiteLocal))
                        {
                            if (!System.Net.IPAddress.IsLoopback(addressInfo.Address))
                            {
                                ipv6Adds.Add(addressInfo.Address);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //unsupported on win xp
                    LogExceptionToFile(ex);
                }
                _ipv6Addresses = ipv6Adds.ToArray();
                return _ipv6Addresses;

            }
        }

        private static string _ipv4Address = "";
        public static string AddressIPv4
        {
            get
            {
                lock (ThreadLock)
                {
                    if (_ipv4Address != "")
                        return _ipv4Address;

                    string detectip = "";
                    foreach (IPAddress ip in AddressListIPv4)
                    {
                        if (detectip == "")
                            detectip = ip.ToString();

                        if (Conf.IPv4Address == ip.ToString())
                        {
                            _ipv4Address = ip.ToString();
                            break;
                        }

                        if (IsValidIP(ip))
                        {
                            detectip = ip.ToString();
                            break;
                        }
                    }
                    if (_ipv4Address == "")
                        _ipv4Address = detectip;
                    if (_ipv4Address == "")
                    {
                        LogErrorToFile(
                            "Unable to find a suitable IP address, check your network connection. Using the local loopback address.");
                        _ipv4Address = "127.0.0.1";
                    }
                    if (Conf.IPv4Address != _ipv4Address)
                    {
                        if (!String.IsNullOrWhiteSpace(Conf.IPv4Address))
                        {
                            LogErrorToFile("Your previous IP address has changed. Consider setting up your PC with a static IP address.");
                        }
                    }

                    Conf.IPv4Address = _ipv4Address;
                    return _ipv4Address;
                }
            }
            set
            {
                lock (ThreadLock)
                {
                    _ipv4Address = value;
                }
            }
        }

        private static bool IsValidIP(IPAddress ip)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                if (!System.Net.IPAddress.IsLoopback(ip))
                {
                    var sip = ip.ToString().Split('.');
                    switch (sip[0])
                    {
                        default:
                            return true;
                        case "0":
                        case "127":
                        case "169":

                            break;
                    }
                }
            }
            return false;
        }

        //IPv6
        private static string _ipv6Address = "";

        public static string AddressIPv6
        {
            get
            {
                lock (ThreadLock)
                {
                    if (_ipv6Address != "")
                        return _ipv6Address;

                    string detectip = "";
                    foreach (IPAddress ip in AddressListIPv6)
                    {
                        if (detectip == "")
                            detectip = ip.ToString();

                        if (Conf.IPv6Address == ip.ToString())
                        {
                            _ipv6Address = ip.ToString();
                            break;
                        }

                        if (ip.IsIPv6Teredo)
                        {
                            detectip = ip.ToString();
                        }
                    }

                    if (_ipv6Address == "")
                        _ipv6Address = detectip;
                    Conf.IPv6Address = _ipv6Address;

                    return _ipv6Address;
                }

            }
            set
            {
                lock (ThreadLock)
                {
                    _ipv6Address = value;
                }
            }
        }

        public static string IPAddress
        {
            get
            {
                if (Conf.IPMode == "IPv4")
                    return AddressIPv4;
                return MakeIPv6Url(AddressIPv6);
            }
        }

        public static string IPAddressExternal
        {
            get
            {
                if (Conf.IPMode == "IPv4")
                    return WsWrapper.ExternalIPv4(false);
                return MakeIPv6Url(AddressIPv6);
            }
        }

        private static string MakeIPv6Url(string ip)
        {
            //strip scope id
            if (ip.IndexOf("%", StringComparison.Ordinal) != -1)
                ip = ip.Substring(0, ip.IndexOf("%", StringComparison.Ordinal));
            return "[" + ip + "]";
        }

        private static void LoadObjects(string path)
        {
            try
            {
                objects c;
                lock (ThreadLock)
                {
                    using (var fs = new FileStream(path, FileMode.Open))
                    {
                        var s = new XmlSerializer(typeof(objects));
                        using (TextReader reader = new StreamReader(fs))
                        {
                            fs.Position = 0;
                            c = (objects)s.Deserialize(reader);
                        }
                    }
                }

                _cameras = c.cameras != null ? c.cameras.ToList() : new List<objectsCamera>();
                bool addActions = c.actions == null;
                _actions = addActions ? new List<objectsActionsEntry>() : c.actions.entries.ToList();


                if (_cameras.Select(oc => oc.settings.desktopresizewidth).Any(rw => rw == 0))
                {
                    throw new Exception("err_old_config");
                }

                _microphones = c.microphones != null ? c.microphones.ToList() : new List<objectsMicrophone>();

                _floorplans = c.floorplans != null ? c.floorplans.ToList() : new List<objectsFloorplan>();

                _remotecommands = c.remotecommands != null ? c.remotecommands.ToList() : new List<objectsCommand>();

                if (_remotecommands.Count == 0)
                {
                    InitRemoteCommands();
                }

                bool bVlc = VlcHelper.VlcInstalled;

                bool bAlertVlc = false;
                int camid = 0;
                string path2;
                _cameras = _cameras.OrderByDescending(p => p.settings.sourceindex != 10).ToList();
                foreach (objectsCamera cam in _cameras)
                {
                    if (cam.id >= camid)
                        camid = cam.id + 1;

                    path2 = Helper.GetMediaDirectory(2, cam.id) + "video\\" + cam.directory + "\\";
                    if (cam.settings.sourceindex == 5 && !bVlc)
                    {
                        bAlertVlc = true;
                    }
                    if (cam.settings.youtube == null)
                    {
                        cam.settings.youtube = new objectsCameraSettingsYoutube
                                {
                                    category = Conf.YouTubeDefaultCategory,
                                    tags = "iSpy, Motion Detection, Surveillance",
                                    @public = true
                                };
                    }
                    if (cam.ptzschedule == null)
                    {
                        cam.ptzschedule = new objectsCameraPtzschedule
                                {
                                    active = false,
                                    entries = new objectsCameraPtzscheduleEntry[] {}
                                };
                    }
                    if (cam.settings.storagemanagement == null)
                    {
                        cam.settings.storagemanagement = new objectsCameraSettingsStoragemanagement
                                {
                                    enabled = false,
                                    maxage = 72,
                                    maxsize = 1024

                                };
                    }
                    bool migrate = false;
                    if (cam.alertevents == null)
                    {
                        cam.alertevents = new objectsCameraAlertevents();
                        migrate = true;
                    }
                    
                    if (cam.settings.cloudprovider==null)
                        cam.settings.cloudprovider = new objectsCameraSettingsCloudprovider();

                    if (cam.alertevents.entries == null)
                        cam.alertevents.entries = new objectsCameraAlerteventsEntry[] {};

                    if (migrate)
                    {
                        var l = new List<objectsCameraAlerteventsEntry>();
                        if (!String.IsNullOrEmpty(cam.alerts.executefile))
                        {
                            l.Add(new objectsCameraAlerteventsEntry
                                  {
                                      type = "Exe",
                                      param1 = cam.alerts.executefile,
                                      param2 = cam.alerts.arguments
                                  });
                        }
                        if (cam.notifications.sendemail)
                        {
                            l.Add(new objectsCameraAlerteventsEntry
                                  {
                                      type = "E",
                                      param1 = cam.settings.emailaddress,
                                      param2 = "True"
                                  });
                        }
                        if (cam.notifications.sendsms)
                        {
                            l.Add(new objectsCameraAlerteventsEntry {type = "SMS", param1 = cam.settings.smsnumber});
                        }
                        if (cam.alerts.maximise)
                        {
                            l.Add(new objectsCameraAlerteventsEntry {type = "M"});
                        }
                        if (!String.IsNullOrEmpty(cam.alerts.playsound))
                        {
                            l.Add(new objectsCameraAlerteventsEntry {type = "S", param1 = cam.alerts.playsound});
                        }

                        string[] alertOptions = cam.alerts.alertoptions.Split(','); //beep,restore

                        if (Convert.ToBoolean(alertOptions[0]))
                            l.Add(new objectsCameraAlerteventsEntry {type = "B"});
                        if (Convert.ToBoolean(alertOptions[1]))
                            l.Add(new objectsCameraAlerteventsEntry {type = "SW"});

                        if (cam.notifications.sendtwitter)
                        {
                            l.Add(new objectsCameraAlerteventsEntry {type = "TM"});
                        }

                        if (!String.IsNullOrEmpty(cam.alerts.trigger))
                        {
                            l.Add(new objectsCameraAlerteventsEntry {type = "TA", param1 = cam.alerts.trigger});
                        }
                        cam.alertevents.entries = l.ToArray();

                    }
                    
                    if (addActions)
                    {
                        _actions.AddRange(cam.alertevents.entries.Select(a => new objectsActionsEntry
                                                                       {
                                                                           mode = "alert", 
                                                                           objectid = cam.id, 
                                                                           objecttypeid = 2,
                                                                           type = a.type,
                                                                           param1 = a.param1, 
                                                                           param2 = a.param2, 
                                                                           param3 = a.param3, 
                                                                           param4 = a.param4
                                                                       }));
                        if (!String.IsNullOrEmpty(cam.settings.emailondisconnect))
                        {
                            _actions.Add(new objectsActionsEntry
                            {
                                mode = "disconnect",
                                objectid = cam.id,
                                objecttypeid = 2,
                                type = "E",
                                param1 = cam.settings.emailondisconnect,
                                param2 = "False"
                            });
                        }
                    }
                    
                    cam.newrecordingcount = 0;
                    if (cam.settings.maxframerate == 0)
                        cam.settings.maxframerate = 10;
                    if (cam.settings.maxframeraterecord == 0)
                        cam.settings.maxframeraterecord = 10;
                    if (cam.settings.timestampfontsize == 0)
                        cam.settings.timestampfontsize = 10;
                    if (cam.recorder.timelapsesave == 0)
                        cam.recorder.timelapsesave = 60;

                    if (cam.x < 0)
                        cam.x = 0;
                    if (cam.y < 0)
                        cam.y = 0;

                    if (cam.detector.minwidth == 0)
                    {
                        cam.detector.minwidth = 20;
                        cam.detector.minheight = 20;
                        cam.detector.highlight = true;
                        cam.settings.reconnectinterval = 0;
                    }
                    if (cam.settings.accessgroups == null)
                        cam.settings.accessgroups = "";
                    if (cam.settings.ptztimetohome == 0)
                        cam.settings.ptztimetohome = 100;
                    if (cam.settings.ptzautohomedelay == 0)
                        cam.settings.ptzautohomedelay = 30;
                    if (cam.settings.ptzurlbase == null)
                        cam.settings.ptzurlbase = "";
                    if (cam.settings.audioport <= 0)
                        cam.settings.audioport = 80;
                    if (cam.ftp.intervalnew < 0)
                        cam.ftp.intervalnew = cam.ftp.interval;

                    if (cam.ftp.server.Length>10)
                    {
                        var ftp = Conf.FTPServers.FirstOrDefault(p => p.name == cam.ftp.server && p.username==cam.ftp.username);
                        if (ftp == null)
                        {
                            ftp = new configurationServer
                                  {
                                      ident = Guid.NewGuid().ToString(),
                                      name=cam.ftp.server,
                                      password = cam.ftp.password,
                                      port = cam.ftp.port,
                                      rename = cam.ftp.rename,
                                      server = cam.ftp.server,
                                      usepassive = cam.ftp.usepassive,
                                      username = cam.ftp.username
                                  };
                            var l = Conf.FTPServers.ToList();
                            l.Add(ftp);
                            Conf.FTPServers = l.ToArray();
                            cam.ftp.ident = ftp.ident;

                        }
                    }

                    if (Conf.MediaDirectories.FirstOrDefault(p => p.ID == cam.settings.directoryIndex) == null)
                        cam.settings.directoryIndex = Conf.MediaDirectories.First().ID;

                    if (String.IsNullOrEmpty(cam.settings.emailondisconnect))
                    {
                        if (cam.settings.notifyondisconnect)
                        {
                            cam.settings.emailondisconnect = cam.settings.emailaddress;
                        }
                    }

                    if (cam.recorder.quality == 0)
                        cam.recorder.quality = 8;
                    if (cam.recorder.timelapseframerate == 0)
                        cam.recorder.timelapseframerate = 5;

                    if (cam.detector.movementintervalnew < 0)
                        cam.detector.movementintervalnew = cam.detector.movementinterval;

                    if (cam.detector.nomovementintervalnew < 0)
                        cam.detector.nomovementintervalnew = cam.detector.nomovementinterval;

                    if (cam.directory == null)
                        throw new Exception("err_old_config");

                    if (String.IsNullOrEmpty(cam.settings.ptzpelcoconfig))
                        cam.settings.ptzpelcoconfig = "COM1|9600|8|One|Odd|1";

                    if (cam.savelocal == null)
                    {
                        cam.savelocal = new objectsCameraSavelocal
                                        {
                                            counter = cam.ftp.counter,
                                            countermax = cam.ftp.countermax,
                                            mode = cam.ftp.mode,
                                            enabled = cam.ftp.savelocal,
                                            filename = cam.ftp.localfilename,
                                            intervalnew = cam.ftp.intervalnew,
                                            minimumdelay = cam.ftp.minimumdelay,
                                            quality = cam.ftp.quality,
                                            text = cam.ftp.text

                                        };
                    }

                    if (cam.alerts.processmode == null)
                        cam.alerts.processmode = "continuous";
                    if (cam.alerts.pluginconfig == null)
                        cam.alerts.pluginconfig = "";
                    if (cam.ftp.quality == 0)
                        cam.ftp.quality = 75;

                    if (cam.ftp.countermax == 0)
                        cam.ftp.countermax = 20;

                    if (cam.settings.audiousername == null)
                    {
                        cam.settings.audiousername = "";
                        cam.settings.audiopassword = "";
                    }

                    if (String.IsNullOrEmpty(cam.settings.timestampforecolor) || cam.settings.timestampforecolor == "0")
                    {
                        cam.settings.timestampforecolor = "255,255,255";
                    }

                    if (String.IsNullOrEmpty(cam.settings.timestampbackcolor) || cam.settings.timestampbackcolor == "0")
                    {
                        cam.settings.timestampbackcolor = "70,70,70";
                    }

                    if (Math.Abs(cam.detector.minsensitivity - 0) < double.Epsilon)
                    {
                        cam.detector.maxsensitivity = 100;
                        //fix for old setting conversion
                        cam.detector.minsensitivity = 100 - cam.detector.sensitivity;

                        if (Math.Abs(cam.detector.minsensitivity - 100) < double.Epsilon)
                        {
                            cam.detector.minsensitivity = 20;
                        }
                    }

                    if (!Directory.Exists(path2))
                    {
                        try
                        {
                            Directory.CreateDirectory(path2);
                        }
                        catch (IOException e)
                        {
                            LogExceptionToFile(e);
                        }
                    }

                    if (String.IsNullOrEmpty(cam.ftp.localfilename))
                    {
                        cam.ftp.localfilename = "{0:yyyy-MM-dd_HH-mm-ss_fff}.jpg";
                    }

                    if (String.IsNullOrEmpty(cam.settings.audiomodel))
                        cam.settings.audiomodel = "None";

                    if (String.IsNullOrEmpty(cam.settings.timestampfont))
                    {
                        cam.settings.timestampfont = FontXmlConverter.ConvertToString(Drawfont);
                        cam.settings.timestampshowback = true;
                    }


                    path2 = Helper.GetMediaDirectory(2, cam.id) + "video\\" + cam.directory + "\\thumbs\\";
                    if (!Directory.Exists(path2))
                    {
                        try
                        {
                            Directory.CreateDirectory(path2);
                        }
                        catch
                        {
                        }
                    }

                    path2 = Helper.GetMediaDirectory(2, cam.id) + "video\\" + cam.directory + "\\grabs\\";
                    if (!Directory.Exists(path2))
                    {
                        try
                        {
                            Directory.CreateDirectory(path2);
                        }
                        catch
                        {
                        }
                    }
                    if (cam.alerts.trigger == null)
                        cam.alerts.trigger = "";

                    if (String.IsNullOrEmpty(cam.rotateMode))
                    {
                        cam.rotateMode = "RotateNoneFlipNone";
                        if (cam.rotate90)
                        {
                            cam.rotateMode = RotateFlipType.Rotate90FlipNone.ToString();
                        }
                        if (cam.flipx)
                        {
                            cam.rotateMode = RotateFlipType.RotateNoneFlipX.ToString();
                        }
                        if (cam.flipy)
                        {
                            cam.rotateMode = RotateFlipType.RotateNoneFlipY.ToString();
                        }
                        if (cam.flipx && cam.flipy)
                        {
                            cam.rotateMode = RotateFlipType.RotateNoneFlipXY.ToString();
                        }
                    }
                }
                int micid = 0;
                foreach (objectsMicrophone mic in _microphones)
                {
                    if (mic.id >= micid)
                        micid = mic.id + 1;
                    if (mic.directory == null)
                        throw new Exception("err_old_config");
                    mic.newrecordingcount = 0;
                    path2 = Helper.GetMediaDirectory(1, mic.id) + "audio\\" + mic.directory + "\\";
                    if (!Directory.Exists(path2))
                        Directory.CreateDirectory(path2);

                    if (mic.settings.accessgroups == null)
                        mic.settings.accessgroups = "";

                    if (mic.settings.storagemanagement == null)
                    {
                        mic.settings.storagemanagement = new objectsMicrophoneSettingsStoragemanagement
                        {
                            enabled = false,
                            maxage = 72,
                            maxsize = 1024

                        };
                    }
                    if (Math.Abs(mic.detector.minsensitivity - (-1)) < double.Epsilon)
                    {
                        mic.detector.minsensitivity = mic.detector.sensitivity;
                        mic.detector.maxsensitivity = 100;
                    }

                    if (Conf.MediaDirectories.FirstOrDefault(p => p.ID == mic.settings.directoryIndex) == null)
                        mic.settings.directoryIndex = Conf.MediaDirectories.First().ID;

                    bool migrate = false;
                    if (mic.alertevents == null)
                    {
                        mic.alertevents = new objectsMicrophoneAlertevents();
                        migrate = true;
                    }
                    if (mic.alertevents.entries == null)
                    {
                        mic.alertevents.entries = new objectsMicrophoneAlerteventsEntry[] { };
                    }

                    if (migrate)
                    {
                        var l = new List<objectsMicrophoneAlerteventsEntry>();
                        if (!String.IsNullOrEmpty(mic.alerts.executefile))
                        {
                            l.Add(new objectsMicrophoneAlerteventsEntry { type = "Exe", param1 = mic.alerts.executefile, param2 = mic.alerts.arguments });
                        }
                        if (mic.notifications.sendemail)
                        {
                            l.Add(new objectsMicrophoneAlerteventsEntry { type = "E", param1 = mic.settings.emailaddress, param2 = "True" });
                        }
                        if (mic.notifications.sendsms)
                        {
                            l.Add(new objectsMicrophoneAlerteventsEntry { type = "SMS", param1 = mic.settings.smsnumber });
                        }

                        string[] alertOptions = mic.alerts.alertoptions.Split(','); //beep,restore

                        if (Convert.ToBoolean(alertOptions[0]))
                            l.Add(new objectsMicrophoneAlerteventsEntry { type = "B" });
                        if (Convert.ToBoolean(alertOptions[1]))
                            l.Add(new objectsMicrophoneAlerteventsEntry { type = "SW" });

                        if (mic.notifications.sendtwitter)
                        {
                            l.Add(new objectsMicrophoneAlerteventsEntry { type = "TM" });
                        }

                        if (!String.IsNullOrEmpty(mic.alerts.trigger))
                        {
                            l.Add(new objectsMicrophoneAlerteventsEntry { type = "TA", param1 = mic.alerts.trigger });
                        }
                    }

                    if (String.IsNullOrEmpty(mic.settings.emailondisconnect))
                    {
                        if (mic.settings.notifyondisconnect)
                        {
                            mic.settings.emailondisconnect = mic.settings.emailaddress;
                        }
                    }

                    if (addActions)
                    {
                        _actions.AddRange(mic.alertevents.entries.Select(a => new objectsActionsEntry
                        {
                            mode = "alert",
                            objectid = mic.id,
                            objecttypeid = 1,
                            type = a.type,
                            param1 = a.param1,
                            param2 = a.param2,
                            param3 = a.param3,
                            param4 = a.param4
                        }));
                        if (!String.IsNullOrEmpty(mic.settings.emailondisconnect))
                        {
                            _actions.Add(new objectsActionsEntry
                                  {
                                      mode = "disconnect",
                                      objectid = mic.id,
                                      objecttypeid = 1,
                                      type = "E",
                                      param1 = mic.settings.emailondisconnect,
                                      param2 = "False"
                                  });
                        }
                    }

                    if (mic.x < 0)
                        mic.x = 0;
                    if (mic.y < 0)
                        mic.y = 0;

                    if (mic.settings.gain <= 0)
                        mic.settings.gain = 1;

                    if (mic.alerts.trigger == null)
                        mic.alerts.trigger = "";
                }
                int fpid = 0;
                foreach (objectsFloorplan ofp in _floorplans)
                {
                    if (ofp.id >= fpid)
                        fpid = ofp.id + 1;

                    if (ofp.x < 0)
                        ofp.x = 0;
                    if (ofp.y < 0)
                        ofp.y = 0;
                    if (ofp.accessgroups == null)
                        ofp.accessgroups = "";
                }
                int rcid = 0;
                foreach (objectsCommand ocmd in _remotecommands)
                {
                    if (ocmd.id >= rcid)
                        rcid = ocmd.id + 1;
                }



                if (bAlertVlc)
                {
                    if (Program.Platform == "x64")
                        MessageBox.Show(LocRm.GetString("InstallVLCx64").Replace("[DIR]", Environment.NewLine + Program.AppPath + "VLC64" + Environment.NewLine));
                    else
                        MessageBox.Show(LocRm.GetString("InstallVLCx86"));
                    OpenUrl(Program.Platform == "x64" ? VLCx64 : VLCx86);
                }

                NeedsSync = true;
                LogMessageToFile("Loaded " + _cameras.Count + " cameras, " + _microphones.Count + " mics and " + _floorplans.Count + " floorplans");
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
                MessageBox.Show(LocRm.GetString("ConfigurationChanged"), LocRm.GetString("Error"));
                _cameras = new List<objectsCamera>();
                _microphones = new List<objectsMicrophone>();
                _remotecommands = new List<objectsCommand>();
                _actions = new List<objectsActionsEntry>();
                _floorplans = new List<objectsFloorplan>();

                InitRemoteCommands();
                
            }

            Filter.CheckedCameraIDs = new List<int>();
            Filter.CheckedMicIDs = new List<int>();
            Filter.Filtered = false;
        }

        internal static int NextCameraId
        {
            get
            {
                if (Cameras != null && Cameras.Count > 0)
                    return Cameras.Max(p => p.id) + 1;
                return 1;
            }
        }

        internal static int NextMicrophoneId
        {
            get
            {
                if (Microphones != null && Microphones.Count > 0)
                    return Microphones.Max(p => p.id) + 1;
                return 1;
            }
        }

        internal static int NextFloorPlanId
        {
            get
            {
                if (FloorPlans != null && FloorPlans.Count > 0)
                    return FloorPlans.Max(p => p.id) + 1;
                return 1;
            }
        }

        internal static int NextCommandId
        {
            get
            {
                if (RemoteCommands != null && RemoteCommands.Count > 0)
                    return RemoteCommands.Max(p => p.id) + 1;
                return 1;
            }
        }

        private static void LoadPTZs(string path)
        {
            try
            {
                var s = new XmlSerializer(typeof(PTZSettings2));
                PTZSettings2 c;
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    fs.Position = 0;
                    using (TextReader reader = new StreamReader(fs))
                    {
                        c = (PTZSettings2)s.Deserialize(reader);
                        reader.Close();
                    }
                    fs.Close();
                }

                _ptzs = c.Camera != null ? c.Camera.ToList() : new List<PTZSettings2Camera>();
            }
            catch (Exception)
            {
                MessageBox.Show(LocRm.GetString("PTZError"), LocRm.GetString("Error"));
            }
        }

        private static void LoadSources(string path)
        {
            try
            {
                var s = new XmlSerializer(typeof(Manufacturers));
                Manufacturers c;
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    fs.Position = 0;
                    using (TextReader reader = new StreamReader(fs))
                    {
                        c = (Manufacturers)s.Deserialize(reader);
                        reader.Close();
                    }
                    fs.Close();
                }
                _sources = c.Manufacturer != null ? c.Manufacturer.Distinct().ToList() : new List<ManufacturersManufacturer>();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, LocRm.GetString("Error"));
            }
        }

        private void LoadCommands()
        {
            lock (ThreadLock)
            {
                for (int i = 0; i < flCommands.Controls.Count; i++)
                {
                    flCommands.Controls.RemoveAt(i);
                    i--;
                }
                foreach (objectsCommand oc in RemoteCommands)
                {
                    var b = new Button
                    {
                        Tag = oc.id,
                        AutoSize = true,
                        UseVisualStyleBackColor = true,
                        Text = oc.name.StartsWith("cmd_") ? LocRm.GetString(oc.name) : oc.name,
                        Width = 110

                    };
                    b.Click += BClick;
                    flCommands.Controls.Add(b);
                }
            }
        }

        private void RemoveObjects()
        {
            
                bool removed = true;
                while (removed)
                {
                    removed = false;
                    foreach (Control c in _pnlCameras.Controls)
                    {
                        if (c is CameraWindow)
                        {
                            var cameraControl = (CameraWindow)c;
                            RemoveCamera(cameraControl, false);
                            Application.DoEvents();
                            removed = true;
                            break;
                        }
                        if (c is VolumeLevel)
                        {
                            var volumeControl = (VolumeLevel)c;
                            RemoveMicrophone(volumeControl, false);
                            Application.DoEvents();
                            removed = true;
                            break;
                        }
                        if (c is FloorPlanControl)
                        {
                            var floorPlanControl = (FloorPlanControl)c;
                            RemoveFloorplan(floorPlanControl, false);
                            Application.DoEvents();
                            removed = true;
                            break;
                        }
                    }
                }

                lock (ThreadLock)   {
                    Masterfilelist.Clear();
                    foreach (Control c in flowPreview.Controls)
                    {
                        var pb = c as PreviewBox;
                        if (pb != null)
                        {
                            pb.MouseDown -= PbMouseDown;
                            pb.MouseEnter -= PbMouseEnter;
                            pb.Dispose();
                        }
                    }
                    flowPreview.Controls.Clear();
                }
        }

        private void RenderObjects()
        {

            for (int index = 0; index < Cameras.Count; index++)
            {
                objectsCamera oc = Cameras[index];
                DisplayCamera(oc);
            }

            for (int index = 0; index < Microphones.Count; index++)
            {
                objectsMicrophone om = Microphones[index];
                DisplayMicrophone(om);
            }

            for (int index = 0; index < FloorPlans.Count; index++)
            {
                objectsFloorplan ofp = FloorPlans[index];
                DisplayFloorPlan(ofp);
            }

            for (int index = 0; index < Cameras.Count; index++)
            {
                objectsCamera oc = Cameras[index];
                var cw = GetCameraWindow(oc.id);
                if (Conf.AutoSchedule && oc.schedule.active && oc.schedule.entries.Any())
                {
                    oc.settings.active = false;
                    cw.ApplySchedule();
                }
                else
                {
                    try
                    {
                        if (oc.settings.active)
                            cw.Enable();
                    }
                    catch (Exception ex)
                    {
                        LogExceptionToFile(ex);
                    }
                }
            }

            for (int index = 0; index < Microphones.Count; index++)
            {
                objectsMicrophone om = Microphones[index];
                var vl = GetVolumeLevel(om.id);
                if (Conf.AutoSchedule && om.schedule.active && om.schedule.entries.Any())
                {
                    om.settings.active = false;
                    vl.ApplySchedule();
                }
                else
                {
                    if (om.settings.active)
                        vl.Enable();
                }
            }


            bool cam = false;
            if (_pnlCameras.Controls.Count > 0)
            {
                //prevents layering issues
                for (int index = 0; index < _pnlCameras.Controls.Count; index++)
                {
                    var c = _pnlCameras.Controls[index];
                    var cw = c as CameraWindow;
                    if (cw != null && cw.VolumeControl == null)
                    {
                        cam = true;
                        cw.BringToFront();
                    }
                }
                _pnlCameras.Controls[0].Focus();
            }
            if (!cam)
                flowPreview.Loading = false;

            NeedsSync = true;
        }

        private DateTime _oldestFile = DateTime.MinValue;
        private void DeleteOldFiles()
        {
            bool fileschanged = false;
            //walk through camera specific management first

            foreach (var camobj in Cameras)
            {
                if (camobj.settings.storagemanagement.enabled)
                {
                    try
                    {
                        var cw = GetCameraWindow(camobj.id);
                        if (cw != null)
                        {
                            bool archive = camobj.settings.storagemanagement.archive;
                            var dirinfo = new DirectoryInfo(Helper.GetMediaDirectory(2, camobj.id) + "video\\" + camobj.directory);

                            var lFi = new List<FileInfo>();
                            lFi.AddRange(dirinfo.GetFiles("*.*", SearchOption.AllDirectories));

                            lFi = lFi.FindAll(f => f.Extension != ".xml");
                            lFi = lFi.OrderBy(f => f.CreationTime).ToList();

                            var size = lFi.Sum(p => p.Length);
                            var targetSize = (camobj.settings.storagemanagement.maxsize) * 1048576d;
                            while (size > targetSize)
                            {
                                for (int i = 0; i < lFi.Count; i++)
                                {
                                    var fi = lFi[i];
                                    if (FileOperations.DeleteOrArchive(fi.FullName, archive))
                                    {
                                        try
                                        {
                                            cw.RemoveFile(fi.Name);
                                        }
                                        catch
                                        {
                                        }
                                        size -= fi.Length;

                                        fileschanged = true;
                                        lFi.Remove(fi);
                                        i--;
                                        if (size < targetSize)
                                            break;
                                    }
                                }
                            }
                            var targetdate = DateTime.Now.AddHours(0 - camobj.settings.storagemanagement.maxage);
                            lFi = lFi.FindAll(p => p.CreationTime < targetdate).ToList();
                            for (int i = 0; i < lFi.Count; i++)
                            {
                                var fi = lFi[i];
                                if (FileOperations.DeleteOrArchive(fi.FullName, archive))
                                {
                                    try
                                    {
                                        cw.RemoveFile(fi.Name);
                                    }
                                    catch
                                    {
                                    }
                                    fileschanged = true;
                                    lFi.Remove(fi);
                                    i--;
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        LogExceptionToFile(ex);
                    }
                }
            }

            foreach (var micobj in Microphones)
            {
                if (micobj.settings.storagemanagement.enabled)
                {
                    try
                    {
                        var vl = GetVolumeLevel(micobj.id);
                        if (vl != null)
                        {
                            bool archive = micobj.settings.storagemanagement.archive;
                            var dirinfo = new DirectoryInfo(Helper.GetMediaDirectory(1, micobj.id) + "audio\\" + micobj.directory);

                            var lFi = new List<FileInfo>();
                            lFi.AddRange(dirinfo.GetFiles("*.*", SearchOption.AllDirectories));

                            lFi = lFi.FindAll(f => f.Extension != ".xml");
                            lFi = lFi.OrderBy(f => f.CreationTime).ToList();

                            var size = lFi.Sum(p => p.Length);
                            var targetSize = (micobj.settings.storagemanagement.maxsize) * 1048576d;
                            while (size > targetSize)
                            {
                                for (int i = 0; i < lFi.Count; i++)
                                {
                                    var fi = lFi[i];
                                    if (FileOperations.DeleteOrArchive(fi.FullName, archive))
                                    {
                                        try
                                        {
                                            vl.RemoveFile(fi.Name);
                                        }
                                        catch
                                        {
                                        }
                                        size -= fi.Length;
                                        fileschanged = true;
                                        lFi.Remove(fi);
                                        i--;
                                    }
                                }
                            }
                            var targetdate = DateTime.Now.AddHours(0 - micobj.settings.storagemanagement.maxage);
                            lFi = lFi.FindAll(p => p.CreationTime < targetdate).ToList();
                            for (int i = 0; i < lFi.Count; i++)
                            {
                                var fi = lFi[i];
                                if (FileOperations.DeleteOrArchive(fi.FullName, archive))
                                {
                                    try
                                    {
                                        vl.RemoveFile(fi.Name);
                                    }
                                    catch
                                    {
                                    }
                                    fileschanged = true;
                                    lFi.Remove(fi);
                                    i--;
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        LogExceptionToFile(ex);
                    }
                }
            }




            if (fileschanged)
            {
                UISync.Execute(RefreshControls);
            }

            //run storage management on each directory
            foreach (var d in Conf.MediaDirectories)
            {
                if (d.Enable_Storage_Management)
                {
                    if (d.DeleteFilesOlderThanDays <= 0)
                        return;

                    DateTime dtref = DateTime.Now.AddDays(0 - d.DeleteFilesOlderThanDays);

                    //don't bother if oldest file isn't past cut-off
                    if (_oldestFile > dtref)
                        return;

                    var lFi = new List<FileInfo>();
                    try
                    {
                        var dirinfo = new DirectoryInfo(d.Entry + "video\\");
                        lFi.AddRange(dirinfo.GetFiles("*.*", SearchOption.AllDirectories));

                        dirinfo = new DirectoryInfo(d.Entry + "audio\\");
                        lFi.AddRange(dirinfo.GetFiles("*.*", SearchOption.AllDirectories));

                    }
                    catch (Exception ex)
                    {
                        LogExceptionToFile(ex);
                        return;
                    }

                    lFi = lFi.FindAll(f => f.Extension != ".xml");
                    lFi = lFi.OrderBy(f => f.CreationTime).ToList();

                    var size = lFi.Sum(p => p.Length);
                    var targetSize = (d.MaxMediaFolderSizeMB * 0.7) * 1048576d;

                    if (size < targetSize)
                    {
                        return;
                    }



                    var lCan = lFi.Where(p => p.CreationTime < dtref).OrderBy(p => p.CreationTime).ToList();
                    bool archive = d.archive;

                    for (int i = 0; i < lCan.Count; i++)
                    {
                        var fi = lCan[i];
                        if (size > targetSize)
                        {
                            if (FileOperations.DeleteOrArchive(fi.FullName, archive))
                            {
                                size -= fi.Length;
                                fileschanged = true;
                                lCan.Remove(fi);
                                i--;
                                lock (ThreadLock)
                                {
                                    Masterfilelist.RemoveAll(p => p.Filename.EndsWith(fi.Name));
                                }
                                if (size < targetSize)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    if (lCan.Count > 0)
                        _oldestFile = lCan.First().CreationTime;
                    else
                    {
                        var o = lFi.FirstOrDefault(p => p.CreationTime > dtref);
                        if (o != null)
                            _oldestFile = o.CreationTime;

                    }

                    if (fileschanged)
                    {
                        UISync.Execute(RefreshControls);
                        LogMessageToFile(LocRm.GetString("MediaStorageLimit").Replace("[AMOUNT]",
                                                                                      d.MaxMediaFolderSizeMB.ToString
                                                                                          (
                                                                                              CultureInfo.
                                                                                                  InvariantCulture)));
                    }

                    if ((size / 1048576) > d.MaxMediaFolderSizeMB && !d.StopSavingFlag && d.StopSavingOnStorageLimit)
                    {
                        d.StopSavingFlag = true;
                    }
                    else
                        d.StopSavingFlag = false;
                }
            }

        }

        private void SetMicrophoneEvents(VolumeLevel vw)
        {
            vw.DoubleClick += VolumeControlDoubleClick;
            vw.MouseDown += VolumeControlMouseDown;
            vw.MouseUp += VolumeControlMouseUp;
            vw.MouseMove += VolumeControlMouseMove;
            vw.RemoteCommand += VolumeControlRemoteCommand;
            vw.Notification += ControlNotification;
            vw.FileListUpdated += VolumeControlFileListUpdated;
        }

        void VolumeControlFileListUpdated(object sender)
        {
            lock (ThreadLock)
            {
                var vl = sender as VolumeLevel;
                if (vl != null)
                {
                    Masterfilelist.RemoveAll(p => p.ObjectId == vl.Micobject.id && p.ObjectTypeId == 1);
                    var l = vl.FileList.ToList();
                    foreach (var ff in l)
                    {
                        Masterfilelist.Add(new FilePreview(ff.Filename, ff.DurationSeconds, vl.Micobject.name,
                                                           ff.CreatedDateTicks, 1, vl.Micobject.id, ff.MaxAlarm,ff.IsTimelapse,ff.IsMergeFile));
                    }
                    if (!vl.LoadedFiles)
                    {
                        vl.LoadedFiles = true;
                        //last one?
                        bool all = true;
                        foreach (Control c in _pnlCameras.Controls)
                        {
                            var cameraWindow = c as CameraWindow;
                            if (cameraWindow != null)
                            {
                                if (!cameraWindow.LoadedFiles)
                                {
                                    all = false;
                                    break;
                                }
                            }
                            var volumeLevel = c as VolumeLevel;
                            if (volumeLevel != null)
                            {
                                if (!volumeLevel.LoadedFiles)
                                {
                                    all = false;
                                    break;
                                }
                            }
                        }
                        if (all)
                        {
                            flowPreview.Loading = false;
                            LoadPreviews();
                        }
                    }
                }
            }

        }

        private void SetFloorPlanEvents(FloorPlanControl fpc)
        {
            fpc.DoubleClick += FloorPlanDoubleClick;
            fpc.MouseDown += FloorPlanMouseDown;
            fpc.MouseUp += FloorPlanMouseUp;
            fpc.MouseMove += FloorPlanMouseMove;
        }

        private void DisplayMicrophone(objectsMicrophone mic)
        {
            var micControl = new VolumeLevel(mic,this);
            SetMicrophoneEvents(micControl);
            micControl.BackColor = Conf.BackColor.ToColor();
            _pnlCameras.Controls.Add(micControl);
            micControl.Location = new Point(mic.x, mic.y);
            micControl.Size = new Size(mic.width, mic.height);
            micControl.BringToFront();
            micControl.Tag = GetControlIndex();

            try
            {
                string path = Helper.GetMediaDirectory(1, mic.id) + "audio\\" + mic.directory + "\\";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                MainForm.LogExceptionToFile(ex);
            }
            micControl.GetFiles();
        }

        internal void DisplayFloorPlan(objectsFloorplan ofp)
        {
            var fpControl = new FloorPlanControl(ofp, this);
            SetFloorPlanEvents(fpControl);
            fpControl.BackColor = Conf.BackColor.ToColor();
            _pnlCameras.Controls.Add(fpControl);
            fpControl.Location = new Point(ofp.x, ofp.y);
            fpControl.Size = new Size(ofp.width, ofp.height);
            fpControl.BringToFront();
            fpControl.Tag = GetControlIndex();
        }

        internal void EditCamera(objectsCamera cr, IWin32Window owner = null)
        {
            int cameraId = Convert.ToInt32(cr.id);
            CameraWindow cw = null;

            for (int index = 0; index < _pnlCameras.Controls.Count; index++)
            {
                Control c = _pnlCameras.Controls[index];
                if (c.GetType() != typeof(CameraWindow)) continue;
                var cameraControl = (CameraWindow)c;
                if (cameraControl.Camobject.id == cameraId)
                {
                    cw = cameraControl;
                    break;
                }
            }

            if (cw == null) return;
            TopMost = false;
            var ac = new AddCamera { CameraControl = cw, MainClass = this };
            ac.ShowDialog(owner ?? this);
            ac.Dispose();
            TopMost = Conf.AlwaysOnTop;
        }

        internal void EditObject(ISpyControl ctrl, IWin32Window owner = null)
        {
            var cw = ctrl as CameraWindow;
            if (cw != null)
            {
                EditCamera(cw.Camobject,owner);
                return;
            }
            var vl = ctrl as VolumeLevel;
            if (vl != null)
            {
                EditMicrophone(vl.Micobject, owner);
                return;
            }
            var fp = ctrl as FloorPlanControl;
            if (fp != null)
            {
                EditFloorplan(fp.Fpobject, owner);
                return;
            }
        }

        internal void EditMicrophone(objectsMicrophone om, IWin32Window owner = null)
        {
            VolumeLevel vlf = null;

            for (int index = 0; index < _pnlCameras.Controls.Count; index++)
            {
                Control c = _pnlCameras.Controls[index];
                if (c.GetType() != typeof(VolumeLevel)) continue;
                var vl = (VolumeLevel)c;
                if (vl.Micobject.id == om.id)
                {
                    vlf = vl;
                    break;
                }
            }

            if (vlf != null)
            {
                TopMost = false;
                var am = new AddMicrophone { VolumeLevel = vlf, MainClass = this };
                am.ShowDialog(owner ?? this);
                am.Dispose();
                TopMost = Conf.AlwaysOnTop;
            }
        }

        internal void EditFloorplan(objectsFloorplan ofp, IWin32Window owner = null)
        {
            FloorPlanControl fpc = null;

            for (int index = 0; index < _pnlCameras.Controls.Count; index++)
            {
                Control c = _pnlCameras.Controls[index];
                if (c.GetType() != typeof(FloorPlanControl)) continue;
                var fp = (FloorPlanControl)c;
                if (fp.Fpobject.id != ofp.id) continue;
                fpc = fp;
                break;
            }

            if (fpc != null)
            {
                var afp = new AddFloorPlan { Fpc = fpc, MainClass = this };
                afp.ShowDialog(owner ?? this);
                afp.Dispose();
                fpc.Invalidate();
            }
        }

        private CameraWindow GetCamera(int cameraId)
        {
            for (int index = 0; index < _pnlCameras.Controls.Count; index++)
            {
                Control c = _pnlCameras.Controls[index];
                if (c.GetType() != typeof(CameraWindow)) continue;
                var cw = (CameraWindow)c;
                if (cw.Camobject.id != cameraId) continue;
                return cw;
            }
            return null;
        }

        public FloorPlanControl GetFloorPlan(int floorPlanId)
        {
            for (int index = 0; index < _pnlCameras.Controls.Count; index++)
            {
                Control c = _pnlCameras.Controls[index];
                if (c.GetType() != typeof(FloorPlanControl)) continue;
                var fp = (FloorPlanControl)c;
                if (fp.Fpobject.id != floorPlanId) continue;
                return fp;
            }
            return null;
        }

        public void RemoveCamera(CameraWindow cameraControl, bool confirm)
        {
            if (confirm &&
                MessageBox.Show(LocRm.GetString("AreYouSure"), LocRm.GetString("Confirm"), MessageBoxButtons.OKCancel,
                                MessageBoxIcon.Warning) == DialogResult.Cancel)
                return;

            var dr = DialogResult.No;
            if (confirm)
            {
                dr = MessageBox.Show(LocRm.GetString("DeleteAllAssociatedMedia"), LocRm.GetString("Confirm"),
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);
            }
            if (dr == DialogResult.Cancel)
                return;

            string folder = cameraControl.Dir.Entry + "video\\" + cameraControl.Camobject.directory + "\\";

            cameraControl.ShuttingDown = true;
            cameraControl.MouseDown -= CameraControlMouseDown;
            cameraControl.MouseUp -= CameraControlMouseUp;
            cameraControl.MouseMove -= CameraControlMouseMove;
            cameraControl.DoubleClick -= CameraControlDoubleClick;
            cameraControl.RemoteCommand -= CameraControlRemoteCommand;
            cameraControl.Notification -= ControlNotification;
            if (cameraControl.Recording)
                cameraControl.RecordSwitch(false);

            cameraControl.Disable();
            cameraControl.SaveFileList();

            

            if (cameraControl.VolumeControl != null)
                RemoveMicrophone(cameraControl.VolumeControl, false);

            if (InvokeRequired)
                Invoke(new Delegates.CameraCommandDelegate(RemoveCameraPanel), cameraControl);
            else
                RemoveCameraPanel(cameraControl);

            

            if (dr == DialogResult.Yes)
            {
                try
                {
                    Directory.Delete(folder, true);
                }
                catch (Exception ex)
                {
                    LogExceptionToFile(ex);
                }
            }
        }

        private void RemoveCameraPanel(CameraWindow cameraControl)
        {
            _pnlCameras.Controls.Remove(cameraControl);
            if (!_closing)
            {
                CameraWindow control = cameraControl;
                var oc = Cameras.FirstOrDefault(p => p.id == control.Camobject.id);
                if (oc != null)
                {
                    lock (ThreadLock)
                    {
                        Masterfilelist.RemoveAll(p => p.ObjectId == oc.id && p.ObjectTypeId == 2);
                    }
                    Actions.RemoveAll(p => p.objectid == control.Camobject.id && p.objecttypeid == 2);
                    Cameras.Remove(oc);
                }

                foreach (var ofp in FloorPlans)
                    ofp.needsupdate = true;

                NeedsSync = true;
                SetNewStartPosition();
            }
            Application.DoEvents();
            cameraControl.Dispose();
            if (!_shuttingDown)
            {
                LoadPreviews();
            }
        }

        public void RemoveMicrophone(VolumeLevel volumeControl, bool confirm)
        {
            if (confirm &&
                MessageBox.Show(LocRm.GetString("AreYouSure"), LocRm.GetString("Confirm"), MessageBoxButtons.OKCancel,
                                MessageBoxIcon.Warning) == DialogResult.Cancel)
                return;

            var dr = DialogResult.No;
            if (confirm)
            {
                dr = MessageBox.Show(LocRm.GetString("DeleteAllAssociatedMedia"), LocRm.GetString("Confirm"),
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);
            }
            if (dr == DialogResult.Cancel)
                return;

            string folder = volumeControl.Dir.Entry + "audio\\" + volumeControl.Micobject.directory + "\\";

            volumeControl.ShuttingDown = true;
            volumeControl.MouseDown -= VolumeControlMouseDown;
            volumeControl.MouseUp -= VolumeControlMouseUp;
            volumeControl.MouseMove -= VolumeControlMouseMove;
            volumeControl.DoubleClick -= VolumeControlDoubleClick;
            volumeControl.RemoteCommand -= VolumeControlRemoteCommand;
            volumeControl.Notification -= ControlNotification;
            if (volumeControl.Recording)
                volumeControl.RecordSwitch(false);

            volumeControl.Disable();
            volumeControl.SaveFileList();

            if (InvokeRequired)
                Invoke(new Delegates.MicrophoneCommandDelegate(RemoveMicrophonePanel), volumeControl);
            else
                RemoveMicrophonePanel(volumeControl);

            if (dr == DialogResult.Yes)
            {
                try
                {
                    Directory.Delete(folder, true);
                }
                catch (Exception ex)
                {
                    LogExceptionToFile(ex);
                }
            }
        }

        private void RemoveMicrophonePanel(VolumeLevel volumeControl)
        {
            _pnlCameras.Controls.Remove(volumeControl);

            if (!_closing)
            {
                var control = volumeControl;
                var om = Microphones.SingleOrDefault(p => p.id == control.Micobject.id);
                if (om != null)
                {
                    lock (ThreadLock)
                    {
                        Masterfilelist.RemoveAll(p => p.ObjectId == om.id && p.ObjectTypeId == 1);
                    }
                    for (var index = 0; index < Cameras.Count(p => p.settings.micpair == om.id); index++)
                    {
                        var oc = Cameras.Where(p => p.settings.micpair == om.id).ToList()[index];
                        oc.settings.micpair = -1;
                    }
                    Actions.RemoveAll(p => p.objectid == control.Micobject.id && p.objecttypeid == 1);
                    Microphones.Remove(om);

                    foreach (var ofp in FloorPlans)
                        ofp.needsupdate = true;
                }
                SetNewStartPosition();
                NeedsSync = true;
            }
            Application.DoEvents();
            volumeControl.Dispose();
        }

        private void RemoveFloorplan(FloorPlanControl fpc, bool confirm)
        {
            if (confirm &&
                MessageBox.Show(LocRm.GetString("AreYouSure"), LocRm.GetString("Confirm"), MessageBoxButtons.OKCancel,
                                MessageBoxIcon.Warning) == DialogResult.Cancel)
                return;

            if (fpc.Fpobject != null && fpc.Fpobject.objects != null && fpc.Fpobject.objects.@object != null)
            {
                foreach (var o in fpc.Fpobject.objects.@object)
                {
                    switch (o.type)
                    {
                        case "camera":
                            CameraWindow cw = GetCameraWindow(o.id);
                            if (cw != null)
                            {
                                //cw.Location = new Point(Location.X + e.X, Location.Y + e.Y);
                                cw.Highlighted = false;
                                cw.Invalidate();
                            }
                            break;
                        case "microphone":
                            VolumeLevel vl = GetVolumeLevel(o.id);
                            if (vl != null)
                            {
                                vl.Highlighted = false;
                                vl.Invalidate();
                            }
                            break;
                    }
                }
            }
            _pnlCameras.Controls.Remove(fpc);


            if (!_closing)
            {
                objectsFloorplan ofp = FloorPlans.SingleOrDefault(p => p.id == fpc.Fpobject.id);
                if (ofp != null)
                    FloorPlans.Remove(ofp);
                SetNewStartPosition();
                NeedsSync = true;
            }
            fpc.Dispose();
        }

        public void SaveFileData()
        {
            try
            {
                foreach (objectsCamera oc in Cameras)
                {
                    CameraWindow occ = GetCameraWindow(oc.id);
                    if (occ != null)
                    {
                        occ.SaveFileList();
                    }
                }

                foreach (objectsMicrophone om in Microphones)
                {
                    VolumeLevel omc = GetVolumeLevel(om.id);
                    if (omc != null)
                    {
                        omc.SaveFileList();
                    }
                }
            }
            catch (Exception ex)
            { LogExceptionToFile(ex); }
        }

        private void RefreshControls()
        {
            LoadPreviews();
        }

        private void AddCamera(int videoSourceIndex, bool startWizard = false)
        {
            CameraWindow cw = NewCameraWindow(videoSourceIndex);
            TopMost = false;
            var ac = new AddCamera { CameraControl = cw, StartWizard = startWizard, IsNew = true, MainClass = this };
            ac.ShowDialog(this);
            if (ac.DialogResult == DialogResult.OK)
            {
                UnlockLayout();
                SetNewStartPosition();
                if (cw.VolumeControl != null && !cw.VolumeControl.IsEnabled)
                    cw.VolumeControl.Enable();
                NeedsSync = true;
            }
            else
            {
                int cid = cw.Camobject.id;
                cw.Disable();
                _pnlCameras.Controls.Remove(cw);
                cw.Dispose();
                Cameras.RemoveAll(p => p.id == cid);
            }
            ac.Dispose();
            TopMost = Conf.AlwaysOnTop;
        }

        private CameraWindow NewCameraWindow(int videoSourceIndex)
        {
            var oc = new objectsCamera
            {
                alerts = new objectsCameraAlerts(),
                detector = new objectsCameraDetector
                {
                    motionzones =
                        new objectsCameraDetectorZone
                        [0]
                },
                notifications = new objectsCameraNotifications(),
                recorder = new objectsCameraRecorder(),
                schedule = new objectsCameraSchedule { entries = new objectsCameraScheduleEntry[0] },
                settings = new objectsCameraSettings(),
                ftp = new objectsCameraFtp(),
                savelocal = new objectsCameraSavelocal(),
                id = -1,
                directory = RandomString(5),
                ptz = -1,
                x = Convert.ToInt32(Random.NextDouble() * 100),
                y = Convert.ToInt32(Random.NextDouble() * 100),
                ptzschedule = new objectsCameraPtzschedule
                {
                    active = false,
                    entries = new objectsCameraPtzscheduleEntry[] { }
                }
            };

            string friendlyName = LocRm.GetString("Camera") + " " + NextCameraId;

            string t = friendlyName;
            int i = 1;
            while (Cameras.FirstOrDefault(p => p.name == t) != null)
            {
                t = friendlyName + " (" + i + ")";
                i++;
            }

            oc.name = t;


            oc.flipx = oc.flipy = false;
            oc.width = 320;
            oc.height = 240;
            oc.description = "";
            oc.resolution = "320x240";
            oc.newrecordingcount = 0;

            oc.alerts.active = true;
            oc.alerts.mode = "movement";
            oc.alerts.alertoptions = "false,false";
            oc.alerts.objectcountalert = 1;
            oc.alerts.minimuminterval = 180;
            oc.alerts.processmode = "continuous";
            oc.alerts.pluginconfig = "";
            oc.alerts.trigger = "";

            oc.notifications.sendemail = false;
            oc.notifications.sendsms = false;
            oc.notifications.sendmms = false;
            oc.notifications.emailgrabinterval = 0;

            oc.ftp.enabled = false;
            oc.ftp.port = 21;
            oc.ftp.mode = 0;
            oc.ftp.server = "ftp://";
            oc.ftp.interval = 10;
            oc.ftp.intervalnew = 10;
            oc.ftp.filename = "mylivecamerafeed.jpg";
            oc.ftp.ready = true;
            oc.ftp.text = "www.ispyconnect.com";
            oc.ftp.quality = 75;
            oc.ftp.counter = 0;
            oc.ftp.countermax = 20;
            oc.ftp.minimumdelay = 0;

            oc.savelocal.enabled = false;
            oc.savelocal.mode = 0;
            oc.savelocal.intervalnew = 10;
            oc.savelocal.filename = "{0:yyyy-MM-dd_HH-mm-ss_fff}.jpg";
            oc.savelocal.text = "www.ispyconnect.com";
            oc.savelocal.quality = 75;
            oc.savelocal.counter = 0;
            oc.savelocal.countermax = 20;
            oc.savelocal.minimumdelay = 0;

            oc.schedule.active = false;

            oc.settings.active = false;
            oc.settings.deleteavi = true;
            oc.settings.ffmpeg = Conf.FFMPEG_Camera;
            oc.settings.emailaddress = EmailAddress;
            oc.settings.smsnumber = MobileNumber;
            oc.settings.suppressnoise = true;
            oc.settings.login = "";
            oc.settings.password = "";
            oc.settings.useragent = "Mozilla/5.0";
            oc.settings.frameinterval = 10;
            oc.settings.sourceindex = videoSourceIndex;
            oc.settings.micpair = -1;
            oc.settings.frameinterval = 200;
            oc.settings.maxframerate = 10;
            oc.settings.maxframeraterecord = 10;
            oc.settings.ptzautotrack = false;
            oc.settings.framerate = 10;
            oc.settings.timestamplocation = 1;
            oc.settings.ptztimetohome = 100;
            oc.settings.ptzchannel = "0";
            oc.settings.timestampformatter = "FPS: {FPS} {0:G} ";
            oc.settings.timestampfontsize = 10;
            oc.settings.notifyondisconnect = false;
            oc.settings.ptzautohomedelay = 30;
            oc.settings.accessgroups = "";
            oc.settings.nobuffer = true;
            oc.settings.reconnectinterval = 0;
            oc.settings.timestampforecolor = "255,255,255";
            oc.settings.timestampbackcolor = "70,70,70";
            oc.settings.timestampfont = FontXmlConverter.ConvertToString(Drawfont);
            oc.settings.timestampshowback = true;

            oc.settings.youtube = new objectsCameraSettingsYoutube
            {
                category = Conf.YouTubeDefaultCategory,
                tags = "iSpy, Motion Detection, Surveillance",
                @public = true
            };
            oc.settings.cloudprovider = new objectsCameraSettingsCloudprovider();
            
            oc.settings.storagemanagement = new objectsCameraSettingsStoragemanagement
            {
                enabled = false,
                maxage = 72,
                maxsize = 1024

            };

            oc.alertevents = new objectsCameraAlertevents { entries = new objectsCameraAlerteventsEntry[] { } };

            oc.settings.desktopresizeheight = 480;
            oc.settings.desktopresizewidth = 640;
            oc.settings.resize = false;
            oc.settings.directoryIndex = Conf.MediaDirectories.First().ID;

            if (VlcHelper.VlcInstalled)
                oc.settings.vlcargs = "-I" + NL + "dummy" + NL + "--ignore-config" + NL + "--rtsp-caching 100";
            else
                oc.settings.vlcargs = "";

            oc.detector.recordondetect = Conf.DefaultRecordOnDetect;
            oc.detector.recordonalert = Conf.DefaultRecordOnAlert;
            oc.detector.keepobjectedges = false;
            oc.detector.processeveryframe = 1;
            oc.detector.nomovementintervalnew = oc.detector.nomovementinterval = 30;
            oc.detector.movementintervalnew = oc.detector.movementinterval = 0;

            oc.detector.calibrationdelay = 15;
            oc.detector.color = ColorTranslator.ToHtml(Conf.TrackingColor.ToColor());
            oc.detector.type = "Two Frames";
            oc.detector.postprocessor = "None";
            oc.detector.minsensitivity = 20;
            oc.detector.maxsensitivity = 100;
            oc.detector.minwidth = 20;
            oc.detector.minheight = 20;
            oc.detector.highlight = true;

            oc.recorder.bufferseconds = 2;
            oc.recorder.inactiverecord = 8;
            oc.recorder.timelapse = 0;
            oc.recorder.timelapseframes = 0;
            oc.recorder.maxrecordtime = 900;
            oc.recorder.timelapsesave = 60;
            oc.recorder.quality = 8;
            oc.recorder.timelapseframerate = 5;
            oc.recorder.crf = true;

            oc.settings.audioport = 80;
            oc.settings.audiomodel = "None";
            oc.settings.audioip = "";
            oc.rotateMode = "RotateNoneFlipNone";

            var cameraControl = new CameraWindow(oc,this) { BackColor = Conf.BackColor.ToColor() };
            _pnlCameras.Controls.Add(cameraControl);

            cameraControl.Location = new Point(oc.x, oc.y);
            cameraControl.Size = new Size(320, 240);
            cameraControl.AutoSize = true;
            cameraControl.BringToFront();
            SetCameraEvents(cameraControl);
            if (Conf.AutoLayout)
                LayoutObjects(0, 0);

            cameraControl.Tag = GetControlIndex();

            return cameraControl;
        }

        public int AddMicrophone(int audioSourceIndex)
        {
            VolumeLevel vl = NewVolumeLevel(audioSourceIndex);
            TopMost = false;
            var am = new AddMicrophone { VolumeLevel = vl, IsNew = true, MainClass = this };
            am.ShowDialog(this);

            int micid = -1;

            if (am.DialogResult == DialogResult.OK)
            {
                UnlockLayout();
                micid = am.VolumeLevel.Micobject.id = NextMicrophoneId;
                Microphones.Add(vl.Micobject);
                SetNewStartPosition();
                NeedsSync = true;
            }
            else
            {
                vl.Disable();
                _pnlCameras.Controls.Remove(vl);
                vl.Dispose();
            }
            am.Dispose();
            TopMost = Conf.AlwaysOnTop;
            return micid;

        }

        private VolumeLevel NewVolumeLevel(int audioSourceIndex)
        {
            var om = new objectsMicrophone
            {
                alerts = new objectsMicrophoneAlerts(),
                detector = new objectsMicrophoneDetector(),
                notifications = new objectsMicrophoneNotifications(),
                recorder = new objectsMicrophoneRecorder(),
                schedule = new objectsMicrophoneSchedule
                {
                    entries
                        =
                        new objectsMicrophoneScheduleEntry
                        [
                        0
                        ]
                },
                settings = new objectsMicrophoneSettings(),
                id = -1,
                directory = RandomString(5),
                x = Convert.ToInt32(Random.NextDouble() * 100),
                y = Convert.ToInt32(Random.NextDouble() * 100),
                width = 160,
                height = 40,
                description = "",
                newrecordingcount = 0
            };

            string friendlyName = LocRm.GetString("Microphone") + " " + NextMicrophoneId;

            string t = friendlyName;
            int i = 1;
            while (Microphones.FirstOrDefault(p => p.name == t) != null)
            {
                t = friendlyName + " (" + i + ")";
                i++;
            }

            om.name = t;

            om.settings.typeindex = audioSourceIndex;
            om.settings.deletewav = true;
            om.settings.ffmpeg = Conf.FFMPEG_Microphone;
            om.settings.buffer = 4;
            om.settings.samples = 8000;
            om.settings.bits = 16;
            om.settings.gain = 100;
            om.settings.channels = 1;
            om.settings.decompress = true;
            om.settings.smsnumber = MobileNumber;
            om.settings.emailaddress = EmailAddress;
            om.settings.active = false;
            om.settings.notifyondisconnect = false;
            om.settings.directoryIndex = Conf.MediaDirectories.First().ID;
            if (VlcHelper.VlcInstalled)
                om.settings.vlcargs = "-I" + NL + "dummy" + NL + "--ignore-config";
            else
                om.settings.vlcargs = "";

            om.settings.storagemanagement = new objectsMicrophoneSettingsStoragemanagement
            {
                enabled = false,
                maxage = 72,
                maxsize = 1024

            };

            om.detector.sensitivity = 60;
            om.detector.minsensitivity = 60;
            om.detector.maxsensitivity = 100;

            om.detector.nosoundinterval = 30;
            om.detector.soundinterval = 0;
            om.detector.recordondetect = Conf.DefaultRecordOnDetect;
            om.detector.recordonalert = Conf.DefaultRecordOnAlert;

            om.alerts.mode = "sound";
            om.alerts.minimuminterval = 180;
            om.alerts.executefile = "";
            om.alerts.active = true;
            om.alerts.alertoptions = "false,false";
            om.alerts.trigger = "";

            om.recorder.inactiverecord = 5;
            om.recorder.maxrecordtime = 900;

            om.notifications.sendemail = false;
            om.notifications.sendsms = false;

            om.schedule.active = false;
            om.alertevents = new objectsMicrophoneAlertevents { entries = new objectsMicrophoneAlerteventsEntry[] { } };

            var volumeControl = new VolumeLevel(om,this) { BackColor = Conf.BackColor.ToColor() };
            _pnlCameras.Controls.Add(volumeControl);

            volumeControl.Location = new Point(om.x, om.y);
            volumeControl.Size = new Size(160, 40);
            volumeControl.BringToFront();
            SetMicrophoneEvents(volumeControl);

            if (Conf.AutoLayout)
                LayoutObjects(0, 0);

            volumeControl.Tag = GetControlIndex();

            return volumeControl;
        }

        private int GetControlIndex()
        {
            int i = 0;
            while (true)
            {

                bool b = false;
                foreach (Control c in _pnlCameras.Controls)
                {
                    if (c.Tag is int)
                    {
                        if (((int)c.Tag) == i)
                        {
                            b = true;
                            break;
                        }
                    }
                }
                if (!b)
                {
                    return i;
                }
                i++;
            }

        }

        private void AddFloorPlan()
        {
            var ofp = new objectsFloorplan
            {
                objects = new objectsFloorplanObjects { @object = new objectsFloorplanObjectsEntry[0] },
                id = -1,
                image = "",
                height = 480,
                width = 640,
                x = Convert.ToInt32(Random.NextDouble() * 100),
                y = Convert.ToInt32(Random.NextDouble() * 100),
                name = LocRm.GetString("FloorPlan") + " " + NextFloorPlanId
            };

            var fpc = new FloorPlanControl(ofp, this) { BackColor = Conf.BackColor.ToColor() };
            _pnlCameras.Controls.Add(fpc);

            fpc.Location = new Point(ofp.x, ofp.y);
            fpc.Size = new Size(320, 240);
            fpc.BringToFront();
            fpc.Tag = GetControlIndex();

            var afp = new AddFloorPlan { Fpc = fpc, Owner = this, MainClass = this };
            afp.ShowDialog(this);
            if (afp.DialogResult == DialogResult.OK)
            {
                UnlockLayout();
                afp.Fpc.Fpobject.id = NextFloorPlanId;
                FloorPlans.Add(ofp);
                SetFloorPlanEvents(fpc);
                SetNewStartPosition();
                fpc.Invalidate();
            }
            else
            {
                _pnlCameras.Controls.Remove(fpc);
                fpc.Dispose();
            }
            afp.Dispose();
        }

        private void SetCameraEvents(CameraWindow cameraControl)
        {
            cameraControl.MouseDown += CameraControlMouseDown;
            cameraControl.MouseWheel += CameraControlMouseWheel;
            cameraControl.MouseUp += CameraControlMouseUp;
            cameraControl.MouseMove += CameraControlMouseMove;
            cameraControl.DoubleClick += CameraControlDoubleClick;
            cameraControl.RemoteCommand += CameraControlRemoteCommand;
            cameraControl.Notification += ControlNotification;
            cameraControl.FileListUpdated += CameraControlFileListUpdated;
        }

        void CameraControlFileListUpdated(object sender)
        {
            lock (ThreadLock)
            {
                var cw = sender as CameraWindow;
                if (cw != null)
                {
                    Masterfilelist.RemoveAll(p => p.ObjectId == cw.Camobject.id && p.ObjectTypeId == 2);
                    var l = cw.FileList.ToList();
                    foreach (var ff in l)
                    {
                        Masterfilelist.Add(new FilePreview(ff.Filename, ff.DurationSeconds, cw.Camobject.name,
                                                           ff.CreatedDateTicks, 2, cw.Camobject.id, ff.MaxAlarm, ff.IsTimelapse, ff.IsMergeFile));
                    }
                    if (!cw.LoadedFiles)
                    {
                        cw.LoadedFiles = true;
                        //last one?
                        bool all = true;
                        foreach (Control c in _pnlCameras.Controls)
                        {
                            var cameraWindow = c as CameraWindow;
                            if (cameraWindow != null)
                            {
                                if (!cameraWindow.LoadedFiles)
                                {
                                    all = false;
                                    break;
                                }
                            }
                            var volumeLevel = c as VolumeLevel;
                            if (volumeLevel != null)
                            {
                                if (!volumeLevel.LoadedFiles)
                                {
                                    all = false;
                                    break;
                                }
                            }
                        }
                        if (all)
                        {
                            flowPreview.Loading = false;
                            LoadPreviews();
                        }
                    }
                }
            }

        }


        public PreviewBox AddPreviewControl(FilePreview fp, Image thumb, string movieName, string name)
        {
            var pb = new PreviewBox(fp.ObjectTypeId, fp.ObjectId, this) {Image = thumb, Duration = fp.Duration};
            pb.Width = pb.Image.Width;
            pb.Height = pb.Image.Height + 20;
            pb.Cursor = Cursors.Hand;
            pb.Selected = false;
            pb.FileName = movieName;
            pb.CreatedDate = new DateTime(fp.CreatedDateTicks);
            pb.MouseDown += PbMouseDown;
            pb.MouseEnter += PbMouseEnter;
            string txt = name + ": " + pb.CreatedDate.ToString(CultureInfo.CurrentUICulture);
            pb.DisplayName = txt;
            pb.IsMerged = fp.IsMerged;
            lock (ThreadLock)
            {
                flowPreview.Controls.Add(pb);
                
            }
            return pb;
        }
        public PreviewBox AddPreviewControl(FilePreview fp, string thumb, string movieName, string name)
        {
            Image bmp;
            try
            {
                if (!File.Exists(thumb))
                {
                    bmp = Resources.notfound;
                }
                else
                {
                    using (var f = File.Open(thumb, FileMode.Open, FileAccess.Read))
                    {
                        bmp = Image.FromStream(f);
                        f.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
                return null;
            }
            return AddPreviewControl(fp, bmp, movieName, name);
        }


        //public PreviewBox AddPreviewControl(int otid, int oid, Image thumb, string movieName, int duration, DateTime createdDate, string name)
        //{
        //    var pb = new PreviewBox(otid,oid,this) { Image = thumb };

        //    lock (ThreadLock)
        //    {
        //        pb.Duration = duration;
        //        pb.Width = pb.Image.Width;
        //        pb.Height = pb.Image.Height + 20;
        //        pb.Cursor = Cursors.Hand;
        //        pb.Selected = false;
        //        pb.FileName = movieName;
        //        pb.CreatedDate = createdDate;
        //        pb.MouseDown += PbMouseDown;
        //        pb.MouseEnter += PbMouseEnter;
        //        string txt = name + ": " + createdDate.ToString(CultureInfo.CurrentUICulture);
        //        pb.DisplayName = txt;
        //        flowPreview.Controls.Add(pb);
        //    }

        //    return pb;
        //}

        //public PreviewBox AddPreviewControl(int otid, int oid, string thumbname, string movieName, int duration, DateTime createdDate, string name)
        //{
        //    string thumb = thumbname;
        //    Image bmp;
        //    try
        //    {
        //        if (!File.Exists(thumb))
        //        {
        //            bmp = Resources.notfound;
        //        }
        //        else
        //        {
        //            using (var f = File.Open(thumb, FileMode.Open, FileAccess.Read))
        //            {
        //                bmp = Image.FromStream(f);
        //                f.Close();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogExceptionToFile(ex);
        //        return null;
        //    }
        //    return AddPreviewControl(otid,oid,bmp, movieName, duration, createdDate, name);
        //}


        private DateTime _lastOver = DateTime.MinValue;

        void PbMouseEnter(object sender, EventArgs e)
        {
            var pb = (PreviewBox)sender;
            tsslMediaInfo.Text = pb.DisplayName;
            _lastOver = Helper.Now;
        }

        void PbMouseDown(object sender, MouseEventArgs e)
        {
            var ctrl = sender as PreviewBox;
            if (ctrl != null)
            {
                ctrl.Focus();
                switch (e.Button)
                {
                    case MouseButtons.Right:
                        ContextTarget = ctrl;
                        ctxtPlayer.Show(ctrl, new Point(e.X, e.Y));
                        break;
                }
            }
        }

        public CameraWindow GetCameraWindow(int cameraId)
        {
            for (int index = 0; index < _pnlCameras.Controls.Count; index++)
            {
                var cw = _pnlCameras.Controls[index] as CameraWindow;
                if (cw != null && cw.Camobject.id == cameraId)
                    return cw;
            }
            return null;
        }

        public ISpyControl GetISpyControl(int ot, int id)
        {
            switch (ot)
            {
                case 1:
                    return GetVolumeLevel(id);
                case 2:
                    return GetCameraWindow(id);
                case 3:
                    return GetFloorPlan(id);
            }
            return null;
        }

        public VolumeLevel GetVolumeLevel(int microphoneId)
        {
            for (int index = 0; index < _pnlCameras.Controls.Count; index++)
            {
                var vl = _pnlCameras.Controls[index] as VolumeLevel;
                if (vl != null && vl.Micobject.id == microphoneId)
                    return vl;
            }
            return null;
        }

        private void SaveObjects(string fileName)
        {
            if (_shuttingDown)
                return;

            if (fileName == "")
                fileName = Program.AppDataPath + @"XML\objects.xml";
            var c = new objects { Version = Convert.ToInt32(Application.ProductVersion.Replace(".", "")), actions = new objectsActions {entries = _actions.ToArray()} };
            foreach (objectsCamera oc in Cameras)
            {
                CameraWindow occ = GetCameraWindow(oc.id);
                if (occ != null)
                {
                    oc.width = occ.Width;
                    oc.height = occ.Height;
                    oc.x = occ.Location.X;
                    oc.y = occ.Location.Y;
                }
            }
            c.cameras = Cameras.ToArray();
            foreach (objectsMicrophone om in Microphones)
            {
                VolumeLevel omc = GetVolumeLevel(om.id);
                if (omc != null)
                {
                    om.width = omc.Width;
                    om.height = omc.Height;
                    om.x = omc.Location.X;
                    om.y = omc.Location.Y;
                }
            }
            c.microphones = Microphones.ToArray();
            foreach (objectsFloorplan of in FloorPlans)
            {
                FloorPlanControl fpc = GetFloorPlan(of.id);
                if (fpc != null)
                {
                    of.width = fpc.Width;
                    of.height = fpc.Height;
                    of.x = fpc.Location.X;
                    of.y = fpc.Location.Y;
                }
            }
            c.floorplans = FloorPlans.ToArray();
            c.remotecommands = RemoteCommands.ToArray();
            lock (ThreadLock)
            {
                var s = new XmlSerializer(typeof(objects));
                var sb = new StringBuilder();
                using (var writer = new StringWriter(sb))
                {
                    try
                    {
                        s.Serialize(writer, c);
                        File.WriteAllText(fileName, sb.ToString(), Encoding.UTF8);
                    }
                    catch (Exception e)
                    {
                        LogExceptionToFile(e);
                    }
                }
            }
        }

        private static void SaveConfig()
        {
            lock (ThreadLock)
            {
                string fileName = Program.AppDataPath + @"XML\config.xml";
                //save configuration

                var s = new XmlSerializer(typeof(configuration));
                var sb = new StringBuilder();
                using (var writer = new StringWriter(sb))
                {
                    try
                    {
                        s.Serialize(writer, Conf);
                        File.WriteAllText(fileName, sb.ToString(), Encoding.UTF8);
                    }
                    catch (Exception e)
                    {
                        LogExceptionToFile(e);
                    }
                }
            }
        }

        private void LoadObjectList(string fileName)
        {
            if (_cameras != null && (_cameras.Count > 0 || _microphones.Count > 0 || _floorplans.Count > 0))
            {
                switch (
                    MessageBox.Show(this, LocRm.GetString("SaveObjectsFirst"), LocRm.GetString("Confirm"),
                                    MessageBoxButtons.YesNoCancel))
                {
                    case DialogResult.Yes:
                        SaveObjectList();
                        break;
                    case DialogResult.No:
                        break;
                    case DialogResult.Cancel:
                        return;
                }
            }
            _houseKeepingTimer.Stop();
            _tsslStats.Text = LocRm.GetString("Loading");
            Application.DoEvents();
            RemoveObjects();
            flowPreview.Loading = true;
            LoadObjects(fileName);
            RenderObjects();
            Application.DoEvents();
            try
            {
                _houseKeepingTimer.Start();
            }
            catch (Exception)
            {
            }
        }

        public void AddObjectExternal(int objectTypidId, int sourceIndex, int width, int height, string name, string url)
        {
            if (!VlcHelper.VlcInstalled && sourceIndex == 5)
                return;
            switch (objectTypidId)
            {
                case 2:
                    if (Cameras.FirstOrDefault(p => p.settings.videosourcestring == url) == null)
                    {
                        if (InvokeRequired)
                            Invoke(new Delegates.AddObjectExternalDelegate(AddCameraExternal), sourceIndex, url, width, height,
                                   name);
                        else
                            AddCameraExternal(sourceIndex, url, width, height, name);
                    }
                    break;
                case 1:
                    if (Microphones.FirstOrDefault(p => p.settings.sourcename == url) == null)
                    {
                        if (InvokeRequired)
                            Invoke(new Delegates.AddObjectExternalDelegate(AddMicrophoneExternal), sourceIndex, url, width, height,
                                   name);
                        else
                            AddMicrophoneExternal(sourceIndex, url, width, height, name);
                    }
                    break;
            }
            NeedsSync = true;
        }

        private void AddCameraExternal(int sourceIndex, string url, int width, int height, string name)
        {
            CameraWindow cw = NewCameraWindow(sourceIndex);
            cw.Camobject.settings.desktopresizewidth = width;
            cw.Camobject.settings.desktopresizeheight = height;
            cw.Camobject.settings.resize = false;
            cw.Camobject.name = name;
            cw.Camobject.settings.directoryIndex = Conf.MediaDirectories.First().ID;

            cw.Camobject.settings.videosourcestring = url;

            cw.Camobject.id = NextCameraId;
            Cameras.Add(cw.Camobject);
            var dir = Helper.GetMediaDirectory(2, cw.Camobject.id);
            string path = dir + "video\\" + cw.Camobject.directory + "\\";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path = dir + "video\\" + cw.Camobject.directory + "\\thumbs\\";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path = dir + "video\\" + cw.Camobject.directory + "\\grabs\\";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            cw.Camobject.settings.accessgroups = "";


            SetNewStartPosition();
            cw.Enable();
            cw.NeedSizeUpdate = true;
        }

        private void AddMicrophoneExternal(int sourceIndex, string url, int width, int height, string name)
        {
            VolumeLevel vl = NewVolumeLevel(sourceIndex);
            vl.Micobject.name = name;
            vl.Micobject.settings.sourcename = url;

            vl.Micobject.id = NextMicrophoneId;
            Microphones.Add(vl.Micobject);
            string path = Helper.GetMediaDirectory(1, vl.Micobject.id) + "audio\\" + vl.Micobject.directory + "\\";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            vl.Micobject.settings.accessgroups = "";
            SetNewStartPosition();
            vl.Enable();
        }

        internal VolumeLevel AddCameraMicrophone(int cameraid, string name)
        {
            if (cameraid == -1)
                cameraid = NextCameraId;
            VolumeLevel vl = NewVolumeLevel(4);
            vl.Micobject.name = name;
            vl.Micobject.settings.sourcename = cameraid.ToString(CultureInfo.InvariantCulture);
            vl.Micobject.id = NextMicrophoneId;
            Microphones.Add(vl.Micobject);
            string path = Helper.GetMediaDirectory(1, vl.Micobject.id) + "audio\\" + vl.Micobject.directory + "\\";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            vl.Micobject.settings.accessgroups = "";
            SetNewStartPosition();
            //vl.Enable();
            return vl;
        }

        #region CameraEvents

        private void CameraControlMouseMove(object sender, MouseEventArgs e)
        {
            if (Resizing) return;
            var cameraControl = (CameraWindow)sender;
            if (e.Button == MouseButtons.Left && !Conf.LockLayout)
            {
                int newLeft = cameraControl.Left + (e.X - cameraControl.Camobject.x);
                int newTop = cameraControl.Top + (e.Y - cameraControl.Camobject.y);
                if (newLeft < 0) newLeft = 0;
                if (newTop < 0) newTop = 0;
                if (newLeft + cameraControl.Width > 5 && newLeft < ClientRectangle.Width - 5)
                {
                    cameraControl.Left = newLeft;
                }
                if (newTop + cameraControl.Height > 5 && newTop < ClientRectangle.Height - 50)
                {
                    cameraControl.Top = newTop;
                }
            }

        }

        private void CameraControlMouseDown(object sender, MouseEventArgs e)
        {
            if (Resizing) return;
            var cameraControl = (CameraWindow)sender;
            cameraControl.Focus();

            switch (e.Button)
            {
                case MouseButtons.Left:
                    cameraControl.Camobject.x = e.X;
                    cameraControl.Camobject.y = e.Y;
                    cameraControl.BringToFront();
                    if (cameraControl.VolumeControl != null)
                        cameraControl.VolumeControl.BringToFront();
                    break;
                case MouseButtons.Right:
                    ContextTarget = cameraControl;
                    pluginCommandsToolStripMenuItem.Visible = false;
                    _setInactiveToolStripMenuItem.Visible = false;
                    _activateToolStripMenuItem.Visible = false;
                    _recordNowToolStripMenuItem.Visible = false;
                    _listenToolStripMenuItem.Visible = false;
                    _recordNowToolStripMenuItem.Visible = false;
                    _takePhotoToolStripMenuItem.Visible = false;
                    _viewMediaOnAMobileDeviceToolStripMenuItem.Visible = _viewMediaToolStripMenuItem.Visible = Helper.HasFeature(Enums.Features.Access_Media);
                    _applyScheduleToolStripMenuItem1.Visible = true;
                    _resetRecordingCounterToolStripMenuItem.Visible = true;
                    _resetRecordingCounterToolStripMenuItem.Text = LocRm.GetString("ResetRecordingCounter") + " (" +
                                                                   cameraControl.Camobject.newrecordingcount + ")";
                    pTZToolStripMenuItem.Visible = false;
                    if (cameraControl.Camobject.settings.active)
                    {
                        _setInactiveToolStripMenuItem.Visible = true;
                        if (Helper.HasFeature(Enums.Features.Recording))
                        {
                            _recordNowToolStripMenuItem.Visible = true;
                            _takePhotoToolStripMenuItem.Visible = true;
                        }
                        if (Helper.HasFeature(Enums.Features.Save_Frames))
                            _takePhotoToolStripMenuItem.Visible = true;

                        if (cameraControl.Camobject.ptz > -1)
                        {
                            pTZToolStripMenuItem.Visible = true;
                            while (pTZToolStripMenuItem.DropDownItems.Count > 1)
                                pTZToolStripMenuItem.DropDownItems.RemoveAt(1);

                            PTZSettings2Camera ptz = PTZs.SingleOrDefault(p => p.id == cameraControl.Camobject.ptz);
                            if (ptz != null)
                            {
                                if (ptz.ExtendedCommands != null && ptz.ExtendedCommands.Command != null)
                                {
                                    foreach (var extcmd in ptz.ExtendedCommands.Command)
                                    {
                                        ToolStripItem tsi = new ToolStripMenuItem
                                        {
                                            Text = extcmd.Name,
                                            Tag =
                                                cameraControl.Camobject.id + "|" + extcmd.Value
                                        };
                                        tsi.Click += TsiClick;
                                        pTZToolStripMenuItem.DropDownItems.Add(tsi);
                                    }
                                }
                            }
                        }

                        if (cameraControl.Camera != null && cameraControl.Camera.Plugin != null)
                        {
                            pluginCommandsToolStripMenuItem.Visible = true;

                            while (pluginCommandsToolStripMenuItem.DropDownItems.Count > 1)
                                pluginCommandsToolStripMenuItem.DropDownItems.RemoveAt(1);

                            var pc = cameraControl.PluginCommands;
                            if (pc != null)
                            {
                                foreach (var c in pc)
                                {
                                    ToolStripItem tsi = new ToolStripMenuItem
                                    {
                                        Text = c,
                                        Tag =
                                            cameraControl.Camobject.id + "|" + c
                                    };
                                    tsi.Click += PCClick;
                                    pluginCommandsToolStripMenuItem.DropDownItems.Add(tsi);
                                }
                            }
                        }

                    }
                    else
                    {
                        _activateToolStripMenuItem.Visible = true;
                    }
                    _recordNowToolStripMenuItem.Text =
                        LocRm.GetString(cameraControl.Recording ? "StopRecording" : "StartRecording");
                    ctxtMnu.Show(cameraControl, new Point(e.X, e.Y));
                    break;

            }
        }

        private void TsiClick(object sender, EventArgs e)
        {
            string[] cfg = ((ToolStripMenuItem)sender).Tag.ToString().Split('|');
            int camid = Convert.ToInt32(cfg[0]);
            var cw = GetCameraWindow(camid);
            if (cw != null && cw.PTZ != null)
            {
                cw.Calibrating = true;
                cw.PTZ.SendPTZCommand(cfg[1]);
            }
        }

        private void PCClick(object sender, EventArgs e)
        {
            string[] cfg = ((ToolStripMenuItem)sender).Tag.ToString().Split('|');
            int camid = Convert.ToInt32(cfg[0]);
            var cw = GetCameraWindow(camid);
            if (cw != null && cw.PluginCommands != null)
            {
                cw.ExecutePluginCommand(cfg[1]);
            }
        }

        private static void CameraControlMouseWheel(object sender, MouseEventArgs e)
        {
            var cameraControl = (CameraWindow)sender;

            cameraControl.PTZNavigate = false;
            if (cameraControl.PTZ != null)
            {


                if (!cameraControl.PTZ.DigitalZoom)
                {
                    cameraControl.Calibrating = true;
                    cameraControl.PTZ.SendPTZCommand(e.Delta > 0 ? Enums.PtzCommand.ZoomIn : Enums.PtzCommand.ZoomOut,
                        true);
                    if (cameraControl.PTZ.IsContinuous)
                        cameraControl.PTZ.SendPTZCommand(Enums.PtzCommand.Stop);
                }
                else
                {
                    Rectangle r = cameraControl.Camera.ViewRectangle;
                    //map location to point in the view rectangle
                    var ox =
                        Convert.ToInt32((Convert.ToDouble(e.Location.X) / Convert.ToDouble(cameraControl.Width)) *
                                        Convert.ToDouble(r.Width));
                    var oy =
                        Convert.ToInt32((Convert.ToDouble(e.Location.Y) / Convert.ToDouble(cameraControl.Height)) *
                                        Convert.ToDouble(r.Height));

                    cameraControl.Camera.ZPoint = new Point(r.Left + ox, r.Top + oy);
                    var f = cameraControl.Camera.ZFactor;
                    if (e.Delta > 0)
                    {
                        f += 0.2f;
                    }
                    else
                        f -= 0.2f;
                    if (f < 1)
                        f = 1;
                    cameraControl.Camera.ZFactor = f;
                }
                ((HandledMouseEventArgs)e).Handled = true;

            }
        }

        private void CameraControlMouseUp(object sender, MouseEventArgs e)
        {
            if (Resizing) return;
            var cameraControl = (CameraWindow)sender;
            switch (e.Button)
            {
                case MouseButtons.Left:
                    cameraControl.Camobject.x = cameraControl.Left;
                    cameraControl.Camobject.y = cameraControl.Top;
                    break;
            }
        }

        private void CameraControlDoubleClick(object sender, EventArgs e)
        {
            Maximise(sender);
        }

        #endregion

        #region VolumeEvents

        private void VolumeControlMouseDown(object sender, MouseEventArgs e)
        {
            if (Resizing) return;
            var volumeControl = (VolumeLevel)sender;
            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (!volumeControl.Paired)
                    {
                        volumeControl.Micobject.x = e.X;
                        volumeControl.Micobject.y = e.Y;
                    }
                    volumeControl.BringToFront();
                    if (volumeControl.Paired)
                    {
                        CameraWindow cw =
                            GetCameraWindow(Cameras.Single(p => p.settings.micpair == volumeControl.Micobject.id).id);
                        cw.BringToFront();
                    }
                    break;
                case MouseButtons.Right:
                    ContextTarget = volumeControl;
                    pluginCommandsToolStripMenuItem.Visible = false;
                    _setInactiveToolStripMenuItem.Visible = false;
                    _activateToolStripMenuItem.Visible = false;
                    _listenToolStripMenuItem.Visible = true;
                    _takePhotoToolStripMenuItem.Visible = false;
                    _resetRecordingCounterToolStripMenuItem.Visible = true;
                    _applyScheduleToolStripMenuItem1.Visible = true;
                    pTZToolStripMenuItem.Visible = false;
                    _viewMediaOnAMobileDeviceToolStripMenuItem.Visible = _viewMediaToolStripMenuItem.Visible = Helper.HasFeature(Enums.Features.Access_Media);
                    _resetRecordingCounterToolStripMenuItem.Text = LocRm.GetString("ResetRecordingCounter") + " (" +
                                                                   volumeControl.Micobject.newrecordingcount + ")";

                    if (volumeControl.Listening)
                    {
                        _listenToolStripMenuItem.Text = LocRm.GetString("StopListening");
                        _listenToolStripMenuItem.Image = Resources.listenoff2;
                    }
                    else
                    {
                        _listenToolStripMenuItem.Text = LocRm.GetString("Listen");
                        _listenToolStripMenuItem.Image = Resources.listen2;
                    }
                    _recordNowToolStripMenuItem.Visible = false;
                    if (volumeControl.Micobject.settings.active)
                    {
                        _setInactiveToolStripMenuItem.Visible = true;
                        if (Helper.HasFeature(Enums.Features.Recording))
                        {
                            _recordNowToolStripMenuItem.Visible = true;
                        }

                        _listenToolStripMenuItem.Enabled = true;
                    }
                    else
                    {
                        _activateToolStripMenuItem.Visible = true;
                        _listenToolStripMenuItem.Enabled = false;
                    }
                    _recordNowToolStripMenuItem.Text =
                        LocRm.GetString(volumeControl.ForcedRecording ? "StopRecording" : "StartRecording");
                    ctxtMnu.Show(volumeControl, new Point(e.X, e.Y));
                    break;
            }
            volumeControl.Focus();
        }

        private void VolumeControlMouseUp(object sender, MouseEventArgs e)
        {
            if (Resizing) return;
            var volumeControl = (VolumeLevel)sender;
            if (e.Button == MouseButtons.Left && !volumeControl.Paired)
            {
                volumeControl.Micobject.x = volumeControl.Left;
                volumeControl.Micobject.y = volumeControl.Top;
            }
        }


        private void VolumeControlMouseMove(object sender, MouseEventArgs e)
        {
            if (Resizing) return;
            var volumeControl = (VolumeLevel)sender;
            if (e.Button == MouseButtons.Left && !volumeControl.Paired && !Conf.LockLayout)
            {
                int newLeft = volumeControl.Left + (e.X - Convert.ToInt32(volumeControl.Micobject.x));
                int newTop = volumeControl.Top + (e.Y - Convert.ToInt32(volumeControl.Micobject.y));
                if (newLeft < 0) newLeft = 0;
                if (newTop < 0) newTop = 0;
                if (newLeft + volumeControl.Width > 5 && newLeft < ClientRectangle.Width - 5)
                {
                    volumeControl.Left = newLeft;
                }
                if (newTop + volumeControl.Height > 5 && newTop < ClientRectangle.Height - 50)
                {
                    volumeControl.Top = newTop;
                }
            }

        }

        #endregion

        #region FloorPlanEvents

        private void FloorPlanMouseDown(object sender, MouseEventArgs e)
        {
            if (Resizing) return;
            var fpc = (FloorPlanControl)sender;
            if (e.Button == MouseButtons.Left)
            {
                fpc.Fpobject.x = e.X;
                fpc.Fpobject.y = e.Y;
                fpc.BringToFront();
            }
            else
            {
                if (e.Button == MouseButtons.Right)
                {
                    ContextTarget = fpc;
                    pluginCommandsToolStripMenuItem.Visible = false;
                    _setInactiveToolStripMenuItem.Visible = false;
                    _listenToolStripMenuItem.Visible = false;
                    _activateToolStripMenuItem.Visible = false;
                    _resetRecordingCounterToolStripMenuItem.Visible = false;
                    _recordNowToolStripMenuItem.Visible = false;
                    _takePhotoToolStripMenuItem.Visible = false;
                    _applyScheduleToolStripMenuItem1.Visible = false;
                    pTZToolStripMenuItem.Visible = false;

                    ctxtMnu.Show(fpc, new Point(e.X, e.Y));
                }
            }
            fpc.Focus();
        }

        private void FloorPlanMouseUp(object sender, MouseEventArgs e)
        {
            if (Resizing) return;
            var fpc = (FloorPlanControl)sender;
            if (e.Button == MouseButtons.Left)
            {
                fpc.Fpobject.x = fpc.Left;
                fpc.Fpobject.y = fpc.Top;
            }
        }

        private void FloorPlanMouseMove(object sender, MouseEventArgs e)
        {
            if (Resizing) return;
            var fpc = (FloorPlanControl)sender;
            if (e.Button == MouseButtons.Left && !Conf.LockLayout)
            {
                int newLeft = fpc.Left + (e.X - Convert.ToInt32(fpc.Fpobject.x));
                int newTop = fpc.Top + (e.Y - Convert.ToInt32(fpc.Fpobject.y));
                if (newLeft < 0) newLeft = 0;
                if (newTop < 0) newTop = 0;
                if (newLeft + fpc.Width > 5 && newLeft < ClientRectangle.Width - 5)
                {
                    fpc.Left = newLeft;
                }
                if (newTop + fpc.Height > 5 && newTop < ClientRectangle.Height - 50)
                {
                    fpc.Top = newTop;
                }
            }
        }

        #endregion

        #region RestoreSavedCameras

        private void DisplayCamera(objectsCamera cam)
        {
            var cameraControl = new CameraWindow(cam,this);
            SetCameraEvents(cameraControl);
            cameraControl.BackColor = Conf.BackColor.ToColor();
            _pnlCameras.Controls.Add(cameraControl);
            cameraControl.Location = new Point(cam.x, cam.y);
            cameraControl.Size = new Size(cam.width, cam.height);
            cameraControl.BringToFront();
            cameraControl.Tag = GetControlIndex();

            var dir = Helper.GetMediaDirectory(2, cam.id);
            string path = dir + "video\\" + cam.directory + "\\";
            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                path = dir + "video\\" + cam.directory + "\\thumbs\\";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    //move existing thumbs into directory
                    var lfi =
                        Directory.GetFiles(dir + "video\\" + cam.directory + "\\", "*.jpg").ToList();
                    foreach (string file in lfi)
                    {
                        string destfile = file;
                        int i = destfile.LastIndexOf(@"\", StringComparison.Ordinal);
                        destfile = file.Substring(0, i) + @"\thumbs" + file.Substring(i);
                        File.Move(file, destfile);
                    }
                }
                path = dir + "video\\" + cam.directory + "\\grabs\\";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
            }

            cameraControl.GetFiles();
        }

        private void DoInvoke(string methodName)
        {
            if (methodName == "show")
            {
                ShowIfUnlocked();
                return;
            }
            if (methodName.StartsWith("bringtofrontcam"))
            {
                int camid = Convert.ToInt32(methodName.Split(',')[1]);
                foreach (Control c in _pnlCameras.Controls)
                {
                    if (c is CameraWindow)
                    {
                        var cameraControl = (CameraWindow)c;
                        if (cameraControl.Camobject.id == camid)
                        {
                            cameraControl.BringToFront();
                            break;
                        }
                    }
                }
                return;
            }
            if (methodName.StartsWith("bringtofrontmic"))
            {
                int micid = Convert.ToInt32(methodName.Split(',')[1]);
                foreach (Control c in _pnlCameras.Controls)
                {
                    if (c is VolumeLevel)
                    {
                        var vl = (VolumeLevel)c;
                        if (vl.Micobject.id == micid)
                        {
                            vl.BringToFront();
                            break;
                        }
                    }
                }
                return;
            }
        }

        private void BClick(object sender, EventArgs e)
        {
            RunCommand((int)((Button)sender).Tag);
        }

        private void CameraControlRemoteCommand(object sender, ThreadSafeCommand e)
        {
            Delegates.InvokeMethod i = DoInvoke;
            Invoke(i, new object[] { e.Command });
        }

        #endregion

        #region Nested type: ListItem

        public struct ListItem
        {
            private readonly string _name;
            internal readonly string[] Value;

            public ListItem(string name, string[] value)
            {
                _name = name;
                Value = value;
            }

            public override string ToString()
            {
                return _name;
            }
        }

        public struct ListItem2
        {
            private readonly string _name;
            internal readonly int Value;

            public ListItem2(string name, int value)
            {
                _name = name;
                Value = value;
            }

            public override string ToString()
            {
                return _name;
            }
        }

        #endregion      

        #region Nested type: clsCompareFileInfo

        public class ClsCompareFileInfo : IComparer
        {
            #region IComparer Members

            public int Compare(object x, object y)
            {
                var file1 = (FileInfo)x;
                var file2 = (FileInfo)y;

                return 0 - DateTime.Compare(file1.CreationTime, file2.CreationTime);
            }

            #endregion
        }

        #endregion
    }

    public class FilePreview
    {
        public string Filename;
        public int Duration;
        public string Name;
        public long CreatedDateTicks;
        public int ObjectTypeId;
        public int ObjectId;
        public double MaxAlarm;
        public bool IsTimeLapse;
        public bool IsMerged;

        public FilePreview(string filename, int duration, string name, long createdDateTicks, int objectTypeId, int objectId, double maxAlarm, bool isTimelapse, bool isMerged)
        {
            Filename = filename;
            Duration = duration;
            Name = name;
            ObjectTypeId = objectTypeId;
            ObjectId = objectId;
            CreatedDateTicks = createdDateTicks;
            MaxAlarm = maxAlarm;
            IsMerged = isMerged;
            IsTimeLapse = isTimelapse;
        }
    }
}
