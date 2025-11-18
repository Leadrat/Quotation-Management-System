using System;
using CRM.Application.Refunds.Dtos;

namespace CRM.Application.Refunds.Commands
{
    public class ApproveRefundCommand
    {
        public Guid RefundId { get; set; }
        public ApproveRefundRequest Request { get; set; } = null!;
        public Guid ApprovedByUserId { get; set; }
    }
}

