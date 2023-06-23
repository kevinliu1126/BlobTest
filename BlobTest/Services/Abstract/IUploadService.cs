using System.Web;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using System.IO;

namespace BlobTest.Services.Abstract
{
    public interface IUploadService
    {
        Task UploadFileAsync(HttpPostedFileBase file, HttpContextBase httpContext);
        string SetContainer(string fileExtension);
        Task<bool> CompareSHA(string SHA, BlobContainerClient containerClient);
    }
}
