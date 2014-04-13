#pragma once
#include "json/json.h"
class CAccount
{
public :
	CAccount();
	CAccount(CString strEmail, CString strPasswd);
	~CAccount();
	void SetEmail(CString strEmail);
	void SetPasswd(CString strPasswd);
	CString GetEmail();
	CString GetPasswd();
private:
	CString m_strEmail;
	CString m_strPasswd;
public:
	static CAccount * FromJson(Json::Value& jsonPayload);
};
