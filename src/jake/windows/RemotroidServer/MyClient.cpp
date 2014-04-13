#include "StdAfx.h"
#include "MyClient.h"



CMyClient::CMyClient(SOCKET clientSocket)
	: m_iCurrent(0), m_ClientSocket(clientSocket)
{
	memset(m_RecvBuffer, 0, sizeof(m_RecvBuffer));
}


CMyClient::~CMyClient(void)
{
	CloseSocket();
}


int CMyClient::RecvPacket(void)
{		
	int iRecvLen = recv(m_ClientSocket, m_RecvBuffer+m_iCurrent, MAXSIZE, NULL);
	
	if(iRecvLen > 0)
	{
		m_iCurrent += iRecvLen;
	}
	return iRecvLen;
}


BOOL CMyClient::GetPacket(char * packet)
{
	if(m_iCurrent < HEADERSIZE)
		return FALSE;	

	int iPacketSize = CUtil::GetPacketSize(m_RecvBuffer);
	//헤더에서 패킷 싸이즈 얻기
	
	//수신받은 데이터가 패킷 싸이즈보다 작으면..
	if(iPacketSize > m_iCurrent)
		return FALSE;		

	memset(packet, 0, MAXSIZE);

	memcpy(packet, m_RecvBuffer, iPacketSize);
	m_iCurrent -= iPacketSize;
	memcpy(m_RecvBuffer, m_RecvBuffer+iPacketSize, m_iCurrent);
	
	return TRUE;
}


int CMyClient::SendPacket(int iOPCode, const char * data, int iDataLen)
{
	char packet[MAXSIZE];

	char bOPCode[OPCODESIZE+1];
	memset(bOPCode, 0, sizeof(bOPCode));
	sprintf(bOPCode, "%2d", iOPCode);

	char bPacketSize[TOTALSIZE+1];
	memset(bPacketSize, 0, sizeof(bPacketSize));
	int iPacketSize = iDataLen + HEADERSIZE;
	sprintf(bPacketSize, "%4d", iPacketSize);

	memset(packet, 0, sizeof(packet));
	memcpy(packet, bOPCode, OPCODESIZE);
	memcpy(packet+OPCODESIZE, bPacketSize, TOTALSIZE);
	if(data != NULL)
		memcpy(packet+HEADERSIZE, data, iDataLen);	
		
	g_CS.Lock();
	int iResult = send(m_ClientSocket, packet, iPacketSize, NULL);
	g_CS.Unlock();
	
	return iResult;
}


void CMyClient::CloseSocket(void)
{	
	closesocket(m_ClientSocket);
	m_ClientSocket = INVALID_SOCKET;	
}


void CMyClient::SetNoDelay(BOOL bOp)
{
	//마우스 무브시 빠르게 전송을 위해 네이글 알고리즘 비사용
	BOOL bOptVal = bOp;
	int bOptLen = sizeof(BOOL);
	int iOptLen = sizeof(int);
	setsockopt(m_ClientSocket, IPPROTO_TCP, TCP_NODELAY, (char*)&bOptVal, bOptLen);
}


void CMyClient::ResetBuffer(void)
{
	memset(m_RecvBuffer, 0, sizeof(m_RecvBuffer));
	m_iCurrent = 0;
}
