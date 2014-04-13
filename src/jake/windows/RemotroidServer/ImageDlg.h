#pragma once
#include "afxwin.h"


// CImageDlg dialog
#include "ResizingDlg.h"
#include "atltypes.h"
#include "RemotroidServerDlg.h"



class CImageDlg : public CDialogEx, IParentControl
{
	DECLARE_DYNAMIC(CImageDlg)

public:
	CImageDlg(CWnd* pParent = NULL);   // standard constructor
	virtual ~CImageDlg();

// Dialog Data
	enum { IDD = IDD_IMAGEDLG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

	DECLARE_MESSAGE_MAP()
private:
	HRSRC		hResource,	hResBack;
	HGLOBAL		hGlobal,	hBackGlobal;
	HGLOBAL		hBuffer;
	LPVOID		pData;
	IStream		*pStream;
	Image		*pImagePng;
	float		m_xScale,	m_yScale;
	
public:
	afx_msg void OnPaint();
	virtual BOOL OnInitDialog();
	
	virtual void OnResizeSkin(int garosero = 0);
	CBitmap m_bitmap[2];
	
private:
	CResizingDlg *pResizeDlg;
protected:
	CRect baseRect;

	
protected:
	
	void SetDlgPosition(void);

private:
	BOOL m_bResizing;

private:
	int m_nHitTest;
private:
	int SetSizeCursor(CPoint point);
public:
	CRect m_oldRect;
private:
	int m_CurCursorState;
public:
	Bitmap *m_pBkgBitmap[2];
	
	Bitmap * PngFromResource(const LPTSTR pName, const LPTSTR pType);
	HBITMAP Create32BitBitmap(CDC * pDC, int cx, int cy);	
	
private:
	CRemotroidServerDlg *m_pControlDlg;
public:
	BOOL InitControlDlg(void);
	virtual void MoveBkgDlg(CRect rect, int garosero);
	afx_msg void OnDestroy();	
	afx_msg void OnActivate(UINT nState, CWnd* pWndOther, BOOL bMinimized);

	void TrayWindow(int state);
};
