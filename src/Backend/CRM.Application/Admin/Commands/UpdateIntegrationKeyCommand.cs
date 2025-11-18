using CRM.Application.Admin.DTOs;

namespace CRM.Application.Admin.Commands;

/// <summary>
/// Command to update an integration key
/// </summary>
public class UpdateIntegrationKeyCommand
{
    public Guid Id { get; set; }
    public string? KeyName { get; set; }
    public string? KeyValue { get; set; }
    public string? Provider { get; set; }
    public Guid UpdatedBy { get; set; }
    public string? IpAddress { get; set; }
}

