using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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

    public static class ImageUtil
    {
        public static Image RotateImage(Image img, float rotationAngle)
        {
            //create an empty Bitmap image
            Bitmap bmp = new Bitmap(img.Width, img.Height);

            //turn the Bitmap into a Graphics object
            Graphics gfx = Graphics.FromImage(bmp);

            //now we set the rotation point to the center of our image
            gfx.TranslateTransform((float)bmp.Width / 2, (float)bmp.Height / 2);

            //now rotate the image
            gfx.RotateTransform(rotationAngle);

            gfx.TranslateTransform(-(float)bmp.Width / 2, -(float)bmp.Height / 2);

            //set the InterpolationMode to HighQualityBicubic so to ensure a high
            //quality image once it is transformed to the specified size
            gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;

            //now draw our new image onto the graphics object
            gfx.DrawImage(img, new Point(0, 0));

            //dispose of our Graphics object
            gfx.Dispose();

            //return the image
            return bmp;
        }
    }
}
