using System;

namespace CRM.Application.Clients.Exceptions
{
    public class ClientNotFoundException : Exception
    {
        public ClientNotFoundException(Guid clientId) : base($"Client not found: {clientId}") { }
    }
}
