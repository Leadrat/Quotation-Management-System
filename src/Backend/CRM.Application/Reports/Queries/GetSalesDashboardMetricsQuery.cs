using System;

namespace CRM.Application.Reports.Queries
{
    public class GetSalesDashboardMetricsQuery
    {
        public Guid UserId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}

