namespace CRM.Application.DocumentTemplates.Services
{
    public interface IPlaceholderIdentificationService
    {
        /// <summary>
        /// Identifies company details in document text and returns placeholder mappings
        /// </summary>
        Task<List<IdentifiedPlaceholder>> IdentifyCompanyDetailsAsync(string documentText, CancellationToken cancellationToken = default);

        /// <summary>
        /// Identifies customer/client company details in document text
        /// </summary>
        Task<List<IdentifiedPlaceholder>> IdentifyCustomerDetailsAsync(string documentText, CancellationToken cancellationToken = default);
    }

    public class IdentifiedPlaceholder
    {
        public string PlaceholderName { get; set; } = string.Empty;
        public string PlaceholderType { get; set; } = string.Empty; // "Company" or "Customer"
        public string OriginalText { get; set; } = string.Empty;
        public int? PositionInDocument { get; set; }
    }
}

