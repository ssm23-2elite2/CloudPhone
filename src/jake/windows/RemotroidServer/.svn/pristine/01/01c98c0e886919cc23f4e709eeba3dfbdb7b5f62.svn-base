#pragma once

#include <afxinet.h>
#include "Account.h"
#include "Response.h"

#define DEFAULTADDRESS	_T("remoteroid-server.appspot.com")
#define DEFAULTHEADER	_T("Content-Type : application/json; charset=utf-8\r\n")
#define PORTNUMBER		80

//웹서버와 POST 통신을 하는 클래스
class CWebRequest
{
public:
	CWebRequest(void);
	~CWebRequest(void);
	
	void SetRequestObject(CString strObject);
	CWebRequest * AttachPayload(CAccount& account);
	CResponse* SendRequest(void);

private:
	CString m_strObject;	
	CInternetSession m_Session;
	CHttpConnection *m_pHttpConnect;
	CHttpFile *m_pFile;
	CString m_strUrl;	
	CString m_strPayload;
	char *m_pPayload;
	INTERNET_PORT port;
	CString m_strHeader;
protected:
	void Disconnect(void);
public:
	CResponse * GetResponse(void);
	CWebRequest * AttachPayload(CDeviceInfo * pDeviceInfo, CAccount * pAccount, CString strServerIP);
};
