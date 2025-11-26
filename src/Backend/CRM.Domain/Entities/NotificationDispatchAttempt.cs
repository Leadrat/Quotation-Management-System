using CRM.Domain.Enums;

namespace CRM.Domain.Entities;

public class NotificationDispatchAttempt
{
    public int Id { get; set; }
    public Guid NotificationId { get; set; }
    public NotificationChannel Channel { get; set; }
    public DispatchStatus Status { get; set; }
    public DateTimeOffset AttemptedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; } // When delivery was confirmed
    public string? ErrorMessage { get; set; }
    public string? ErrorDetails { get; set; } // Detailed error information
    public string? ExternalReference { get; set; }
    public string? ExternalId { get; set; } // External system identifier
    public int RetryCount { get; set; }
    public int AttemptNumber { get; set; } = 1; // Current attempt number
    public DateTimeOffset? NextRetryAt { get; set; }
    public int? NotificationTemplateId { get; set; } // Reference to the template used
    
    // Navigation properties
    public UserNotification Notification { get; set; } = null!;
    public NotificationTemplate? NotificationTemplate { get; set; }
}