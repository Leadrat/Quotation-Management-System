using System;

namespace CRM.Shared.DTOs;

public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Mobile { get; set; }
    public string? PhoneCode { get; set; }
    public Guid RoleId { get; set; }
    public Guid? ReportingManagerId { get; set; }
}
