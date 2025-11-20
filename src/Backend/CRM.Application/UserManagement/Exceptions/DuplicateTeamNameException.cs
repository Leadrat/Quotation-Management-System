using System;

namespace CRM.Application.UserManagement.Exceptions;

public class DuplicateTeamNameException : Exception
{
    public DuplicateTeamNameException(string name) : base($"Team name already exists: {name}") { }
}

