package org.secmem232.cloudphone.network;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.InetSocketAddress;
import java.net.Socket;

import org.secmem232.cloudphone.network.PacketHeader.OpCode;

import android.util.Log;


public class CloudPhoneSocket implements PacketListener {
	private final static String LOG = "CloudPhoneSocket";
	private Socket socket;
	private OutputStream sendStream;
	private InputStream recvStream;

	private ServerConnectionListener mServerConnectionListener;
	private PacketReceiver packetReceiver;	
	private CameraSender streamSender;
	private ScreenTransmissionListener mScreenTransListener;
	
	public CloudPhoneSocket(ServerConnectionListener listener){
		Log.w(LOG, "CloudPhoneSocket");
		mServerConnectionListener = listener;
	}

	public boolean isConnected(){
		Log.w(LOG, "isConnected");
		return socket != null ? socket.isConnected() : false;		
	}
	
	public void setScreenTransMissionListener(ScreenTransmissionListener listener){
		Log.w(LOG, "setScreenTransMissionListener");
		mScreenTransListener = listener;
	}

	public synchronized boolean connect(String ip, int port){
		Log.w(LOG, "connect");
		try{
			socket = new Socket();
			socket.connect(new InetSocketAddress(ip, port), 2000); // Set timeout to 2 seconds

			// Open outputStream
			sendStream = socket.getOutputStream();
			streamSender = new CameraSender(sendStream);

			// Open inputStream
			recvStream = socket.getInputStream();		

			// Create and start packet receiver
			packetReceiver = new PacketReceiver(recvStream);
			packetReceiver.setPacketListener(this);
			packetReceiver.start();	

			mServerConnectionListener.onServerConnected(ip);

			return true;
		} catch(IOException e) {
			e.printStackTrace();
			mServerConnectionListener.onServerConnectionFailed();

			return false;
		}
	}

	public void disconnect(){
		Log.w(LOG, "disconnect");
		synchronized(this){
			if(socket != null){
				try{				
					recvStream.close();
					sendStream.close();
					packetReceiver = null;				
					socket.close();		
					socket = null;
				} catch(IOException e) {
					e.printStackTrace();
				} finally{
					mServerConnectionListener.onServerDisconnected();
				}
			}
		}
	}

	private void cleanup(){
		Log.w(LOG, "cleanup");
		synchronized(this){
			if(socket != null){
				try{
					recvStream.close();
					sendStream.close();
					packetReceiver = null;
					socket.close();
					socket = null;

				} catch(IOException e) {
					e.printStackTrace();
				}
			}
		}
	}

	@Override
	public void onPacketReceived(Packet packet) {
		switch(packet.getOpcode()){		
		case OpCode.SCREEN_SEND_REQUESTED:			
			Log.w(LOG, "onPacketReceived.SCREEN_SEND_REQUESTED");
			mScreenTransListener.onScreenTransferRequested();
			break;
		case OpCode.SCREEN_STOP_REQUESTED:
			Log.w(LOG, "onPacketReceived.SCREEN_STOP_REQUESTED");
			mScreenTransListener.onScreenTransferStopRequested();
			break;
		
		}
	}

	@Override
	public void onInterrupt() {
		Log.w(LOG, "onInterrupt");
		synchronized(this){
			if(socket!=null){
				try{					
					recvStream.close();
					sendStream.close();
					packetReceiver = null;
					socket.close();
					socket=null;
								
				}catch(IOException e){
					e.printStackTrace();
				}finally{
					mServerConnectionListener.onServerConnectionInterrupted();				
				}
			}
		}
	}
}
