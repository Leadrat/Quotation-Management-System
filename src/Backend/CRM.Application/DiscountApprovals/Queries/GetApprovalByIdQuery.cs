using CRM.Application.DiscountApprovals.Dtos;

namespace CRM.Application.DiscountApprovals.Queries
{
    public class GetApprovalByIdQuery
    {
        public Guid ApprovalId { get; set; }
        public Guid RequestorUserId { get; set; }
        public string RequestorRole { get; set; } = string.Empty;
    }
}

