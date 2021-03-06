package org.secmem232.cloudphone.network;

import java.io.IOException;
import java.io.InputStream;

public class PacketReceiver extends Thread {		

	private byte[] buffer = new byte[Packet.MAX_LENGTH*2];
	private int bufferOffset = 0;
	private Packet packet;
	
	private InputStream recvStream;
	private PacketListener mListener;
	
	public PacketReceiver(InputStream recvStream){
		this.recvStream = recvStream;
	}
	
	public PacketReceiver(InputStream recvStream, PacketListener listener){
		this.recvStream = recvStream;
		this.mListener = listener;		
	}		
	
	public void setPacketListener(PacketListener listener){
		this.mListener = listener;
	}
	
	private int readPacketDataFromStream() throws IOException{
		
		int readLen = recvStream.read(buffer, bufferOffset, Packet.MAX_LENGTH);			
			
		if(readLen>0)
			bufferOffset+=readLen;
		
		return readLen;
	}
	
	private boolean tryReadPacketData(){
		if(bufferOffset < PacketHeader.LENGTH)
			return false; // try fetching more data from stream
		
		PacketHeader header = PacketHeader.parse(buffer);
		
		if(bufferOffset < header.getPacketLength())
			return false; //  try fetching more data from stream
		
		packet = Packet.parse(buffer);
		bufferOffset-=header.getPacketLength();
		
		System.arraycopy(buffer, header.getPacketLength(), buffer, 0, bufferOffset);
		return true;
	}	
	

	@Override
	public void run() {	
		if(mListener==null)
			throw new IllegalStateException("Packet listener must be set first.");
		
		while(true){
			try{
				int readLen = readPacketDataFromStream();
				if(readLen < 0){
					//when host was closed
					throw new IOException();
				}
				
				while(tryReadPacketData()){
					mListener.onPacketReceived(packet);
				}				
				
			} catch(IOException e){
				e.printStackTrace();					
				mListener.onInterrupt();
				break;
			}
		}
	}		
}
