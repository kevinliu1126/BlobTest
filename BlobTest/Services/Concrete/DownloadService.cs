using BlobTest.Services.Abstract;

namespace BlobTest.Services.Concrete
{
    public class DownloadService : IDownloadService
    {
        public string GetDownloadURL(string filename)
        {
            string fileExtension = filename.Split('.')[1];
            string url;
            switch (fileExtension)
            {
                case "png":
                case "jpg":
                case "jpeg":
                    url = "https://interplastblobtest.blob.core.windows.net/blobcontainer1";
                    break;
                case "doc":
                case "docx":
                case "pdf":
                    url = "https://interplastblobtest.blob.core.windows.net/blobcontainer2";
                    break;
                case "xls":
                case "xlsx":
                    url = "https://interplastblobtest.blob.core.windows.net/blobcontainer3";
                    break;
                case "json":
                    url = "https://interplastblobtest.blob.core.windows.net/blobcontainer4";
                    break;
                default:
                    url = "https://interplastblobtest.blob.core.windows.net/blobcontainer5";
                    break;
            }
            url = url + "/" + filename;
            return url;
        }

        public string GetContainername(string filename)
        {
            string fileExtension = filename.Split('.')[1];
            string container;
            switch (fileExtension)
            {
                case "png":
                case "jpg":
                case "jpeg":
                    container = "blobcontainer1";
                    break;
                case "doc":
                case "docx":
                case "pdf":
                    container = "blobcontainer2";
                    break;
                case "xls":
                case "xlsx":
                    container = "blobcontainer3";
                    break;
                case "json":
                    container = "blobcontainer4";
                    break;
                default:
                    container = "blobcontainer5";
                    break;
            }
            return container;
        }
    }
}
