namespace CRM.Application.DocumentTemplates.Services
{
    public interface ITemplateConversionService
    {
        /// <summary>
        /// Converts a document to a Word template by replacing identified text with placeholders
        /// </summary>
        Task<string> ConvertToTemplateAsync(
            string sourceFilePath,
            string outputFilePath,
            List<IdentifiedPlaceholder> placeholders,
            string mimeType,
            CancellationToken cancellationToken = default);
    }
}

