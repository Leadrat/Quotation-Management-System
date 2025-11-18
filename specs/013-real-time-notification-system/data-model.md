# Spec-013: Real-Time Notification System - Data Model

## Database Tables

### 1. Notifications

Stores all notifications sent to users across the system.

```sql
CREATE TABLE "Notifications" (
    "NotificationId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "RecipientUserId" UUID NOT NULL REFERENCES "Users"("UserId") ON DELETE CASCADE,
    "RelatedEntityType" VARCHAR(50) NOT NULL, -- 'Quotation', 'Approval', 'ClientResponse', etc.
    "RelatedEntityId" UUID NOT NULL,
    "EventType" VARCHAR(50) NOT NULL, -- 'SENT', 'VIEWED', 'APPROVED', 'REJECTED', 'EXPIRED', 'RESPONSE', etc.
    "Message" VARCHAR(500) NOT NULL,
    "IsRead" BOOLEAN NOT NULL DEFAULT false,
    "IsArchived" BOOLEAN NOT NULL DEFAULT false,
    "DeliveredChannels" VARCHAR(255), -- Comma-separated: 'in-app,email,push'
    "DeliveryStatus" VARCHAR(50) NOT NULL DEFAULT 'SENT', -- 'SENT', 'DELIVERED', 'FAILED'
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ReadAt" TIMESTAMPTZ NULL,
    "ArchivedAt" TIMESTAMPTZ NULL,
    "Meta" JSONB NULL -- Extra context: { "oldStatus": "Draft", "newStatus": "Sent", "discountPercentage": 15.0 }
);

-- Indexes
CREATE INDEX "IX_Notifications_RecipientUserId" ON "Notifications"("RecipientUserId");
CREATE INDEX "IX_Notifications_IsRead" ON "Notifications"("IsRead");
CREATE INDEX "IX_Notifications_IsArchived" ON "Notifications"("IsArchived");
CREATE INDEX "IX_Notifications_RelatedEntity" ON "Notifications"("RelatedEntityType", "RelatedEntityId");
CREATE INDEX "IX_Notifications_DeliveryStatus" ON "Notifications"("DeliveryStatus");
CREATE INDEX "IX_Notifications_CreatedAt" ON "Notifications"("CreatedAt" DESC);
CREATE INDEX "IX_Notifications_Unread" ON "Notifications"("RecipientUserId", "IsRead") WHERE "IsRead" = false;
```

### 2. NotificationPreferences

Stores user preferences for notification channels and event types.

```sql
CREATE TABLE "NotificationPreferences" (
    "UserId" UUID PRIMARY KEY REFERENCES "Users"("UserId") ON DELETE CASCADE,
    "PreferenceData" JSONB NOT NULL DEFAULT '{}',
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- PreferenceData JSONB structure:
-- {
--   "quotationSent": { "inApp": true, "email": true, "push": false, "muted": false },
--   "quotationViewed": { "inApp": true, "email": false, "push": false, "muted": false },
--   "quotationAccepted": { "inApp": true, "email": true, "push": true, "muted": false },
--   "quotationRejected": { "inApp": true, "email": true, "push": true, "muted": false },
--   "approvalNeeded": { "inApp": true, "email": true, "push": true, "muted": false },
--   "approvalApproved": { "inApp": true, "email": true, "push": false, "muted": false },
--   "approvalRejected": { "inApp": true, "email": true, "push": false, "muted": false },
--   "quotationExpiring": { "inApp": true, "email": true, "push": false, "muted": false },
--   "quotationExpired": { "inApp": true, "email": true, "push": false, "muted": false },
--   "clientResponse": { "inApp": true, "email": true, "push": true, "muted": false }
-- }
```

### 3. EmailNotificationLog

Audit log for all email notifications sent, including delivery status.

```sql
CREATE TABLE "EmailNotificationLog" (
    "LogId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "NotificationId" UUID NULL REFERENCES "Notifications"("NotificationId") ON DELETE SET NULL,
    "RecipientEmail" VARCHAR(255) NOT NULL,
    "EventType" VARCHAR(50) NOT NULL,
    "Subject" VARCHAR(255) NOT NULL,
    "SentAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "DeliveredAt" TIMESTAMPTZ NULL,
    "Status" VARCHAR(50) NOT NULL DEFAULT 'SENT', -- 'SENT', 'DELIVERED', 'BOUNCED', 'FAILED', 'OPENED', 'CLICKED'
    "ErrorMsg" TEXT NULL,
    "RetryCount" INTEGER NOT NULL DEFAULT 0,
    "LastRetryAt" TIMESTAMPTZ NULL
);

-- Indexes
CREATE INDEX "IX_EmailNotificationLog_NotificationId" ON "EmailNotificationLog"("NotificationId");
CREATE INDEX "IX_EmailNotificationLog_RecipientEmail" ON "EmailNotificationLog"("RecipientEmail");
CREATE INDEX "IX_EmailNotificationLog_EventType" ON "EmailNotificationLog"("EventType");
CREATE INDEX "IX_EmailNotificationLog_Status" ON "EmailNotificationLog"("Status");
CREATE INDEX "IX_EmailNotificationLog_SentAt" ON "EmailNotificationLog"("SentAt" DESC);
CREATE INDEX "IX_EmailNotificationLog_Failed" ON "EmailNotificationLog"("Status") WHERE "Status" IN ('FAILED', 'BOUNCED');
```

## Entity Relationships

```
Users (1) ──< (N) Notifications
Users (1) ──< (1) NotificationPreferences
Notifications (1) ──< (N) EmailNotificationLog
```

## C# Domain Entities

### Notification Entity

```csharp
namespace CRM.Domain.Entities
{
    [Table("Notifications")]
    public class Notification
    {
        public Guid NotificationId { get; set; }
        public Guid RecipientUserId { get; set; }
        public string RelatedEntityType { get; set; } = string.Empty; // "Quotation", "Approval", etc.
        public Guid RelatedEntityId { get; set; }
        public string EventType { get; set; } = string.Empty; // "SENT", "VIEWED", "APPROVED", etc.
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public bool IsArchived { get; set; } = false;
        public string? DeliveredChannels { get; set; } // "in-app,email,push"
        public string DeliveryStatus { get; set; } = "SENT"; // "SENT", "DELIVERED", "FAILED"
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ReadAt { get; set; }
        public DateTimeOffset? ArchivedAt { get; set; }
        public string? Meta { get; set; } // JSON string for extra context

        // Navigation properties
        public virtual User RecipientUser { get; set; } = null!;
    }
}
```

### NotificationPreference Entity

```csharp
namespace CRM.Domain.Entities
{
    [Table("NotificationPreferences")]
    public class NotificationPreference
    {
        public Guid UserId { get; set; }
        public string PreferenceData { get; set; } = "{}"; // JSON string
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        // Navigation property
        public virtual User User { get; set; } = null!;
    }
}
```

### EmailNotificationLog Entity

```csharp
namespace CRM.Domain.Entities
{
    [Table("EmailNotificationLog")]
    public class EmailNotificationLog
    {
        public Guid LogId { get; set; }
        public Guid? NotificationId { get; set; }
        public string RecipientEmail { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public DateTimeOffset SentAt { get; set; }
        public DateTimeOffset? DeliveredAt { get; set; }
        public string Status { get; set; } = "SENT"; // "SENT", "DELIVERED", "BOUNCED", "FAILED", etc.
        public string? ErrorMsg { get; set; }
        public int RetryCount { get; set; } = 0;
        public DateTimeOffset? LastRetryAt { get; set; }

        // Navigation property
        public virtual Notification? Notification { get; set; }
    }
}
```

## Enums

```csharp
namespace CRM.Domain.Enums
{
    public enum NotificationEventType
    {
        QuotationCreated,
        QuotationSent,
        QuotationViewed,
        QuotationAccepted,
        QuotationRejected,
        ApprovalNeeded,
        ApprovalApproved,
        ApprovalRejected,
        QuotationExpiring,
        QuotationExpired,
        ClientResponse,
        CommentMention
    }

    public enum NotificationDeliveryStatus
    {
        Sent,
        Delivered,
        Failed
    }

    public enum EmailNotificationStatus
    {
        Sent,
        Delivered,
        Bounced,
        Failed,
        Opened,
        Clicked
    }

    public enum NotificationChannel
    {
        InApp,
        Email,
        Push
    }
}
```

