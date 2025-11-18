using System;
using System.Collections.Generic;

namespace CRM.Application.Notifications.Commands
{
    public class ArchiveNotificationsCommand
    {
        public List<Guid>? NotificationIds { get; set; } // null or empty = archive all
        public Guid RequestedByUserId { get; set; }
    }
}

