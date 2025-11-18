using System;
using CRM.Application.Refunds.Dtos;

namespace CRM.Application.Refunds.Commands
{
    public class ApproveAdjustmentCommand
    {
        public Guid AdjustmentId { get; set; }
        public ApproveAdjustmentRequest Request { get; set; } = null!;
        public Guid ApprovedByUserId { get; set; }
    }
}

