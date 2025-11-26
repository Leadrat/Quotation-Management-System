namespace CRM.Infrastructure.Services.DocumentProcessing
{
    public interface IWordDocumentService
    {
        /// <summary>
        /// Opens a Word document for reading/writing
        /// </summary>
        Task<WordDocument> OpenDocumentAsync(string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Extracts text content from a Word document
        /// </summary>
        Task<string> ExtractTextAsync(string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Replaces text in a Word document while preserving formatting
        /// </summary>
        Task ReplaceTextWithPlaceholderAsync(string filePath, string originalText, string placeholder, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves a Word document to a new location
        /// </summary>
        Task SaveDocumentAsync(WordDocument document, string outputPath, CancellationToken cancellationToken = default);
    }

    public class WordDocument
    {
        public string FilePath { get; set; } = string.Empty;
        public object? DocumentObject { get; set; } // DocumentFormat.OpenXml.WordprocessingDocument
    }
}

