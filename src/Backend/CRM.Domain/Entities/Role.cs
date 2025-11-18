using System;
using System.Collections.Generic;

namespace CRM.Domain.Entities;

public class Role
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public string GetDisplayName() => RoleName;
}
