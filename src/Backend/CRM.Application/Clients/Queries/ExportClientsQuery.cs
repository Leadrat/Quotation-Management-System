using System;

namespace CRM.Application.Clients.Queries
{
    public class ExportClientsQuery
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
        public int MaxRows { get; set; } = 10000; // hard cap
        public string Format { get; set; } = "csv"; // csv|excel

        // Requestor context
        public Guid RequestorUserId { get; set; }
        public string RequestorRole { get; set; } = string.Empty; // Admin or SalesRep
    }
}
