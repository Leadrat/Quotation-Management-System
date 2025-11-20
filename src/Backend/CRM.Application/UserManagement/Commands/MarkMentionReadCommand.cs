using System;

namespace CRM.Application.UserManagement.Commands;

public class MarkMentionReadCommand
{
    public Guid MentionId { get; set; }
    public Guid UserId { get; set; }
}

