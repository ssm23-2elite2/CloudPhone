#pragma once


// CSplash dialog

class CSplash : public CDialogEx
{
	DECLARE_DYNAMIC(CSplash)

public:
	CSplash(CWnd* pParent = NULL);   // standard constructor
	virtual ~CSplash();

// Dialog Data
	enum { IDD = IDD_LOADINGDLG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

	DECLARE_MESSAGE_MAP()
public:
	afx_msg void OnBnClickedOk();
	afx_msg void OnBnClickedCancel();
	virtual BOOL OnInitDialog();
	afx_msg void OnTimer(UINT_PTR nIDEvent);	
};
