using System;
using CRM.Application.Common.Results;

namespace CRM.Application.Payments.Queries
{
    public class GetPaymentsByUserQuery
    {
        public Guid UserId { get; set; }
        public string? Status { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public Guid? QuotationId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}

