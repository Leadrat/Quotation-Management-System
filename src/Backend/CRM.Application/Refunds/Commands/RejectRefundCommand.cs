using System;
using CRM.Application.Refunds.Dtos;

namespace CRM.Application.Refunds.Commands
{
    public class RejectRefundCommand
    {
        public Guid RefundId { get; set; }
        public RejectRefundRequest Request { get; set; } = null!;
        public Guid RejectedByUserId { get; set; }
    }
}

