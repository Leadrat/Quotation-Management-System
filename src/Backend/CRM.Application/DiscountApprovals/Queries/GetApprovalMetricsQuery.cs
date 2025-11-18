using CRM.Application.DiscountApprovals.Dtos;

namespace CRM.Application.DiscountApprovals.Queries
{
    public class GetApprovalMetricsQuery
    {
        public DateTimeOffset? DateFrom { get; set; }
        public DateTimeOffset? DateTo { get; set; }
        public Guid? ApproverUserId { get; set; }
    }
}

