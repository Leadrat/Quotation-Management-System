using System;
using System.Collections.Generic;

namespace CRM.Application.Reports.Dtos
{
    public class AuditReportDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<AuditEntryData> Changes { get; set; } = new();
        public List<ApprovalHistoryData> Approvals { get; set; } = new();
        public List<PaymentHistoryData> Payments { get; set; } = new();
        public List<UserActivityData> UserActivity { get; set; } = new();
    }

    public class AuditEntryData
    {
        public Guid EntryId { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string Action { get; set; } = string.Empty; // "Created", "Updated", "Deleted"
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTimeOffset Timestamp { get; set; }
        public Dictionary<string, object>? Changes { get; set; }
    }

    public class ApprovalHistoryData
    {
        public Guid ApprovalId { get; set; }
        public Guid QuotationId { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public Guid RequestedByUserId { get; set; }
        public string RequestedByUserName { get; set; } = string.Empty;
        public Guid? ApprovedByUserId { get; set; }
        public string? ApprovedByUserName { get; set; }
        public DateTimeOffset RequestedAt { get; set; }
        public DateTimeOffset? ApprovedAt { get; set; }
    }

    public class PaymentHistoryData
    {
        public Guid PaymentId { get; set; }
        public Guid QuotationId { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public string PaymentGateway { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? PaymentDate { get; set; }
    }

    public class UserActivityData
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string ActivityType { get; set; } = string.Empty; // "Login", "QuotationCreated", etc.
        public DateTimeOffset Timestamp { get; set; }
        public string? Details { get; set; }
    }
}

