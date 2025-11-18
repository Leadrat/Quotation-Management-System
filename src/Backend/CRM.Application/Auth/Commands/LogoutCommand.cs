using System;

namespace CRM.Application.Auth.Commands;

public class LogoutCommand
{
    public Guid UserId { get; set; }
    public string? RefreshToken { get; set; }
}
