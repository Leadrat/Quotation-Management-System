using System.Collections.Generic;
using CRM.Application.Quotations.Dtos;

namespace CRM.Application.Quotations.Queries
{
    public class GetQuotationsByClientQuery
    {
        public Guid ClientId { get; set; }
        public Guid RequestorUserId { get; set; }
        public string RequestorRole { get; set; } = string.Empty;
    }
}

