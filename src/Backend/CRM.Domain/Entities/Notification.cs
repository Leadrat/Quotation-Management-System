using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Domain.Entities
{
    [Table("Notifications")]
    public class Notification
    {
        public Guid NotificationId { get; set; }
        public Guid RecipientUserId { get; set; }
        public string RelatedEntityType { get; set; } = string.Empty; // "Quotation", "Approval", etc.
        public Guid RelatedEntityId { get; set; }
        public string EventType { get; set; } = string.Empty; // "SENT", "VIEWED", "APPROVED", etc.
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public bool IsArchived { get; set; } = false;
        public string? DeliveredChannels { get; set; } // "in-app,email,push"
        public string DeliveryStatus { get; set; } = "SENT"; // "SENT", "DELIVERED", "FAILED"
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ReadAt { get; set; }
        public DateTimeOffset? ArchivedAt { get; set; }
        public string? Meta { get; set; } // JSON string for extra context

        // Navigation properties
        public virtual User RecipientUser { get; set; } = null!;

        // Domain methods
        public void MarkAsRead()
        {
            IsRead = true;
            ReadAt = DateTimeOffset.UtcNow;
        }

        public void Archive()
        {
            IsArchived = true;
            ArchivedAt = DateTimeOffset.UtcNow;
        }

        public void Unarchive()
        {
            IsArchived = false;
            ArchivedAt = null;
        }

        public bool IsUnread() => !IsRead && !IsArchived;

        public bool IsDelivered() => DeliveryStatus == "DELIVERED";
    }
}

