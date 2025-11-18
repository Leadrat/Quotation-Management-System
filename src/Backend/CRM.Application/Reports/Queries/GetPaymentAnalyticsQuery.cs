using System;
using CRM.Domain.Enums;

namespace CRM.Application.Reports.Queries
{
    public class GetPaymentAnalyticsQuery
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public PaymentGateway? Gateway { get; set; }
        public PaymentStatus? Status { get; set; }
    }
}

