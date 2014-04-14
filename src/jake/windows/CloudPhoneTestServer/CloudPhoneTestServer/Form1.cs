﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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

        public delegate void showImage(Image image, byte bOrientation);
        public showImage showImageDelegate;

        public delegate void logii(String msg);
        public logii _logi;
        public delegate void logww(String msg);
        public logww _logw;
        public delegate void logee(String msg);
        public logee _loge;

        private ArrayList clientList = new ArrayList();

        public CloudPhoneForm()
        {
            InitializeComponent();
        }

        // 초기화
        private void CloudPhoneForm_Load(object sender, EventArgs e)
        {
            this.listLog.DrawMode = DrawMode.OwnerDrawFixed;
            CheckForIllegalCrossThreadCalls = false;

            isConnected = false;
            statusBar.Text = "Hello, Jake";
            showImageDelegate = new showImage(showImageMethod);

            _logi = new logii(logi);
            _logw = new logww(logw);
            _loge = new logee(loge);

            isConnected = true;
            logi("서버 쓰레드 생성 CloudPhoneTestServer new Thread");
            serverThread = new Thread(new ThreadStart(ListenerThread));
            serverThread.Start();
        }

        // 종료시 처리
        private void CloudPhoneForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (serverThread != null && serverThread.IsAlive)
            {
                client.Stop();
                serverThread.Abort();
                serverThread = null;
            }

            string processName = "CloudPhoneTestServer"; // .exe 는 빼셔야 되여~
            Process[] processes = Process.GetProcessesByName(processName);

            foreach (Process process in processes)
            {
                process.Kill();
            }
        }

        // 클라이언트의 요청을 받아서 이미지를 뿌릴 때 수행되는 함수
        public void showImageMethod(Image image, byte bOrientation)
        {
            logi("이미지를 보여줍니다. CloudPhoneTestServer.showImageMethod");
            switch (bOrientation)
            {
                case Orientation.VERTICAL:
                    image = ImageUtil.RotateImage(image, 90);
                    break;
                case Orientation.HORIZONTAL:
                    break;
            }
            camViewer.Image = image;
            camViewer.Invalidate();
            camViewer.Refresh();
        }

        // On 버튼이 눌리면
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

        // 클라이언트의 연결을 기다리는 Thread
        public void ListenerThread()
        {
            try
            {
                IPHostEntry ipHost = Dns.Resolve("211.189.20.137");
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
                    ConnectionHandler conn = new ConnectionHandler(this);

                    logi("클라이언트 통신용 쓰레드 생성 CloudPhoneTestServer new ConnectionHandler");
                    conn.threadListener = this.client;
                    Thread newThread = new Thread(new ThreadStart(conn.clientHandler));
                    newThread.Start();
                }
            }
            catch (Exception e)
            {
                loge("ListenerThread : " + e.Message);
            }
        }

        // Off 버튼이 눌리면
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

        // listLog의 Log 색상을 다르게 하기 위해 오버라이딩
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
