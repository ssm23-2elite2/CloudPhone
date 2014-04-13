
#include "StdAfx.h"
#include "RecvFile.h"



CRecvFile::CRecvFile(void):m_hRecvFile(NULL)
	, m_iTotalFileSize(0)
	, m_iRecvFileSize(0)
	, pProgressBar(NULL)
{
	
}


CRecvFile::~CRecvFile(void)
{
	CloseFileHandle();
}

void CRecvFile::SetListener(IFileTranceiverListener *listener)
{
	m_Listener = listener;
}



//파일 정보(이름, 크기) 수신시..
HANDLE CRecvFile::RecvFileInfo(char * data)
{
	char bFileName[FILENAMESIZE+1];
	memset(bFileName, 0, sizeof(bFileName));
	memcpy(bFileName, data, FILENAMESIZE);
	//상위 100 바이트에서 파일 이름 추출

	char bFileSize[FILESIZESIZE+1];
	memset(bFileSize, 0, sizeof(bFileSize));
	memcpy(bFileSize, data+FILENAMESIZE, FILESIZESIZE);
	//하위 100바이트에서 파일 크기 추출

	m_iTotalFileSize = atoll(bFileSize);
	//앞으로 받을 파일 크기
	memset(m_uniFileName, 0, sizeof(m_uniFileName));
	CUtil::UtfToUni(m_uniFileName, bFileName);	

	CreateDirectory(directoryPath, NULL);

	memset(fullFilePath, 0, MAX_PATH);
	
	_tcscpy(fullFilePath, directoryPath);
	_tcscat(fullFilePath, m_uniFileName);

	//해당 경로에 이미 같은 이름의 파일이 있을 경우 처리
	int index = 1;
	CString strIndex, temp, name, ext;
	while(PathFileExists(fullFilePath))
	{
		temp = m_uniFileName;		
		AfxExtractSubString(name, temp, 0, '.');
		AfxExtractSubString(ext, temp, 1, '.');
		strIndex.Format(_T("-%d"), index);
		temp = name+strIndex+_T(".")+ext;
		index++;

		_tcscpy(fullFilePath, directoryPath);
		_tcscat(fullFilePath, temp);
	}
	
	if(m_hRecvFile)
	{
		CloseHandle(m_hRecvFile);
	}
	m_hRecvFile = CreateFile(fullFilePath, GENERIC_WRITE, NULL, NULL, CREATE_ALWAYS,
		FILE_ATTRIBUTE_NORMAL, NULL);

	if(m_hRecvFile == INVALID_HANDLE_VALUE)
	{		
		AfxMessageBox(_T("현재 위치에 파일을 저장할 수 없습니다"));
	}
	else
	{
		pProgressBar->SetPos(0);
		m_Listener->StartFileTranceiver(TRUE);
	}

	m_iRecvFileSize = 0;
	//현재 받은 파일 크기
	return m_hRecvFile;
}


//파일 본문 내용
void CRecvFile::RecvFileData(char * data, int packetSize)
{
	int iCurrentRecvLen = packetSize - HEADERSIZE;

	DWORD dwWrite;
	if(m_hRecvFile == NULL)
		return;

	WriteFile(m_hRecvFile, data, iCurrentRecvLen, &dwWrite, NULL);
	m_iRecvFileSize += iCurrentRecvLen;
	
	int percent = (int)(((float)m_iRecvFileSize/m_iTotalFileSize)*100);
	
	pProgressBar->SetPos(percent);		

	if(m_iTotalFileSize <= m_iRecvFileSize)
	{		
		m_Listener->StartFileTranceiver(FALSE);
		CloseHandle(m_hRecvFile);
		m_hRecvFile = NULL;		
	}
}


unsigned long long CRecvFile::atoll(char * str)
{
	unsigned long long rVal = 0;
	int sign = 1;

	while (*str && (*str == ' ' || *str == '\t')) str++;
	if (*str == NULL) return(0);

	// 부호 처리
	if (*str == '+' || *str == '-') {
		sign = (*str++ == '+') ? 1 : -1;
	}

	// 정수값 추출
	while (*str && *str >= '0' && *str <= '9') {
		rVal = rVal * 10 + *str++ - '0';
	}

	return(rVal * sign);
}


void CRecvFile::CloseFileHandle(void)
{
	if(m_hRecvFile != NULL)
	{
		CloseHandle(m_hRecvFile);
		DeleteFile(fullFilePath);		
		m_hRecvFile = NULL;
		m_Listener->StartFileTranceiver(FALSE);
	}
}

void CRecvFile::SetFilePath(TCHAR * path)
{	
	_tcscpy(directoryPath, path);

	if(directoryPath[_tcslen(directoryPath)-1] != _T('\\'))
	{
		_tcscat(directoryPath, _T("\\"));
	}	
}


void CRecvFile::SetDefaultPath(void)
{
	//현재 바탕화면의 절대경로 얻어오기
	SHGetFolderPath(NULL, CSIDL_DESKTOP, NULL, 0, directoryPath);
	_tcscat(directoryPath, _T("\\Remotroid\\"));	
}


void CRecvFile::SetProgressBar(CTextProgressCtrl * pProgressBar)
{
	this->pProgressBar = pProgressBar;
}
