#pragma once


class CMyClient
{
public:
	CMyClient(SOCKET clientSocket);
	~CMyClient(void);
private:
	int m_iCurrent;
	char m_RecvBuffer[MAXSIZE*2];	
	SOCKET m_ClientSocket;
public:
	int RecvPacket(void);
	BOOL GetPacket(char * packet);
	int SendPacket(int iOPCode, const char * data, int iDataLen);
	void CloseSocket(void);
	void SetNoDelay(BOOL bOp);
	void ResetBuffer(void);
};

