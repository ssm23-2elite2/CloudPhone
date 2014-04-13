
#pragma once


//어떤 종류의 이벤트인지 설정
#define SETCOORDINATES	0
#define TOUCHDOWN		1
#define TOUCHUP			2
#define KEYDOWN			3
#define KEYUP			4
#define BACKBUTTON		5
#define HOMEBUTTON		6
#define MENUBUTTON		7
#define VOLUMEDOWN		8
#define VOLUMEUP		9
#define POWER			10


#define EVENTCODE_SIZE	2
#define XPOSITION_SIZE	4
#define YPOSITION_SIZE	4
#define KEYCODE_SIZE	4
#define MAXEVENT_SIZE	EVENTCODE_SIZE+XPOSITION_SIZE+YPOSITION_SIZE


class CVitualEventPacket
{
public:
	
	CVitualEventPacket(char eventCode);
	CVitualEventPacket(char eventCode, int xPos, int yPos);
	CVitualEventPacket(char eventCode, int keyCode);
	~CVitualEventPacket(void);

private:
	char m_EventCode;
	int m_xPos;
	int m_yPos;
	int m_keyCode;
 	char bEventCode[EVENTCODE_SIZE+1];
 	char bXPos[XPOSITION_SIZE+1];
 	char bYPos[YPOSITION_SIZE+1];
 	char bKeyCode[KEYCODE_SIZE+1];
	char buffer[MAXEVENT_SIZE];

public:
	char* asByteArray();	
	int payloadSize;

private:
	char* SetCoordinates();
	char* KeyDownUp();	
	char* TouchUp();
	char* (CVitualEventPacket::*Event)();	
};

