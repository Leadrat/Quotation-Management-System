using System;

namespace CRM.Application.Quotations.Queries
{
    public class GetQuotationResponseQuery
    {
        public Guid QuotationId { get; set; }
        public Guid RequestorUserId { get; set; }
        public string RequestorRole { get; set; } = string.Empty;
    }
}


