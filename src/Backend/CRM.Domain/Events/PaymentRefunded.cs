using System;

namespace CRM.Domain.Events
{
    public class PaymentRefunded
    {
        public Guid PaymentId { get; set; }
        public Guid QuotationId { get; set; }
        public string PaymentGateway { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public decimal TotalAmountPaid { get; set; }
        public bool IsPartialRefund { get; set; }
        public string RefundReason { get; set; } = string.Empty;
        public DateTimeOffset RefundDate { get; set; }
        public Guid? RefundedByUserId { get; set; }
    }
}

