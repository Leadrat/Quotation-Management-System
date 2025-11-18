using CRM.Application.Admin.DTOs;

namespace CRM.Application.Admin.Commands;

/// <summary>
/// Command to update notification settings
/// </summary>
public class UpdateNotificationSettingsCommand
{
    public string? BannerMessage { get; set; }
    public string? BannerType { get; set; }
    public bool IsVisible { get; set; }
    public Guid UpdatedBy { get; set; }
    public string? IpAddress { get; set; }
}

