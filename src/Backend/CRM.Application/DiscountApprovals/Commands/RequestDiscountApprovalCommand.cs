using CRM.Application.DiscountApprovals.Dtos;

namespace CRM.Application.DiscountApprovals.Commands
{
    public class RequestDiscountApprovalCommand
    {
        public CreateDiscountApprovalRequest Request { get; set; } = null!;
        public Guid RequestedByUserId { get; set; }
    }
}

