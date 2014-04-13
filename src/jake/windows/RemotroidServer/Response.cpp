#include "StdAfx.h"
#include "Response.h"


CResponse::CResponse(void)
{
}


CResponse::~CResponse(void)
{
}

//json으로 부터 데이터를 파싱한다
BOOL CResponse::ParseFromStr(CString strResponse)
{
	//유니코드를 ansi로 변경한다

	char *jsonStr = CUtil::UniToAnsi(strResponse);
	

	Json::Reader jReader;
	Json::Value jsonRoot;
 
	BOOL bOK = jReader.parse(jsonStr, jsonRoot);

		
	if (!bOK)
	{
		TCHAR temp[1024];
		wsprintf(temp, _T("%s"), jReader.getFormatedErrorMessages());
		AfxMessageBox(temp);
		return FALSE;
	}
	
	//result = _ttoi(CString(jsonRoot.get("result", "-1").asCString()));
	result = jsonRoot.get("result", -1).asInt();
	

	if(result == -1)
	{
		errorCode = jsonRoot.get("errorCode", 0).asInt();
	}
	else
	{
		jsonPaylod = jsonRoot["data"];
	}
	
	delete jsonStr;
	return TRUE;
}


CAccount * CResponse::GetAccountFromPayload(void)
{
	CAccount *account = CAccount::FromJson(jsonPaylod);
	return account;
}


CMyDeviceMgr * CResponse::GetDeviceListFromPayload(void)
{
	CMyDeviceMgr *pDeviceList = CMyDeviceMgr::GetDeviceMgrFromPayload(jsonPaylod);
	return pDeviceList;
}
