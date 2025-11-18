using System;

namespace CRM.Domain.Events
{
    public class UserNotificationPreferenceUpdated
    {
        public Guid UserId { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}

