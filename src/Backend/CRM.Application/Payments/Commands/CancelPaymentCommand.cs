namespace CRM.Application.Payments.Commands
{
    public class CancelPaymentCommand
    {
        public Guid PaymentId { get; set; }
        public Guid CancelledByUserId { get; set; }
    }
}

