using System.Collections.Generic;

namespace CRM.Application.UserManagement.Requests;

public class CreateCustomRoleRequest
{
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> Permissions { get; set; } = new();
}

