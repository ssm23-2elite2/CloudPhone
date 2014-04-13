#pragma once
#include "DrawJpg.h"
#include "MyClient.h"
#include "VitualEventPacket.h"
#include "AniStatic.h"
#include "atlimage.h"
#include "afxwin.h"

#include "ResizingControl.h"


// CScreen
#define WIDTH	360
#define HEIGHT	599

#define WIDTH_LENGTH	4
#define HEIGHT_LENGTH	4

#define LEFT	34
#define TOP		92
#define RIGHT	LEFT+WIDTH
#define BOTTOM	TOP+HEIGHT

#define COORDINATE_TRANSFORM(position, length, resolution)	position * (resolution/length)

class CScreen : public CStatic, public CResizingControl
{
	DECLARE_DYNAMIC(CScreen)

public:
	CScreen();
	virtual ~CScreen();

protected:
	DECLARE_MESSAGE_MAP()

private:
	CDrawJpg drawJpg;	
	CMyClient *pClient;
public:
	void InitDrawJpg(void);
	afx_msg void OnDestroy();
	afx_msg void OnLButtonDown(UINT nFlags, CPoint point);
	afx_msg void OnLButtonUp(UINT nFlags, CPoint point);


	LRESULT OnSetJpgInfo(WPARAM wParam, LPARAM lParam);
	LRESULT OnRecvJpgData(WPARAM wParam, LPARAM lParam);	
	LRESULT OnSetResolution(WPARAM wParam, LPARAM lParam);
	afx_msg void OnMouseMove(UINT nFlags, CPoint point);
			
	void SetClient(CMyClient * pClient);	
	virtual BOOL PreCreateWindow(CREATESTRUCT& cs);	
	
private:
	int widthResolution;
	int heightResolution;
	int width;
	int height;

	inline void CoordinateTransform(CPoint& point);

	BOOL m_bTrack;
	CAniStatic aniWait;
	
public:
	afx_msg void OnMouseLeave();
	void SetJpgInfo(char *data);
	void RecvJpgData(char * data, int iPacketSize);
	afx_msg void OnPaint();
	CString m_strMyIp;
	afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);
	BOOL m_isConnect;
	void SetDisconnect();

	CImage m_bkgImg;
	CFont newFont;
	LOGFONT lf;
	
	afx_msg BOOL OnMouseWheel(UINT nFlags, short zDelta, CPoint pt);
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	afx_msg BOOL OnEraseBkgnd(CDC* pDC);
	
	afx_msg void OnSize(UINT nType, int cx, int cy);	
	
	void ExcludePaintControl(CDC & dc);
	void EnableAnimation(BOOL cond);
	void TurnGaroSero(int garosero);
	int m_rotationState;
	BOOL m_isScreenON;
	void SetScreenState(BOOL isScreenOn);
	CBrush m_BlackBrush;
	BOOL m_isLbuttonDown;
};


