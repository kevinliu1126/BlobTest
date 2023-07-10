using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BlobTest.Services.Abstract
{
    public interface ITempService
    {
        string FileExist(string filename, string email);

        string SetContainer(string fileExtension);

        byte[] ReadFileContent(HttpPostedFileBase file);

        string CalculateSHA(HttpPostedFileBase file);

        string CompareSHA(string SHA, string Container);

        void UploadDataToSQL(string fileName, string email, string uniqueName, string Container, string SHA);

        Task UploadFileToAzure(string Container, string uniqueName, HttpPostedFileBase file, string SHA);

        bool HasTwo(string Container, string uniqueName);

        void UpdateDataToSQL(string fileName, string email, string SHA, string uniqueName, string Container);

        Task UpdateFileToAzure(string Container, string uniqueName, HttpPostedFileBase file, string SHA);

        Task UploadFile(HttpPostedFileBase file, HttpContextBase httpContext);

        Task UpdateFile(HttpPostedFileBase file, string uniqueName, HttpContextBase httpContext);
    }
}
