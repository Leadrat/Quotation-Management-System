using System;

namespace CRM.Application.UserManagement.Commands;

public class DeleteTeamCommand
{
    public Guid TeamId { get; set; }
    public Guid DeletedByUserId { get; set; }
    public string RequestorRole { get; set; } = string.Empty;
}

