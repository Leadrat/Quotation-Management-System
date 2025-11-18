using System;

namespace CRM.Application.Reports.Queries
{
    public class GetClientEngagementMetricsQuery
    {
        public Guid? ClientId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}

