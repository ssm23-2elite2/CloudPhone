#pragma once

#include "stdafx.h"

//파일 송수신 상태
typedef enum FILETRANCEIVERSTATE
{
	NORMAL, SENDING, RECEIVEING, READYRECV
};

interface IFileTranceiverListener
{
public:
	virtual void SetFileTranceiverState(FILETRANCEIVERSTATE state) = 0;
	virtual void StartFileTranceiver(BOOL cond) = 0;
};

