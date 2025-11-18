using System;

namespace CRM.Domain.Events
{
    public class RefundReversed
    {
        public Guid RefundId { get; set; }
        public Guid PaymentId { get; set; }
        public Guid QuotationId { get; set; }
        public string ReversedReason { get; set; } = string.Empty;
        public Guid ReversedByUserId { get; set; }
        public DateTimeOffset ReversedDate { get; set; }
    }
}

