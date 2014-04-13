#pragma once

#define INIFILEPATH	_T(".\\history.ini")
#define MAXHISTORY	10

class CINIMgr
{
public:
	CINIMgr(void);
	~CINIMgr(void);
	
private:
	int m_iCount;

public:
	int GetCount(void);
	CString GetHistoryString(int);	
	void InitialINIFile(void);
	void WriteString(CString str);
private:
	int m_iCurrent;
};

