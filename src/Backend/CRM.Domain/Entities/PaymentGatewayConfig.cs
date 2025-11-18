using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Domain.Entities
{
    [Table("PaymentGatewayConfigs")]
    public class PaymentGatewayConfig
    {
        public Guid ConfigId { get; set; }
        public Guid? CompanyId { get; set; } // Nullable until Company entity is created
        public string GatewayName { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty; // Encrypted
        public string ApiSecret { get; set; } = string.Empty; // Encrypted
        public string? WebhookSecret { get; set; } // Encrypted
        public bool Enabled { get; set; } = false;
        public bool IsTestMode { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public Guid? CreatedByUserId { get; set; }
        public string? Metadata { get; set; } // JSON string

        // Navigation properties
        public virtual User? CreatedByUser { get; set; }

        // Domain methods
        public void Enable()
        {
            Enabled = true;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void Disable()
        {
            Enabled = false;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void UpdateCredentials(string apiKey, string apiSecret, string? webhookSecret = null)
        {
            ApiKey = apiKey;
            ApiSecret = apiSecret;
            if (webhookSecret != null)
                WebhookSecret = webhookSecret;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void SwitchToProduction()
        {
            IsTestMode = false;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void SwitchToTestMode()
        {
            IsTestMode = true;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}

