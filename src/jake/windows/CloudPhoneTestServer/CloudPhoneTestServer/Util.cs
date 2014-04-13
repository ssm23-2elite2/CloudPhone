using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudPhoneTestServer
{
    public static class Util
    {
        public static int GetOpCode(byte[] packet) {
            byte[] bOpCode = new byte[PacketHeader.OPCODE_LENGTH];
            Buffer.BlockCopy(packet, 0, bOpCode, 0, PacketHeader.OPCODE_LENGTH);
            return int.Parse(Encoding.Default.GetString(bOpCode));
        }

        public static int GetPacketSize(byte[] packet)
        {
            byte[] bPacketSize = new byte[PacketHeader.PAYLOAD_LENGTH];
            Buffer.BlockCopy(packet, PacketHeader.OPCODE_LENGTH, bPacketSize, 0, PacketHeader.PAYLOAD_LENGTH);
            return int.Parse(Encoding.Default.GetString(bPacketSize)); 
        }
    }
}
