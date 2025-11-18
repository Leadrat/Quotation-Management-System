using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Domain.Entities
{
    [Table("EmailNotificationLog")]
    public class EmailNotificationLog
    {
        public Guid LogId { get; set; }
        public Guid? NotificationId { get; set; }
        public string RecipientEmail { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public DateTimeOffset SentAt { get; set; }
        public DateTimeOffset? DeliveredAt { get; set; }
        public string Status { get; set; } = "SENT"; // "SENT", "DELIVERED", "BOUNCED", "FAILED", etc.
        public string? ErrorMsg { get; set; }
        public int RetryCount { get; set; } = 0;
        public DateTimeOffset? LastRetryAt { get; set; }

        // Navigation property
        public virtual Notification? Notification { get; set; }

        // Domain methods
        public void MarkAsDelivered()
        {
            Status = "DELIVERED";
            DeliveredAt = DateTimeOffset.UtcNow;
        }

        public void MarkAsFailed(string errorMessage)
        {
            Status = "FAILED";
            ErrorMsg = errorMessage;
        }

        public void IncrementRetry()
        {
            RetryCount++;
            LastRetryAt = DateTimeOffset.UtcNow;
        }
    }
}

