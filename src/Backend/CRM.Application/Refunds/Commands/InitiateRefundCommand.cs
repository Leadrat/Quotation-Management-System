using System;
using CRM.Application.Refunds.Dtos;

namespace CRM.Application.Refunds.Commands
{
    public class InitiateRefundCommand
    {
        public CreateRefundRequest Request { get; set; } = null!;
        public Guid RequestedByUserId { get; set; }
    }
}

