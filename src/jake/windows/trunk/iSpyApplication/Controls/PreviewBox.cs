﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using iSpyApplication.Properties;
using iSpyApplication.Video;

namespace iSpyApplication.Controls
{
    public class PreviewBox: AForge.Controls.PictureBox
    {
        private readonly Brush _bPlay = new SolidBrush(Color.FromArgb(90,0,0,0));
        public bool Selected;
        public string FileName = "";
        public string DisplayName;
        public DateTime CreatedDate = DateTime.MinValue;
        public int Duration;
        private bool _linkPlay, _linkHover;
        public bool ShowThumb = true;
        public int Otid,Oid;
        public MainForm MainClass;
        public bool IsMerged;

        public PreviewBox(int otid, int oid, MainForm mainForm)
        {
            Otid = otid;
            Oid = oid;
            MainClass = mainForm;
        }

        public FilesFile FileData
        {
            get
            {
                string fn = FileName.Substring(FileName.LastIndexOf("\\", StringComparison.Ordinal) + 1);

                switch (Otid)
                {
                    case 1:
                        var vl = MainClass.GetVolumeLevel(Oid);
                        if (vl != null)
                            return vl.FileList.FirstOrDefault(p => p.Filename == fn);
                        return null;
                    case 2:
                        var cw = MainClass.GetCameraWindow(Oid);
                        if (cw != null)
                            return cw.FileList.FirstOrDefault(p => p.Filename == fn);
                        return null;
                }
                return null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (Image != null)
            {
                Image.Dispose();
            }
            _bPlay.Dispose();

            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            var g = pe.Graphics;

            if (ShowThumb)
            {
                if (IsMerged)
                {
                    g.DrawImage(Resources.merged,Width-Resources.merged.Width-2,Height-22-Resources.merged.Height,Resources.merged.Width,Resources.merged.Height);
                }

                if (_linkPlay)
                {
                    g.FillRectangle(_bPlay, 0, 0, Width, Height - 20);
                }
                if (Selected)
                {
                    g.DrawImage(Resources.checkbox, Width - 17, Height - 19, 17, 16);
                }
                else
                {
                    if (_linkHover)
                    {
                        g.DrawImage(Resources.checkbox_off, Width - 17, Height - 19, 17, 16);
                    }
                }

                if (_linkPlay)
                {
                    g.DrawString(">", MainForm.DrawfontBig, Brushes.White, Width/2 - 10, 20);
                }

                
                g.DrawString(
                    CreatedDate.Hour + ":" + ZeroPad(CreatedDate.Minute) + ":" + ZeroPad(CreatedDate.Second) + " (" +
                    RecordTime(Duration) + ")", MainForm.Drawfont, Brushes.White, 0, Height - 18);
            }
        }
        private static string RecordTime(decimal sec)
        {
            var hr = Math.Floor(sec / 3600);
            var min = Math.Floor((sec - (hr * 3600)) / 60);
            sec -= ((hr * 3600) + (min * 60));
            string m = min.ToString(CultureInfo.InvariantCulture);
            string s = sec.ToString(CultureInfo.InvariantCulture);
            while (m.Length < 2) { m = "0" + m; }
            while (s.Length < 2) { s = "0" + s; }
            string h = (hr!=0) ? hr + ":" : "";
            return h + m + ':' + s;
        }
        private static string ZeroPad(int i)
        {
            if (i < 10)
                return "0" + i;
            return i.ToString(CultureInfo.InvariantCulture);
        }
        
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool last = _linkPlay;
            bool last2 = _linkHover;
            _linkPlay = e.Location.Y < Height - 20;
            _linkHover = e.Location.Y > Height - 20;
            if (last != _linkPlay || last2 != _linkHover)
                Invalidate();
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            _linkPlay = false;
            _linkHover = false;

            Invalidate();
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            PlayMedia((Enums.PlaybackMode)MainForm.Conf.PlaybackMode);
        }

        protected override void  OnMouseClick(MouseEventArgs e)
        {
 	        base.OnMouseClick(e);
            if (e.Button == MouseButtons.Left)
            {
                if (e.Y > Height - 20)
                {
                    Selected = !Selected;
                    Invalidate();

                    if ((ModifierKeys & Keys.Shift) == Keys.Shift)
                    {
                        if (TopLevelControl != null)
                            ((MainForm)TopLevelControl).SelectMediaRange(this, (PreviewBox)(Parent.Tag));
                    }
                    else
                    {
                        Parent.Tag = this;
                    }
                }
                else
                {
                    PlayMedia((Enums.PlaybackMode)MainForm.Conf.PlaybackMode);
                    
                }
            }
        }

        public void PlayMedia(Enums.PlaybackMode mode)
        {
            if (mode < 0)
                MainForm.Conf.PlaybackMode = 0;
            if (!VlcHelper.VlcInstalled && mode == Enums.PlaybackMode.iSpy)
            {
                if (Program.Platform == "x64")
                    MessageBox.Show(this, LocRm.GetString("InstallVLCx64").Replace("[DIR]", Environment.NewLine + Program.AppPath + "VLC64" + Environment.NewLine));
                else
                    MessageBox.Show(this, LocRm.GetString("InstallVLCx86"));
                MainForm.OpenUrl(Program.Platform == "x64" ? MainForm.VLCx64 : MainForm.VLCx86);
                MainForm.Conf.PlaybackMode = 0;
                mode = Enums.PlaybackMode.Website;
            }

           
            string movie = FileName;
            if (!File.Exists(movie))
            {
                MessageBox.Show(this, LocRm.GetString("FileNotFound"));
                return;                
            }
            if (MainForm.Conf.PlaybackMode == 0 && movie.EndsWith(".avi"))
            {
                mode = Enums.PlaybackMode.iSpy;
            }

            string[] parts = FileName.Split('\\');
            string fn = parts[parts.Length - 1];
            switch (mode)
            {
                case Enums.PlaybackMode.Website:
                    string url = MainForm.Webserver + "/MediaViewer.aspx?oid=" + Oid + "&ot=2&fn=" + fn + "&port=" + MainForm.Conf.ServerPort;
                    if (WsWrapper.WebsiteLive && MainForm.Conf.ServicesEnabled)
                    {
                        MainForm.OpenUrl(url);
                    }
                    else
                    {
                        if (!WsWrapper.WebsiteLive)
                        {
                            MessageBox.Show(this, LocRm.GetString("iSpyDown"));
                        }
                        else
                        {
                            if (TopLevelControl != null)
                                ((MainForm) TopLevelControl).Connect(url, false);
                        }
                    }
                    break;
                case Enums.PlaybackMode.iSpy:
                    if (TopLevelControl != null)
                        ((MainForm)TopLevelControl).Play(movie, Oid, DisplayName);
                    break;
                case Enums.PlaybackMode.Default:
                    try
                    {
                        Process.Start(movie);
                    }
                    catch (Exception ex)
                    {
                        MainForm.LogExceptionToFile(ex);
                        MessageBox.Show(LocRm.GetString("NoPlayerForThisFile"));
                    }
                    break;
            }
        }
        
    }
}
