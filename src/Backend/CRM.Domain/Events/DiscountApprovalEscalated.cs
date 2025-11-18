using System;

namespace CRM.Domain.Events
{
    public class DiscountApprovalEscalated
    {
        public Guid ApprovalId { get; set; }
        public Guid QuotationId { get; set; }
        public Guid EscalatedByUserId { get; set; }
        public Guid AdminUserId { get; set; }
        public string? Reason { get; set; }
        public DateTimeOffset EscalationDate { get; set; }
    }
}

