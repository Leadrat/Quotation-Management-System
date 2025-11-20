using System;

namespace CRM.Application.UserManagement.Exceptions;

public class TaskAssignmentNotFoundException : Exception
{
    public TaskAssignmentNotFoundException(Guid assignmentId) : base($"Task assignment not found: {assignmentId}") { }
}

