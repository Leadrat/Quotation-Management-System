using System;

namespace CRM.Application.Reports.Queries
{
    public class GetApprovalWorkflowMetricsQuery
    {
        public Guid? ManagerId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}

