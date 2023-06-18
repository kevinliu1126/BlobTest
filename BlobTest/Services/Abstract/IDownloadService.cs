namespace BlobTest.Services.Abstract
{
    public interface IDownloadService
    {
        string Downloadfile(string filename);

        string GetContainername(string filename);
    }
}
