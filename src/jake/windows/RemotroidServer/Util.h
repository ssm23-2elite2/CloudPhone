#pragma once
class CUtil
{
public:
	CUtil(void);
	~CUtil(void);
	static int GetOpCode(char * packet);
	static int GetPacketSize(char * packet);
	static void UniToUtf(TCHAR * uni, char * utf);
	static void UtfToUni(TCHAR * uni, char * utf);
	static void AniMinimizeToTray(HWND hwnd);
	static void GetTrayWndRect(RECT * pRect);
	static void AniMaximiseFromTray(HWND hwnd);
	static void SetHanEngMode(HWND hWnd);
	static TCHAR * UtfToUniEx(const char * utf);

	static char * UniToAnsi(const CString uni);
	static char * UniToUtfEx(const CString uni);
	static char * AnsiToUtf(const char *ansi);
	static TCHAR * AnsiToUni(const char * ansi);
	static char * GetClipboardText(HWND hwnd);
};

