using System;

namespace CRM.Application.Clients.Queries
{
    public class GetClientHistoryQuery
    {
        public Guid ClientId { get; set; }
        public Guid RequestorUserId { get; set; }
        public string RequestorRole { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public bool IncludeAccessLogs { get; set; }
    }

    public class GetClientTimelineQuery
    {
        public Guid ClientId { get; set; }
        public Guid RequestorUserId { get; set; }
        public string RequestorRole { get; set; } = string.Empty;
    }
}

