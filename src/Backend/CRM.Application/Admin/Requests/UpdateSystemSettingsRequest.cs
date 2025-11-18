namespace CRM.Application.Admin.Requests;

/// <summary>
/// Request to update system settings
/// </summary>
public class UpdateSystemSettingsRequest
{
    public Dictionary<string, object> Settings { get; set; } = new();
}

