using System;

namespace CRM.Domain.Events
{
    public class DiscountApprovalRejected
    {
        public Guid ApprovalId { get; set; }
        public Guid QuotationId { get; set; }
        public Guid RejectedByUserId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Comments { get; set; }
        public DateTimeOffset RejectionDate { get; set; }
    }
}

