// RemotroidServerDlg.cpp : implementation file
//

#include "stdafx.h"
#include "RemotroidServer.h"
#include "RemotroidServerDlg.h"
#include "afxdialogex.h"

#include "RecvFile.h"


#ifdef _DEBUG
#define new DEBUG_NEW
#endif


// CAboutDlg dialog used for App About

class CAboutDlg : public CDialogEx
{
public:
	CAboutDlg();

// Dialog Data
	enum { IDD = IDD_ABOUTBOX };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

// Implementation
protected:
	DECLARE_MESSAGE_MAP()
};

CAboutDlg::CAboutDlg() : CDialogEx(CAboutDlg::IDD)
{
}

void CAboutDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);
}

BEGIN_MESSAGE_MAP(CAboutDlg, CDialogEx)
END_MESSAGE_MAP()




CRemotroidServerDlg::CRemotroidServerDlg(CWnd* pParent /*=NULL*/)
	: CDialogEx(CRemotroidServerDlg::IDD, pParent)	
	, m_pClient(NULL)
	, pRecvThread(NULL)
	, m_isClickedEndBtn(FALSE)
	, pAcceptThread(NULL)
	, pUdpRecvThread(NULL)
	, m_isReadyRecv(FALSE)	
	, isTray(FALSE)	
	, m_bResizing(FALSE)	
	, m_pBkgDlg(NULL)
	, m_bInit(FALSE)
	, m_GaroSeroState(0)
	, pConnectThread(NULL)
	, m_fileTranceiverState(NORMAL)
	, m_isKakaoTalk(FALSE)
	, m_lastScreenState(FALSE)
	, m_isLbuttonDown(FALSE)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);

	m_strEmail = _T("");
	m_strPasswd = _T("");

	
}

CRemotroidServerDlg::~CRemotroidServerDlg()
{
	m_TrayIcon.RemoveIcon();
	DestroyFont();
}

void CRemotroidServerDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_BTN_BACK, m_BackButton);
	DDX_Control(pDX, IDC_BTN_HOME, m_HomeButton);
	DDX_Control(pDX, IDC_BTN_MENU, m_MenuButton);	
	DDX_Control(pDX, IDC_PROGRESS1, m_progressCtrl);
	DDX_Control(pDX, IDC_BTN_TRAY, m_TrayButton);
	DDX_Control(pDX, IDC_BTN_CLOSE, m_CloseButton);
	DDX_Control(pDX, IDC_BTN_POWER, m_PowerButton);
	DDX_Control(pDX, IDC_BTN_VOLUMEDOWN, m_VolumeDownButton);
	DDX_Control(pDX, IDC_BTN_VOLUMNUP, m_VolumeUpButton);
	DDX_Control(pDX, IDC_BTN_EXPLORER, m_ExplorerBtn);

	DDX_Text(pDX, IDC_EDIT_EMAIL, m_strEmail);
	DDX_Text(pDX, IDC_EDIT_PASSWD, m_strPasswd);
	DDX_Control(pDX, IDC_EDIT_EMAIL, m_ctrlEmail);
	DDX_Control(pDX, IDC_EDIT_PASSWD, m_ctrlPasswd);
	DDX_Control(pDX, IDC_BTN_CONNECT, m_btnConnect);
	DDX_Control(pDX, IDC_BTN_FILECANCEL, m_FileCancelButton);
}

BEGIN_MESSAGE_MAP(CRemotroidServerDlg, CDialogEx)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_WM_DESTROY()
	ON_BN_CLICKED(IDOK, &CRemotroidServerDlg::OnBnClickedOk)
	ON_BN_CLICKED(IDCANCEL, &CRemotroidServerDlg::OnBnClickedCancel)	
	ON_WM_DROPFILES()	
	ON_MESSAGE(WM_MYENDRECV, OnEndRecv)
	ON_MESSAGE(WM_MYENDACCEPT, OnEndAccept)
	ON_MESSAGE(WM_READYRECVFILE, OnReadyRecvFile)	
	ON_MESSAGE(WM_CREATEPOPUPDLG, OnCreatePopupDlg)
	ON_MESSAGE(WM_CLOSEPOPDLG, OnClosePopDlg)
	ON_MESSAGE(WM_ICON_NOTIFY, OnTrayNotification)
	ON_MESSAGE(WM_MYDBLCLKTRAY, OnMyDblClkTray)
	
	
	ON_WM_MOUSEMOVE()
	ON_WM_CTLCOLOR()
	ON_WM_KEYDOWN()	
	ON_WM_CHAR()
	ON_WM_LBUTTONUP()
	
	ON_BN_CLICKED(IDC_BTN_BACK, &CRemotroidServerDlg::OnClickedBtnBack)
	ON_BN_CLICKED(IDC_BTN_HOME, &CRemotroidServerDlg::OnClickedBtnHome)
	ON_BN_CLICKED(IDC_BTN_MENU, &CRemotroidServerDlg::OnClickedBtnMenu)		
	ON_WM_KEYUP()
	ON_BN_CLICKED(IDC_BTN_TRAY, &CRemotroidServerDlg::OnBnClickedBtnTray)
	ON_BN_CLICKED(IDC_BTN_CLOSE, &CRemotroidServerDlg::OnBnClickedBtnClose)
	ON_WM_CLOSE()
	
	ON_WM_TIMER()
	
	ON_WM_MOUSEWHEEL()
	ON_WM_NCHITTEST()
	ON_WM_SETFOCUS()
	ON_WM_SETCURSOR()
	
	ON_WM_SIZE()

	ON_WM_MOVE()
	ON_WM_LBUTTONDOWN()
	ON_WM_MOVING()
	ON_BN_CLICKED(IDC_BTN_VOLUMNUP, &CRemotroidServerDlg::OnBnClickedBtnVolumnup)
	ON_BN_CLICKED(IDC_BTN_VOLUMEDOWN, &CRemotroidServerDlg::OnBnClickedBtnVolumedown)
	ON_BN_CLICKED(IDC_BTN_POWER, &CRemotroidServerDlg::OnBnClickedBtnPower)
	ON_BN_CLICKED(IDC_BTN_EXPLORER, &CRemotroidServerDlg::OnBnClickedBtnExplorer)
	
	ON_BN_CLICKED(IDC_BTN_CONNECT, &CRemotroidServerDlg::OnBnClickedBtnConnect)
	ON_BN_CLICKED(IDC_BTN_FILECANCEL, &CRemotroidServerDlg::OnBnClickedBtnFilecancel)
END_MESSAGE_MAP()


// CRemotroidServerDlg message handlers

BOOL CRemotroidServerDlg::OnInitDialog()
{
	CDialogEx::OnInitDialog();

	
	// Add "About..." menu item to system menu.

	// IDM_ABOUTBOX must be in the system command range.
	ASSERT((IDM_ABOUTBOX & 0xFFF0) == IDM_ABOUTBOX);
	ASSERT(IDM_ABOUTBOX < 0xF000);

	CMenu* pSysMenu = GetSystemMenu(FALSE);
	if (pSysMenu != NULL)
	{
		BOOL bNameValid;
		CString strAboutMenu;
		bNameValid = strAboutMenu.LoadString(IDS_ABOUTBOX);
		ASSERT(bNameValid);
		if (!strAboutMenu.IsEmpty())
		{
			pSysMenu->AppendMenu(MF_SEPARATOR);
			pSysMenu->AppendMenu(MF_STRING, IDM_ABOUTBOX, strAboutMenu);
		}
	}

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon

	// TODO: Add extra initialization here
	//��ũ�� ������ ��ġ �� ��Ÿ�� ����

// 	const DWORD dwExStyle =  WS_EX_LAYERED;
// 	ModifyStyleEx(0, dwExStyle );
	
 	//COLORREF cr = GetSysColor(COLOR_BTNFACE);


	//��Ʈ���� ������ ���̾�α� ��� �����ϰ�
	SetBackgroundColor(RGB(1,50,100));
 	SetLayeredWindowAttributes(RGB(1,50,100),0, LWA_COLORKEY);		


	//�ʱ���ġ��
		
	//��ư �� ��ũ�� ��ġ ����
	SetControlPos();


	//�̸��� ����Ʈ �����丮 ����
	InitEditHistory();

/*
	m_UDPServerSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
	if(m_UDPServerSocket == INVALID_SOCKET)
	{
		MessageBox(_T("UDP���� ���� ����"));
		EndDialog(IDCANCEL);
		return TRUE;
	}

	SOCKADDR_IN addr;
	memset(&addr, 0, sizeof(addr));
	addr.sin_addr.s_addr = htonl(INADDR_ANY);
	addr.sin_family = AF_INET;
	addr.sin_port = htons(UDPPORT);

	if(bind(m_UDPServerSocket, (sockaddr *)&addr, sizeof(addr)) == SOCKET_ERROR )
	{
		MessageBox(_T("udp bind error"));
		EndDialog(IDCANCEL);
		return TRUE;
	}
	

	int optVal;
	int optlen = sizeof(optVal);
	getsockopt(m_UDPServerSocket, SOL_SOCKET, SO_RCVBUF, (char*)&optVal, &optlen);

	optVal = optVal * 2;
	setsockopt(m_UDPServerSocket, SOL_SOCKET, SO_RCVBUF, (char*)&optVal, sizeof(optVal));

	pUdpRecvThread = AfxBeginThread(UDPRecvFunc, this);
	//pUdpRecvThread->m_bAutoDelete = FALSE;
	*/
	
	InitServerSocket();
	

	//�ڽ��� IP ������
	PHOSTENT hostinfo;
	char name[255];
	char ip[255];
	if(gethostname(name, sizeof(name)) == 0)
	{
		if((hostinfo = gethostbyname(name)) != NULL)
		{
			strcpy(ip, inet_ntoa(*(struct in_addr *)*hostinfo->h_addr_list));
			m_strMyIp = CA2W(ip);
			screen.m_strMyIp = m_strMyIp;
		}
	}		
	

	CRect systemRc, rc;
	::SystemParametersInfo(SPI_GETWORKAREA, 0, &systemRc, 0);
	GetClientRect(&rc);	
	

	//Ʈ���� ������ ���
	if(!m_TrayIcon.Create(this, WM_ICON_NOTIFY, _T("Remoteroid"), NULL, NULL))
		return -1;
	m_TrayIcon.SetIcon(IDR_MAINFRAME);
	
	SetWindowText(_T("Remoteroid"));

	//������ ����
	TOGGLEKEYS tk;
	SystemParametersInfo(SPI_SETBEEP, false, &tk, 0);	

	//SetTimer(0, 0, NULL);	

	//�̸��� ��Ʈ ����
	InitFont();

	//���� �ۼ��Ž� ����� ���콺 Ŀ�� ���
	fileTransferCursor = (HCURSOR)LoadImage(AfxGetInstanceHandle(), 
		MAKEINTRESOURCE(IDC_CURSOR1), IMAGE_CURSOR, 32, 32, LR_DEFAULTCOLOR);

	//���� �ۼ��� ��⿡ �����ʷ� ���
	fileSender.SetListener(this);
	recvFileClass.SetListener(this);
	
	
	m_bInit = TRUE;
	
	
	return FALSE;  // return TRUE  unless you set the focus to a control
}

BOOL CRemotroidServerDlg::InitServerSocket(void)
{
	m_ServerSocket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
	if(m_ServerSocket == INVALID_SOCKET)
	{
		MessageBox(_T("Server Socket Error"));
		EndDialog(IDCANCEL);
		return TRUE;
	}
	
	SOCKADDR_IN addr;
	memset(&addr, 0, sizeof(addr));
	addr.sin_addr.s_addr = htonl(INADDR_ANY);
	addr.sin_family = AF_INET;
	addr.sin_port = htons(PORT);

	if(bind(m_ServerSocket, (sockaddr*)&addr, sizeof(addr)) == SOCKET_ERROR)
	{
		MessageBox(_T("bind error"));
		EndDialog(IDCANCEL);
		return TRUE;
	}

	

	if(listen(m_ServerSocket , SOMAXCONN) == SOCKET_ERROR)
	{
		MessageBox(_T("listen error"));
		EndDialog(IDCANCEL);
		return TRUE;
	}	
	
	pAcceptThread = AfxBeginThread(AcceptFunc, this);	
	pAcceptThread->m_bAutoDelete = FALSE;
	return 0;
}


void CRemotroidServerDlg::OnSysCommand(UINT nID, LPARAM lParam)
{
	if ((nID & 0xFFF0) == IDM_ABOUTBOX)
	{
		CAboutDlg dlgAbout;
		dlgAbout.DoModal();
	}
	else
	{
		CDialogEx::OnSysCommand(nID, lParam);
	}
}

// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.

void CRemotroidServerDlg::OnPaint()
{
	if (IsIconic())
	{
		CPaintDC dc(this); // device context for painting

		SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

		// Center icon in client rectangle
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// Draw the icon
		dc.DrawIcon(x, y, m_hIcon);		
	}
	else
	{
		CDialogEx::OnPaint();		
	}
}

// The system calls this function to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CRemotroidServerDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}


UINT CRemotroidServerDlg::AcceptFunc(LPVOID pParam)
{
	CRemotroidServerDlg *pDlg = (CRemotroidServerDlg *)pParam;

	SOCKADDR_IN addr;
	memset(&addr, 0, sizeof(addr));
	int iAddrLen = sizeof(addr);
	SOCKET ClientSocket = accept(pDlg->m_ServerSocket, (sockaddr*)&addr, &iAddrLen);
	
	if(ClientSocket == INVALID_SOCKET)
	{		
		return 0;
	}	


	CMyClient *pClient = new CMyClient(ClientSocket);
	pClient->SetNoDelay(TRUE);

	pDlg->SetClientSocket(pClient);
	pDlg->pRecvThread = AfxBeginThread(RecvFunc, pDlg);
	pDlg->pRecvThread->m_bAutoDelete = FALSE;
	
	if(pDlg->m_isClickedEndBtn == FALSE)
	{
		pDlg->PostMessage(WM_MYENDACCEPT, 0, 0);
	}
	return 0;
}


//ȭ�� ������ UDP�� ���ؼ� �Ѵ�.
UINT CRemotroidServerDlg::UDPRecvFunc(LPVOID pParam)
{
	CRemotroidServerDlg *pDlg = (CRemotroidServerDlg *)pParam;
	char packet[MAXSIZE];
	SOCKADDR_IN addr;
	memset(&addr, 0, sizeof(addr));
	int iAddrLen = sizeof(addr);
	while (TRUE)
	{
		memset(packet, 0, sizeof(packet));
		int iRecvLen = recvfrom(pDlg->m_UDPServerSocket, packet, MAXSIZE, NULL, (sockaddr*)&addr, &iAddrLen);
				
		if(iRecvLen < 0)
		{
			break;
		}

		int iOPCode = CUtil::GetOpCode(packet);
		int iPacketSize = CUtil::GetPacketSize(packet);

		char *data = packet+HEADERSIZE;

		switch(iOPCode)
		{
		case OP_SENDJPGINFO:
			//pDlg->SendMessage(WM_RECVJPGINFO, 0, (LPARAM)data);				
			pDlg->screen.SetJpgInfo(data);
			//pDlg->screen.SendMessage(WM_RECVJPGINFO, 0, (LPARAM)data);
			break;
		case OP_SENDJPGDATA:
			//pDlg->SendMessage(WM_RECVJPGDATA, (WPARAM)iPacketSize, (LPARAM)data);
			pDlg->screen.RecvJpgData(data, iPacketSize);
			//pDlg->screen.SendMessage(WM_RECVJPGDATA, iPacketSize, (LPARAM)data);
			break;
		}
	}	
	return 0;
}



UINT CRemotroidServerDlg::RecvFunc(LPVOID pParam)
{	
	CRemotroidServerDlg *pDlg = (CRemotroidServerDlg *)pParam;
	CMyClient *pClient = pDlg->GetClientSocket();
		

	char bPacket[MAXSIZE];			

	CRecvFile& recvFileClass = pDlg->recvFileClass;	
	CTextProgressCtrl& prgressBar = pDlg->m_progressCtrl;

	//�α��� ȭ�� �����
	pDlg->EnableLoginWnd(SUCCESS);
	
	while (TRUE)
	{		
		int iRecvLen = pClient->RecvPacket();		
		if(iRecvLen <= 0)
		{			
			break;
		}

		while(pClient->GetPacket(bPacket))
		{
			int iOPCode = CUtil::GetOpCode(bPacket);

			int iPacketSize = CUtil::GetPacketSize(bPacket);
			
			//packet ũ�Ⱑ 0�̸� ���� ��Ŷ
			if(iPacketSize == 0)
			{				
				pClient->ResetBuffer();
				break;
			}

			char *data = bPacket+HEADERSIZE;

			switch(iOPCode)
			{
			case OP_SENDFILEINFO:				
				if(recvFileClass.RecvFileInfo(data) == INVALID_HANDLE_VALUE)
				{
					//���� ���� ���� �غ� ���нÿ�
					pDlg->m_fileTranceiverState = NORMAL;
				}
				else
				{
					//������ ���� ���� �غ� �Ǹ� req ��û�� �����Ѵ�
					pClient->SendPacket(OP_REQFILEDATA, NULL, 0);
				}
				break;
			case OP_SENDFILEDATA:
				recvFileClass.RecvFileData(data, iPacketSize);				
				break;		
			case OP_SENDJPGINFO:
				//pDlg->screen.SendMessage(WM_RECVJPGINFO, 0, (LPARAM)data);

				//data�� ù��° ����Ʈ���� ���μ��ο� ���� ������ ����ִ�	
				pDlg->TurnGaroSero(data[0]);
				pDlg->screen.SetJpgInfo(data+1);
				break;
			case OP_SENDJPGDATA:
				//pDlg->screen.SendMessage(WM_RECVJPGDATA, iPacketSize, (LPARAM)data);				
				pDlg->screen.RecvJpgData(data, iPacketSize);
				break;
			case OP_REQFILEDATA:
				pDlg->fileSender.SendFileData();
				break;
			case OP_READYSEND:
				pDlg->SendMessage(WM_READYRECVFILE, 0, 0);
				break;
			case OP_SENDDEVICEINFO:				
				pDlg->screen.SendMessage(WM_RECVDEVICEINFO, 0, (LPARAM)data);
				//����̽� ������ �����ϸ� ȭ�� ������ ��û�Ѵ�
				pClient->SendPacket(OP_REQSENDSCREEN, NULL, 0);
				break;
			case OP_KAKAOTALKMSG:
			case OP_SENDNOTIFICATION:
				if(iPacketSize == HEADERSIZE)
					break;

				if(iOPCode == OP_KAKAOTALKMSG)
					pDlg->m_isKakaoTalk = TRUE;
				else
					pDlg->m_isKakaoTalk = FALSE;

				pDlg->SendMessage(WM_CREATEPOPUPDLG, iPacketSize, (LPARAM)data);
				break;
			case OP_COMPLETEFILETRANSFER:
				//���� ������ �Ϸ� �������
				recvFileClass.CloseFileHandle();
				pDlg->m_fileTranceiverState = NORMAL;				
				break;
			case OP_SCREENSTATEON:
				pDlg->SetScreenState(TRUE);
				break;
			case OP_SCREENSTATEOFF:
				pDlg->SetScreenState(FALSE);
				break;
			}
		}
	}
		
	
	recvFileClass.CloseFileHandle();		
	pDlg->fileSender.DeleteFileList();
	pDlg->m_fileTranceiverState = NORMAL;
	pDlg->screen.SetDisconnect();
	pDlg->m_lastScreenState = FALSE;
	

	delete pClient;	

	//���� ��ư�� ���� ���ᰡ �ƴ� Ŭ���̾�Ʈ ��������
	if(pDlg->m_isClickedEndBtn == FALSE)
	{
		pDlg->PostMessage(WM_MYENDRECV, 0, 0);
	}
	return 0;
}


void CRemotroidServerDlg::SetClientSocket(CMyClient * pClient)
{
	m_pClient = pClient;
	fileSender.SetClient(pClient);
	m_fileTranceiverState = NORMAL;
	screen.InitDrawJpg();
	screen.SetClient(pClient);
	screen.m_isScreenON = TRUE;
	m_lastScreenState = TRUE;
}

CMyClient * CRemotroidServerDlg::GetClientSocket(void)
{
	return m_pClient;
}





void CRemotroidServerDlg::OnDestroy()
{		
	// TODO: Add your message handler code here		
	m_isClickedEndBtn = TRUE;
	EndAccept();
	EndConnect();

	DestroyCursor(fileTransferCursor);


	CDialogEx::OnDestroy();	
}


void CRemotroidServerDlg::OnBnClickedOk()
{
	// TODO: Add your control notification handler code here	
//	CDialogEx::OnOK();
}


void CRemotroidServerDlg::OnBnClickedCancel()
{
	// TODO: Add your control notification handler code here	
	CDialogEx::OnCancel();
}


void CRemotroidServerDlg::OnDropFiles(HDROP hDropInfo)
{
	// TODO: Add your message handler code here and/or call default
	
	//������ �ȵǾ� �ְų� ������ �ۼ��� ���̶�� FALSE
	if(pRecvThread == NULL || m_fileTranceiverState != NORMAL)
	{
		return;
	}

	m_fileTranceiverState = SENDING;
		

	TCHAR path[MAX_PATH];
	memset(path, 0, sizeof(path));
	int count = 0;

	count = DragQueryFile(hDropInfo, 0xffffffff, NULL, 0);
	for(int i=0; i<count; i++)
	{
		CFile *pFile; 
		DragQueryFile(hDropInfo, i, path, MAX_PATH);
		TRY 
		{
			pFile = new CFile(path, CFile::modeRead | CFile::shareDenyRead);
		}
		CATCH (CFileException, e)
		{
			MessageBox(_T("�ٸ� ���α׷����� ������Դϴ�"));			
			continue;
		}
		END_CATCH

		fileSender.AddSendFile(pFile);
	}

	if(fileSender.StartSendFile() == -1)
	{
		fileSender.DeleteFileList();
		m_fileTranceiverState = NORMAL;
	}


	CDialogEx::OnDropFiles(hDropInfo);
}


//////////////////////////////////////////////////////////////////
////������ ���� ���Ḧ ���� �Լ���
void CRemotroidServerDlg::EndAccept(void)
{
	if(pAcceptThread == NULL)
	{
		return;
	}

	closesocket(m_ServerSocket);

	while (TRUE)
	{
		DWORD dwResult = WaitForSingleObject(pAcceptThread->m_hThread, 100);
		if(dwResult !=WAIT_TIMEOUT)
			break;
		MSG msg;
		while (::PeekMessage(&msg, NULL, NULL, NULL,PM_REMOVE))
		{
			::TranslateMessage(&msg);
			::DispatchMessage(&msg);
		}
	}
	
	delete pAcceptThread;
	pAcceptThread = NULL;
}

void CRemotroidServerDlg::EndConnect(void)
{
	if(pRecvThread == NULL)
		return;
	
	m_pClient->CloseSocket();

	while(TRUE)
	{
		DWORD dwResult = WaitForSingleObject(pRecvThread->m_hThread, 100);
		if(dwResult !=WAIT_TIMEOUT)
			break;
		MSG msg;
		while (::PeekMessage(&msg, NULL, NULL, NULL,PM_REMOVE))
		{
			::TranslateMessage(&msg);
			::DispatchMessage(&msg);
		}
	}
	
	delete pRecvThread;
	pRecvThread = NULL;
}


//Ŭ���̾�Ʈ ���� ����� ���� recv ������ ����� ȣ��
LRESULT CRemotroidServerDlg::OnEndRecv(WPARAM wParam, LPARAM lParam)
{
	WaitForSingleObject(pRecvThread->m_hThread, 500);	
	delete pRecvThread;
	pRecvThread = NULL;

	pAcceptThread = AfxBeginThread(AcceptFunc, this);
	pAcceptThread->m_bAutoDelete = FALSE;
	//�α��� �̸��� ����Ʈ ��
	EnableLoginWnd(END);
	
	return LRESULT();
}

LRESULT CRemotroidServerDlg::OnEndAccept(WPARAM wParam, LPARAM lParam)
{
	WaitForSingleObject(pAcceptThread->m_hThread, 500);
	delete pAcceptThread;
	pAcceptThread = NULL;
	return LRESULT();
}

////������ ���� ���Ḧ ���� �Լ���
//////////////////////////////////////////////////////////////////





HBRUSH CRemotroidServerDlg::OnCtlColor(CDC* pDC, CWnd* pWnd, UINT nCtlColor)
{
	HBRUSH hbr = CDialogEx::OnCtlColor(pDC, pWnd, nCtlColor);

	// TODO:  Change any attributes of the DC here
	if(pWnd->GetDlgCtrlID() == 1234)
	{
		pDC->SetBkMode(TRANSPARENT);
		return (HBRUSH)GetStockObject(BLACK_BRUSH);
	}		
	
	// TODO:  Return a different brush if the default is not desired
	return hbr;
}



void CRemotroidServerDlg::OnKeyUp(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	// TODO: Add your message handler code here and/or call default
	
	int keyCode;
	if( (m_pClient != NULL) && ((keyCode=keyCodeGen.GetKeyCode(nChar)) != INVALID_KEYCODE) )
	{		
		CVitualEventPacket event(KEYUP, keyCode);
		m_pClient->SendPacket(OP_VIRTUALEVENT, event.asByteArray(), event.payloadSize);		
	}	
}


void CRemotroidServerDlg::OnKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	// TODO: Add your message handler code here and/or call default	


	//�ѱ۸��� ��ȯ�� �ȵǰ� �ؾ��Ѵ�
	if(nChar == VK_HANGUL)
		CUtil::SetHanEngMode(GetSafeHwnd());	

	int keyCode;
	if( (m_pClient != NULL) && ((keyCode=keyCodeGen.GetKeyCode(nChar)) != INVALID_KEYCODE) )
	{			
		CVitualEventPacket event(KEYDOWN, keyCode);
		m_pClient->SendPacket(OP_VIRTUALEVENT, event.asByteArray(), event.payloadSize);			
	}	
}


BOOL CRemotroidServerDlg::PreTranslateMessage(MSG* pMsg)
{
	// TODO: Add your specialized code here and/or call the base class
	//esc ���� ����
	if(pMsg->wParam == VK_ESCAPE)
		return TRUE;	

	if((pMsg->message == WM_KEYDOWN) || (pMsg->message == WM_KEYUP))
	{
		//�ѿ�Ű ������ ��� ó��
		if(pMsg->wParam == VK_PROCESSKEY)
			pMsg->wParam = ImmGetVirtualKey(GetSafeHwnd());

		
		//��Ʈ�� V ó��
		BOOL bShift = ((GetKeyState(VK_SHIFT) & 0x8000) != 0); // Shift Ű�� ���ȴ����� ���� ����
		BOOL bControl = ((GetKeyState(VK_CONTROL) & 0x8000) != 0); // Control Ű�� ���ȴ����� ���� ����
		BOOL bAlt = ((GetKeyState(VK_MENU) & 0x8000) != 0);
		if(bControl && !bShift && !bAlt && pRecvThread != NULL && pMsg->message == WM_KEYDOWN)
		{
           if(pMsg->wParam == 'v' || pMsg->wParam == 'V')
           {
              // Paste �Լ� ����
			   SendClipboardText();
			   Sleep(100);
           }           
		}
		

		SendMessage(pMsg->message, pMsg->wParam, pMsg->lParam);
	}
	return CDialogEx::PreTranslateMessage(pMsg);
}


void CRemotroidServerDlg::OnChar(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	// TODO: Add your message handler code here and/or call default
	CString str = _T("");
	str.Format(_T("%c"), nChar);
	//MessageBox(str);
 	CDialogEx::OnChar(nChar, nRepCnt, nFlags);
}


////////////////////////////////////////////////////////////
////�巡�� �� ������� ������ ���� �ޱ� ����//////////////////
void CRemotroidServerDlg::GetStoreFilePath(void)
{
	BOOL bResult = FALSE;
	POINT pt;
	memset(&pt, 0, sizeof(pt));
	GetCursorPos(&pt);

	CWnd *wnd = WindowFromPoint(pt);
	WCHAR temp[MAX_PATH];
	CWnd *pParent = NULL;		

	for(pParent = wnd; pParent->GetParent(); pParent = pParent->GetParent());				

	CWnd *pToolbarWnd = pParent->FindWindowEx(pParent->GetSafeHwnd(), NULL, _T("WorkerW"), NULL);  
	if(!pToolbarWnd) goto ENDSEARCH;
	pToolbarWnd = pToolbarWnd->FindWindowEx(pToolbarWnd->GetSafeHwnd(), NULL, _T("ReBarWindow32"), NULL);  
	if(!pToolbarWnd) goto ENDSEARCH;
	pToolbarWnd = pToolbarWnd->FindWindowEx(pToolbarWnd->GetSafeHwnd(), NULL, _T("Address Band Root"), NULL);  
	if(!pToolbarWnd) goto ENDSEARCH;
	pToolbarWnd = pToolbarWnd->FindWindowEx(pToolbarWnd->GetSafeHwnd(), NULL, _T("msctls_progress32"), NULL);  
	if(!pToolbarWnd) goto ENDSEARCH;
	pToolbarWnd = pToolbarWnd->FindWindowEx(pToolbarWnd->GetSafeHwnd(), NULL, _T("Breadcrumb Parent"), NULL);  
	if(!pToolbarWnd) goto ENDSEARCH;
	pToolbarWnd = pToolbarWnd->FindWindowEx(pToolbarWnd->GetSafeHwnd(), NULL, _T("ToolbarWindow32"), NULL);  
	if(!pToolbarWnd) goto ENDSEARCH;

	//Ž���Ⱑ ����Ű�� ���� ��� ȹ��	
	pToolbarWnd->GetWindowText(temp, MAX_PATH);
	if(temp[4] >= _T('A') && temp[4] <= _T('Z'))
		bResult = TRUE;

ENDSEARCH:
	//��θ� ã�� ���ϸ� ����Ʈ��..
	bResult ==  TRUE ? recvFileClass.SetFilePath(temp+4) : recvFileClass.SetDefaultPath();
	return;
}


//�ȵ���̵忡�� ���� ���� ���� ���� �غ�
LRESULT CRemotroidServerDlg::OnReadyRecvFile(WPARAM wParam, LPARAM lParam)
{
	//�ȵ���̵忡�� ��Ŭ�� ���� ��� ����
	if(screen.m_isLbuttonDown == FALSE)
		return 0;

	//������ �ۼ��� ���� ��쿡 ����
	if(m_fileTranceiverState != NORMAL)
		return 0;


	m_fileTranceiverState = RECEIVEING;
	
	//::SetSystemCursor(LoadCursor(0, IDC_HAND), OCR_NORMAL);
	
	SetCapture();		
	SetCursor(fileTransferCursor);
 	

	/*
	SetCapture();	
	AfxLockTempMaps();
	
	Invalidate(FALSE);

	CPoint point;
	GetCursorPos(&point);
	

	imageList = new CImageList;
	

	imageList->Create(50,50,ILC_COLOR32, 0,1);


	POINT p = {0,0};

	//	imageList->Draw(pDC, 0, point, ILD_NORMAL);
	imageList->Add(AfxGetApp()->LoadIcon(IDR_MAINFRAME));
	//imageList->Add(&bitmap, RGB(0,0,0));		

	imageList->BeginDrag(0, 0);

	imageList->DragEnter(GetDesktopWindow(), point);

	HTREEITEM hDropTargetItem = NULL;

	MSG msg;
	while(GetMessage(&msg, NULL, 0, 0))
	{
		if(GetCapture() != this) break;
		switch(msg.message)			
		{
		case WM_LBUTTONDOWN:
		case WM_LBUTTONUP:
			goto ExitDrag;
		case WM_MOUSEMOVE:			
			point.x = (short)LOWORD(msg.lParam);
			point.y = (short)HIWORD(msg.lParam);			
			ClientToScreen(&point);
			imageList->DragMove(point);			
			break;
		default:
			TranslateMessage(&msg);
			DispatchMessage(&msg);
			break;
		}
	}

ExitDrag:
	imageList->DragLeave(GetDesktopWindow());
    imageList->EndDrag();
    delete imageList;

	 Invalidate(FALSE);
    // ȭ���� �����ϰ�
    ReleaseCapture();
    // ���콺 ĸó�� �����ϰ�
    AfxUnlockTempMaps(FALSE);
	*/

	return LRESULT();
}


void CRemotroidServerDlg::OnLButtonUp(UINT nFlags, CPoint point)
{
	// TODO: Add your message handler code here and/or call default
	m_isLbuttonDown = FALSE;
	

	//���� ���� ������ �巡�� �� �� ����� ��쿡 ������
	if(m_fileTranceiverState == RECEIVEING && !m_bResizing)
	{
		
		GetStoreFilePath();
//		SystemParametersInfo(SPI_SETCURSORS, 0, NULL, 0);
		ReleaseCapture();		

		CVitualEventPacket event(TOUCHUP);
		m_pClient->SendPacket(OP_VIRTUALEVENT, event.asByteArray(), event.payloadSize);
		m_pClient->SendPacket(OP_REQFILEINFO, 0, 0);
	}
	else if(m_bResizing)	//�׵θ��� ũ��������
	{				
		m_bResizing = FALSE;
		CRect rect;
		pResizeDlg->GetWindowRect(&mainDlgRect);
		pResizeDlg->ShowWindow(SW_HIDE);
		MoveWindow(mainDlgRect);		
		ReleaseCapture();
	}
	CDialogEx::OnLButtonUp(nFlags, point);
}
/////�巡�� �� ������� ������ ���� �ޱ� ����//////////////////
////////////////////////////////////////////////////////////




void CRemotroidServerDlg::ClickHardwareKey(int state)
{
	SetFocus();	

	if(m_pClient == NULL)
		return;

	CVitualEventPacket event(state);
	m_pClient->SendPacket(OP_VIRTUALEVENT, event.asByteArray(), event.payloadSize);
}

void CRemotroidServerDlg::OnClickedBtnBack()
{
	// TODO: Add your control notification handler code here
	//TurnGaroSero(SERO);
	ClickHardwareKey(BACKBUTTON);	
}
void CRemotroidServerDlg::OnClickedBtnHome()
{
	// TODO: Add your control notification handler code here
	ClickHardwareKey(HOMEBUTTON);
}
void CRemotroidServerDlg::OnClickedBtnMenu()
{
	// TODO: Add your control notification handler code here
	ClickHardwareKey(MENUBUTTON);	
}
void CRemotroidServerDlg::OnBnClickedBtnVolumnup()
{
	// TODO: Add your control notification handler code here
	ClickHardwareKey(VOLUMEUP);
}
void CRemotroidServerDlg::OnBnClickedBtnVolumedown()
{
	// TODO: Add your control notification handler code here		
	ClickHardwareKey(VOLUMEDOWN);
}
void CRemotroidServerDlg::OnBnClickedBtnPower()
{
	// TODO: Add your control notification handler code here
	ClickHardwareKey(POWER);
}
void CRemotroidServerDlg::OnBnClickedBtnExplorer()
{
	// TODO: Add your control notification handler code here

	SetFocus();	

	if(m_pClient == NULL)
		return;

	m_pClient->SendPacket(OP_STARTEXPLORER, NULL, 0);	
}



LRESULT CRemotroidServerDlg::OnCreatePopupDlg(WPARAM wParam, LPARAM lParam)
{
	//tray�� ���� ��츸 �˾� ����
	if(!isTray)
		return 0;

	//�ִ� 10�� ������ �˾� ����
	if(CPopupDlg::numOfDlg == MAXPOPUPDLG)
		return 0;

	char *data = (char*)lParam;
	int payloadSize = wParam-HEADERSIZE;
	TCHAR *notiText = new TCHAR[payloadSize*2];
	memset(notiText, 0, payloadSize*2);
	CUtil::UtfToUni(notiText, data);

	CPopupDlg* pDlg = new CPopupDlg(this);
	pDlg->m_strNoti.Format(_T("%s"), notiText);
	pDlg->pClient = m_pClient;
	pDlg->m_isKakao = m_isKakaoTalk;
	
	delete [] notiText;
	CPopupDlg::numOfDlg++;	
	m_popDlgMgr.InsertPopDlg(pDlg);

	//�˸� �Ҹ� ���
	PlaySound((LPCTSTR)MAKEINTRESOURCE(IDR_WAVE1), NULL, SND_ASYNC|SND_RESOURCE);


	
	pDlg->Create(IDD_POPUPDLG, this);
	pDlg->ShowWindow(SW_HIDE);
	pDlg->SetLayeredWindowAttributes(0, 220, LWA_ALPHA);
	pDlg->AnimateWindow(300, AW_SLIDE | AW_VER_NEGATIVE);		
	
	return LRESULT();
}


//�˾� �����찡 ����� �� ������ �������� ��ġ�� �Ʒ��� ������ ����
LRESULT CRemotroidServerDlg::OnClosePopDlg(WPARAM wParam, LPARAM lParam)
{
	CPopupDlg *pDlg = (CPopupDlg *)wParam;
	m_popDlgMgr.RemoveAndMove(pDlg);
	return LRESULT();
}


//Ȱ��ȭ ��Ű�� �˾� ���̾�α� ��� ����
void CRemotroidServerDlg::DestroyAllPopupDlg()
{
	m_popDlgMgr.DestroyAllPopupDlg();
}



LRESULT CRemotroidServerDlg::OnTrayNotification(WPARAM wParam, LPARAM lParam)
{
	return m_TrayIcon.OnTrayNotification(wParam, lParam);
}

LRESULT CRemotroidServerDlg::OnMyDblClkTray(WPARAM wParam, LPARAM lParam)
{	
	if(isTray == FALSE)
		return 0;

	isTray = FALSE;

	//ȭ���� ���� �ִ� ���¸� ���� ��û �ʿ� ����
	if(m_pClient != NULL && m_lastScreenState == TRUE)
		m_pClient->SendPacket(OP_REQSENDSCREEN, NULL, 0);
	
	CUtil::AniMaximiseFromTray(AfxGetMainWnd()->GetSafeHwnd());
	ShowWindow(SW_RESTORE);

	if(m_pBkgDlg == NULL)
		return 0;
	m_pBkgDlg->TrayWindow(SW_RESTORE);

	//Ȱ��ȭ �ϸ� ��� �˾�â ����
	DestroyAllPopupDlg();
	return LRESULT();
}


void CRemotroidServerDlg::OnBnClickedBtnTray()
{
	// TODO: Add your control notification handler code here
	SetFocus();
	isTray = TRUE;

	//Ʈ���̷� ���� ȭ�������� �����Ѵ�
	//ȭ���� ���� �ִ� ������ ���� 
	if(m_pClient != NULL && m_lastScreenState == TRUE)
		m_pClient->SendPacket(OP_REQSTOPSCREEN, NULL, 0);
	CUtil::AniMinimizeToTray(GetSafeHwnd());	
	ShowWindow(SW_HIDE);	

	if(m_pBkgDlg == NULL)
		return;
	m_pBkgDlg->TrayWindow(SW_HIDE);
}


void CRemotroidServerDlg::OnBnClickedBtnClose()
{
	// TODO: Add your control notification handler code here
	SetFocus();

	PostMessage(WM_CLOSE, 0, 0);
}


void CRemotroidServerDlg::OnClose()
{
	// TODO: Add your message handler code here and/or call default
	
	if(pRecvThread != NULL)
	{
		int ret = MessageBox(_T("���� ���� ���Դϴ�\n�����Ͻðڽ��ϱ�?"), _T("����"), MB_YESNO);
		if(ret == IDNO)		
			return;		
	}
	else
	{
		int ret = MessageBox(_T("�����Ͻðڽ��ϱ�?"), _T("����"), MB_YESNO);
		if(ret == IDNO)
			return;
	}
	GetParent()->PostMessage(WM_CLOSE, 0, 0);
	CDialogEx::OnClose();
}



void CRemotroidServerDlg::OnTimer(UINT_PTR nIDEvent)
{
	// TODO: Add your message handler code here and/or call default
 
	
	KillTimer(0);
	CDialogEx::OnTimer(nIDEvent);
}


BOOL CRemotroidServerDlg::OnMouseWheel(UINT nFlags, short zDelta, CPoint pt)
{
	// TODO: Add your message handler code here and/or call default
	TRACE("nFlgs : %d, zDelta : %d, x : %d, y : %d\n", nFlags, zDelta, pt.x, pt.y);
	return CDialogEx::OnMouseWheel(nFlags, zDelta, pt);
}




void CRemotroidServerDlg::OnSize(UINT nType, int cx, int cy)
{
	CDialogEx::OnSize(nType, cx, cy);
	if(m_bInit == FALSE)
		return;	
			
	m_ResizeContolMgr.ResizingControl(cx, cy, m_GaroSeroState);
	
	GetWindowRect(&mainDlgRect);

	if(m_pBkgDlg == NULL)
		return;

 	m_pBkgDlg->MoveBkgDlg(mainDlgRect, m_GaroSeroState);
	
	// TODO: Add your message handler code here
}



void CRemotroidServerDlg::OnMove(int x, int y)
{
	CDialogEx::OnMove(x, y); 	
	// TODO: Add your message handler code here
	if(m_pBkgDlg != NULL && y == 0)
	{
		RECT rect;
		GetWindowRect(&rect);
		m_pBkgDlg->MoveBkgDlg(rect, m_GaroSeroState);
	}
}

void CRemotroidServerDlg::OnMoving(UINT fwSide, LPRECT pRect)
{	
	CDialogEx::OnMoving(fwSide, pRect);
	mainDlgRect = *pRect;

	if(m_pBkgDlg == NULL)
		return;
	m_pBkgDlg->MoveBkgDlg(mainDlgRect, m_GaroSeroState);	


	// TODO: Add your message handler code here
}



void CRemotroidServerDlg::OnLButtonDown(UINT nFlags, CPoint point)
{
	// TODO: Add your message handler code here and/or call default	
	
	
	//�𼭸��� ���콺�� Ŭ�� ������� ũ�� ���� �׵θ� ���̾�α� �ʱ�ȭ
	//������ ���� ������ ũ�������� ���´�
	if(pRecvThread != NULL && (m_CurCursorState = SetSizeCursor(point)) != -1)
	{
		ClientToScreen(&point);
		m_bResizing = TRUE;
		pResizeDlg->InitResizingDlg(mainDlgRect, point, m_CurCursorState);
		pResizeDlg->ResizingDlg(point);
		pResizeDlg->ShowWindow(SW_SHOW);
		SetCapture();		
	}	
	CDialogEx::OnLButtonDown(nFlags, point);
}


//���̾�α� �̵��� ����..
void CRemotroidServerDlg::OnMouseMove(UINT nFlags, CPoint point)
{
	// TODO: Add your message handler code here and/or call default
	
	CRect screenRect;
	screen.GetWindowRect(&screenRect);
	ScreenToClient(&screenRect);
	CRect moveRect;
	GetClientRect(&moveRect);
	moveRect.bottom = screenRect.top - 20;
	moveRect.left += 40;
	moveRect.right -= 40;

	if(PtInRect(&moveRect, point) && m_isReadyRecv == FALSE )	//�巡��
	{		
		PostMessage( WM_NCLBUTTONDOWN, HTCAPTION, MAKELPARAM( point.x, point.y));
	}
	else if(pRecvThread != NULL && !m_bResizing)	//������ ���� ������ ũ�� ������ ���´�
	{
		SetSizeCursor(point);
	}
	else if(m_bResizing)
	{
		ClientToScreen(&point);
		pResizeDlg->ResizingDlg(point);			
	}
	CDialogEx::OnMouseMove(nFlags, point);
}

//ũ�⺯ȯ�� ������ �׵θ��� ���̾�α�
void CRemotroidServerDlg::SetResizingDlg(void)
{
	pResizeDlg = new CResizingDlg;
	pResizeDlg->Create(IDD_RESIZING, this);	
	
	GetWindowRect(&mainDlgRect);
	pResizeDlg->MoveWindow(&mainDlgRect);
	pResizeDlg->ShowWindow(SW_HIDE);
}

//�����̿� ���콺�� ���� ��� ũ������ ���콺 ����
int CRemotroidServerDlg::SetSizeCursor(CPoint point)
{
	ClientToScreen(&point);
	int result;
	if( (result = CResizingDlg::SearchSide(mainDlgRect, point)) != -1)
	{
		switch (result)
		{
		case HTTOPLEFT:
		case HTBOTTOMRIGHT:
			SetCursor(LoadCursor(NULL, IDC_SIZENWSE));
			break;
		case HTTOPRIGHT:			
		case HTBOTTOMLEFT:
			SetCursor(LoadCursor(NULL, IDC_SIZENESW));
			break;		
		}
	}
	return result;
}


void CRemotroidServerDlg::SetBkgDlg(IParentControl * pBkgDlg)
{
	m_pBkgDlg = pBkgDlg;
}


void CRemotroidServerDlg::PostNcDestroy()
{
	// TODO: Add your specialized code here and/or call the base class
	delete this;
	CDialogEx::PostNcDestroy();
}


void CRemotroidServerDlg::SetControlPos(void)
{


	screen.CreateEx(WS_EX_TOPMOST
		, _T("STATIC"), NULL, WS_CHILD|WS_VISIBLE|SS_NOTIFY, CRect(LEFT, TOP, RIGHT, BOTTOM), this, 1234);	
	screen.SetLayeredWindowAttributes(0, 255, LWA_ALPHA);
	
	

	//���α׷��� �ٿ� ���� ��� ��ư�� ������ ������ ���� ������ �Ѵ�.
	m_progressCtrl.ShowWindow(SW_HIDE);
	m_progressCtrl.SetBarBkColor(RGB(56,58,60));
	m_progressCtrl.SetBarColor(RGB(7,215,7));
	m_progressCtrl.SetTextColor(RGB(255,255,255));	
	m_progressCtrl.SetRange(0, 100);
	m_FileCancelButton.ShowWindow(SW_HIDE);

	recvFileClass.SetProgressBar(&m_progressCtrl);
	fileSender.SetProgressBar(&m_progressCtrl);	
	
	
	m_CloseButton.LoadBitmaps(IDB_BITMAP_CLOSE_MAIN);
	m_CloseButton.SetHoverBitmapID(IDB_BITMAP_HOVER_MAIN);
	m_CloseButton.SetGaroBitmapID(IDB_BITMAP_CLOSE_MAIN_GARO, IDB_BITMAP_HOVER_MAIN_GARO);
	

	m_TrayButton.LoadBitmaps(IDB_BITMAP_TRAYBTN);
	m_TrayButton.SetHoverBitmapID(IDB_BITMAP_TRAY_HOVER);
	m_TrayButton.SetGaroBitmapID(IDB_BITMAP_TRAY_GARO, IDB_BITMAP_TRAY_HOVER_GARO);

	m_ExplorerBtn.LoadBitmaps(IDB_BITMAP_EXPLORER, IDB_BITMAP_EXPLORER_CLICK);
	m_ExplorerBtn.SetHoverBitmapID(IDB_BITMAP_EXPLORER_HOVER); 
	m_ExplorerBtn.SetGaroBitmapID(IDB_BITMAP_EXPLORER_GARO, IDB_BITMAP_EXPLORER_HOVER_GARO, IDB_BITMAP_EXPLORER_CLICK_GARO);

	m_HomeButton.LoadBitmaps(IDB_BITMAP_HOME, IDB_BITMAP_HOME_CLICK);
	m_HomeButton.SetHoverBitmapID(IDB_BITMAP_HOME_OVER);
	m_HomeButton.SetGaroBitmapID(IDB_BITMAP_HOME_GARO, IDB_BITMAP_HOME_OVER_GARO, IDB_BITMAP_HOME_CLICK_GARO);
	
	m_BackButton.LoadBitmaps(IDB_BITMAP_BACK,IDB_BITMAP_BACK_CLICK);
	m_BackButton.SetHoverBitmapID(IDB_BITMAP_BACK_OVER);
	m_BackButton.SetGaroBitmapID(IDB_BITMAP_BACK_GARO, IDB_BITMAP_BACK_OVER_GARO, IDB_BITMAP_BACK_CLICK_GARO);


	m_MenuButton.LoadBitmaps(IDB_BITMAP_MENU, IDB_BITMAP_MENU_CLICK);
	m_MenuButton.SetHoverBitmapID(IDB_BITMAP_MENU_OVER);
	m_MenuButton.SetGaroBitmapID(IDB_BITMAP_MENU_GARO, IDB_BITMAP_MENU_OVER_GARO, IDB_BITMAP_MENU_CLICK_GARO);

	m_VolumeUpButton.LoadBitmaps(IDB_BITMAP_VOLUME, IDB_BITMAP_VOLUME_CLICK);
	m_VolumeUpButton.SetHoverBitmapID(IDB_BITMAP_VOLUME_HOVER);
	m_VolumeUpButton.SetGaroBitmapID(IDB_BITMAP_VOLUME_GARO, IDB_BITMAP_VOLUME_HOVER_GARO, IDB_BITMAP_VOLUME_CLICK_GARO);

	m_VolumeDownButton.LoadBitmaps(IDB_BITMAP_VOLUME, IDB_BITMAP_VOLUME_CLICK);
	m_VolumeDownButton.SetHoverBitmapID(IDB_BITMAP_VOLUME_HOVER);
	m_VolumeDownButton.SetGaroBitmapID(IDB_BITMAP_VOLUME_GARO, IDB_BITMAP_VOLUME_HOVER_GARO, IDB_BITMAP_VOLUME_CLICK_GARO);

	m_PowerButton.LoadBitmaps(IDB_BITMAP_POWER, IDB_BITMAP_POWER_CLICK);
	m_PowerButton.SetHoverBitmapID(IDB_BITMAP_POWER_HOVER);
	m_PowerButton.SetGaroBitmapID(IDB_BITMAP_POWER_GARO, IDB_BITMAP_POWER_HOVER_GARO, IDB_BITMAP_POWER_CLICK_GARO);

	m_FileCancelButton.LoadBitmaps(IDB_BITMAP_CLOSE_MAIN);
	m_FileCancelButton.SetHoverBitmapID(IDB_BITMAP_HOVER_MAIN);
	m_FileCancelButton.SetGaroBitmapID(IDB_BITMAP_CLOSE_MAIN_GARO, IDB_BITMAP_HOVER_MAIN_GARO);
	

	m_btnConnect.MoveWindow(150,470,121,41);
	m_btnConnect.LoadBitmaps(IDB_BITMAP_CONNECTBTN);
	m_btnConnect.SetHoverBitmapID(IDB_BITMAP_CONNECTBTN_HOVER);
	m_btnConnect.SetGaroBitmapID(IDB_BITMAP_CONNECTBTN, IDB_BITMAP_CONNECTBTN_HOVER);
	

	//���̾�α� ũ�� ������ ���� ���� ��Ʈ�� �޴����� ���
	screen.InitRatio(screen.m_hWnd, SCREENLEFT, SCREENTOP, SCREENWIDTH, SCREENHEIGHT, DLGWIDTH, DLGHEIGHT);
	m_progressCtrl.InitRatio(m_progressCtrl.m_hWnd, SCREENLEFT, SCREENTOP-11, SCREENWIDTH-15, 10, DLGWIDTH, DLGHEIGHT);
	m_FileCancelButton.InitRatio(m_FileCancelButton.m_hWnd, SCREENLEFT+SCREENWIDTH-14, SCREENTOP-11, 13, 10, DLGWIDTH, DLGHEIGHT);
	m_MenuButton.InitRatio(m_MenuButton.m_hWnd, SCREENLEFT+20, SCREENBOTTOM+8, BUTTONWIDTH, BUTTONHEIGHT,DLGWIDTH, DLGHEIGHT);
	m_HomeButton.InitRatio(m_HomeButton.m_hWnd, SCREENLEFT+20+BUTTONWIDTH, SCREENBOTTOM+8, BUTTONWIDTH, BUTTONHEIGHT,DLGWIDTH, DLGHEIGHT);
	m_BackButton.InitRatio(m_BackButton.m_hWnd, SCREENLEFT+20+BUTTONWIDTH*2, SCREENBOTTOM+8, BUTTONWIDTH, BUTTONHEIGHT,DLGWIDTH, DLGHEIGHT);
	m_ExplorerBtn.InitRatio(m_ExplorerBtn.m_hWnd, SCREENRIGHT-(23*3), SCREENTOP-70, 23, 20, DLGWIDTH, DLGHEIGHT);
	m_TrayButton.InitRatio(m_TrayButton.m_hWnd, SCREENRIGHT-(23*2), SCREENTOP-70, 19, 16,DLGWIDTH, DLGHEIGHT);
	m_CloseButton.InitRatio(m_CloseButton.m_hWnd, SCREENRIGHT-23, SCREENTOP-70, 19, 16,DLGWIDTH, DLGHEIGHT);
	m_VolumeUpButton.InitRatio(m_VolumeUpButton.m_hWnd, 1, 156, SIDEBTNWIDTH, SIDEBTNHEIGHT, DLGWIDTH, DLGHEIGHT);
	m_VolumeDownButton.InitRatio(m_VolumeDownButton.m_hWnd, 1, 242, SIDEBTNWIDTH, SIDEBTNHEIGHT, DLGWIDTH, DLGHEIGHT);
	m_PowerButton.InitRatio(m_PowerButton.m_hWnd, 416, 163, 10, 52, DLGWIDTH, DLGHEIGHT);
	
	

	m_ResizeContolMgr.InsertControl(&m_progressCtrl);
	m_ResizeContolMgr.InsertControl(&m_FileCancelButton);
	m_ResizeContolMgr.InsertControl(&m_MenuButton);
	m_ResizeContolMgr.InsertControl(&m_HomeButton);
	m_ResizeContolMgr.InsertControl(&m_BackButton);
	m_ResizeContolMgr.InsertControl(&m_TrayButton);
	m_ResizeContolMgr.InsertControl(&m_CloseButton);
	m_ResizeContolMgr.InsertControl(&screen);
	m_ResizeContolMgr.InsertControl(&m_VolumeUpButton);
	m_ResizeContolMgr.InsertControl(&m_VolumeDownButton);
	m_ResizeContolMgr.InsertControl(&m_PowerButton);
	m_ResizeContolMgr.InsertControl(&m_ExplorerBtn);
	
}


void CRemotroidServerDlg::TurnGaroSero(int rotaion)
{
	//���� ���δ� 1, ������ ���δ� 3�̱� ������ ������ ���� ���ηθ� �����Ѵ�
	int garosero = rotaion == 0 ? 0 : 1;

	if(m_GaroSeroState == garosero)
		return;

	m_GaroSeroState = garosero;

	CRect newRect;
	
	newRect.left = (mainDlgRect.left + mainDlgRect.Width()/2) - (mainDlgRect.Height()/2);
	newRect.top = (mainDlgRect.top + mainDlgRect.Height()/2) - (mainDlgRect.Width()/2);
	newRect.right = newRect.left + mainDlgRect.Height();
	newRect.bottom = newRect.top + mainDlgRect.Width();
	mainDlgRect = newRect;
	
	screen.TurnGaroSero(rotaion);

	MoveWindow(mainDlgRect);
}




#include "Web.h"
//////////////////////////////////////////////////////////////////
//�α��� ����
UINT CRemotroidServerDlg::RequireConnectClient(LPVOID pParam)
{
	CRemotroidServerDlg *pDlg = (CRemotroidServerDlg *)pParam;
	

	//Login 
	CResponse *pResponse = CWeb::DoLogin(pDlg->m_strEmail, pDlg->m_strPasswd);
	
	if(!CWeb::GetErrorMsg(pResponse))
	{
		pDlg->bLoginOK = FALSE;
		return 0;	
	}

	//response �κ��� ���� ��ü ����
	CAccount *pAccount = pResponse->GetAccountFromPayload();

	//��� �Ϸ��� reponse ��ü ����
	delete pResponse;

	//��ϵ� ����̽� ���� ��û
	pResponse = CWeb::GetDeviceList(pAccount);
	
	if(!CWeb::GetErrorMsg(pResponse))
	{
		pDlg->bLoginOK = FALSE;
		return 0;
	}


	//�׽�Ʈ������ ����̽� ����Ʈ�߿� ù��°�� ������
	CMyDeviceMgr *pDeviceMgr = pResponse->GetDeviceListFromPayload();
	CDeviceInfo *pDeviceInfo = pDeviceMgr->GetDeviceInfoFromList();

	delete pResponse;

	TRACE(pDlg->m_strMyIp);
	pResponse = CWeb::RequestConnection(pDeviceInfo, pAccount, pDlg->m_strMyIp);
	
	if(!CWeb::GetErrorMsg(pResponse))
	{
		pDlg->bLoginOK = FALSE;
		return 0;	
	}

	delete pResponse;	
	delete pDeviceMgr;
	delete pAccount;
	
	pDlg->bLoginOK = TRUE;

	return 0;
}


UINT CRemotroidServerDlg::StartingWaitingAni(LPVOID pParam)
{
	CRemotroidServerDlg *pDlg = (CRemotroidServerDlg *)pParam;
	pDlg->EnableLoginWnd(START);
	CWinThread *pRequireConnectionThread = AfxBeginThread(RequireConnectClient, pParam);

	WaitForSingleObject(pRequireConnectionThread->m_hThread, INFINITE);		

	if(pDlg->bLoginOK == FALSE)
	{
		pDlg->EnableLoginWnd(FAIL);
	}

	return 0;
}



void CRemotroidServerDlg::OnBnClickedBtnConnect()
{
	// TODO: Add your control notification handler code here

	
	UpdateData(TRUE);
	pConnectThread = AfxBeginThread(StartingWaitingAni, this);	
	m_ctrlEmail.AddString(m_strEmail);
	m_IniMgr.WriteString(m_strEmail);
}


void CRemotroidServerDlg::InitFont(void)
{	
	memset(&lf, 0, sizeof(lf));
	lf.lfHeight = 20;
	
	wsprintf(lf.lfFaceName, _T("���� ���"));

	editFont.CreateFontIndirect(&lf);

	m_ctrlEmail.SetFont(&editFont);
	m_ctrlPasswd.SetFont(&editFont);
}


void CRemotroidServerDlg::DestroyFont(void)
{
	editFont.DeleteObject();
}




//���� �� �� �̸��� edit ��� ���� ��Ʈ�� ����
void CRemotroidServerDlg::EnableLoginWnd(CONNECTSTATE state)
{
	
	switch(state)
	{
	case START:	//���� ��ư ������ ���
		m_ctrlEmail.ShowWindow(SW_SHOW);
		m_ctrlPasswd.ShowWindow(SW_SHOW);

		m_ctrlEmail.EnableWindow(FALSE);
		m_ctrlPasswd.EnableWindow(FALSE);	
		
		m_btnConnect.
		m_btnConnect.EnableWindow(FALSE);
		m_btnConnect.ShowWindow(SW_HIDE);
		screen.EnableAnimation(TRUE);
		break;
	case SUCCESS:	//���� ����			
		m_ctrlEmail.SetReadOnly(TRUE);
		m_ctrlPasswd.SetReadOnly(TRUE);	

		m_ctrlEmail.EnableWindow(FALSE);
		m_ctrlPasswd.EnableWindow(FALSE);	

		m_ctrlEmail.ShowWindow(SW_HIDE);
		m_ctrlPasswd.ShowWindow(SW_HIDE);		

		m_btnConnect.EnableWindow(FALSE);
		m_btnConnect.ShowWindow(SW_HIDE);
		
		screen.EnableAnimation(FALSE);
		break;
	case FAIL:
		m_ctrlEmail.ShowWindow(SW_SHOW);
		m_ctrlPasswd.ShowWindow(SW_SHOW);
		
		m_ctrlEmail.EnableWindow(TRUE);
		m_ctrlPasswd.EnableWindow(TRUE);	

		
		m_btnConnect.ShowWindow(SW_SHOW);
		m_btnConnect.EnableWindow(TRUE);
		screen.EnableAnimation(FALSE);
		break;
	case END:
		MoveWindow(m_firstPosition);
		m_pBkgDlg->MoveBkgDlg(m_firstPosition, SERO);

		m_ctrlEmail.SetReadOnly(FALSE);
		m_ctrlPasswd.SetReadOnly(FALSE);

		m_ctrlEmail.ShowWindow(SW_SHOW);
		m_ctrlPasswd.ShowWindow(SW_SHOW);

		m_ctrlEmail.EnableWindow(TRUE);
		m_ctrlPasswd.EnableWindow(TRUE);	


		m_btnConnect.ShowWindow(SW_SHOW);
		m_btnConnect.EnableWindow(TRUE);

		screen.EnableAnimation(FALSE);
		break;
	}
}


//���� ���� �۽� ���� ���¸� ����
void CRemotroidServerDlg::SetFileTranceiverState(FILETRANCEIVERSTATE state)
{
	m_fileTranceiverState = state;
}

//���� �ۼ����� ���۵Ǹ� ���α׷��� �ٿ� �˽� ��ư Ȱ��ȭ
void CRemotroidServerDlg::StartFileTranceiver(BOOL cond)
{
	if(cond)
	{
		m_progressCtrl.ShowWindow(SW_SHOW);
		m_FileCancelButton.ShowWindow(SW_SHOW);
	}
	else
	{
		m_progressCtrl.ShowWindow(SW_HIDE);
		m_FileCancelButton.ShowWindow(SW_HIDE);
	}
}


void CRemotroidServerDlg::OnBnClickedBtnFilecancel()
{
	// TODO: Add your control notification handler code here

	//������ ������ ���� ��
	if(m_fileTranceiverState == SENDING)
	{
		fileSender.DeleteFileList();
		m_pClient->SendPacket(OP_FILETRANSFERCANCEL, NULL, 0);
	}
	else if(m_fileTranceiverState == RECEIVEING)
	{
		m_pClient->SendPacket(OP_REQSTOPFILETRANFER, NULL, 0);		
	}
}





void CRemotroidServerDlg::SendClipboardText(void)
{
	if(m_pClient == NULL)
		return;

	char *clipboardText = CUtil::GetClipboardText(GetSafeHwnd());

	if(clipboardText == NULL)
		return;

	int len = strlen(clipboardText);

	len = len > 4096-HEADERSIZE ? 4096-HEADERSIZE : len;

	m_pClient->SendPacket(OP_SENDCLIPBOARDTEXT, clipboardText, len);
	delete clipboardText;
}


void CRemotroidServerDlg::SetScreenState(BOOL isScreenOn)
{	
	if(isScreenOn == TRUE && m_lastScreenState == FALSE)
	{
		//Ʈ���� ���°� �ƴ� ��쿡�� ��ũ���� ������ ȭ�� ���� ��û�Ѵ�.
		if(!isTray)
			m_pClient->SendPacket(OP_REQSENDSCREEN, NULL, 0);		
	}
	else if(isScreenOn == FALSE && m_lastScreenState == TRUE)
	{
		if(!isTray)
			m_pClient->SendPacket(OP_REQSTOPSCREEN, NULL, 0);
	}
	screen.SetScreenState(isScreenOn);

	m_lastScreenState = isScreenOn;
}


void CRemotroidServerDlg::InitEditHistory(void)
{
	m_IniMgr.InitialINIFile();
	int count = m_IniMgr.GetCount();

	for(int i=0; i<count; i++)
	{
		m_ctrlEmail.AddString(m_IniMgr.GetHistoryString(i));
	}
}
