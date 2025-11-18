using System;

namespace CRM.Shared.Exceptions;

public class InvalidTokenException : Exception
{
    public InvalidTokenException(string message = "Invalid or expired token") : base(message)
    {
    }
}
