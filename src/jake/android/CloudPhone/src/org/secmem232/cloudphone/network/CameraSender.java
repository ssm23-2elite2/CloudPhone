package org.secmem232.cloudphone.network;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;

import org.secmem232.cloudphone.network.PacketHeader.OpCode;
import org.secmem232.cloudphone.util.ConvertUtil;


public class CameraSender extends PacketSender {
	private static final int MAXDATASIZE = Packet.MAX_LENGTH - PacketHeader.LENGTH;

	private static final int INFOLENGTH = 10;
	private static final int ORIENTATION_INFO_LENGTH = 1;

	private byte[] sendBuffer = new byte[MAXDATASIZE];
	private byte[] sizeInfo = new byte[INFOLENGTH];

	public CameraSender(OutputStream out, InputStream in) {
		super(out, in);
	}

	public void SendCameraPreview(byte[] image, int orientation, int size) throws IOException{
		int totalSize = size;
		int transmittedSize = 0;		

		sizeInfo[0] = (byte)orientation;
		int length = ConvertUtil.itoa(totalSize, sizeInfo, 1) + ORIENTATION_INFO_LENGTH;

		Packet packet = new Packet(OpCode.INFO_SEND, sizeInfo, length);

		Send(packet);

		while(totalSize > transmittedSize){
			int CurTransSize = (totalSize - transmittedSize) > MAXDATASIZE ? MAXDATASIZE : (totalSize - transmittedSize);
			System.arraycopy(image, transmittedSize, sendBuffer, 0, CurTransSize);
			transmittedSize += CurTransSize;

			Packet dataPacket = new Packet(OpCode.DATA_SEND, sendBuffer, CurTransSize);
			Send(dataPacket);
		}		
	}
}
