namespace CRM.Application.Notifications.Dtos;

public class NotificationDto
{
    public Guid NotificationId { get; set; }
    public Guid RecipientUserId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
    public bool IsArchived { get; set; }
    public DateTimeOffset? ArchivedAt { get; set; }
    public string SentVia { get; set; } = string.Empty;
    public string DeliveredChannels { get; set; } = string.Empty;
    public string DeliveryStatus { get; set; } = string.Empty;
    public string? Meta { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public NotificationTypeDto NotificationType { get; set; } = null!;
}
