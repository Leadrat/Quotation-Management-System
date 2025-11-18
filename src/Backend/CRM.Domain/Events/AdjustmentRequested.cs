using System;

namespace CRM.Domain.Events
{
    public class AdjustmentRequested
    {
        public Guid AdjustmentId { get; set; }
        public Guid QuotationId { get; set; }
        public string AdjustmentType { get; set; } = string.Empty;
        public decimal OriginalAmount { get; set; }
        public decimal AdjustedAmount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public Guid RequestedByUserId { get; set; }
        public string? ApprovalLevel { get; set; }
        public DateTimeOffset RequestDate { get; set; }
    }
}

