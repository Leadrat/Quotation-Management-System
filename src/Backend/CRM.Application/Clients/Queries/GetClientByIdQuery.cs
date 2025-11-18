using System;

namespace CRM.Application.Clients.Queries
{
    public class GetClientByIdQuery
    {
        public Guid ClientId { get; set; }
        public Guid RequestorUserId { get; set; }
        public string RequestorRole { get; set; } = string.Empty; // Admin or SalesRep
    }
}
