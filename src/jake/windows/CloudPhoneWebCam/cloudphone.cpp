#include <stdio.h>
#include <string.h>
#include <windows.h>		//LocalAlloc, LPTR

#define EXPORTDLL extern "C" __declspec(dllexport)

EXPORTDLL char* getMessage();
EXPORTDLL void copyMessage(char * _input, char * _output);
extern "C" void printMessage(char* _input);

//dll ������ ��� �ڵ�� extern "C" �ȿ� ���� �Ǿ�� �Ѵ�.
extern "C"
{
	//C#���� �ҷ��� �� �� ����.
	//dll ���ο����� ȣ���� �����ϴ�.
	char * result = "C++ Dll�κ��� ����";
}

EXPORTDLL char* getMessage()
{
	char * returnchar = "Hello";
	return returnchar;
}

EXPORTDLL void copyMessage(char * _input, char * _output)
{
	strcpy(_output, _input);
}