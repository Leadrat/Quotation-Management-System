using System.IO;
using Microsoft.Extensions.Configuration;

namespace CRM.Infrastructure.Admin.FileStorage;

/// <summary>
/// Local filesystem implementation of IFileStorageService
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public LocalFileStorageService(IConfiguration configuration)
    {
        var configuredPath = configuration["FileStorage:BasePath"];
        _basePath = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(Environment.CurrentDirectory, "wwwroot", "uploads")
            : configuredPath;
        
        // Ensure base directory exists
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string folder)
    {
        // Validate file type based on folder
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        
        if (folder == "templates")
        {
            // Template files: PDF, Word, Excel, HTML
            var allowedTemplateExtensions = new[] { ".pdf", ".doc", ".docx", ".html", ".htm", ".xls", ".xlsx" };
            if (!allowedTemplateExtensions.Contains(extension))
            {
                throw new ArgumentException($"File type {extension} is not allowed for templates. Allowed types: {string.Join(", ", allowedTemplateExtensions)}");
            }
        }
        else if (folder == "company-logos")
        {
            // Company logos: PNG, JPG, JPEG, SVG, WEBP
            var allowedLogoExtensions = new[] { ".png", ".jpg", ".jpeg", ".svg", ".webp" };
            if (!allowedLogoExtensions.Contains(extension))
            {
                throw new ArgumentException($"File type {extension} is not allowed for company logos. Allowed types: {string.Join(", ", allowedLogoExtensions)}");
            }
        }
        else
        {
            // Default: images only
            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".svg" };
            if (!allowedExtensions.Contains(extension))
            {
                throw new ArgumentException($"File type {extension} is not allowed. Allowed types: {string.Join(", ", allowedExtensions)}");
            }
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
        // If path starts with "uploads/", remove it since _basePath already includes it
        if (cleanPath.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
        {
            cleanPath = cleanPath.Substring("uploads/".Length);
        }
        var fullPath = Path.Combine(_basePath, cleanPath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public Task<bool> FileExistsAsync(string filePath)
    {
        var cleanPath = filePath.TrimStart('/');
        // If path starts with "uploads/", remove it since _basePath already includes it
        if (cleanPath.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
        {
            cleanPath = cleanPath.Substring("uploads/".Length);
        }
        var fullPath = Path.Combine(_basePath, cleanPath);
        return Task.FromResult(File.Exists(fullPath));
    }
}

