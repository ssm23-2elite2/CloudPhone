
// RemotroidServer.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "RemotroidServer.h"
#include "RemotroidServerDlg.h"
#include "Splash.h"

#include "ImageDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


// CRemotroidServerApp

BEGIN_MESSAGE_MAP(CRemotroidServerApp, CWinApp)
	ON_COMMAND(ID_HELP, &CWinApp::OnHelp)
END_MESSAGE_MAP()


// CRemotroidServerApp construction

CRemotroidServerApp::CRemotroidServerApp()
{
	// support Restart Manager
	m_dwRestartManagerSupportFlags = AFX_RESTART_MANAGER_SUPPORT_RESTART;

	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}


// The one and only CRemotroidServerApp object

CRemotroidServerApp theApp;


ULONG_PTR gdiplusToken;

// CRemotroidServerApp initialization

BOOL CRemotroidServerApp::InitInstance()
{
	// InitCommonControlsEx() is required on Windows XP if an application
	// manifest specifies use of ComCtl32.dll version 6 or later to enable
	// visual styles.  Otherwise, any window creation will fail.
	INITCOMMONCONTROLSEX InitCtrls;
	InitCtrls.dwSize = sizeof(InitCtrls);
	// Set this to include all the common control classes you want to use
	// in your application.
	InitCtrls.dwICC = ICC_WIN95_CLASSES;
	InitCommonControlsEx(&InitCtrls);

	CWinApp::InitInstance();


	AfxEnableControlContainer();

	// Create the shell manager, in case the dialog contains
	// any shell tree view or shell list view controls.
	CShellManager *pShellManager = new CShellManager;

	// Standard initialization
	// If you are not using these features and wish to reduce the size
	// of your final executable, you should remove from the following
	// the specific initialization routines you do not need
	// Change the registry key under which our settings are stored
	// TODO: You should modify this string to be something appropriate
	// such as the name of your company or organization
	SetRegistryKey(_T("Local AppWizard-Generated Applications"));

	
	//gdi+
	GdiplusStartupInput gdiplusStartupInput;
	if(::GdiplusStartup(&gdiplusToken, &gdiplusStartupInput, NULL) != Ok)
	{
		AfxMessageBox(_T("gdi+ error"));
		return FALSE;
	}

	WSADATA wd;
	memset(&wd,0, sizeof(wd));
	WSAStartup(MAKEWORD(2,2), &wd);

// 	CSplash splashDlg;
// 	splashDlg.DoModal();

// 	CRemotroidServerDlg dlg;
// 	m_pMainWnd = &dlg;

	CImageDlg dlg;
	m_pMainWnd = &dlg;

	INT_PTR nResponse = dlg.DoModal();
	if (nResponse == IDOK)
	{
		// TODO: Place code here to handle when the dialog is
		//  dismissed with OK
	}
	else if (nResponse == IDCANCEL)
	{
		// TODO: Place code here to handle when the dialog is
		//  dismissed with Cancel
	}

	// Delete the shell manager created above.
	if (pShellManager != NULL)
	{
		delete pShellManager;
	}

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	WSACleanup();
	::GdiplusShutdown(gdiplusToken);

	return FALSE;
}

