using System;

namespace CRM.Application.Clients.Queries
{
    public class GetAllClientsQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public Guid? CreatedByUserId { get; set; }
        public Guid RequestorUserId { get; set; }
        public string RequestorRole { get; set; } = string.Empty; // "Admin" or "SalesRep"
    }

}
