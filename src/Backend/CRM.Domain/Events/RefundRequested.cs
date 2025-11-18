using System;

namespace CRM.Domain.Events
{
    public class RefundRequested
    {
        public Guid RefundId { get; set; }
        public Guid PaymentId { get; set; }
        public Guid QuotationId { get; set; }
        public decimal RefundAmount { get; set; }
        public string RefundReason { get; set; } = string.Empty;
        public Guid RequestedByUserId { get; set; }
        public string? ApprovalLevel { get; set; }
        public DateTimeOffset RequestDate { get; set; }
    }
}

