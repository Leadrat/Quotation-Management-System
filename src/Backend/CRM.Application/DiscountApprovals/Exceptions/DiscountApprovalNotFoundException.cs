using System;

namespace CRM.Application.DiscountApprovals.Exceptions
{
    public class DiscountApprovalNotFoundException : Exception
    {
        public DiscountApprovalNotFoundException(Guid approvalId)
            : base($"Discount approval with ID {approvalId} was not found.")
        {
            ApprovalId = approvalId;
        }

        public Guid ApprovalId { get; }
    }
}

