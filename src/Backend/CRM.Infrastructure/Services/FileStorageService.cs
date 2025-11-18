using System;
using System.IO;
using System.Threading.Tasks;
using CRM.Application.Reports.Services;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _storagePath;
        private readonly ILogger<FileStorageService> _logger;

        public FileStorageService(ILogger<FileStorageService> logger)
        {
            _logger = logger;
            // Store in wwwroot/exports or configured path
            _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "exports");
            
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
        }

        public async Task<string> SaveFileAsync(string fileName, byte[] fileBytes)
        {
            var filePath = Path.Combine(_storagePath, fileName);
            await File.WriteAllBytesAsync(filePath, fileBytes);
            _logger.LogInformation("File saved: {FilePath}, Size: {Size} bytes", filePath, fileBytes.Length);
            return filePath;
        }

        public async Task<byte[]> GetFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            return await File.ReadAllBytesAsync(filePath);
        }

        public Task DeleteFileAsync(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("File deleted: {FilePath}", filePath);
            }

            return Task.CompletedTask;
        }
    }
}

