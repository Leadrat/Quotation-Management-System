using System;

namespace CRM.Domain.UserManagement.Events;

public class TaskAssigned
{
    public Guid AssignmentId { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public Guid EntityId { get; init; }
    public Guid AssignedToUserId { get; init; }
    public string AssignedToUserName { get; init; } = string.Empty;
    public Guid AssignedByUserId { get; init; }
    public string AssignedByUserName { get; init; } = string.Empty;
    public DateTime? DueDate { get; init; }
    public DateTime CreatedAt { get; init; }
}

