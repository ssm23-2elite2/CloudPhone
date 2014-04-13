#pragma once
#include "afxwin.h"
#include "MyBitmapBtn.h"
#include "resource.h"
#include "MyClient.h"

// CPopupDlg dialog

#define MAXNOTI_LENGTH	50
#define MOVESTEP		10
#define MAXPOPUPDLG		10

class CPopupDlg : public CDialogEx
{
	DECLARE_DYNAMIC(CPopupDlg)

public:
	CPopupDlg(CWnd* pParent = NULL);   // standard constructor
	virtual ~CPopupDlg();

// Dialog Data
	enum { IDD = IDD_POPUPDLG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

	DECLARE_MESSAGE_MAP()
public:
	afx_msg void OnBnClickedOk();
	afx_msg void OnBnClickedCancel();
	virtual void PostNcDestroy();
	virtual BOOL OnInitDialog();	
private:
	CMyBitmapBtn m_CloseBtn;

public:
	afx_msg void OnBnClickedBtnClose();		
	afx_msg void OnPaint();
	virtual BOOL DestroyWindow();
	afx_msg void OnDestroy();
	CString m_strNoti;
	static int numOfDlg;
	LRESULT OnCtlColorStatic(WPARAM wParam, LPARAM lParam);
	LRESULT OnMovePopDlg(WPARAM wParam, LPARAM lParam);
private:
	int m_dlgHeight;
	int m_targetHeight;
	CRect dlgRect;
public:
	afx_msg void OnTimer(UINT_PTR nIDEvent);
	BOOL m_bFlagAllDestroy;
	afx_msg void OnBnClickedBtnSend();
	CMyBitmapBtn m_btnSend;
	
	void SendBtnEnable(BOOL bCond);
	BOOL m_isKakao;
	CMyClient *pClient;
};

