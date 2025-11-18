using System;

namespace CRM.Shared.Exceptions
{
    public class PasswordReuseException : Exception
    {
        public PasswordReuseException() : base("New password cannot be the same as the current password") { }
    }
}
