using System.Collections.Generic;

namespace CRM.Application.UserManagement.Requests;

public class UpdateUserGroupRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<string>? Permissions { get; set; }
}

