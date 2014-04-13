// EditTrans.cpp : implementation file
//

#include "stdafx.h"
#include "RemotroidServer.h"
#include "EditTrans.h"


// CEditTrans

IMPLEMENT_DYNAMIC(CEditTrans, CEdit)

CEditTrans::CEditTrans()
{

}

CEditTrans::~CEditTrans()
{
}


BEGIN_MESSAGE_MAP(CEditTrans, CEdit)
	ON_CONTROL_REFLECT(EN_KILLFOCUS, &CEditTrans::OnEnKillfocus)
	ON_CONTROL_REFLECT(EN_UPDATE, &CEditTrans::OnEnUpdate)
	ON_WM_CTLCOLOR_REFLECT()
	ON_WM_LBUTTONDOWN()
	ON_WM_KEYDOWN()
	ON_WM_MOUSEACTIVATE()
	ON_CONTROL_REFLECT(EN_SETFOCUS, &CEditTrans::OnEnSetfocus)
	ON_WM_LBUTTONUP()
END_MESSAGE_MAP()



// CEditTrans message handlers



HBRUSH CEditTrans::CtlColor(CDC* pDC, UINT nCtlColor)
{
	// TODO:  Change any attributes of the DC here

	// TODO:  Return a non-NULL brush if the parent's handler should not be called
	m_Brush.DeleteObject();
	m_Brush.CreateStockObject(HOLLOW_BRUSH);
	pDC->SetBkMode(TRANSPARENT);

	return (HBRUSH)m_Brush;
}



void CEditTrans::OnEnKillfocus()
{
	// TODO: Add your control notification handler code here
	UpdateCtrl();
}


void CEditTrans::OnEnUpdate()
{
	// TODO:  If this is a RICHEDIT control, the control will not
	// send this notification unless you override the CEdit::OnInitDialog()
	// function to send the EM_SETEVENTMASK message to the control
	// with the ENM_UPDATE flag ORed into the lParam mask.

	// TODO:  Add your control notification handler code here
	UpdateCtrl();	
	
}


void CEditTrans::OnLButtonDown(UINT nFlags, CPoint point)
{
	// TODO: Add your message handler code here and/or call default
	
	
	UpdateCtrl();
	CEdit::OnLButtonDown(nFlags, point);
	
}


void CEditTrans::UpdateCtrl(void)
{
	CWnd *pParent = GetParent();
	CRect rect;

	GetWindowRect(rect);
	pParent->ScreenToClient(rect);
	//rect.DeflateRect(2,2);

	//::DestroyCaret();
	pParent->InvalidateRect(rect, TRUE);	
}


void CEditTrans::OnKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	// TODO: Add your message handler code here and/or call default

	UpdateCtrl();
	CEdit::OnKeyDown(nChar, nRepCnt, nFlags);
}


int CEditTrans::OnMouseActivate(CWnd* pDesktopWnd, UINT nHitTest, UINT message)
{
	// TODO: Add your message handler code here and/or call default

	return MA_NOACTIVATE;
}


void CEditTrans::OnEnSetfocus()
{
	// TODO: Add your control notification handler code here
	::DestroyCaret();
}


void CEditTrans::OnLButtonUp(UINT nFlags, CPoint point)
{
	// TODO: Add your message handler code here and/or call default
	::DestroyCaret();
	CEdit::OnLButtonUp(nFlags, point);
}
