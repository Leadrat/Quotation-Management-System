using System;

namespace CRM.Domain.UserManagement.Events;

public class UserMentioned
{
    public Guid MentionId { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public Guid EntityId { get; init; }
    public Guid MentionedUserId { get; init; }
    public string MentionedUserName { get; init; } = string.Empty;
    public Guid MentionedByUserId { get; init; }
    public string MentionedByUserName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

