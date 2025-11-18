using CRM.Application.DiscountApprovals.Dtos;

namespace CRM.Application.DiscountApprovals.Commands
{
    public class RejectDiscountApprovalCommand
    {
        public Guid ApprovalId { get; set; }
        public RejectDiscountApprovalRequest Request { get; set; } = null!;
        public Guid RejectedByUserId { get; set; }
    }
}

