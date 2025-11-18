using System.Collections.Generic;
using CRM.Application.DiscountApprovals.Dtos;

namespace CRM.Application.DiscountApprovals.Queries
{
    public class GetApprovalTimelineQuery
    {
        public Guid? ApprovalId { get; set; }
        public Guid? QuotationId { get; set; }
    }
}

