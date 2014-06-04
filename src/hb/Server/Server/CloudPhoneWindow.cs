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

//using MySql.Data;
//using MySql.Data.MySqlClient;

namespace Server
{

    public partial class CloudPhoneWindow : Form
    {

        public ClientData clientData = null;
        private bool isConnected;
        private Dictionary<String, int> Dict_ClientID;
        private Dictionary<int, ClientData> Dict_Client;
        public Thread tcpThread = null;
        public Thread udpThread = null;
        public Thread ACKThread = null;
        private Thread serverThread = null;
        private int tcpPortNum = 20000;
        private int udpPortNum = 8000;
        private int avdPortNum = 5554;
        private TcpListener tcpClient = null; // 각종 메시지를 받기 위한 tcp 소켓
        private bool isRunning; // 써버 쓰레드
        private Socket udpSock = null;

        private ConnectHandler con = null;
        //private DBConnect db = null;

        // 돌릴 쓰레드 선언
        public CheckBackGroundThread checkBG = null;
        public CheckACKThread checkACK = null;

        public delegate void d_logMSG(String msgType, String msg);
        public delegate void d_startCMD();
        public delegate void d_controlCMD(String cmd_string);
        public delegate void d_exitCMD();
        public delegate int d_getPortNum(String ClientID);
        public delegate DateTime d_getTime(int portNum);
        public delegate void d_setTime(DateTime t, int portNum);
        public delegate void d_exitClient(int portNum);

        public d_exitClient _exitClient;
        public d_setTime _setTime;
        public d_getTime _getTime;
        public d_getPortNum _getPortNum;
        public d_logMSG _logMSG;
        public d_controlCMD _controlCMD;
        public d_startCMD _startCMD;
        public d_exitCMD _exitCMD;

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
            isRunning = false;
            this.Log_List.Items.Add(new ListBoxItem(Color.Black, "Hello, Welcome to the Cloud Phone Server! -- " + date));
            CheckForIllegalCrossThreadCalls = false;

            isConnected = false;
            notifyIcon.ContextMenuStrip = trayMenuStrip;



            _logMSG = new d_logMSG(logMSG);
            _controlCMD = new d_controlCMD(ControlCMD);
            _startCMD = new d_startCMD(startCMD);
            _exitCMD = new d_exitCMD(exitCMD);
            _getPortNum = new d_getPortNum(getPortNum);
            _getTime = new d_getTime(getTime);
            _setTime = new d_setTime(setTime);
            _exitClient = new d_exitClient(exitClient);
            
            proc_cmd = new Process();
            startInfo = new ProcessStartInfo();
            Dict_ClientID = new Dictionary<String, int>();
            startInfo.FileName = "cmd.exe";
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.CreateNoWindow = true;

            proc_cmd.EnableRaisingEvents = false;
            proc_cmd.StartInfo = startInfo;
        }

        // 종료시 처리 ( X를 눌렀을 때 최소화만 시킴)
        private void CloudPhoneWindow_FormClosing(object sender, FormClosingEventArgs e)
        {

            notifyIcon.Visible = false;
            isConnected = false;

            if (serverThread != null && serverThread.IsAlive)
            {
                if (Dict_Client.Count > 0)
                {
                    foreach (KeyValuePair<int, ClientData> data in Dict_Client)
                    {
                        // allow the worker thread to do some work:
                        Thread.Sleep(1);
                        // Request that the worker thread stop itself:
                        data.Value._UdpThreadClass.RequestStop();
                        // Use the Join method to block the current thread 
                        // until the object's thread terminates.
                        data.Value.UDPThread.Abort();

                        Thread.Sleep(1);
                        data.Value._AckThreadClass.RequestStop();
                        data.Value.ACKThread.Abort();
                    }
                }

                Thread.Sleep(1);
                isRunning = false;
                serverThread.Abort(); // 써버 쓰레드 종료
                serverThread = null;
                logMSG("info", "Exit");
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
            isConnected = false;

            if (serverThread != null && serverThread.IsAlive)
            {
                if (Dict_Client.Count > 0)
                {
                    foreach (KeyValuePair<int, ClientData> data in Dict_Client)
                    {
                        // allow the worker thread to do some work:
                        Thread.Sleep(1);
                        // Request that the worker thread stop itself:
                        data.Value._UdpThreadClass.RequestStop();
                        // Use the Join method to block the current thread 
                        // until the object's thread terminates.
                        data.Value.UDPThread.Abort();

                        Thread.Sleep(1);
                        data.Value._AckThreadClass.RequestStop();
                        data.Value.ACKThread.Abort();
                    }
                }

                Thread.Sleep(1);
                isRunning = false;
                serverThread.Abort(); // 써버 쓰레드 종료
                serverThread = null;
                logMSG("info", "Exit");
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
            //  logMSG("info", "서버 시작. btn_start_Click");

            if (isConnected)
            {
                logMSG("warn", "서버가 이미 실행중입니다.");
            }
            else
            {
                isConnected = true;
                //   logMSG("info", "서버 쓰레드 생성, btn_start_Click -> CreateThread");
                serverThread = new Thread(new ThreadStart(ListenerThread));
                serverThread.Start();
            }
        }

        public void ListenerThread()
        {
            try
            {
                IPHostEntry ipHost = Dns.Resolve("192.168.0.8");
                IPAddress ipAddr = ipHost.AddressList[0];
                tcpClient = new TcpListener(ipAddr, tcpPortNum);
                udpSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                tcpClient.Start();
                isRunning = true;

               // db = new DBConnect(this);
                con = new ConnectHandler(this);
                checkACK = new CheckACKThread(this, avdPortNum);

                Dict_Client = new Dictionary<int, ClientData>();
                // 클라이언트의 연결 요청 확인
                logMSG("info", "TCP 클라이언트 대기중... CloudPhoneTestServer.ListenerThread");

                while (isRunning)
                {
                    clientData = new ClientData();
                    clientData.AVDPortNum = avdPortNum;

                    while (!tcpClient.Pending())
                    {
                        if (isRunning == false)
                        {
                            tcpClient.Server.Close(0);
                            tcpClient.Stop();
                            tcpClient = null;
                        }
                        Thread.Sleep(100);
                    }

                    //logMSG("info", "클라이언트의 요청 발견... CloudPhoneTestServer.ListenerThread");

                    // logMSG("info", "TCP 클라이언트 통신용 쓰레드 생성 CloudPhoneTestServer new ConnectionHandler");
                    con.threadListener = this.tcpClient;

                    tcpThread = new Thread(new ThreadStart(con.clientHandler));
                    tcpThread.Start();
                    logMSG("info", "tcpThread 시작");

                    Thread.Sleep(2000);

                    // TCP로 메시지를 받았으면 바로 BackGroundThread 실행(UDP)
                    //logMSG("info", "UDP Thread 실행 준비");

                    setClientInfo(con.ip, con.clientID, clientData);

                    // DB 에 클라이언트 ID가 있는 지 체크하고 등록한다.
                    //db.CheckClientID(clientData.ClientID);
                    Dict_ClientID.Add(clientData.ClientID, avdPortNum);

                    ACKThread = new Thread(new ThreadStart(checkACK.SendAckToClient));
                    

                    clientData._TcpThreadClass = con;
                    clientData.TCPThread = tcpThread;
                    clientData._AckThreadClass = checkACK;
                    clientData.ACKThread = ACKThread;
                    Dict_Client.Add(avdPortNum , clientData);
                    ACKThread.Start();
                    logMSG("info", "ACKThread 시작");
                    StartAVD(avdPortNum);

                    IPHostEntry _ipHost = Dns.Resolve(clientData.ClientIP);
                    IPAddress _ipAddr = _ipHost.AddressList[0];
                    IPEndPoint ipep = new IPEndPoint(_ipAddr, udpPortNum);

                    checkBG = new CheckBackGroundThread(this, udpSock, ipep);
                    udpThread = new Thread(new ThreadStart(checkBG.udpCheckbg));
                    //logMSG("info", "UDP 클라이언트 통신용 쓰레드 생성... CheckBackGroundThread 실행");
                    udpThread.Start();
                    logMSG("info", "udpThread 시작");

                    // ClientData Set
                    Dict_Client[avdPortNum]._UdpThreadClass = checkBG;
                    Dict_Client[avdPortNum].UDPThread = udpThread;
                    
                    Thread.Sleep(300);
                }

            }
            catch (Exception e)
            {
                logMSG("error", "ListenerThread : " + e.Message);
                isRunning = false;
                isConnected = false;
            }
            finally // TCP, UDP 종료
            {
                if (tcpClient != null)
                {
                    try
                    {
                       // tcpClient.Server.Shutdown(SocketShutdown.Both);
                    }
                    catch (Exception e)
                    {
                        logMSG("error", "tcpClient Shutdown Err : " + e.Message);
                    }
                    finally
                    {
                        tcpClient.Server.Close(0);
                        tcpClient.Stop();
                        
                        tcpClient = null;
                    }
                }

                if (udpSock != null)
                {
                   // udpSock.Shutdown(SocketShutdown.Both);
                    udpSock.Close();
                    udpSock = null;
                }
            }
        }

        public void setClientInfo(String ip, String clientID, ClientData d)
        {
            d.ClientID = clientID;
            d.ClientIP = ip;
        }

        private void btn_stop_Click(object sender, EventArgs e)
        {
            /* 쓰레드 끝 */
            isConnected = false;

            if (serverThread != null && serverThread.IsAlive)
            {
                if (Dict_Client.Count > 0)
                {

                    foreach (KeyValuePair<int, ClientData> data in Dict_Client)
                    {
                        Thread.Sleep(1);
                        data.Value._UdpThreadClass.RequestStop();
                        data.Value.UDPThread.Abort();

                        Thread.Sleep(1);
                        data.Value._AckThreadClass.RequestStop();
                        data.Value.ACKThread.Abort();

                    }
                }

                Thread.Sleep(1);
                isRunning = false;
                serverThread.Abort();
                //serverThread.Join(); // 써버 쓰레드 종료
                serverThread = null;

                logMSG("info", "서버 OFF, CloudPhoneWindow.btn_stop_Click");

            }
            else
            {
                logMSG("warn", "서버가 실행되어 있지 않습니다");
            }
        }

        public DateTime getTime(int portNum)
        {
            return Dict_Client[portNum]._AckThreadClass.getTime();
        }

        public void setTime(DateTime t, int portNum)
        {
            Dict_Client[portNum]._AckThreadClass.setTime(t);
        }

        public int getPortNum(String ClientID)
        {
            return Dict_ClientID[ClientID];
        }

        // AVD 시작
        public void StartAVD(int ClientNum)
        {
            String cmd_string = @"start emulator -avd client_dev -sdcard ""C:\sdcard.iso"" -port " + ClientNum + " -gpu off";
            // String cmd_string = @"emulator -avd my_android";
            logMSG("info", "StartAVD");
            startCMD();
            ControlCMD(cmd_string);
            exitCMD();
            Thread.Sleep(27000);
            String chmod = @"adb shell chmod 777 ""/dev/graphics/fb0"""; // frame buffer 권한주기
            logMSG("info", chmod);
            startCMD();
            ControlCMD(chmod);
            exitCMD();
        }

        public void startCMD()
        {
            proc_cmd.Start();
        }

        public void exitClient(int portNum)
        {
            Thread.Sleep(1);
            Dict_Client[portNum]._AckThreadClass.RequestStop();
            Dict_Client[portNum].ACKThread.Abort();

            Thread.Sleep(1);
            Dict_Client[portNum]._UdpThreadClass.RequestStop();
            Dict_Client[portNum].UDPThread.Abort();

            Dict_ClientID.Remove(Dict_Client[portNum].ClientID);
            Dict_Client.Remove(portNum);
            

        }

        public void ControlCMD(String cmd_Msg)
        {
            try
            {
                //logMSG("info", "ControlCMD : " + cmd_Msg);
                // CMD에 입력

                proc_cmd.StandardInput.Write(cmd_Msg + Environment.NewLine);

            }

            catch (Exception e)
            {
                logMSG("error", "ControlCMD Error : " + e.Message);
            }
        }

        public void exitCMD()
        {
            proc_cmd.StandardInput.Close();
            proc_cmd.WaitForExit();
            proc_cmd.Close();
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

            if (msgType == "info")
                Log_List.Items.Add(new ListBoxItem(Color.Green, "Info : " + date + msg));
            else if (msgType == "warn")
                Log_List.Items.Add(new ListBoxItem(Color.DarkOrange, "Warning : " + date + msg));
            else if (msgType == "error")
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

    public class ClientData
    {
        private Thread tcpThread;
        private Thread udpThread;
        private Thread ackThread;
        private CheckACKThread _ackThreadClass;
        private CheckBackGroundThread _udpThreadClass;
        private ConnectHandler _tcpThreadClass;
        private int avdPortNum;
        private String clientID;
        private String clientIP;

        public Thread TCPThread
        {
            get { return tcpThread; }
            set { tcpThread = value; }
        }
        public Thread UDPThread
        {
            get { return udpThread; }
            set { udpThread = value; }
        }
        public Thread ACKThread
        {
            get { return ackThread; }
            set { ackThread = value; }
        }
        public CheckACKThread _AckThreadClass
        {
            get { return _ackThreadClass; }
            set { _ackThreadClass = value; }
        }
        public CheckBackGroundThread _UdpThreadClass
        {
            get { return _udpThreadClass; }
            set { _udpThreadClass = value; }
        }
        public ConnectHandler _TcpThreadClass
        {
            get { return _tcpThreadClass; }
            set { _tcpThreadClass = value; }
        }
        public int AVDPortNum
        {
            get { return avdPortNum; }
            set { avdPortNum = value; }
        }
        public String ClientID
        {
            get { return clientID; }
            set { clientID = value; }
        }
        public String ClientIP
        {
            get { return clientIP; }
            set { clientIP = value; }
        }

        public ClientData()
        {
            tcpThread = null;
            udpThread = null;
            ackThread = null;
            _ackThreadClass = null;
            _udpThreadClass = null;
            _tcpThreadClass = null;
            avdPortNum = 0;
            clientID = null;
            clientIP = null;
        }

    }
}