package org.secmem232.cloudphone;
import java.util.List;

interface ICloudPhone{
	String getConnectionStatus();
	boolean isConnected();
	void connect(String ip, int port);
	void disconnect();	
}