using System;

namespace CRM.Domain.Events
{
    public class AdjustmentApplied
    {
        public Guid AdjustmentId { get; set; }
        public Guid QuotationId { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal AdjustedAmount { get; set; }
        public DateTimeOffset AppliedDate { get; set; }
    }
}

