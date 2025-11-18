namespace CRM.Application.Admin.Requests;

/// <summary>
/// Request to create a new integration key
/// </summary>
public class CreateIntegrationKeyRequest
{
    public string KeyName { get; set; } = string.Empty;
    public string KeyValue { get; set; } = string.Empty; // Plain text, will be encrypted
    public string Provider { get; set; } = string.Empty;
}

