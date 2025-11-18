using System;

namespace CRM.Application.Notifications.Dtos
{
    public class NotificationDto
    {
        public Guid NotificationId { get; set; }
        public Guid RecipientUserId { get; set; }
        public string RelatedEntityType { get; set; } = string.Empty;
        public Guid RelatedEntityId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public bool IsArchived { get; set; }
        public string? DeliveredChannels { get; set; }
        public string DeliveryStatus { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ReadAt { get; set; }
        public DateTimeOffset? ArchivedAt { get; set; }
        public string? Meta { get; set; }

        // Computed properties
        public bool IsUnread => !IsRead && !IsArchived;
        public bool IsDelivered => DeliveryStatus == "DELIVERED";
        public string FormattedDate => CreatedAt.ToString("MMM dd, yyyy HH:mm");
        public string EntityLinkUrl => $"/{RelatedEntityType.ToLower()}/{RelatedEntityId}";
    }
}

