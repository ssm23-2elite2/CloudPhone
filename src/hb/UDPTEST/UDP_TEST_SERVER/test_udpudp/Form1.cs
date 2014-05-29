using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Windows.Forms;


namespace test_udpudp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            int i = 0;

            IPHostEntry ipHost = Dns.Resolve("211.189.20.132");
            IPAddress ipAddr = ipHost.AddressList[0];
            Socket udpSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ipep = new IPEndPoint(ipAddr, 20001);

            String path = Application.StartupPath + "\\bg.png"; // 프로젝트 폴더 / bin에 있는 파일.

            byte[] data = new byte[4096 * 10];

            FileStream fs = new FileStream(path, FileMode.Open);
            
            String header = "size" + fs.Length.ToString();
            
            data = Encoding.UTF8.GetBytes(header);

            udpSock.SendTo(data, ipep);

            Array.Clear(data, 0, data.Length);

            BinaryReader br = new BinaryReader(fs);

            int count = (int)fs.Length / (4096 * 10) + 1;

            for(i = 0 ; i < count ; i ++){
                data = br.ReadBytes(4096 * 10);
                udpSock.SendTo(data, ipep);
                Array.Clear(data, 0, data.Length);
            }

            br.Close();
            udpSock.Close();

        }
    }
}