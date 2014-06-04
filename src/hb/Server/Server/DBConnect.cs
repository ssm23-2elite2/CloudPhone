//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using MySql.Data.Types;
//using MySql.Data.MySqlClient;
//using MySql.Data;

//namespace Server
//{
//    public class DBConnect
//    {
//        private CloudPhoneWindow cloudPhoneWindow;
//        MySqlConnection _connect = null;
//        String _connectStr = "Server=127.0.0.1;Database=cloudphone;Uid=root;pwd=dmsql12!";

//        public DBConnect(CloudPhoneWindow c)
//        {
//            cloudPhoneWindow = c;
//           // cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "DB CONNECT CLASS 실행");
//            DBCnt();
//        }

//        public void DBCnt()
//        {
//            try
//            {
//                _connect = new MySqlConnection(_connectStr);
//                _connect.Open();
//                cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "DB OPEN");
//            }
//            catch (Exception e)
//            {
//                cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "error", "DB OPEN ERR : " + e.Message);
//            }
//        }

//        public void DBDisCnt()
//        {
//            if (_connect != null)
//            {
//                _connect.Dispose();
//                _connect.Close();
//            }

//        }

//        public bool CheckClientID(String clientID)
//        {
//            String query = "select * from cp_client where client_ID ='" + clientID + "'";
//            MySqlCommand cmd = new MySqlCommand(query, _connect);

//            MySqlDataReader rdr = cmd.ExecuteReader();
//            try
//            {
//                while (rdr.Read())
//                {
//                    if (rdr["client_ID"].ToString() == clientID)
//                    {
//                        cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "클라이언트ID 존재. DBConnect 클래스 - CheckClientID");
//                        return true;
//                    }

//                }

//                rdr.Close();
//                cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "info", "클라이언트ID 없음 , DB에 자동 등록시킨다. DBConnect 클래스 - CheckClientID"); // Insert 자동으로 시킨다 일단.
//                InsertClientID(clientID);

//                return true;
//            }
//            catch (Exception e)
//            {
//                cloudPhoneWindow.Invoke(cloudPhoneWindow._logMSG, "error", "CheckClientID 오류 : " + e.Message + ". DBConnect 클래스 - CheckClientID");
//                return false;
//            }

//        }

//        public void InsertClientID(String clientID)
//        {
//            String query = "insert into cp_client(client_ID) value (@client_ID)";
//            MySqlCommand cmd = new MySqlCommand();

//            cmd.Connection = _connect;
//            cmd.CommandText = query;
//            cmd.Parameters.Add("@client_ID", MySqlDbType.VarChar, 30);
//            cmd.Parameters[0].Value = clientID;

//            cmd.ExecuteNonQuery();
//        }

//        public void DeleteClientID(String clientID)
//        {
//            String query = "delete from cp_client where client_ID='" + clientID + "'";
//            MySqlCommand cmd = new MySqlCommand();

//            cmd.Connection = _connect;
//            cmd.CommandText = query;
//            cmd.Parameters.Add("@client_ID", MySqlDbType.VarChar, 30);
//            cmd.Parameters[0].Value = clientID;

//            cmd.ExecuteNonQuery();
//        }


//    }
//}
