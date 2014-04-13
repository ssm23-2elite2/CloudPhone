#include "StdAfx.h"
#include "DrawJpg.h"


CDrawJpg::CDrawJpg()
	: m_iTotalJpgSize(0)
	, m_iRecvJpgSize(0)	
	, m_pBitmapDataGaro(NULL)
	, m_GaroSeroState(SERO)
	, m_rotationState(ROTATION0)
{
	m_pBitmapData = new BYTE[MAXRESOLUTION];
	m_pJpgData = new BYTE[MAXRESOLUTION];
	m_pBitmapDataGaro = new BYTE[MAXRESOLUTION];

	memset(&image, 0, sizeof(image));
	if(ijlInit(&image) != IJL_OK)
	{
		TRACE("Cannot initialize Intel JPEG library\n");
	}
}


CDrawJpg::~CDrawJpg(void)
{
	
	delete [] m_pBitmapData;
	delete [] m_pJpgData;
	delete [] m_pBitmapDataGaro;

	if(ijlFree(&image) != IJL_OK)
	{
		TRACE("cannot free intel jpg\n");
	}
}


// 한 프레임의 JPG 크기 정보를 세팅
void CDrawJpg::SetJpgInfo(char * data)
{	
	
	memset(m_bJpgSize, 0, sizeof(m_bJpgSize));
	memcpy(m_bJpgSize, data, JPGSIZELEGNTH-1);
	m_iTotalJpgSize = atoi(m_bJpgSize);

		
	m_iRecvJpgSize = 0;
	memset(m_pJpgData, 0, MAXRESOLUTION);
}

//수신받은 jpg data를 버퍼에 순서대로 저장
void CDrawJpg::RecvJpgData(char * data, int packetSize)
{
	int jpgDataSize = packetSize - HEADERSIZE;
	
	memcpy(m_pJpgData+m_iRecvJpgSize, data, jpgDataSize);
	m_iRecvJpgSize += jpgDataSize;
		
	if(m_iRecvJpgSize == m_iTotalJpgSize)
	{		
		DrawScreen();
		m_iRecvJpgSize = 0;
	}
}


// 한 프레임 jpg 출력
void CDrawJpg::DrawScreen(void)
{	
	if(!SetIJLInfo())
	{
		return;
	}
	SetBitmapInfo();
}

//Intel Jpeg Library 초기화 작업
BOOL CDrawJpg::SetIJLInfo(void)
{
	try
	{		
		image.JPGFile = NULL;
		image.JPGBytes = m_pJpgData;
		image.JPGSizeBytes = m_iTotalJpgSize;

		if((err=ijlRead(&image, IJL_JBUFF_READPARAMS)) != IJL_OK)
		{
			TRACE("cannot read jpeg file header %s\n", ijlErrorStr(err));
			AfxThrowUserException();
		}

		switch(image.JPGChannels)
		{
		case 1:
			image.JPGColor = IJL_G;
			image.DIBChannels = 3;
			image.DIBColor = IJL_BGR;
			break;
		case 3:
			image.JPGColor = IJL_YCBCR;
			image.DIBChannels = 3;
			image.DIBColor    = IJL_BGR;
			break;
		case 4:
			image.JPGColor    = IJL_YCBCRA_FPX;
			image.DIBChannels = 4;
			image.DIBColor    = IJL_RGBA_FPX;
			break;

		default:
			// This catches everything else, but no
			// color twist will be performed by the IJL.
			image.DIBColor = (IJL_COLOR)IJL_OTHER;
			image.JPGColor = (IJL_COLOR)IJL_OTHER;
			image.DIBChannels = image.JPGChannels;
			break;
		}
		image.DIBWidth = image.JPGWidth;
		image.DIBHeight = image.JPGHeight;
		image.DIBPadBytes = IJL_DIB_PAD_BYTES(image.DIBWidth, image.DIBChannels);
		int iImageSize = (image.DIBWidth * image.DIBChannels + image.DIBPadBytes)*
			image.DIBHeight;

		memset(m_pBitmapData, 0, MAXRESOLUTION);
		image.DIBBytes = m_pBitmapData;

		if((err=ijlRead(&image, IJL_JBUFF_READWHOLEIMAGE)) != IJL_OK)
		{
			TRACE("cannot read image data : %s\n", ijlErrorStr(err));
			AfxThrowUserException();
		}
	}	
	catch (CException* e)
	{
		return FALSE;
	}
	return TRUE;
}


void CDrawJpg::SetBitmapInfo(void)
{
	memset(&bmi, 0, sizeof(BITMAPINFO));
	BITMAPINFOHEADER& bih = bmi.bmiHeader;
	memset(&bih, 0, sizeof(BITMAPINFOHEADER));
	bih.biSize = sizeof(BITMAPINFOHEADER);
	bih.biWidth = image.DIBWidth;
	bih.biHeight = -(image.DIBHeight);
	bih.biCompression = BI_RGB;
	bih.biPlanes = 1;
	//픽셀당 비트수.. 컬러수*8비트
	bih.biBitCount = image.DIBChannels*8;
	
	hdc = ::GetDC(this->screenHandle);
// 	memDC = ::CreateCompatibleDC(hdc);
// 	bitmap = ::CreateCompatibleBitmap(hdc,screenXSize,screenXSize);
// 	oldBitmap = (HBITMAP)SelectObject(memDC, bitmap);	
// 		
 	
	::SetStretchBltMode(hdc, HALFTONE);	

	if(m_rotationState != ROTATION0)
	{
		RotationScreen(m_rotationState);
		::StretchDIBits(hdc, 0, 0, screenXSize, screenYSize, 0, 0,
		image.DIBHeight, image.DIBWidth, m_pBitmapDataGaro, &bmi, DIB_RGB_COLORS, SRCCOPY);
	}
	else
	{		
		::StretchDIBits(hdc, 0, 0, screenXSize, screenYSize, 0, 0,
		image.DIBWidth, image.DIBHeight, m_pBitmapData, &bmi, DIB_RGB_COLORS, SRCCOPY);
		
	}
	

// 	::StretchDIBits(memDC, 0, 0, screenXSize, screenYSize, 0, 0,
// 		image.DIBWidth, image.DIBHeight, m_pBitmapData, &bmi, DIB_RGB_COLORS, SRCCOPY);
// 
// 	::BitBlt(hdc,0,0,screenXSize,screenYSize,memDC,0,0,SRCCOPY);
// 
// 	::SelectObject(memDC, oldBitmap);
// 	::DeleteObject(bitmap);
// 	::DeleteDC(memDC);
	::ReleaseDC(screenHandle, hdc);

}

//가로모드일때 화면을 병경해준다
void CDrawJpg::RotationScreen(int rotation)
{
	memset(m_pBitmapDataGaro, 0, MAXRESOLUTION);	
		
 	for(int col=image.DIBWidth-1; col>=0; col--)
	{
		for(int row=0; row<image.DIBHeight; row++)
		{
			int srcPos = 0;
			if(rotation == ROTATION90)
				srcPos = row * image.DIBWidth * image.DIBChannels + col * image.DIBChannels;
			else
				srcPos = ((image.DIBHeight-1-row) * image.DIBWidth) * image.DIBChannels + (image.DIBWidth-1-col) * image.DIBChannels;
				
			
			int desPos = (image.DIBWidth-1-col)*image.DIBHeight*image.DIBChannels + row*image.DIBChannels;
// 			*(m_pBitmapDataGaro+row+( (image.DIBWidth-1-col)*image.DIBHeight) ) = 
// 				*(m_pBitmapData+(row*image.DIBWidth)+col);
			memcpy(m_pBitmapDataGaro+desPos, m_pBitmapData+srcPos, image.DIBChannels);
		}
	}	

	BITMAPINFOHEADER& bih = bmi.bmiHeader;
	bih.biHeight = -(image.DIBWidth);		
	bih.biWidth = image.DIBHeight;
}



void CDrawJpg::InitDrawJpg(HWND screenHandle, int XSize, int YSize)
{		
	this->screenHandle = screenHandle;
	screenXSize = XSize;
	screenYSize = YSize;	
}

