# Spec-014: Payment Processing & Integration - Data Model

## Database Schema

### Payments Table

```sql
CREATE TABLE "Payments" (
    "PaymentId" UUID PRIMARY KEY NOT NULL,
    "QuotationId" UUID NOT NULL,
    "PaymentGateway" VARCHAR(50) NOT NULL,
    "PaymentReference" VARCHAR(255) NOT NULL UNIQUE,
    "AmountPaid" DECIMAL(18, 2) NOT NULL,
    "Currency" VARCHAR(3) NOT NULL DEFAULT 'INR',
    "PaymentStatus" INTEGER NOT NULL DEFAULT 0,
    "PaymentDate" TIMESTAMPTZ NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "FailureReason" TEXT NULL,
    "IsRefundable" BOOLEAN NOT NULL DEFAULT TRUE,
    "RefundAmount" DECIMAL(18, 2) NULL,
    "RefundReason" TEXT NULL,
    "RefundDate" TIMESTAMPTZ NULL,
    "Metadata" JSONB NULL,
    CONSTRAINT "FK_Payments_Quotations_QuotationId" FOREIGN KEY ("QuotationId") 
        REFERENCES "Quotations" ("QuotationId") ON DELETE CASCADE
);

CREATE INDEX "IX_Payments_QuotationId" ON "Payments" ("QuotationId");
CREATE INDEX "IX_Payments_PaymentReference" ON "Payments" ("PaymentReference");
CREATE INDEX "IX_Payments_PaymentStatus" ON "Payments" ("PaymentStatus");
CREATE INDEX "IX_Payments_PaymentDate" ON "Payments" ("PaymentDate");
```

### PaymentGatewayConfigs Table

```sql
CREATE TABLE "PaymentGatewayConfigs" (
    "ConfigId" UUID PRIMARY KEY NOT NULL,
    "CompanyId" UUID NOT NULL,
    "GatewayName" VARCHAR(50) NOT NULL,
    "ApiKey" TEXT NOT NULL, -- Encrypted
    "ApiSecret" TEXT NOT NULL, -- Encrypted
    "WebhookSecret" TEXT NULL, -- Encrypted
    "Enabled" BOOLEAN NOT NULL DEFAULT FALSE,
    "IsTestMode" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "CreatedByUserId" UUID NULL,
    "Metadata" JSONB NULL,
    CONSTRAINT "FK_PaymentGatewayConfigs_Companies_CompanyId" FOREIGN KEY ("CompanyId") 
        REFERENCES "Companies" ("CompanyId") ON DELETE CASCADE,
    CONSTRAINT "FK_PaymentGatewayConfigs_Users_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") 
        REFERENCES "Users" ("UserId") ON DELETE SET NULL,
    CONSTRAINT "UQ_PaymentGatewayConfigs_CompanyId_GatewayName" UNIQUE ("CompanyId", "GatewayName")
);

CREATE INDEX "IX_PaymentGatewayConfigs_CompanyId" ON "PaymentGatewayConfigs" ("CompanyId");
CREATE INDEX "IX_PaymentGatewayConfigs_GatewayName" ON "PaymentGatewayConfigs" ("GatewayName");
CREATE INDEX "IX_PaymentGatewayConfigs_Enabled" ON "PaymentGatewayConfigs" ("Enabled");
```

## Entity Relationships

```
Quotations (1) ──→ (N) Payments
Companies (1) ──→ (N) PaymentGatewayConfigs
Users (1) ──→ (N) PaymentGatewayConfigs (CreatedBy)
```

## C# Domain Entities

### PaymentStatus Enum

```csharp
namespace CRM.Domain.Enums
{
    public enum PaymentStatus
    {
        Pending = 0,
        Processing = 1,
        Success = 2,
        Failed = 3,
        Refunded = 4,
        PartiallyRefunded = 5,
        Cancelled = 6
    }
}
```

### PaymentGateway Enum

```csharp
namespace CRM.Domain.Enums
{
    public enum PaymentGateway
    {
        Stripe = 0,
        Razorpay = 1,
        PayPal = 2,
        Custom = 99
    }
}
```

### Payment Entity

```csharp
namespace CRM.Domain.Entities
{
    [Table("Payments")]
    public class Payment
    {
        public Guid PaymentId { get; set; }
        public Guid QuotationId { get; set; }
        public string PaymentGateway { get; set; } = string.Empty;
        public string PaymentReference { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
        public string Currency { get; set; } = "INR";
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        public DateTimeOffset? PaymentDate { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public string? FailureReason { get; set; }
        public bool IsRefundable { get; set; } = true;
        public decimal? RefundAmount { get; set; }
        public string? RefundReason { get; set; }
        public DateTimeOffset? RefundDate { get; set; }
        public string? Metadata { get; set; } // JSON string for additional data

        // Navigation properties
        public virtual Quotation Quotation { get; set; } = null!;

        // Domain methods
        public void MarkAsSuccess(DateTimeOffset paymentDate)
        {
            PaymentStatus = PaymentStatus.Success;
            PaymentDate = paymentDate;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void MarkAsFailed(string reason)
        {
            PaymentStatus = PaymentStatus.Failed;
            FailureReason = reason;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void ProcessRefund(decimal amount, string reason)
        {
            if (!IsRefundable)
                throw new InvalidOperationException("Payment is not refundable");

            if (amount > AmountPaid)
                throw new InvalidOperationException("Refund amount cannot exceed payment amount");

            if (RefundAmount.HasValue && RefundAmount.Value + amount > AmountPaid)
                throw new InvalidOperationException("Total refund amount cannot exceed payment amount");

            RefundAmount = (RefundAmount ?? 0) + amount;
            RefundReason = reason;
            RefundDate = DateTimeOffset.UtcNow;

            if (RefundAmount >= AmountPaid)
                PaymentStatus = PaymentStatus.Refunded;
            else
                PaymentStatus = PaymentStatus.PartiallyRefunded;

            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void Cancel()
        {
            if (PaymentStatus != PaymentStatus.Pending && PaymentStatus != PaymentStatus.Processing)
                throw new InvalidOperationException("Only pending or processing payments can be cancelled");

            PaymentStatus = PaymentStatus.Cancelled;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
```

### PaymentGatewayConfig Entity

```csharp
namespace CRM.Domain.Entities
{
    [Table("PaymentGatewayConfigs")]
    public class PaymentGatewayConfig
    {
        public Guid ConfigId { get; set; }
        public Guid CompanyId { get; set; }
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
    }
}
```

## Notes

- `PaymentReference` must be unique to prevent duplicate payment processing
- `Metadata` fields use JSONB in PostgreSQL for flexible storage of gateway-specific data
- `ApiKey`, `ApiSecret`, and `WebhookSecret` should be encrypted at rest
- `PaymentStatus` enum supports the full payment lifecycle
- Foreign key constraints ensure data integrity
- Indexes optimize common query patterns

