using System;

namespace CRM.Application.Reports.Queries
{
    public class GetTeamPerformanceMetricsQuery
    {
        public Guid? TeamId { get; set; }
        public Guid? UserId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}

