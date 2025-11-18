using CRM.Application.Admin.DTOs;

namespace CRM.Application.Admin.Commands;

/// <summary>
/// Command to update system settings
/// </summary>
public class UpdateSystemSettingsCommand
{
    public Dictionary<string, object> Settings { get; set; } = new();
    public Guid ModifiedBy { get; set; }
    public string? IpAddress { get; set; }
}

