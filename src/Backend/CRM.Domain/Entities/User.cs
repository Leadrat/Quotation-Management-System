using System;
using System.Collections.Generic;

namespace CRM.Domain.Entities;

public class User
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Mobile { get; set; }
    public string? PhoneCode { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid RoleId { get; set; }
    public Guid? ReportingManagerId { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int LoginAttempts { get; set; }
    public bool IsLockedOut { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public virtual Role? Role { get; set; }
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual User? ReportingManager { get; set; }
    public virtual ICollection<User> DirectReports { get; set; } = new List<User>();

    public string GetFullName() => string.Join(" ", new[] { FirstName, LastName }).Trim();
}
