using System;

namespace CRM.Application.Notifications.Queries
{
    public class GetEmailNotificationLogsQuery
    {
        public Guid? UserId { get; set; }
        public string? RecipientEmail { get; set; }
        public string? EventType { get; set; }
        public string? Status { get; set; }
        public DateTimeOffset? DateFrom { get; set; }
        public DateTimeOffset? DateTo { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public Guid RequestorUserId { get; set; }
        public string RequestorRole { get; set; } = string.Empty;
    }
}

