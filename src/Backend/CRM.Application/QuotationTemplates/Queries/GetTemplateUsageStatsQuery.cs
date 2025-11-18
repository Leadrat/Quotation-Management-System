using System;

namespace CRM.Application.QuotationTemplates.Queries
{
    public class GetTemplateUsageStatsQuery
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid RequestorUserId { get; set; }
    }
}

