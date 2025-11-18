using System;
using System.Collections.Generic;

namespace CRM.Application.Payments.Services
{
    public class PaymentGatewayRequest
    {
        public Guid PaymentId { get; set; }
        public Guid QuotationId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public string GatewayName { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
        public string? WebhookSecret { get; set; }
        public bool IsTestMode { get; set; } = true;
        public Dictionary<string, string> Metadata { get; set; } = new();
        public string? CustomerEmail { get; set; }
        public string? CustomerName { get; set; }
        public string? Description { get; set; }
    }
}

