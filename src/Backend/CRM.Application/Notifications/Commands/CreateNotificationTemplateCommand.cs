using CRM.Application.Notifications.DTOs;
using CRM.Domain.Enums;

namespace CRM.Application.Notifications.Commands;

public class CreateNotificationTemplateCommand
{
    public string TemplateKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public NotificationChannel Channel { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string BodyTemplate { get; set; } = string.Empty;
    public List<string> Variables { get; set; } = new();
    public bool IsActive { get; set; } = true;
}

public class CreateNotificationTemplateCommandResult
{
    public NotificationTemplateDto Template { get; set; } = null!;
}