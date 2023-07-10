using Azure;
using Azure.Storage.Blobs;
using BlobTest.Services.Abstract;
using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Data;
using System.Threading.Tasks;

namespace BlobTest.Services.Concrete
{
    public class DeleteService : IDeleteService
    {
        public string GetContainername(string filename)
        {
            string fileExtension = filename.Split('.')[1];
            string container;
            switch (fileExtension)
            {
                case "png":
                case "jpg":
                case "jpeg":
                    container = "blobcontainer1";
                    break;
                case "doc":
                case "docx":
                case "pdf":
                    container = "blobcontainer2";
                    break;
                case "xls":
                case "xlsx":
                    container = "blobcontainer3";
                    break;
                case "json":
                    container = "blobcontainer4";
                    break;
                default:
                    container = "blobcontainer5";
                    break;
            }
            return container;
        }

        public bool HasTwo(string Container, string uniqueName)
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
            string sql = $"SELECT `filename` from `file` WHERE `savename` = '{uniqueName}' AND `Container` = '{Container}'";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                bool hasRows = reader.HasRows;
                int rowCount = 0;
                while (reader.Read())
                {
                    rowCount++;
                }
                return rowCount >= 2;
            }
        }

        public async Task DeleteFileBlob(string container, string filename)
        {
            // 创建存储账户连接字符串
            string connectionString = ConfigurationManager.AppSettings["AzureConnectionString"];

            // 创建 Blob 客户端
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            // 获取 Blob 容器
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(container);

            try
            {
                // 删除 Blob
                await containerClient.DeleteBlobAsync(filename);
                Console.WriteLine("Blob deleted successfully");
            }
            catch (RequestFailedException ex)
            {
                // 处理删除操作失败的情况
                Console.WriteLine($"Error deleting blob: {ex.Message}");
            }
        }

        public void DeleteFileSQL(string container, string filename)
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
            string sql = $"DELETE FROM `file` WHERE `filename` = '{filename}' AND `Container` = '{container}'";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            int index = cmd.ExecuteNonQuery();
            if (index > 0)
            {
                Console.WriteLine("success");
            }
            else
            {
                Console.WriteLine("error");
            }
            conn.Close();
        }

        public async Task DeleteFile(string fileName, string uniqueName)
        {
            string container = GetContainername(fileName);
            if (!HasTwo(container, uniqueName))
            {
                await DeleteFileBlob(container, uniqueName);
            }
            DeleteFileSQL(container, fileName);
        }
    }
}
