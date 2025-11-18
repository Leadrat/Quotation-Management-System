using System;

namespace CRM.Domain.Events
{
    public class PaymentCancelled
    {
        public Guid PaymentId { get; set; }
        public Guid QuotationId { get; set; }
        public string PaymentGateway { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
        public DateTimeOffset CancelledAt { get; set; }
        public Guid? CancelledByUserId { get; set; }
    }
}

