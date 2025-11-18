using CRM.Application.Payments.Dtos;

namespace CRM.Application.Payments.Commands
{
    public class UpdatePaymentStatusCommand
    {
        public UpdatePaymentStatusRequest Request { get; set; } = null!;
        public string GatewayName { get; set; } = string.Empty;
    }
}

