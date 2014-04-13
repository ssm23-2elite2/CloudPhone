#pragma once
#include "afxcoll.h"


// CEditHistory

class CEditHistory : public CEdit
{
	DECLARE_DYNAMIC(CEditHistory)

public:
	CEditHistory();
	virtual ~CEditHistory();

protected:
	DECLARE_MESSAGE_MAP()


private:
	CPtrList m_StringList;
	BOOL m_bDisableHistory;
	BOOL m_bDisableInternally;
	BOOL m_bInUpdate;
	CString m_strTarget;

public:
	void AddString(CString str);
	void DeleteList(void);
	afx_msg void OnKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags);
	afx_msg void OnEnUpdate();	
};
	


