namespace CRM.Application.Admin.Requests;

/// <summary>
/// Request to update notification settings (global messages)
/// </summary>
public class UpdateNotificationSettingsRequest
{
    public string? BannerMessage { get; set; }
    public string? BannerType { get; set; } // "info", "warning", "error"
    public bool IsVisible { get; set; }
}

