using CRM.Application.Payments.Dtos;

namespace CRM.Application.Payments.Commands
{
    public class InitiatePaymentCommand
    {
        public InitiatePaymentRequest Request { get; set; } = null!;
        public Guid InitiatedByUserId { get; set; }
    }
}

