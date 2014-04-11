package org.secmem232.cloudphone.network;

public interface PacketListener {
	public void onPacketReceived(Packet packet);
	public void onInterrupt();
}