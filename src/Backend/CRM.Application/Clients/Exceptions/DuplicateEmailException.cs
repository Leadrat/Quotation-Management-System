using System;

namespace CRM.Application.Clients.Exceptions
{
    public class DuplicateEmailException : Exception
    {
        public DuplicateEmailException(string email) : base($"Email already exists: {email}") {}
    }
}
