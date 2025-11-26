using CRM.Application.UserManagement.Requests;

namespace CRM.Application.UserManagement.Commands;

public class CreateCustomRoleCommand
{
    public CreateCustomRoleRequest Request { get; set; } = null!;
    public Guid CreatedByUserId { get; set; }
    public string RequestorRole { get; set; } = string.Empty;
}

