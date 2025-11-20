using System.ComponentModel.DataAnnotations;

namespace CRM.Application.QuotationTemplates.Dtos
{
    public class UploadQuotationTemplateRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Description { get; set; }

        [Required]
        public string Visibility { get; set; } = string.Empty; // "Public", "Team", "Private"

        [Required]
        public string TemplateType { get; set; } = string.Empty; // "Quotation" or "ProFormaInvoice"
    }
}

