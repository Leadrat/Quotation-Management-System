using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using CRM.Domain.Entities;

namespace CRM.Domain.UserManagement;

[Table("UserGroups")]
public class UserGroup
{
    public Guid GroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Permissions { get; set; } = "[]"; // JSONB array of permission strings
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public virtual User CreatedByUser { get; set; } = null!;
    public virtual ICollection<UserGroupMember> Members { get; set; } = new List<UserGroupMember>();

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
        Permissions = JsonSerializer.Serialize(permissions);
    }
}

