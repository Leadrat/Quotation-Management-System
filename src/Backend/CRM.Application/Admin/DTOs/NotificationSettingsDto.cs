namespace CRM.Application.Admin.DTOs;

/// <summary>
/// DTO for notification settings (global messages)
/// </summary>
public class NotificationSettingsDto
{
    public Guid Id { get; set; }
    public string? BannerMessage { get; set; }
    public string? BannerType { get; set; }
    public bool IsVisible { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid UpdatedBy { get; set; }
}

