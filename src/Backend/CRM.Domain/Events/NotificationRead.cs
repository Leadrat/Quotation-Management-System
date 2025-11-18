using System;

namespace CRM.Domain.Events
{
    public class NotificationRead
    {
        public Guid NotificationId { get; set; }
        public Guid UserId { get; set; }
        public DateTimeOffset ReadAt { get; set; }
    }
}

