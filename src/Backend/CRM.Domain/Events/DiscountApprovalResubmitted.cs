using System;

namespace CRM.Domain.Events
{
    public class DiscountApprovalResubmitted
    {
        public Guid NewApprovalId { get; set; }
        public Guid PreviousApprovalId { get; set; }
        public Guid QuotationId { get; set; }
        public Guid ResubmittedByUserId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Comments { get; set; }
        public DateTimeOffset ResubmissionDate { get; set; }
    }
}

