using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CloudPhoneTestClient
{
    public partial class CloudPhoneForm : Form
    {
        private TcpClient server;
        private NetworkStream ns;
        private bool isConnected;

        public CloudPhoneForm()
        {
            InitializeComponent();
        }

        private void CloudPhoneForm_Load(object sender, EventArgs e)
        {
            try
            {
                server = new TcpClient("211.189.20.137", 3737);
                ns = server.GetStream();
                isConnected = true;
            }
            catch (SocketException)
            {
                MessageBox.Show("서버와의 연결을 실패했습니다.");
            }
        }

        private void CloudPhoneForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (isConnected)
            {
                isConnected = false;
                ns.Close();
                server.Close();
            }
        }

        private void btn_connectionOn_Click(object sender, EventArgs e)
        {
            if (isConnected == false)
            {
                try
                {
                    server = new TcpClient("211.189.20.137", 3737);
                    ns = server.GetStream();
                    isConnected = true;
                }
                catch (SocketException)
                {
                    MessageBox.Show("서버와의 연결을 실패했습니다.");
                }
            }
        }

        private void btn_connectionOff_Click(object sender, EventArgs e)
        {
            if (isConnected)
            {
                isConnected = false;
                ns.Close();
                server.Close();
            }
        }
        
        public void SendThread(object img)
        {
            byte[] bs = imageToByteArray((Image)img);

            byte[] buffer = BitConverter.GetBytes(bs.Length);

            ns.Write(buffer, 0, buffer.Length);
            ns.Write(bs, 0, bs.Length);
        }

        private void btn_openClick(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog so the user can select a Cursor.
            if (isConnected)
            {
                OpenFileDialog fileDlg = new OpenFileDialog();
                fileDlg.Filter = "Image Files|*.jpg";
                fileDlg.Title = "Select a Image File";

                if (fileDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Image image = Image.FromFile(fileDlg.FileName, true);
                    Thread sendThread = new Thread(new ParameterizedThreadStart(SendThread));
                    sendThread.Start(image);
                }
            }
            else
            {
                MessageBox.Show("연결을 먼저 해야합니다.");
            }
        }

        public byte[] imageToByteArray(Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            return ms.ToArray();
        }
    }
}
