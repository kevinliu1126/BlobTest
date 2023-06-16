using System.Web;
using System.Threading.Tasks;

namespace BlobTest.Services.Abstract
{
    public interface IUploadService
    {
        Task UploadFileAsync(HttpPostedFileBase file, HttpContextBase httpContext);
    }
}
