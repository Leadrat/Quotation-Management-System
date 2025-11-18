using System;

namespace CRM.Application.Payments.Dtos
{
    public class PaymentGatewayConfigDto
    {
        public Guid ConfigId { get; set; }
        public Guid? CompanyId { get; set; }
        public string GatewayName { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public bool IsTestMode { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        // Note: ApiKey, ApiSecret, WebhookSecret are NOT included for security
    }

    public class CreatePaymentGatewayConfigRequest
    {
        public Guid? CompanyId { get; set; }
        public string GatewayName { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
        public string? WebhookSecret { get; set; }
        public bool Enabled { get; set; } = false;
        public bool IsTestMode { get; set; } = true;
    }

    public class UpdatePaymentGatewayConfigRequest
    {
        public string? ApiKey { get; set; }
        public string? ApiSecret { get; set; }
        public string? WebhookSecret { get; set; }
        public bool? Enabled { get; set; }
        public bool? IsTestMode { get; set; }
    }
}

