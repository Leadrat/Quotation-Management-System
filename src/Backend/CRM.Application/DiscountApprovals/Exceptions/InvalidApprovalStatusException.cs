using System;

namespace CRM.Application.DiscountApprovals.Exceptions
{
    public class InvalidApprovalStatusException : Exception
    {
        public InvalidApprovalStatusException(string currentStatus, string requiredStatus)
            : base($"Approval is in {currentStatus} status, but {requiredStatus} status is required for this operation.")
        {
            CurrentStatus = currentStatus;
            RequiredStatus = requiredStatus;
        }

        public string CurrentStatus { get; }
        public string RequiredStatus { get; }
    }
}

