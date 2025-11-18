namespace CRM.Application.Admin.Requests;

/// <summary>
/// Request to update an integration key
/// </summary>
public class UpdateIntegrationKeyRequest
{
    public string? KeyName { get; set; }
    public string? KeyValue { get; set; } // Plain text, will be encrypted if provided
    public string? Provider { get; set; }
}

