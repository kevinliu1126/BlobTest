namespace BlobTest.Services.Abstract
{
    public interface ILoginService
    {
        bool SendinfoToSQL(string email, string passwword);
    }
}
