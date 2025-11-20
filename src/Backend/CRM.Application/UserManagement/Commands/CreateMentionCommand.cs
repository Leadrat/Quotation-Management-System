using CRM.Application.UserManagement.Requests;

namespace CRM.Application.UserManagement.Commands;

public class CreateMentionCommand
{
    public CreateMentionRequest Request { get; set; } = null!;
    public Guid MentionedByUserId { get; set; }
}

