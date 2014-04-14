package org.secmem232.cloudphone.network;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;

import android.os.StrictMode;

public class PacketSender {
	private OutputStream sendStream;
	private InputStream recvStream;

	public PacketSender(OutputStream sendStream, InputStream recvStream){
		this.sendStream = sendStream;
		this.recvStream = recvStream;
		
	}

	public void setOutputStream(OutputStream stream){
		this.sendStream = stream;
	}
	
	public void setInputStream(InputStream stream) {
		this.recvStream = stream;
	}

	public void Send(Packet packet) throws IOException{
		//get packet size for transmission
		int packetSize = packet.getHeader().getPacketLength();
		synchronized(java.lang.Object.class){
			sendStream.write(packet.asByteArray(), 0, packetSize);

			StrictMode.setThreadPolicy(new StrictMode.ThreadPolicy.Builder().permitNetwork().build());
			recvStream.read();
		}
	}
}
