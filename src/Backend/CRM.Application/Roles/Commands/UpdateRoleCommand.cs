using System;

namespace CRM.Application.Roles.Commands;

public class UpdateRoleCommand
{
    public Guid RoleId { get; set; }
    public string? RoleName { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}
