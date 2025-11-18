namespace CRM.Domain.Admin.Events;

/// <summary>
/// Domain event raised when system settings are updated
/// </summary>
public class SettingsUpdated
{
    public string SettingKey { get; init; } = string.Empty;
    public Guid ModifiedBy { get; init; }
    public DateTimeOffset ModifiedAt { get; init; }
    public object? OldValue { get; init; }
    public object? NewValue { get; init; }
}

