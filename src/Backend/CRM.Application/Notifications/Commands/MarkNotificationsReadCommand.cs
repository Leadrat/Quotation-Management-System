using System;
using System.Collections.Generic;

namespace CRM.Application.Notifications.Commands
{
    public class MarkNotificationsReadCommand
    {
        public List<Guid>? NotificationIds { get; set; } // null or empty = mark all
        public Guid RequestedByUserId { get; set; }
    }
}

