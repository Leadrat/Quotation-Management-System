using System;

namespace CRM.Application.Users.Commands;

public class CreateUserCommand
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Mobile { get; set; }
    public string? PhoneCode { get; set; }
    public Guid RoleId { get; set; }
    public Guid? ReportingManagerId { get; set; }
    public Guid CreatedByUserId { get; set; }
}
