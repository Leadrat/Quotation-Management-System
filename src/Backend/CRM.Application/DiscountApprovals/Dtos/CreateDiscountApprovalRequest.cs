using System;
using System.ComponentModel.DataAnnotations;

namespace CRM.Application.DiscountApprovals.Dtos
{
    public class CreateDiscountApprovalRequest
    {
        [Required]
        public Guid QuotationId { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Discount percentage must be between 0 and 100.")]
        public decimal DiscountPercentage { get; set; }

        [Required]
        [MinLength(10, ErrorMessage = "Reason must be at least 10 characters.")]
        [MaxLength(2000, ErrorMessage = "Reason cannot exceed 2000 characters.")]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(5000, ErrorMessage = "Comments cannot exceed 5000 characters.")]
        public string? Comments { get; set; }
    }
}

