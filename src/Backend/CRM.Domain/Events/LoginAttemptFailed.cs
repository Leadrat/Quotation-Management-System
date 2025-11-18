using System;

namespace CRM.Domain.Events;

public class LoginAttemptFailed
{
    public string Email { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public string IpAddress { get; init; } = string.Empty;
    public string FailureReason { get; init; } = string.Empty; // WrongPassword, UserNotFound, Inactive
    public int Attempt { get; init; }
}
