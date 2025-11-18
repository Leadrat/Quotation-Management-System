using CRM.Application.DiscountApprovals.Dtos;

namespace CRM.Application.DiscountApprovals.Commands
{
    public class ResubmitDiscountApprovalCommand
    {
        public Guid ApprovalId { get; set; } // The rejected approval ID
        public ResubmitDiscountApprovalRequest Request { get; set; } = null!;
        public Guid ResubmittedByUserId { get; set; }
    }
}

