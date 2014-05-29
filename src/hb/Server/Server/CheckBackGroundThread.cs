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
    class CheckBackGroundThread
    {
        CloudPhoneWindow cloudPhoneWindow;
        Byte[] data = new Byte[4096 * 10];

        public bool isRunning { get; set; }

        private EndPoint localEP = new IPEndPoint(IPAddress.Any, 20001);
        private EndPoint remoteEP = new IPEndPoint(IPAddress.None, 20002);
        private int recvSize;
        public Socket udpSock; // udp 통신을 위한 소켓
        public BinaryReader br; // 파일 전송을 위한 바이너리리더
        public FileStream fs; // 파일 전송을 위한 스트림

        public EndPoint udpSender; // 클라이언트의 정보를 담은 IP END POINT

        public CheckBackGroundThread(CloudPhoneWindow _c, Socket _s, IPEndPoint _ipep)
        {
            isRunning = true;
            cloudPhoneWindow = _c;
            udpSender = _ipep;
            udpSock = _s;
            cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "BG 쓰레드 클라이언트 생성");
        }

        public void udpCheckbg()
        {
            udpSock.Bind(localEP);

            while (isRunning)
            {
                //int i = 0, count = 0;
                //String bgPath;
                //String path = Application.StartupPath + "\\..\\..\\background\\bg.png";

                udpSock.ReceiveFrom(data, ref remoteEP);
                udpSock.SendTo(data, udpSender);
                
                //String tmp = Encoding.UTF8.GetString(data).Trim();
                //cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "받은 Size : " + tmp);

                //if (tmp.Substring(0, 4).Equals("size") == true)
                //{
                //    recvSize = Convert.ToInt32(tmp.Substring(4, tmp.Length));
                //}

                //count = (recvSize / (4096 * 10)) + 1;

                //for (i = 0; i < count; i++)
                //{
                //    udpSock.ReceiveFrom(data, ref remoteEP);
                //    cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "Data 받음");
                //    udpSock.SendTo(data, udpSender);
                //    cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "Data 전송 끝");
                //}

                //if (!System.IO.File.Exists(Application.StartupPath + "\\..\\..\\background\\bg.png"))
                //{
                //    System.Threading.Thread.Sleep(10000);
                //    bgPath = @"adb shell screencap -p /sdcard/bg.png";
                //    // cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "bgPath : " + bgPath);
                //    cloudPhoneWindow.ControlCMD(bgPath);
                //    System.Threading.Thread.Sleep(500);
                //    bgPath = @"adb pull /sdcard/bg.png ""C:\Users\Hyunbin\Desktop\Software Membership\창의과제 및 미니프로젝트\CloudPhone\src\hb\Server\Server\background\bg.png""";
                //    cloudPhoneWindow.ControlCMD(bgPath);
                //    System.Threading.Thread.Sleep(100);
                //}
                //else
                //{
                //    bgPath = @"adb shell screencap -p /sdcard/bg.png";
                //    // cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "bgPath : " + bgPath);
                //    cloudPhoneWindow.ControlCMD(bgPath);
                //    System.Threading.Thread.Sleep(500);
                //    bgPath = @"adb pull /sdcard/bg.png ""C:\Users\Hyunbin\Desktop\Software Membership\창의과제 및 미니프로젝트\CloudPhone\src\hb\Server\Server\background\bg.png""";
                //    cloudPhoneWindow.ControlCMD(bgPath);
                //    System.Threading.Thread.Sleep(100);

                //    fs = new FileStream(Application.StartupPath + "\\..\\..\\background\\bg.png", FileMode.Open);

                //    /*
                //     *  Img 크기 구해서 먼저 패킷으로 보낸다. 
                //     */

                //    int fSize = (int)fs.Length;

                //    cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "file SIze  : " + fSize);

                //    String fileSize = "size" + fSize.ToString();
                //    data = Encoding.UTF8.GetBytes(fileSize);
                //    udpSock.SendTo(data, udpSender);

                //    Array.Clear(data, 0, data.Length); // 배열 초기화

                //    cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "파일 전송 시작");

                //    br = new BinaryReader(fs);

                //    count = fSize / (4096 * 10) + 1;

                //    try
                //    {
                //        for (i = 0; i < count; i++)
                //        {
                //            data = br.ReadBytes(4096 * 10);
                //            cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "전송 Byte : " + udpSock.SendTo(data, udpSender));
                //            Array.Clear(data, 0, data.Length);
                //        }

                //        cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "파일 전송 끝");

                //        // isRunning = false; /// TEST CODE
                //    }
                //    catch (Exception e)
                //    {
                //        cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "error", "파일 전송 에러 : " + e.Message);
                //        isRunning = false;
                //    }

                //    Thread.Sleep(200); // 0.2초 쉰다

                //    br.Close();
                //    fs.Close();
                //}
            }
        }
    }
}
