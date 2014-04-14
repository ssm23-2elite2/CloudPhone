package org.secmem232.cloudphone;

import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.util.List;

import android.annotation.SuppressLint;
import android.content.Context;
import android.content.res.Configuration;
import android.graphics.ImageFormat;
import android.graphics.Rect;
import android.graphics.YuvImage;
import android.hardware.Camera;
import android.hardware.Camera.PreviewCallback;
import android.hardware.Camera.Size;
import android.os.Build;
import android.util.Log;
import android.view.Display;
import android.view.Surface;
import android.view.SurfaceHolder;
import android.view.SurfaceView;
import android.view.WindowManager;
import android.widget.FrameLayout;
import android.widget.Toast;

/**
 * This class assumes the parent layout is RelativeLayout.LayoutParams.
 */
@SuppressLint("NewApi")
public class CameraPreview extends SurfaceView implements SurfaceHolder.Callback {
	private static boolean DEBUGGING = true;
	private static final String LOG_TAG = "CameraPreview";
	private static final String CAMERA_PARAM_ORIENTATION = "orientation";
	private static final String CAMERA_PARAM_LANDSCAPE = "landscape";
	private static final String CAMERA_PARAM_PORTRAIT = "portrait";
	protected Context mContext;
	private SurfaceHolder mHolder;
	protected Camera mCamera;
	protected List<Camera.Size> mPreviewSizeList;
	protected List<Camera.Size> mPictureSizeList;
	protected Camera.Size mPreviewSize;
	protected Camera.Size mPictureSize;
	private int mSurfaceChangedCallDepth = 0;
	private int mCameraId;
	private LayoutMode mLayoutMode;
	private int mCenterPosX = -1;
	private int mCenterPosY;
	
	private PreviewReadyCallback mPreviewReadyCallback = null;
	private CameraPreviewListener mCameraPreviewListener = null;
	
	private final String LOG = "CameraPreview";
	public static enum LayoutMode {
		FitToParent, // Scale to the size that no side is larger than the parent
		NoBlank // Scale to the size that no side is smaller than the parent
	};

	public interface PreviewReadyCallback {
		public void onPreviewReady();
	}
	
	public interface CameraPreviewListener {
		public void onPreview(byte[] image);
	}


	/**
	 * State flag: true when surface's layout size is set and surfaceChanged()
	 * process has not been completed.
	 */
	protected boolean mSurfaceConfiguring = false;

	@SuppressLint("NewApi")
	public CameraPreview(Context context, int cameraId, LayoutMode mode) {
		super(context); // Always necessary
		Log.w(LOG, "cameraId :" + cameraId);
		mContext = context;
		mLayoutMode = mode;
		mHolder = getHolder();
		mHolder.addCallback(this);
		mHolder.setType(SurfaceHolder.SURFACE_TYPE_PUSH_BUFFERS);
		
		if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.GINGERBREAD) {
			if (Camera.getNumberOfCameras() > cameraId) {
				mCameraId = cameraId;
			} else {
				mCameraId = 0;
			}
		} else {
			mCameraId = 0;
		}

		if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.GINGERBREAD) {
			mCamera = Camera.open(mCameraId);
		} else {
			mCamera = Camera.open();
		}
		Camera.Parameters cameraParams = mCamera.getParameters();
		mPreviewSizeList = cameraParams.getSupportedPreviewSizes();
		mPictureSizeList = cameraParams.getSupportedPictureSizes();
	}

	@Override
	public void surfaceCreated(SurfaceHolder holder) {
		Log.w(LOG, "surfaceCreated");
		try {
			mCamera.setPreviewDisplay(holder);
			mCamera.setPreviewCallback(mPreviewCallBack);
			mCamera.setOneShotPreviewCallback(mPreviewCallBack);
			mCamera.startPreview();
		} catch (Exception e) {
			mCamera.setPreviewCallback(null);
			mCamera.release();
			mCamera = null;
		}
	}

	@Override
	public void surfaceChanged(SurfaceHolder holder, int format, int width, int height) {
		Log.w(LOG, "surfaceChanged");
		mSurfaceChangedCallDepth++;
		doSurfaceChanged(width, height);
		mSurfaceChangedCallDepth--;
	}

	private void doSurfaceChanged(int width, int height) {
		Log.i(LOG, "doSurfaceChanged");
		mCamera.stopPreview();

		Camera.Parameters cameraParams = mCamera.getParameters();
		boolean portrait = isPortrait();

		// The code in this if-statement is prevented from executed again when surfaceChanged is
		// called again due to the change of the layout size in this if-statement.
		if (!mSurfaceConfiguring) {
			Camera.Size previewSize = determinePreviewSize(portrait, width, height);
			Camera.Size pictureSize = determinePictureSize(previewSize);
			if (DEBUGGING) { 
				Log.v(LOG_TAG, "Desired Preview Size - w: " + width + ", h: " + height); 
			}

			mPreviewSize = previewSize;
			mPictureSize = pictureSize;
			mSurfaceConfiguring = adjustSurfaceLayoutSize(previewSize, portrait, width, height);

			if (mSurfaceConfiguring && (mSurfaceChangedCallDepth <= 1)) {
				return;
			}
		}

		configureCameraParameters(cameraParams, portrait);
		mSurfaceConfiguring = false;

		try {
			mCamera.setPreviewCallback(mPreviewCallBack);
			mCamera.setOneShotPreviewCallback(mPreviewCallBack);
			
			mCamera.startPreview();
		} catch (Exception e) {
			Log.w(LOG_TAG, "Failed to start preview: " + e.getMessage());

			// Remove failed size
			mPreviewSizeList.remove(mPreviewSize);
			mPreviewSize = null;

			// Reconfigure
			if (mPreviewSizeList.size() > 0) { // prevent infinite loop
				surfaceChanged(null, 0, width, height);
			} else {
				Toast.makeText(mContext, "Can't start preview", Toast.LENGTH_LONG).show();
				Log.w(LOG_TAG, "Gave up starting preview");
			}
		}

		if (null != mPreviewReadyCallback) {
			mPreviewReadyCallback.onPreviewReady();
		}
	}

	/**
	 * @param cameraParams
	 * @param portrait
	 * @param reqWidth must be the value of the parameter passed in surfaceChanged
	 * @param reqHeight must be the value of the parameter passed in surfaceChanged
	 * @return Camera.Size object that is an element of the list returned from Camera.Parameters.getSupportedPreviewSizes.
	 */
	protected Camera.Size determinePreviewSize(boolean portrait, int reqWidth, int reqHeight) {
		// Meaning of width and height is switched for preview when portrait,
		// while it is the same as user's view for surface and metrics.
		// That is, width must always be larger than height for setPreviewSize.
		int reqPreviewWidth; // requested width in terms of camera hardware
		int reqPreviewHeight; // requested height in terms of camera hardware
		if (portrait) {
			reqPreviewWidth = reqHeight;
			reqPreviewHeight = reqWidth;
		} else {
			reqPreviewWidth = reqWidth;
			reqPreviewHeight = reqHeight;
		}

		// Adjust surface size with the closest aspect-ratio
		float reqRatio = ((float) reqPreviewWidth) / reqPreviewHeight;
		float curRatio, deltaRatio;
		float deltaRatioMin = Float.MAX_VALUE;
		Camera.Size retSize = null;
		for (Camera.Size size : mPreviewSizeList) {
			curRatio = ((float) size.width) / size.height;
			deltaRatio = Math.abs(reqRatio - curRatio);
			if (deltaRatio < deltaRatioMin) {
				deltaRatioMin = deltaRatio;
				retSize = size;
			}
		}

		return retSize;
	}

	protected Camera.Size determinePictureSize(Camera.Size previewSize) {
		Camera.Size retSize = null;
		for (Camera.Size size : mPictureSizeList) {
			if (size.equals(previewSize)) {
				return size;
			}
		}

		if (DEBUGGING) { Log.v(LOG_TAG, "Same picture size not found."); }

		// if the preview size is not supported as a picture size
		float reqRatio = ((float) previewSize.width) / previewSize.height;
		float curRatio, deltaRatio;
		float deltaRatioMin = Float.MAX_VALUE;
		for (Camera.Size size : mPictureSizeList) {
			curRatio = ((float) size.width) / size.height;
			deltaRatio = Math.abs(reqRatio - curRatio);
			if (deltaRatio < deltaRatioMin) {
				deltaRatioMin = deltaRatio;
				retSize = size;
			}
		}

		return retSize;
	}


	protected boolean adjustSurfaceLayoutSize(Camera.Size previewSize, boolean portrait,
			int availableWidth, int availableHeight) {
		float tmpLayoutHeight, tmpLayoutWidth;
		if (portrait) {
			tmpLayoutHeight = previewSize.width;
			tmpLayoutWidth = previewSize.height;
		} else {
			tmpLayoutHeight = previewSize.height;
			tmpLayoutWidth = previewSize.width;
		}

		float factH, factW, fact;
		factH = availableHeight / tmpLayoutHeight;
		factW = availableWidth / tmpLayoutWidth;
		if (mLayoutMode == LayoutMode.FitToParent) {
			// Select smaller factor, because the surface cannot be set to the size larger than display metrics.
			if (factH < factW) {
				fact = factH;
			} else {
				fact = factW;
			}
		} else {
			if (factH < factW) {
				fact = factW;
			} else {
				fact = factH;
			}
		}

		FrameLayout.LayoutParams layoutParams = (FrameLayout.LayoutParams)this.getLayoutParams();

		int layoutHeight = (int) (tmpLayoutHeight * fact);
		int layoutWidth = (int) (tmpLayoutWidth * fact);
		if (DEBUGGING) {
			Log.v(LOG_TAG, "Preview Layout Size - w: " + layoutWidth + ", h: " + layoutHeight);
			Log.v(LOG_TAG, "Scale factor: " + fact);
		}

		boolean layoutChanged;
		if ((layoutWidth != this.getWidth()) || (layoutHeight != this.getHeight())) {
			layoutParams.height = layoutHeight;
			layoutParams.width = layoutWidth;
			if (mCenterPosX >= 0) {
				layoutParams.topMargin = mCenterPosY - (layoutHeight / 2);
				layoutParams.leftMargin = mCenterPosX - (layoutWidth / 2);
			}
			this.setLayoutParams(layoutParams); // this will trigger another surfaceChanged invocation.
			layoutChanged = true;
		} else {
			layoutChanged = false;
		}

		return layoutChanged;
	}

	/**
	 * @param x X coordinate of center position on the screen. Set to negative value to unset.
	 * @param y Y coordinate of center position on the screen.
	 */
	public void setCenterPosition(int x, int y) {
		mCenterPosX = x;
		mCenterPosY = y;
	}

	protected void configureCameraParameters(Camera.Parameters cameraParams, boolean portrait) {
		if (Build.VERSION.SDK_INT < Build.VERSION_CODES.FROYO) { // for 2.1 and before
			if (portrait) {
				cameraParams.set(CAMERA_PARAM_ORIENTATION, CAMERA_PARAM_PORTRAIT);
			} else {
				cameraParams.set(CAMERA_PARAM_ORIENTATION, CAMERA_PARAM_LANDSCAPE);
			}
		} else { // for 2.2 and later
			int angle;
			Display display = ((WindowManager) mContext.getSystemService(Context.WINDOW_SERVICE)).getDefaultDisplay();
			switch (display.getRotation()) {
			case Surface.ROTATION_0: // This is display orientation
				angle = 90; // This is camera orientationc
				break;
			case Surface.ROTATION_90:
				angle = 0;
				break;
			case Surface.ROTATION_180:
				angle = 270;
				break;
			case Surface.ROTATION_270:
				angle = 180;
				break;
			default:
				angle = 90;
				break;
			}
			Log.v(LOG_TAG, "angle: " + angle);
			mCamera.setDisplayOrientation(angle);
		}

		cameraParams.setPreviewSize(mPreviewSize.width, mPreviewSize.height);
		cameraParams.setPictureSize(mPictureSize.width, mPictureSize.height);
		if (DEBUGGING) {
			Log.v(LOG_TAG, "Preview Actual Size - w: " + mPreviewSize.width + ", h: " + mPreviewSize.height);
			Log.v(LOG_TAG, "Picture Actual Size - w: " + mPictureSize.width + ", h: " + mPictureSize.height);
		}

		mCamera.setParameters(cameraParams);
	}

	@Override
	public void surfaceDestroyed(SurfaceHolder holder) {
		stop();
	}

	public void stop() {
		if (null == mCamera) {
			return;
		}
		mCamera.stopPreview();
		mCamera.setPreviewCallback(null);
		mCamera.release();
		mCamera = null;
	}

	public boolean isPortrait() {
		return (mContext.getResources().getConfiguration().orientation == Configuration.ORIENTATION_PORTRAIT);
	}

	
	private Camera.PreviewCallback mPreviewCallBack = new PreviewCallback() {
		@Override
		public void onPreviewFrame(byte[] data, Camera camera) {
			Log.w(LOG, "onPreviewFrame");
			// Yuv to JPEG
			
			Camera.Parameters parameters = camera.getParameters();
			Size size = parameters.getPreviewSize();
			YuvImage image = new YuvImage(data, ImageFormat.NV21,
					size.width, size.height, null);
			Rect rectangle = new Rect();
			rectangle.bottom = size.height;
			rectangle.top = 0;
			rectangle.left = 0;
			rectangle.right = size.width;
			ByteArrayOutputStream out = new ByteArrayOutputStream();
			image.compressToJpeg(rectangle, 50, out);
			
			if(mCameraPreviewListener != null) {
				mCameraPreviewListener.onPreview(out.toByteArray());
				camera.setPreviewCallback(this);
			}
		}
	};
	
	public Camera.Size getPreviewSize() {
		return mPreviewSize;
	}

	public void setOnPreviewReady(PreviewReadyCallback cb) {
		mPreviewReadyCallback = cb;
	}
	
	public void setCameraPreviewListener(CameraPreviewListener listener) { 
		mCameraPreviewListener = listener;
	}
}
