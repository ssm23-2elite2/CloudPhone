#pragma once
#include "afxcoll.h"
#include "DeviceInfo.h"
#include "json/json.h"

class CMyDeviceMgr
{
public:
	CMyDeviceMgr(void);
	~CMyDeviceMgr(void);
private:
	CPtrList m_deviceList;
public:
	void InsertDevice(CDeviceInfo * pDevice);
	CDeviceInfo * GetDeviceInfoFromList(void);
	static CMyDeviceMgr * GetDeviceMgrFromPayload(Json::Value &jsonPayload);
};

