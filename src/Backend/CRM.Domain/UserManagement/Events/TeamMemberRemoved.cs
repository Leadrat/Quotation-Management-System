using System;

namespace CRM.Domain.UserManagement.Events;

public class TeamMemberRemoved
{
    public Guid TeamMemberId { get; init; }
    public Guid TeamId { get; init; }
    public string TeamName { get; init; } = string.Empty;
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public DateTime RemovedAt { get; init; }
    public Guid RemovedByUserId { get; init; }
}

