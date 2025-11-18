using System;
using System.Collections.Generic;

namespace CRM.Application.Reports.Dtos
{
    public class PaymentAnalyticsDto
    {
        public decimal CollectionRate { get; set; }
        public int FailedPaymentsCount { get; set; }
        public decimal TotalRefunds { get; set; }
        public List<PaymentMethodDistributionData> PaymentMethodDistribution { get; set; } = new();
        public List<PaymentStatusData> PaymentStatusBreakdown { get; set; } = new();
        public List<RefundData> Refunds { get; set; } = new();
    }

    public class PaymentMethodDistributionData
    {
        public string PaymentMethod { get; set; } = string.Empty; // "Stripe", "Razorpay", etc.
        public int Count { get; set; }
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class PaymentStatusData
    {
        public string Status { get; set; } = string.Empty; // "Success", "Failed", "Pending", "Refunded"
        public int Count { get; set; }
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class RefundData
    {
        public Guid PaymentId { get; set; }
        public Guid QuotationId { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTimeOffset RefundDate { get; set; }
    }
}

