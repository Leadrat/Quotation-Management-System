using System;

namespace CRM.Domain.UserManagement.Events;

public class UserActivityLogged
{
    public Guid ActivityId { get; init; }
    public Guid UserId { get; init; }
    public string ActionType { get; init; } = string.Empty;
    public string? EntityType { get; init; }
    public Guid? EntityId { get; init; }
    public string IpAddress { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}

