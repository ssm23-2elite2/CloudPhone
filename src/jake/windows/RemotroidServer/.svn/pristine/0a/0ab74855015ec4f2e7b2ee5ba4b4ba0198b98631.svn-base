#pragma once


#include "json/json.h"
#include "Account.h"
#include "MyDeviceMgr.h"

class CResponse
{
public:
	CResponse(void);
	~CResponse(void);

	int result;
	int errorCode;	

//private:
	Json::Value jsonPaylod;
public:
	BOOL ParseFromStr(CString strResponse);
	CAccount * GetAccountFromPayload(void);
	CMyDeviceMgr * GetDeviceListFromPayload(void);
};

