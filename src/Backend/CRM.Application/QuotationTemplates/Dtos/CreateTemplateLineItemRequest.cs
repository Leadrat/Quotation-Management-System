using System.ComponentModel.DataAnnotations;

namespace CRM.Application.QuotationTemplates.Dtos
{
    public class CreateTemplateLineItemRequest
    {
        [Required]
        [StringLength(255, MinimumLength = 1)]
        public string ItemName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit rate must be greater than 0")]
        public decimal UnitRate { get; set; }
    }
}

