package org.secmem232.cloudphone.util;

import java.util.List;

import org.secmem232.cloudphone.CloudPhoneService;

import android.app.ActivityManager;
import android.app.ActivityManager.RunningServiceInfo;
import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.util.Log;


public class Util {
	private final static String LOG = "Util";
	public static class Services{
		public static boolean isServiceAliveU(Context context){
			Log.d(LOG, "isServiceAliveU");
			String serviceCls = CloudPhoneService.class.getName();
			ActivityManager manager = (ActivityManager)context.getSystemService(Context.ACTIVITY_SERVICE);
			List<RunningServiceInfo> serviceList = manager.getRunningServices(Integer.MAX_VALUE);
			for(RunningServiceInfo info : serviceList){
				if(info.service.getClassName().equals(serviceCls)){
					return true;
				}
			}
			return false;
		}
		
		public static ComponentName startPassUService(Context context){
			Log.d(LOG, "startPassUService");
			if(!isServiceAliveU(context)){
				Intent intent = new Intent(context, CloudPhoneService.class);
				return context.startService(intent);
			} else {
				return null;
			}
		}
	}
	
}
