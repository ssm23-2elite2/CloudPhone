﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

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
                int receiveLength = 0;

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
                            receiveLength = ns.Read(buffer, 0, buffer.Length);
                            stream.Write(buffer, 0, receiveLength);
                            totalLength += receiveLength;
                        }

                        String receive = Encoding.UTF8.GetString(buffer, 0, totalLength);
                        
                        /*
                         *  receive받은 String으로 Unpacking함 
                         */
                        UnPackingMessage(receive);


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


        // Client로 값 전달
        private void SendData()
        {

        }

        // Client로부터 값 받음
        private void ReceiveData()
        {


        }

        // Client Login Process
        private bool ClientLogin()
        {
            

            return true;
        }

        // Client Logout Process
        private bool ClientLogout()
        {

            return true;
        }

        // Client로부터 온 메시지 Unpack
        private void UnPackingMessage(String str)
        {
            String[] strings = str.Split('/');

            // Unpacking해서 메시지 헤더에 따라서 돌린다.
            // 메시지 구조  :  ClientID / Type / Size / Data
            // ACTION_DOWN, ACTION_MOVE, ACTION_UP, ACTION_POINTER_DOWN,
            // ACTION_POINTER_UP, KEYCODE_HOME, KEYCODE_VOLUME_DOWN, KEYCODE_VOLUME_UP, KEYCODE_POWER, GPS, GYRO, BATTERY


            switch (strings[1])
            {
                case "0": // Login
                    
                    break;

                case "1": // ACTION

                    break;

                case "2": // KEYCODE VALUE

                    break;

                case "3": // GPS VALUE

                    break;

                case "4": // GYRO VALUE

                    break;

                case "5": // BATTERY VALUE

                    break;

                case "6": // KEYCODE_HOME

                    break;

                case "7": // KEYCODE_VOLUME_DOWN

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
