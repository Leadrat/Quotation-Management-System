using System;

namespace CRM.Application.Reports.Queries
{
    public class GetDiscountAnalyticsQuery
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? UserId { get; set; }
        public Guid? TeamId { get; set; }
    }
}

