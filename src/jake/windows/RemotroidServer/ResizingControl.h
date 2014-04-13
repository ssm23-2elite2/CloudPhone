#pragma once

/************************************************************************/
/* 컨트롤들리 리사이징이 가능하게 만드는 클래스
/************************************************************************/
#define SERO 0
#define GARO 1

class CResizingControl
{
public:
	CResizingControl(HWND m_hReszingWnd=NULL);
	~CResizingControl(void);
	
public:
	typedef enum
	{
		LEFTRATIO, TOPRATIO, WIDTHRATIO, HEIGHTRATIO
	}MOVEPOS;

	
	void InitRatio(HWND hwnd, int left, int top, int width, int height, int dlgWidth, int dlgHeight);
	void ResizingControl(int cx, int cy);


	double ratio[2][4];
	HWND m_hReszingWnd;

	//반올림
	int round(double d)
	{
		int n = (int)(d+0.5);
		return n;
	}
	int m_garoSeroState;
	
};

