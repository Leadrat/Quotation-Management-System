using System;

namespace CRM.Shared.Exceptions;

public class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException(string message = "Invalid email or password") : base(message)
    {
    }
}
