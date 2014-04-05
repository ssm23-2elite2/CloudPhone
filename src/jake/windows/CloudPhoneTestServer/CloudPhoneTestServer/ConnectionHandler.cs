using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CloudPhoneTestServer
{

    public class ConnectionHandler
    {
        public ConnectionHandler()
        {
            isRunning = true;
        }

        public TcpListener threadListener;

        public Boolean isRunning { get; set; }

        public void clientHandler()
        {
            TcpClient client = threadListener.AcceptTcpClient();
            NetworkStream ns = client.GetStream();
            byte[] buffer = new byte[4096];

            while (isRunning)
            {
                try
                {
                    ns.Read(buffer, 0, buffer.Length);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            ns.Close();
            client.Close();
        }
    }
}
