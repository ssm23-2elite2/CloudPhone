using System;
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


using MySql.Data;
using MySql.Data.MySqlClient;

namespace Server
{

    public partial class CloudPhoneWindow : Form
    {

        private bool isConnected;
        private TcpListener client;
        private Thread serverThread;
        private ConnectHandler con;
        private DBConnect db;

        public delegate void logMSGd(String msgType, String msg);
        public logMSGd _logMSG;
        
        private ArrayList clientThreadList = new ArrayList();
        private ArrayList clientIDList = new ArrayList();


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
                    logMSG("info", "클라이언트의 요청 발견... CloudPhoneTestServer.ListenerThread");
                    con = new ConnectHandler(this);
                    db = new DBConnect(this);
                    logMSG("info", "클라이언트 통신용 쓰레드 생성 CloudPhoneTestServer new ConnectionHandler");
                    
                    con.threadListener = this.client;
                    Thread newThread = new Thread(new ThreadStart(con.clientHandler));
                    newThread.Start();
                    clientThreadList.Add(newThread);
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
                
                EndThreading(); // 검사하던 쓰레드들도 다 종료
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
             *  Client에 화면 전송해주는 쓰레드 시작
             *  센서 값 검사하는(받아서 쏘아주는) 쓰레드 시작
             */



        }

        // Client Login Process
        public bool ClientLogin(String clientID)
        {
            clientIDList.Add(clientID);
            return  db.CheckClientID(clientID);
        }

        // Client Logout Process
        public void ClientLogout(String clientID)
        {
            int index = clientIDList.IndexOf(clientID);

            // index를 이용하여 Thread를 받아서 쓰레드 종료시키고, 그에 관련된 쓰레드들도 다 종료시킨다.(센서값)
            IEnumerator e = clientThreadList.GetEnumerator();

            for (int i = 0; i < index; i++)
            {
                e.MoveNext();
            }

            Thread obj = (Thread)e.Current;
            obj.Abort();
            clientThreadList.RemoveAt(index);
            clientIDList.RemoveAt(index);

        }

        // 검사 쓰레드 종료
        public void EndThreading()
        {


        }

        // AVD 시작
        public void StartAVD(int ClientNum)
        {

        }

        // AVD 종료
        public void ExitAVD(int ExitCode) 
        {


        }

        // AVD 생성
        public void CreateAVD(int ClientNum, int Version) 
        {


        }

        // AVD 삭제
        public void RemoveAVD(int ClientNum, int Version) 
        {


        }

        // ADB를 통해 AVD로 명령어 전달(센서값)
        public void CmdToAVD(String SensorType, String str) 
        {


        }

        // Client가 등록되었는지 확인해주는 함수
        public bool isRegistration(int clientNum)
        {


            return true;

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