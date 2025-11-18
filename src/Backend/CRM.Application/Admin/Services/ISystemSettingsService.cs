namespace CRM.Application.Admin.Services;

/// <summary>
/// Service for managing system settings with caching
/// </summary>
public interface ISystemSettingsService
{
    /// <summary>
    /// Gets all system settings (cached)
    /// </summary>
    Task<Dictionary<string, object>> GetAllSettingsAsync();

    /// <summary>
    /// Gets a specific setting value
    /// </summary>
    Task<object?> GetSettingAsync(string key);

    /// <summary>
    /// Updates system settings
    /// </summary>
    Task UpdateSettingsAsync(Dictionary<string, object> settings, Guid modifiedBy);

    /// <summary>
    /// Invalidates the settings cache
    /// </summary>
    void InvalidateCache();
}

