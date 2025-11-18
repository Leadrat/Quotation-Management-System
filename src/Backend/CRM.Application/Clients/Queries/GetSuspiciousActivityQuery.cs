using System;

namespace CRM.Application.Clients.Queries
{
    public class GetSuspiciousActivityQuery
    {
        public short MinScore { get; set; } = 7;
        public string? Status { get; set; }
        public DateTimeOffset? DateFrom { get; set; }
        public DateTimeOffset? DateTo { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}

