namespace CRM.Application.Roles.Commands;

public class CreateRoleCommand
{
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
}
