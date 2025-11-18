using System;

namespace CRM.Domain.Events
{
    public class NotificationPublished
    {
        public Guid NotificationId { get; set; }
        public Guid RecipientUserId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string RelatedEntityType { get; set; } = string.Empty;
        public Guid RelatedEntityId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}

