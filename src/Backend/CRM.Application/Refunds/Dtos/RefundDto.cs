using System;
using CRM.Domain.Enums;

namespace CRM.Application.Refunds.Dtos
{
    public class RefundDto
    {
        public Guid RefundId { get; set; }
        public Guid PaymentId { get; set; }
        public Guid QuotationId { get; set; }
        public decimal RefundAmount { get; set; }
        public string RefundReason { get; set; } = string.Empty;
        public RefundReasonCode RefundReasonCode { get; set; }
        public string RequestedByUserName { get; set; } = string.Empty;
        public string? ApprovedByUserName { get; set; }
        public RefundStatus RefundStatus { get; set; }
        public string? ApprovalLevel { get; set; }
        public string? Comments { get; set; }
        public string? FailureReason { get; set; }
        public DateTimeOffset RequestDate { get; set; }
        public DateTimeOffset? ApprovalDate { get; set; }
        public DateTimeOffset? CompletedDate { get; set; }
        public string? PaymentGatewayReference { get; set; }
        public string? ReversedReason { get; set; }
        public DateTimeOffset? ReversedDate { get; set; }
    }
}

