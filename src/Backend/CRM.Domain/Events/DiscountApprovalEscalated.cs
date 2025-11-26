using System;

namespace CRM.Domain.Events
{
    public class DiscountApprovalEscalated
    {
        public Guid ApprovalId { get; set; }
        public Guid QuotationId { get; set; }
        public Guid EscalatedByUserId { get; set; }
        public Guid AdminUserId { get; set; }
        public Guid NewApproverUserId { get; set; } // Add missing NewApproverUserId
        public int EscalatedToLevel { get; set; } // Add missing EscalatedToLevel
        public string? Reason { get; set; }
        public DateTimeOffset EscalationDate { get; set; }
    }
}

