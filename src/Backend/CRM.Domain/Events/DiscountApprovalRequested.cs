using System;

namespace CRM.Domain.Events
{
    public class DiscountApprovalRequested
    {
        public Guid ApprovalId { get; set; }
        public Guid QuotationId { get; set; }
        public Guid RequestedByUserId { get; set; }
        public Guid? ApproverUserId { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal Threshold { get; set; }
        public string ApprovalLevel { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string? Comments { get; set; }
        public DateTimeOffset RequestDate { get; set; }
    }
}

