using System;
using System.ComponentModel.DataAnnotations;
using CRM.Domain.Enums;

namespace CRM.Application.Refunds.Dtos
{
    public class CreateRefundRequest
    {
        [Required]
        public Guid PaymentId { get; set; }

        public Guid? QuotationId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Refund amount must be greater than 0")]
        public decimal? RefundAmount { get; set; } // If null, full refund

        [Required]
        [MaxLength(500)]
        public string RefundReason { get; set; } = string.Empty;

        [Required]
        public RefundReasonCode RefundReasonCode { get; set; }

        [MaxLength(1000)]
        public string? Comments { get; set; }
    }
}

