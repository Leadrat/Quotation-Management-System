using System;
using System.Collections.Generic;

namespace CRM.Application.Reports.Dtos
{
    public class FinanceDashboardMetricsDto
    {
        public decimal TotalPaymentsReceivedThisMonth { get; set; }
        public decimal PaymentSuccessRate { get; set; }
        public int FailedPaymentsCount { get; set; }
        public decimal TotalRefunds { get; set; }
        public decimal CollectionPercent { get; set; }
        public List<PaymentTrendData> PaymentTrend { get; set; } = new();
        public List<PaymentMethodData> PaymentMethodDistribution { get; set; } = new();
        public List<PaymentFunnelData> PaymentFunnel { get; set; } = new();
        public List<PaymentListData> Payments { get; set; } = new();
    }

    public class PaymentTrendData
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }

    public class PaymentMethodData
    {
        public string PaymentMethod { get; set; } = string.Empty; // "Stripe", "Razorpay", etc.
        public int Count { get; set; }
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class PaymentFunnelData
    {
        public string Stage { get; set; } = string.Empty; // "Quotations", "Accepted", "Paid"
        public int Count { get; set; }
        public decimal Value { get; set; }
    }

    public class PaymentListData
    {
        public Guid PaymentId { get; set; }
        public Guid QuotationId { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string PaymentGateway { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset? PaymentDate { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}

