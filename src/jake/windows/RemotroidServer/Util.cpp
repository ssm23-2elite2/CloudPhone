#include "StdAfx.h"
#include "Util.h"


CUtil::CUtil(void)
{
}


CUtil::~CUtil(void)
{
}


int CUtil::GetOpCode(char * packet)
{
	char bOPCode[OPCODESIZE+1];
	memset(bOPCode, 0, sizeof(bOPCode));
	memcpy(bOPCode, packet, OPCODESIZE);
	return atoi(bOPCode);
}


int CUtil::GetPacketSize(char * packet)
{
	char bPacketSize[TOTALSIZE+1];
	memset(bPacketSize, 0, sizeof(bPacketSize));
	memcpy(bPacketSize, packet+OPCODESIZE, TOTALSIZE);
	return atoi(bPacketSize);
}


void CUtil::UniToUtf(TCHAR * uni, char * utf)
{
	int nLen = WideCharToMultiByte(CP_UTF8, 0, uni, _tcslen(uni)+1, NULL, NULL, NULL, NULL);	
	WideCharToMultiByte(CP_UTF8, 0, uni, _tcslen(uni)+1, utf, nLen, NULL, NULL);
}


void CUtil::UtfToUni(TCHAR * uni, char * utf)
{
	int nLen = MultiByteToWideChar(CP_UTF8, 0, utf, strlen(utf)+1, NULL, NULL);
	MultiByteToWideChar(CP_UTF8, 0, utf, strlen(utf)+1, uni, nLen);
}

TCHAR * CUtil::UtfToUniEx(const char * utf)
{
	int nLen = MultiByteToWideChar(CP_UTF8, 0, utf, strlen(utf)+1, NULL, NULL);
	TCHAR *uniTemp = new TCHAR[nLen+1];
	memset(uniTemp, 0, nLen+1);
	MultiByteToWideChar(CP_UTF8, 0, utf, strlen(utf)+1, uniTemp, nLen);
	return uniTemp;
}

char * CUtil::UniToUtfEx(const CString uni)
{
	int nLen = WideCharToMultiByte(CP_UTF8, 0, uni, _tcslen(uni)+1, NULL, NULL, NULL, NULL);
	char *utf = new char[nLen + 1];
	WideCharToMultiByte(CP_UTF8, 0, uni, _tcslen(uni)+1, utf, nLen, NULL, NULL);
	return utf;
}



char * CUtil::UniToAnsi(const CString uni)
{
	int nLen = WideCharToMultiByte(CP_ACP, 0, uni, _tcslen(uni)+1, NULL, NULL, NULL, NULL);
	char *ansi = new char[nLen+1];
	memset(ansi, 0, nLen+1);
	WideCharToMultiByte(CP_UTF8, 0, uni, _tcslen(uni)+1, ansi, nLen, NULL, NULL);
	return ansi;
}


TCHAR * CUtil::AnsiToUni(const char * ansi)
{
	int nLen = MultiByteToWideChar(CP_ACP, 0, ansi, strlen(ansi)+1, NULL, NULL);
	TCHAR *uni = new TCHAR[nLen+1];
	MultiByteToWideChar(CP_ACP, 0, ansi, strlen(ansi)+1, uni, nLen);
	return uni;
}


char *  CUtil::AnsiToUtf(const char *ansi)
{
	TCHAR *uni = AnsiToUni(ansi);
	char *utf = UniToUtfEx(uni);
	delete uni;
	return utf;
}


//클립보드에 있는 데이터를 UTF-8로 리턴해줌
char * CUtil::GetClipboardText(HWND hwnd)
{
	OpenClipboard(hwnd);
	HGLOBAL hGlobal = GetClipboardData(CF_TEXT);

	char *temp;

	if(hGlobal)
	{
		char *pGlobal = (char *)GlobalLock(hGlobal);
		temp = new char[strlen(pGlobal)+1];
		memset(temp, 0, strlen(pGlobal)+1);
		strcpy(temp, pGlobal);
		GlobalUnlock(hGlobal);
	}
	else
	{
		CloseClipboard();
		return NULL;
	}

	CloseClipboard();
	char * utf = AnsiToUtf(temp);
	delete [] temp;
	return utf;
}



//트래이로 이동하는 애니메이션
void CUtil::AniMinimizeToTray(HWND hwnd)
{
	RECT rectTo, rectFrom;
	::GetWindowRect(hwnd, &rectFrom);

	GetTrayWndRect(&rectTo);
	DrawAnimatedRects(hwnd, IDANI_CAPTION, &rectFrom, &rectTo);
}


void CUtil::GetTrayWndRect(RECT * pRect)
{
	HWND hwndTaskBar=::FindWindow(_T("Shell_TrayWnd"), NULL);
	if (hwndTaskBar){
		HWND hwndTray=::FindWindowEx(hwndTaskBar, NULL, _T("TrayNotifyWnd"), NULL);
		if (hwndTray)
			::GetWindowRect(hwndTray, pRect);
		else{
			//tray부분을 못찾으면 task바의 구석탱이를 그렇다고 믿게 하자.
			::GetWindowRect(hwndTaskBar, pRect);
			pRect->left=pRect->right-20;
			pRect->top=pRect->bottom-20;
		}
	}
	else{
		//task바를 못찾으면 그냥 화면 하단부
		int nWidth = GetSystemMetrics(SM_CXSCREEN);
		int nHeight = GetSystemMetrics(SM_CYSCREEN);
		SetRect(pRect, nWidth-40, nHeight-20, nWidth, nHeight);
	}
}


void CUtil::AniMaximiseFromTray(HWND hwnd)
{
	RECT rectFrom;
	GetTrayWndRect(&rectFrom);
	WINDOWPLACEMENT wndpl;
	::GetWindowPlacement(hwnd, &wndpl);    //최소화된 상태에서 죽으면 GetWindowRect로 안됨

	DrawAnimatedRects(hwnd, IDANI_CAPTION, &rectFrom, &wndpl.rcNormalPosition);
}


void CUtil::SetHanEngMode(HWND hFocusWnd)
{
	HIMC hImc;
	DWORD dwConv, dwSent, dwTemp;	
	hImc = ImmGetContext(hFocusWnd);
	if(hImc != NULL)
	{
		ImmGetConversionStatus(hImc, &dwConv, &dwSent);
		dwTemp = dwConv & ~IME_CMODE_LANGUAGE;
		dwTemp |= IME_CMODE_ALPHANUMERIC;
		dwConv = dwTemp;
		ImmSetConversionStatus(hImc,dwConv,dwSent);
		ImmReleaseContext(hFocusWnd, hImc);
	}
}


