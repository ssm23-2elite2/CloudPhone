// Splash.cpp : implementation file
//

#include "stdafx.h"
#include "RemotroidServer.h"
#include "Splash.h"
#include "afxdialogex.h"


// CSplash dialog

IMPLEMENT_DYNAMIC(CSplash, CDialogEx)

CSplash::CSplash(CWnd* pParent /*=NULL*/)
	: CDialogEx(CSplash::IDD, pParent)
{

}

CSplash::~CSplash()
{
}

void CSplash::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);
}


BEGIN_MESSAGE_MAP(CSplash, CDialogEx)
	ON_BN_CLICKED(IDOK, &CSplash::OnBnClickedOk)
	ON_BN_CLICKED(IDCANCEL, &CSplash::OnBnClickedCancel)
	ON_WM_TIMER()
END_MESSAGE_MAP()


// CSplash message handlers


void CSplash::OnBnClickedOk()
{
	// TODO: Add your control notification handler code here
	CDialogEx::OnOK();
}


void CSplash::OnBnClickedCancel()
{
	// TODO: Add your control notification handler code here
	CDialogEx::OnCancel();
}


BOOL CSplash::OnInitDialog()
{
	CDialogEx::OnInitDialog();

	// TODO:  Add extra initialization here
	
	//배경으로 쓰이는 비트맵 크기 구하기
	CBitmap bgImage;
	BITMAP bmpInfo;
	bgImage.LoadBitmap(IDB_BITMAP_LOADINNG);
	bgImage.GetBitmap(&bmpInfo);
	MoveWindow(0, 0, bmpInfo.bmWidth, bmpInfo.bmHeight);

	SetTimer(0, 0, NULL);
	return TRUE;  // return TRUE unless you set the focus to a control
	// EXCEPTION: OCX Property Pages should return FALSE
}


void CSplash::OnTimer(UINT_PTR nIDEvent)
{
	// TODO: Add your message handler code here and/or call default
	ShowWindow(SW_HIDE);

	AnimateWindow(300, AW_BLEND);
	Sleep(1000);
	AnimateWindow(300, AW_BLEND | AW_HIDE);
	
 	EndDialog(0);
	CDialogEx::OnTimer(nIDEvent);
}

