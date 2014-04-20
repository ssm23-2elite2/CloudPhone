using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.Types;
using MySql.Data.MySqlClient;
using MySql.Data;

namespace Server
{
    class DBConnect
    {
        private CloudPhoneWindow cloudPhoneWindow;
        MySqlConnection _connect = new MySqlConnection();
        String _connectStr = "Server=211.189.20.131;Database=CloudPhoen;Uid;binensky;pwd:dmsql12!";

        public DBConnect(CloudPhoneWindow c)
        {
            cloudPhoneWindow = c;
            cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "DB CONNECT CLASS 실행");
        }

        public void DBCnt()
        {
            try
            {
                _connect.ConnectionString = _connectStr;
                _connect.Open();
            }
            catch (Exception e)
            {
                cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "error", "DB OPEN ERR : " + e.Message);
            }
        }

        public void DBDisCnt()
        {
            if (_connect != null)
            {
                _connect.Dispose();
                _connect.Close();
            }

        }

        public bool CheckClientID(String clientID)
        {
            List<String> list = new List<String>();
            String query = "select count(cp_client) from client_ID where ID ='" + clientID + "'";
            MySqlCommand cmd = new MySqlCommand(query, _connect);
            MySqlDataReader dr = cmd.ExecuteReader();
            try
            {
                while (dr.Read())
                {
                    list.Add(dr.GetString(0));
                }

                if (list.Count() == 1)
                {
                    cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "클라이언트ID 존재. DBConnect 클래스 - CheckClientID");
                    return true;
                }
                else
                {
                    cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "클라이언트ID 없음 , DB에 자동 등록시킨다. DBConnect 클래스 - CheckClientID"); // Insert 자동으로 시킨다 일단.
                    InsertClientID(clientID);
                    return true;
                }
            }
            catch (Exception e)
            {
                cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "error", "CheckClientID 오류 : " + e.Message + ". DBConnect 클래스 - CheckClientID");
                return false;
            }
        }

        public void InsertClientID(String clientID)
        {
            String query = "insert into cp_client (client_ID) values (" + clientID + ")";
            MySqlCommand cmd = new MySqlCommand(query, _connect);
        }

        public void DeleteClientID(String clientID)
        {
            String query = "delete from cp_client where client_ID='" + clientID + "'";
            MySqlCommand cmd = new MySqlCommand(query, _connect);
        }


    }
}
