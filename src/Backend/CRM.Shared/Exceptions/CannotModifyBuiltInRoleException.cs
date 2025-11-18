using System;

namespace CRM.Shared.Exceptions;

public class CannotModifyBuiltInRoleException : Exception
{
    public CannotModifyBuiltInRoleException(string message = "Cannot modify built-in roles") : base(message) { }
}
