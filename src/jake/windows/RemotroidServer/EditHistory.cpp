// EditHistory.cpp : implementation file
//

#include "stdafx.h"
#include "RemotroidServer.h"
#include "EditHistory.h"


// CEditHistory

IMPLEMENT_DYNAMIC(CEditHistory, CEdit)

CEditHistory::CEditHistory()
: m_bDisableHistory(FALSE)
, m_bDisableInternally(FALSE)
, m_bInUpdate(FALSE)
, m_strTarget(_T(""))
{

}

CEditHistory::~CEditHistory()
{
	DeleteList();
}


BEGIN_MESSAGE_MAP(CEditHistory, CEdit)
	ON_WM_KEYDOWN()
	ON_CONTROL_REFLECT(EN_UPDATE, &CEditHistory::OnEnUpdate)
END_MESSAGE_MAP()



// CEditHistory message handlers




void CEditHistory::AddString(CString str)
{
	CString *pStr = new CString(str);
	m_StringList.AddHead((void*)pStr);
}


void CEditHistory::DeleteList(void)
{
	POSITION pos = m_StringList.GetHeadPosition();

	while(pos)
	{
		CString *pStr = (CString *)m_StringList.GetNext(pos);
		delete pStr;
	}
	m_StringList.RemoveAll();
}


void CEditHistory::OnKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	// TODO: Add your message handler code here and/or call default
	if( (nChar ==VK_DELETE) || (nChar == VK_BACK) )
	{
		m_bDisableHistory = TRUE;
		m_bDisableInternally = TRUE;
	}
	else if( m_bDisableHistory && m_bDisableInternally)
	{
		m_bDisableHistory = FALSE;
		m_bDisableInternally = FALSE;
		UpdateWindow();
	}
	CEdit::OnKeyDown(nChar, nRepCnt, nFlags);
}


void CEditHistory::OnEnUpdate()
{
	// TODO:  If this is a RICHEDIT control, the control will not
	// send this notification unless you override the CEdit::OnInitDialog()
	// function to send the EM_SETEVENTMASK message to the control
	// with the ENM_UPDATE flag ORed into the lParam mask.

	// TODO:  Add your control notification handler code here	
	if(m_bInUpdate)
	{
		return;
	}
	int iInputStringLength = 0;

	m_bInUpdate = TRUE;
	
	if(m_StringList.GetSize() && (iInputStringLength = GetWindowTextLength())
		&& (!m_bDisableHistory) )
	{
		POSITION pos = m_StringList.GetHeadPosition();
		
		GetWindowText(m_strTarget);

		while(pos)
		{
			CString *pHistoryStr = (CString *)m_StringList.GetNext(pos);

			if(pHistoryStr && (!_tcsncmp(m_strTarget, *pHistoryStr, iInputStringLength)))
			{
				int		iStart = 0;
				int		iEnd = 0;
				
				GetSel( iStart, iEnd );						// Get Selection (Cursor Position)
				SetWindowText( *pHistoryStr );					// Set New Window Text
				SetSel( iStart, -1 );						// Reset Selection
				
				UpdateWindow();								// Force Update
				
				break;			
			}
		}		
	}	
	m_bInUpdate = FALSE;
}

