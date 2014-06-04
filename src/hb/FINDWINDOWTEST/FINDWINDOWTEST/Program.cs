using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FINDWINDOWTEST
{
    class Program
    {
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        public static void ErrExit(int PortNum)
        {
            // 타이틀 바의 이름으로 찾아서 프로그램 종료
            IntPtr hwd = FindWindow(null, PortNum.ToString() + ":client_dev");
            if (hwd.ToString() != "0")
                SendMessage(hwd, 0x0010, 0, 0);
        }

        static void Main(string[] args)
        {
              ErrExit(5554);
        }
    }
}
