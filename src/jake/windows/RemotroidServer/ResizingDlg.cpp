// ResizingDlg.cpp : implementation file
//

#include "stdafx.h"
#include "RemotroidServer.h"
#include "ResizingDlg.h"
#include "afxdialogex.h"

#include "RemotroidServerDlg.h"


// CResizingDlg dialog

IMPLEMENT_DYNAMIC(CResizingDlg, CDialogEx)

CResizingDlg::CResizingDlg(CWnd* pParent /*=NULL*/)
	: CDialogEx(CResizingDlg::IDD, pParent)
	, m_garoSeroRatio(0)
	, m_garoSeroState(0)
{

}

CResizingDlg::~CResizingDlg()
{
}

void CResizingDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);
}


BEGIN_MESSAGE_MAP(CResizingDlg, CDialogEx)
	ON_WM_WINDOWPOSCHANGING()
	
END_MESSAGE_MAP()


// CResizingDlg message handlers


BOOL CResizingDlg::OnInitDialog()
{
	CDialogEx::OnInitDialog();

	// TODO:  Add extra initialization here


	COLORREF cr = GetSysColor(COLOR_BTNFACE);	
	SetLayeredWindowAttributes(cr, 60, LWA_ALPHA);

	return TRUE;  // return TRUE unless you set the focus to a control
	// EXCEPTION: OCX Property Pages should return FALSE
}



void CResizingDlg::OnWindowPosChanging(WINDOWPOS* lpwndpos)
{
	CDialogEx::OnWindowPosChanging(lpwndpos);

		
	// TODO: Add your message handler code here
}


int CResizingDlg::SearchSide(CRect rc, CPoint point)
{
	long x, y;

    x = point.x;// + rc.left;
    y = point.y;// + rc.top;

	if((rc.left <= x) && (x <= rc.left+SIDE))
    {
        if((rc.top <= y) && (y <= rc.top+SIDE))
        {
            return HTTOPLEFT;
        }
        if((rc.bottom >= y) && (y >= rc.bottom-SIDE))
        {			
            return HTBOTTOMLEFT;
        }

        return -1;
    }

    if((rc.right >= x) && (x >= rc.right-SIDE))
    {
        if((rc.top <= y) && (y <= rc.top+SIDE))
        {
            return HTTOPRIGHT;
        }
        if((rc.bottom >= y) && (y >= rc.bottom-SIDE))
        {			
            return HTBOTTOMRIGHT;
        }

        return -1;
    }    
	
	return -1;
}

//크기조절하는 마우스 위치별로 테두리 다이얼로그 위치가 달라짐
void CResizingDlg::ResizingDlg(CPoint point)
{
	int width, height;		
	switch(m_CurCursorState)
	{
	case HTTOPLEFT:			
		height = baseRect.bottom - point.y - yDiff;

  		if(m_garoSeroState == SERO && (height > MAXHEIGHT || height < MINHEIGHT))
  			return;	
		if(m_garoSeroState == GARO && (height > MAXHEIGHT_GARO || height < MINHEIGHT_GARO))
  			return;	

		width = (int)((float)height*m_garoSeroRatio);
		MoveWindow(baseRect.right-width, point.y+yDiff, width, height);
		break;
	case HTTOPRIGHT:
		height = baseRect.bottom - point.y - yDiff;

		if(m_garoSeroState == SERO && (height > MAXHEIGHT || height < MINHEIGHT))
 			return;
		if(m_garoSeroState == GARO && (height > MAXHEIGHT_GARO || height < MINHEIGHT_GARO))
  			return;	

		width = (int)((float)height*m_garoSeroRatio);
		MoveWindow(baseRect.left, point.y+yDiff, width, height);
		break;
	case HTBOTTOMLEFT:
		height = point.y - baseRect.top + yDiff;

		if(m_garoSeroState == SERO && (height > MAXHEIGHT || height < MINHEIGHT) )
 			return;
		if(m_garoSeroState == GARO && (height > MAXHEIGHT_GARO || height < MINHEIGHT_GARO))
  			return;	
		
		width = (int)((float)height*m_garoSeroRatio);
		MoveWindow(baseRect.right-width, baseRect.top, width, height);
		break;
	case HTBOTTOMRIGHT:
		height = point.y - baseRect.top + yDiff;
		
		if(m_garoSeroState == SERO && (height > MAXHEIGHT || height < MINHEIGHT))
			return;
		if(m_garoSeroState == GARO && (height > MAXHEIGHT_GARO || height < MINHEIGHT_GARO))
  			return;	

		width = (int)((float)height*m_garoSeroRatio);
		MoveWindow(baseRect.left, baseRect.top, width, height);
		break;
	}	
}


void CResizingDlg::InitResizingDlg(CRect rect, CPoint point, int CursorState)
{
	baseRect = rect;
	m_CurCursorState = CursorState;

	m_garoSeroRatio = (float) ( (float)rect.Width() / rect.Height() );

	if(rect.Width() > rect.Height())
		m_garoSeroState = GARO;
	else
		m_garoSeroState = SERO;

	switch(m_CurCursorState)
	{
	case HTTOPLEFT:			
		xDiff = baseRect.left - point.x;
		yDiff = baseRect.top - point.y;
		break;
	case HTTOPRIGHT:
		xDiff = baseRect.right - point.x;
		yDiff = baseRect.top - point.y;
		break;
	case HTBOTTOMLEFT:
		xDiff = baseRect.left - point.x;
		yDiff = baseRect.bottom - point.y;
		break;
	case HTBOTTOMRIGHT:
		xDiff = baseRect.right - point.x;
		yDiff = baseRect.bottom - point.y;
		break;
	}	
}


void CResizingDlg::PostNcDestroy()
{
	// TODO: Add your specialized code here and/or call the base class
	delete this;
	CDialogEx::PostNcDestroy();
}
