namespace CRM.Application.Common.Security
{
    public interface IPasswordHasher
    {
        string Hash(string password, int? workFactor = null);
        bool Verify(string hash, string password);
    }
}
