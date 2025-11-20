using System;
using System.Collections.Generic;

namespace CRM.Application.UserManagement.Requests;

public class CreateUserGroupRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> Permissions { get; set; } = new();
}

