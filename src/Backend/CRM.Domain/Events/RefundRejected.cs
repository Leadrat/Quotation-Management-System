using System;

namespace CRM.Domain.Events
{
    public class RefundRejected
    {
        public Guid RefundId { get; set; }
        public Guid PaymentId { get; set; }
        public Guid QuotationId { get; set; }
        public Guid RequestedByUserId { get; set; }
        public string RejectionReason { get; set; } = string.Empty;
        public Guid RejectedByUserId { get; set; }
        public DateTimeOffset RejectionDate { get; set; }
    }
}
