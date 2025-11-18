using System;

namespace CRM.Domain.Events
{
    public class AdjustmentApproved
    {
        public Guid AdjustmentId { get; set; }
        public Guid QuotationId { get; set; }
        public Guid ApprovedByUserId { get; set; }
        public DateTimeOffset ApprovalDate { get; set; }
    }
}

