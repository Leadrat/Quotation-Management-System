using System;
using System.Collections.Generic;

namespace CRM.Application.UserManagement.DTOs;

public class CustomRoleDto
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> Permissions { get; set; } = new();
    public bool IsBuiltIn { get; set; }
    public bool IsActive { get; set; }
    public int UserCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

