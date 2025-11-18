namespace CRM.Infrastructure.Admin.FileStorage;

/// <summary>
/// Service for storing and retrieving files (logos, documents, etc.)
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file and returns the URL/path to access it
    /// </summary>
    /// <param name="fileStream">File stream to upload</param>
    /// <param name="fileName">Original file name</param>
    /// <param name="contentType">MIME type of the file</param>
    /// <param name="folder">Folder/subdirectory to store the file in</param>
    /// <returns>URL or relative path to the stored file</returns>
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string folder);

    /// <summary>
    /// Deletes a file
    /// </summary>
    /// <param name="filePath">Path or URL of the file to delete</param>
    Task DeleteFileAsync(string filePath);

    /// <summary>
    /// Checks if a file exists
    /// </summary>
    /// <param name="filePath">Path or URL of the file</param>
    /// <returns>True if file exists</returns>
    Task<bool> FileExistsAsync(string filePath);
}

