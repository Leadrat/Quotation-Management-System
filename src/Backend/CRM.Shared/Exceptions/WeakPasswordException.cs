using System;

namespace CRM.Shared.Exceptions
{
    public class WeakPasswordException : Exception
    {
        public WeakPasswordException() : base("Password does not meet strength requirements") { }
    }
}
