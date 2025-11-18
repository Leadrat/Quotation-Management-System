using System;
using System.Collections.Generic;

namespace CRM.Application.Payments.Dtos
{
    public class PaymentDashboardDto
    {
        public PaymentSummaryDto Summary { get; set; } = new();
        public List<PaymentDto> RecentPayments { get; set; } = new();
        public List<PaymentStatusCountDto> StatusCounts { get; set; } = new();
    }

    public class PaymentSummaryDto
    {
        public decimal TotalPending { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalRefunded { get; set; }
        public decimal TotalFailed { get; set; }
        public int PendingCount { get; set; }
        public int PaidCount { get; set; }
        public int RefundedCount { get; set; }
        public int FailedCount { get; set; }
    }

    public class PaymentStatusCountDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }
}

