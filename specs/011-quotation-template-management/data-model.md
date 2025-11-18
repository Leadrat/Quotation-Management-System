# Data Model: Quotation Template Management

**Spec**: Spec-011  
**Last Updated**: 2025-11-15

## Overview

This document defines the database schema for Quotation Template Management, including two main tables: `QuotationTemplates` and `QuotationTemplateLineItems`.

---

## Table: QuotationTemplates

**Purpose**: Store master quotation templates with metadata, versioning, and approval information.

### Columns

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| `TemplateId` | UUID | PRIMARY KEY, NOT NULL | Unique identifier for template |
| `Name` | VARCHAR(100) | NOT NULL | Template name (unique per owner) |
| `Description` | VARCHAR(255) | NULLABLE | Optional description |
| `OwnerUserId` | UUID | NOT NULL, FK -> Users.UserId | User who created template |
| `OwnerRole` | VARCHAR(50) | NOT NULL, DEFAULT 'SalesRep' | Role of owner (for team visibility) |
| `Visibility` | VARCHAR(50) | NOT NULL | 'Public', 'Team', or 'Private' |
| `IsApproved` | BOOLEAN | NOT NULL, DEFAULT false | Whether template is admin-approved |
| `ApprovedByUserId` | UUID | NULLABLE, FK -> Users.UserId | Admin who approved (if approved) |
| `ApprovedAt` | TIMESTAMPTZ | NULLABLE | Timestamp of approval |
| `Version` | INTEGER | NOT NULL, DEFAULT 1 | Current version number |
| `PreviousVersionId` | UUID | NULLABLE, FK -> QuotationTemplates.TemplateId | Link to previous version (for history) |
| `UsageCount` | INTEGER | NOT NULL, DEFAULT 0 | Number of times template applied |
| `LastUsedAt` | TIMESTAMPTZ | NULLABLE | Last time template was applied |
| `CreatedAt` | TIMESTAMPTZ | NOT NULL, DEFAULT NOW() | Creation timestamp |
| `UpdatedAt` | TIMESTAMPTZ | NOT NULL, DEFAULT NOW() | Last update timestamp |
| `DeletedAt` | TIMESTAMPTZ | NULLABLE | Soft delete timestamp (NULL = active) |

### Indexes

```sql
-- Primary key
PRIMARY KEY (TemplateId)

-- Unique constraint: Name must be unique per owner (excluding deleted)
CREATE UNIQUE INDEX IX_QuotationTemplates_Name_Owner_Active 
ON QuotationTemplates (Name, OwnerUserId) 
WHERE DeletedAt IS NULL;

-- Index for owner and visibility queries
CREATE INDEX IX_QuotationTemplates_Owner_Visibility 
ON QuotationTemplates (OwnerUserId, Visibility) 
WHERE DeletedAt IS NULL;

-- Index for approval and visibility queries
CREATE INDEX IX_QuotationTemplates_Approved_Visibility 
ON QuotationTemplates (IsApproved, Visibility) 
WHERE DeletedAt IS NULL;

-- Index for name search
CREATE INDEX IX_QuotationTemplates_Name 
ON QuotationTemplates (Name) 
WHERE DeletedAt IS NULL;

-- Index for sorting by update time
CREATE INDEX IX_QuotationTemplates_UpdatedAt 
ON QuotationTemplates (UpdatedAt DESC) 
WHERE DeletedAt IS NULL;

-- Index for version history lookup
CREATE INDEX IX_QuotationTemplates_PreviousVersion 
ON QuotationTemplates (PreviousVersionId) 
WHERE PreviousVersionId IS NOT NULL;
```

### Foreign Keys

```sql
ALTER TABLE QuotationTemplates
ADD CONSTRAINT FK_QuotationTemplates_OwnerUser
FOREIGN KEY (OwnerUserId) REFERENCES Users(UserId) ON DELETE RESTRICT;

ALTER TABLE QuotationTemplates
ADD CONSTRAINT FK_QuotationTemplates_ApprovedByUser
FOREIGN KEY (ApprovedByUserId) REFERENCES Users(UserId) ON DELETE SET NULL;

ALTER TABLE QuotationTemplates
ADD CONSTRAINT FK_QuotationTemplates_PreviousVersion
FOREIGN KEY (PreviousVersionId) REFERENCES QuotationTemplates(TemplateId) ON DELETE SET NULL;
```

### Constraints

- `Visibility` must be one of: 'Public', 'Team', 'Private'
- `OwnerRole` must be valid role name (e.g., 'SalesRep', 'Admin')
- `Version` must be >= 1
- `UsageCount` must be >= 0
- `Name` cannot be empty (enforced by application, min 3 chars)

---

## Table: QuotationTemplateLineItems

**Purpose**: Store line items for each template (similar to QuotationLineItems but for templates).

### Columns

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| `LineItemId` | UUID | PRIMARY KEY, NOT NULL | Unique identifier for line item |
| `TemplateId` | UUID | NOT NULL, FK -> QuotationTemplates.TemplateId | Parent template |
| `SequenceNumber` | INTEGER | NOT NULL | Order of item in template (1-based) |
| `ItemName` | VARCHAR(255) | NOT NULL | Name of item/service |
| `Description` | TEXT | NULLABLE | Optional description |
| `Quantity` | DECIMAL(10,2) | NOT NULL | Default quantity |
| `UnitRate` | DECIMAL(12,2) | NOT NULL | Default unit rate/price |
| `Amount` | DECIMAL(12,2) | NOT NULL | Calculated: Quantity * UnitRate |
| `CreatedAt` | TIMESTAMPTZ | NOT NULL, DEFAULT NOW() | Creation timestamp |

### Indexes

```sql
-- Primary key
PRIMARY KEY (LineItemId)

-- Index for template lookup
CREATE INDEX IX_QuotationTemplateLineItems_TemplateId 
ON QuotationTemplateLineItems (TemplateId);

-- Index for ordering within template
CREATE INDEX IX_QuotationTemplateLineItems_Template_Sequence 
ON QuotationTemplateLineItems (TemplateId, SequenceNumber);
```

### Foreign Keys

```sql
ALTER TABLE QuotationTemplateLineItems
ADD CONSTRAINT FK_QuotationTemplateLineItems_Template
FOREIGN KEY (TemplateId) REFERENCES QuotationTemplates(TemplateId) ON DELETE CASCADE;
```

### Constraints

- `SequenceNumber` must be >= 1
- `Quantity` must be > 0
- `UnitRate` must be > 0
- `Amount` = Quantity * UnitRate (enforced by application, can be recalculated)

---

## Entity Relationships

```
Users (1) ──< (many) QuotationTemplates (OwnerUserId)
Users (1) ──< (many) QuotationTemplates (ApprovedByUserId)
QuotationTemplates (1) ──< (many) QuotationTemplateLineItems
QuotationTemplates (1) ──< (1) QuotationTemplates (PreviousVersionId) [self-reference]
```

---

## Visibility Rules

### Public Templates
- `Visibility = 'Public'` AND `IsApproved = true`
- Visible to all sales reps and admins
- Can be used by anyone when creating quotations

### Team Templates
- `Visibility = 'Team'` AND `OwnerRole` matches user's role
- Visible to users with same role as owner
- Example: SalesRep templates visible to all SalesReps

### Private Templates
- `Visibility = 'Private'`
- Visible only to owner (OwnerUserId = current user)
- Admins can see all private templates

### Deleted Templates
- `DeletedAt IS NOT NULL`
- Hidden from normal queries
- Only visible in admin restore view or via explicit query

---

## Version History Structure

Version history uses a linked list structure:

```
TemplateId: abc-123 (Version 3, Current)
  └─ PreviousVersionId: def-456 (Version 2)
      └─ PreviousVersionId: ghi-789 (Version 1, Original)
```

To get all versions:
```sql
WITH RECURSIVE version_chain AS (
  SELECT TemplateId, Version, PreviousVersionId, 1 as depth
  FROM QuotationTemplates
  WHERE TemplateId = @CurrentTemplateId
  
  UNION ALL
  
  SELECT t.TemplateId, t.Version, t.PreviousVersionId, vc.depth + 1
  FROM QuotationTemplates t
  INNER JOIN version_chain vc ON t.TemplateId = vc.PreviousVersionId
)
SELECT * FROM version_chain ORDER BY depth DESC;
```

---

## Migration Script

```sql
-- Create QuotationTemplates table
CREATE TABLE QuotationTemplates (
    TemplateId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name VARCHAR(100) NOT NULL,
    Description VARCHAR(255),
    OwnerUserId UUID NOT NULL,
    OwnerRole VARCHAR(50) NOT NULL DEFAULT 'SalesRep',
    Visibility VARCHAR(50) NOT NULL CHECK (Visibility IN ('Public', 'Team', 'Private')),
    IsApproved BOOLEAN NOT NULL DEFAULT false,
    ApprovedByUserId UUID,
    ApprovedAt TIMESTAMPTZ,
    Version INTEGER NOT NULL DEFAULT 1,
    PreviousVersionId UUID,
    UsageCount INTEGER NOT NULL DEFAULT 0,
    LastUsedAt TIMESTAMPTZ,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DeletedAt TIMESTAMPTZ,
    
    CONSTRAINT FK_QuotationTemplates_OwnerUser 
        FOREIGN KEY (OwnerUserId) REFERENCES Users(UserId) ON DELETE RESTRICT,
    CONSTRAINT FK_QuotationTemplates_ApprovedByUser 
        FOREIGN KEY (ApprovedByUserId) REFERENCES Users(UserId) ON DELETE SET NULL,
    CONSTRAINT FK_QuotationTemplates_PreviousVersion 
        FOREIGN KEY (PreviousVersionId) REFERENCES QuotationTemplates(TemplateId) ON DELETE SET NULL
);

-- Create QuotationTemplateLineItems table
CREATE TABLE QuotationTemplateLineItems (
    LineItemId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    TemplateId UUID NOT NULL,
    SequenceNumber INTEGER NOT NULL,
    ItemName VARCHAR(255) NOT NULL,
    Description TEXT,
    Quantity DECIMAL(10,2) NOT NULL CHECK (Quantity > 0),
    UnitRate DECIMAL(12,2) NOT NULL CHECK (UnitRate > 0),
    Amount DECIMAL(12,2) NOT NULL,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT FK_QuotationTemplateLineItems_Template 
        FOREIGN KEY (TemplateId) REFERENCES QuotationTemplates(TemplateId) ON DELETE CASCADE
);

-- Create indexes (as defined above)
-- ... (index creation statements)
```

---

## Sample Data

### Example Template

```sql
-- Insert template
INSERT INTO QuotationTemplates (
    TemplateId, Name, Description, OwnerUserId, OwnerRole, 
    Visibility, IsApproved, Version, UsageCount
) VALUES (
    '550e8400-e29b-41d4-a716-446655440000',
    'Standard Web Development Package',
    'Complete web development service package',
    'user-123-uuid',
    'SalesRep',
    'Public',
    true,
    1,
    0
);

-- Insert line items
INSERT INTO QuotationTemplateLineItems (
    LineItemId, TemplateId, SequenceNumber, ItemName, 
    Description, Quantity, UnitRate, Amount
) VALUES
    ('line-1-uuid', '550e8400-e29b-41d4-a716-446655440000', 1, 
     'Frontend Development', 'React/Next.js frontend', 40, 1500.00, 60000.00),
    ('line-2-uuid', '550e8400-e29b-41d4-a716-446655440000', 2, 
     'Backend Development', 'Node.js/Express API', 30, 2000.00, 60000.00),
    ('line-3-uuid', '550e8400-e29b-41d4-a716-446655440000', 3, 
     'Database Design', 'PostgreSQL schema and migrations', 10, 1800.00, 18000.00);
```

---

## Notes

- Soft delete preserves all data for audit and restore
- Version history allows rollback to previous versions
- Cascade delete on line items when template is deleted
- Unique constraint on Name+Owner ensures no duplicates per user
- Partial indexes (WHERE DeletedAt IS NULL) improve query performance
- Self-referential FK for version history allows linked list traversal

---

**Last Updated**: 2025-11-15

