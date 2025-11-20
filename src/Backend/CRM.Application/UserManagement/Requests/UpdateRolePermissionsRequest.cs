using System.Collections.Generic;

namespace CRM.Application.UserManagement.Requests;

public class UpdateRolePermissionsRequest
{
    public List<string> Permissions { get; set; } = new();
}

