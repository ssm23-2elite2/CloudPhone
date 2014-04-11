package org.secmem232.cloudphone;

import org.secmem232.cloudphone.intent.CloudPhoneIntent;
import org.secmem232.cloudphone.network.CloudPhoneSocket;
import org.secmem232.cloudphone.network.ScreenTransmissionListener;
import org.secmem232.cloudphone.network.ServerConnectionListener;

import android.app.Service;
import android.content.Context;
import android.content.Intent;
import android.graphics.PixelFormat;
import android.location.Location;
import android.location.LocationListener;
import android.location.LocationManager;
import android.os.AsyncTask;
import android.os.Bundle;
import android.os.IBinder;
import android.os.RemoteException;
import android.util.Log;
import android.view.Gravity;
import android.view.WindowManager;
import android.widget.FrameLayout;
import android.widget.FrameLayout.LayoutParams;

public class CloudPhoneService extends Service implements LocationListener, ServerConnectionListener, ScreenTransmissionListener {
	private final static String LOG = "CloudPhoneService";
	
	//카메라 관련
	private LocationManager mLocationManager;
	private int mCameraId = 0;
	private CloudPhoneCameraPreview mPreview;
	private FrameLayout mLayout;
	private WindowManager.LayoutParams mPreviewLayoutParams;
	private WindowManager mWindowManager;
		
	//서비스 상태
	public enum ServiceState { IDLE, CONNECTING, CONNECTED };
	
	private CloudPhoneSocket mSocket;
	private static ServiceState mState = ServiceState.IDLE;
	
	//서버 소켓 관련
	private IBinder mBinder = new ICloudPhone.Stub() {

		@Override
		public boolean isConnected() throws RemoteException {
			Log.w(LOG, "isConnected");
			return (mSocket != null && mSocket.isConnected()) ? true : false;
		}

		@Override
		public String getConnectionStatus() throws RemoteException {
			Log.w(LOG, "getConnectionStatus");
			return mState.name();
		}
		
		@Override
		public void connect(final String ip, final int port) throws RemoteException {
			Log.w(LOG, "connect");
			new AsyncTask<Void, Void, Void>() {
				@Override
				protected Void doInBackground(Void... params) {
					if( mSocket.connect(ip, port)) {
						if(mSocket != null && mSocket.isConnected()) {
							mState = ServiceState.CONNECTING;
							sendBroadcast(new Intent(CloudPhoneIntent.ACTION_CONNECTED));
						}
					}
					return null;
				}
			}.execute();
		}

		@Override
		public void disconnect() throws RemoteException {
			Log.w(LOG, "disconnect");
			new AsyncTask<Void, Void, Void>(){
				@Override
				protected Void doInBackground(Void... params) {
					mSocket.disconnect();
					mState = ServiceState.IDLE;
					return null;
				}
			}.execute();
		}
	};

	@Override
	public IBinder onBind(Intent intent) {
		Log.w(LOG, "onBind");
		return mBinder;
	}
	
	@Override
	public void onCreate() {
		super.onCreate();
		Log.w(LOG, "onCreate");
		mSocket = new CloudPhoneSocket(this);
		mSocket.setScreenTransMissionListener(this);
		mLocationManager = (LocationManager) getSystemService(Context.LOCATION_SERVICE);
		mLocationManager.requestLocationUpdates(LocationManager.GPS_PROVIDER, Integer.MAX_VALUE, Integer.MAX_VALUE, this);	
	}
	
	private void createCameraPreview() {
		mPreview = new CloudPhoneCameraPreview(this, mCameraId, CameraPreview.LayoutMode.NoBlank, false);
		LayoutParams previewLayoutParams = new LayoutParams(LayoutParams.MATCH_PARENT, LayoutParams.MATCH_PARENT);
		mLayout.addView(mPreview, 0, previewLayoutParams);
	}
	
	public ServiceState getConnectionState() {
		Log.w(LOG, "getConnectionState");
		return mState;
	}

	@Override
	public void onScreenTransferRequested() {
		Log.w(LOG, "onScreenTransferRequested");
		Thread mThread = new Thread(){
			@Override
			public void run() {
				
			}
		};
		mThread.setDaemon(true);
		mThread.start();
	}

	@Override
	public void onScreenTransferStopRequested() {
		Log.w(LOG, "onScreenTransferStopRequested");
		
	}

	@Override
	public void onScreenTransferInterrupted() {
		Log.w(LOG, "onScreenTransferInterrupted");
		onScreenTransferStopRequested();
	}

	@Override
	public void onServerConnected(String ipAddress) {
		Log.w(LOG, "onServerConnected");
		mState = ServiceState.CONNECTED;
	}

	@Override
	public void onServerConnectionFailed() {
		Log.w(LOG, "onServerConnectionFailed");
		mState = ServiceState.IDLE;
		sendBroadcast(new Intent(CloudPhoneIntent.ACTION_CONNECTION_FAILED));
	}

	@Override
	public void onServerConnectionInterrupted() {
		Log.w(LOG, "onServerConnectionInterrupted");
		mState = ServiceState.IDLE;
		sendBroadcast(new Intent(CloudPhoneIntent.ACTION_INTERRUPTED));
	}

	@Override
	public void onServerDisconnected() {
		Log.w(LOG, "onServerDisconnected");
		mState = ServiceState.IDLE;		
	}

	@Override
	public void onLocationChanged(Location location) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void onStatusChanged(String provider, int status, Bundle extras) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void onProviderEnabled(String provider) {
		createCameraPreview();
		mPreviewLayoutParams = new WindowManager.LayoutParams(
				WindowManager.LayoutParams.WRAP_CONTENT,
				WindowManager.LayoutParams.WRAP_CONTENT,
				WindowManager.LayoutParams.TYPE_PHONE,//항상 최 상위. 터치 이벤트 받을 수 있음.
				WindowManager.LayoutParams.FLAG_NOT_FOCUSABLE,  //포커스를 가지지 않음
				PixelFormat.TRANSLUCENT);
		mPreviewLayoutParams.gravity = Gravity.LEFT | Gravity.TOP;
		mPreviewLayoutParams.setTitle("");
		mWindowManager = (WindowManager) getSystemService(WINDOW_SERVICE);
		mWindowManager.addView(mLayout, mPreviewLayoutParams);
	}

	@Override
	public void onProviderDisabled(String provider) {
		if(mLayout != null) {
			mPreview.stop();
			((WindowManager) getSystemService(WINDOW_SERVICE)).removeView(mLayout);
			mLayout = null;
		}
	}
}
