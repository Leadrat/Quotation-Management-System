using System;

namespace CRM.Domain.Events
{
    public class PaymentSuccess
    {
        public Guid PaymentId { get; set; }
        public Guid QuotationId { get; set; }
        public string PaymentGateway { get; set; } = string.Empty;
        public string PaymentReference { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTimeOffset PaymentDate { get; set; }
    }
}

