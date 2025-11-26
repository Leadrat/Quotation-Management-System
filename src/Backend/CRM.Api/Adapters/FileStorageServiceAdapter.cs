using CRM.Application.Common.Services;

namespace CRM.Api.Adapters
{
    /// <summary>
    /// Adapter to bridge Infrastructure IFileStorageService to Application IFileStorageService
    /// </summary>
    public class FileStorageServiceAdapter : IFileStorageService
    {
        private readonly CRM.Infrastructure.Admin.FileStorage.IFileStorageService _infrastructureService;

        public FileStorageServiceAdapter(CRM.Infrastructure.Admin.FileStorage.IFileStorageService infrastructureService)
        {
            _infrastructureService = infrastructureService;
        }

        public Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string folder)
        {
            return _infrastructureService.UploadFileAsync(fileStream, fileName, contentType, folder);
        }

        public Task DeleteFileAsync(string filePath)
        {
            return _infrastructureService.DeleteFileAsync(filePath);
        }

        public Task<bool> FileExistsAsync(string filePath)
        {
            return _infrastructureService.FileExistsAsync(filePath);
        }
    }
}

