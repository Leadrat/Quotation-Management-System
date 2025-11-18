using System;

namespace CRM.Domain.Events
{
    public class RefundFailed
    {
        public Guid RefundId { get; set; }
        public Guid PaymentId { get; set; }
        public Guid QuotationId { get; set; }
        public string FailureReason { get; set; } = string.Empty;
        public DateTimeOffset FailedDate { get; set; }
    }
}

