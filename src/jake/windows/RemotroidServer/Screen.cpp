
// Screen.cpp : implementation file
//

#include "stdafx.h"
#include "RemotroidServer.h"
#include "Screen.h"



// CScreen

IMPLEMENT_DYNAMIC(CScreen, CStatic)

CScreen::CScreen()
: pClient(NULL)
, widthResolution(0)
, heightResolution(0)
, width(0)
, height(0)
, m_bTrack(FALSE)
, m_strMyIp(_T(""))
, m_isConnect(FALSE)
, m_rotationState(0)
, m_isScreenON(FALSE)
, m_isLbuttonDown(FALSE)
{
	m_bkgImg.LoadFromResource(AfxGetInstanceHandle(), IDB_BITMAP_WAITING);		
	ZeroMemory(&lf, sizeof(lf));
	lf.lfHeight = 45;
	lf.lfWeight = FW_BOLD;
	
	wsprintf(lf.lfFaceName, _T("HY견고딕"));
	
	newFont.CreateFontIndirect(&lf);

	m_BlackBrush.CreateStockObject(BLACK_BRUSH);
}

CScreen::~CScreen()
{	
	m_BlackBrush.DeleteObject();
	newFont.DeleteObject();	
}


BEGIN_MESSAGE_MAP(CScreen, CStatic)
	ON_WM_DESTROY()
	ON_MESSAGE(WM_RECVJPGINFO, OnSetJpgInfo)
	ON_MESSAGE(WM_RECVJPGDATA, OnRecvJpgData)	
	ON_MESSAGE(WM_RECVDEVICEINFO, OnSetResolution)
	ON_WM_LBUTTONDOWN()
	ON_WM_LBUTTONUP()
	ON_WM_MOUSEMOVE()		
	ON_WM_MOUSELEAVE()
	ON_WM_PAINT()
	ON_WM_CREATE()
		
	ON_WM_MOUSEWHEEL()
	ON_WM_ERASEBKGND()
	
	ON_WM_SIZE()
	
END_MESSAGE_MAP()
 


// CScreen message handlers




void CScreen::InitDrawJpg(void)
{
	CRect rect;
	GetClientRect(&rect);	
	width = rect.Width();
	height = rect.Height();
	drawJpg.InitDrawJpg(GetSafeHwnd(), width, height);		
}


void CScreen::OnDestroy()
{
	CStatic::OnDestroy();

	// TODO: Add your message handler code here	
}


//마우스 포인터와 스마트폰 해상도의 차이를 맞추기 위해
LRESULT CScreen::OnSetResolution(WPARAM wParam, LPARAM lParam)
{
	char *data = (char *)lParam;

	char bWidth[WIDTH_LENGTH+1];
	memset(bWidth, 0, sizeof(bWidth));
	memcpy(bWidth, data, WIDTH_LENGTH);
	widthResolution = atoi(bWidth);

	char bHeight[HEIGHT_LENGTH+1];
	memset(bHeight, 0, sizeof(bHeight));
	memcpy(bHeight, data+WIDTH_LENGTH, HEIGHT_LENGTH);
	heightResolution = atoi(bHeight);
	return 0;
}


LRESULT CScreen::OnSetJpgInfo(WPARAM wParam, LPARAM lParam)
{	
	char *data = (char*)lParam;
	drawJpg.SetJpgInfo(data);

	return 0;
}

void CScreen::SetJpgInfo(char *data)
{
	drawJpg.SetJpgInfo(data);
}


LRESULT CScreen::OnRecvJpgData(WPARAM wParam, LPARAM lParam)
{	
	int packetSize = wParam;
	char *data = (char *)lParam;
	drawJpg.RecvJpgData(data, packetSize);
	return 0;
}



void CScreen::RecvJpgData(char * data, int iPacketSize)
{
	//화면이 꺼져 있다면 jpg 수신하지 않는다
	if(m_isScreenON == FALSE)
		return;

	drawJpg.RecvJpgData(data, iPacketSize);
}


inline void CScreen::CoordinateTransform(CPoint& point)
{
	CPoint temp;
	if(m_rotationState == ROTATION0)
	{
		point.x = point.x*((float)widthResolution/width);
		point.y = point.y*((float)heightResolution/height);
	}
	else if(m_rotationState == ROTATION90)
	{
		temp.y = point.x*((float)heightResolution/width);
		temp.x = widthResolution - point.y*((float)widthResolution/height);
		point = temp;
	}
	else
	{
		temp.x = point.y*((float)widthResolution/height);
		temp.y = heightResolution - point.x * ((float)heightResolution/width);
		point = temp;
	}
}


void CScreen::OnLButtonDown(UINT nFlags, CPoint point)
{
	// TODO: Add your message handler code here and/or call default
	m_isLbuttonDown = TRUE;

	if(pClient == NULL)
		return;
	
	CoordinateTransform(point);
	CVitualEventPacket event(TOUCHDOWN, point.x, point.y);
	pClient->SendPacket(OP_VIRTUALEVENT, event.asByteArray(), event.payloadSize);

	
	CStatic::OnLButtonDown(nFlags, point);
}



void CScreen::OnLButtonUp(UINT nFlags, CPoint point)
{
	// TODO: Add your message handler code here and/or call default
	m_isLbuttonDown = FALSE;
	
	if(pClient == NULL)
		return;
	

	if(m_bTrack)
	{	
		TRACKMOUSEEVENT MouseEvent;
		memset(&MouseEvent, 0, sizeof(0));
		MouseEvent.cbSize = sizeof(MouseEvent);
		MouseEvent.dwFlags = TME_CANCEL|TME_LEAVE;
		MouseEvent.hwndTrack = m_hWnd;

		::_TrackMouseEvent(&MouseEvent);
		m_bTrack = FALSE;		
	}
		
		
	CVitualEventPacket event(TOUCHUP);
	pClient->SendPacket(OP_VIRTUALEVENT, event.asByteArray(), event.payloadSize);	

	
	CStatic::OnLButtonUp(nFlags, point);
}


void CScreen::SetClient(CMyClient * pClient)
{
	m_isConnect = TRUE;
	EnableAnimation(FALSE);

	this->pClient = pClient;	
}


void CScreen::OnMouseMove(UINT nFlags, CPoint point)
{
	// TODO: Add your message handler code here and/or call default		

	if((nFlags & MK_LBUTTON)==MK_LBUTTON)
	{
		if(pClient == NULL)
			return;

		if(m_bTrack == FALSE)
		{
			//////마우스 추적 시작			
			TRACKMOUSEEVENT MouseEvent;
			memset(&MouseEvent, 0, sizeof(0));
			MouseEvent.cbSize = sizeof(MouseEvent);
			MouseEvent.dwFlags = TME_LEAVE;
			MouseEvent.hwndTrack = m_hWnd;
			MouseEvent.dwHoverTime = 0;
			m_bTrack = ::_TrackMouseEvent(&MouseEvent);
		}

		CoordinateTransform(point);
		CVitualEventPacket event(SETCOORDINATES, point.x, point.y);		
		
		pClient->SendPacket(OP_VIRTUALEVENT, event.asByteArray(), event.payloadSize);		
	}	
	CStatic::OnMouseMove(nFlags, point);
}



//마우스가 벗어나면 Up 이벤트 전송해야 한다
void CScreen::OnMouseLeave()
{
	// TODO: Add your message handler code here and/or call default	
	
	if(pClient == NULL)
		return;

	if(m_bTrack)
	{		
		TRACKMOUSEEVENT MouseEvent;
		memset(&MouseEvent, 0, sizeof(0));
		MouseEvent.cbSize = sizeof(MouseEvent);
		MouseEvent.dwFlags = TME_CANCEL|TME_LEAVE;
		MouseEvent.hwndTrack = m_hWnd;

		::_TrackMouseEvent(&MouseEvent);
		m_bTrack = FALSE;

		CVitualEventPacket event(TOUCHUP);
		pClient->SendPacket(OP_VIRTUALEVENT, event.asByteArray(), event.payloadSize);	
	}	
}

BOOL CScreen::PreCreateWindow(CREATESTRUCT& cs)
{
	// TODO: Add your specialized code here and/or call the base class

	//더블클릭을 막기 위해서
	WNDCLASS wc;
	memset(&wc, 0, sizeof(wc));
	GetClassInfo(cs.hInstance, cs.lpszClass, &wc);	
	
	wc.style = wc.style & (~CS_DBLCLKS);	
	cs.lpszClass = AfxRegisterWndClass(wc.style, wc.hCursor, wc.hbrBackground, wc.hIcon);	
	return CStatic::PreCreateWindow(cs);
}


void CScreen::OnPaint()
{
	CPaintDC dc(this); // device context for painting
	// TODO: Add your message handler code here
	// Do not call CStatic::OnPaint() for painting messages
		
					//접속중인데 화면이 꺼져있다면
	if(m_isConnect && !m_isScreenON)
	{
			CRect rect;
			GetClientRect(&rect);
			dc.FillRect(&rect, &m_BlackBrush);
	}
	else if(!m_isConnect)			//접속 대기상태라면
	{
		/*
		CDC *imagePDC = CDC::FromHandle(m_bkgImg.GetDC());

		CFont *pOldFont = imagePDC->SelectObject(&newFont);

		imagePDC->SetTextColor(RGB(255,255,255));
		imagePDC->SetBkMode(TRANSPARENT);
		imagePDC->TextOut(40,250, m_strMyIp);

		imagePDC->SelectObject(pOldFont);

		//m_bkgImg.ReleaseDC();

		*/
		CRect rt;
		GetClientRect(&rt);

		dc.SetStretchBltMode(HALFTONE);

		ExcludePaintControl(dc);

		m_bkgImg.StretchBlt(dc.m_hDC, rt);


		//ReleaseDC(imagePDC);
		//m_bkgImg.ReleaseDC();	
	}
}


int CScreen::OnCreate(LPCREATESTRUCT lpCreateStruct)
{
	if (CStatic::OnCreate(lpCreateStruct) == -1)
		return -1;

	// TODO:  Add your specialized creation code here
 	aniWait.Create(_T(""), WS_CHILD|WS_VISIBLE,CRect(81,351,281,487), this, 0);
 	aniWait.myRect = CRect(81,351,281,487);	
	aniWait.ShowWindow(SW_HIDE);		
	return 0;
}


void CScreen::EnableAnimation(BOOL cond)
{
	if(cond)
	{		
		aniWait.ShowWindow(SW_SHOW);		
		aniWait.SetAnimation(TRUE);
	}
	else
	{
		aniWait.SetAnimation(FALSE);
		aniWait.ShowWindow(SW_HIDE);		
	}
}



void CScreen::SetDisconnect()
{
	m_isConnect = FALSE;		
	RedrawWindow();
}





BOOL CScreen::OnMouseWheel(UINT nFlags, short zDelta, CPoint pt)
{
	// TODO: Add your message handler code here and/or call default
	TRACE("nFlgs : %d, zDelta : %d, x : %d, y : %d", nFlags, zDelta, pt.x, pt.y);
	return CStatic::OnMouseWheel(nFlags, zDelta, pt);
}


BOOL CScreen::PreTranslateMessage(MSG* pMsg)
{
	// TODO: Add your specialized code here and/or call the base class
	if(pMsg->message == WM_MOUSEHWHEEL)
	{
		SendMessage(pMsg->message, pMsg->wParam, pMsg->lParam);
	}
	return CStatic::PreTranslateMessage(pMsg);
}


BOOL CScreen::OnEraseBkgnd(CDC* pDC)
{
	// TODO: Add your message handler code here and/or call default



	return FALSE;
}


void CScreen::OnSize(UINT nType, int cx, int cy)
{
	CStatic::OnSize(nType, cx, cy);
	width = cx;
	height = cy;
	drawJpg.InitDrawJpg(GetSafeHwnd(), cx, cy);
	// TODO: Add your message handler code here
}



//edit 컨트롤이랑 버튼 부분은 그림 X
void CScreen::ExcludePaintControl(CDC & dc)
{
	CRect excludeRt;

	for(int i=IDC_EDIT_EMAIL; i<IDC_EDIT_EMAIL+3; i++)
	{
		CWnd *pWnd = GetParent()->GetDlgItem(i);
		if(pWnd != NULL && pWnd->IsWindowVisible())
		{
			pWnd->GetWindowRect(&excludeRt);
			ScreenToClient(&excludeRt);
			dc.ExcludeClipRect(&excludeRt);
		}
	}	
}


void CScreen::TurnGaroSero(int rotation)
{
	drawJpg.m_rotationState = rotation;	
	m_rotationState = rotation;
}


void CScreen::SetScreenState(BOOL isScreenOn)
{
	m_isScreenON = isScreenOn;
	RedrawWindow();
}
