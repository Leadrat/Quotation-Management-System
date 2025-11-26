using System;
using System.ComponentModel.DataAnnotations.Schema;
using CRM.Domain.Entities;

namespace CRM.Domain.UserManagement;

[Table("Mentions")]
public class Mention
{
    public Guid MentionId { get; set; }
    public string EntityType { get; set; } = string.Empty; // Comment, Note
    public Guid EntityId { get; set; }
    public Guid MentionedUserId { get; set; }
    public Guid MentionedByUserId { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual User MentionedUser { get; set; } = null!;
    public virtual User MentionedByUser { get; set; } = null!;

    public void MarkAsRead()
    {
        IsRead = true;
    }
}

