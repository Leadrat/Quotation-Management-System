# Data Model: Document Template Upload & Conversion

**Spec**: Spec-001  
**Last Updated**: 2025-01-27

## Overview

This document defines the database schema for Document Template Upload & Conversion, extending the existing `QuotationTemplates` table to support file-based templates and adding a new `TemplatePlaceholders` table to track identified placeholders.

---

## Table: QuotationTemplates (Extension)

**Purpose**: Extend existing QuotationTemplates table (from Spec-011) to support file-based templates.

### New Columns

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| `IsFileBased` | BOOLEAN | NOT NULL, DEFAULT false | Indicates if template is file-based (Word document) or structured (line items) |
| `TemplateFilePath` | VARCHAR(500) | NULLABLE | Relative path to Word template file (only for file-based templates) |
| `TemplateType` | VARCHAR(50) | NULLABLE | Template type: 'Quotation' or 'ProformaInvoice' (only for file-based templates) |
| `OriginalFileName` | VARCHAR(255) | NULLABLE | Original uploaded file name (for reference) |
| `FileSizeBytes` | BIGINT | NULLABLE | Size of uploaded file in bytes |
| `ProcessingStatus` | VARCHAR(50) | NULLABLE | Status: 'Pending', 'Processing', 'Completed', 'Failed' (for file-based templates) |
| `ProcessingErrorMessage` | TEXT | NULLABLE | Error message if processing failed |

### Indexes

```sql
-- Index for file-based templates by type
CREATE INDEX IX_QuotationTemplates_IsFileBased_TemplateType
ON QuotationTemplates (IsFileBased, TemplateType)
WHERE DeletedAt IS NULL AND IsFileBased = true;

-- Index for processing status
CREATE INDEX IX_QuotationTemplates_ProcessingStatus
ON QuotationTemplates (ProcessingStatus)
WHERE DeletedAt IS NULL AND IsFileBased = true;
```

### Notes

- `IsFileBased = false`: Existing structured templates (line items, etc.) from Spec-011
- `IsFileBased = true`: New file-based templates (Word documents with placeholders)
- `TemplateFilePath` is relative to file storage root (e.g., `templates/{templateId}/template.docx`)
- `TemplateType` is required when `IsFileBased = true`
- Existing columns (Name, Description, OwnerUserId, Visibility, IsApproved, etc.) apply to both template types

---

## Table: TemplatePlaceholders

**Purpose**: Store identified placeholders in file-based templates for tracking and management.

### Columns

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| `PlaceholderId` | UUID | PRIMARY KEY, NOT NULL | Unique identifier for placeholder |
| `TemplateId` | UUID | NOT NULL, FK -> QuotationTemplates.TemplateId | Reference to template |
| `PlaceholderName` | VARCHAR(100) | NOT NULL | Placeholder name (e.g., "CompanyName", "CustomerAddress") |
| `PlaceholderType` | VARCHAR(50) | NOT NULL | Type: 'Company' (user's company) or 'Customer' (customer company) |
| `OriginalText` | TEXT | NULLABLE | Original text that was replaced (for reference) |
| `PositionInDocument` | INTEGER | NULLABLE | Character position or paragraph index in document |
| `IsManuallyAdded` | BOOLEAN | NOT NULL, DEFAULT false | Whether placeholder was manually added by user (vs auto-identified) |
| `CreatedAt` | TIMESTAMPTZ | NOT NULL, DEFAULT NOW() | Creation timestamp |
| `UpdatedAt` | TIMESTAMPTZ | NOT NULL, DEFAULT NOW() | Last update timestamp |

### Indexes

```sql
-- Primary key
PRIMARY KEY (PlaceholderId)

-- Index for template lookups
CREATE INDEX IX_TemplatePlaceholders_TemplateId
ON TemplatePlaceholders (TemplateId);

-- Index for placeholder type queries
CREATE INDEX IX_TemplatePlaceholders_TemplateId_Type
ON TemplatePlaceholders (TemplateId, PlaceholderType);

-- Composite index for template and name (for uniqueness check)
CREATE UNIQUE INDEX IX_TemplatePlaceholders_TemplateId_PlaceholderName
ON TemplatePlaceholders (TemplateId, PlaceholderName);
```

### Foreign Keys

```sql
ALTER TABLE TemplatePlaceholders
ADD CONSTRAINT FK_TemplatePlaceholders_Template
FOREIGN KEY (TemplateId) REFERENCES QuotationTemplates(TemplateId) 
ON DELETE CASCADE;
```

### Notes

- One placeholder per template per name (enforced by unique index)
- Placeholders are automatically created during document conversion
- Users can manually add/edit placeholders in preview interface
- `OriginalText` helps users understand what was replaced
- `PositionInDocument` can be used for highlighting in preview UI

---

## Relationships

### QuotationTemplates → TemplatePlaceholders

- **Relationship**: One-to-Many
- **Description**: A file-based template can have multiple placeholders
- **Cascade**: DELETE CASCADE (when template is deleted, placeholders are deleted)

### QuotationTemplates → Users (Owner)

- **Relationship**: Many-to-One (existing from Spec-011)
- **Description**: Templates are owned by users (Admin or SalesRep)
- **Cascade**: RESTRICT (cannot delete user with templates)

### QuotationTemplates → Users (ApprovedBy)

- **Relationship**: Many-to-One (existing from Spec-011)
- **Description**: Templates can be approved by Admin users
- **Cascade**: SET NULL (if approver is deleted, approval remains but ApprovedByUserId is null)

---

## State Transitions

### Template Processing Status

```
Pending → Processing → Completed
                    ↓
                  Failed
```

- **Pending**: Document uploaded, awaiting processing
- **Processing**: Conversion in progress
- **Completed**: Successfully converted to template
- **Failed**: Processing failed (error message in ProcessingErrorMessage)

### Template Lifecycle (inherited from Spec-011)

- **Create**: New template created (IsFileBased = true, ProcessingStatus = 'Pending')
- **Process**: Document conversion triggers ProcessingStatus = 'Processing'
- **Complete**: Conversion successful, ProcessingStatus = 'Completed', TemplateFilePath set
- **Update**: User edits placeholders, UpdatedAt updated
- **Approve**: Admin approves (IsApproved = true, ApprovedByUserId set)
- **Soft Delete**: DeletedAt set, template hidden from lists

---

## Validation Rules

### QuotationTemplates (File-Based)

- `IsFileBased = true` requires:
  - `TemplateFilePath` must be set (after processing completes)
  - `TemplateType` must be 'Quotation' or 'ProformaInvoice'
  - `OriginalFileName` should be set (for reference)
- `TemplateFilePath` must be relative path (not absolute)
- `FileSizeBytes` must be > 0 and <= 50MB (52,428,800 bytes)
- `ProcessingStatus` must be one of: 'Pending', 'Processing', 'Completed', 'Failed'

### TemplatePlaceholders

- `PlaceholderName` must match pattern: `^[A-Z][a-zA-Z0-9]+$` (PascalCase, alphanumeric)
- `PlaceholderType` must be 'Company' or 'Customer'
- `PlaceholderName` must be unique per template
- `OriginalText` can be null (for manually added placeholders)

---

## Data Migration

### Migration Script

```sql
-- Add new columns to QuotationTemplates
ALTER TABLE QuotationTemplates
ADD COLUMN IsFileBased BOOLEAN NOT NULL DEFAULT false,
ADD COLUMN TemplateFilePath VARCHAR(500) NULL,
ADD COLUMN TemplateType VARCHAR(50) NULL,
ADD COLUMN OriginalFileName VARCHAR(255) NULL,
ADD COLUMN FileSizeBytes BIGINT NULL,
ADD COLUMN ProcessingStatus VARCHAR(50) NULL,
ADD COLUMN ProcessingErrorMessage TEXT NULL;

-- Create TemplatePlaceholders table
CREATE TABLE TemplatePlaceholders (
    PlaceholderId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    TemplateId UUID NOT NULL,
    PlaceholderName VARCHAR(100) NOT NULL,
    PlaceholderType VARCHAR(50) NOT NULL,
    OriginalText TEXT NULL,
    PositionInDocument INTEGER NULL,
    IsManuallyAdded BOOLEAN NOT NULL DEFAULT false,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UpdatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT FK_TemplatePlaceholders_Template
        FOREIGN KEY (TemplateId) REFERENCES QuotationTemplates(TemplateId) ON DELETE CASCADE
);

-- Create indexes
CREATE INDEX IX_QuotationTemplates_IsFileBased_TemplateType
ON QuotationTemplates (IsFileBased, TemplateType)
WHERE DeletedAt IS NULL AND IsFileBased = true;

CREATE INDEX IX_QuotationTemplates_ProcessingStatus
ON QuotationTemplates (ProcessingStatus)
WHERE DeletedAt IS NULL AND IsFileBased = true;

CREATE INDEX IX_TemplatePlaceholders_TemplateId
ON TemplatePlaceholders (TemplateId);

CREATE INDEX IX_TemplatePlaceholders_TemplateId_Type
ON TemplatePlaceholders (TemplateId, PlaceholderType);

CREATE UNIQUE INDEX IX_TemplatePlaceholders_TemplateId_PlaceholderName
ON TemplatePlaceholders (TemplateId, PlaceholderName);
```

---

## Notes

- File-based templates coexist with structured templates (Spec-011)
- Both template types share the same visibility, approval, and versioning mechanisms
- Template file storage uses existing FileStorageServiceAdapter
- Placeholders are stored in database for tracking, but actual template file contains the placeholders
- When template is updated (placeholders edited), TemplateFilePath may point to new version of file

