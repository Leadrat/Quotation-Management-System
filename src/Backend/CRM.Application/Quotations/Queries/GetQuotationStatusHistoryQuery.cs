using System;

namespace CRM.Application.Quotations.Queries
{
    public class GetQuotationStatusHistoryQuery
    {
        public Guid QuotationId { get; set; }
        public Guid RequestorUserId { get; set; }
        public string RequestorRole { get; set; } = string.Empty;
    }
}


