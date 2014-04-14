using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CloudPhoneTestServer
{
    public class ConnectionHandler
    {
        private CloudPhoneForm cloudphoneForm;
        private int iTotalSize;
        private byte bOrientation;
        private int iRecvSize;

        public ConnectionHandler(CloudPhoneForm form)
        {
            isRunning = true;
            cloudphoneForm = form;
            cloudphoneForm.Invoke(cloudphoneForm._logi, "클라이언트 생성 ConnectionHandler");
        }

        public TcpListener threadListener;

        public Boolean isRunning { get; set; }

        public void clientHandler()
        {
            int iOPCode;
            int iPacketSize;
            MemoryStream imageStream = null;

            try
            {
                TcpClient client = threadListener.AcceptTcpClient();
                NetworkStream ns = client.GetStream();
                
                while (isRunning)
                {
                    
                    try
                    {
                        byte[] packet = new byte[4096 * 10];
                        int iRecvLen = ns.Read(packet, 0, packet.Length);
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

                                if (iRecvSize == iTotalSize)
                                {
                                    Image image = Image.FromStream(imageStream);
                                    cloudphoneForm.Invoke(cloudphoneForm.showImageDelegate, image, bOrientation);
                                    imageStream.SetLength(0);
                                    imageStream = null;
                                }
                                
                                break;
                        }
                        //echo 
                        packet = null;
                        //GC.Collect();
                        ns.Write(new byte[1], 0, 1);
                    }
                    catch (Exception e)
                    {
                        cloudphoneForm.Invoke(cloudphoneForm._loge, "clientHandler : " + e.ToString());
                        break;
                    }
                
                }
                ns.Close();
                client.Close();
            }
            catch (Exception e2)
            {
                cloudphoneForm.Invoke(cloudphoneForm._loge, "clientHandler : " + e2.Message);
            }
            cloudphoneForm.Invoke(cloudphoneForm._logi, "클라이언트가 종료되었습니다.");
        }
    }
}
