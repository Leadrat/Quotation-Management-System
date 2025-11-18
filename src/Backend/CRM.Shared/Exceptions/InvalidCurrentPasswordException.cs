using System;

namespace CRM.Shared.Exceptions
{
    public class InvalidCurrentPasswordException : Exception
    {
        public InvalidCurrentPasswordException() : base("Invalid current password") {}
    }
}
