#include "StdAfx.h"
#include "FileSender.h"


CFileSender::CFileSender()
	: totalFileSize(0), m_pClient(NULL),
	sendedFileSize(0),isSending(FALSE)	
	, pSendFileThread(NULL)
	, m_progressCtrl(NULL)
	, m_Listener(NULL)
	, m_bCond(FALSE)
{
	memset(buffer, 0, sizeof(buffer));
}


CFileSender::~CFileSender(void)
{	
	DeleteFileList();	
}

void CFileSender::SetClient(CMyClient *pClient)
{
	m_pClient = pClient;
}

int CFileSender::SendPacket(int iOPCode, const char * data, int iDataLen)
{
	if(m_pClient == NULL)
		return -1;

	return m_pClient->SendPacket(iOPCode,data,iDataLen);
}


void CFileSender::AddSendFile(CFile * pFile)
{
	sendFileList.AddTail((void*)pFile);		
	return;
}


int CFileSender::StartSendFile(void)
{
	return SendFileInfo();
}

int CFileSender::SendFileInfo()
{	
	if(sendFileList.IsEmpty())
	{			
		m_Listener->SetFileTranceiverState(NORMAL);
		return -1;		
	}

	CFile *pFile = (CFile *)sendFileList.GetHead();
	
	totalFileSize = pFile->GetLength();
	CString fileName = pFile->GetFileName();

	//유니코드 형식의 파일 이름을 UTF-8로 변환
	TCHAR uniFileName[FILENAMESIZE];
	char utfFileName[FILENAMESIZE];	
	memset(uniFileName, 0, sizeof(uniFileName));
	memset(utfFileName, 0, sizeof(utfFileName));
	_tcscpy(uniFileName, fileName);
	CUtil::UniToUtf(uniFileName, utfFileName);

	//프로토콜에 맞춰서 상위 100바이트는 파일 이름 이후 100바이트는 파일 크기	
	memset(buffer, 0, sizeof(buffer));
	memcpy(buffer, utfFileName, FILENAMESIZE);
	sprintf(buffer+FILENAMESIZE, "%100llu", totalFileSize);

	sendedFileSize = 0;
	return SendPacket(OP_SENDFILEINFO, buffer, FILENAMESIZE+FILESIZESIZE);
}

void CFileSender::SendFileData()
{
	if(pSendFileThread != NULL)
	{
		delete pSendFileThread;
	}
	m_bCond = TRUE;
	pSendFileThread = AfxBeginThread(SendFileThread, this);
	pSendFileThread->m_bAutoDelete = FALSE;
}



UINT CFileSender::SendFileThread(LPVOID pParam)
{	
	CFileSender *pDlg = (CFileSender *)pParam;	
	CFile *pFile = (CFile *)pDlg->sendFileList.RemoveHead();
	unsigned long long totalFileSize = pFile->GetLength();
	unsigned long long sendedFileSize = 0;

	pDlg->m_Listener->StartFileTranceiver(TRUE);
	
	while(totalFileSize > sendedFileSize)
	{
		int iCurrentSendSize = (totalFileSize-sendedFileSize) > MAXDATASIZE ? MAXDATASIZE : totalFileSize-sendedFileSize;		

		pFile->Read(pDlg->buffer, iCurrentSendSize);

		//접속이 종료되거나 X버튼 누를 경우
		if(pDlg->SendPacket(OP_SENDFILEDATA, pDlg->buffer, iCurrentSendSize) == SOCKET_ERROR || pDlg->m_bCond == FALSE)
		{				
			delete pFile;			
			pDlg->m_Listener->StartFileTranceiver(FALSE);
			pDlg->m_Listener->SetFileTranceiverState(NORMAL);
			return 0;
		}
		sendedFileSize += iCurrentSendSize;
		int percent = (int)(((float)sendedFileSize/totalFileSize)*100);		
		pDlg->m_progressCtrl->SetPos(percent);
	}
	pDlg->m_Listener->StartFileTranceiver(FALSE);

	delete pFile;
	pDlg->SendFileInfo();
	return 0;
}


void CFileSender::DeleteFileList(void)
{
	if(pSendFileThread != NULL)
	{		
		m_bCond = FALSE;
		while(TRUE)
		{
			DWORD dwResult = WaitForSingleObject(pSendFileThread->m_hThread, 100);				
			if(dwResult !=WAIT_TIMEOUT)
				break;			
			MSG msg;
			while (::PeekMessage(&msg, NULL, NULL, NULL,PM_REMOVE))
			{
				::TranslateMessage(&msg);
				::DispatchMessage(&msg);
			}
		}		
		delete pSendFileThread;		
		pSendFileThread = NULL;
	}

	POSITION pos = sendFileList.GetHeadPosition();
	
	while(pos)
	{	
		CFile *pFile = (CFile *)sendFileList.GetNext(pos);			
		delete pFile;				
	}	
	sendFileList.RemoveAll();		
	return;
}


void CFileSender::SetProgressBar(CTextProgressCtrl * pProgressCtrl)
{
	this->m_progressCtrl = pProgressCtrl;
}


void CFileSender::SetListener(IFileTranceiverListener *listener)
{
	m_Listener = listener;
}
