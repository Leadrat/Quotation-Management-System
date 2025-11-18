using System;

namespace CRM.Application.Quotations.Queries
{
    public class GetQuotationByAccessTokenQuery
    {
        public Guid QuotationId { get; set; }
        public string AccessToken { get; set; } = string.Empty;
    }
}


