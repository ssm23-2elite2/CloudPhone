// KakaoPopupDlg.cpp : implementation file
//

#include "stdafx.h"
#include "RemotroidServer.h"
#include "KakaoPopupDlg.h"
#include "afxdialogex.h"


// CKakaoPopupDlg dialog

IMPLEMENT_DYNAMIC(CKakaoPopupDlg, CDialogEx)

CKakaoPopupDlg::CKakaoPopupDlg(CWnd* pParent /*=NULL*/)
	: CDialogEx(CKakaoPopupDlg::IDD, pParent)
	, pStream(NULL)
	, pImagePNG(NULL)
	, pClient(NULL)
	
{
	m_TextBoxSize.cx = 0;
	m_TextBoxSize.cy = 0;

	m_BackBitmap.LoadBitmap(IDB_BITMAP_KAKAOBACK);
	m_strRecvEdit = _T("");
	hResource = FindResource(AfxGetApp()->m_hInstance, MAKEINTRESOURCE(IDB_PNG_TEXTBOX), _T("PNG"));

	DWORD imageSize = SizeofResource(AfxGetApp()->m_hInstance, hResource);
	hGlobal = LoadResource(AfxGetApp()->m_hInstance, hResource);
	//리소스를 메모리에 로드
	pData = LockResource(hGlobal);
	//메모리에 로드된 리소스의 포인터 획득
	hBuffer = GlobalAlloc(GMEM_MOVEABLE, imageSize);
	//이미지 싸이즈 만큼 버퍼 획득
	LPVOID pBuffer = GlobalLock(hBuffer);
	//획득한 버터의 포인터
	CopyMemory(pBuffer, pData, imageSize);
	//획득한 힙 메모리에 이미지 복사
	CreateStreamOnHGlobal(hBuffer, TRUE, &pStream);
	pImagePNG = new Image(pStream);
	m_strSendText = _T("");
}

CKakaoPopupDlg::~CKakaoPopupDlg()
{
	if(pStream != NULL)
		pStream->Release();
}

void CKakaoPopupDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);

	DDX_Control(pDX, IDC_EDIT_RECVTEXT, m_ctrlRecvEdit);
	DDX_Text(pDX, IDC_EDIT_RECVTEXT, m_strRecvEdit);
	DDX_Control(pDX, IDC_EDIT_SENDTEXT, m_ctrlSendEdit);
	DDX_Text(pDX, IDC_EDIT_SENDTEXT, m_strSendText);
	DDX_Control(pDX, IDC_BUTTON1, m_SendBtn);
}


BEGIN_MESSAGE_MAP(CKakaoPopupDlg, CDialogEx)
	ON_BN_CLICKED(IDOK, &CKakaoPopupDlg::OnBnClickedOk)
	ON_BN_CLICKED(IDCANCEL, &CKakaoPopupDlg::OnBnClickedCancel)
	ON_BN_CLICKED(IDC_BUTTON1, &CKakaoPopupDlg::OnBnClickedButton1)
	
	
	ON_WM_PAINT()
	ON_MESSAGE(WM_INPUTEDIT, OnInputEdit)
	
	
END_MESSAGE_MAP()


// CKakaoPopupDlg message handlers


void CKakaoPopupDlg::OnBnClickedOk()
{
	// TODO: Add your control notification handler code here
	CDialogEx::OnOK();
}


void CKakaoPopupDlg::OnBnClickedCancel()
{
	// TODO: Add your control notification handler code here
	CDialogEx::OnCancel();
}


void CKakaoPopupDlg::PostNcDestroy()
{
	// TODO: Add your specialized code here and/or call the base class
	delete this;
	CDialogEx::PostNcDestroy();
}


void CKakaoPopupDlg::OnBnClickedButton1()
{
	// TODO: Add your control notification handler code here
	UpdateData(TRUE);
	char *utf = CUtil::UniToUtfEx(m_strSendText);

	if (pClient != NULL)
	{
		pClient->SendPacket(OP_KAKAOTALKREPLY, utf, strlen(utf)); 
	}
	delete utf;

	GetParent()->PostMessage(WM_MYDBLCLKTRAY, 0, 0);
	DestroyWindow();
}



void CKakaoPopupDlg::OnPaint()
{
	CPaintDC dc(this); // device context for painting
	// TODO: Add your message handler code here
	// Do not call CDialogEx::OnPaint() for painting messages
	
	CDC      MemDC;
    BITMAP   bit;
    
    m_BackBitmap.GetBitmap(&bit);
    MemDC.CreateCompatibleDC(&dc);
    MemDC.SelectObject(m_BackBitmap); 
    dc.BitBlt(0, 0, bit.bmWidth, bit.bmHeight, &MemDC, 0, 0, SRCCOPY);
	

	CRect rect;
	GetClientRect(&rect);

	if(pImagePNG->GetLastStatus() != Ok)
		return;

	Graphics gdip(dc.m_hDC);
	gdip.DrawImage(pImagePNG, 15,30, m_TextBoxSize.cx, m_TextBoxSize.cy);
}


BOOL CKakaoPopupDlg::OnInitDialog()
{
	CDialogEx::OnInitDialog();

	// TODO:  Add extra initialization here
	
	m_SendBtn.EnableWindow(FALSE);

	GetTextBoxRect();
	return TRUE;  // return TRUE unless you set the focus to a control
	// EXCEPTION: OCX Property Pages should return FALSE
}


//문자열이 출력될 경우 길이 계산
void CKakaoPopupDlg::GetTextBoxRect(void)
{
	CDC *pDC = GetDC();
	m_TextBoxSize = pDC->GetTextExtent(m_strRecvEdit); 

	CRect rt;
	m_ctrlRecvEdit.GetClientRect(&rt);

	int lineNum = m_TextBoxSize.cx / rt.Width()+1;


	m_TextBoxSize.cx = m_TextBoxSize.cx > TEXTBOXMAXWIDTH ? TEXTBOXMAXWIDTH+30 : m_TextBoxSize.cx+30;
	m_TextBoxSize.cy = m_TextBoxSize.cy * lineNum > rt.Height() ? rt.Height()+20 : m_TextBoxSize.cy * lineNum+20;

}



LRESULT CKakaoPopupDlg::OnInputEdit(WPARAM wParam, LPARAM lParam)
{
	UpdateData(TRUE);

	int a = m_strSendText.GetLength() ;
	if(m_strSendText.GetLength() > 0)
		m_SendBtn.EnableWindow(TRUE);
	else
		m_SendBtn.EnableWindow(FALSE);

	return 0;
}
