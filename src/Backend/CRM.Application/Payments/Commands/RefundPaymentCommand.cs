using CRM.Application.Payments.Dtos;

namespace CRM.Application.Payments.Commands
{
    public class RefundPaymentCommand
    {
        public Guid PaymentId { get; set; }
        public RefundPaymentRequest Request { get; set; } = null!;
        public Guid RefundedByUserId { get; set; }
    }
}

