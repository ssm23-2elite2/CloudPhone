using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

namespace Server
{
    public class ConnectHandler
    {
        private CloudPhoneWindow cloudPhoneWindow;
        public TcpListener threadListener;
        public bool isRunning { get; set; }
        public String ip;
        TcpClient client;
        public String clientID;
        NetworkStream ns;

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
                client = threadListener.AcceptTcpClient();
                ns = client.GetStream();
                System.IO.StreamReader sr = new System.IO.StreamReader(ns);

                while (isRunning)
                {
                    try
                    {
                        String receive = "";

                        receive = sr.ReadLine();
                        cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "Receive MSG : " + receive);
                        
                        UnPackingMessage(receive);
                       
                    }
                    catch (Exception e)
                    {
                        cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "error", "clientHandler with while: " + e.Message);
                        isRunning = false;
                    }
                }

                sr.Close();
                ns.Close();
                client.Close();

            }
            catch(Exception e)
            {
                cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "error", "clientHandler : " + e.Message);
                isRunning = false;
            }
        }

        // Client로 메시지 전달
        private void SendMsg(String msg)
        {
            if (ns != null && ns.CanWrite == true)
            {
                System.IO.StreamWriter sw = new System.IO.StreamWriter(ns);
                sw.WriteLine(msg);
                sw.Flush();
                sw.Close();
            }
        }

        // Client로부터 온 메시지 Unpack
        private void UnPackingMessage(String str)
        {
            String[] strings = str.Split('/');

            // Unpacking해서 메시지 헤더에 따라서 돌린다.
            // 메시지 구조  :  Type / Size / Data ( Data는 '쉼표'로 구분 ',')
            // ACTION_DOWN, ACTION_MOVE, ACTION_UP, ACTION_POINTER_DOWN,
            // ACTION_POINTER_UP, KEYCODE_HOME, KEYCODE_VOLUME_DOWN, KEYCODE_VOLUME_UP, KEYCODE_POWER, GPS, GYRO, BATTERY

            switch (strings[0])
            {
                case "0": // Login 메시지가 들어오면, 있는 ID인지 검사하도록  CloudPhoneWindow에 있는 CheckClientID를 실행시킨다.
                    String[] data = strings[2].Split(',');
                    ip = data[0];
                    clientID = data[1];
                    cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "Client ip : " + ip + ", Client ID : " + clientID);
                    break;

                case "1": // AVD MSG ( 생성 / 삭제 / 실행 / 종료 ) 
                    cloudPhoneWindow.DecideAVDMsg(strings[2]);
                    break;

                case "2": // ACTION

                    break;

                case "3": // KEYCODE VALUE

                    break;

                case "4": // GPS VALUE
                    
                    break;

                case "5": // GYRO VALUE

                    break;

                case "6": // BATTERY VALUE

                    break;

                case "7": // Client LogOut

                    break;
                default:

                    break;
            }
        }

        // Client로 보낼 메시지를 Pack한다.
        private String PackingMessage(params String[] strings)
        {
            String result = "";

            foreach (String msg in strings)
            {
                result += msg;
                result += '/';
            }

            return result;
        }
    }
}
