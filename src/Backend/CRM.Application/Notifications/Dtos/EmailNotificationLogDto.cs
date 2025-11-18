using System;

namespace CRM.Application.Notifications.Dtos
{
    public class EmailNotificationLogDto
    {
        public Guid LogId { get; set; }
        public Guid? NotificationId { get; set; }
        public string RecipientEmail { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public DateTimeOffset SentAt { get; set; }
        public DateTimeOffset? DeliveredAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ErrorMsg { get; set; }
        public int RetryCount { get; set; }
        public DateTimeOffset? LastRetryAt { get; set; }

        // Computed properties
        public string FormattedSentAt => SentAt.ToString("MMM dd, yyyy HH:mm");
        public string? FormattedDeliveredAt => DeliveredAt?.ToString("MMM dd, yyyy HH:mm");
    }
}

