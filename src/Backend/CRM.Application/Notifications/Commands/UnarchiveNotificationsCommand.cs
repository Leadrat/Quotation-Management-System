using System;
using System.Collections.Generic;

namespace CRM.Application.Notifications.Commands
{
    public class UnarchiveNotificationsCommand
    {
        public List<Guid> NotificationIds { get; set; } = new List<Guid>(); // required, non-empty
        public Guid RequestedByUserId { get; set; }
    }
}

