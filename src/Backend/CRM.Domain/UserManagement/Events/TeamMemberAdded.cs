using System;

namespace CRM.Domain.UserManagement.Events;

public class TeamMemberAdded
{
    public Guid TeamMemberId { get; init; }
    public Guid TeamId { get; init; }
    public string TeamName { get; init; } = string.Empty;
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public DateTime AddedAt { get; init; }
    public Guid AddedByUserId { get; init; }
}

