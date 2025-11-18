using System;
using CRM.Application.Quotations.Dtos;

namespace CRM.Application.Quotations.Commands
{
    public class SendQuotationCommand
    {
        public Guid QuotationId { get; set; }
        public Guid RequestedByUserId { get; set; }
        public SendQuotationRequest Request { get; set; } = null!;
    }
}

 