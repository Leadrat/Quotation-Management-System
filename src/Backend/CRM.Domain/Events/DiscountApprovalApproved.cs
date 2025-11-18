using System;

namespace CRM.Domain.Events
{
    public class DiscountApprovalApproved
    {
        public Guid ApprovalId { get; set; }
        public Guid QuotationId { get; set; }
        public Guid ApprovedByUserId { get; set; }
        public decimal DiscountPercentage { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Comments { get; set; }
        public DateTimeOffset ApprovalDate { get; set; }
    }
}

