using System;

namespace CRM.Application.UserManagement.DTOs;

public class MentionDto
{
    public Guid MentionId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public Guid MentionedUserId { get; set; }
    public string MentionedUserName { get; set; } = string.Empty;
    public Guid MentionedByUserId { get; set; }
    public string MentionedByUserName { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

