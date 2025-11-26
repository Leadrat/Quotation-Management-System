using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Admin.FileStorage;

/// <summary>
/// Local filesystem implementation of IFileStorageService
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly ILogger<LocalFileStorageService>? _logger;

    public LocalFileStorageService(IConfiguration configuration, ILogger<LocalFileStorageService>? logger = null)
    {
        _logger = logger;
        var configuredPath = configuration["FileStorage:BasePath"];
        _basePath = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(Environment.CurrentDirectory, "wwwroot", "uploads")
            : configuredPath;
        
        _logger?.LogInformation("LocalFileStorageService initialized. BasePath: {BasePath}, CurrentDirectory: {CurrentDirectory}", 
            _basePath, Environment.CurrentDirectory);
        
        // Ensure base directory exists
        try
        {
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
                _logger?.LogInformation("Created upload directory: {BasePath}", _basePath);
            }
            else
            {
                _logger?.LogInformation("Upload directory already exists: {BasePath}", _basePath);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to create upload directory: {BasePath}, Error: {Message}", _basePath, ex.Message);
            throw new InvalidOperationException($"Failed to create upload directory at '{_basePath}': {ex.Message}", ex);
        }
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string folder)
    {
        try
        {
            _logger?.LogInformation("Starting file upload. FileName: {FileName}, ContentType: {ContentType}, Folder: {Folder}, StreamLength: {StreamLength}, CanSeek: {CanSeek}", 
                fileName, contentType, folder, fileStream?.Length ?? 0, fileStream?.CanSeek ?? false);

            // Validate file type based on folder
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            _logger?.LogInformation("File extension: {Extension}", extension);
            
            if (folder == "templates")
            {
                // Template files: PDF, Word, Excel, HTML
                var allowedTemplateExtensions = new[] { ".pdf", ".doc", ".docx", ".html", ".htm", ".xls", ".xlsx" };
                if (!allowedTemplateExtensions.Contains(extension))
                {
                    _logger?.LogWarning("File type {Extension} not allowed for templates", extension);
                    throw new ArgumentException($"File type {extension} is not allowed for templates. Allowed types: {string.Join(", ", allowedTemplateExtensions)}");
                }
            }
            else if (folder == "company-logos")
            {
                // Company logos: PNG, JPG, JPEG, SVG, WEBP
                var allowedLogoExtensions = new[] { ".png", ".jpg", ".jpeg", ".svg", ".webp" };
                if (!allowedLogoExtensions.Contains(extension))
                {
                    _logger?.LogWarning("File type {Extension} not allowed for company logos", extension);
                    throw new ArgumentException($"File type {extension} is not allowed for company logos. Allowed types: {string.Join(", ", allowedLogoExtensions)}");
                }
            }
            else
            {
                // Default: images only
                var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".svg" };
                if (!allowedExtensions.Contains(extension))
                {
                    _logger?.LogWarning("File type {Extension} not allowed", extension);
                    throw new ArgumentException($"File type {extension} is not allowed. Allowed types: {string.Join(", ", allowedExtensions)}");
                }
            }

            // Ensure stream position is at the beginning
            if (fileStream.CanSeek && fileStream.Position > 0)
            {
                fileStream.Position = 0;
                _logger?.LogInformation("Reset stream position to 0");
            }

            // Create folder directory if it doesn't exist
            var folderPath = Path.Combine(_basePath, folder);
            _logger?.LogInformation("Target folder path: {FolderPath}", folderPath);
            
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    _logger?.LogInformation("Created folder directory: {FolderPath}", folderPath);
                }
                else
                {
                    _logger?.LogInformation("Folder directory already exists: {FolderPath}", folderPath);
                }
            }
            catch (Exception dirEx)
            {
                _logger?.LogError(dirEx, "Failed to create folder directory: {FolderPath}, Error: {Message}", folderPath, dirEx.Message);
                throw new InvalidOperationException($"Failed to create folder directory at '{folderPath}': {dirEx.Message}", dirEx);
            }

            // Generate unique filename: {timestamp}_{guid}{extension}
            var uniqueFileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(folderPath, uniqueFileName);
            _logger?.LogInformation("Generated file path: {FilePath}", filePath);

            // Save file
            try
            {
                using (var fileStreamOut = new FileStream(filePath, FileMode.Create))
                {
                    await fileStream.CopyToAsync(fileStreamOut);
                    _logger?.LogInformation("File copied successfully. FilePath: {FilePath}, Size: {Size} bytes", filePath, fileStreamOut.Length);
                }
            }
            catch (UnauthorizedAccessException unauthEx)
            {
                _logger?.LogError(unauthEx, "Permission denied writing to file: {FilePath}", filePath);
                throw new UnauthorizedAccessException($"Permission denied: Cannot write to file at '{filePath}'. Please check file permissions.", unauthEx);
            }
            catch (DirectoryNotFoundException dirEx)
            {
                _logger?.LogError(dirEx, "Directory not found for file: {FilePath}", filePath);
                throw new DirectoryNotFoundException($"Directory not found for file path '{filePath}': {dirEx.Message}", dirEx);
            }
            catch (IOException ioEx)
            {
                _logger?.LogError(ioEx, "I/O error writing file: {FilePath}, Error: {Message}", filePath, ioEx.Message);
                throw new IOException($"I/O error writing file to '{filePath}': {ioEx.Message}", ioEx);
            }

            // Return relative path from wwwroot or absolute URL
            var relativePath = Path.Combine("uploads", folder, uniqueFileName).Replace('\\', '/');
            var result = $"/{relativePath}";
            _logger?.LogInformation("File upload completed successfully. FileUrl: {FileUrl}", result);
            return result;
        }
        catch (ArgumentException)
        {
            // Re-throw ArgumentException as-is (file type validation)
            throw;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error during file upload. FileName: {FileName}, Folder: {Folder}, Error: {Message}, Type: {Type}", 
                fileName, folder, ex.Message, ex.GetType().Name);
            throw;
        }
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

