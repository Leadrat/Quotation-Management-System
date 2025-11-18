using System;

namespace CRM.Application.DiscountApprovals.Exceptions
{
    public class UnauthorizedApprovalActionException : Exception
    {
        public UnauthorizedApprovalActionException(Guid approvalId, Guid userId, string action)
            : base($"User {userId} is not authorized to {action} approval {approvalId}.")
        {
            ApprovalId = approvalId;
            UserId = userId;
            Action = action;
        }

        public Guid ApprovalId { get; }
        public Guid UserId { get; }
        public string Action { get; }
    }
}

