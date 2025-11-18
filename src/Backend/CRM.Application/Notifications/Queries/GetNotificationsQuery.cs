using System;

namespace CRM.Application.Notifications.Queries
{
    public class GetNotificationsQuery
    {
        public bool? Unread { get; set; }
        public bool? Archived { get; set; }
        public string? EventType { get; set; }
        public string? EntityType { get; set; }
        public Guid? EntityId { get; set; }
        public DateTimeOffset? DateFrom { get; set; }
        public DateTimeOffset? DateTo { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public Guid RequestorUserId { get; set; }
    }
}

