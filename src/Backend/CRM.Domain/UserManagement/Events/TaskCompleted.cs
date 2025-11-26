using System;

namespace CRM.Domain.UserManagement.Events;

public class TaskCompleted
{
    public Guid AssignmentId { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public Guid EntityId { get; init; }
    public Guid AssignedToUserId { get; init; }
    public string AssignedToUserName { get; init; } = string.Empty;
    public DateTime CompletedAt { get; init; }
}

