# Data Model: System Administration & Configuration Console

## Overview

This document defines the database schema for the System Administration & Configuration Console. All tables use UUID primary keys, TIMESTAMPTZ for timestamps, and follow the existing CRM database conventions.

## Tables

### 1. SystemSettings

Stores system-wide configuration settings as key-value pairs with JSONB values for flexibility.

```sql
CREATE TABLE "SystemSettings" (
    "Key" VARCHAR(255) NOT NULL PRIMARY KEY,
    "Value" JSONB NOT NULL,
    "LastModifiedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "LastModifiedBy" UUID NOT NULL,
    CONSTRAINT "FK_SystemSettings_Users_LastModifiedBy" 
        FOREIGN KEY ("LastModifiedBy") REFERENCES "Users"("UserId") ON DELETE RESTRICT
);

CREATE INDEX "IX_SystemSettings_LastModifiedAt" ON "SystemSettings"("LastModifiedAt");
```

**Columns**:
- `Key` (VARCHAR(255), PK): Setting key identifier (e.g., "CompanyName", "DateFormat", "DefaultCurrency")
- `Value` (JSONB): Setting value (flexible structure, can be string, number, boolean, object, array)
- `LastModifiedAt` (TIMESTAMPTZ): Timestamp of last modification
- `LastModifiedBy` (UUID, FK to Users): User who last modified the setting

**Example Values**:
- Key: "CompanyName", Value: `"Acme Corporation"`
- Key: "DateFormat", Value: `"dd/MM/yyyy"`
- Key: "NotificationSettings", Value: `{"emailEnabled": true, "smsEnabled": false}`

**Indexes**:
- Primary key on `Key`
- Index on `LastModifiedAt` for audit queries

---

### 2. IntegrationKeys

Stores encrypted API keys and credentials for third-party services.

```sql
CREATE TABLE "IntegrationKeys" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "KeyName" VARCHAR(255) NOT NULL,
    "KeyValueEncrypted" TEXT NOT NULL,
    "Provider" VARCHAR(100) NOT NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "LastUsedAt" TIMESTAMPTZ NULL,
    CONSTRAINT "UQ_IntegrationKeys_KeyName" UNIQUE ("KeyName")
);

CREATE INDEX "IX_IntegrationKeys_Provider" ON "IntegrationKeys"("Provider");
CREATE INDEX "IX_IntegrationKeys_CreatedAt" ON "IntegrationKeys"("CreatedAt");
```

**Columns**:
- `Id` (UUID, PK): Primary key
- `KeyName` (VARCHAR(255), UNIQUE): Human-readable name (e.g., "Stripe Production Key", "SendGrid API Key")
- `KeyValueEncrypted` (TEXT): Encrypted key value (AES-256-GCM encrypted, base64 encoded)
- `Provider` (VARCHAR(100)): Service provider (e.g., "Stripe", "Razorpay", "SendGrid", "Twilio")
- `CreatedAt` (TIMESTAMPTZ): Creation timestamp
- `UpdatedAt` (TIMESTAMPTZ): Last update timestamp
- `LastUsedAt` (TIMESTAMPTZ, NULLABLE): Last time the key was used (updated by service when key is accessed)

**Indexes**:
- Primary key on `Id`
- Unique constraint on `KeyName`
- Index on `Provider` for filtering
- Index on `CreatedAt` for sorting

**Security Notes**:
- `KeyValueEncrypted` contains AES-256-GCM encrypted data
- Encryption key stored in environment variable or key vault
- Decryption only performed when explicitly requested (via "Show Key" action)

---

### 3. AuditLog

Immutable log of all system and admin actions for security and compliance.

```sql
CREATE TABLE "AuditLog" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "ActionType" VARCHAR(100) NOT NULL,
    "Entity" VARCHAR(100) NOT NULL,
    "EntityId" UUID NULL,
    "PerformedBy" UUID NOT NULL,
    "IpAddress" VARCHAR(45) NULL,
    "Timestamp" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "Changes" JSONB NULL,
    CONSTRAINT "FK_AuditLog_Users_PerformedBy" 
        FOREIGN KEY ("PerformedBy") REFERENCES "Users"("UserId") ON DELETE RESTRICT
);

CREATE INDEX "IX_AuditLog_PerformedBy" ON "AuditLog"("PerformedBy");
CREATE INDEX "IX_AuditLog_Timestamp" ON "AuditLog"("Timestamp" DESC);
CREATE INDEX "IX_AuditLog_Entity" ON "AuditLog"("Entity");
CREATE INDEX "IX_AuditLog_ActionType" ON "AuditLog"("ActionType");
CREATE INDEX "IX_AuditLog_EntityId" ON "AuditLog"("EntityId") WHERE "EntityId" IS NOT NULL;
```

**Columns**:
- `Id` (UUID, PK): Primary key
- `ActionType` (VARCHAR(100)): Type of action (e.g., "SettingsUpdated", "IntegrationKeyCreated", "BrandingChanged", "RetentionPolicyUpdated")
- `Entity` (VARCHAR(100)): Entity type affected (e.g., "SystemSettings", "IntegrationKey", "CustomBranding")
- `EntityId` (UUID, NULLABLE): ID of affected entity (if applicable)
- `PerformedBy` (UUID, FK to Users): User who performed the action
- `IpAddress` (VARCHAR(45), NULLABLE): IP address of the user (supports IPv4 and IPv6)
- `Timestamp` (TIMESTAMPTZ): When the action occurred
- `Changes` (JSONB, NULLABLE): Before/after values or change description

**Example Changes JSONB**:
```json
{
  "before": {
    "CompanyName": "Old Company"
  },
  "after": {
    "CompanyName": "New Company"
  }
}
```

**Indexes**:
- Primary key on `Id`
- Index on `PerformedBy` for filtering by user
- Index on `Timestamp` (DESC) for chronological queries
- Index on `Entity` for filtering by entity type
- Index on `ActionType` for filtering by action
- Partial index on `EntityId` (only non-null values)

**Security Notes**:
- Table is append-only (no UPDATE or DELETE operations)
- Sensitive data (passwords, keys) masked in `Changes` JSONB
- Entries older than retention period can be archived to separate table

---

### 4. CustomBranding

Stores company-specific branding configuration (logo, colors, footer).

```sql
CREATE TABLE "CustomBranding" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "LogoUrl" VARCHAR(500) NULL,
    "PrimaryColor" VARCHAR(7) NOT NULL DEFAULT '#FFFFFF',
    "SecondaryColor" VARCHAR(7) NOT NULL DEFAULT '#10B981',
    "AccentColor" VARCHAR(7) NULL,
    "FooterHtml" TEXT NULL,
    "UpdatedBy" UUID NOT NULL,
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "FK_CustomBranding_Users_UpdatedBy" 
        FOREIGN KEY ("UpdatedBy") REFERENCES "Users"("UserId") ON DELETE RESTRICT
);
```

**Columns**:
- `Id` (UUID, PK): Primary key
- `LogoUrl` (VARCHAR(500), NULLABLE): URL to uploaded logo file
- `PrimaryColor` (VARCHAR(7)): Primary theme color (hex format, e.g., "#FFFFFF")
- `SecondaryColor` (VARCHAR(7)): Secondary theme color (hex format, e.g., "#10B981")
- `AccentColor` (VARCHAR(7), NULLABLE): Accent color (hex format)
- `FooterHtml` (TEXT, NULLABLE): Custom footer HTML (sanitized)
- `UpdatedBy` (UUID, FK to Users): User who last updated branding
- `UpdatedAt` (TIMESTAMPTZ): Last update timestamp

**Constraints**:
- Color format validation: Must match hex color pattern `^#[0-9A-Fa-f]{6}$`
- FooterHtml sanitized before storage (XSS prevention)

**Notes**:
- Only one branding configuration per organization (can add CompanyId FK for multi-tenant)
- LogoUrl points to file stored in `wwwroot/uploads/branding/` or S3

---

### 5. DataRetentionPolicy

Stores data retention policies for different entity types.

```sql
CREATE TABLE "DataRetentionPolicy" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "EntityType" VARCHAR(100) NOT NULL,
    "RetentionPeriodMonths" INTEGER NOT NULL CHECK ("RetentionPeriodMonths" > 0),
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "AutoPurgeEnabled" BOOLEAN NOT NULL DEFAULT false,
    "CreatedBy" UUID NOT NULL,
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "FK_DataRetentionPolicy_Users_CreatedBy" 
        FOREIGN KEY ("CreatedBy") REFERENCES "Users"("UserId") ON DELETE RESTRICT,
    CONSTRAINT "UQ_DataRetentionPolicy_EntityType" UNIQUE ("EntityType")
);

CREATE INDEX "IX_DataRetentionPolicy_IsActive" ON "DataRetentionPolicy"("IsActive") WHERE "IsActive" = true;
```

**Columns**:
- `Id` (UUID, PK): Primary key
- `EntityType` (VARCHAR(100), UNIQUE): Type of entity (e.g., "Quotation", "Payment", "AuditLog", "Client")
- `RetentionPeriodMonths` (INTEGER): Retention period in months (must be > 0)
- `IsActive` (BOOLEAN): Whether the policy is active
- `AutoPurgeEnabled` (BOOLEAN): Whether automatic purging is enabled (requires explicit confirmation)
- `CreatedBy` (UUID, FK to Users): User who created the policy
- `UpdatedAt` (TIMESTAMPTZ): Last update timestamp

**Constraints**:
- Unique constraint on `EntityType` (one policy per entity type)
- Check constraint: `RetentionPeriodMonths > 0`

**Indexes**:
- Primary key on `Id`
- Unique constraint on `EntityType`
- Partial index on `IsActive` (only active policies) for background job queries

**Notes**:
- Background job queries active policies and purges data exceeding retention period
- Purging actions logged to AuditLog
- Soft delete preferred over hard delete (set `DeletedAt` instead of DELETE)

---

### 6. NotificationSettings (Global System Messages)

Stores global banner message settings.

```sql
CREATE TABLE "NotificationSettings" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "Message" TEXT NOT NULL,
    "MessageType" VARCHAR(20) NOT NULL CHECK ("MessageType" IN ('info', 'warning', 'error')),
    "IsVisible" BOOLEAN NOT NULL DEFAULT false,
    "UpdatedBy" UUID NOT NULL,
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "FK_NotificationSettings_Users_UpdatedBy" 
        FOREIGN KEY ("UpdatedBy") REFERENCES "Users"("UserId") ON DELETE RESTRICT
);
```

**Columns**:
- `Id` (UUID, PK): Primary key
- `Message` (TEXT): Banner message text (sanitized HTML allowed)
- `MessageType` (VARCHAR(20)): Message type - 'info', 'warning', or 'error'
- `IsVisible` (BOOLEAN): Whether the banner is currently visible to all users
- `UpdatedBy` (UUID, FK to Users): User who last updated the settings
- `UpdatedAt` (TIMESTAMPTZ): Last update timestamp

**Constraints**:
- Check constraint: `MessageType IN ('info', 'warning', 'error')`
- Only one active banner at a time (can be enforced via application logic or unique constraint on `IsVisible = true`)

**Notes**:
- Message can contain sanitized HTML (same sanitization rules as FooterHtml)
- When `IsVisible = true`, banner appears at top of all pages for all authenticated users
- Toggling visibility updates `IsVisible` and logs to AuditLog

---

## Relationships

```
Users (1) ──< (N) SystemSettings.LastModifiedBy
Users (1) ──< (N) AuditLog.PerformedBy
Users (1) ──< (N) CustomBranding.UpdatedBy
Users (1) ──< (N) DataRetentionPolicy.CreatedBy
Users (1) ──< (N) NotificationSettings.UpdatedBy
```

## Seed Data

### SystemSettings (Default Values)

```sql
INSERT INTO "SystemSettings" ("Key", "Value", "LastModifiedAt", "LastModifiedBy")
VALUES 
    ('CompanyName', '"CRM Quotation Management System"', NOW(), (SELECT "UserId" FROM "Users" WHERE "Email" = 'admin@example.com' LIMIT 1)),
    ('DateFormat', '"dd/MM/yyyy"', NOW(), (SELECT "UserId" FROM "Users" WHERE "Email" = 'admin@example.com' LIMIT 1)),
    ('TimeFormat', '"24h"', NOW(), (SELECT "UserId" FROM "Users" WHERE "Email" = 'admin@example.com' LIMIT 1)),
    ('DefaultCurrency', '"INR"', NOW(), (SELECT "UserId" FROM "Users" WHERE "Email" = 'admin@example.com' LIMIT 1)),
    ('NotificationSettings', '{"emailEnabled": true, "smsEnabled": false}', NOW(), (SELECT "UserId" FROM "Users" WHERE "Email" = 'admin@example.com' LIMIT 1));
```

### CustomBranding (Default Values)

```sql
INSERT INTO "CustomBranding" ("Id", "PrimaryColor", "SecondaryColor", "AccentColor", "UpdatedBy", "UpdatedAt")
VALUES 
    (gen_random_uuid(), '#FFFFFF', '#10B981', NULL, (SELECT "UserId" FROM "Users" WHERE "Email" = 'admin@example.com' LIMIT 1), NOW());
```

### DataRetentionPolicy (Default Values)

```sql
INSERT INTO "DataRetentionPolicy" ("Id", "EntityType", "RetentionPeriodMonths", "IsActive", "AutoPurgeEnabled", "CreatedBy", "UpdatedAt")
VALUES 
    (gen_random_uuid(), 'Quotation', 24, true, false, (SELECT "UserId" FROM "Users" WHERE "Email" = 'admin@example.com' LIMIT 1), NOW()),
    (gen_random_uuid(), 'Payment', 36, true, false, (SELECT "UserId" FROM "Users" WHERE "Email" = 'admin@example.com' LIMIT 1), NOW()),
    (gen_random_uuid(), 'AuditLog', 60, true, false, (SELECT "UserId" FROM "Users" WHERE "Email" = 'admin@example.com' LIMIT 1), NOW()),
    (gen_random_uuid(), 'Client', 48, true, false, (SELECT "UserId" FROM "Users" WHERE "Email" = 'admin@example.com' LIMIT 1), NOW());
```

## Migration Notes

1. **Create tables in order**: SystemSettings, IntegrationKeys, AuditLog, CustomBranding, DataRetentionPolicy, NotificationSettings
2. **Add foreign keys after Users table exists** (from Spec-001)
3. **Create indexes after table creation** for better performance
4. **Seed default data** after tables are created
5. **Rollback plan**: Drop tables in reverse order, remove indexes first

## Future Enhancements

- **Multi-tenant support**: Add `CompanyId` column to SystemSettings, CustomBranding, NotificationSettings
- **Settings versioning**: Add `Version` column to SystemSettings to track history
- **Integration key versioning**: Support multiple versions of same key for rotation
- **Audit log archival**: Create `AuditLogArchive` table for old entries
- **Branding templates**: Support multiple branding configurations with templates

