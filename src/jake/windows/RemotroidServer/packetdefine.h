#pragma once

#define PORT				50000
#define UDPPORT				50001
#define MAXSIZE				4096
#define HEADERSIZE			6
#define OPCODESIZE			2
#define TOTALSIZE			4
#define FILENAMESIZE		100
#define FILESIZESIZE		100
#define JPGSIZELEGNTH		10

#define SERO				0
#define GARO				1

#define ROTATION0			0
#define ROTATION90			1
#define ROTATION270			3


#define OP_SENDFILEINFO				1
#define OP_SENDFILEDATA				2
#define OP_SENDJPGINFO				3
#define OP_SENDJPGDATA				4
#define OP_REQFILEDATA				5
#define OP_READYSEND				6
#define OP_REQFILEINFO				7
#define OP_VIRTUALEVENT				8
#define OP_SENDDEVICEINFO			9
#define OP_SENDNOTIFICATION			10
#define OP_REQSENDSCREEN			11
#define OP_REQSTOPSCREEN			12
#define OP_STARTEXPLORER			13
#define OP_KAKAOTALKREPLY			14
#define OP_KAKAOTALKMSG				15

#define OP_COMPLETEFILETRANSFER		16
#define OP_REQSTOPFILETRANFER		17
#define OP_FILETRANSFERCANCEL		18
#define OP_SENDCLIPBOARDTEXT		19
#define OP_SCREENSTATEON			20
#define OP_SCREENSTATEOFF			21



#define WM_RECVJPGINFO		WM_USER+100
#define WM_RECVJPGDATA		WM_USER+101
#define WM_MYENDRECV		WM_USER+102
#define WM_MYENDACCEPT		WM_USER+103
#define WM_READYRECVFILE	WM_USER+104
#define WM_RECVDEVICEINFO	WM_USER+105
#define WM_CREATEPOPUPDLG	WM_USER+106
#define WM_MOVEPOPDLG		WM_USER+107
#define WM_CLOSEPOPDLG		WM_USER+108
#define WM_ICON_NOTIFY		WM_USER+109
#define WM_MYDBLCLKTRAY		WM_USER+110
#define WM_INPUTEDIT		WM_USER+111


