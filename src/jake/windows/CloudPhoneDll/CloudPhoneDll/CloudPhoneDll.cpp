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
	/*
	ofstream outFile("hello.jpg", std::ios::out | std::ios::binary);
	outFile.write((const char*)buf, length);
	outFile.close();

	return 0;
	*/
	
	HANDLE hDv;
	WCHAR DeviceLink[] = L"\\\\.\\cloudphone";
	DWORD dwRet;

	hDv = CreateFileW(
		DeviceLink,
		GENERIC_READ | GENERIC_WRITE,
		0,
		NULL,
		OPEN_EXISTING,
		FILE_ATTRIBUTE_NORMAL,
		NULL
		);

	if (hDv == INVALID_HANDLE_VALUE)
	{
		printf("Get Device Handle Fail! : 0x%X \n", GetLastError());
		return -3;
	}

	if (!DeviceIoControl(hDv, IOCTL_IMAGE, 0, 0, buf, length, &dwRet, 0))
	{
		printf("DeviceIOControl Fail!! \n");
		_getch();
		CloseHandle(hDv);
		return 1;
	}

	CloseHandle(hDv);
	return 0;
}

