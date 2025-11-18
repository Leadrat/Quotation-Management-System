using System;

namespace CRM.Domain.Events
{
    public class RefundCompleted
    {
        public Guid RefundId { get; set; }
        public Guid PaymentId { get; set; }
        public Guid QuotationId { get; set; }
        public decimal RefundAmount { get; set; }
        public string? PaymentGatewayReference { get; set; }
        public DateTimeOffset CompletedDate { get; set; }
    }
}

