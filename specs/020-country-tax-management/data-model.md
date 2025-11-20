# Data Model: Multi-Country & Jurisdiction Tax Management (Spec-020)

**Date**: 2025-01-27  
**Spec**: [spec.md](./spec.md)  
**Plan**: [plan.md](./plan.md)  
**Research**: [research.md](./research.md)

## Overview

This document defines the data model for multi-country and multi-jurisdiction tax management. The model supports multiple countries, jurisdictions (hierarchical), tax frameworks (GST, VAT, etc.), tax rates (by jurisdiction and category), product/service categories, and tax calculation audit logging.

## Entity Relationships

```
Country (1) ────── (N) Jurisdiction
Country (1) ────── (1) TaxFramework
Jurisdiction (N) ──── (1) Country
Jurisdiction (1) ──── (N) Jurisdiction (self-ref: ParentJurisdictionId)
Jurisdiction (1) ────── (N) TaxRate
TaxFramework (1) ────── (N) TaxRate
ProductServiceCategory (1) ────── (N) TaxRate
Client (N) ────── (1) Country
Client (N) ────── (1) Jurisdiction
QuotationLineItem (N) ────── (1) ProductServiceCategory
Quotation (1) ────── (N) TaxCalculationLog
```

## Entities

### 1. Country

Represents a country with its tax framework configuration.

**Table Name**: `Countries`

#### Schema Definition

| Column | Data Type | Constraints | Description | Sample |
|--------|-----------|-------------|-------------|--------|
| `CountryId` | UUID | PRIMARY KEY, NOT NULL | Unique identifier | `A1B2C3D4-...` |
| `CountryName` | VARCHAR(100) | NOT NULL, UNIQUE | Country name | `India`, `United Arab Emirates` |
| `CountryCode` | VARCHAR(2) | NOT NULL, UNIQUE | ISO 3166-1 alpha-2 code | `IN`, `AE` |
| `TaxFrameworkType` | VARCHAR(20) | NOT NULL | Framework type enum | `GST`, `VAT` |
| `DefaultCurrency` | VARCHAR(3) | NOT NULL | ISO 4217 currency code | `INR`, `AED` |
| `IsActive` | BOOLEAN | NOT NULL, DEFAULT true | Active flag | `true`, `false` |
| `IsDefault` | BOOLEAN | NOT NULL, DEFAULT false | Company default country | `true`, `false` |
| `CreatedAt` | TIMESTAMPTZ | NOT NULL | Creation timestamp | `2025-01-27T10:00:00Z` |
| `UpdatedAt` | TIMESTAMPTZ | NOT NULL | Last update timestamp | `2025-01-27T10:30:00Z` |
| `DeletedAt` | TIMESTAMPTZ | NULLABLE | Soft delete timestamp | `null` |

#### Indexes

- **PRIMARY KEY**: `CountryId`
- **UNIQUE INDEX**: `CountryCode` (case-insensitive comparison)
- **UNIQUE INDEX**: `CountryName` (case-insensitive comparison)
- **INDEX**: `IsActive` (for active country queries)
- **INDEX**: `IsDefault` (for default country lookup)

#### Constraints

- `CountryCode` must match regex: `^[A-Z]{2}$` (ISO 3166-1 alpha-2)
- `CountryName` must be between 2 and 100 characters
- `DefaultCurrency` must be exactly 3 uppercase letters (ISO 4217)
- Only one country can have `IsDefault = true` (enforced in application logic)
- `DeletedAt IS NULL` for active countries

#### Relationships

- **One-to-Many** with `Jurisdictions` (via `CountryId`, CASCADE DELETE)
- **One-to-One** with `TaxFramework` (via `CountryId`, CASCADE DELETE)
- **One-to-Many** with `Clients` (via `CountryId`, SET NULL on delete)

---

### 2. Jurisdiction

Represents a tax jurisdiction within a country (state, province, emirate, city, etc.).

**Table Name**: `Jurisdictions`

#### Schema Definition

| Column | Data Type | Constraints | Description | Sample |
|--------|-----------|-------------|-------------|--------|
| `JurisdictionId` | UUID | PRIMARY KEY, NOT NULL | Unique identifier | `B2C3D4E5-...` |
| `CountryId` | UUID | NOT NULL, FK → Countries.CountryId | Parent country | `A1B2C3D4-...` |
| `ParentJurisdictionId` | UUID | NULLABLE, FK → Jurisdictions.JurisdictionId | Parent jurisdiction (hierarchy) | `null`, `C3D4E5F6-...` |
| `JurisdictionName` | VARCHAR(100) | NOT NULL | Jurisdiction name | `Maharashtra`, `Dubai` |
| `JurisdictionCode` | VARCHAR(20) | NULLABLE | Jurisdiction code | `27`, `DXB` |
| `JurisdictionType` | VARCHAR(20) | NULLABLE | Type (State, City, Emirate, etc.) | `State`, `City`, `Emirate` |
| `IsActive` | BOOLEAN | NOT NULL, DEFAULT true | Active flag | `true`, `false` |
| `CreatedAt` | TIMESTAMPTZ | NOT NULL | Creation timestamp | `2025-01-27T10:00:00Z` |
| `UpdatedAt` | TIMESTAMPTZ | NOT NULL | Last update timestamp | `2025-01-27T10:30:00Z` |
| `DeletedAt` | TIMESTAMPTZ | NULLABLE | Soft delete timestamp | `null` |

#### Indexes

- **PRIMARY KEY**: `JurisdictionId`
- **FOREIGN KEY**: `CountryId` (with CASCADE DELETE)
- **FOREIGN KEY**: `ParentJurisdictionId` (with SET NULL on delete)
- **INDEX**: `CountryId` (for country → jurisdictions query)
- **INDEX**: `ParentJurisdictionId` (for hierarchy traversal)
- **COMPOSITE UNIQUE INDEX**: `(CountryId, ParentJurisdictionId, JurisdictionCode)` WHERE `JurisdictionCode IS NOT NULL` (for unique codes within parent)
- **INDEX**: `IsActive` (for active jurisdiction queries)

#### Constraints

- `JurisdictionName` must be between 2 and 100 characters
- `JurisdictionCode` must be unique within parent (country or parent jurisdiction) if provided
- Cannot create circular hierarchy (self-referencing validation)
- Maximum hierarchy depth: 3 levels (Country → Jurisdiction → Sub-Jurisdiction)
- `DeletedAt IS NULL` for active jurisdictions

#### Relationships

- **Many-to-One** with `Country` (via `CountryId`, CASCADE DELETE)
- **Many-to-One** with `Jurisdiction` (via `ParentJurisdictionId`, SET NULL on delete - self-reference)
- **One-to-Many** with `Jurisdictions` (via `ParentJurisdictionId`, children jurisdictions)
- **One-to-Many** with `TaxRates` (via `JurisdictionId`, CASCADE DELETE)
- **One-to-Many** with `Clients` (via `JurisdictionId`, SET NULL on delete)

---

### 3. TaxFramework

Represents the tax framework for a country (e.g., GST for India, VAT for UAE).

**Table Name**: `TaxFrameworks`

#### Schema Definition

| Column | Data Type | Constraints | Description | Sample |
|--------|-----------|-------------|-------------|--------|
| `TaxFrameworkId` | UUID | PRIMARY KEY, NOT NULL | Unique identifier | `C3D4E5F6-...` |
| `CountryId` | UUID | NOT NULL, FK → Countries.CountryId | Associated country | `A1B2C3D4-...` |
| `FrameworkName` | VARCHAR(100) | NOT NULL | Framework name | `Goods and Services Tax`, `Value Added Tax` |
| `FrameworkType` | VARCHAR(20) | NOT NULL | Framework type enum | `GST`, `VAT` |
| `Description` | TEXT | NULLABLE | Framework description | `GST framework for India...` |
| `TaxComponents` | JSONB | NOT NULL | Tax component definitions | `[{ "name": "CGST", "code": "CGST", ... }]` |
| `IsActive` | BOOLEAN | NOT NULL, DEFAULT true | Active flag | `true`, `false` |
| `CreatedAt` | TIMESTAMPTZ | NOT NULL | Creation timestamp | `2025-01-27T10:00:00Z` |
| `UpdatedAt` | TIMESTAMPTZ | NOT NULL | Last update timestamp | `2025-01-27T10:30:00Z` |
| `DeletedAt` | TIMESTAMPTZ | NULLABLE | Soft delete timestamp | `null` |

#### TaxComponents JSONB Structure

```json
[
  {
    "name": "CGST",
    "code": "CGST",
    "isCentrallyGoverned": false,
    "description": "Central Goods and Services Tax"
  },
  {
    "name": "SGST",
    "code": "SGST",
    "isCentrallyGoverned": false,
    "description": "State Goods and Services Tax"
  },
  {
    "name": "IGST",
    "code": "IGST",
    "isCentrallyGoverned": true,
    "description": "Integrated Goods and Services Tax"
  }
]
```

**For VAT**:
```json
[
  {
    "name": "VAT",
    "code": "VAT",
    "isCentrallyGoverned": true,
    "description": "Value Added Tax"
  }
]
```

#### Indexes

- **PRIMARY KEY**: `TaxFrameworkId`
- **FOREIGN KEY**: `CountryId` (with CASCADE DELETE)
- **UNIQUE INDEX**: `CountryId` WHERE `DeletedAt IS NULL` (one active framework per country)
- **INDEX**: `FrameworkType` (for framework type queries)
- **INDEX**: `IsActive` (for active framework queries)
- **GIN INDEX**: `TaxComponents` (for JSONB queries)

#### Constraints

- `FrameworkName` must be between 2 and 100 characters
- `FrameworkType` must be valid enum value (`GST`, `VAT`, etc.)
- `TaxComponents` must be valid JSONB array with required fields: `name`, `code`, `isCentrallyGoverned`
- Only one active framework per country (enforced in application logic)
- `DeletedAt IS NULL` for active frameworks

#### Relationships

- **Many-to-One** with `Country` (via `CountryId`, CASCADE DELETE)
- **One-to-Many** with `TaxRates` (via `TaxFrameworkId`, CASCADE DELETE)

---

### 4. TaxRate

Represents a tax rate configuration for a jurisdiction and optionally a product/service category.

**Table Name**: `TaxRates`

#### Schema Definition

| Column | Data Type | Constraints | Description | Sample |
|--------|-----------|-------------|-------------|--------|
| `TaxRateId` | UUID | PRIMARY KEY, NOT NULL | Unique identifier | `D4E5F6G7-...` |
| `JurisdictionId` | UUID | NULLABLE, FK → Jurisdictions.JurisdictionId | Associated jurisdiction | `B2C3D4E5-...`, `null` |
| `TaxFrameworkId` | UUID | NOT NULL, FK → TaxFrameworks.TaxFrameworkId | Associated tax framework | `C3D4E5F6-...` |
| `ProductServiceCategoryId` | UUID | NULLABLE, FK → ProductServiceCategories.CategoryId | Category-specific rate | `E5F6G7H8-...`, `null` |
| `TaxRate` | DECIMAL(5,2) | NOT NULL | Tax rate percentage | `18.00`, `5.00`, `9.00` |
| `EffectiveFrom` | DATE | NOT NULL | Effective start date | `2025-01-01` |
| `EffectiveTo` | DATE | NULLABLE | Effective end date | `null`, `2025-12-31` |
| `IsExempt` | BOOLEAN | NOT NULL, DEFAULT false | Tax exempt flag | `false` |
| `IsZeroRated` | BOOLEAN | NOT NULL, DEFAULT false | Zero-rated flag | `false` |
| `TaxComponents` | JSONB | NOT NULL | Component rates breakdown | `[{ "component": "CGST", "rate": 9.0 }]` |
| `Description` | TEXT | NULLABLE | Rate description | `Standard GST rate` |
| `CreatedAt` | TIMESTAMPTZ | NOT NULL | Creation timestamp | `2025-01-27T10:00:00Z` |
| `UpdatedAt` | TIMESTAMPTZ | NOT NULL | Last update timestamp | `2025-01-27T10:30:00Z` |

#### TaxComponents JSONB Structure

For GST (intra-state):
```json
[
  { "component": "CGST", "rate": 9.0 },
  { "component": "SGST", "rate": 9.0 }
]
```

For GST (inter-state):
```json
[
  { "component": "IGST", "rate": 18.0 }
]
```

For VAT:
```json
[
  { "component": "VAT", "rate": 5.0 }
]
```

#### Indexes

- **PRIMARY KEY**: `TaxRateId`
- **FOREIGN KEY**: `JurisdictionId` (with SET NULL on delete)
- **FOREIGN KEY**: `TaxFrameworkId` (with CASCADE DELETE)
- **FOREIGN KEY**: `ProductServiceCategoryId` (with SET NULL on delete)
- **INDEX**: `JurisdictionId` (for jurisdiction rate queries)
- **INDEX**: `TaxFrameworkId` (for framework rate queries)
- **INDEX**: `ProductServiceCategoryId` (for category rate queries)
- **INDEX**: `EffectiveFrom, EffectiveTo` (for effective date queries)
- **COMPOSITE INDEX**: `(JurisdictionId, ProductServiceCategoryId, EffectiveFrom, EffectiveTo)` (for rate lookup query)

#### Constraints

- `TaxRate` must be between 0.00 and 100.00 (percentage)
- `EffectiveFrom` must be <= `EffectiveTo` if `EffectiveTo IS NOT NULL`
- At least one of `JurisdictionId` or `ProductServiceCategoryId` must be provided (country default uses `JurisdictionId IS NULL`)
- `TaxComponents` must match framework components (validation in application logic)
- Cannot overlap effective dates for same jurisdiction+category (enforced in application logic)

#### Relationships

- **Many-to-One** with `Jurisdiction` (via `JurisdictionId`, SET NULL on delete)
- **Many-to-One** with `TaxFramework` (via `TaxFrameworkId`, CASCADE DELETE)
- **Many-to-One** with `ProductServiceCategory` (via `ProductServiceCategoryId`, SET NULL on delete)

---

### 5. ProductServiceCategory

Represents a product or service category for tax rule assignment.

**Table Name**: `ProductServiceCategories`

#### Schema Definition

| Column | Data Type | Constraints | Description | Sample |
|--------|-----------|-------------|-------------|--------|
| `CategoryId` | UUID | PRIMARY KEY, NOT NULL | Unique identifier | `E5F6G7H8-...` |
| `CategoryName` | VARCHAR(100) | NOT NULL, UNIQUE | Category name | `Services`, `Products`, `Software` |
| `CategoryCode` | VARCHAR(20) | NULLABLE, UNIQUE | Category code | `SRV`, `PROD`, `SW` |
| `Description` | TEXT | NULLABLE | Category description | `Professional services category` |
| `IsActive` | BOOLEAN | NOT NULL, DEFAULT true | Active flag | `true`, `false` |
| `CreatedAt` | TIMESTAMPTZ | NOT NULL | Creation timestamp | `2025-01-27T10:00:00Z` |
| `UpdatedAt` | TIMESTAMPTZ | NOT NULL | Last update timestamp | `2025-01-27T10:30:00Z` |
| `DeletedAt` | TIMESTAMPTZ | NULLABLE | Soft delete timestamp | `null` |

#### Indexes

- **PRIMARY KEY**: `CategoryId`
- **UNIQUE INDEX**: `CategoryName` (case-insensitive comparison)
- **UNIQUE INDEX**: `CategoryCode` WHERE `CategoryCode IS NOT NULL` (case-insensitive comparison)
- **INDEX**: `IsActive` (for active category queries)

#### Constraints

- `CategoryName` must be between 2 and 100 characters
- `CategoryCode` must be unique if provided
- `DeletedAt IS NULL` for active categories

#### Relationships

- **One-to-Many** with `TaxRates` (via `ProductServiceCategoryId`, SET NULL on delete)
- **One-to-Many** with `QuotationLineItems` (via `ProductServiceCategoryId`, SET NULL on delete)

---

### 6. TaxCalculationLog

Represents an audit log entry for tax calculations and configuration changes.

**Table Name**: `TaxCalculationLogs`

#### Schema Definition

| Column | Data Type | Constraints | Description | Sample |
|--------|-----------|-------------|-------------|--------|
| `LogId` | UUID | PRIMARY KEY, NOT NULL | Unique identifier | `F6G7H8I9-...` |
| `QuotationId` | UUID | NULLABLE, FK → Quotations.QuotationId | Associated quotation | `G7H8I9J0-...`, `null` |
| `ActionType` | VARCHAR(20) | NOT NULL | Action type enum | `Calculation`, `ConfigurationChange` |
| `CountryId` | UUID | NULLABLE, FK → Countries.CountryId | Country used in calculation | `A1B2C3D4-...`, `null` |
| `JurisdictionId` | UUID | NULLABLE, FK → Jurisdictions.JurisdictionId | Jurisdiction used | `B2C3D4E5-...`, `null` |
| `CalculationDetails` | JSONB | NOT NULL | Calculation breakdown | `{ "subtotal": 50000, ... }` |
| `ChangedByUserId` | UUID | NOT NULL, FK → Users.UserId | User who triggered action | `H8I9J0K1-...` |
| `ChangedAt` | TIMESTAMPTZ | NOT NULL | Action timestamp | `2025-01-27T10:00:00Z` |

#### CalculationDetails JSONB Structure

For tax calculation:
```json
{
  "subtotal": 50000.00,
  "discountAmount": 5000.00,
  "taxableAmount": 45000.00,
  "taxBreakdown": [
    { "component": "CGST", "rate": 9.0, "amount": 4050.00 },
    { "component": "SGST", "rate": 9.0, "amount": 4050.00 }
  ],
  "totalTax": 8100.00,
  "lineItems": [
    {
      "lineItemId": "...",
      "categoryId": "...",
      "amount": 25000.00,
      "taxAmount": 4500.00
    }
  ]
}
```

For configuration change:
```json
{
  "entityType": "TaxRate",
  "entityId": "...",
  "changeType": "Created",
  "oldValue": null,
  "newValue": {
    "jurisdictionId": "...",
    "taxRate": 18.0,
    "effectiveFrom": "2025-01-01"
  }
}
```

#### Indexes

- **PRIMARY KEY**: `LogId`
- **FOREIGN KEY**: `QuotationId` (with SET NULL on delete)
- **FOREIGN KEY**: `CountryId` (with SET NULL on delete)
- **FOREIGN KEY**: `JurisdictionId` (with SET NULL on delete)
- **FOREIGN KEY**: `ChangedByUserId` (with SET NULL on delete)
- **INDEX**: `QuotationId` (for quotation audit queries)
- **INDEX**: `ChangedAt` (for date range queries)
- **INDEX**: `ActionType` (for action type filtering)
- **INDEX**: `CountryId, JurisdictionId` (for location-based queries)
- **COMPOSITE INDEX**: `(ChangedAt, ActionType)` (for filtered audit queries)

#### Constraints

- `ActionType` must be valid enum value (`Calculation`, `ConfigurationChange`)
- `CalculationDetails` must be valid JSONB
- `ChangedAt` must be current or past timestamp

#### Relationships

- **Many-to-One** with `Quotation` (via `QuotationId`, SET NULL on delete)
- **Many-to-One** with `Country` (via `CountryId`, SET NULL on delete)
- **Many-to-One** with `Jurisdiction` (via `JurisdictionId`, SET NULL on delete)
- **Many-to-One** with `User` (via `ChangedByUserId`, SET NULL on delete)

---

### 7. Modified Entities

#### 7.1. Client (MODIFIED)

**New Fields**:
- `CountryId` (UUID, nullable, FK → Countries.CountryId)
- `JurisdictionId` (UUID, nullable, FK → Jurisdictions.JurisdictionId)

**Existing Fields Retained** (for backward compatibility):
- `State` (string)
- `City` (string)
- `StateCode` (string)
- `PinCode` (string)

**Migration Strategy**:
- New clients: Populate CountryId/JurisdictionId during creation
- Existing clients: Populate from State/StateCode where possible, or leave null (use company default)

#### 7.2. QuotationLineItem (MODIFIED)

**New Fields**:
- `ProductServiceCategoryId` (UUID, nullable, FK → ProductServiceCategories.CategoryId)

**Usage**:
- Category assigned during line item creation/editing
- Used for category-based tax rate lookup

#### 7.3. Quotation (MODIFIED)

**New Fields**:
- `TaxCountryId` (UUID, nullable, FK → Countries.CountryId)
- `TaxJurisdictionId` (UUID, nullable, FK → Jurisdictions.JurisdictionId)
- `TaxFrameworkId` (UUID, nullable, FK → TaxFrameworks.TaxFrameworkId)
- `TaxBreakdown` (JSONB, nullable) - Component-wise tax breakdown

**Existing Fields Retained** (for backward compatibility):
- `TaxAmount` (decimal)
- `CgstAmount` (decimal?)
- `SgstAmount` (decimal?)
- `IgstAmount` (decimal?)

**Migration Strategy**:
- New quotations: Populate new fields with framework-based calculation
- Existing quotations: Retain old fields, new fields remain null (dual-write for GST during transition)

---

## Enumerations

### TaxFrameworkType

```csharp
public enum TaxFrameworkType
{
    GST = 0,    // Goods and Services Tax (India)
    VAT = 1,    // Value Added Tax (UAE, EU, etc.)
    // Future: SalesTax, ExciseTax, etc.
}
```

### TaxCalculationActionType

```csharp
public enum TaxCalculationActionType
{
    Calculation = 0,           // Tax calculation for quotation
    ConfigurationChange = 1    // Tax configuration change
}
```

---

## Database Migrations

### Migration 1: Create Tax Management Tables

1. Create `Countries` table
2. Create `Jurisdictions` table
3. Create `TaxFrameworks` table
4. Create `TaxRates` table
5. Create `ProductServiceCategories` table
6. Create `TaxCalculationLogs` table
7. Add indexes

### Migration 2: Modify Existing Tables

1. Add `CountryId`, `JurisdictionId` to `Clients` table
2. Add `ProductServiceCategoryId` to `QuotationLineItems` table
3. Add `TaxCountryId`, `TaxJurisdictionId`, `TaxFrameworkId`, `TaxBreakdown` to `Quotations` table
4. Add foreign key constraints
5. Add indexes

### Migration 3: Seed Initial Data

1. Seed India country (IN) with GST framework
2. Seed UAE country (AE) with VAT framework
3. Seed initial jurisdictions (e.g., Maharashtra, Karnataka for India; Dubai, Abu Dhabi for UAE)
4. Seed initial tax rates (e.g., 18% GST for India, 5% VAT for UAE)
5. Seed initial product/service categories (Services, Products, Software)

---

## Data Integrity Rules

1. **Country Code Uniqueness**: Country codes must be unique (case-insensitive)
2. **Jurisdiction Code Uniqueness**: Jurisdiction codes must be unique within parent (country or parent jurisdiction)
3. **One Active Framework Per Country**: Only one active tax framework per country at a time
4. **No Circular Jurisdiction Hierarchy**: Jurisdictions cannot reference themselves in parent chain
5. **Tax Rate Effective Date Validation**: Effective dates must not overlap for same jurisdiction+category
6. **Tax Component Consistency**: TaxRate components must match framework components
7. **Soft Delete Integrity**: Deleted countries/jurisdictions cannot be used in new quotations

---

## Validation Rules

### Country
- Country code: 2 uppercase letters (ISO 3166-1 alpha-2)
- Country name: 2-100 characters, unique (case-insensitive)
- Default currency: 3 uppercase letters (ISO 4217)

### Jurisdiction
- Jurisdiction name: 2-100 characters
- Jurisdiction code: Unique within parent if provided
- Maximum hierarchy depth: 3 levels

### Tax Rate
- Tax rate: 0.00-100.00 (percentage)
- Effective dates: `EffectiveFrom <= EffectiveTo` if both provided
- Tax components: Must match framework components

### ProductServiceCategory
- Category name: 2-100 characters, unique (case-insensitive)
- Category code: Unique if provided

---

## Sample Data

### Countries

```json
{
  "countryId": "A1B2C3D4-E5F6-47G8-H9I0-J1K2L3M4N5O6",
  "countryName": "India",
  "countryCode": "IN",
  "taxFrameworkType": "GST",
  "defaultCurrency": "INR",
  "isActive": true,
  "isDefault": true
}

{
  "countryId": "B2C3D4E5-F6G7-48H9-I0J1-K2L3M4N5O6P7",
  "countryName": "United Arab Emirates",
  "countryCode": "AE",
  "taxFrameworkType": "VAT",
  "defaultCurrency": "AED",
  "isActive": true,
  "isDefault": false
}
```

### Jurisdictions

```json
{
  "jurisdictionId": "C3D4E5F6-G7H8-49I0-J1K2-L3M4N5O6P7Q8",
  "countryId": "A1B2C3D4-E5F6-47G8-H9I0-J1K2L3M4N5O6",
  "parentJurisdictionId": null,
  "jurisdictionName": "Maharashtra",
  "jurisdictionCode": "27",
  "jurisdictionType": "State",
  "isActive": true
}

{
  "jurisdictionId": "D4E5F6G7-H8I9-50J0-K1L2-M3N4O5P6Q7R8",
  "countryId": "B2C3D4E5-F6G7-48H9-I0J1-K2L3M4N5O6P7",
  "parentJurisdictionId": null,
  "jurisdictionName": "Dubai",
  "jurisdictionCode": "DXB",
  "jurisdictionType": "Emirate",
  "isActive": true
}
```

### Tax Frameworks

```json
{
  "taxFrameworkId": "E5F6G7H8-I9J0-51K1-L2M3-N4O5P6Q7R8S9",
  "countryId": "A1B2C3D4-E5F6-47G8-H9I0-J1K2L3M4N5O6",
  "frameworkName": "Goods and Services Tax",
  "frameworkType": "GST",
  "taxComponents": [
    { "name": "CGST", "code": "CGST", "isCentrallyGoverned": false },
    { "name": "SGST", "code": "SGST", "isCentrallyGoverned": false },
    { "name": "IGST", "code": "IGST", "isCentrallyGoverned": true }
  ],
  "isActive": true
}

{
  "taxFrameworkId": "F6G7H8I9-J0K1-52L2-M3N4-O5P6Q7R8S9T0",
  "countryId": "B2C3D4E5-F6G7-48H9-I0J1-K2L3M4N5O6P7",
  "frameworkName": "Value Added Tax",
  "frameworkType": "VAT",
  "taxComponents": [
    { "name": "VAT", "code": "VAT", "isCentrallyGoverned": true }
  ],
  "isActive": true
}
```

### Tax Rates

```json
{
  "taxRateId": "G7H8I9J0-K1L2-53M3-N4O5-P6Q7R8S9T0U1",
  "jurisdictionId": "C3D4E5F6-G7H8-49I0-J1K2-L3M4N5O6P7Q8",
  "taxFrameworkId": "E5F6G7H8-I9J0-51K1-L2M3-N4O5P6Q7R8S9",
  "productServiceCategoryId": null,
  "taxRate": 18.00,
  "effectiveFrom": "2025-01-01",
  "effectiveTo": null,
  "isExempt": false,
  "isZeroRated": false,
  "taxComponents": [
    { "component": "CGST", "rate": 9.0 },
    { "component": "SGST", "rate": 9.0 }
  ]
}

{
  "taxRateId": "H8I9J0K1-L2M3-54N4-O5P6-Q7R8S9T0U1V2",
  "jurisdictionId": "D4E5F6G7-H8I9-50J0-K1L2-M3N4O5P6Q7R8",
  "taxFrameworkId": "F6G7H8I9-J0K1-52L2-M3N4-O5P6Q7R8S9T0",
  "productServiceCategoryId": null,
  "taxRate": 5.00,
  "effectiveFrom": "2025-01-01",
  "effectiveTo": null,
  "isExempt": false,
  "isZeroRated": false,
  "taxComponents": [
    { "component": "VAT", "rate": 5.0 }
  ]
}
```

### ProductServiceCategories

```json
{
  "categoryId": "I9J0K1L2-M3N4-55O5-P6Q7-R8S9T0U1V2W3",
  "categoryName": "Services",
  "categoryCode": "SRV",
  "description": "Professional services",
  "isActive": true
}

{
  "categoryId": "J0K1L2M3-N4O5-56P6-Q7R8-S9T0U1V2W3X4",
  "categoryName": "Products",
  "categoryCode": "PROD",
  "description": "Physical products",
  "isActive": true
}
```

---

## References

- [ISO 3166-1 Alpha-2 Country Codes](https://en.wikipedia.org/wiki/ISO_3166-1_alpha-2)
- [ISO 4217 Currency Codes](https://en.wikipedia.org/wiki/ISO_4217)
- [PostgreSQL JSONB Documentation](https://www.postgresql.org/docs/current/datatype-json.html)
- [Research Document](./research.md) for detailed technical decisions

