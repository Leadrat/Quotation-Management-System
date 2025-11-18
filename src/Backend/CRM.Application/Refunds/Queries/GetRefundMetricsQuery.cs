using System;

namespace CRM.Application.Refunds.Queries
{
    public class GetRefundMetricsQuery
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}

