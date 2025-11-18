namespace CRM.Application.Admin.Commands;

/// <summary>
/// Command to delete an integration key
/// </summary>
public class DeleteIntegrationKeyCommand
{
    public Guid Id { get; set; }
    public Guid DeletedBy { get; set; }
    public string? IpAddress { get; set; }
}

