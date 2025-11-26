using System;

namespace CRM.Application.Payments.Dtos
{
    public class CreateManualPaymentRequest
    {
        public Guid QuotationId { get; set; }
        public decimal AmountReceived { get; set; }
        public string Currency { get; set; } = "INR";
        public string Method { get; set; } = "Manual"; // e.g., Cash, BankTransfer, Cheque
        public DateTimeOffset PaymentDate { get; set; } = DateTimeOffset.UtcNow;
        public string? Remarks { get; set; }
    }
}
