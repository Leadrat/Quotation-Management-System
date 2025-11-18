using System;

namespace CRM.Domain.Events;

public class UserLoggedIn
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public string IpAddress { get; init; } = string.Empty;
    public string UserAgent { get; init; } = string.Empty;
}
