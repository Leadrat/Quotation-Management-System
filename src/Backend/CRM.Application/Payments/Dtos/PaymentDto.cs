using System;
using CRM.Domain.Enums;

namespace CRM.Application.Payments.Dtos
{
    public class PaymentDto
    {
        public Guid PaymentId { get; set; }
        public Guid QuotationId { get; set; }
        public string PaymentGateway { get; set; } = string.Empty;
        public string PaymentReference { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
        public string Currency { get; set; } = string.Empty;
        public PaymentStatus PaymentStatus { get; set; }
        public string StatusLabel => PaymentStatus.ToString();
        public DateTimeOffset? PaymentDate { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public string? FailureReason { get; set; }
        public bool IsRefundable { get; set; }
        public decimal? RefundAmount { get; set; }
        public string? RefundReason { get; set; }
        public DateTimeOffset? RefundDate { get; set; }
        public bool CanBeRefunded { get; set; }
        public bool CanBeCancelled { get; set; }
        public string? PaymentUrl { get; set; }
        public string? ClientSecret { get; set; } // For Stripe payment intents
    }
}

