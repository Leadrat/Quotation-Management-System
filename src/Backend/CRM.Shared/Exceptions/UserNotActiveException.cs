using System;

namespace CRM.Shared.Exceptions;

public class UserNotActiveException : Exception
{
    public UserNotActiveException(string message = "Account inactive") : base(message)
    {
    }
}
