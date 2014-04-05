package com.ssm232.elite.cloudphonetest;

import java.io.OutputStream;
import java.net.Socket;
import java.util.List;

import android.app.Service;
import android.content.Context;
import android.content.Intent;
import android.graphics.PixelFormat;
import android.graphics.Rect;
import android.hardware.Camera;
import android.location.Location;
import android.location.LocationListener;
import android.location.LocationManager;
import android.os.Bundle;
import android.os.IBinder;
import android.util.Log;
import android.view.Gravity;
import android.view.View;
import android.view.WindowManager;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.FrameLayout;
import android.widget.FrameLayout.LayoutParams;
import android.widget.Spinner;

import com.ssm232.elite.cloudphonetest.camera.CameraPreview;
import com.ssm232.elite.cloudphonetest.camera.ResizableCameraPreview;

public class CloudPhoneService extends Service implements LocationListener, AdapterView.OnItemSelectedListener {

	private final String LOG = "CloudPhoneService";
	private ResizableCameraPreview mPreview;
	private ArrayAdapter<String> mAdapter;
	private FrameLayout mLayout;
	private int mCameraId = 0;

	private WindowManager.LayoutParams previewLayoutParams;
	private WindowManager wm;
	private LocationManager lm;

	private int spinner_size = 1024;
	private int spinner_camera = 2048;
	private Spinner mSpinnerSize;
	private Spinner mSpinnerCamera;

	private String ip = "211.189.20.137";
	private int port = 3737;

	@Override
	public IBinder onBind(Intent intent) {
		return null;
	}

	@Override
	@Deprecated
	public void onStart(Intent intent, int startId) {
		super.onStart(intent, startId);
		Log.w(LOG, "onStart");
		
		lm = (LocationManager) getSystemService(Context.LOCATION_SERVICE);
		lm.requestLocationUpdates(LocationManager.GPS_PROVIDER, Integer.MAX_VALUE, Integer.MAX_VALUE, this);
	}

	private void createCameraPreview() {
		Log.w(LOG, "createCameraPreview");
		mPreview = new ResizableCameraPreview(this, mCameraId, CameraPreview.LayoutMode.NoBlank, false, ip, port);
		LayoutParams previewLayoutParams = new LayoutParams(LayoutParams.MATCH_PARENT, LayoutParams.MATCH_PARENT);
		mLayout.addView(mPreview, 0, previewLayoutParams);

		mAdapter.clear();
		mAdapter.add("Auto");
		List<Camera.Size> sizes = mPreview.getSupportedPreivewSizes();
		for (Camera.Size size : sizes) {
			mAdapter.add(size.width + " x " + size.height);
		}
	}

	@Override
	public void onDestroy() {
		super.onDestroy();
	}

	@Override
	public void onItemSelected(AdapterView<?> parent, View view, int position,
			long id) {
		if(parent.getId() == spinner_size) {
			Log.w(LOG, "spinner_size " + position);
			Rect rect = new Rect();
			mLayout.getDrawingRect(rect);
			if (0 == position) { // "Auto" selected
				mPreview.surfaceChanged(null, 0, rect.width(), rect.height());
			} else {
				mPreview.setPreviewSize(position - 1, rect.width(), rect.height());
			}
		} else if (parent.getId() == spinner_camera) {
			if(position == 3) {
				onDestroy();
			} else {
				mPreview.stop();
				mLayout.removeView(mPreview);
				mCameraId = position;
				createCameraPreview();
			}
		}
	}

	@Override
	public void onNothingSelected(AdapterView<?> parent) {
		// TODO Auto-generated method stub
		
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
		Log.w(LOG, "onProviderEnabled");
		
		try {
			// Spinner for preview sizes
			mLayout = new FrameLayout(this);
			mSpinnerSize = new Spinner(this);
			mAdapter = new ArrayAdapter<String>(this, android.R.layout.simple_spinner_item);
			mAdapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
			mSpinnerSize.setAdapter(mAdapter);
			mSpinnerSize.setOnItemSelectedListener(this);

			// Spinner for camera ID
			mSpinnerCamera = new Spinner(this);
			ArrayAdapter<String> adapter;
			adapter = new ArrayAdapter<String>(this, android.R.layout.simple_spinner_item);
			adapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
			mSpinnerCamera.setAdapter(adapter);
			mSpinnerCamera.setOnItemSelectedListener(this);
			adapter.add("0");
			adapter.add("1");
			adapter.add("2");
			adapter.add("Exit");

			createCameraPreview();

			LayoutParams params = new LayoutParams(LayoutParams.WRAP_CONTENT, LayoutParams.WRAP_CONTENT);
			mLayout.addView(mSpinnerSize, params);
			mSpinnerSize.setId(spinner_size);

			params = new LayoutParams(LayoutParams.WRAP_CONTENT, LayoutParams.WRAP_CONTENT);
			params.gravity = Gravity.RIGHT | Gravity.TOP;
			mLayout.addView(mSpinnerCamera, params);
			mSpinnerCamera.setId(spinner_camera);

			//LayoutParams previewLayoutParams = new LayoutParams(LayoutParams.WRAP_CONTENT, LayoutParams.WRAP_CONTENT);
			previewLayoutParams = new WindowManager.LayoutParams(
					WindowManager.LayoutParams.WRAP_CONTENT,
					WindowManager.LayoutParams.WRAP_CONTENT,
					WindowManager.LayoutParams.TYPE_PHONE,//�׻� �� ����. ��ġ �̺�Ʈ ���� �� ����.
					WindowManager.LayoutParams.FLAG_NOT_FOCUSABLE,  //��Ŀ���� ������ ����
					PixelFormat.TRANSLUCENT);
			previewLayoutParams.gravity = Gravity.LEFT | Gravity.TOP;
			previewLayoutParams.setTitle("");
			wm = (WindowManager) getSystemService(WINDOW_SERVICE);
			wm.addView(mLayout, previewLayoutParams);
		} catch (Exception e) {
			e.printStackTrace();
		}
		
	}

	@Override
	public void onProviderDisabled(String provider) {
		Log.w(LOG, "onProviderDisabled");
		if(mLayout != null) {
			mPreview.stop();
			((WindowManager) getSystemService(WINDOW_SERVICE)).removeView(mLayout);
			mLayout = null;
		}
	}
}
