using System;

namespace CRM.Application.DiscountApprovals.Dtos
{
    public class ApprovalTimelineDto
    {
        public Guid ApprovalId { get; set; }
        public Guid QuotationId { get; set; }
        public string EventType { get; set; } = string.Empty; // Requested, Approved, Rejected, Escalated, Resubmitted
        public string Status { get; set; } = string.Empty;
        public string? PreviousStatus { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string? Comments { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}

