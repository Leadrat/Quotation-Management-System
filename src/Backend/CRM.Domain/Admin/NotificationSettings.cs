namespace CRM.Domain.Admin;

/// <summary>
/// Global system messages/notifications (banners)
/// </summary>
public class NotificationSettings
{
    public Guid Id { get; set; }
    public string? BannerMessage { get; set; } // Sanitized HTML
    public string? BannerType { get; set; } // "info", "warning", "error"
    public bool IsVisible { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid UpdatedBy { get; set; }

    // Navigation property
    public Entities.User? UpdatedByUser { get; set; }
}

