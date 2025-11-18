using CRM.Application.Admin.DTOs;

namespace CRM.Application.Admin.Services;

/// <summary>
/// Service for managing global notification settings (banners)
/// </summary>
public interface INotificationSettingsService
{
    /// <summary>
    /// Gets current notification settings
    /// </summary>
    Task<NotificationSettingsDto?> GetSettingsAsync();

    /// <summary>
    /// Updates notification settings
    /// </summary>
    Task<NotificationSettingsDto> UpdateSettingsAsync(
        string? bannerMessage,
        string? bannerType,
        bool isVisible,
        Guid updatedBy);
}

