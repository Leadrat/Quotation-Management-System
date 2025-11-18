using CRM.Application.Payments.Dtos;

namespace CRM.Application.Payments.Commands
{
    public class UpdatePaymentGatewayConfigCommand
    {
        public Guid ConfigId { get; set; }
        public UpdatePaymentGatewayConfigRequest Request { get; set; } = null!;
        public Guid UpdatedByUserId { get; set; }
    }
}

