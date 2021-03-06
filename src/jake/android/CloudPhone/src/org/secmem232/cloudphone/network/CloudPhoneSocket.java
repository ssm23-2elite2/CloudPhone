package org.secmem232.cloudphone.network;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.Socket;

import android.util.Log;

public class CloudPhoneSocket {
	private final static String LOG = "CloudPhoneSocket";
	private Socket mSocket;
	private OutputStream mSendStream;
	private InputStream mRecvStream;
	
	private DatagramSocket mUDPSocket;
	private InetAddress mServerAddr;

	private CameraSenderListener mCameraSenderListener;
	private ServerConnectionListener mServerConnectionListener;
	private CameraSender mCameraSender;

	public CloudPhoneSocket(ServerConnectionListener listener){
		Log.w(LOG, "CloudPhoneSocket");
		mServerConnectionListener = listener;
	}

	public void setCameraSenderListener(CameraSenderListener listener){
		mCameraSenderListener = listener;
	}

	public boolean isConnected(){
		Log.w(LOG, "isConnected");
		return mSocket != null ? mSocket.isConnected() : false;		
	}

	public synchronized boolean connect(String ip, int port){
		Log.w(LOG, "connect");
		try{
			mSocket = new Socket();

			mServerAddr = InetAddress.getByName(ip);
			mUDPSocket = new DatagramSocket();
			mCameraSender = new CameraSender(mUDPSocket, mServerAddr, port, mSendStream, mRecvStream);

			mServerConnectionListener.onServerConnected(ip);

			return true;
		} catch(IOException e) {
			e.printStackTrace();
			mServerConnectionListener.onServerConnectionFailed();

			return false;
		}
	}

	//Send image
	public void SendCameraPreview(byte[] image, int orientation, int size){
		try{
			mCameraSender.SendCameraPreview(image, orientation, size);
		}catch(IOException e){
			e.printStackTrace();
			mCameraSenderListener.onCameraSenderInterrupted();
		}
	}

	public void disconnect(){
		Log.w(LOG, "disconnect");
		synchronized(this){
			if(mSocket != null){
				try{				
					mRecvStream.close();
					mSendStream.close();		
					mSocket.close();		
					mSocket = null;
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
			if(mSocket != null){
				try{
					mRecvStream.close();
					mSendStream.close();
					mSocket.close();
					mSocket = null;

				} catch(IOException e) {
					e.printStackTrace();
				}
			}
		}
	}

	/*
	@Override
	public void onPacketReceived(Packet packet) {
		switch(packet.getOpcode()){		
		case OpCode.SCREEN_SEND_REQUESTED:			
			Log.w(LOG, "onPacketReceived.SCREEN_SEND_REQUESTED");
			break;
		case OpCode.SCREEN_STOP_REQUESTED:
			Log.w(LOG, "onPacketReceived.SCREEN_STOP_REQUESTED");
			break;
		}
	}*/

	/*
	@Override
	public void onInterrupt() {
		Log.w(LOG, "onInterrupt");
		synchronized(this){
			if(mSocket!=null){
				try{					
					mRecvStream.close();
					mSendStream.close();
					mPacketReceiver = null;
					mSocket.close();
					mSocket=null;

				}catch(IOException e){
					e.printStackTrace();
				}finally{
					mServerConnectionListener.onServerConnectionInterrupted();				
				}
			}
		}
	}*/
}
