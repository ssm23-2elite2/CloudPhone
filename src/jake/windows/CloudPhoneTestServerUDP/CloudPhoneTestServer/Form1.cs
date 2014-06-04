using System;
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
using System.Runtime.InteropServices;

namespace CloudPhoneTestServer
{
    public partial class CloudPhoneForm : Form
    {
        [DllImport("CloudPhoneDll.dll")]
        public static extern int DisplayWebCam(byte[] pArray, int length);

        private Boolean isConnected;
        private UdpClient client;
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
        private int iOPCode;
        private int iPacketSize;
        private byte bOrientation;
        int iTotalSize;
        int iRecvSize;
        MemoryStream imageStream;

        private Boolean justOne = true;
        public CloudPhoneForm()
        {
            InitializeComponent();
        }

        // �ʱ�ȭ
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
            logi("���� ������ ���� CloudPhoneTestServer new Thread");
            serverThread = new Thread(new ThreadStart(ListenerThread));
            serverThread.Start();
        }

        // ����� ó��
        private void CloudPhoneForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (serverThread != null && serverThread.IsAlive)
            {
                //client.Stop();
                serverThread.Abort();
                serverThread = null;
            }

            string processName = "CloudPhoneTestServer"; // .exe �� ���ž� �ǿ�~
            Process[] processes = Process.GetProcessesByName(processName);

            foreach (Process process in processes)
            {
                process.Kill();
            }
        }

        // Ŭ���̾�Ʈ�� ��û�� �޾Ƽ� �̹����� �Ѹ� �� ����Ǵ� �Լ�
        public void showImageMethod(Image image, byte bOrientation)
        {
            //logi("�̹����� �����ݴϴ�. CloudPhoneTestServer.showImageMethod");
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

        // On ��ư�� ������
        private void btn_serverOn_Click(object sender, EventArgs e)
        {
            logi("������ �մϴ�. CloudPhoneTestServer.btn_serverOn_Click");
            if (isConnected)
            {
                logw("������ �̹� ����Ǿ��ֽ��ϴ�.");
            }
            else
            {
                isConnected = true;
                logi("���� ������ ���� CloudPhoneTestServer new Thread");
                serverThread = new Thread(new ThreadStart(ListenerThread));
                serverThread.Start();
            }
        }

        // Ŭ���̾�Ʈ�� ������ ��ٸ��� Thread
        public void ListenerThread()
        {

            client = new UdpClient(3838);
            var remoteEP = new IPEndPoint(IPAddress.Any, 3838);
            logi("Ŭ���̾�Ʈ �����... CloudPhoneTestServer.ListenerThread");

            while (true)
            {
               // try
                {
                    byte[] packet = client.Receive(ref remoteEP);
                    //logi("Ŭ���̾�Ʈ�� ��û �߰�!... CloudPhoneTestServer.ListenerThread");

                    int iRecvLen = packet.Length;
                    if (iRecvLen <= 0)
                    {
                        break;
                    }

                    try
                    {
                        iOPCode = Util.GetOpCode(packet);
                        iPacketSize = Util.GetPacketSize(packet);
                    }
                    catch (FormatException e)
                    {
                        continue;
                    }

                    packet = packet.Skip(PacketHeader.LENGTH).ToArray();

                    switch (iOPCode)
                    {
                        case OpCode.INFO_SEND:
                            // Orientation
                            bOrientation = packet[0];

                            // Size
                            byte[] bInfoLength = new byte[PacketPayload.INFO_LENGTH];
                            Buffer.BlockCopy(packet, 1, bInfoLength, 0, PacketPayload.INFO_LENGTH);
                            bInfoLength.Reverse();
                            iTotalSize = int.Parse(Encoding.Default.GetString(bInfoLength));

                            bInfoLength = null;
                            iRecvSize = 0;
                            imageStream = new MemoryStream();
                            break;
                        case OpCode.DATA_SEND:
                            imageStream.Write(packet, 0, iPacketSize - PacketHeader.LENGTH);
                            iRecvSize += iPacketSize - PacketHeader.LENGTH;
                            
                            if (iRecvSize >= iTotalSize)
                            {
                                logi("iRecvSize " + iRecvSize);
                                //if (justOne == true)
                                {
                                    justOne = false;
                                    int n = DisplayWebCam(imageStream.ToArray(), imageStream.ToArray().Length);
                                    logi("DisplayWebCam " + n);
                                }
                                //Image image = Image.FromStream(imageStream);
                                //showImageMethod(image, bOrientation);

                                imageStream.SetLength(0);
                                imageStream = null;
                            }

                            break;
                    }
                    packet = null;
                }
              //  catch (Exception e)
             //   {
              //      loge("ListenerThread : " + e.Message);
              //      continue;
               // }
            }

        }

        // Off ��ư�� ������
        private void btn_serverOff_Click(object sender, EventArgs e)
        {
            if (serverThread != null && serverThread.IsAlive)
            {
                isConnected = false;
                //client.Stop();
                serverThread.Abort();
                serverThread = null;
                logi("������ ���ϴ�. CloudPhoneTestServer.btn_serverOff_Click");
            }
            else
            {
                logw("������ ����Ǿ����� �ʽ��ϴ�.");
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
            listLog.Items.Add(new ListBoxItem(Color.Green, "���� : " + date + msg));
            statusBar.Text = "���� : " + date + msg;
            logend();
        }

        private void logw(String msg)
        {
            String date = System.DateTime.Now.ToString("MM-dd hh:mm:ss ");
            listLog.Items.Add(new ListBoxItem(Color.Orange, "��� : " + date + msg));
            statusBar.Text = "��� : " + date + msg;
            logend();
        }

        private void loge(String msg)
        {
            String date = System.DateTime.Now.ToString("MM-dd hh:mm:ss ");
            listLog.Items.Add(new ListBoxItem(Color.Red, "���� : " + date + msg));
            statusBar.Text = "���� : " + date + msg;
            logend();
        }

        // listLog�� Log ������ �ٸ��� �ϱ� ���� �������̵�
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