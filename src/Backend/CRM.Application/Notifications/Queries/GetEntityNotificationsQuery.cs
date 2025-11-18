using System;

namespace CRM.Application.Notifications.Queries
{
    public class GetEntityNotificationsQuery
    {
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public Guid RequestorUserId { get; set; }
    }
}

