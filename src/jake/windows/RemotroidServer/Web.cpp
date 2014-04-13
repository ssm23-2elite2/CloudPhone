#include "StdAfx.h"
#include "Web.h"
#include "WebRequest.h"
#include "Account.h"

CWeb::CWeb(void)
{
}


CWeb::~CWeb(void)
{
}


CResponse * CWeb::RequestConnection(CDeviceInfo * pDeviceInfo, CAccount * pAccount, CString strServerIP)
{
	CWebRequest webRequest; 
	webRequest.SetRequestObject(_T("/apis/device/wakeup"));
	return webRequest.AttachPayload(pDeviceInfo, pAccount, strServerIP)->SendRequest();
}


CResponse * CWeb::DoLogin(CString strEmail, CString strPasswd)
{
	CWebRequest webRequest; 
	webRequest.SetRequestObject(_T("/apis/account/login"));
	CAccount account(strEmail, strPasswd);	

	return webRequest.AttachPayload(account)->SendRequest();	
}


CResponse * CWeb::GetDeviceList(CAccount * pAccount)
{
	CWebRequest webRequest; 
	webRequest.SetRequestObject(_T("/apis/device/list"));

	return webRequest.AttachPayload(*pAccount)->SendRequest();	
}


BOOL CWeb::GetErrorMsg(CResponse * pResponse)
{
	if(pResponse == NULL)
		return FALSE;

	switch(pResponse->errorCode)
	{	
	case 0:
		AfxMessageBox(_T("서버와 접속이 원활하지 않습니다"));
		return  FALSE;
	case AUTHENTICATION_FAILED:
		AfxMessageBox(_T("등록된 계정이 없습니다"));
		return FALSE;
	case NO_DEVICE:
		AfxMessageBox(_T("등록된 장치가 없습니다"));
		return FALSE;	
	}
	return TRUE;
}

