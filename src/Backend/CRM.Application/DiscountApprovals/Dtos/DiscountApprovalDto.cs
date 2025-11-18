using System;

namespace CRM.Application.DiscountApprovals.Dtos
{
    public class DiscountApprovalDto
    {
        public Guid ApprovalId { get; set; }
        public Guid QuotationId { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public Guid ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public Guid RequestedByUserId { get; set; }
        public string RequestedByUserName { get; set; } = string.Empty;
        public Guid? ApproverUserId { get; set; }
        public string? ApproverUserName { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset RequestDate { get; set; }
        public DateTimeOffset? ApprovalDate { get; set; }
        public DateTimeOffset? RejectionDate { get; set; }
        public decimal CurrentDiscountPercentage { get; set; }
        public decimal Threshold { get; set; }
        public string ApprovalLevel { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string? Comments { get; set; }
        public bool EscalatedToAdmin { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        // Computed properties
        public bool IsPending => Status == "Pending";
        public bool IsApproved => Status == "Approved";
        public bool IsRejected => Status == "Rejected";
    }
}

