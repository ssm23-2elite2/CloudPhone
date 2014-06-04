package org.secmem232.cloudphone.network;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;

import android.os.StrictMode;

public class PacketSender {
	private int port;
	private DatagramSocket socket;
	private InetAddress serverAddr;
	private OutputStream sendStream;
	private InputStream recvStream;

	public PacketSender(DatagramSocket socket, InetAddress serverAddr, int port, OutputStream sendStream, InputStream recvStream){
		this.socket = socket;
		this.serverAddr = serverAddr;
		this.port = port;
		this.sendStream = sendStream;
		this.recvStream = recvStream;
	}

	public void setOutputStream(OutputStream stream){
		this.sendStream = stream;
	}
	
	public void setInputStream(InputStream stream) {
		this.recvStream = stream;
	}

	public synchronized void Send(Packet packet) throws IOException{
		//get packet size for transmission
		int packetSize = packet.getHeader().getPacketLength();
		synchronized(java.lang.Object.class){
			StrictMode.setThreadPolicy(new StrictMode.ThreadPolicy.Builder().permitNetwork().build());
			DatagramPacket data = new DatagramPacket(packet.asByteArray(), packetSize, serverAddr, port);
			socket.send(data);
			/*
			sendStream.write(packet.asByteArray(), 0, packetSize);
			StrictMode.setThreadPolicy(new StrictMode.ThreadPolicy.Builder().permitNetwork().build());
			recvStream.read();*/
		}
	}
}
