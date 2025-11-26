using CRM.Application.UserManagement.Requests;

namespace CRM.Application.UserManagement.Commands;

public class AssignTaskCommand
{
    public AssignTaskRequest Request { get; set; } = null!;
    public Guid AssignedByUserId { get; set; }
    public string RequestorRole { get; set; } = string.Empty;
}

