using System;
using CRM.Application.Common.Results;

namespace CRM.Application.Quotations.Queries
{
    public class GetAllQuotationsQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public Guid? ClientId { get; set; }
        public Guid? CreatedByUserId { get; set; }
        public string? Status { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public Guid RequestorUserId { get; set; }
        public string RequestorRole { get; set; } = string.Empty;
    }
}

