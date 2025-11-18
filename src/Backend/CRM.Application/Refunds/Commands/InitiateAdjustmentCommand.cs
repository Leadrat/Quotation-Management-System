using CRM.Application.Refunds.Dtos;

namespace CRM.Application.Refunds.Commands
{
    public class InitiateAdjustmentCommand
    {
        public CreateAdjustmentRequest Request { get; set; } = null!;
        public Guid RequestedByUserId { get; set; }
    }
}

