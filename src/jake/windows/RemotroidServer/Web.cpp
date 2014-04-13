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
		AfxMessageBox(_T("������ ������ ��Ȱ���� �ʽ��ϴ�"));
		return  FALSE;
	case AUTHENTICATION_FAILED:
		AfxMessageBox(_T("��ϵ� ������ �����ϴ�"));
		return FALSE;
	case NO_DEVICE:
		AfxMessageBox(_T("��ϵ� ��ġ�� �����ϴ�"));
		return FALSE;	
	}
	return TRUE;
}

