using System.ComponentModel;
using System.Web;

namespace BlobTest.Models
{    public class UploadFileModel
    {
        [DisplayName("Upload File")]
        public string FileDetails { get; set; }

        public HttpPostedFileBase File { get; set; }
    }
}
