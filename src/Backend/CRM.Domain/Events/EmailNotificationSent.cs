using System;

namespace CRM.Domain.Events
{
    public class EmailNotificationSent
    {
        public Guid LogId { get; set; }
        public Guid? NotificationId { get; set; }
        public string RecipientEmail { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public DateTimeOffset SentAt { get; set; }
    }
}

