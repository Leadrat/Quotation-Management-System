using System;
using CRM.Domain.Enums;

namespace CRM.Domain.Entities
{
    /// <summary>
    /// Audit trail for all refund actions
    /// </summary>
    public class RefundTimeline
    {
        public Guid TimelineId { get; set; }
        public Guid RefundId { get; set; }
        public RefundTimelineEventType EventType { get; set; }
        public Guid ActedByUserId { get; set; }
        public string? Comments { get; set; }
        public DateTimeOffset EventDate { get; set; }
        public string? IpAddress { get; set; }

        // Navigation properties
        public virtual Refund Refund { get; set; } = null!;
        public virtual User ActedByUser { get; set; } = null!;
    }
}

