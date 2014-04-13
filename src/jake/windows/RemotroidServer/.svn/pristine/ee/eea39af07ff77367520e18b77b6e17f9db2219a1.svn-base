#pragma once
#include "TextProgressCtrl.h"
#include "IFileTranceiverListener.h"

class CRecvFile
{
public:
	CRecvFile(void);
	~CRecvFile(void);
private:
	HANDLE m_hRecvFile;
	TCHAR m_uniFileName[100];
	unsigned long long m_iTotalFileSize;	
	unsigned long long m_iRecvFileSize;

	TCHAR directoryPath[MAX_PATH];
	CTextProgressCtrl *pProgressBar;
	TCHAR fullFilePath[MAX_PATH];

public:
	HANDLE RecvFileInfo(char * data);
	void RecvFileData(char * data, int packetSize);
	unsigned long long atoll(char * str);
	void CloseFileHandle(void);	
	void SetFilePath(TCHAR * path);
	void SetDefaultPath(void);
	void SetProgressBar(CTextProgressCtrl * pProgressBar);
	void SetListener(IFileTranceiverListener *listener);
	IFileTranceiverListener *m_Listener;
};

