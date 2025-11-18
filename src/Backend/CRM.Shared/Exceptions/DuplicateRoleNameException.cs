using System;

namespace CRM.Shared.Exceptions;

public class DuplicateRoleNameException : Exception
{
    public DuplicateRoleNameException(string roleName) : base($"Role name '{roleName}' already exists") { }
}
