using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;


namespace GPSTEST
{
    class Program
    {
        public static void TelnetConnectAndSendMsg(int portNum, String Msg)
        {
            TcpClient cpTcpClient = new TcpClient("localhost", portNum);
            if (true == cpTcpClient.Connected)
            {
               NetworkStream cpNetStream = cpTcpClient.GetStream();
               StreamWriter cpStmWriter = new StreamWriter(cpNetStream);
                cpStmWriter.WriteLine(Msg);
                cpStmWriter.Flush();
            }
            else
            {
                return;
            }
            return;
        }

        static void Main(string[] args)
        {
          

          TelnetConnectAndSendMsg(5554, "geo fix 127.03 37.42");
        }
    }
}
