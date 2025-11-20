using System;
using CRM.Application.UserManagement.Requests;

namespace CRM.Application.UserManagement.Commands;

public class UpdateRolePermissionsCommand
{
    public Guid RoleId { get; set; }
    public UpdateRolePermissionsRequest Request { get; set; } = null!;
    public Guid UpdatedByUserId { get; set; }
    public string RequestorRole { get; set; } = string.Empty;
}

