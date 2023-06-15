using System.Data;
using System;
using System.Configuration;
using BlobTest.Services.Abstract;
using System.Security.Principal;
using MySql.Data.MySqlClient;
using System.Web;

namespace BlobTest.Services.Concrete
{
    public class LoginService : ILoginService
    {
        public bool SendinfoToSQL(string email, string pass)
        {
            string server = ConfigurationManager.AppSettings["SQLServer"];
            string database = ConfigurationManager.AppSettings["SQLDatabase"];
            string userId = ConfigurationManager.AppSettings["SQLUserId"];
            string password = ConfigurationManager.AppSettings["SQLPassword"];


            string connectionString = $"server={server};" +
                                      $"port=3306;user id={userId};" +
                                      $"password={password};" +
                                      $"database={database};";

            MySqlConnection conn = new MySqlConnection(connectionString);
            if (conn.State != ConnectionState.Open)
                conn.Open();
            string sql = $"SELECT * FROM `account` WHERE `email` = '{email}' AND `password` = '{pass}'";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                int permission = reader.GetInt32(2);
                HttpContext.Current.Session["email"] = email;
                HttpContext.Current.Session["password"] = pass;
                HttpContext.Current.Session["permission"] = permission;
                conn.Close();
                return true;
            }
            else
            {
                conn.Close();
                return false;
            }
        }
    }
}
