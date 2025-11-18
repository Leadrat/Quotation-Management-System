using System;

namespace CRM.Application.Refunds.Commands
{
    public class RejectAdjustmentCommand
    {
        public Guid AdjustmentId { get; set; }
        public Guid RejectedByUserId { get; set; }
    }
}

