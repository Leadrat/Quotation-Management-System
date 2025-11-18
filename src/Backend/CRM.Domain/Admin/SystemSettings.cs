using System.Text.Json;

namespace CRM.Domain.Admin;

/// <summary>
/// System-wide configuration settings stored as key-value pairs
/// </summary>
public class SystemSettings
{
    public string Key { get; set; } = string.Empty;
    public JsonDocument Value { get; set; } = null!;
    public DateTimeOffset LastModifiedAt { get; set; }
    public Guid LastModifiedBy { get; set; }

    // Navigation property
    public Entities.User? LastModifiedByUser { get; set; }
}

