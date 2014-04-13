#pragma once
#include "atlimage.h"
#include "atltypes.h"
#include "afxwin.h"


// CAniStatic

class CAniStatic : public CStatic
{
	DECLARE_DYNAMIC(CAniStatic)

public:
	CAniStatic();
	virtual ~CAniStatic();

protected:
	DECLARE_MESSAGE_MAP()
public:
	
private:	
	CBitmap m_bmp;
	BITMAP m_bmpInfo;
	BLENDFUNCTION bf;
	
public:
	
	afx_msg void OnPaint();
	afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);
	
	int alpahValue;
	int pos;
	void SetAnimation(BOOL cond);
	afx_msg void OnTimer(UINT_PTR nIDEvent);
	CRect myRect;

};


