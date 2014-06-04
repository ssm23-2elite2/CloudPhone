using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Server
{
    public class CheckACKThread
    {
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        private static System.Timers.Timer aTimer;
        public static int portNum;
        public static DateTime compare;
        public static bool isRunning = true;
        CloudPhoneWindow cloudPhoneWindow;

        public CheckACKThread(CloudPhoneWindow _cloudPhoneWindow, int _portNum)
        {
            cloudPhoneWindow = _cloudPhoneWindow;
            portNum = _portNum;
            
        }

        public void SendAckToClient()
        {
            aTimer = new System.Timers.Timer(30000); // 30초마다 이벤트 발생

            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Enabled = true;

            while (isRunning)
            {

            }

            // Server Client Exit
            cloudPhoneWindow.Invoke(cloudPhoneWindow._exitClient, portNum);

            return;
        }

        public void RequestStop()
        {
            isRunning = false;
        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            TimeSpan ts = e.SignalTime - compare;

            if (ts.Seconds >= 30) // 30초동안 응답이 없으면 해당 AVD를 종료시킨다.
            {
               // 종료 코드
                ErrExit(portNum);
                isRunning = false;
            }
        }

        public DateTime getTime()
        {
            return compare;
        }

        public void setTime(DateTime t)
        {
            compare = t;
        }

        public static void ErrExit(int PortNum)
        {
            // 타이틀 바의 이름으로 찾아서 프로그램 종료
            IntPtr hwd = FindWindow(null, PortNum.ToString() + ":client_dev");
            if (hwd.ToString() != "0")
                SendMessage(hwd, 0x0010, 0, 0);
        }
    }

}
