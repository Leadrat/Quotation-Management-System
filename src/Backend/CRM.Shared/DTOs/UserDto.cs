using System;

namespace CRM.Shared.DTOs;

public class UserDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Mobile { get; set; }
    public string? PhoneCode { get; set; }
    public bool IsActive { get; set; }
    public Guid RoleId { get; set; }
    public string? RoleName { get; set; }
    public Guid? ReportingManagerId { get; set; }
    public string? ReportingManagerName { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
