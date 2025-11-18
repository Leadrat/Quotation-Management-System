using System.ComponentModel.DataAnnotations;
using CRM.Domain.Enums;

namespace CRM.Application.Refunds.Dtos
{
    public class CreateAdjustmentRequest
    {
        [Required]
        public Guid QuotationId { get; set; }

        [Required]
        public AdjustmentType AdjustmentType { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Original amount must be >= 0")]
        public decimal OriginalAmount { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Adjusted amount must be greater than 0")]
        public decimal AdjustedAmount { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
}

