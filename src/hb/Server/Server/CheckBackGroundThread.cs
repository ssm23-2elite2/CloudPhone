using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        byte[] data = new byte[4096];

        public bool isRunning { get; set; }
        //public TcpListener threadListener;
        //NetworkStream ns;
        //TcpClient client;

        //public IPEndPoint udpSender;
        //public UdpClient udpClient;
        //public String endIP;

        public CheckBackGroundThread(CloudPhoneWindow _c)
        {
            isRunning = true;
            cloudPhoneWindow = _c;
            cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "BG 쓰레드 클라이언트 생성");
        }

        public void tcpCheckbg()
        {
            //IPHostEntry ipHost = Dns.Resolve(endIP);
            //IPAddress ipAddr = ipHost.AddressList[0];

            //IPEndPoint ipep = new IPEndPoint(ipAddr, 20001);
            //udpClient = new UdpClient(ipep);
            try
            {
                //client = threadListener.AcceptTcpClient();
                //ns = client.GetStream();

                while (isRunning)
                {

                    try
                    {
                        int i = 0;
                        System.IO.StreamWriter sw = new System.IO.StreamWriter(ns);

                        //IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                        //byte[] data = udpClient.Receive(ref remote);

                        String path = Application.StartupPath + "\\..\\..\\background\\bg.png";
                        while (!System.IO.File.Exists(Application.StartupPath + "\\..\\..\\background\\bg.png"))
                        {
                            cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "path : " + path + " 없어서 기달");
                            System.Threading.Thread.Sleep(100);
                        }


                        cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "파일 전송 시작");

                        FileStream fs = new FileStream(Application.StartupPath + "\\..\\..\\background\\bg.png", FileMode.Open);

                        while ((i = fs.Read(data, 0, 4096)) > 0)
                        {
                            // ns.Write(data, 0, i); // TCP

                        }

                        cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "파일 전송 끝");

                        fs.Close();
                        sw.Close();
                    }
                    catch (Exception e)
                    {
                        cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "error", "tcpCheckBG while msg : " + e.Message);
                        isRunning = false;
                    }

                }
                //ns.Close();
                //client.Close();
            }
            catch (Exception e)
            {
                cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "error", "tcpCheckBG msg : " + e.Message);
                isRunning = false;
            }
        }
    }
}
