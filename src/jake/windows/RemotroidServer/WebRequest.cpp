#include "StdAfx.h"

#include "WebRequest.h"


CWebRequest::CWebRequest()
{
	m_strHeader = DEFAULTHEADER;
	m_strUrl = DEFAULTADDRESS;	
	m_pHttpConnect = NULL;
	m_pFile = NULL;
	port = PORTNUMBER;
	m_pPayload = NULL;
}

CWebRequest::~CWebRequest(void)
{
	Disconnect();
}

void CWebRequest::SetRequestObject(CString strObject)
{
	m_strObject = strObject;		
}

CWebRequest * CWebRequest::AttachPayload(CAccount& account)
{
	Json::StyledWriter writer;
	Json::Value root;
	writer.write(root);	
	
	char *email = CUtil::UniToAnsi(account.GetEmail());
	char *passwd = CUtil::UniToAnsi(account.GetPasswd());

	root["email"] = email;
	root["password"] = passwd;

	delete email;
	delete passwd;

	std::string jsonPayload = root.toStyledString();	

	//json ���ڿ��� utf-8�� ��ȯ
	const char *temp = jsonPayload.c_str();	
	m_pPayload = CUtil::AnsiToUtf(temp);	
	
	return this;
}

//����̽� ����, ���� ������, ���� ������ payload �ۼ�
CWebRequest * CWebRequest::AttachPayload(CDeviceInfo * pDeviceInfo, CAccount * pAccount, CString strServerIP)
{

	Json::StyledWriter writer;
	Json::Value root;
	writer.write(root);	

	char *deviceUUID = CUtil::UniToAnsi(pDeviceInfo->GetUUID());
	char *nickname = CUtil::UniToAnsi(pDeviceInfo->GetNickname());
	char *email = CUtil::UniToAnsi(pAccount->GetEmail());
	char *passwd = CUtil::UniToAnsi(pAccount->GetPasswd());
	char *registrationKey = CUtil::UniToAnsi(pDeviceInfo->GetRegKey());
	char *serverIpAddress = CUtil::UniToAnsi(strServerIP);



	root["device"]["deviceUUID"] = deviceUUID;
	root["device"]["nickname"] = nickname;
	root["device"]["ownerAccount"]["email"] = email;
	root["device"]["ownerAccount"]["password"] = passwd;
	root["device"]["registrationKey"] = registrationKey;
	root["serverIpAddress"] = serverIpAddress;

	delete deviceUUID;
	delete nickname;
	delete email;
	delete passwd;
	delete registrationKey;
	delete serverIpAddress;

	std::string jsonPayload = root.toStyledString();
	const char *temp = jsonPayload.c_str();
	m_pPayload = CUtil::AnsiToUtf(temp);

	return this;
}


//����� payload�� �ۼ��ϰ� ������ post�� ������
CResponse* CWebRequest::SendRequest(void)
{	
	TRY 
	{		
		m_pHttpConnect = m_Session.GetHttpConnection(m_strUrl,port);	
	}
	CATCH (CInternetException, e)
	{
		e->ReportError();		
		return NULL;
	}
	END_CATCH

	TRY 
	{
		m_pFile = m_pHttpConnect->OpenRequest(CHttpConnection::HTTP_VERB_POST, m_strObject);
		m_pFile->SendRequest(m_strHeader, (LPVOID)m_pPayload, strlen(m_pPayload));
		DWORD dwStatusCode;
		m_pFile->QueryInfoStatusCode(dwStatusCode);

		if(dwStatusCode != HTTP_STATUS_OK)
		{
			AfxMessageBox(_T("���� ���� �ҷ�"));			
			return NULL;
		}
	}
	CATCH (CInternetException, e)
	{		
		e->ReportError();			
		return NULL;
	}
	END_CATCH	
	
	return GetResponse();
}


void CWebRequest::Disconnect(void)
{
	if(m_pHttpConnect != NULL)
	{
		delete m_pHttpConnect;
		m_pHttpConnect = NULL;		
	}
	if(m_pFile != NULL)
	{
		delete m_pFile;
		m_pFile = NULL;
	}	
	if(m_pPayload != NULL)
	{
		delete m_pPayload;
		m_pPayload = NULL;
	}
}

//response �����͸� �Ľ��Ѵ�, �� �� utf-8�� ���Źޱ� ������ ��ȭ�۾��� �ʿ�
CResponse * CWebRequest::GetResponse(void)
{
	if(m_pFile == NULL)
		return NULL;

	CResponse *pResponse = NULL;
	char *temp = new char[m_pFile->GetLength()+1];
	memset(temp, 0, m_pFile->GetLength()+1);
	m_pFile->Read(temp, m_pFile->GetLength());

	TCHAR *szResponse = CUtil::UtfToUniEx(temp);
	pResponse = new CResponse();
	

	//�Ľ� 
	if(!pResponse->ParseFromStr(szResponse))
	{
		delete pResponse;
		return NULL;
	}

	delete temp;
	delete szResponse;
	return pResponse;
}


