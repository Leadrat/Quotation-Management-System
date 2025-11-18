using System.Threading.Tasks;

namespace CRM.Application.Reports.Services
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(string fileName, byte[] fileBytes);
        Task<byte[]> GetFileAsync(string filePath);
        Task DeleteFileAsync(string filePath);
    }
}

