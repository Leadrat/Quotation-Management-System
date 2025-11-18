using System;
using System.Collections.Generic;

namespace CRM.Application.Notifications.Dtos
{
    public class UnarchiveNotificationsRequest
    {
        public List<Guid> NotificationIds { get; set; } = new List<Guid>(); // Required - must specify IDs
    }
}
