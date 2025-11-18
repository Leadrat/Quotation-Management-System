using System;
using System.Text.Json;

namespace CRM.Domain.Entities
{
    /// <summary>
    /// Manages scheduled report delivery via email
    /// </summary>
    public class ScheduledReport
    {
        public Guid ReportId { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string ReportName { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public JsonDocument ReportConfig { get; set; } = null!;
        public string RecurrencePattern { get; set; } = string.Empty; // "daily", "weekly", "monthly"
        public string EmailRecipients { get; set; } = string.Empty; // Comma-separated emails
        public bool IsActive { get; set; }
        public DateTimeOffset? LastSentAt { get; set; }
        public DateTimeOffset NextScheduledAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        // Navigation
        public User CreatedByUser { get; set; } = null!;
    }
}

