# Data Model: Quotation Entity & CRUD Operations (Spec-009)

**Spec**: Spec-009  
**Last Updated**: 2025-11-15

## Overview

This document defines the database schema, entity relationships, and data constraints for the Quotation management system. The model consists of two main tables: `Quotations` (header) and `QuotationLineItems` (detail lines).

## Entity: Quotations

**Table Name**: `Quotations`  
**Purpose**: Store quotation header information including client reference, status, dates, totals, and metadata.

### Schema Definition

| Column | Data Type | Constraints | Description | Sample |
|--------|-----------|-------------|-------------|--------|
| `QuotationId` | UUID | PRIMARY KEY, NOT NULL | Unique identifier | `A1B2C3D4-E5F6-47G8-H9I0-J1K2L3M4N5O6` |
| `ClientId` | UUID | NOT NULL, FK → Clients.ClientId | Reference to client | `B2C3D4E5-...` |
| `CreatedByUserId` | UUID | NOT NULL, FK → Users.UserId | Sales rep who created | `C3D4E5F6-...` |
| `QuotationNumber` | VARCHAR(50) | UNIQUE, NOT NULL | Human-readable number | `QT-2025-001234` |
| `Status` | VARCHAR(50) | NOT NULL, DEFAULT 'DRAFT' | Current status | `DRAFT`, `SENT`, `VIEWED`, `ACCEPTED`, `REJECTED`, `EXPIRED`, `CANCELLED` |
| `QuotationDate` | DATE | NOT NULL | Date quotation created | `2025-11-15` |
| `ValidUntil` | DATE | NOT NULL | Expiration date | `2025-12-15` |
| `SubTotal` | DECIMAL(12,2) | NOT NULL, DEFAULT 0.00 | Sum of line items | `50000.00` |
| `DiscountAmount` | DECIMAL(12,2) | NOT NULL, DEFAULT 0.00 | Total discount | `5000.00` |
| `DiscountPercentage` | DECIMAL(5,2) | NOT NULL, DEFAULT 0.00 | Discount % (0-100) | `10.00` |
| `TaxAmount` | DECIMAL(12,2) | NOT NULL, DEFAULT 0.00 | Total tax | `9000.00` |
| `CgstAmount` | DECIMAL(12,2) | NULLABLE, DEFAULT 0.00 | Central GST (intra-state) | `4500.00` |
| `SgstAmount` | DECIMAL(12,2) | NULLABLE, DEFAULT 0.00 | State GST (intra-state) | `4500.00` |
| `IgstAmount` | DECIMAL(12,2) | NULLABLE, DEFAULT 0.00 | Integrated GST (inter-state) | `9000.00` |
| `TotalAmount` | DECIMAL(12,2) | NOT NULL | Final total | `54000.00` |
| `Notes` | TEXT | NULLABLE | Terms and conditions | `Payment due within 30 days` |
| `CreatedAt` | TIMESTAMPTZ | NOT NULL | Creation timestamp | `2025-11-15T10:00:00Z` |
| `UpdatedAt` | TIMESTAMPTZ | NOT NULL | Last update timestamp | `2025-11-15T10:30:00Z` |

### Indexes

- **PRIMARY KEY**: `QuotationId`
- **UNIQUE**: `QuotationNumber`
- **INDEX**: `ClientId` (for fast client-based queries)
- **INDEX**: `CreatedByUserId` (for user-based queries)
- **INDEX**: `Status` (for status filtering)
- **INDEX**: `QuotationDate` (for date range queries)
- **INDEX**: `ValidUntil` (for expiration checks)
- **INDEX**: `CreatedAt` (for sorting)
- **COMPOUND INDEX**: `(ClientId, Status)` - Fast queries like "Get all SENT quotations for client X"
- **COMPOUND INDEX**: `(CreatedByUserId, Status, CreatedAt DESC)` - Fast dashboard queries

### Constraints

- `QuotationNumber` must be unique across all quotations
- `Status` must be one of: DRAFT, SENT, VIEWED, ACCEPTED, REJECTED, EXPIRED, CANCELLED
- `DiscountPercentage` must be between 0 and 100
- `DiscountAmount` cannot exceed `SubTotal`
- `ValidUntil` must be after `QuotationDate`
- `TotalAmount` = `SubTotal` - `DiscountAmount` + `TaxAmount`
- `ClientId` is immutable (cannot change after creation)
- `CreatedByUserId` is immutable (cannot change after creation)
- `QuotationNumber` is immutable (cannot change after creation)

### Relationships

- **Many-to-One** with `Clients` (via `ClientId`)
- **Many-to-One** with `Users` (via `CreatedByUserId`)
- **One-to-Many** with `QuotationLineItems` (via `QuotationId`)

## Entity: QuotationLineItems

**Table Name**: `QuotationLineItems`  
**Purpose**: Store individual items/services within a quotation.

### Schema Definition

| Column | Data Type | Constraints | Description | Sample |
|--------|-----------|-------------|-------------|--------|
| `LineItemId` | UUID | PRIMARY KEY, NOT NULL | Unique identifier | `D4E5F6G7-...` |
| `QuotationId` | UUID | NOT NULL, FK → Quotations.QuotationId | Parent quotation | `A1B2C3D4-...` |
| `SequenceNumber` | INT | NOT NULL | Line number (1, 2, 3...) | `1`, `2`, `3` |
| `ItemName` | VARCHAR(255) | NOT NULL | Product/service name | `Cloud Storage 1TB/month` |
| `Description` | TEXT | NULLABLE | Detailed description | `Monthly cloud storage subscription` |
| `Quantity` | DECIMAL(10,2) | NOT NULL | Number of units | `10.00`, `1.50` |
| `UnitRate` | DECIMAL(12,2) | NOT NULL | Price per unit | `1000.00`, `500.50` |
| `Amount` | DECIMAL(12,2) | NOT NULL | Total (Quantity × UnitRate) | `10000.00` |
| `CreatedAt` | TIMESTAMPTZ | NOT NULL | Creation timestamp | `2025-11-15T10:00:00Z` |
| `UpdatedAt` | TIMESTAMPTZ | NOT NULL | Last update timestamp | `2025-11-15T10:30:00Z` |

### Indexes

- **PRIMARY KEY**: `LineItemId`
- **FOREIGN KEY**: `QuotationId` (with ON DELETE CASCADE)
- **INDEX**: `QuotationId` (for fast line item retrieval)
- **COMPOUND INDEX**: `(QuotationId, SequenceNumber)` - For ordered retrieval

### Constraints

- `Quantity` must be > 0
- `UnitRate` must be > 0
- `Amount` = `Quantity` × `UnitRate` (auto-calculated)
- `SequenceNumber` must be unique within a quotation
- `ItemName` must be between 2 and 255 characters
- `Description` max length: 1000 characters

### Relationships

- **Many-to-One** with `Quotations` (via `QuotationId`, CASCADE DELETE)

## Status Enumeration

**Enum Name**: `QuotationStatus`

| Value | Numeric | Description | Editable? | Deletable? |
|-------|---------|-------------|-----------|------------|
| `DRAFT` | 0 | Not yet sent to client | Yes | Yes |
| `SENT` | 1 | Email sent to client | No | No |
| `VIEWED` | 2 | Client opened quotation | No | No |
| `ACCEPTED` | 3 | Client accepted | No | No |
| `REJECTED` | 4 | Client rejected | No | No |
| `EXPIRED` | 5 | ValidUntil date passed | No | No |
| `CANCELLED` | 6 | Sales rep cancelled | No | Yes |

## Tax Calculation Rules

### Intra-State (Same State)
- **Condition**: Client state code == Company state code
- **CGST**: (SubTotal - DiscountAmount) × 9%
- **SGST**: (SubTotal - DiscountAmount) × 9%
- **IGST**: 0
- **Total Tax**: CGST + SGST = (SubTotal - DiscountAmount) × 18%

### Inter-State (Different State)
- **Condition**: Client state code != Company state code
- **CGST**: 0
- **SGST**: 0
- **IGST**: (SubTotal - DiscountAmount) × 18%
- **Total Tax**: IGST = (SubTotal - DiscountAmount) × 18%

### Total Amount Calculation
```
TotalAmount = SubTotal - DiscountAmount + TaxAmount
```

## Example Data

### Quotation (Intra-State)
```json
{
  "quotationId": "A1B2C3D4-E5F6-47G8-H9I0-J1K2L3M4N5O6",
  "clientId": "B2C3D4E5-F6G7-48H9-I0J1-K2L3M4N5O6P7",
  "createdByUserId": "C3D4E5F6-G7H8-49I0-J1K2-L3M4N5O6P7Q8",
  "quotationNumber": "QT-2025-001234",
  "status": "DRAFT",
  "quotationDate": "2025-11-15",
  "validUntil": "2025-12-15",
  "subTotal": 50000.00,
  "discountAmount": 5000.00,
  "discountPercentage": 10.00,
  "taxAmount": 8100.00,
  "cgstAmount": 4050.00,
  "sgstAmount": 4050.00,
  "igstAmount": 0.00,
  "totalAmount": 53100.00,
  "notes": "Payment due within 30 days",
  "createdAt": "2025-11-15T10:00:00Z",
  "updatedAt": "2025-11-15T10:00:00Z"
}
```

### QuotationLineItems
```json
[
  {
    "lineItemId": "D4E5F6G7-H8I9-50J0-K1L2-M3N4O5P6Q7R8",
    "quotationId": "A1B2C3D4-E5F6-47G8-H9I0-J1K2L3M4N5O6",
    "sequenceNumber": 1,
    "itemName": "Cloud Storage 1TB/month",
    "description": "Monthly cloud storage subscription",
    "quantity": 10.00,
    "unitRate": 5000.00,
    "amount": 50000.00,
    "createdAt": "2025-11-15T10:00:00Z",
    "updatedAt": "2025-11-15T10:00:00Z"
  }
]
```

## Validation Rules

### Quotation Validation
- `ClientId` must exist in `Clients` table
- `CreatedByUserId` must exist in `Users` table
- `QuotationDate` cannot be in the future
- `ValidUntil` must be after `QuotationDate`
- `DiscountPercentage` must be between 0 and 100
- `DiscountAmount` cannot exceed `SubTotal`
- At least one line item required

### Line Item Validation
- `ItemName` required, 2-255 characters
- `Quantity` must be > 0, max 9999999.99
- `UnitRate` must be > 0, max 999999.99
- `SequenceNumber` must be unique within quotation
- `Description` max 1000 characters

## Performance Considerations

- Indexes on `ClientId`, `Status`, `CreatedByUserId` for fast filtering
- Compound indexes for common query patterns
- Line items loaded eagerly with quotation (avoid N+1 queries)
- Pagination for large quotation lists (default 10, max 100)
- Tax calculation cached for reporting (future optimization)

