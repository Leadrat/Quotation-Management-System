namespace CRM.Application.DocumentTemplates.Services
{
    public interface IDocumentProcessingService
    {
        /// <summary>
        /// Processes a document (PDF or Word) and extracts text content
        /// </summary>
        Task<string> ProcessDocumentAsync(string filePath, string mimeType, CancellationToken cancellationToken = default);
        Task<List<CRM.Domain.Entities.TemplatePlaceholder>> AnalyzeDocumentAsync(string filePath, Guid templateId);
    }
}

