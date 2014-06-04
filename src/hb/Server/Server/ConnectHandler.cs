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
        private CloudPhoneWindow cloudPhoneWindow = null;
        public TcpListener threadListener = null;
        public bool isRunning { get; set; }
        public String ip = null;
        TcpClient client = null;
        public String clientID = null;
        NetworkStream ns = null;

        // TelNet 원격 접속(GPS)을 위한 선언
        private TcpClient cpTcpClient;
        private NetworkStream cpNetStream = null;
        private StreamWriter cpStmWriter = null;

        public ConnectHandler(CloudPhoneWindow c)
        {
            isRunning = true;
            cloudPhoneWindow = c;
            cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "클라이언트 생성, ConnectHandler");

        }

        public void clientHandler()
        {
            //try
            //{
                client = threadListener.AcceptTcpClient();
                ns = client.GetStream();
                System.IO.StreamReader sr = new System.IO.StreamReader(ns);

                while (isRunning)
                {
                   // try
                   // {
                        String receive = null;
                        
                        receive = sr.ReadLine();
                        cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "Receive MSG : " + receive);
                        if (receive != null)
                        {
                            UnPackingMessage(receive);
                        }
                        else
                            isRunning = false;

                    //}
                    //catch (Exception e)
                    //{
                    //    cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "error", "clientHandler with while: " + e.Message);
                    //    isRunning = false;
                    //}
                }

                sr.Close();
                ns.Close();
                client.Close();

                sr = null;
                ns = null;
                client = null;
                

            //}
            //catch (Exception e)
            //{
            //    cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "error", "clientHandler : " + e.Message);
            //    isRunning = false;
            //}
        }

        public void RequestStop()
        {
            isRunning = false;
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

        public void TelnetConnectAndSendMsg(int portNum, String Msg)
        {
            try
            {
                cpTcpClient = new TcpClient("localhost", portNum);
                if (true == cpTcpClient.Connected)
                {
                   // cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "Telnet Msg : " + Msg);
                    cpNetStream = cpTcpClient.GetStream();
                    cpStmWriter = new StreamWriter(cpNetStream);
                    cpStmWriter.WriteLine(Msg);
                    cpStmWriter.Flush();
                }
                else
                {
                    return;
                }
            }
            catch (Exception e)
            {
                cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "error", "Telnet Connect Err : " + e.Message);
            }
            finally
            {
                if (null != cpStmWriter)
                    cpStmWriter.Close();

                if (null != cpNetStream)
                    cpNetStream.Close();

                if (null != cpTcpClient)
                    cpTcpClient.Close();

                cpTcpClient = null;
                cpNetStream = null;
                cpStmWriter = null;

            }
            return; 
        }

        // Client로부터 온 메시지 Unpack
        private void UnPackingMessage(String str)
        {
            if (str == null) return;

            String[] strings = str.Split('/');
            int portNum = 0;
            // Unpacking해서 메시지 헤더에 따라서 돌린다.
            // 메시지 구조  :  Type / clientID / Data ( Data는 '쉼표'로 구분 ',')

            switch (strings[0])
            {
                case "0": // Login 메시지가 들어오면, 있는 ID인지 검사하도록  CloudPhoneWindow에 있는 CheckClientID를 실행시킨다.
                    String[] loginData = strings[2].Split(',');
                    ip = loginData[0];
                    clientID = loginData[1];
                    // cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "Client ip : " + ip + ", Client ID : " + clientID);
                    break;

                case "1": // AVD ACK MSG
                    // cloudPhoneWindow.DecideAVDMsg(strings[2]);
                    portNum = (int)cloudPhoneWindow.Invoke(cloudPhoneWindow._getPortNum, strings[1]);
                    cloudPhoneWindow.Invoke(cloudPhoneWindow._setTime, DateTime.Now, portNum);
                    //SendMsg("ACK");
                    break;

                case "2": // ACTION { ACTIONTYPE : (ACTION_UP, ACTION_DOWN, ACTION_MOVE) / X / Y }
                    String[] ActionData = strings[2].Split(',');
                    //cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "ACTION TYPE : " + ActionData[0] + ", 좌표 : " + ActionData[1] + ", " + ActionData[2]);

                    portNum = (int)cloudPhoneWindow.Invoke(cloudPhoneWindow._getPortNum, strings[1]);

                    if (ActionData[0] == "ACTION_DOWN")
                    {
                        cloudPhoneWindow.Invoke(cloudPhoneWindow._startCMD);
                        cloudPhoneWindow.Invoke(cloudPhoneWindow._controlCMD, "adb -s emulator-" + portNum + " shell sendevent /dev/input/event0 3 0 " + ActionData[1]);
                        cloudPhoneWindow.Invoke(cloudPhoneWindow._controlCMD, "adb -s emulator-" + portNum + " shell sendevent /dev/input/event0 3 1 " + ActionData[2]);
                        cloudPhoneWindow.Invoke(cloudPhoneWindow._controlCMD, "adb -s emulator-" + portNum + " shell sendevent /dev/input/event0 1 330 1");
                        cloudPhoneWindow.Invoke(cloudPhoneWindow._controlCMD, "adb -s emulator-" + portNum + " shell sendevent /dev/input/event0 0 0 0");
                        cloudPhoneWindow.Invoke(cloudPhoneWindow._exitCMD);
                    }
                    else if (ActionData[0] == "ACTION_UP")
                    {
                        cloudPhoneWindow.Invoke(cloudPhoneWindow._startCMD);
                        cloudPhoneWindow.Invoke(cloudPhoneWindow._controlCMD, "adb -s emulator-" + portNum + " shell sendevent /dev/input/event0 1 330 0");
                        cloudPhoneWindow.Invoke(cloudPhoneWindow._controlCMD, "adb -s emulator-" + portNum + " shell sendevent /dev/input/event0 0 0 0");
                        cloudPhoneWindow.Invoke(cloudPhoneWindow._exitCMD);
                    }
                    else if (ActionData[0] == "ACTION_MOVE")
                    {
                        cloudPhoneWindow.Invoke(cloudPhoneWindow._startCMD);
                        cloudPhoneWindow.Invoke(cloudPhoneWindow._controlCMD, "adb -s emulator-" + portNum + " shell sendevent /dev/input/event0 3 0 " + ActionData[1]);
                        cloudPhoneWindow.Invoke(cloudPhoneWindow._controlCMD, "adb -s emulator-" + portNum + " shell sendevent /dev/input/event0 3 1 " + ActionData[2]);
                        cloudPhoneWindow.Invoke(cloudPhoneWindow._controlCMD, "adb -s emulator-" + portNum + " shell sendevent /dev/input/event0 0 0 0");
                        cloudPhoneWindow.Invoke(cloudPhoneWindow._exitCMD);
                    }

                    break;

                case "3": // KEYCODE VALUE
                    String[] KeyData = strings[2].Split(',');
                    //cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "KEY Code : " + KeyData[0]);
                    //adb shell input keyevent <keycode>
                    portNum = (int)cloudPhoneWindow.Invoke(cloudPhoneWindow._getPortNum, strings[1]);
                    cloudPhoneWindow.Invoke(cloudPhoneWindow._startCMD);
                    cloudPhoneWindow.Invoke(cloudPhoneWindow._controlCMD, "adb -s emulator-" + portNum + " shell input keyevent " + KeyData[0]);
                    cloudPhoneWindow.Invoke(cloudPhoneWindow._exitCMD);
                    break;

                case "4": // GPS VALUE, GPSData[0] : 

                    String[] GPSData = strings[2].Split(',');
                    //cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "GPSValue : " + GPSData[0] + ", " + GPSData[1]);
                    portNum = (int)cloudPhoneWindow.Invoke(cloudPhoneWindow._getPortNum, strings[1]);
                    String telnetMsg = "geo fix " + GPSData[0] + " " + GPSData[1];
                    TelnetConnectAndSendMsg(portNum, telnetMsg);
                    break;

                case "5": // GYRO VALUE

                    break;

                case "6": // BATTERY VALUE
                    String[] BattData = strings[2].Split(',');
                    // cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "잔량 : " + BattData[0] + ", 충전상태 : " + BattData[1]);
                    portNum = (int)cloudPhoneWindow.Invoke(cloudPhoneWindow._getPortNum, strings[1]);
                    cloudPhoneWindow.Invoke(cloudPhoneWindow._startCMD);
                    cloudPhoneWindow.Invoke(cloudPhoneWindow._controlCMD, "adb -s emulator-" + portNum + " shell am broadcast -a android.intent.action.BATTERY_CHANGED --ei level " + BattData[0] + " --ei scale 100 --ei status 4 --ei health 3  --ez present true --ei voltage 4196 --ei plugged " + BattData[1]);
                    cloudPhoneWindow.Invoke(cloudPhoneWindow._exitCMD);
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
