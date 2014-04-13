package org.secmem232.cloudphone.network;

public interface CameraSenderListener {
	public void onCameraSenderRequested();
	public void onCameraSenderStopRequested();
	public void onCameraSenderInterrupted();
}