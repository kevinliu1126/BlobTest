using System.Web;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using System.IO;
using System;

namespace BlobTest.Services.Abstract
{
    public interface IUploadService
    {
        string FileExist(string filename, string email);
        Task UploadFileAsync(HttpPostedFileBase file, HttpContextBase httpContext);
        Tuple<string, int> SetContainer(string fileExtension);
        Task<string> CompareSHA(string SHA, BlobContainerClient containerClient);
    }
}
