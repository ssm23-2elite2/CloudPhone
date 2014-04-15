﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Collections;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Server
{

    public partial class CloudPhoneWindow : Form
    {

        private bool isConnected;
        private TcpListener client;
        private Thread serverThread;

        public delegate void logMSGd(String msgType, String msg);
        public logMSGd _logMSG;


        private ArrayList clientList = new ArrayList();


        public CloudPhoneWindow()
        {
            InitializeComponent();

        }

        enum MSG
        {
            ACTION_DOWN, ACTION_MOVE, ACTION_UP, ACTION_POINTER_DOWN,
            ACTION_POINTER_UP, KEYCODE_HOME, KEYCODE_VOLUME_DOWN, KEYCODE_VOLUME_UP, KEYCODE_POWER, GPS, GYRO, BATTERY
        }

        // 초기화
        private void CloudPhoneWindow_Load(object sender, EventArgs e)
        {
            String date = System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss ");
            this.Log_List.DrawMode = DrawMode.OwnerDrawFixed;

            this.Log_List.Items.Add(new ListBoxItem(Color.Black, "Hello, Welcome to the Cloud Phone Server! -- " + date));
            CheckForIllegalCrossThreadCalls = false;

            isConnected = false;
            notifyIcon.ContextMenuStrip = trayMenuStrip;

            _logMSG = new logMSGd(logMSG);
        }


        // 종료시 처리 ( X를 눌렀을 때 최소화만 시킴)
        private void CloudPhoneWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Visible = false;
        }

        // Tray Icon Menu에서 'About...'을 눌렀을 때
        private void TrayMenu_About_Click(object sender, EventArgs e)
        {

        }

        // Tray Icon DoubleClick 했을 때
        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            this.Visible = true;
        }

        // Tray Icon Menu에서 'About...'을 눌렀을 때( 진짜 종료 )
        private void TrayMenu_Exit_Click(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            this.Dispose();

            if (serverThread != null && serverThread.IsAlive)
            {
                client.Stop();
                serverThread.Abort();
                serverThread = null;
            }

            String processName = "CloudPhoneWindow";
            Process[] processes = Process.GetProcessesByName(processName);

            foreach (Process process in processes)
            {
                process.Kill();
            }

            Application.Exit();
        }


        private void CloudPhoneWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
               
        }


        private void btn_start_Click(object sender, EventArgs e)
        {
            /* 멀티 쓰레딩 시작 */
            logMSG("info", "서버 시작. btn_start_Click");

            if (isConnected)
            {
                logMSG("warn", "서버가 이미 실행중입니다.");
            }
            else
            {
                isConnected = true;
                logMSG("info", "서버 쓰레드 생성, btn_start_Click -> CreateThread");
                serverThread = new Thread(new ThreadStart(ListenerThread));
                serverThread.Start();
            }
        }

        public void ListenerThread()
        {

            try
            {
                IPHostEntry ipHost = Dns.Resolve("211.189.20.131");
                IPAddress ipAddr = ipHost.AddressList[0];
                client = new TcpListener(ipAddr, 20000);
                client.Start();
                logMSG("info", "클라이언트 대기중... CloudPhoneTestServer.ListenerThread");

                while (true)
                {
                    // 클라이언트의 연결 요청 확인
                    while (!client.Pending())
                    {
                        Thread.Sleep(100);
                    }
                    logMSG("클라이언트의 요청 발견... CloudPhoneTestServer.ListenerThread", "info");
                    ConnectHandler con = new ConnectHandler(this);

                    logMSG("클라이언트 통신용 쓰레드 생성 CloudPhoneTestServer new ConnectionHandler", "info");
                    
                    con.threadListener = this.client;
                    Thread newThread = new Thread(new ThreadStart(con.clientHandler));
                    newThread.Start();
                    clientList.Add(newThread);
                    StartThreading();
                }
            }
            catch (Exception e)
            {
                logMSG("error", "ListenerThread : " + e.Message);
            }
        }

        private void btn_stop_Click(object sender, EventArgs e)
        {
            /* 쓰레드 끝 */

            if (serverThread != null && serverThread.IsAlive)
            {
                isConnected = false;
                client.Stop();
                serverThread.Abort();
                serverThread = null;
                logMSG("info", "서버 OFF, CloudPhoneWindow.btn_stop_Click");
                //EndThreading();
            }
            else
            {
                logMSG("warn", "서버가 실행되어 있지 않습니다");
            }
        }


        // 각종 검사하는 쓰레드 시작
        private void StartThreading()
        {

            /*
             *  쓰레드 시작
             *  디비랑 연동되어 정보가 디비에 없으면 새로 추가시킨다(등록).
             *  디비에 있는 정보에 해당하는 AVD가 없으면 실행해준다.
             *  Client에 화면 전송해주는 쓰레드 시작
             *  센서 값 검사하는(받아서 쏘아주는) 쓰레드 시작
             */

        }

        // 검사 쓰레드 종료
        private void EndThreading()
        {


        }

        // AVD 시작
        private void StartAVD(int ClientNum)
        {

        }

        // AVD 종료
        private void ExitAVD(int ExitCode) 
        {


        }

        // AVD 생성
        private void CreateAVD(int ClientNum, int Version) 
        {


        }

        // AVD 삭제
        private void RemoveAVD(int ClientNum, int Version) 
        {


        }

        // ADB를 통해 AVD로 명령어 전달(센서값)
        private void CmdToAVD(String SensorType, String str) 
        {


        }

        // Client가 등록되었는지 확인해주는 함수
        private bool isRegistration(int clientNum)
        {


            return true;

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

        // Client로 값 전달
        private void SendData() 
        {


        }

        // Client로부터 값 받음
        private void ReceiveData()
        {


        }

        // Client로부터 온 메시지 Unpack
        private void UnPackingMessage(String str) 
        {
            String[] strings = str.Split('/');

            // Unpacking해서 메시지 헤더에 따라서 돌린다.

            //ACTION_DOWN, ACTION_MOVE, ACTION_UP, ACTION_POINTER_DOWN,
            //ACTION_POINTER_UP, KEYCODE_HOME, KEYCODE_VOLUME_DOWN, KEYCODE_VOLUME_UP, KEYCODE_POWER, GPS, GYRO, BATTERY

            switch (strings[1])
            {
                case "0": // ACTION_DOWN

                    break;

                case "1": // ACTION_MOVE

                    break;

                case "2": // ACTION_UP

                    break;

                case "3": // ACTION_POINTER_DOWN

                    break;

                case "4": // ACTION_POINTER_UP

                    break;

                case "5": // KEYCODE_HOME

                    break;

                case "6": // KEYCODE_VOLUME_DOWN

                    break;

                case "7": // KEYCODE_VOLUME_UP

                    break;

                case "8": // KEYCODE_POWER

                    break;

                case "9": // GPS

                    break;

                case "10": // GYRO

                    break;

                case "11": // BATTERY

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

        // LogEnd
        private void logEnd()
        {
            Log_List.TopIndex = Math.Max(Log_List.Items.Count - Log_List.ClientSize.Height / Log_List.ItemHeight + 1, 0);
            Log_List.SelectedIndex = Log_List.Items.Count - 1;
        }

        // LogMessage 출력해준다.
        private void logMSG(String msgType, String msg)
        {
            String date = System.DateTime.Now.ToString("MM-dd hh:mm:ss ");
            
            if(msgType == "info")
                Log_List.Items.Add(new ListBoxItem(Color.Green, "Info : " + date + msg));
            else if(msgType == "warn")
                Log_List.Items.Add(new ListBoxItem(Color.DarkOrange, "Warning : " + date + msg));
            else if(msgType == "error")
                Log_List.Items.Add(new ListBoxItem(Color.Red, "Error : " + date + msg));

            logEnd();

        }

        // listLog의 Log 색상을 다르게 하기 위해 오버라이딩
        private void Log_List_DrawItem(object sender, DrawItemEventArgs e)
        {
            bool isItemSelected = ((e.State & DrawItemState.Selected) == DrawItemState.Selected);
            ListBoxItem item = Log_List.Items[e.Index] as ListBoxItem; // Get the current item and cast it to MyListBoxItem
            if (item != null)
            {
                // Background Color
                SolidBrush backgroundColorBrush = new SolidBrush((isItemSelected) ? item.ItemColor : Color.White);
                e.Graphics.FillRectangle(backgroundColorBrush, e.Bounds);

                SolidBrush itemTextColorBrush = (isItemSelected) ? new SolidBrush(Color.White) : new SolidBrush(item.ItemColor);
                e.Graphics.DrawString(item.Message, e.Font, itemTextColorBrush, Log_List.GetItemRectangle(e.Index).Location);
            }
        }
    }

    public class ListBoxItem
    {
        public ListBoxItem(Color c, String str)
        {
            ItemColor = c;
            Message = str;
        }

        public Color ItemColor { get; set; }
        public String Message { get; set; }
    }
}