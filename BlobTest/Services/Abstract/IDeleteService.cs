using System.Threading.Tasks;

namespace BlobTest.Services.Abstract
{
    public interface IDeleteService
    {
        string GetContainername(string filename);

        bool HasTwo(string Container, string uniqueName);

        void DeleteFileSQL(string container, string fileName);

        Task DeleteFileBlob(string container, string uniqueName);

        Task DeleteFile(string fileName, string uniqueName);
    }
}
