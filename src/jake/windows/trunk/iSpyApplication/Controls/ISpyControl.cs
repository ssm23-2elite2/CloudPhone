﻿using System.Drawing;
using System.Windows.Forms;

namespace iSpyApplication.Controls
{
    public interface ISpyControl
    {
        bool IsEnabled { get;}
        bool Talking { get; set; }
        bool Listening { get; }
        bool Recording { get; }
        bool CanTalk { get; }
        bool CanListen { get; }
        bool CanRecord { get; }
        bool CanEnable { get; }
        bool CanGrab { get; }
        bool HasFiles { get; }
        string ObjectName { get; }

        void Disable(bool stopSource=true);
        void Enable();
        string RecordSwitch(bool record);
        void Talk(IWin32Window f = null);
        void Listen();
        string SaveFrame(Bitmap bmp = null);
        


    }
}
