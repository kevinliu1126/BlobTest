namespace BlobTest.Services.Abstract
{
    public interface IDownloadService
    {
        string GetDownloadURL(string filename);

        string GetContainername(string filename);
    }
}
