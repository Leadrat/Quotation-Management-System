using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRM.Application.QuotationTemplates.Dtos
{
    public class CreateQuotationTemplateRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Description { get; set; }

        [Required]
        public string Visibility { get; set; } = string.Empty; // "Public", "Team", "Private"

        [Range(0, 100)]
        public decimal? DiscountDefault { get; set; }

        [StringLength(2000)]
        public string? Notes { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one line item is required")]
        public List<CreateTemplateLineItemRequest> LineItems { get; set; } = new();
    }
}

