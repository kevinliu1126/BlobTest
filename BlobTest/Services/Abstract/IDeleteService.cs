using System.Threading.Tasks;

namespace BlobTest.Services.Abstract
{
    public interface IDeleteService
    {
        Task DeleteFileBlob(string filename);

        void DeleteFileSQL(string filename);

        string GetContainername(string filename);
    }
}
