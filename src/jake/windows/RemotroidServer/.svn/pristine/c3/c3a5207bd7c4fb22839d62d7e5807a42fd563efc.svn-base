#pragma once
#include "json/json.h"


class CDeviceInfo
{
public:
	CDeviceInfo(void);
	~CDeviceInfo(void);
private:
	CString m_strUUID;
	CString m_strNickname;
	CString m_strRegKey;
public:
	void SetUUID(CString strUUID);
	void SetNickname(CString strNickname);
	void SetRegKey(CString strRegKey);

	CString GetUUID();
	CString GetNickname();
	CString GetRegKey();

	
	static CDeviceInfo * FromJson(Json::Value& jsonPayload);
};

