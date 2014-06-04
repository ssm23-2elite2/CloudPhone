

typedef enum {

	BLACK = 0,
	WHITE,
	YELLOW,
	CYAN,
	GREEN,
	MAGENTA,
	RED,
	BLUE,
	GREY,

	MAX_COLOR,
	TRANSPARENT,

} COLOR;

#define POSITION_CENTER ((ULONG)-1)


class CImageSynthesizer {

protected:

	//
	// The width and height the synthesizer is set to. 
	//
	ULONG m_Width;
	ULONG m_Height;

	PUCHAR m_SynthesisBuffer;

	PUCHAR m_Cursor;

public:

	virtual void PutPixel(PUCHAR *ImageLocation, COLOR Color) = 0;

	virtual void PutPixel(COLOR Color)
	{
		PutPixel(&m_Cursor, Color);
	}

	virtual void PutMyPixel(UCHAR r, UCHAR g, UCHAR b) = 0;

	virtual PUCHAR GetImageLocation(ULONG LocX, ULONG LocY) = 0;

	void SetImageSize(ULONG Width, ULONG Height)
	{
		m_Width = Width;
		m_Height = Height;
	}

	void SetImage();

	void SetBuffer(PUCHAR SynthesisBuffer)
	{
		m_SynthesisBuffer = SynthesisBuffer;
	}

	CImageSynthesizer() : m_Width(0), m_Height(0), m_SynthesisBuffer(NULL)
	{
	}

	CImageSynthesizer(ULONG Width, ULONG Height) : m_Width(Width), m_Height(Height), m_SynthesisBuffer(NULL)
	{
	}

	virtual ~CImageSynthesizer()
	{
	}

};

/*************************************************

CRGB24Synthesizer

Image synthesizer for RGB24 format.

*************************************************/

class CRGB24Synthesizer : public CImageSynthesizer {

private:

	const static UCHAR Colors[MAX_COLOR][3];

	BOOLEAN m_FlipVertical;

public:

	// Place a pixel at a specific cursor location.  *ImageLocation must
	// reside within the synthesis buffer.
	//
	virtual void PutPixel(PUCHAR *ImageLocation, COLOR Color)
	{
		if (Color != TRANSPARENT) {
			*(*ImageLocation)++ = Colors[(ULONG)Color][0];
			*(*ImageLocation)++ = Colors[(ULONG)Color][1];
			*(*ImageLocation)++ = Colors[(ULONG)Color][2];
		}
		else {
			*ImageLocation += 3;
		}
	}

	// Place a pixel at the default cursor location.  The cursor location
	// must be set via GetImageLocation(x, y).
	// 
	virtual void PutPixel(COLOR Color)
	{
		if (Color != TRANSPARENT) {
			*m_Cursor++ = Colors[(ULONG)Color][1];
			*m_Cursor++ = Colors[(ULONG)Color][1];
			*m_Cursor++ = Colors[(ULONG)Color][2];
		}
		else {
			m_Cursor += 3;
		}
	}

	virtual void PutMyPixel(UCHAR r, UCHAR g, UCHAR b) {
		*m_Cursor++ = r;
		*m_Cursor++ = g;
		*m_Cursor++ = b;
	}

	virtual PUCHAR GetImageLocation(ULONG LocX, ULONG LocY)
	{
		if (m_FlipVertical) {
			return (m_Cursor = (m_SynthesisBuffer + 3 * (LocX + (m_Height - 1 - LocY) * m_Width)));
		}
		else {
			return (m_Cursor = (m_SynthesisBuffer + 3 * (LocX + LocY * m_Width)));
		}
	}

	//
	// DEFAULT CONSTRUCTOR:
	//
	CRGB24Synthesizer(BOOLEAN FlipVertical) : m_FlipVertical(FlipVertical)
	{
	}



	//
	// CONSTRUCTOR:
	//
	CRGB24Synthesizer(BOOLEAN FlipVertical, ULONG Width, ULONG Height) : CImageSynthesizer(Width, Height), m_FlipVertical(FlipVertical)
	{
	}

	//
	// DESTRUCTOR:
	//
	virtual ~CRGB24Synthesizer()
	{
	}

};

/*************************************************

CYUVSynthesizer

Image synthesizer for YUV format.

*************************************************/

class CYUVSynthesizer : public CImageSynthesizer {

private:

	const static UCHAR Colors[MAX_COLOR][3];

	BOOLEAN m_Parity;

public:

	// Place a pixel at a specific cursor location.  *ImageLocation must
	// reside within the synthesis buffer.
	//
	virtual void PutPixel(PUCHAR *ImageLocation, COLOR Color)
	{
		BOOLEAN Parity = (((*ImageLocation - m_SynthesisBuffer) & 0x2) != 0);

#if DBG
		//
		// Check that the current pixel points to a valid start pixel
		// in the UYVY buffer.
		//
		BOOLEAN Odd = (((*ImageLocation - m_SynthesisBuffer) & 0x1) != 0);
		ASSERT((m_Parity && Odd) || (!m_Parity && !Odd));
#endif // DBG

		if (Color != TRANSPARENT) {
			if (Parity) {
				*(*ImageLocation)++ = Colors[(ULONG)Color][2];
			}
			else {
				*(*ImageLocation)++ = Colors[(ULONG)Color][1];
				*(*ImageLocation)++ = Colors[(ULONG)Color][0];
				*(*ImageLocation)++ = Colors[(ULONG)Color][1];
			}
		}
		else {
			*ImageLocation += (Parity ? 1 : 3);
		}

	}

	// Place a pixel at the default cursor location.  The cursor location
	// must be set via GetImageLocation(x, y).
	//
	virtual void PutPixel(COLOR Color)
	{

		if (Color != TRANSPARENT) {
			if (m_Parity) {
				*m_Cursor++ = Colors[(ULONG)Color][2];
			}
			else {
				*m_Cursor++ = Colors[(ULONG)Color][1];
				*m_Cursor++ = Colors[(ULONG)Color][0];
				*m_Cursor++ = Colors[(ULONG)Color][1];
			}
		}
		else {
			m_Cursor += (m_Parity ? 1 : 3);
		}

		m_Parity = !m_Parity;

	}

	virtual void PutMyPixel(UCHAR b, UCHAR g, UCHAR r) {

		UCHAR y = (UCHAR)(((257 * r) + (504 * g) + (95 * b)) / 1000 + 16);
		UCHAR u = (UCHAR)((-(148 * r) - (291 * g) + (499 * b)) / 1000 + 128);
		UCHAR v = (UCHAR)(((439 * r) - (368 * g) - (71 * b)) / 1000 + 128);

		/*
		*m_Cursor++ = y;
		*m_Cursor++ = u;
		*m_Cursor++ = y;
		*m_Cursor++ = v;
		*/

		if (m_Parity){
			*m_Cursor++ = v;
		}
		else{
			*m_Cursor++ = y;
			*m_Cursor++ = u;
			*m_Cursor++ = y;
		}


		m_Parity = !m_Parity;
	}


	virtual PUCHAR GetImageLocation(ULONG LocX, ULONG LocY)
	{

		m_Cursor = m_SynthesisBuffer + ((LocX + LocY * m_Width) << 1);
		if (m_Parity = ((LocX & 1) != 0))
			m_Cursor++;

		return m_Cursor;
	}

	//
	// DEFAULT CONSTRUCTOR:
	//
	CYUVSynthesizer()
	{
	}

	//
	// CONSTRUCTOR:
	//
	CYUVSynthesizer(ULONG Width, ULONG Height) : CImageSynthesizer(Width, Height)
	{
	}

	//
	// DESTRUCTOR:
	//
	virtual ~CYUVSynthesizer()
	{
	}

};


