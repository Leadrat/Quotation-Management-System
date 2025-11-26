using System;

namespace CRM.Application.UserManagement.Requests;

public class CreateMentionRequest
{
    public string EntityType { get; set; } = string.Empty; // Comment, Note
    public Guid EntityId { get; set; }
    public Guid MentionedUserId { get; set; }
}

