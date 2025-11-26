using System;

namespace CRM.Application.UserManagement.Exceptions;

public class UnauthorizedTeamOperationException : Exception
{
    public UnauthorizedTeamOperationException(string message) : base(message) { }
}

