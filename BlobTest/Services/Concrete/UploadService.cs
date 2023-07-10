using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MySql.Data.MySqlClient;
using BlobTest.Services.Abstract;
using System.Data;
using System.Web.Mvc;
using System.Configuration;
using System;
using System.Web;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;

namespace BlobTest.Services.Concrete
{
    public class UploadService : IUploadService
    {

        public Tuple<string, int> SetContainer(string fileExtension)
        {
            string Container;
            int SQLcontainer;
            switch (fileExtension)
            {
                case ".png":
                case ".jpg":
                case ".jpeg":
                    Container = "blobcontainer1";
                    SQLcontainer = 1;
                    break;
                case ".doc":
                case ".docx":
                case ".pdf":
                    Container = "blobcontainer2";
                    SQLcontainer = 2;
                    break;
                case ".xls":
                case ".xlsx":
                    Container = "blobcontainer3";
                    SQLcontainer = 3;
                    break;
                case ".json":
                    Container = "blobcontainer4";
                    SQLcontainer = 4;
                    break;
                default:
                    Container = "blobcontainer5";
                    SQLcontainer = 5;
                    break;
            }
            return Tuple.Create(Container, SQLcontainer);
        }
        
        [HttpPost]
        public string FileExist(string filename, string email)
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
            string sql = $"SELECT `savename` FROM `file` WHERE `filename` = '{filename}' AND `email` = '{email}'";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                return reader.GetString(0);
            }
            else
            {
                return null;
            }
        }

        [HttpPost]
        public void UploadFileToMySQL(string fileName, string email, string uniqueName, int SQLcontainer)
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
            string sql = $"INSERT INTO `file`(`filename`, `email`, `savename`, `Container`) VALUES ('{fileName}', '{email}', '{uniqueName}', '{SQLcontainer}')";
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

        [HttpPost]
        public void UpdateSQLInfo(string filename, string email, string uniqueName, int SQLcontainer)
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
            string sql = $"UPDATE `file` SET `savename` = '{uniqueName}' WHERE `filename` = '{filename}' AND `email` = '{email}' AND `Container` = '{SQLcontainer}'";
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

        public static byte[] ReadFileContent(HttpPostedFileBase file)
        {
            string temp = string.Empty;

            using (StreamReader reader = new StreamReader(file.InputStream))
            {
                temp = reader.ReadToEnd();
            }
            return Encoding.UTF8.GetBytes(temp);
        }

        public static string CalculateSHA256(HttpPostedFileBase file)
        {
            using (SHA256Managed sha256 = new SHA256Managed())
            {
                byte[] fileContent = ReadFileContent(file);
                byte[] hashBytes = sha256.ComputeHash(fileContent);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2")); // 將每個位元組轉換為16進位字串
                }

                return sb.ToString();
            }
        }

        public async Task<string> CompareSHA(string SHA, BlobContainerClient containerClient, string fileExtension)
        {
            foreach (BlobItem blobItem in containerClient.GetBlobs())
            {
                BlobClient blobClient = containerClient.GetBlobClient(blobItem.Name);
                BlobProperties properties = await blobClient.GetPropertiesAsync();
                IDictionary<string, string> metadata = properties.Metadata;

                Console.WriteLine($"Blob: {blobItem.Name}");
                string blobExtension = System.IO.Path.GetExtension(blobItem.Name);
                // 獲取 Metadata 鍵值對
                foreach (var item in metadata)
                {
                    string key = item.Key;
                    string value = item.Value;
                    if(key == "SHA")
                    {
                        if(value == SHA && blobExtension == fileExtension)
                        {
                            return blobItem.Name;
                        }
                    }
                }
            }
            return null;
        }

        public async Task UploadFileAsync(HttpPostedFileBase file, HttpContextBase httpContext)
        {
            string fileExtension = System.IO.Path.GetExtension(file.FileName);
            string email = httpContext.Session["email"] as string;
            string password = httpContext.Session["password"] as string;
            if (email == null || password == null)
            {
                return;
            }
            Tuple<string, int> result = SetContainer(fileExtension);
            string Container = result.Item1;
            int SQLcontainer = result.Item2;
            string ConnectionString = ConfigurationManager.AppSettings["AzureConnectionString"];
            var uniqueName = Guid.NewGuid().ToString() + fileExtension;
            BlobServiceClient blobServiceClient = new BlobServiceClient(ConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(Container);
            string SHA = CalculateSHA256(file);
            Task<string> SHAexist = CompareSHA(SHA, containerClient, fileExtension);
            if(await SHAexist != null)
            {
                UploadFileToMySQL(file.FileName, email, SHAexist.Result, SQLcontainer);
                Console.WriteLine("File content exist!!!");
            }
            else
            {
                using (Stream fileStream = file.InputStream)
                {
                    var response = await containerClient.UploadBlobAsync(uniqueName, fileStream);
                }
                var blobClient = containerClient.GetBlobClient(uniqueName);

                IDictionary<string, string> metadata = new Dictionary<string, string>
                {
                    { "SHA", SHA }
                };

                await blobClient.SetMetadataAsync(metadata);
                UploadFileToMySQL(file.FileName, email, uniqueName, SQLcontainer);
            }
        }
        public async Task UpdateFileAsync(HttpPostedFileBase file, string filename, HttpContextBase httpContext)
        {
            string fileExtension = System.IO.Path.GetExtension(file.FileName);
            string email = httpContext.Session["email"] as string;
            string password = httpContext.Session["password"] as string;
            if (email == null || password == null)
            {
                return;
            }
            Tuple<string, int> result = SetContainer(fileExtension);
            string Container = result.Item1;
            int SQLcontainer = result.Item2;
            string ConnectionString = ConfigurationManager.AppSettings["AzureConnectionString"];
            BlobServiceClient blobServiceClient = new BlobServiceClient(ConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(Container);
            var blobClient = containerClient.GetBlobClient(filename);
            string SHA = CalculateSHA256(file);
            Task<string> SHAexist = CompareSHA(SHA, containerClient, fileExtension);
            if (await SHAexist != null)
            {
                UpdateSQLInfo(file.FileName, email, SHAexist.Result, SQLcontainer);
                Console.WriteLine("File content exist!!!");
            }
            else
            {
                using (Stream fileStream = file.InputStream)
                {
                    await blobClient.UploadAsync(fileStream, overwrite: true);
                }
                IDictionary<string, string> metadata = new Dictionary<string, string>
                {
                    { "SHA", SHA }
                };
                await blobClient.SetMetadataAsync(metadata);
            }
        }
    }

    
}
