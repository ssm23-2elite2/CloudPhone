#pragma once


#include "Response.h"

#define AUTHENTICATION_FAILED	257
#define NO_DEVICE				513


class CWeb
{
public:
	CWeb(void);
	virtual ~CWeb(void);
	static CResponse * DoLogin(CString strEmail, CString strPasswd);
	static CResponse * GetDeviceList(CAccount * pAccount);
	static BOOL GetErrorMsg(CResponse * pResponse);
	static CResponse * RequestConnection(CDeviceInfo * pDeviceInfo, CAccount * pAccount, CString strServerIP);
};

