using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace CRM.Infrastructure.Admin.FileStorage;

/// <summary>
/// Local filesystem implementation of IFileStorageService
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly IWebHostEnvironment _environment;

    public LocalFileStorageService(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _environment = environment;
        _basePath = configuration["FileStorage:BasePath"] 
            ?? Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads");
        
        // Ensure base directory exists
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string folder)
    {
        // Validate file type
        var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".svg" };
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(extension))
        {
            throw new ArgumentException($"File type {extension} is not allowed. Allowed types: {string.Join(", ", allowedExtensions)}");
        }

        // Create folder directory if it doesn't exist
        var folderPath = Path.Combine(_basePath, folder);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Generate unique filename: {timestamp}_{guid}{extension}
        var uniqueFileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(folderPath, uniqueFileName);

        // Save file
        using (var fileStreamOut = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileStreamOut);
        }

        // Return relative path from wwwroot or absolute URL
        var relativePath = Path.Combine("uploads", folder, uniqueFileName).Replace('\\', '/');
        return $"/{relativePath}";
    }

    public Task DeleteFileAsync(string filePath)
    {
        // Remove leading slash if present
        var cleanPath = filePath.TrimStart('/');
        var fullPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, cleanPath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public Task<bool> FileExistsAsync(string filePath)
    {
        var cleanPath = filePath.TrimStart('/');
        var fullPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, cleanPath);
        return Task.FromResult(File.Exists(fullPath));
    }
}

