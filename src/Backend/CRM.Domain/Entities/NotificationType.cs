namespace CRM.Domain.Entities;

public class NotificationType
{
    public Guid NotificationTypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    
    public ICollection<UserNotification> Notifications { get; set; } = new List<UserNotification>();
}