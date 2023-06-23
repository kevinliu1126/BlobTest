using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MySql.Data.MySqlClient;
using BlobTest.Models;
using BlobTest.Services.Abstract;
using System.Data;
using System.Web.Mvc;
using System.Configuration;
using System;
using System.Web;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;

namespace BlobTest.Services.Concrete
{
    public class UploadService : IUploadService
    {

        public string SetContainer(string fileExtension)
        {
            string Container;
            switch (fileExtension)
            {
                case ".png":
                    Container = "blobcontainer1";
                    break;
                case ".jpg":
                case ".jpeg":
                    Container = "blobcontainer1";
                    break;
                case ".doc":
                    Container = "blobcontainer2";
                    break;
                case ".docx":
                    Container = "blobcontainer2";
                    break;
                case ".pdf":
                    Container = "blobcontainer2";
                    break;
                case ".xls":
                    Container = "blobcontainer3";
                    break;
                case ".xlsx":
                    Container = "blobcontainer3";
                    break;
                case ".json":
                    Container = "blobcontainer4";
                    break;
                default:
                    Container = "blobcontainer5";
                    break;
            }
            return Container;
        }

        [HttpPost]
        public void UploadFileToMySQL(string fileName, string email, string uniqueName)
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
            string sql = $"INSERT INTO `file`(`filename`, `email`, `savename`) VALUES ('{fileName}', '{email}', '{uniqueName}')";
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

        public static string ComputeSHA2Hash(HttpPostedFileBase file)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(file.InputStream);
                string hashString = BitConverter.ToString(hashBytes).Replace("-", string.Empty);
                return hashString;
            }
        }

        public async Task<bool> CompareSHA(string SHA, BlobContainerClient containerClient)
        {
            foreach (BlobItem blobItem in containerClient.GetBlobs())
            {
                BlobClient blobClient = containerClient.GetBlobClient(blobItem.Name);
                BlobProperties properties = await blobClient.GetPropertiesAsync();
                IDictionary<string, string> metadata = properties.Metadata;

                Console.WriteLine($"Blob: {blobItem.Name}");

                // 獲取 Metadata 鍵值對
                foreach (var item in metadata)
                {
                    string key = item.Key;
                    string value = item.Value;
                    if(key == "SHA")
                    {
                        if(value == SHA)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public async Task UploadFileAsync(HttpPostedFileBase file, HttpContextBase httpContext)
        {
            string fileExtension = Path.GetExtension(file.FileName);
            string email = httpContext.Session["email"] as string;
            string password = httpContext.Session["password"] as string;
            if (email == null || password == null)
            {
                return;
            }
            string Container = SetContainer(fileExtension);
            string ConnectionString = ConfigurationManager.AppSettings["AzureConnectionString"];
            var uniqueName = Guid.NewGuid().ToString() + fileExtension;
            BlobServiceClient blobServiceClient = new BlobServiceClient(ConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(Container);
            string SHA = ComputeSHA2Hash(file);
            Task<bool> SHAexist = CompareSHA(SHA, containerClient);
            if(await SHAexist)
            {
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
                UploadFileToMySQL(file.FileName, email, uniqueName);
            }
        }
    }
}
