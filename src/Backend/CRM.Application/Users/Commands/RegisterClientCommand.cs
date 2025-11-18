using System;

namespace CRM.Application.Users.Commands;

public class RegisterClientCommand
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Mobile { get; set; }
    public string? PhoneCode { get; set; }
}
