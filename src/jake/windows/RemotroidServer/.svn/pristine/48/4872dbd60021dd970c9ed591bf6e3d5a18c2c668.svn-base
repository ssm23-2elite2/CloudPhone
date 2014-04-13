
#pragma once
#include "EditTrans.h"
#include "afxwin.h"
#include "atltypes.h"
#include "MyEdit.h"
#include "MyClient.h"
// CKakaoPopupDlg dialog

#define TEXTBOXMAXWIDTH	200
#define TEXTBOXMINWIDTH 25
#define TEXTBOXMINHEIGHT 20

class CKakaoPopupDlg : public CDialogEx
{
	DECLARE_DYNAMIC(CKakaoPopupDlg)

public:
	CKakaoPopupDlg(CWnd* pParent = NULL);   // standard constructor
	virtual ~CKakaoPopupDlg();

// Dialog Data
	enum { IDD = IDD_KAKAODLG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

	DECLARE_MESSAGE_MAP()
public:
	afx_msg void OnBnClickedOk();
	afx_msg void OnBnClickedCancel();
	virtual void PostNcDestroy();
	
	afx_msg void OnBnClickedButton1();
	
	CEditTrans m_ctrlRecvEdit;
	
	afx_msg void OnPaint();

	CBitmap m_BackBitmap;
	CString m_strRecvEdit;
	virtual BOOL OnInitDialog();
	
	HRSRC hResource;
	HGLOBAL hBuffer;
	LPVOID pData;
	HGLOBAL hGlobal;
	IStream *pStream;
	Image *pImagePNG;
	CMyEdit m_ctrlSendEdit;
	CString m_strSendText;
	CButton m_SendBtn;
	CSize m_TextBoxSize;
	void GetTextBoxRect(void);

	LRESULT OnInputEdit(WPARAM wParam, LPARAM lParam);
	CMyClient *pClient;
	
};
