using System;
using System.Collections.Generic;

namespace CRM.Application.Clients.Queries
{
    public class ExportClientHistoryQuery
    {
        public List<Guid>? ClientIds { get; set; }
        public string? ActionType { get; set; }
        public DateTimeOffset? DateFrom { get; set; }
        public DateTimeOffset? DateTo { get; set; }
        public Guid RequestorUserId { get; set; }
        public string RequestorRole { get; set; } = string.Empty;
        public int MaxRows { get; set; } = 5000;
    }
}

