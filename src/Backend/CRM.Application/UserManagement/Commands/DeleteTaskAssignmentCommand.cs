using System;

namespace CRM.Application.UserManagement.Commands;

public class DeleteTaskAssignmentCommand
{
    public Guid AssignmentId { get; set; }
    public Guid DeletedByUserId { get; set; }
    public string RequestorRole { get; set; } = string.Empty;
}

