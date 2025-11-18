using CRM.Application.Common.Results;
using CRM.Application.DiscountApprovals.Dtos;

namespace CRM.Application.DiscountApprovals.Queries
{
    public class GetPendingApprovalsQuery
    {
        public Guid? ApproverUserId { get; set; }
        public string? Status { get; set; }
        public decimal? DiscountPercentageMin { get; set; }
        public decimal? DiscountPercentageMax { get; set; }
        public DateTimeOffset? DateFrom { get; set; }
        public DateTimeOffset? DateTo { get; set; }
        public Guid? RequestedByUserId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public Guid RequestorUserId { get; set; }
        public string RequestorRole { get; set; } = string.Empty;
    }
}

