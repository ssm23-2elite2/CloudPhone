#include "StdAfx.h"
#include "VitualEventPacket.h"



CVitualEventPacket::CVitualEventPacket(char eventCode)
	: m_EventCode(eventCode)
	, payloadSize(EVENTCODE_SIZE)
{
	Event = &CVitualEventPacket::TouchUp;
}

CVitualEventPacket::CVitualEventPacket(char eventCode, int xPos, int yPos)
	: m_EventCode(eventCode), m_xPos(xPos), m_yPos(yPos)
	, payloadSize(EVENTCODE_SIZE+XPOSITION_SIZE+YPOSITION_SIZE)
{
	Event = &CVitualEventPacket::SetCoordinates;
}
CVitualEventPacket::CVitualEventPacket(char eventCode, int keyCode)
	: m_EventCode(eventCode), m_keyCode(keyCode)
	, payloadSize(EVENTCODE_SIZE+KEYCODE_SIZE)
{
	Event = &CVitualEventPacket::KeyDownUp;
}

CVitualEventPacket::~CVitualEventPacket(void)
{
}


char* CVitualEventPacket::asByteArray()
{
	return (this->*Event)();
}

//터지점 설정
char* CVitualEventPacket::SetCoordinates()
{	
	wsprintfA(bEventCode, "%2d", m_EventCode);
	wsprintfA(bXPos, "%4d", m_xPos);
	wsprintfA(bYPos, "%4d", m_yPos);

	memcpy(buffer, bEventCode, EVENTCODE_SIZE);
	memcpy(buffer+EVENTCODE_SIZE, bXPos, XPOSITION_SIZE);
	memcpy(buffer+EVENTCODE_SIZE+XPOSITION_SIZE, bYPos, YPOSITION_SIZE);
	

// 	memcpy(buffer, &m_EventCode, sizeof(m_EventCode));
// 	memcpy(buffer+EVENTCODE_SIZE, &m_xPos, sizeof(m_xPos));
// 	memcpy(buffer+EVENTCODE_SIZE+XPOSITION_SIZE, &m_yPos, sizeof(m_yPos));

	return buffer;
}

//키입력시
char* CVitualEventPacket::KeyDownUp()
{
	
	wsprintfA(bEventCode, "%2d", m_EventCode);
	wsprintfA(bKeyCode, "%4d", m_keyCode);

	memcpy(buffer, bEventCode, EVENTCODE_SIZE);
	memcpy(buffer+EVENTCODE_SIZE, bKeyCode, KEYCODE_SIZE);	

// 	memcpy(buffer, &m_EventCode, sizeof(m_EventCode));
// 	memcpy(buffer+EVENTCODE_SIZE, &m_keyCode, sizeof(m_keyCode));
	return buffer;
}

//터치 다운업 시
char* CVitualEventPacket::TouchUp()
{	
	wsprintfA(bEventCode, "%2d", m_EventCode);
	memcpy(buffer, bEventCode, EVENTCODE_SIZE);

//	memcpy(buffer, &m_EventCode, sizeof(m_EventCode));
	return buffer;
}

