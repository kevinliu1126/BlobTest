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
using System.Drawing;
using Xceed.Words.NET;
using Xceed.Document.NET;
using System.Web.Optimization;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace BlobTest.Services.Concrete
{
    public class UploadService : IUploadService
    {

        public Tuple<string, int, byte[]> SetContainer(HttpPostedFileBase file, string fileExtension)
        {
            string Container;
            int SQLcontainer;
            byte[] fileContent; 
            switch (fileExtension)
            {
                case ".png":
                case ".jpg":
                case ".jpeg":
                    Container = "blobcontainer1";
                    SQLcontainer = 1;
                    fileContent = Readimage(file);
                    break;
                case ".doc":
                case ".docx":
                    Container = "blobcontainer2";
                    SQLcontainer = 2;
                    fileContent = ReadDoc(file);
                    break;
                case ".pdf":
                    Container = "blobcontainer2";
                    SQLcontainer = 2;
                    fileContent = ReadPDF(file);
                    break;
                case ".xls":
                case ".xlsx":
                    Container = "blobcontainer3";
                    SQLcontainer = 3;
                    fileContent = Readimage(file);
                    break;
                case ".json":
                    Container = "blobcontainer4";
                    SQLcontainer = 4;
                    fileContent = Readimage(file);
                    break;
                default:
                    Container = "blobcontainer5";
                    SQLcontainer = 5;
                    string temp = string.Empty;

                    using (StreamReader reader = new StreamReader(file.InputStream))
                    {
                        temp = reader.ReadToEnd();
                    }
                    fileContent = Encoding.UTF8.GetBytes(temp);
                    break;
            }
            return Tuple.Create(Container, SQLcontainer, fileContent);
        }

        public byte[] Readimage(HttpPostedFileBase file)
        {
            // 讀取圖片
            using (System.Drawing.Image image = System.Drawing.Image.FromStream(file.InputStream))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    return ms.ToArray();
                }
            }
        }

        public byte[] ReadDoc(HttpPostedFileBase file)
        {
            DocX document = DocX.Load(file.InputStream);
            string text = document.Text;
            return Encoding.UTF8.GetBytes(text);
        }

        public byte[] ReadPDF(HttpPostedFileBase file)
        {
            string text = "";
            using (PdfReader reader = new PdfReader(file.InputStream))
            {
                
                for (int page = 1; page <= reader.NumberOfPages; page++)
                {
                    string temp = PdfTextExtractor.GetTextFromPage(reader, page);
                    text += temp;
                }
            }
            return Encoding.UTF8.GetBytes(text);
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

        public static string ReadFileContent(HttpPostedFileBase file)
        {
            string fileContent = string.Empty;

            using (StreamReader reader = new StreamReader(file.InputStream))
            {
                fileContent = reader.ReadToEnd();
            }

            return fileContent;
        }

        public static string CalculateSHA256(byte[] input)
        {
            using (SHA256Managed sha256 = new SHA256Managed())
            {
                byte[] hashBytes = sha256.ComputeHash(input);

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
            Tuple<string, int, byte[]> result = SetContainer(file, fileExtension);
            string Container = result.Item1;
            int SQLcontainer = result.Item2;
            byte[] fileContent = result.Item3;
            string ConnectionString = ConfigurationManager.AppSettings["AzureConnectionString"];
            var uniqueName = Guid.NewGuid().ToString() + fileExtension;
            BlobServiceClient blobServiceClient = new BlobServiceClient(ConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(Container);
            string SHA = CalculateSHA256(fileContent);
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
            Tuple<string, int, byte[]> result = SetContainer(file, fileExtension);
            string Container = result.Item1;
            int SQLcontainer = result.Item2;
            byte[] fileContent = result.Item3;
            string ConnectionString = ConfigurationManager.AppSettings["AzureConnectionString"];
            BlobServiceClient blobServiceClient = new BlobServiceClient(ConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(Container);
            var blobClient = containerClient.GetBlobClient(filename);
            string SHA = CalculateSHA256(fileContent);
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
