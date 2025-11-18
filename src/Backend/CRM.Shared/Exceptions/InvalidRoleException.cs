using System;

namespace CRM.Shared.Exceptions;

public class InvalidRoleException : Exception
{
    public InvalidRoleException(string message = "Invalid role") : base(message) { }
}
