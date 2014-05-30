using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace DB_TEST
{
    public class MySQLTest
    {
        MySqlConnection _connect = null;
        String strConnection = "Server=127.0.0.1;Database=cloudphone;Uid=root;pwd=dmsql12!";

        public void OpenDB()
        {
            try
            {
                
                _connect = new MySqlConnection(strConnection);
                _connect.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine("DB Connect Err : " + e.Message);
            }

        }

        public bool CheckClientID(String id)
        {
            List<String> list = new List<String>();
            String query = "select * from cp_client where client_ID ='" + id + "'";
            MySqlCommand cmd = new MySqlCommand(query, _connect);
           // MySqlCommand cmd = new MySqlCommand();

            MySqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                if (rdr["client_ID"].ToString() == id)
                {
                    Console.WriteLine("clientID : {0}", rdr["client_ID"]);
                    return true;
                }
                
            }

            rdr.Close();

            return false;
           
        }

        public void InsertClientID(String id)
        {
            String query = "insert into cp_client(client_ID) value (@client_ID)";
            MySqlCommand cmd = new MySqlCommand();

            cmd.Connection = _connect;
            cmd.CommandText = query;
            cmd.Parameters.Add("@client_ID", MySqlDbType.VarChar, 30);
            cmd.Parameters[0].Value = id;

            cmd.ExecuteNonQuery(); 
        }
        public void DeleteClientID(String clientID)
        {
            String query = "delete from cp_client where client_ID='" + clientID + "'";
            MySqlCommand cmd = new MySqlCommand();

            cmd.Connection = _connect;
            cmd.CommandText = query;
            cmd.Parameters.Add("@client_ID", MySqlDbType.VarChar, 30);
            cmd.Parameters[0].Value = clientID;

            cmd.ExecuteNonQuery(); 
        }

    }

    class Program
    {
       

        public static void Main(string[] args)
        {
            MySQLTest t = new MySQLTest();

            t.OpenDB();
            t.DeleteClientID("jkinject@gmail.com");
           // t.InsertClientID("jkinject@gmail.com");

            bool check = t.CheckClientID("jkinject@gmail.com");
            Console.WriteLine("Check : " + check);

        }

       
    }
}
