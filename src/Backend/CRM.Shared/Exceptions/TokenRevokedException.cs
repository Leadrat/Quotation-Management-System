using System;

namespace CRM.Shared.Exceptions;

public class TokenRevokedException : Exception
{
    public TokenRevokedException(string message = "Token has been revoked") : base(message)
    {
    }
}
