using System;

namespace CRM.Shared.Exceptions;

public class CannotDeleteRoleInUseException : Exception
{
    public CannotDeleteRoleInUseException(string message = "Cannot delete role with active users") : base(message) { }
}
