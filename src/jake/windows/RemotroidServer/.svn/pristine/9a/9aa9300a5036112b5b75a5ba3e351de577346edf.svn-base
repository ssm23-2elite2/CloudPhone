#include "StdAfx.h"
#include "ResizingControl.h"



CResizingControl::CResizingControl(HWND hwnd):m_hReszingWnd(NULL)
	, m_garoSeroState(0)
{
}


CResizingControl::~CResizingControl(void)
{
}


//초기 위치를 받아서 리사이징시 비율을 계산한다
void CResizingControl::InitRatio(HWND hwnd, int left, int top, int width, int height, int dlgWidth, int dlgHeight)
{
	m_hReszingWnd = hwnd;
	ratio[SERO][LEFTRATIO] = (double)((double)left/dlgWidth);
	ratio[SERO][TOPRATIO] = (double)((double)top/dlgHeight);
	ratio[SERO][WIDTHRATIO] = (double)((double)width/dlgWidth);
	ratio[SERO][HEIGHTRATIO] = (double)((double)height/dlgHeight);

	int left_Garo = top;
	int top_Garo = dlgWidth-(left+width);
	int width_Garo = height;
	int height_Garo = width;
	int dlgWidth_garo = dlgHeight;
	int dlgHeight_garo = dlgWidth;

	ratio[GARO][LEFTRATIO] = (double)((double)left_Garo/dlgWidth_garo);
	ratio[GARO][TOPRATIO] = (double)((double)top_Garo/dlgHeight_garo);
	ratio[GARO][WIDTHRATIO] = (double)((double)width_Garo/dlgWidth_garo);
	ratio[GARO][HEIGHTRATIO] = (double)((double)height_Garo/dlgHeight_garo);
	
	::MoveWindow(m_hReszingWnd, left, top, width, height, TRUE);
}


//비율에 맞게 컨트롤 이동
void CResizingControl::ResizingControl(int cx, int cy)
{
	if(m_hReszingWnd == NULL)
		return;

	::MoveWindow(m_hReszingWnd, round(ratio[m_garoSeroState][LEFTRATIO]*cx), round(ratio[m_garoSeroState][TOPRATIO]*cy), 
		round(ratio[m_garoSeroState][WIDTHRATIO]*cx), round(ratio[m_garoSeroState][HEIGHTRATIO]*cy), TRUE);
	::RedrawWindow(m_hReszingWnd, NULL, NULL, RDW_INVALIDATE | RDW_UPDATENOW);

// 	::DeferWindowPos(hdwp, m_hReszingWnd, HWND_TOP, ratio[m_garoSeroState][LEFTRATIO]*cx, round(ratio[m_garoSeroState][TOPRATIO]*cy), 
//  		round(ratio[m_garoSeroState][WIDTHRATIO]*cx), round(ratio[m_garoSeroState][HEIGHTRATIO]*cy), SWP_NOZORDER);

}
