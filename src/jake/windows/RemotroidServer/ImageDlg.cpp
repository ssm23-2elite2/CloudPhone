// ImageDlg.cpp : implementation file
//

#include "stdafx.h"
#include "RemotroidServer.h"
#include "ImageDlg.h"
#include "afxdialogex.h"



// CImageDlg dialog

IMPLEMENT_DYNAMIC(CImageDlg, CDialogEx)

CImageDlg::CImageDlg(CWnd* pParent /*=NULL*/)
	: CDialogEx(CImageDlg::IDD, pParent), pStream(NULL)
	, pResizeDlg(NULL)
	, m_bResizing(FALSE)
	, m_nHitTest(0)
	, m_CurCursorState(0)	
{
	memset(m_pBkgBitmap, 0, sizeof(m_pBkgBitmap));
}

CImageDlg::~CImageDlg()
{
	
}

void CImageDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);
}


BEGIN_MESSAGE_MAP(CImageDlg, CDialogEx)
	ON_WM_PAINT()		
	ON_WM_SETCURSOR()	
	ON_WM_DESTROY()	
	ON_WM_TIMER()	
	ON_WM_ACTIVATE()	
END_MESSAGE_MAP()


// CImageDlg message handlers


void CImageDlg::OnPaint()
{	
	CPaintDC dc(this); // device context for painting
	// TODO: Add your message handler code here
	// Do not call CDialogEx::OnPaint() for painting messages

	
}


BOOL CImageDlg::OnInitDialog()
{
	CDialogEx::OnInitDialog();

	// TODO:  Add extra initialization here
	///////////////////////////////

// 	m_bitmap.LoadBitmap(IDB_BITMAP1);
// 	m_bitmap.GetBitmap(&m_bmp);

	m_pBkgBitmap[SERO] = PngFromResource(MAKEINTRESOURCE(IDB_PNG_BKG), _T("PNG"));
	m_pBkgBitmap[GARO] = PngFromResource(MAKEINTRESOURCE(IDB_PNG_BKG_GARO), _T("PNG"));
	
	CDC *pDC = GetDC();
	m_bitmap[SERO].Attach(Create32BitBitmap(pDC, m_pBkgBitmap[SERO]->GetWidth(), m_pBkgBitmap[SERO]->GetHeight()));	
	m_bitmap[GARO].Attach(Create32BitBitmap(pDC, m_pBkgBitmap[GARO]->GetWidth(), m_pBkgBitmap[GARO]->GetHeight()));	
	ReleaseDC(pDC);

	SetDlgPosition();
	
	OnResizeSkin();
	

	if(InitControlDlg() == FALSE)
		EndDialog(IDCANCEL);
	/*
	hResource = FindResource(AfxGetApp()->m_hInstance,
		MAKEINTRESOURCEW(IDB_RESIZE), _T("PNG"));
	
	DWORD imageSize = SizeofResource(AfxGetApp()->m_hInstance, hResource);
	hGlobal = LoadResource(AfxGetApp()->m_hInstance, hResource);
	pData = LockResource(hGlobal);
	hBuffer = GlobalAlloc(GMEM_MOVEABLE, imageSize);
	LPVOID pBuffer = GlobalLock(hBuffer);
	CopyMemory(pBuffer, pData, imageSize);
	CreateStreamOnHGlobal(hBuffer, TRUE, &pStream);
	pImagePng = new Image(pStream);
	*/

	//비트맵 모양에 맞춰서 다이얼로그 모양 만들기	


// 	hResBack = FindResource(AfxGetApp()->m_hInstance, MAKEINTRESOURCE(IDR_RGN1), _T("RGN"));
// 	hBackGlobal = LoadResource(AfxGetApp()->m_hInstance, hResBack);
// 	m_xScale = m_yScale = 1;
// 
// 	if(hBackGlobal)
// 	{
// 		BYTE *rgndata = (BYTE FAR*)LockResource(hBackGlobal);       
// 		
// 		if (rgndata) 
// 		{
// 			HRGN rgn;      
// 			XFORM xform;      
// 			xform.eM11 = (FLOAT) 1;          
// 			xform.eM22 = (FLOAT) 1; 
// 			xform.eM12 = (FLOAT) 0.0;       
// 			xform.eM21 = (FLOAT) 0.0;             
// 			xform.eDx  = (FLOAT) 0;             
// 			xform.eDy  = (FLOAT) 0; 
// 			
// 			rgn = ExtCreateRegion(&xform, sizeof
// 				(RGNDATAHEADER) + (sizeof(RECT) * ((RGNDATA*)rgndata)->rdh.nCount),(RGNDATA*)rgndata);
// 			VERIFY(rgn!=NULL);  // if you want more comprehensive checking - feel free!
// 		/*	::SetWindowRgn(m_hWnd, rgn, TRUE);		*/	
// 			::UnlockResource(hBackGlobal);
// 		}
// 	}
// 	if(hBackGlobal) ::FreeResource(hBackGlobal);
	///////////////////////////////				
		
	
	return FALSE;  // return TRUE unless you set the focus to a control
	// EXCEPTION: OCX Property Pages should return FALSE
}



void CImageDlg::OnResizeSkin(int garosero)
{
	/*
	CDC *pDC = GetDC();
	CDC MemDC;
	MemDC.CreateCompatibleDC(pDC);	
	
	CBitmap * old = MemDC.SelectObject(&m_bitmap);

	
	pDC->SetStretchBltMode(HALFTONE);

	pDC->StretchBlt(0,0,rc->Width(), rc->Height(), &MemDC, 0,0,
  		m_bmp.bmWidth, m_bmp.bmHeight, SRCCOPY);
	*/

// 	BLENDFUNCTION blendfunction= { AC_SRC_OVER, 0, 255, AC_SRC_ALPHA };
// 	UpdateLayeredWindow(pDC, &ptDst, &sz, &MemDC, &ptSrc, RGB(244,244,244), &blendfunction, ULW_ALPHA);
	
//  	pDC->StretchBlt(0,0,rc->Width(), rc->Height(), &MemDC, 0,0,
//  		m_bmp.bmWidth, m_bmp.bmHeight, SRCCOPY);
	
	//MemDC.SelectObject(old);

	
	CDC *pDC = GetDC();
	CDC MemDC;
	MemDC.CreateCompatibleDC(pDC);
	CBitmap *pOldBitmap = MemDC.SelectObject(&m_bitmap[garosero]);
	Graphics gp(MemDC.GetSafeHdc());

 	gp.Clear( Color(0,0,0,0) );
 	gp.SetInterpolationMode( InterpolationModeHighQuality );

	baseRect.DeflateRect(1,1,1,1);		
	gp.DrawImage(m_pBkgBitmap[garosero], 0, 0, baseRect.Width(), baseRect.Height());

	

	POINT ptDst = {0};
		
	SIZE sz = {baseRect.Width(), baseRect.Height()};

	POINT ptSrc = {0};
	
	HDC srcHDC = gp.GetHDC();

	CDC *pSrcDC = CDC::FromHandle(srcHDC);
		
 	BLENDFUNCTION blendfunction= { AC_SRC_OVER, 0, 255, AC_SRC_ALPHA }; 				
	
//	BOOL tr = UpdateLayeredWindow(&dc, ((LPPOINT)&rec), &sz, pSrcDC, &ptSrc, 0, &blendfunction, ULW_ALPHA);

	BOOL tr = ::UpdateLayeredWindow(GetSafeHwnd(), pDC->m_hDC, ((LPPOINT)&baseRect), &sz, srcHDC, &ptSrc, 0, &blendfunction, ULW_ALPHA);	

	
// 	dc.StretchBlt(0,0,rc->Width(), rc->Height(), &MemDC, 0,0,
//   		m_bmp.bmWidth, m_bmp.bmHeight, SRCCOPY);

	gp.ReleaseHDC(srcHDC);
	MemDC.SelectObject(pOldBitmap);
	DeleteDC(MemDC);	
	ReleaseDC(pDC);
}


void CImageDlg::SetDlgPosition(void)
{
	//다이얼로그가 가운데 생성될 수 있도록..
	CWnd * pDesktopWnd = GetDesktopWindow();
	CRect desktopRect;
	pDesktopWnd->GetWindowRect(&desktopRect);

	int dlgHeight = (float)m_pBkgBitmap[SERO]->GetHeight();
	int dlgWidth = (float)m_pBkgBitmap[SERO]->GetWidth();
	
	int top = (desktopRect.Height()/2) - (dlgHeight/2);
	int left = (desktopRect.Width()/2) - (dlgWidth/2);
	MoveWindow(left, top, dlgWidth, dlgHeight);

	
	GetWindowRect(&baseRect);
}




Bitmap * CImageDlg::PngFromResource(const LPTSTR pName, const LPTSTR pType)
{
	Bitmap * bitmap = NULL;
	HRSRC hResource = FindResource(AfxGetApp()->m_hInstance, pName, pType);
	if (!hResource)	return NULL;

	DWORD imageSize = SizeofResource(AfxGetApp()->m_hInstance, hResource);
	if(!imageSize) return NULL;

	const void * pResourceData = LockResource(LoadResource(AfxGetApp()->m_hInstance, hResource));
	
	HGLOBAL hBuffer = GlobalAlloc(GMEM_MOVEABLE, imageSize);
	if(hBuffer)
	{
		void *pBuffer = GlobalLock(hBuffer);
		if(pBuffer)
		{
			CopyMemory(pBuffer, pResourceData, imageSize);

			IStream *pStream = NULL;
			if(CreateStreamOnHGlobal(hBuffer, FALSE, &pStream) == S_OK)
			{
				bitmap = Bitmap::FromStream(pStream);
				pStream->Release();
				if(bitmap)
				{
					if(bitmap->GetLastStatus() != Ok)
					{
						delete bitmap;
						bitmap = NULL;
					}
				}
			}
			GlobalUnlock(hBuffer);
		}
		GlobalFree(hBuffer);
	}
	return bitmap;
}


HBITMAP CImageDlg::Create32BitBitmap(CDC * pDC, int cx, int cy)
{
	BITMAPINFO bi = { 0 };
	bi.bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
	bi.bmiHeader.biWidth = cx;
	bi.bmiHeader.biHeight = cy;
	bi.bmiHeader.biPlanes = 1;
	bi.bmiHeader.biBitCount = 32;
	bi.bmiHeader.biCompression = BI_RGB;
	bi.bmiHeader.biSizeImage = 0;
	bi.bmiHeader.biXPelsPerMeter = 0;
	bi.bmiHeader.biYPelsPerMeter = 0;
	bi.bmiHeader.biClrUsed = 0;
	bi.bmiHeader.biClrImportant = 0;
	return CreateDIBSection(pDC->m_hDC, &bi, DIB_RGB_COLORS, NULL, NULL, 0);
}


BOOL CImageDlg::InitControlDlg(void)
{
 	m_pControlDlg = new CRemotroidServerDlg;	
	m_pControlDlg->m_firstPosition = baseRect;

 	BOOL rt = m_pControlDlg->Create(IDD_REMOTROIDSERVER_DIALOG, this);	
	if(rt == FALSE)
	{
		return FALSE;
	}

	m_pControlDlg->MoveWindow(baseRect);
	m_pControlDlg->SetResizingDlg();
	m_pControlDlg->SetBkgDlg(this);
	
	return TRUE;
}


void CImageDlg::MoveBkgDlg(CRect rect, int garosero)
{
	baseRect = rect;
	OnResizeSkin(garosero);
}


void CImageDlg::OnDestroy()
{
	__super::OnDestroy();
	m_bitmap[SERO].DeleteObject();
	m_bitmap[GARO].DeleteObject();
	delete m_pBkgBitmap[GARO];
	delete m_pBkgBitmap[SERO];
// 	
	// TODO: Add your message handler code here
}


//포커스를 컨트롤 다이얼로그로 옮긴다
void CImageDlg::OnActivate(UINT nState, CWnd* pWndOther, BOOL bMinimized)
{
	__super::OnActivate(nState, pWndOther, bMinimized);
	if(m_pControlDlg != NULL && m_pControlDlg->m_hWnd != NULL)
	{
		m_pControlDlg->SetFocus();
	}
	// TODO: Add your message handler code here
}


void CImageDlg::TrayWindow(int state)
{
	ShowWindow(state);
}
