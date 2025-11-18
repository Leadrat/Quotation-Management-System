using System;

namespace CRM.Application.Clients.Queries
{
    public class GetSavedSearchesQuery
    {
        public Guid RequestorUserId { get; set; }
        public bool IsAdmin { get; set; }
        public Guid? UserId { get; set; } // admin may specify
    }
}
