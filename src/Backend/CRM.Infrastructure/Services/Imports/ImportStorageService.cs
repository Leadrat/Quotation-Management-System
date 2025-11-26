using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CRM.Infrastructure.Services.Imports;

public interface IImportStorageService
{
    Task<string> SaveSourceAsync(IFormFile file);
}

public class ImportStorageService : IImportStorageService
{
    private readonly FileStorageService _fileStorage;

    public ImportStorageService(FileStorageService fileStorage)
    {
        _fileStorage = fileStorage;
    }

    public Task<string> SaveSourceAsync(IFormFile file)
    {
        // Store under uploads/imports
        return _fileStorage.SaveFileAsync(file, "imports");
    }
}
