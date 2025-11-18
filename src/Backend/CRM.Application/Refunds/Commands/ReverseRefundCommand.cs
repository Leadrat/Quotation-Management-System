using System;
using CRM.Application.Refunds.Dtos;

namespace CRM.Application.Refunds.Commands
{
    public class ReverseRefundCommand
    {
        public Guid RefundId { get; set; }
        public ReverseRefundRequest Request { get; set; } = null!;
        public Guid ReversedByUserId { get; set; }
    }
}

