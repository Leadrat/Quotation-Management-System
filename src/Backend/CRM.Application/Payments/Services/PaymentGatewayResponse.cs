using System;
using System.Collections.Generic;

namespace CRM.Application.Payments.Services
{
    public class PaymentGatewayResponse
    {
        public bool Success { get; set; }
        public string PaymentReference { get; set; } = string.Empty;
        public string? PaymentUrl { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorCode { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new();
        public DateTimeOffset? ExpiresAt { get; set; }
    }

    public class RefundGatewayResponse
    {
        public bool Success { get; set; }
        public string RefundReference { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorCode { get; set; }
        public DateTimeOffset RefundedAt { get; set; }
    }

    public class PaymentVerificationResponse
    {
        public bool IsValid { get; set; }
        public string PaymentReference { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // "success", "failed", "pending"
        public DateTimeOffset? PaymentDate { get; set; }
        public string? FailureReason { get; set; }
    }

    public class RefundStatusResponse
    {
        public bool Success { get; set; }
        public string RefundReference { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // "pending", "succeeded", "failed", "canceled"
        public decimal RefundAmount { get; set; }
        public DateTimeOffset? RefundedAt { get; set; }
        public string? FailureReason { get; set; }
        public string? ErrorMessage { get; set; }
    }
}

