using Azure;
using Azure.Storage.Blobs;
using BlobTest.Services.Abstract;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls;
using WebGrease.Activities;

namespace BlobTest.Services.Concrete
{
    public class TempService : ITempService
    {
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

        public string SetContainer(string fileExtension)
        {
            string Container;
            switch (fileExtension)
            {
                case ".png":
                case ".jpg":
                case ".jpeg":
                    Container = "blobcontainer1";
                    break;
                case ".doc":
                case ".docx":
                case ".pdf":
                    Container = "blobcontainer2";
                    break;
                case ".xls":
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

        public byte[] ReadFileContent(HttpPostedFileBase file)
        {
            string temp = string.Empty;

            using (StreamReader reader = new StreamReader(file.InputStream))
            {
                temp = reader.ReadToEnd();
            }
            return Encoding.UTF8.GetBytes(temp);
        }

        public string CalculateSHA(HttpPostedFileBase file)
        {
            byte[] fileBytes;
            using (var memoryStream = new MemoryStream())
            {
                file.InputStream.CopyTo(memoryStream);
                fileBytes = memoryStream.ToArray();
                using (var sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(fileBytes);
                    string shaValue = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                    return shaValue;
                }
            }
        }

        public string CompareSHA(string SHA, string Container)
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
            {
                conn.Open();
                string sql = $"SELECT `savename`FROM `file` WHERE `SHA` = '{SHA}' AND `Container` = '{Container}'";
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
            else
            {
                Console.WriteLine("Connection Error");
                return null;
            }
        }

        public void UploadDataToSQL(string fileName, string email, string uniqueName, string Container, string SHA)
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
            string sql = $"INSERT INTO `file`(`filename`, `email`, `savename`, `Container`, `SHA`) VALUES ('{fileName}', '{email}', '{uniqueName}', '{Container}', '{SHA}')";
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

        public async Task UploadFileToAzure(string Container, string uniqueName, HttpPostedFileBase file, string SHA)
        {
            string ConnectionString = ConfigurationManager.AppSettings["AzureConnectionString"];
            BlobServiceClient blobServiceClient = new BlobServiceClient(ConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(Container);
            file.InputStream.Position = 0;
            using (Stream fileStream = file.InputStream)
            {
                await containerClient.UploadBlobAsync(uniqueName, fileStream);
            }
            var blobClient = containerClient.GetBlobClient(uniqueName);
            IDictionary<string, string> metadata = new Dictionary<string, string>
                {
                    { "SHA", SHA }
                };

            await blobClient.SetMetadataAsync(metadata);
        }

        public void UpdateDataToSQL(string fileName, string email, string uniqueName, string SHA, string Container)
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
            string sql = $"UPDATE `file` SET `savename` = '{uniqueName}', `SHA` = '{SHA}' WHERE `fileName` = '{fileName}' AND `email` = '{email}' AND `Container` = '{Container}'";
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

        public async Task UpdateFileToAzure(string Container, string uniqueName, HttpPostedFileBase file, string SHA)
        {
            string ConnectionString = ConfigurationManager.AppSettings["AzureConnectionString"];
            BlobServiceClient blobServiceClient = new BlobServiceClient(ConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(Container);
            var blobClient = containerClient.GetBlobClient(uniqueName);
            file.InputStream.Position = 0;
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

        public async Task DeleteFileBlob(string Container, string filename)
        {
            string connectionString = ConfigurationManager.AppSettings["AzureConnectionString"];
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(Container);
            try
            {
                await containerClient.DeleteBlobAsync(filename);
                Console.WriteLine("Blob deleted successfully");
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"Error deleting blob: {ex.Message}");
            }
        }

        public async Task UploadFile(HttpPostedFileBase file, HttpContextBase httpContext)
        {
            string fileExtension = Path.GetExtension(file.FileName);
            string email = httpContext.Session["email"] as string;
            string password = httpContext.Session["password"] as string;
            if (email == null || password == null)
            {
                return;
            }
            string Container = SetContainer(fileExtension);
            string SHA = CalculateSHA(file);
            string uniqueName = CompareSHA(SHA, Container);
            if (uniqueName != null)
            {
                UploadDataToSQL(file.FileName, email, uniqueName, Container, SHA);
            }
            else
            {
                uniqueName = Guid.NewGuid().ToString() + fileExtension;
                UploadDataToSQL(file.FileName, email, uniqueName, Container, SHA);
                await UploadFileToAzure(Container, uniqueName, file, SHA);
            }
        }

        public async Task UpdateFile(HttpPostedFileBase file, string uniqueName, HttpContextBase httpContext)
        {
            string fileExtension = Path.GetExtension(file.FileName);
            string email = httpContext.Session["email"] as string;
            string password = httpContext.Session["password"] as string;
            if (email == null || password == null)
            {
                return;
            }
            string Container = SetContainer(fileExtension);
            string SHA = CalculateSHA(file);
            string uniqueNameSHA = CompareSHA(SHA, Container);
            if (HasTwo(Container, uniqueName))
            {
                if (uniqueNameSHA != null)
                {
                    UpdateDataToSQL(file.FileName, email, uniqueNameSHA, SHA, Container);
                }
                else
                {
                    uniqueName = Guid.NewGuid().ToString() + fileExtension;
                    UpdateDataToSQL(file.FileName, email, uniqueName, SHA, Container);
                    await UploadFileToAzure(Container, uniqueName, file, SHA);
                }
            }
            else
            {
                if (uniqueNameSHA != null)
                {
                    UpdateDataToSQL(file.FileName, email, uniqueNameSHA, SHA, Container);
                    await DeleteFileBlob(Container, uniqueName);
                }
                else
                {
                    UpdateDataToSQL(file.FileName, email, uniqueName, SHA, Container);
                    await UpdateFileToAzure(Container, uniqueName, file, SHA);
                }
            }
            
        }
    }
}