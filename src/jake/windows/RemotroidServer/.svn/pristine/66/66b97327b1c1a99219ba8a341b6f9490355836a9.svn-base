#pragma once
#include "afxwin.h"


// CEditTrans

class CEditTrans : public CEdit
{
	DECLARE_DYNAMIC(CEditTrans)

public:
	CEditTrans();
	virtual ~CEditTrans();

protected:
	DECLARE_MESSAGE_MAP()
public:
	afx_msg void OnEnKillfocus();
	afx_msg void OnEnUpdate();
	afx_msg HBRUSH CtlColor(CDC* pDC, UINT nCtlColor);
	afx_msg void OnLButtonDown(UINT nFlags, CPoint point);
	CBrush m_Brush;
	void UpdateCtrl(void);
	afx_msg void OnKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags);
	afx_msg int OnMouseActivate(CWnd* pDesktopWnd, UINT nHitTest, UINT message);
	afx_msg void OnEnSetfocus();
	afx_msg void OnLButtonUp(UINT nFlags, CPoint point);
};


