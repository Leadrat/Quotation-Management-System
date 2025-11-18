using System;

namespace CRM.Application.Auth.Commands.Results;

public class LogoutResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
