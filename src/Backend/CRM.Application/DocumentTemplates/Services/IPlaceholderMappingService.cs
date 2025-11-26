namespace CRM.Application.DocumentTemplates.Services
{
    public interface IPlaceholderMappingService
    {
        /// <summary>
        /// Maps placeholder names to actual data values for quotation generation
        /// </summary>
        Task<Dictionary<string, string>> MapPlaceholdersToDataAsync(
            Guid templateId,
            Guid quotationId,
            CancellationToken cancellationToken = default);
    }
}

