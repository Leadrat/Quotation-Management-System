using System;

namespace CRM.Application.UserManagement.Exceptions;

public class TeamNotFoundException : Exception
{
    public TeamNotFoundException(Guid teamId) : base($"Team not found: {teamId}") { }
}

