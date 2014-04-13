#include "StdAfx.h"
#include "PopupDlgMgr.h"


CPopupDlgMgr::CPopupDlgMgr(void)
{
}


CPopupDlgMgr::~CPopupDlgMgr(void)
{
	m_ptrList.RemoveAll();
}


void CPopupDlgMgr::InsertPopDlg(CPopupDlg * pDlg)
{
	m_ptrList.AddTail((void *)pDlg);
}


void CPopupDlgMgr::RemoveAndMove(CPopupDlg * pDlg)
{
	POSITION oldPos = m_ptrList.Find(pDlg);	
	POSITION pos = oldPos;	

	//지워야할 팝업 윈도우 다음 윈도우부터 아래로 이동
	m_ptrList.GetNext(pos);
	while (pos)
	{
		CDialogEx *pDlg  =(CDialogEx *)m_ptrList.GetNext(pos);
		if(pDlg != NULL)
			pDlg->PostMessage(WM_MOVEPOPDLG, 0, 0);
	}		

	//팝업 다이얼로그가 종료될 때 스스로 delete 해주기 때문에 리스트에서만 삭제하면 된다
	m_ptrList.RemoveAt(oldPos);
}


void CPopupDlgMgr::DestroyAllPopupDlg(void)
{
	POSITION pos = m_ptrList.GetHeadPosition();

	while(pos)
	{
		CPopupDlg *pDlg  =(CPopupDlg *)m_ptrList.GetNext(pos);
		if(pDlg != NULL)
		{
			pDlg->m_bFlagAllDestroy = TRUE;
			pDlg->DestroyWindow();		
		}
	}
	m_ptrList.RemoveAll();
}
