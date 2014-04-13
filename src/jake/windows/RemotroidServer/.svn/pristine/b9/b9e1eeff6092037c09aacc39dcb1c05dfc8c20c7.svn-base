#include "StdAfx.h"
#include "INIMgr.h"


CINIMgr::CINIMgr(void)
	: m_iCount(0)
	, m_iCurrent(0)
{
}


CINIMgr::~CINIMgr(void)
{
}


void CINIMgr::InitialINIFile(void)
{
	CFileFind find;
	BOOL bRet = find.FindFile(INIFILEPATH);

	if(!bRet)		//존재하지 않을경우
	{
		CFile file;
		CFileException fileException;

		if (!file.Open(INIFILEPATH, CFile::modeCreate|CFile::modeWrite, &fileException))
			return;		
		
		file.Close();
		WritePrivateProfileString(_T("COUNT"), _T("count"), _T("0"), INIFILEPATH);
		WritePrivateProfileString(_T("COUNT"), _T("current"), _T("0"), INIFILEPATH);
		m_iCount = 0;
	}
	else
	{
		TCHAR strCount[255];
		GetPrivateProfileString(_T("COUNT"), _T("count"), _T("0"), strCount, 255, INIFILEPATH);
		m_iCount = _ttoi(strCount);
		GetPrivateProfileString(_T("COUNT"), _T("current"), _T("0"), strCount, 255, INIFILEPATH);
		m_iCurrent = _ttoi(strCount);
	}
}


int CINIMgr::GetCount(void)
{
	return m_iCount;
}


CString CINIMgr::GetHistoryString(int num)
{
	TCHAR str[4096];
	TCHAR strCount[255];
	_itow(num, strCount, 10);
	GetPrivateProfileString(_T("EMAIL"), strCount, _T(""), str, 4096, INIFILEPATH);
	return str; 
}




void CINIMgr::WriteString(CString str)
{		
	TCHAR strCount[255];
	_itow(m_iCurrent, strCount, 10);
	WritePrivateProfileString(_T("EMAIL"), strCount, str, INIFILEPATH);
	m_iCount++;
	m_iCurrent++;

	if(m_iCount >= MAXHISTORY)
	{
		m_iCount=MAXHISTORY;
		m_iCurrent = m_iCurrent % MAXHISTORY;
	}
	
	_itow(m_iCount, strCount, 10);
	WritePrivateProfileString(_T("COUNT"), _T("count"), strCount, INIFILEPATH);		
	_itow(m_iCurrent, strCount, 10);
	WritePrivateProfileString(_T("COUNT"), _T("current"), strCount, INIFILEPATH);		
}
