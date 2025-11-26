using System;

namespace CRM.Application.UserManagement.Commands;

public class RemoveTeamMemberCommand
{
    public Guid TeamId { get; set; }
    public Guid UserId { get; set; }
    public Guid RemovedByUserId { get; set; }
    public string RequestorRole { get; set; } = string.Empty;
}

