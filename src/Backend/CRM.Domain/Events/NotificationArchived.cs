using System;

namespace CRM.Domain.Events
{
    public class NotificationArchived
    {
        public Guid NotificationId { get; set; }
        public Guid UserId { get; set; }
        public DateTimeOffset ArchivedAt { get; set; }
    }
}

