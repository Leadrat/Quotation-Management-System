# Data Model: Quotation Management (Spec-010)

**Spec**: Spec-010  
**Date**: 2025-11-15

## Database Tables

### QuotationAccessLinks

**Purpose**: Store secure access tokens for clients to view quotations without authentication.

**Table Name**: `QuotationAccessLinks`

| Column | Data Type | Constraints | Description |
|--------|-----------|-------------|-------------|
| AccessLinkId | UUID | PRIMARY KEY, NOT NULL | Unique identifier |
| QuotationId | UUID | NOT NULL, FK → Quotations.QuotationId | Reference to quotation |
| ClientEmail | VARCHAR(255) | NOT NULL | Email address this link was sent to |
| AccessToken | VARCHAR(500) | UNIQUE, NOT NULL | Secure random token (32+ chars) |
| IsActive | BOOLEAN | NOT NULL, DEFAULT true | Link enabled/disabled flag |
| CreatedAt | TIMESTAMPTZ | NOT NULL, DEFAULT CURRENT_TIMESTAMP | When link was created |
| ExpiresAt | TIMESTAMPTZ | NULLABLE | When access link expires (optional) |
| SentAt | TIMESTAMPTZ | NULLABLE | When email was sent to client |
| FirstViewedAt | TIMESTAMPTZ | NULLABLE | When client first opened the link |
| LastViewedAt | TIMESTAMPTZ | NULLABLE | When client last opened the link |
| ViewCount | INT | NOT NULL, DEFAULT 0 | Number of times client viewed quotation |
| IpAddress | VARCHAR(50) | NULLABLE | IP address of last view |

**Indexes:**
- PRIMARY KEY: `AccessLinkId`
- UNIQUE: `AccessToken`
- INDEX: `QuotationId`
- INDEX: `ClientEmail`
- INDEX: `(QuotationId, IsActive)`

**Sample Data:**
```sql
INSERT INTO QuotationAccessLinks VALUES (
  'a1b2c3d4-e5f6-7890-abcd-ef1234567890',  -- AccessLinkId
  'q1w2e3r4-t5y6-u7i8-o9p0-qwertyuiop12',  -- QuotationId
  'john.smith@abccorp.com',                 -- ClientEmail
  'a7f3d9c2e1b5k4m6n8p9q0r2s4t5u6v7w8x9y0z1a2b3c4d5e',  -- AccessToken
  true,                                      -- IsActive
  '2025-11-15T10:30:00Z',                   -- CreatedAt
  '2025-12-15T23:59:59Z',                   -- ExpiresAt
  '2025-11-15T10:30:05Z',                   -- SentAt
  '2025-11-15T14:20:00Z',                   -- FirstViewedAt
  '2025-11-15T16:45:00Z',                   -- LastViewedAt
  3,                                         -- ViewCount
  '192.168.1.100'                           -- IpAddress
);
```

### QuotationStatusHistory

**Purpose**: Immutable log of all quotation status transitions.

**Table Name**: `QuotationStatusHistory`

| Column | Data Type | Constraints | Description |
|--------|-----------|-------------|-------------|
| HistoryId | UUID | PRIMARY KEY, NOT NULL | Unique identifier |
| QuotationId | UUID | NOT NULL, FK → Quotations.QuotationId | Reference to quotation |
| PreviousStatus | VARCHAR(50) | NULLABLE | Status before this change (NULL if first) |
| NewStatus | VARCHAR(50) | NOT NULL | Status after change |
| ChangedByUserId | UUID | NULLABLE, FK → Users.UserId | User who triggered change (NULL if system) |
| Reason | VARCHAR(500) | NULLABLE | Reason for status change |
| ChangedAt | TIMESTAMPTZ | NOT NULL, DEFAULT CURRENT_TIMESTAMP | When change occurred |
| IpAddress | VARCHAR(50) | NULLABLE | IP of user/system making change |

**Indexes:**
- PRIMARY KEY: `HistoryId`
- INDEX: `QuotationId`
- INDEX: `(QuotationId, ChangedAt DESC)`
- INDEX: `ChangedByUserId`

**Sample Data:**
```sql
INSERT INTO QuotationStatusHistory VALUES (
  'h1i2j3k4-l5m6-n7o8-p9q0-rstuvwxyz12',  -- HistoryId
  'q1w2e3r4-t5y6-u7i8-o9p0-qwertyuiop12',  -- QuotationId
  'DRAFT',                                  -- PreviousStatus
  'SENT',                                   -- NewStatus
  'u1v2w3x4-y5z6-a7b8-c9d0-efghijklmnop',  -- ChangedByUserId
  'Sent via email to john.smith@abccorp.com',  -- Reason
  '2025-11-15T10:30:00Z',                   -- ChangedAt
  '10.0.0.1'                                -- IpAddress
);
```

### QuotationResponses

**Purpose**: Store client's response to quotation.

**Table Name**: `QuotationResponses`

| Column | Data Type | Constraints | Description |
|--------|-----------|-------------|-------------|
| ResponseId | UUID | PRIMARY KEY, NOT NULL | Unique identifier |
| QuotationId | UUID | NOT NULL, FK → Quotations.QuotationId, UNIQUE | Reference to quotation (only one response) |
| ResponseType | VARCHAR(50) | NOT NULL | ACCEPTED, REJECTED, NEEDS_MODIFICATION |
| ClientEmail | VARCHAR(255) | NOT NULL | Email of client responding |
| ClientName | VARCHAR(255) | NULLABLE | Name of person approving |
| ResponseMessage | TEXT | NULLABLE, MAX 2000 | Client's message/comment |
| ResponseDate | TIMESTAMPTZ | NOT NULL, DEFAULT CURRENT_TIMESTAMP | When client responded |
| IpAddress | VARCHAR(50) | NULLABLE | Client's IP when responding |
| UserAgent | TEXT | NULLABLE | Client's browser/device info |
| NotifiedAdminAt | TIMESTAMPTZ | NULLABLE | When admin/sales rep was notified |

**Indexes:**
- PRIMARY KEY: `ResponseId`
- UNIQUE: `QuotationId`
- INDEX: `ResponseType`
- INDEX: `ClientEmail`

**Sample Data:**
```sql
INSERT INTO QuotationResponses VALUES (
  'r1s2t3u4-v5w6-x7y8-z9a0-bcdefghijklm',  -- ResponseId
  'q1w2e3r4-t5y6-u7i8-o9p0-qwertyuiop12',  -- QuotationId
  'ACCEPTED',                                -- ResponseType
  'john.smith@abccorp.com',                 -- ClientEmail
  'John Smith',                              -- ClientName
  'Looks good! We can proceed with this proposal.',  -- ResponseMessage
  '2025-11-15T11:00:00Z',                   -- ResponseDate
  '192.168.1.100',                           -- IpAddress
  'Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/120.0.0.0',  -- UserAgent
  '2025-11-15T11:00:30Z'                    -- NotifiedAdminAt
);
```

## C# Entity Classes

### QuotationAccessLink

```csharp
namespace CRM.Domain.Entities
{
    public class QuotationAccessLink
    {
        public Guid AccessLinkId { get; set; }
        public Guid QuotationId { get; set; }
        public string ClientEmail { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ExpiresAt { get; set; }
        public DateTimeOffset? SentAt { get; set; }
        public DateTimeOffset? FirstViewedAt { get; set; }
        public DateTimeOffset? LastViewedAt { get; set; }
        public int ViewCount { get; set; } = 0;
        public string? IpAddress { get; set; }

        // Navigation properties
        public virtual Quotation Quotation { get; set; } = null!;

        public bool IsExpired() => ExpiresAt.HasValue && ExpiresAt.Value < DateTimeOffset.UtcNow;
        public bool IsValid() => IsActive && !IsExpired();
    }
}
```

### QuotationStatusHistory

```csharp
namespace CRM.Domain.Entities
{
    public class QuotationStatusHistory
    {
        public Guid HistoryId { get; set; }
        public Guid QuotationId { get; set; }
        public string? PreviousStatus { get; set; }
        public string NewStatus { get; set; } = string.Empty;
        public Guid? ChangedByUserId { get; set; }
        public string? Reason { get; set; }
        public DateTimeOffset ChangedAt { get; set; }
        public string? IpAddress { get; set; }

        // Navigation properties
        public virtual Quotation Quotation { get; set; } = null!;
        public virtual User? ChangedByUser { get; set; }
    }
}
```

### QuotationResponse

```csharp
namespace CRM.Domain.Entities
{
    public class QuotationResponse
    {
        public Guid ResponseId { get; set; }
        public Guid QuotationId { get; set; }
        public string ResponseType { get; set; } = string.Empty; // ACCEPTED, REJECTED, NEEDS_MODIFICATION
        public string ClientEmail { get; set; } = string.Empty;
        public string? ClientName { get; set; }
        public string? ResponseMessage { get; set; }
        public DateTimeOffset ResponseDate { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTimeOffset? NotifiedAdminAt { get; set; }

        // Navigation properties
        public virtual Quotation Quotation { get; set; } = null!;
    }
}
```

## DTO Models

### QuotationAccessLinkDto

```csharp
namespace CRM.Application.Quotations.Dtos
{
    public class QuotationAccessLinkDto
    {
        public Guid AccessLinkId { get; set; }
        public Guid QuotationId { get; set; }
        public string ClientEmail { get; set; } = string.Empty;
        public string ViewUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ExpiresAt { get; set; }
        public DateTimeOffset? SentAt { get; set; }
        public DateTimeOffset? FirstViewedAt { get; set; }
        public DateTimeOffset? LastViewedAt { get; set; }
        public int ViewCount { get; set; }
        public string? IpAddress { get; set; }
    }
}
```

### QuotationStatusHistoryDto

```csharp
namespace CRM.Application.Quotations.Dtos
{
    public class QuotationStatusHistoryDto
    {
        public Guid HistoryId { get; set; }
        public Guid QuotationId { get; set; }
        public string? PreviousStatus { get; set; }
        public string NewStatus { get; set; } = string.Empty;
        public string? ChangedByUserName { get; set; }
        public string? Reason { get; set; }
        public DateTimeOffset ChangedAt { get; set; }
        public string? IpAddress { get; set; }
    }
}
```

### QuotationResponseDto

```csharp
namespace CRM.Application.Quotations.Dtos
{
    public class QuotationResponseDto
    {
        public Guid ResponseId { get; set; }
        public Guid QuotationId { get; set; }
        public string ResponseType { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string? ClientName { get; set; }
        public string? ResponseMessage { get; set; }
        public DateTimeOffset ResponseDate { get; set; }
        public string? IpAddress { get; set; }
        public DateTimeOffset? NotifiedAdminAt { get; set; }
    }
}
```

### SendQuotationRequest

```csharp
namespace CRM.Application.Quotations.Dtos
{
    public class SendQuotationRequest
    {
        public string RecipientEmail { get; set; } = string.Empty;
        public List<string>? CcEmails { get; set; }
        public List<string>? BccEmails { get; set; }
        public string? CustomMessage { get; set; }
    }
}
```

### SubmitQuotationResponseRequest

```csharp
namespace CRM.Application.Quotations.Dtos
{
    public class SubmitQuotationResponseRequest
    {
        public string ResponseType { get; set; } = string.Empty; // ACCEPTED, REJECTED, NEEDS_MODIFICATION
        public string? ClientName { get; set; }
        public string? ResponseMessage { get; set; }
        public string? ClientEmail { get; set; }
    }
}
```

## Relationships

```
Quotations (1) ──→ (N) QuotationAccessLinks
Quotations (1) ──→ (N) QuotationStatusHistory
Quotations (1) ──→ (1) QuotationResponses
QuotationStatusHistory (N) ──→ (1) Users (ChangedByUser, nullable)
```

## Validation Rules

### QuotationAccessLink
- `AccessToken` must be unique across all links
- `AccessToken` must be at least 32 characters
- `ExpiresAt` must be after `CreatedAt` if provided
- `ClientEmail` must be valid email format

### QuotationStatusHistory
- `NewStatus` must be valid QuotationStatus enum value
- `ChangedAt` is immutable (set on creation only)
- `Reason` max length: 500 characters

### QuotationResponse
- `ResponseType` must be one of: ACCEPTED, REJECTED, NEEDS_MODIFICATION
- `ResponseMessage` max length: 2000 characters
- Only one response per quotation (enforced by UNIQUE constraint)
- `ResponseDate` is immutable (set on creation only)

## Security Considerations

1. **AccessToken Generation**: Must use cryptographically secure random generator (RNGCryptoServiceProvider or similar)
2. **Token Storage**: Never log or expose full token in plain text
3. **Token Validation**: Must check IsActive, ExpiresAt, and match QuotationId
4. **IP Tracking**: Store IP addresses for audit but don't expose in public endpoints
5. **Email Validation**: Validate all email addresses before sending

