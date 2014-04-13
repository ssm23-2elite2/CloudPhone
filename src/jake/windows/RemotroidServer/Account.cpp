#include "StdAfx.h"
#include "Account.h"


CAccount::CAccount()
{}

CAccount::CAccount(CString strEmail, CString strPasswd):m_strEmail(strEmail), m_strPasswd(strPasswd)
{}

CAccount::~CAccount()
{
}

void CAccount::SetEmail(CString strEmail)
{
	m_strEmail = strEmail;
}	
void CAccount::SetPasswd(CString strPasswd)
{
	m_strPasswd = strPasswd;
}
CString CAccount::GetEmail()
{
	return m_strEmail;
}
CString CAccount::GetPasswd()
{
	return m_strPasswd;
}

CAccount * CAccount::FromJson(Json::Value &jsonPayload)
{
	CAccount *account = new CAccount();
	account->SetEmail(CString(jsonPayload.get("email", "").asCString()));
	account->SetPasswd(CString(jsonPayload.get("password", "").asCString()));
	return account;
}
