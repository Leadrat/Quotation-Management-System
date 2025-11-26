using System;

namespace CRM.Application.Quotations.Commands
{
    public class MarkQuotationAsExpiredCommand
    {
        public Guid QuotationId { get; set; }
        public string? Reason { get; set; }
    }
}

 
