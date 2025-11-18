using System;

namespace CRM.Domain.Events;

public class TokenRefreshed
{
    public Guid UserId { get; init; }
    public DateTime Timestamp { get; init; }
    public string IpAddress { get; init; } = string.Empty;
}
