using System;

namespace CRM.Application.Users.Commands.Results;

public class RegisterResult
{
    public bool Success { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string RedirectUrl { get; set; } = "/login";
}
