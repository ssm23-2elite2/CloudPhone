#include "StdAfx.h"
#include "MyDeviceMgr.h"


CMyDeviceMgr::CMyDeviceMgr(void)
{
}


CMyDeviceMgr::~CMyDeviceMgr(void)
{
	POSITION pos = m_deviceList.GetHeadPosition();
	while(pos)
	{
		CDeviceInfo *pDevice = (CDeviceInfo *)m_deviceList.GetNext(pos);
		delete pDevice;
	}
	m_deviceList.RemoveAll();
}


void CMyDeviceMgr::InsertDevice(CDeviceInfo * pDevice)
{
	m_deviceList.AddTail((void *)pDevice);
}


//헤드에 있는 디바이스 정보 추출
CDeviceInfo * CMyDeviceMgr::GetDeviceInfoFromList(void)
{
	POSITION pos = m_deviceList.GetHeadPosition();

	if(pos == NULL)
		return NULL;

	return (CDeviceInfo *)m_deviceList.GetAt(pos);
}


CMyDeviceMgr * CMyDeviceMgr::GetDeviceMgrFromPayload(Json::Value &jsonPayload)
{
	CMyDeviceMgr *pDeviceList = new CMyDeviceMgr;
		
	for(int i=0; i<jsonPayload.size(); i++)
	{
		pDeviceList->InsertDevice(CDeviceInfo::FromJson(jsonPayload[i]));
	}
	
	return pDeviceList;
}
