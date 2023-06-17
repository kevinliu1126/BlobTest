using BlobTest.Models;
using BlobTest.Services.Abstract;
using MySql.Data.MySqlClient;
using System.Data;
using System.Collections.Generic;
using System.Web;
using System.Configuration;

namespace BlobTest.Services.Concrete
{
    public class GetFileService : IGetFileService
    {

        public List<GetFileModel> GetFiles(int permission, HttpContextBase httpContext)
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
            string sql = $"SELECT `file_ID`, `filename`, `input_time`, `savename` FROM `file` NATURAL JOIN `account` WHERE `account`.`permission` >= {permission}";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            var files = new List<GetFileModel>();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    files.Add(new GetFileModel
                    {
                        File_ID = reader.GetInt32(0),
                        Filename = reader.GetString(1),
                        Input_time = reader.GetDateTime(2),
                        Save_name = reader.GetString(3),
                    });
                }
            }
            conn.Close();
            return files;
        }
    }
}
