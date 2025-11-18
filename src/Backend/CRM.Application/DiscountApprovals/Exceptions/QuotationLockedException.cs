using System;

namespace CRM.Application.DiscountApprovals.Exceptions
{
    public class QuotationLockedException : Exception
    {
        public QuotationLockedException(Guid quotationId, Guid? approvalId = null)
            : base($"Quotation with ID {quotationId} is locked and cannot be edited while pending approval.")
        {
            QuotationId = quotationId;
            ApprovalId = approvalId;
        }

        public Guid QuotationId { get; }
        public Guid? ApprovalId { get; }
    }
}

