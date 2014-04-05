using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CloudPhoneTestServer
{

    public class ConnectionHandler
    {
        private CloudPhoneForm cloudphoneForm;
        private int size;

        public ConnectionHandler(CloudPhoneForm form)
        {
            isRunning = true;
            cloudphoneForm = form;
            cloudphoneForm.Invoke(cloudphoneForm._logi, "클라이언트 생성 ConnectionHandler");
        }

        public TcpListener threadListener;

        public Boolean isRunning { get; set; }

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
                        while(totalLength < fileLength)
                        {
                            int receiveLength = ns.Read(buffer, 0, buffer.Length);
                            stream.Write(buffer, 0, receiveLength);
                            totalLength += receiveLength;
                        }

                        Image image = Image.FromStream(stream);
                        cloudphoneForm.Invoke(cloudphoneForm.myDelegate, image);
                    }
                    catch (Exception e)
                    {
                        cloudphoneForm.Invoke(cloudphoneForm._loge, "clientHandler : " + e.Message);
                        isRunning = false;
                    }
                }
                ns.Close();
                client.Close();
            }
            catch (Exception e2)
            {
                cloudphoneForm.Invoke(cloudphoneForm._loge, "clientHandler : " + e2.Message);
            }
            cloudphoneForm.Invoke(cloudphoneForm._logi, "클라이언트가 종료되었습니다.");
        }
    }
}
