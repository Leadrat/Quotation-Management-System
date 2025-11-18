using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRM.Application.QuotationTemplates.Dtos
{
    public class UpdateQuotationTemplateRequest
    {
        [StringLength(100, MinimumLength = 3)]
        public string? Name { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }

        public string? Visibility { get; set; } // "Public", "Team", "Private"

        [Range(0, 100)]
        public decimal? DiscountDefault { get; set; }

        [StringLength(2000)]
        public string? Notes { get; set; }

        public List<UpdateTemplateLineItemRequest>? LineItems { get; set; }
    }
}

