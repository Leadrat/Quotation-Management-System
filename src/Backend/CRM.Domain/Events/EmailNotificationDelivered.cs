using System;

namespace CRM.Domain.Events
{
    public class EmailNotificationDelivered
    {
        public Guid LogId { get; set; }
        public DateTimeOffset DeliveredAt { get; set; }
    }
}

