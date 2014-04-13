// MyEdit.cpp : implementation file
//

#include "stdafx.h"
#include "RemotroidServer.h"
#include "MyEdit.h"


// CMyEdit

IMPLEMENT_DYNAMIC(CMyEdit, CEdit)

CMyEdit::CMyEdit()
{

}

CMyEdit::~CMyEdit()
{
}


BEGIN_MESSAGE_MAP(CMyEdit, CEdit)
	ON_WM_KEYDOWN()
	ON_WM_KEYUP()
END_MESSAGE_MAP()



// CMyEdit message handlers




void CMyEdit::OnKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	// TODO: Add your message handler code here and/or call default
	

	CEdit::OnKeyDown(nChar, nRepCnt, nFlags);
}


void CMyEdit::OnKeyUp(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	// TODO: Add your message handler code here and/or call default
	CWnd *pParent = GetParent();
	pParent->SendMessage(WM_INPUTEDIT, 0, 0);

	CEdit::OnKeyUp(nChar, nRepCnt, nFlags);
}
