using System;
using System.Collections.Generic;

namespace CRM.Application.Notifications.Dtos
{
    public class ArchiveNotificationsRequest
    {
        public List<Guid>? NotificationIds { get; set; } // null or empty = archive all
    }
}
