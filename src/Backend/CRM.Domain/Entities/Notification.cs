namespace CRM.Domain.Entities;

public class UserNotification
{
    public Guid Id { get; set; }
    public Guid NotificationId { get; set; }
    public Guid UserId { get; set; }
    public Guid RecipientUserId { get; set; } // Alias for UserId to maintain compatibility
    public Guid NotificationTypeId { get; set; }
    public string EventType { get; set; } = string.Empty; // For compatibility with existing code
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
    public bool IsArchived { get; set; } = false;
    public DateTimeOffset? ArchivedAt { get; set; }
    public string SentVia { get; set; } = string.Empty;
    public string DeliveredChannels { get; set; } = string.Empty;
    public string DeliveryStatus { get; set; } = "PENDING";
    public string? Meta { get; set; }
    public string? Metadata { get; set; } // Additional metadata field
    public int Priority { get; set; } = 1; // Default priority
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public NotificationType NotificationType { get; set; } = null!;
}