// AniStatic.cpp : implementation file
//

#include "stdafx.h"
#include "RemotroidServer.h"
#include "AniStatic.h"
#include <atlimage.h>


// CAniStatic

IMPLEMENT_DYNAMIC(CAniStatic, CStatic)

CAniStatic::CAniStatic()
: alpahValue(250), pos(10)
{
	m_bmp.LoadBitmap(IDB_BITMAPWAITING3);
	m_bmp.GetBitmap(&m_bmpInfo);

	//알파블랜딩 효과를 위해
	memset(&bf, 0, sizeof(bf));
	bf.BlendOp = AC_SRC_OVER;
	bf.BlendFlags = 0;
	bf.SourceConstantAlpha = alpahValue;
	bf.AlphaFormat = 0;	
}

CAniStatic::~CAniStatic()
{
}


BEGIN_MESSAGE_MAP(CAniStatic, CStatic)		
	ON_WM_PAINT()
	ON_WM_CREATE()
	ON_WM_TIMER()
END_MESSAGE_MAP()



// CAniStatic message handlers


void CAniStatic::OnPaint()
{
	CPaintDC dc(this); // device context for painting
	// TODO: Add your message handler code here
	// Do not call CStatic::OnPaint() for painting messages
	
	CDC memDC;
	memDC.CreateCompatibleDC(&dc);
	CBitmap *pOldBmp = NULL;
	pOldBmp = memDC.SelectObject(&m_bmp);

	bf.SourceConstantAlpha = alpahValue;

	dc.AlphaBlend(0,0,m_bmpInfo.bmWidth, m_bmpInfo.bmHeight, &memDC, 0,0,m_bmpInfo.bmWidth, m_bmpInfo.bmHeight, bf);
	memDC.SelectObject(pOldBmp);	
}


int CAniStatic::OnCreate(LPCREATESTRUCT lpCreateStruct)
{
	if (CStatic::OnCreate(lpCreateStruct) == -1)
		return -1;

	// TODO:  Add your specialized creation code here		
	return 0;
}


void CAniStatic::SetAnimation(BOOL cond)
{
	
	if(cond)
		SetTimer(0, 50, NULL);
	else
		KillTimer(0);
	
}


void CAniStatic::OnTimer(UINT_PTR nIDEvent)
{
	// TODO: Add your message handler code here and/or call default
	
	if(alpahValue == 250 || alpahValue == 0)
	{
		//Sleep(200);
		pos = pos * -1;		
	}
		
	alpahValue += pos;	
	
	GetParent()->InvalidateRect(&myRect, FALSE);
	CStatic::OnTimer(nIDEvent);
}
