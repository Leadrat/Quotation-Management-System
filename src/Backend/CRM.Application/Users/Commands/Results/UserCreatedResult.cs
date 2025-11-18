using System;

namespace CRM.Application.Users.Commands.Results;

public class UserCreatedResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public bool EmailSent { get; set; }
    public DateTime? TemporaryPasswordExpiry { get; set; }
}
