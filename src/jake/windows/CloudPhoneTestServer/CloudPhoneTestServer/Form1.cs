using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CloudPhoneTestServer
{
    public partial class CloudPhoneForm : Form
    {
        private Boolean isConnected;
        private TcpListener client;
        private Thread serverThread;

        public CloudPhoneForm()
        {
            InitializeComponent();
            this.listLog.DrawMode = DrawMode.OwnerDrawFixed;
          
            isConnected = false;
            statusBar.Text = "Hello, Jake";
        }

        private void btn_serverOn_Click(object sender, EventArgs e)
        {
            logi("서버를 켭니다. CloudPhoneTestServer.btn_serverOn_Click");
            if (isConnected)
            {
                logw("서버가 이미 실행되어있습니다.");
            }
            else
            {
                isConnected = true;
                logi("서버 쓰레드 생성 CloudPhoneTestServer new Thread");
                serverThread = new Thread(new ThreadStart(ListenerThread));
                serverThread.Start();
            }            
        }

        public void ListenerThread()
        {
            try
            {
                IPHostEntry ipHost = Dns.Resolve("localhost");
                IPAddress ipAddr = ipHost.AddressList[0];
                client = new TcpListener(ipAddr, 3737);
                client.Start();
                logi("클라이언트 대기중... CloudPhoneTestServer.ListenerThread");

                while (true)
                {
                    // 클라이언트의 연결 요청 확인
                    while (!client.Pending())
                    {
                        Thread.Sleep(100);
                    }
                    logi("클라이언트의 요청 발견!... CloudPhoneTestServer.ListenerThread");
                    ConnectionHandler conn = new ConnectionHandler();

                    logi("클라이언트 통신용 쓰레드 생성 CloudPhoneTestServer new ConnectionHandler");
                    conn.threadListener = this.client;
                    Thread newThread = new Thread(new ThreadStart(conn.clientHandler));
                    newThread.Start();
                }
            }
            catch (Exception e)
            {
                loge(e.Message);
            }
        }

        private void btn_serverOff_Click(object sender, EventArgs e)
        {
            if (serverThread != null && serverThread.IsAlive)
            {
                isConnected = false;
                client.Stop();
                serverThread.Abort();
                serverThread = null;
                logi("서버를 끕니다. CloudPhoneTestServer.btn_serverOff_Click");
            }
            else
            {
                logw("서버가 실행되어있지 않습니다.");
            }
        }


        private void logend()
        {
            listLog.TopIndex = Math.Max(listLog.Items.Count - listLog.ClientSize.Height / listLog.ItemHeight + 1, 0);
            listLog.SelectedIndex = listLog.Items.Count - 1;
        }

        private void logi(String msg)
        {
            String date = System.DateTime.Now.ToString("MM-dd hh:mm:ss ");
            listLog.Items.Add(new ListBoxItem(Color.Green, "정보 : " + date + msg));
            statusBar.Text = "정보 : " + date + msg; 
            logend();
        }

        private void logw(String msg)
        {
            String date = System.DateTime.Now.ToString("MM-dd hh:mm:ss ");
            listLog.Items.Add(new ListBoxItem(Color.Orange, "경고 : " + date + msg));
            statusBar.Text = "경고 : " + date + msg;
            logend();
        }

        private void loge(String msg)
        {
            String date = System.DateTime.Now.ToString("MM-dd hh:mm:ss ");
            listLog.Items.Add(new ListBoxItem(Color.Red, "에러 : " + date + msg));
            statusBar.Text = "에러 : " + date + msg; 
            logend();
        }

        private void listLog_DrawItem(object sender, DrawItemEventArgs e)
        {
            bool isItemSelected = ((e.State & DrawItemState.Selected) == DrawItemState.Selected);
            ListBoxItem item = listLog.Items[e.Index] as ListBoxItem; // Get the current item and cast it to MyListBoxItem
            if (item != null)
            {
                // Background Color
                SolidBrush backgroundColorBrush = new SolidBrush((isItemSelected) ? item.ItemColor : Color.White);
                e.Graphics.FillRectangle(backgroundColorBrush, e.Bounds);

                SolidBrush itemTextColorBrush = (isItemSelected) ? new SolidBrush(Color.White) : new SolidBrush(item.ItemColor);
                e.Graphics.DrawString(item.Message, e.Font, itemTextColorBrush, listLog.GetItemRectangle(e.Index).Location);
            }
        }

        private void CloudPhoneForm_Load(object sender, EventArgs e)
        {

        }

        private void CloudPhoneForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (serverThread != null && serverThread.IsAlive)
            {
                client.Stop();
                serverThread.Abort();
                serverThread = null;
            }
        }
    }

    public class ListBoxItem
    {
        public ListBoxItem(Color c, string m)
        {
            ItemColor = c;
            Message = m;
        }
        public Color ItemColor { get; set; }
        public string Message { get; set; }
    }
}
