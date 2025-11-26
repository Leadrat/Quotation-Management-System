namespace CRM.Application.Notifications.Dtos;

public class NotificationTypeDto
{
    public Guid NotificationTypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string? Description { get; set; }
}
