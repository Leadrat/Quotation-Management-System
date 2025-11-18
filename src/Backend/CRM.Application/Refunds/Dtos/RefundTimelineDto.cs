using System;
using CRM.Domain.Enums;

namespace CRM.Application.Refunds.Dtos
{
    public class RefundTimelineDto
    {
        public Guid TimelineId { get; set; }
        public Guid RefundId { get; set; }
        public RefundTimelineEventType EventType { get; set; }
        public Guid ActedByUserId { get; set; }
        public string ActedByUserName { get; set; } = string.Empty;
        public string? Comments { get; set; }
        public DateTimeOffset EventDate { get; set; }
        public string? IpAddress { get; set; }
    }
}

