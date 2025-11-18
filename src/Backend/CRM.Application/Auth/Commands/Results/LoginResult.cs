using System;

namespace CRM.Application.Auth.Commands.Results;

public class LoginResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public object User { get; set; } = new { };
    public DateTime Timestamp { get; set; }
}
