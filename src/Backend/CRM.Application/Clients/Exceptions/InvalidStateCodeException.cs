using System;

namespace CRM.Application.Clients.Exceptions
{
    public class InvalidStateCodeException : Exception
    {
        public InvalidStateCodeException(string code) : base($"Invalid state code '{code}'") { }
    }
}
