using System;

namespace CRM.Application.Reports.Queries
{
    public class GetAuditComplianceReportQuery
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? EntityType { get; set; }
        public Guid? UserId { get; set; }
    }
}

