using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Server
{
    public class ConnectHandler
    {
        private CloudPhoneWindow cloudPhoneWindow;
        private int size;
        public TcpListener threadListener;
        public bool isRunning { get; set; }

        public ConnectHandler(CloudPhoneWindow c)
        {
            isRunning = true;
            cloudPhoneWindow = c;
            cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "클라이언트 생성, ConnectHandler");

        }

        public void clientHandler()
        {
            try
            {
                TcpClient client = threadListener.AcceptTcpClient();
                NetworkStream ns = client.GetStream();
                byte[] buffer = new byte[1024];

                while (isRunning)
                {
                    try
                    {
                        int totalLength = 0;

                        ns.Read(buffer, 0, buffer.Length);
                        int fileLength = BitConverter.ToInt32(buffer, 0);
                        var stream = new MemoryStream();
                        while (totalLength < fileLength)
                        {
                            int receiveLength = ns.Read(buffer, 0, buffer.Length);
                            stream.Write(buffer, 0, receiveLength);
                            totalLength += receiveLength;
                        }

                    }
                    catch (Exception e)
                    {
                        cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "clientHandler : " + e.Message);
                        isRunning = false;
                    }
                }
                ns.Close();
                client.Close();

            }
            catch(Exception e)
            {
                cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "error", "clientHandler : " + e.Message);
                isRunning = false;
            }
        }
    }
}
