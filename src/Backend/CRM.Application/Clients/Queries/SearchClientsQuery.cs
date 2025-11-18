using System;

namespace CRM.Application.Clients.Queries
{
    public class SearchClientsQuery
    {
        public string? SearchTerm { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? StateCode { get; set; }
        public string? Gstin { get; set; }
        public Guid? CreatedByUserId { get; set; } // internal; API uses userId
        public DateTimeOffset? CreatedDateFrom { get; set; }
        public DateTimeOffset? CreatedDateTo { get; set; }
        public DateTimeOffset? UpdatedDateFrom { get; set; }
        public DateTimeOffset? UpdatedDateTo { get; set; }
        public string SortBy { get; set; } = "CreatedAtDesc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool IncludeDeleted { get; set; } = false;

        // Requestor context
        public Guid RequestorUserId { get; set; }
        public string RequestorRole { get; set; } = string.Empty; // Admin or SalesRep
    }
}
