using System.Collections.Generic;
using CRM.Application.DiscountApprovals.Dtos;

namespace CRM.Application.DiscountApprovals.Commands
{
    public class BulkApproveDiscountApprovalsCommand
    {
        public BulkApproveRequest Request { get; set; } = null!;
        public Guid ApprovedByUserId { get; set; }
    }
}

