using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Windows.Forms;



namespace Server
{
    public class CheckBackGroundThread
    {
        CloudPhoneWindow cloudPhoneWindow;
        Byte[] data = new Byte[4096 * 10];

        public bool isRunning { get; set; }

        private EndPoint localEP = new IPEndPoint(IPAddress.Any, 20001);
        private EndPoint remoteEP = new IPEndPoint(IPAddress.None, 20002);
        public Socket udpSock; // udp 통신을 위한 소켓

        public EndPoint udpSender; // 클라이언트의 정보를 담은 IP END POINT

        public CheckBackGroundThread(CloudPhoneWindow _c, Socket _s, IPEndPoint _ipep)
        {
            isRunning = true;
            cloudPhoneWindow = _c;
            udpSender = _ipep;
            udpSock = _s;
          //  cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "BG 쓰레드 클라이언트 생성");
        }

        public void udpCheckbg()
        {
            udpSock.Bind(localEP);
            cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "udp BG Send Start, udpCheckbg");
            while (isRunning)
            {
                udpSock.ReceiveFrom(data, ref remoteEP);
                udpSock.SendTo(data, udpSender);
            }

            udpSock.Close();
            udpSock = null;
        }

        public void RequestStop()
        {
            isRunning = false;
        }
    }
}
