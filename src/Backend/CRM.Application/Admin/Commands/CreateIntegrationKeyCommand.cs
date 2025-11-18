using CRM.Application.Admin.DTOs;

namespace CRM.Application.Admin.Commands;

/// <summary>
/// Command to create a new integration key
/// </summary>
public class CreateIntegrationKeyCommand
{
    public string KeyName { get; set; } = string.Empty;
    public string KeyValue { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public string? IpAddress { get; set; }
}

