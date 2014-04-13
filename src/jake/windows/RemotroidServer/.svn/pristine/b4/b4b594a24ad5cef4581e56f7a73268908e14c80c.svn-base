// PopupDlg.cpp : implementation file
//

#include "stdafx.h"
#include "RemotroidServer.h"
#include "PopupDlg.h"
#include "afxdialogex.h"


// CPopupDlg dialog



IMPLEMENT_DYNAMIC(CPopupDlg, CDialogEx)


CPopupDlg::CPopupDlg(CWnd* pParent /*=NULL*/)
	: CDialogEx(CPopupDlg::IDD, pParent)	
	, m_strNoti(_T(""))	
	, m_dlgHeight(0)
	, m_bFlagAllDestroy(FALSE)
	, m_isKakao(FALSE)
	, pClient(NULL)
{

}

CPopupDlg::~CPopupDlg()
{	
}

void CPopupDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_BTN_CLOSE, m_CloseBtn);
	DDX_Text(pDX, IDC_STATIC_TEXT, m_strNoti);
	DDX_Control(pDX, IDC_BTN_SEND, m_btnSend);
}


BEGIN_MESSAGE_MAP(CPopupDlg, CDialogEx)
	ON_BN_CLICKED(IDOK, &CPopupDlg::OnBnClickedOk)
	ON_BN_CLICKED(IDCANCEL, &CPopupDlg::OnBnClickedCancel)
	ON_BN_CLICKED(IDC_BTN_CLOSE, &CPopupDlg::OnBnClickedBtnClose)
	ON_WM_PAINT()
	ON_WM_DESTROY()
	ON_MESSAGE(WM_CTLCOLORSTATIC, OnCtlColorStatic)
	ON_MESSAGE(WM_MOVEPOPDLG, OnMovePopDlg)
	ON_WM_TIMER()
	ON_BN_CLICKED(IDC_BTN_SEND, &CPopupDlg::OnBnClickedBtnSend)
	
END_MESSAGE_MAP()


// CPopupDlg message handlers


void CPopupDlg::OnBnClickedOk()
{
	// TODO: Add your control notification handler code here
	DestroyWindow();
}


void CPopupDlg::OnBnClickedCancel()
{
	// TODO: Add your control notification handler code here
	DestroyWindow();
}


void CPopupDlg::PostNcDestroy()
{
	// TODO: Add your specialized code here and/or call the base class
	delete this;
	CDialogEx::PostNcDestroy();
}

int CPopupDlg::numOfDlg = 0;

BOOL CPopupDlg::OnInitDialog()
{
	CDialogEx::OnInitDialog();
	
	// TODO:  Add extra initialization here
	m_CloseBtn.LoadBitmaps(IDB_BITMAP_CLOSEBTN);
	m_CloseBtn.SetHoverBitmapID(IDB_BITMAP_CLOSEHOVER);
	m_CloseBtn.SetGaroBitmapID(IDB_BITMAP_CLOSEBTN, IDB_BITMAP_CLOSEHOVER);


	m_btnSend.SetWindowPos(&CWnd::wndTopMost, 0, 0, 32, 20, SWP_NOMOVE);
	m_btnSend.LoadBitmaps(IDB_BITMAP_SENDBTN);
	m_btnSend.SetHoverBitmapID(IDB_BITMAP_SENDBTN_HOVER);
	m_btnSend.SetGaroBitmapID(IDB_BITMAP_SENDBTN, IDB_BITMAP_SENDBTN_HOVER);

	//카카오톡 메시지만 답장 버튼 사용
	if(!m_isKakao)
		m_btnSend.ShowWindow(SW_HIDE);


	//배경으로 쓰이는 비트맵 크기 구하기
	CBitmap bgImage;
	BITMAP bmpInfo;
	bgImage.LoadBitmap(IDB_BITMAP_POPUPDLG);
	bgImage.GetBitmap(&bmpInfo);

	//아래로 한칸씩 내려갈 때 사용
	m_dlgHeight = bmpInfo.bmHeight;
	
	//바탕화면 크기 구하기
	CRect systemRc;
	::SystemParametersInfo(SPI_GETWORKAREA, 0, &systemRc, 0);


	//생성될 위치 구하기
	dlgRect.left = systemRc.Width()-bmpInfo.bmWidth;
	dlgRect.top = systemRc.Height()-bmpInfo.bmHeight*numOfDlg;
	dlgRect.right = dlgRect.left + bmpInfo.bmWidth;
	dlgRect.bottom = dlgRect.top + bmpInfo.bmHeight;


	SetWindowPos(&wndTopMost, dlgRect.left, dlgRect.top, dlgRect.Width(), dlgRect.Height(), SWP_SHOWWINDOW);

	CRect rc;
	GetClientRect(&rc);
	m_CloseBtn.MoveWindow(rc.Width()-20, 8, 19, 16);		
	
	return TRUE;  // return TRUE unless you set the focus to a control
	// EXCEPTION: OCX Property Pages should return FALSE
}


void CPopupDlg::OnBnClickedBtnClose()
{
	// TODO: Add your control notification handler code here
	
	DestroyWindow();
}



void CPopupDlg::OnPaint()
{
	CPaintDC dc(this); // device context for painting
	// TODO: Add your message handler code here
	// Do not call CDialogEx::OnPaint() for painting messages	
}


BOOL CPopupDlg::DestroyWindow()
{
	// TODO: Add your specialized code here and/or call the base class
	
	AnimateWindow(200, AW_VER_POSITIVE | AW_HIDE);	
	numOfDlg--;


	//전체 종료일 때는 postmessage 보내면 안됨
	if(!m_bFlagAllDestroy)
		GetParent()->PostMessage(WM_CLOSEPOPDLG, (WPARAM)this, 0);
	
	return CDialogEx::DestroyWindow();
}


void CPopupDlg::OnDestroy()
{	
	// TODO: Add your message handler code here
}

//text static의 배경색을 투명하게 하기 위해
LRESULT CPopupDlg::OnCtlColorStatic(WPARAM wParam, LPARAM lParam)
{
	HDC hdc = (HDC)wParam;
	HWND hwndStatic = (HWND) lParam;

	if(GetDlgItem(IDC_STATIC_TEXT)->m_hWnd == hwndStatic)
	{
		SetBkMode(hdc, TRANSPARENT);
		return (HRESULT)(HBRUSH)GetStockObject(NULL_BRUSH);
	}
	return LRESULT();
}

//밑에 있는 팝업 윈도우가 종료되면 밑으로 한칸 내려가기
LRESULT CPopupDlg::OnMovePopDlg(WPARAM wParam, LPARAM lParam)
{
	m_targetHeight = dlgRect.top + m_dlgHeight;

	SetTimer(0, 10, NULL);
	return LRESULT();
}


void CPopupDlg::OnTimer(UINT_PTR nIDEvent)
{
	// TODO: Add your message handler code here and/or call default
	if(dlgRect.top < m_targetHeight)
	{	
		dlgRect.OffsetRect(0, MOVESTEP);
		SetWindowPos(&wndTopMost, dlgRect.left, dlgRect.top, 0, 0, SWP_NOSIZE|SWP_SHOWWINDOW);
	}
	else
	{
		KillTimer(0);
	}
	CDialogEx::OnTimer(nIDEvent);
}

#include "KakaoPopupDlg.h"
void CPopupDlg::OnBnClickedBtnSend()
{
	// TODO: Add your control notification handler code here
	CKakaoPopupDlg *pDlg = new CKakaoPopupDlg(GetParent());
	pDlg->m_strRecvEdit = m_strNoti;
	pDlg->pClient = pClient;
	pDlg->Create(IDD_KAKAODLG, GetParent());
	pDlg->ShowWindow(SW_SHOW);

	DestroyWindow();
}




void CPopupDlg::SendBtnEnable(BOOL bCond)
{
	m_btnSend.EnableWindow(bCond);
}
