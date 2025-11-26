using System;
using CRM.Application.UserManagement.Requests;

namespace CRM.Application.UserManagement.Commands;

public class UpdateTaskStatusCommand
{
    public Guid AssignmentId { get; set; }
    public UpdateTaskStatusRequest Request { get; set; } = null!;
    public Guid UpdatedByUserId { get; set; }
}

