using BlobTest.Models;
using System.Collections.Generic;
using System.Web;

namespace BlobTest.Services.Abstract
{
    public interface IGetFileService
    {
        List<GetFileModel> GetFiles(int permission, HttpContextBase httpContext);
    }
}
