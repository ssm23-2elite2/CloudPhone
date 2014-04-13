
// RemotroidServerDlg.h : header file
//

#pragma once

#include "MyClient.h"
#include "FileSender.h"
#include "screen.h"

#include "afxwin.h"
#include "recvfile.h"
#include "MyBitmapBtn.h"
#include "TextProgressCtrl.h"
#include "VitualEventPacket.h"
#include "KeyCodeGen.h"
#include "PopupDlg.h"
#include "PopupDlgMgr.h"
#include "TrayIcon.h"
#include "AniStatic.h"
#include "atltypes.h"
#include "ResizingDlg.h"
#include "ResizeControlMgr.h"
#include "IFileTranceiverListener.h"
#include "KakaoPopupDlg.h"
#include "EditHistory.h"
#include "INIMgr.h"


#define DLGHEIGHT		781
#define DLGWIDTH		427

#define DLGHEIGHT_GARO	427
#define DLGWIDTH_GARO	781
	
#define SCREENWIDTH		358
#define SCREENHEIGHT	598

#define WIDTH_LENGTH	4
#define HEIGHT_LENGTH	4

#define SCREENLEFT		34
#define SCREENTOP		93
#define SCREENRIGHT		LEFT+WIDTH
#define SCREENBOTTOM	TOP+HEIGHT

#define SCREENWIDTH_GARO	598
#define SCREENHEIGHT_GARO	358


#define BUTTONWIDTH			107
#define BUTTONHEIGHT		60

#define BUTTONWIDTH_GARO	60
#define BUTTONHEIGHT_GARO	107

#define SIDEBTNWIDTH		9
#define SIDEBTNHEIGHT		81


interface IParentControl
{
	virtual ~IParentControl(){};
	virtual void MoveBkgDlg(CRect rect, int garosero) = 0;
	virtual void TrayWindow(int state) = 0;
	/*virtual void TurnGaroSero(int garosero)=0;*/
};


//웹서버를 통한 접속 상태
typedef enum CONNECTSTATE
{
	START, SUCCESS, FAIL, END
};



// CRemotroidServerDlg dialog
class CRemotroidServerDlg : public CDialogEx, public IFileTranceiverListener
{
// Construction
public:
	CRemotroidServerDlg(CWnd* pParent = NULL);	// standard constructor
	virtual ~CRemotroidServerDlg();

// Dialog Data
	enum { IDD = IDD_REMOTROIDSERVER_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support


// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	
	afx_msg LONG OnTrayNotification(UINT wParam, LONG lParam);
	DECLARE_MESSAGE_MAP()




private:
	SOCKET m_ServerSocket;	
	CMyClient *m_pClient;
	CFileSender fileSender;
	CRecvFile recvFileClass;
	CScreen screen;
	CKeyCodeGen keyCodeGen;

public:
	static UINT AcceptFunc(LPVOID pParam);	
	static UINT RecvFunc(LPVOID pParam);	
	void SetClientSocket(CMyClient * pClient);
	CMyClient * GetClientSocket(void);
	
	
	afx_msg void OnDestroy();
	afx_msg void OnBnClickedOk();
	afx_msg void OnBnClickedCancel();	
	afx_msg void OnDropFiles(HDROP hDropInfo);
	afx_msg void OnMouseMove(UINT nFlags, CPoint point);
	afx_msg HBRUSH OnCtlColor(CDC* pDC, CWnd* pWnd, UINT nCtlColor);	

	afx_msg void OnKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags);	
	afx_msg void OnChar(UINT nChar, UINT nRepCnt, UINT nFlags);
	afx_msg void OnLButtonUp(UINT nFlags, CPoint point);
		
	LRESULT OnEndAccept(WPARAM wParam, LPARAM lParam);
	LRESULT OnReadyRecvFile(WPARAM wParam, LPARAM lParam);
	LRESULT OnEndRecv(WPARAM wParam, LPARAM lParam);

	static UINT UDPRecvFunc(LPVOID pParam);

	virtual BOOL PreTranslateMessage(MSG* pMsg);		

private:	
	CWinThread *pConnectThread;
	CWinThread *pRecvThread;
	CWinThread *pAcceptThread;
	CWinThread *pUdpRecvThread;
	BOOL m_isClickedEndBtn;
	SOCKET m_UDPServerSocket;
	BOOL m_isReadyRecv;	

	CPopupDlgMgr m_popDlgMgr;
	

private:
	void EndAccept(void);
	void EndConnect(void);
	void ReadyRecvFile(void);		
	void GetStoreFilePath(void);
public:
	CMyBitmapBtn m_BackButton;
	CMyBitmapBtn m_HomeButton;
	CMyBitmapBtn m_MenuButton;
	CMyBitmapBtn m_TrayButton;
	CMyBitmapBtn m_CloseButton;
	CMyBitmapBtn m_PowerButton;
	CMyBitmapBtn m_VolumeDownButton;
	CMyBitmapBtn m_VolumeUpButton;
	CMyBitmapBtn m_ExplorerBtn;
	CMyBitmapBtn m_FileCancelButton;

	CTextProgressCtrl m_progressCtrl;
	CTrayIcon m_TrayIcon;
	
	afx_msg void OnClickedBtnBack();
	afx_msg void OnClickedBtnHome();
	afx_msg void OnClickedBtnMenu();	

	afx_msg void OnBnClickedBtnVolumnup();
	afx_msg void OnBnClickedBtnVolumedown();
	afx_msg void OnBnClickedBtnPower();
	afx_msg void OnBnClickedBtnExplorer();
	
	afx_msg void OnKeyUp(UINT nChar, UINT nRepCnt, UINT nFlags);
	LRESULT OnAnimation(WPARAM wParam, LPARAM lParam);
	LRESULT OnCreatePopupDlg(WPARAM wParam, LPARAM lParam);	

	LRESULT OnClosePopDlg(WPARAM wParam, LPARAM lParam);	
	LRESULT OnMyDblClkTray(WPARAM wParam, LPARAM lParam);
	afx_msg void OnBnClickedBtnTray();
	afx_msg void OnBnClickedBtnClose();
	afx_msg void OnClose();
private:
	BOOL isTray;
	CResizingDlg *pResizeDlg;
	int m_CurCursorState;
	

public:
	CString m_strMyIp;
	afx_msg void OnTimer(UINT_PTR nIDEvent);

	afx_msg BOOL OnMouseWheel(UINT nFlags, short zDelta, CPoint pt);
	

	CRect mainDlgRect;
	
	afx_msg void OnSize(UINT nType, int cx, int cy);

	afx_msg void OnMove(int x, int y);
	afx_msg void OnLButtonDown(UINT nFlags, CPoint point);
	
	BOOL m_bResizing;
public:
	void SetResizingDlg(void);
	int SetSizeCursor(CPoint point);
private:
	
public:
	void SetBkgDlg(IParentControl * pBkgDlg);
private:
	IParentControl *m_pBkgDlg;
	virtual void PostNcDestroy();
public:
	afx_msg void OnMoving(UINT fwSide, LPRECT pRect);
private:
	BOOL m_bInit;
	CResizeControlMgr m_ResizeContolMgr;
public:
	void SetControlPos(void);
	int m_GaroSeroState;
	void TurnGaroSero(int garosero);
	

	CImageList *imageList;
	
	
	static UINT CRemotroidServerDlg::RequireConnectClient(LPVOID parma);
	CString m_strEmail;
	CString m_strPasswd;
	CEditHistory m_ctrlEmail;
	CEdit m_ctrlPasswd;
	afx_msg void OnBnClickedBtnConnect();
	CMyBitmapBtn m_btnConnect;
private:
	CFont editFont;
	LOGFONT lf;
	FILETRANCEIVERSTATE m_fileTranceiverState;	
	

public:
	void InitFont(void);
	void DestroyFont(void);
	
	void ClickHardwareKey(int state);
	BOOL InitServerSocket(void);
	static UINT StartingWaitingAni(LPVOID pParam);
	BOOL bLoginOK;
	void EnableLoginWnd(CONNECTSTATE state);
	CRect m_firstPosition;
	void DestroyAllPopupDlg();	
	void SetFileTranceiverState(FILETRANCEIVERSTATE state);
	
	void StartFileTranceiver(BOOL cond);
	afx_msg void OnBnClickedBtnFilecancel();
	HCURSOR fileTransferCursor;
	
	void SendClipboardText(void);
private:
	BOOL m_isKakaoTalk;
	BOOL m_lastScreenState;
public:
	void SetScreenState(BOOL isScreenOn);
private:
	BOOL m_isLbuttonDown;
	CINIMgr m_IniMgr;
protected:
	void InitEditHistory(void);
};

