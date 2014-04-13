package org.secmem232.cloudphone.network;

import java.io.IOException;
import java.io.OutputStream;

public class PacketSender {
	private OutputStream sendStream;	

	public PacketSender(OutputStream stream){
		this.sendStream = stream;		
	}

	public void setOutputStream(OutputStream stream){
		this.sendStream = stream;
	}

	public void Send(Packet packet) throws IOException{
		//get packet size for transmission
		int packetSize = packet.getHeader().getPacketLength();
		synchronized(java.lang.Object.class){
			sendStream.write(packet.asByteArray(), 0, packetSize);
		}
	}
}
