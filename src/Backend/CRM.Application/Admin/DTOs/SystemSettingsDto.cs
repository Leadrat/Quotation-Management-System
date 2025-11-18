namespace CRM.Application.Admin.DTOs;

/// <summary>
/// DTO for system settings response
/// </summary>
public class SystemSettingsDto
{
    public Dictionary<string, object> Settings { get; set; } = new();
}

