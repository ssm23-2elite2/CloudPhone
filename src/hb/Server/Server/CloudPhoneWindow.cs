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

        private TcpListener tcpClient; // 각종 메시지를 받기 위한 tcp 소켓
        private TcpListener tcpBGClient; // BackGround를 보내기 위한 tcp 소켓

        private UdpClient udpClient;
        private IPEndPoint udpSender;
        private Thread serverThread;

        private ConnectHandler con;
        private DBConnect db;

        public delegate void logMSGd(String msgType, String msg);
        public logMSGd _logMSG;
        
        private ArrayList clientThreadList = new ArrayList();
        private ArrayList clientUDPThreadList = new ArrayList();
        private ArrayList clientIDList = new ArrayList();

        

        private Process proc_cmd;
        private ProcessStartInfo startInfo;
        
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

            proc_cmd = new Process();
            startInfo = new ProcessStartInfo();

            startInfo.FileName = "cmd.exe";
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            proc_cmd.EnableRaisingEvents = false;
            proc_cmd.StartInfo = startInfo;

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

        // Tray Icon Menu에서 'Exit'을 눌렀을 때( 진짜 종료 )
        private void TrayMenu_Exit_Click(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
           

            if (serverThread != null && serverThread.IsAlive)
            {
                tcpClient.Stop();
                serverThread.Abort();
                serverThread = null;
            }

            String processName = "Server";
            Process[] processes = Process.GetProcessesByName(processName);

            foreach (Process process in processes)
            {
                process.Kill();
            }
            this.Dispose();
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
                tcpClient = new TcpListener(ipAddr, 20000);
                tcpClient.Start();
                logMSG("info", "TCP 클라이언트 대기중... CloudPhoneTestServer.ListenerThread");

                tcpBGClient = new TcpListener(ipAddr, 20001);
                tcpBGClient.Start();
                logMSG("info", "TCP BG 클라이언트 대기중... CloudPhoneTestServer.ListenerThread");

                //IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 20001);
                //udpClient = new UdpClient(ipep);
                //udpSender = new IPEndPoint(ipAddr, 20001);
                
                while (true)
                {
                    // 클라이언트의 연결 요청 확인
                    while (!tcpClient.Pending())
                    {
                        Thread.Sleep(100);
                    }
                    logMSG("info", "클라이언트의 요청 발견... CloudPhoneTestServer.ListenerThread");
                    con = new ConnectHandler(this);
                    db = new DBConnect(this);
                    logMSG("info", "TCP 클라이언트 통신용 쓰레드 생성 CloudPhoneTestServer new ConnectionHandler");
                    con.threadListener = this.tcpClient;
                    Thread newThread = new Thread(new ThreadStart(con.clientHandler));
                    newThread.Start();
                    clientThreadList.Add(newThread);

                    //// TCP로 메시지를 받았으면 바로 BackGroundThread 실행(UDP)
                    

                    //String ip = con.ip;
                    //logMSG("info", "받은 IP : " + ip);

                    //CheckBackGroundThread cbg = new CheckBackGroundThread(this, udpClient, udpSender, ip);
                    //Thread udpThread = new Thread(new ThreadStart(cbg.udpCheckbg));
                    
                    //logMSG("info", "UDP 클라이언트 통신용 쓰레드 생성 CloudPhoneTestServer new ConnectionHandler");
                    //udpThread.Start();
                    //clientUDPThreadList.Add(udpThread);
                    //Thread.Sleep(300);







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
                tcpClient.Stop();

                udpClient.Close();
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
             *  센서 값 검사하는(받아서 쏘아주는) 쓰레드 시작
             */

        }

        // Client Login Process
        public bool ClientLogin(String clientID)
        {
            logMSG("info", "CloudPhoneWindow - ClientLogin");
            clientIDList.Add(clientID);
            return  db.CheckClientID(clientID);
        }

        // Client Logout Process
        public void ClientLogout(String clientID)
        {
            logMSG("info", "CloudPhoneWindow - ClientLogout");
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
            clientUDPThreadList.RemoveAt(index);
            clientIDList.RemoveAt(index);
        }

        // 검사 쓰레드 종료
        public void EndThreading()
        {


        }

        // AVD Method ,  avdmsg[] = { { clientNum } , {executeMSG } , {intData or strData} } -> strData는 좀 더 정확하게 수정요망
        public void DecideAVDMsg(String AVDMsg)
        {
            String[] avdmsg = AVDMsg.Split(',');
            int toNum = Convert.ToInt32(avdmsg[0]);

            if (avdmsg[1] == "start")
            {
                StartAVD(toNum);
            }
            
            else if (avdmsg[1] == "exit")
            {
                ExitAVD(toNum);
            }

            else if (avdmsg[1] == "create")
            {
                CreateAVD(toNum, avdmsg[2]);
            }

            else if (avdmsg[1] == "remove")
            {
                RemoveAVD(toNum, avdmsg[2]);
            }
        }

        // AVD 시작
        public void StartAVD(int ClientNum)
        {
            String cmd_string = @"emulator -avd my_android -ramdisk ""D:\Program Files\Java\adt\adt-bundle-windows-x86_64-20140321\sdk\img\ramdisk.img\"" -system ""D:\Program Files\Java\adt\adt-bundle-windows-x86_64-20140321\sdk\img\system.img"" -initdata ""D:\Program Files\Java\adt\adt-bundle-windows-x86_64-20140321\sdk\img\userdata.img"" -sdcard ""D:\Program Files\Java\adt\adt-bundle-windows-x86_64-20140321\sdk\img\sdcard.iso""-no-window";
            logMSG("info", cmd_string);
            ControlCMD(cmd_string);
        }

        // AVD 종료
        public void ExitAVD(int ClientNum) 
        {
            String cmd_string = "";

            ControlCMD(cmd_string);
        }

        // AVD 생성
        public void CreateAVD(int ClientNum, String Version) 
        {
            String cmd_string = "";
            
            ControlCMD(cmd_string);
        }

        // AVD 삭제
        public void RemoveAVD(int ClientNum, String Version) 
        {
            String cmd_string = "";

            ControlCMD(cmd_string);

        }

        // ADB를 통해 AVD로 명령어 전달(센서값) , str -> SensorValue
        public void SensorValueToAVD(String SensorType, String str)
        {
            String cmd_string = "";

            ControlCMD(cmd_string);

        }


        public void ControlCMD(String cmd_Msg)
        {
            String ret, ret_buf;

            try
            {
                // CMD에 입력
                proc_cmd.Start();
                proc_cmd.StandardInput.Write(cmd_Msg + Environment.NewLine);
                proc_cmd.StandardInput.Close();

                ret = proc_cmd.StandardOutput.ReadToEnd();
                ret_buf = ret.Substring(ret.IndexOf(cmd_Msg) + cmd_Msg.Length);
                logMSG("info", "ControlCMD Return String : " + ret_buf);
            }

            catch (Exception e)
            {
                logMSG("error", "ControlCMD Error : " + e.Message);
            }

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