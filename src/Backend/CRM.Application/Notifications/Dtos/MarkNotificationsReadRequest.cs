using System;
using System.Collections.Generic;

namespace CRM.Application.Notifications.Dtos
{
    public class MarkNotificationsReadRequest
    {
        public List<Guid>? NotificationIds { get; set; } // null or empty = mark all
    }
}
