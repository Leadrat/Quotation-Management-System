using CRM.Application.DiscountApprovals.Dtos;

namespace CRM.Application.DiscountApprovals.Commands
{
    public class ApproveDiscountApprovalCommand
    {
        public Guid ApprovalId { get; set; }
        public ApproveDiscountApprovalRequest Request { get; set; } = null!;
        public Guid ApprovedByUserId { get; set; }
    }
}

