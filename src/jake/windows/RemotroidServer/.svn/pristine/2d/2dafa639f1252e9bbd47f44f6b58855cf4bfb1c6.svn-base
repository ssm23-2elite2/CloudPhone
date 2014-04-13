#pragma once
#include "afxcoll.h"
#include "PopupDlg.h"

class CPopupDlgMgr
{
public:
	CPopupDlgMgr(void);
	~CPopupDlgMgr(void);
private:
	CPtrList m_ptrList;
public:
	void InsertPopDlg(CPopupDlg * pDlg);
	void RemoveAndMove(CPopupDlg * pDlg);
	void DestroyAllPopupDlg(void);
};

