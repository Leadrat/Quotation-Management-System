using System;

namespace CRM.Application.Quotations.Queries
{
    public class GetQuotationAccessLinkQuery
    {
        public Guid QuotationId { get; set; }
        public Guid RequestorUserId { get; set; }
        public string RequestorRole { get; set; } = string.Empty;
    }
}


