using CRM.Application.Payments.Dtos;

namespace CRM.Application.Payments.Commands
{
    public class CreatePaymentGatewayConfigCommand
    {
        public CreatePaymentGatewayConfigRequest Request { get; set; } = null!;
        public Guid CreatedByUserId { get; set; }
    }
}

