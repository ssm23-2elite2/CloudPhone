// CloudPhoneDll.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include <Windows.h>
#include <winioctl.h>
#include <stdio.h>
#include <conio.h>
#include <iostream>
#include <fstream>

#define IOCTL_IMAGE	CTL_CODE(FILE_DEVICE_UNKNOWN,0x4000,METHOD_BUFFERED,FILE_ANY_ACCESS)

using std::ofstream;

extern "C" __declspec(dllexport)
int DisplayWebCam(BYTE *buf, int length)
{
	HANDLE hdevice = CreateFile(TEXT("\\\\.\\cloudphone"), 
		GENERIC_READ | GENERIC_WRITE, 
		0, 
		NULL, 
		OPEN_EXISTING, 
		0, 
		NULL
		);
	if (hdevice == INVALID_HANDLE_VALUE)
	{
		printf("Unable to open UsbcameraFilter device - error %d\n",
			GetLastError());
		return 1;
	}

	DWORD dwRet;
	if (!DeviceIoControl(hdevice, IOCTL_IMAGE, NULL, 0, buf, sizeof(buf), &dwRet, NULL))
	{
		printf("DeviceIOControl Fail!! \n");
		_getch();
		CloseHandle(hdevice);
		return 1;
	}

	CloseHandle(hdevice);
	
	return 0;
}

