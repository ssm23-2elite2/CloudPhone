package org.secmem232.cloudphone;

import org.secmem232.cloudphone.CameraPreview.CameraPreviewListener;
import org.secmem232.cloudphone.intent.CloudPhoneIntent;
import org.secmem232.cloudphone.network.CloudPhoneSocket;
import org.secmem232.cloudphone.network.ScreenTransmissionListener;
import org.secmem232.cloudphone.network.ServerConnectionListener;

import android.app.Notification;
import android.app.NotificationManager;
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
import android.view.WindowManager.LayoutParams;

public class CloudPhoneService extends Service implements LocationListener, ServerConnectionListener, ScreenTransmissionListener,
		CameraPreviewListener {
	private static final String LOG = "CloudPhoneService";
	
	//카메라 관련
	private LocationManager mLocationManager;
	private CloudPhoneCameraPreview mPreview;
	private WindowManager mWindowManager;
	private NotificationManager mNotificationManager;
	private int mCameraId = 0;
	
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
		
		mNotificationManager = (NotificationManager) getSystemService (Context.NOTIFICATION_SERVICE);
        mWindowManager = (WindowManager) getSystemService (Context.WINDOW_SERVICE);
		mSocket = new CloudPhoneSocket(this);
		mSocket.setScreenTransMissionListener(this);
		mLocationManager = (LocationManager) getSystemService(Context.LOCATION_SERVICE);
		mLocationManager.requestLocationUpdates(LocationManager.GPS_PROVIDER, Integer.MAX_VALUE, Integer.MAX_VALUE, this);	
		
		
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
		Notification notification = new Notification.Builder(this)
            .setContentTitle("Background Video Recorder")
            .setContentText("")
            .setSmallIcon(R.drawable.ic_launcher)
            .build();
        startForeground(3737, notification);
        
        mPreview = new CloudPhoneCameraPreview(this, 0, CameraPreview.LayoutMode.NoBlank, false);
		LayoutParams mLayoutParams = new WindowManager.LayoutParams(
				1,1,
			WindowManager.LayoutParams.TYPE_SYSTEM_OVERLAY,
			WindowManager.LayoutParams.FLAG_WATCH_OUTSIDE_TOUCH,
			PixelFormat.TRANSLUCENT
		);
		mPreview.setCameraPreviewListener(this);
		mLayoutParams.gravity = Gravity.LEFT | Gravity.TOP;
		mWindowManager.addView(mPreview, mLayoutParams);
	}

	@Override
	public void onProviderDisabled(String provider) {
		mNotificationManager.cancel(3737);
		if(mPreview != null) {
			mPreview.stop();
			mWindowManager.removeView(mPreview);
			mPreview = null;
		}
	}

	@Override
	public void onPreview(byte[] image) {
		// 실제 전송처리
	}
}
