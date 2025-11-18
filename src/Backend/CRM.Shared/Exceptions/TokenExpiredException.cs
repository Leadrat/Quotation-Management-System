using System;

namespace CRM.Shared.Exceptions;

public class TokenExpiredException : Exception
{
    public TokenExpiredException(string message = "Token has expired") : base(message)
    {
    }
}
