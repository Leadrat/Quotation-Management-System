using System;
using System.Collections.Generic;
using System.Text.Json;

namespace CRM.Domain.Entities;

public class Role
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Permissions { get; set; } = "[]"; // JSONB array of permission strings
    public bool IsBuiltIn { get; set; } = false; // Built-in roles cannot be modified
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public string GetDisplayName() => RoleName;

    public List<string> GetPermissions()
    {
        if (string.IsNullOrWhiteSpace(Permissions) || Permissions == "[]")
            return new List<string>();
        
        try
        {
            return JsonSerializer.Deserialize<List<string>>(Permissions) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    public void SetPermissions(List<string> permissions)
    {
        Permissions = JsonSerializer.Serialize(permissions ?? new List<string>());
        UpdatedAt = DateTime.UtcNow;
    }
}
