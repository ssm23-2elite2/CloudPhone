using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudPhoneTestClient
{
   static class OpCode
    {
        public const int INVALID = -1;
        public const int INFO_SEND = 1;
        public const int DATA_SEND = 2;
        public const int SCREEN_SEND_REQUESTED = 3;
		public const int SCREEN_STOP_REQUESTED = 4;
		public const int SCREEN_ON_STATE_INFO = 5;
        public const int SCREEN_OFF_STATE_INFO = 6;
    }

    static class PacketHeader
    {
        public const int LENGTH = 6;
        public const int OPCODE_LENGTH = 2;
        public const int PAYLOAD_LENGTH = 4;
    }

    static class PacketPayload
    {
        public const int INFO_LENGTH = 10;
        public const int ORIENTATION_INFO_LENGTH = 1;
    }
}
