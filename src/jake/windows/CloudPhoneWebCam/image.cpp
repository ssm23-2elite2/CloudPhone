#include "cloudphone.h"

/**************************************************************************

    Constants

**************************************************************************/
extern UCHAR psyImageBuf_[320*240][3];

//
// Standard definition of EIA-189-A color bars.  The actual color definitions
// are either in CRGB24Synthesizer or CYUVSynthesizer.
//
const COLOR g_ColorBars[] = 
    {WHITE, YELLOW, CYAN, GREEN, MAGENTA, RED, BLUE, BLACK};

const UCHAR CRGB24Synthesizer::Colors [MAX_COLOR][3] = {
    {0, 0, 0},          // BLACK
    {255, 255, 255},    // WHITE
    {0, 255, 255},      // YELLOW
    {255, 255, 0},      // CYAN
    {0, 255, 0},        // GREEN
    {255, 0, 255},      // MAGENTA
    {0, 0, 255},        // RED
    {255, 0, 0},        // BLUE
    {128, 128, 128}     // GREY
};
// u , y, v
const UCHAR CYUVSynthesizer::Colors [MAX_COLOR][3] = {
    {128, 16, 128},     // BLACK
    {128, 235, 128},    // WHITE
    {16, 211, 146},     // YELLOW
    {166, 170, 16},     // CYAN
    {54, 145, 34},      // GREEN
    {202, 106, 222},    // MAGENTA
    {90, 81, 240},      // RED
    {240, 41, 109},     // BLUE
    {128, 125, 128},    // GREY
};

/**************************************************************************

    LOCKED CODE

**************************************************************************/

#ifdef ALLOC_PRAGMA
#pragma code_seg()
#endif // ALLOC_PRAGMA

void CImageSynthesizer::setImage() {
    GetImageLocation (0, 0);
	ULONG w = 320;
	for (ULONG line = 0; line < 240; line++) {
		GetImageLocation (0, line);		
		PUCHAR ImageStart = m_Cursor;	
		ULONG x=0;
		for (; x < w; x++) {
			PutPsyPixel(psyImageBuf_[line * w + x][0], psyImageBuf_[line * w + x][1], psyImageBuf_[line * w + x][2]);
		}
		PUCHAR ImageEnd = m_Cursor;
	}
}
