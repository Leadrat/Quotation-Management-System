# Data Model: Country-Specific Identifiers & Bank Details (Spec-023)

**Date**: 2025-01-27  
**Spec**: [spec.md](./spec.md)  
**Plan**: [plan.md](./plan.md)  
**Research**: [research.md](./research.md)

## Overview

This document defines the data model for country-specific company identifiers and bank details management. The model supports master configuration of identifier types and bank field types per country, with validation rules and display properties. Company identifier and bank detail values are stored using a hybrid approach: master configuration in normalized relational tables, actual values in JSONB columns for flexibility.

## Entity Relationships

```
IdentifierType (1) ────── (N) CountryIdentifierConfiguration
Country (1) ────── (N) CountryIdentifierConfiguration
CompanyDetails (1) ────── (N) CompanyIdentifierValue (stored as JSONB)

BankFieldType (1) ────── (N) CountryBankFieldConfiguration
Country (1) ────── (N) CountryBankFieldConfiguration
CompanyDetails (1) ────── (N) CompanyBankDetails (FieldValues as JSONB)

Country (1) ────── (1) Country (from Spec-020, referenced by CountryId)
Client (N) ────── (1) Country (from Spec-006, for quotation country filtering)
Quotation (1) ────── (N) Client (from Spec-009, references client country)
```

## Entities

### 1. IdentifierType

Represents a type of company identifier (e.g., PAN, VAT, Business License) that can be configured for different countries.

**Table Name**: `IdentifierTypes`

#### Schema Definition

| Column | Data Type | Constraints | Description | Sample |
|--------|-----------|-------------|-------------|--------|
| `IdentifierTypeId` | UUID | PRIMARY KEY, NOT NULL | Unique identifier | `A1B2C3D4-...` |
| `Name` | VARCHAR(50) | NOT NULL, UNIQUE | Identifier type name (code) | `PAN`, `VAT`, `BUSINESS_LICENSE` |
| `DisplayName` | VARCHAR(100) | NOT NULL | Human-readable name | `PAN Number`, `VAT Number`, `Trade License Number` |
| `Description` | TEXT | NULLABLE | Description of identifier type | `Permanent Account Number for India` |
| `IsActive` | BOOLEAN | NOT NULL, DEFAULT true | Active flag | `true`, `false` |
| `CreatedAt` | TIMESTAMPTZ | NOT NULL | Creation timestamp | `2025-01-27T10:00:00Z` |
| `UpdatedAt` | TIMESTAMPTZ | NOT NULL | Last update timestamp | `2025-01-27T10:30:00Z` |
| `DeletedAt` | TIMESTAMPTZ | NULLABLE | Soft delete timestamp | `null` |

#### Indexes

- **PRIMARY KEY**: `IdentifierTypeId`
- **UNIQUE INDEX**: `Name` (case-insensitive comparison)
- **INDEX**: `IsActive` (for active identifier type queries)

#### Constraints

- `Name` must be between 2 and 50 characters, alphanumeric with underscores
- `DisplayName` must be between 2 and 100 characters
- `DeletedAt IS NULL` for active identifier types

#### Relationships

- **One-to-Many** with `CountryIdentifierConfigurations` (via `IdentifierTypeId`, CASCADE DELETE)

---

### 2. CountryIdentifierConfiguration

Represents the configuration of which identifier types are required/optional for each country, including validation rules and display properties.

**Table Name**: `CountryIdentifierConfigurations`

#### Schema Definition

| Column | Data Type | Constraints | Description | Sample |
|--------|-----------|-------------|-------------|--------|
| `ConfigurationId` | UUID | PRIMARY KEY, NOT NULL | Unique identifier | `B2C3D4E5-...` |
| `CountryId` | UUID | NOT NULL, FK → Countries.CountryId | Country reference | `C3D4E5F6-...` |
| `IdentifierTypeId` | UUID | NOT NULL, FK → IdentifierTypes.IdentifierTypeId | Identifier type reference | `A1B2C3D4-...` |
| `IsRequired` | BOOLEAN | NOT NULL, DEFAULT false | Required flag | `true`, `false` |
| `ValidationRegex` | VARCHAR(500) | NULLABLE | Regex pattern for validation | `^[A-Z]{5}[0-9]{4}[A-Z]{1}$` |
| `MinLength` | INTEGER | NULLABLE | Minimum length | `10` |
| `MaxLength` | INTEGER | NULLABLE | Maximum length | `15` |
| `DisplayName` | VARCHAR(100) | NULLABLE | Override display name for this country | `PAN Number` |
| `HelpText` | TEXT | NULLABLE | Help text for form field | `10-character alphanumeric Permanent Account Number` |
| `DisplayOrder` | INTEGER | NOT NULL, DEFAULT 0 | Display order in form | `1`, `2`, `3` |
| `IsActive` | BOOLEAN | NOT NULL, DEFAULT true | Active flag | `true`, `false` |
| `CreatedAt` | TIMESTAMPTZ | NOT NULL | Creation timestamp | `2025-01-27T10:00:00Z` |
| `UpdatedAt` | TIMESTAMPTZ | NOT NULL | Last update timestamp | `2025-01-27T10:30:00Z` |
| `DeletedAt` | TIMESTAMPTZ | NULLABLE | Soft delete timestamp | `null` |

#### Indexes

- **PRIMARY KEY**: `ConfigurationId`
- **UNIQUE INDEX**: `(CountryId, IdentifierTypeId)` (one configuration per country/identifier type)
- **FOREIGN KEY**: `CountryId` (with CASCADE DELETE)
- **FOREIGN KEY**: `IdentifierTypeId` (with CASCADE DELETE)
- **INDEX**: `CountryId` (for country-specific queries)
- **INDEX**: `IsActive` (for active configuration queries)
- **INDEX**: `(CountryId, IsActive)` (for active configurations per country)

#### Constraints

- `ValidationRegex` must be valid regex pattern (if provided)
- `MinLength` must be >= 0 (if provided)
- `MaxLength` must be > `MinLength` (if both provided)
- `DisplayOrder` must be >= 0
- `DeletedAt IS NULL` for active configurations

#### Relationships

- **Many-to-One** with `Countries` (via `CountryId`, CASCADE DELETE)
- **Many-to-One** with `IdentifierTypes` (via `IdentifierTypeId`, CASCADE DELETE)

---

### 3. CompanyDetails (Modified from Spec-022)

Represents the company's overall details configuration, now with country-specific identifier values stored as JSONB.

**Table Name**: `CompanyDetails`

#### Schema Modification

| Column | Data Type | Constraints | Description | Sample |
|--------|-----------|-------------|-------------|--------|
| ... (existing columns from Spec-022) | ... | ... | ... | ... |
| `IdentifierValues` | JSONB | NULLABLE | Country-specific identifier values | `{ "countryId1": { "PAN": "ABCDE1234F", "TAN": "ABCD12345E" }, "countryId2": { "VAT": "DE123456789" } }` |

**JSONB Structure**:
```json
{
  "countryId1": {
    "PAN": "ABCDE1234F",
    "TAN": "ABCD12345E",
    "GST": "27ABCDE1234F1Z5"
  },
  "countryId2": {
    "VAT": "DE123456789"
  }
}
```

#### Indexes

- **GIN INDEX**: `IdentifierValues` (for efficient JSONB queries)
- **INDEX**: `(IdentifierValues @> '{"countryId1": {}}')` (for country-specific queries)

#### Constraints

- `IdentifierValues` must be valid JSONB (if provided)
- Identifier values must match configured validation rules for their country (enforced in application code)
- Required identifiers for a country must be present (enforced in application code)

---

### 4. BankFieldType

Represents a type of bank field (e.g., IFSC, IBAN, SWIFT, Routing Number) that can be configured for different countries.

**Table Name**: `BankFieldTypes`

#### Schema Definition

| Column | Data Type | Constraints | Description | Sample |
|--------|-----------|-------------|-------------|--------|
| `BankFieldTypeId` | UUID | PRIMARY KEY, NOT NULL | Unique identifier | `D4E5F6G7-...` |
| `Name` | VARCHAR(50) | NOT NULL, UNIQUE | Bank field type name (code) | `IFSC`, `IBAN`, `SWIFT`, `ROUTING_NUMBER` |
| `DisplayName` | VARCHAR(100) | NOT NULL | Human-readable name | `IFSC Code`, `IBAN`, `SWIFT Code`, `Routing Number` |
| `Description` | TEXT | NULLABLE | Description of bank field type | `Indian Financial System Code` |
| `IsActive` | BOOLEAN | NOT NULL, DEFAULT true | Active flag | `true`, `false` |
| `CreatedAt` | TIMESTAMPTZ | NOT NULL | Creation timestamp | `2025-01-27T10:00:00Z` |
| `UpdatedAt` | TIMESTAMPTZ | NOT NULL | Last update timestamp | `2025-01-27T10:30:00Z` |
| `DeletedAt` | TIMESTAMPTZ | NULLABLE | Soft delete timestamp | `null` |

#### Indexes

- **PRIMARY KEY**: `BankFieldTypeId`
- **UNIQUE INDEX**: `Name` (case-insensitive comparison)
- **INDEX**: `IsActive` (for active bank field type queries)

#### Constraints

- `Name` must be between 2 and 50 characters, alphanumeric with underscores
- `DisplayName` must be between 2 and 100 characters
- `DeletedAt IS NULL` for active bank field types

#### Relationships

- **One-to-Many** with `CountryBankFieldConfigurations` (via `BankFieldTypeId`, CASCADE DELETE)

---

### 5. CountryBankFieldConfiguration

Represents the configuration of which bank fields are required/optional for each country, including validation rules and display properties.

**Table Name**: `CountryBankFieldConfigurations`

#### Schema Definition

| Column | Data Type | Constraints | Description | Sample |
|--------|-----------|-------------|-------------|--------|
| `ConfigurationId` | UUID | PRIMARY KEY, NOT NULL | Unique identifier | `E5F6G7H8-...` |
| `CountryId` | UUID | NOT NULL, FK → Countries.CountryId | Country reference | `C3D4E5F6-...` |
| `BankFieldTypeId` | UUID | NOT NULL, FK → BankFieldTypes.BankFieldTypeId | Bank field type reference | `D4E5F6G7-...` |
| `IsRequired` | BOOLEAN | NOT NULL, DEFAULT false | Required flag | `true`, `false` |
| `ValidationRegex` | VARCHAR(500) | NULLABLE | Regex pattern for validation | `^[A-Z]{4}0[0-9A-Z]{6}$` |
| `MinLength` | INTEGER | NULLABLE | Minimum length | `11` |
| `MaxLength` | INTEGER | NULLABLE | Maximum length | `34` |
| `DisplayName` | VARCHAR(100) | NULLABLE | Override display name for this country | `IFSC Code` |
| `HelpText` | TEXT | NULLABLE | Help text for form field | `11-character alphanumeric Indian Financial System Code` |
| `DisplayOrder` | INTEGER | NOT NULL, DEFAULT 0 | Display order in form | `1`, `2`, `3` |
| `IsActive` | BOOLEAN | NOT NULL, DEFAULT true | Active flag | `true`, `false` |
| `CreatedAt` | TIMESTAMPTZ | NOT NULL | Creation timestamp | `2025-01-27T10:00:00Z` |
| `UpdatedAt` | TIMESTAMPTZ | NOT NULL | Last update timestamp | `2025-01-27T10:30:00Z` |
| `DeletedAt` | TIMESTAMPTZ | NULLABLE | Soft delete timestamp | `null` |

#### Indexes

- **PRIMARY KEY**: `ConfigurationId`
- **UNIQUE INDEX**: `(CountryId, BankFieldTypeId)` (one configuration per country/bank field type)
- **FOREIGN KEY**: `CountryId` (with CASCADE DELETE)
- **FOREIGN KEY**: `BankFieldTypeId` (with CASCADE DELETE)
- **INDEX**: `CountryId` (for country-specific queries)
- **INDEX**: `IsActive` (for active configuration queries)
- **INDEX**: `(CountryId, IsActive)` (for active configurations per country)

#### Constraints

- `ValidationRegex` must be valid regex pattern (if provided)
- `MinLength` must be >= 0 (if provided)
- `MaxLength` must be > `MinLength` (if both provided)
- `DisplayOrder` must be >= 0
- `DeletedAt IS NULL` for active configurations

#### Relationships

- **Many-to-One** with `Countries` (via `CountryId`, CASCADE DELETE)
- **Many-to-One** with `BankFieldTypes` (via `BankFieldTypeId`, CASCADE DELETE)

---

### 6. CompanyBankDetails (Modified from Spec-022)

Represents the actual bank detail values assigned to a company for their country, now with country-specific field values stored as JSONB.

**Table Name**: `BankDetails`

#### Schema Modification

| Column | Data Type | Constraints | Description | Sample |
|--------|-----------|-------------|-------------|--------|
| ... (existing columns from Spec-022) | ... | ... | ... | ... |
| `FieldValues` | JSONB | NULLABLE | Country-specific bank field values | `{ "IFSC": "HDFC0001234", "AccountNumber": "1234567890", "BankName": "HDFC Bank", "BranchName": "Mumbai" }` |

**JSONB Structure**:
```json
{
  "IFSC": "HDFC0001234",
  "AccountNumber": "1234567890",
  "BankName": "HDFC Bank",
  "BranchName": "Mumbai"
}
```

For Dubai/UAE:
```json
{
  "IBAN": "AE070331234567890123456",
  "SWIFT": "HSBCAEAD",
  "AccountNumber": "1234567890",
  "BankName": "HSBC UAE",
  "BranchName": "Dubai"
}
```

#### Indexes

- **GIN INDEX**: `FieldValues` (for efficient JSONB queries)
- **INDEX**: `(CountryId, FieldValues)` (for country-specific queries)

#### Constraints

- `FieldValues` must be valid JSONB (if provided)
- Bank field values must match configured validation rules for their country (enforced in application code)
- Required bank fields for a country must be present (enforced in application code)

---

## Relationships Summary

### Master Configuration
- `IdentifierType` → `CountryIdentifierConfiguration` (1:N)
- `Country` → `CountryIdentifierConfiguration` (1:N)
- `BankFieldType` → `CountryBankFieldConfiguration` (1:N)
- `Country` → `CountryBankFieldConfiguration` (1:N)

### Company Values
- `CompanyDetails` → `IdentifierValues` (JSONB column, flexible structure)
- `CompanyDetails` → `BankDetails` (1:N, existing from Spec-022)
- `BankDetails` → `FieldValues` (JSONB column, flexible structure)

### References
- `CountryIdentifierConfiguration` → `Country` (N:1, FK to Countries table from Spec-020)
- `CountryBankFieldConfiguration` → `Country` (N:1, FK to Countries table from Spec-020)

---

## Constraints Summary

### IdentifierType
- Name must be unique, alphanumeric with underscores
- Soft delete support (DeletedAt)

### CountryIdentifierConfiguration
- Unique constraint: (CountryId, IdentifierTypeId) - one configuration per country/identifier type
- Validation regex must be valid (if provided)
- MinLength/MaxLength validation

### CompanyDetails.IdentifierValues (JSONB)
- Must be valid JSONB structure
- Values validated against CountryIdentifierConfiguration rules (application-level)
- Required identifiers must be present (application-level)

### BankFieldType
- Name must be unique, alphanumeric with underscores
- Soft delete support (DeletedAt)

### CountryBankFieldConfiguration
- Unique constraint: (CountryId, BankFieldTypeId) - one configuration per country/bank field type
- Validation regex must be valid (if provided)
- MinLength/MaxLength validation

### CompanyBankDetails.FieldValues (JSONB)
- Must be valid JSONB structure
- Values validated against CountryBankFieldConfiguration rules (application-level)
- Required bank fields must be present (application-level)

---

## State Management

### Master Configuration Entities (IdentifierType, BankFieldType)
- **Active**: `DeletedAt IS NULL AND IsActive = true`
- **Inactive**: `DeletedAt IS NOT NULL OR IsActive = false`
- Soft delete: Set `DeletedAt` to current timestamp, set `IsActive = false`

### Country Configuration Entities (CountryIdentifierConfiguration, CountryBankFieldConfiguration)
- **Active**: `DeletedAt IS NULL AND IsActive = true`
- **Inactive**: `DeletedAt IS NOT NULL OR IsActive = false`
- Soft delete: Set `DeletedAt` to current timestamp, set `IsActive = false`
- Enable/Disable: Set `IsActive = false` without soft delete

### Company Values (JSONB)
- **Active**: Present in JSONB structure for the country
- **Inactive**: Not present or set to null
- Add/Update: Upsert in JSONB structure
- Delete: Remove from JSONB structure

---

## Migration Strategy

### Phase 1: Create Master Configuration Tables
1. Create `IdentifierTypes` table
2. Create `CountryIdentifierConfigurations` table
3. Create `BankFieldTypes` table
4. Create `CountryBankFieldConfigurations` table
5. Seed initial data (PAN, TAN, GST for India; VAT for EU; Business License for Dubai; IFSC for India; IBAN/SWIFT for Dubai)

### Phase 2: Add JSONB Columns
1. Add `IdentifierValues` JSONB column to `CompanyDetails` table
2. Add `FieldValues` JSONB column to `BankDetails` table
3. Create GIN indexes on JSONB columns

### Phase 3: Migrate Existing Data
1. Extract existing PAN, TAN, GST values from `CompanyDetails` table
2. Convert to JSONB format: `{ "countryId": { "PAN": "...", "TAN": "...", "GST": "..." } }`
3. Store in `IdentifierValues` JSONB column
4. Extract existing bank details from `BankDetails` table
5. Convert to JSONB format per country
6. Store in `FieldValues` JSONB column

### Phase 4: Update Application Code
1. Update company details services to use JSONB values
2. Update validation logic to use CountryIdentifierConfiguration rules
3. Update frontend to dynamically render fields based on country configuration
4. Add fallback to old columns during transition period

### Phase 5: Deprecate Old Columns
1. Mark old columns as deprecated (PAN, TAN, GST in CompanyDetails; IFSC, IBAN, SWIFT in BankDetails)
2. Remove in future migration after all code uses new JSONB columns

---

## Seed Data

### Initial IdentifierTypes
- PAN (Permanent Account Number)
- TAN (Tax Deduction and Collection Account Number)
- GST (Goods and Services Tax Identification Number)
- VAT (Value Added Tax Number)
- BUSINESS_LICENSE (Trade License Number)

### Initial CountryIdentifierConfigurations
- India: PAN (required, regex: `^[A-Z]{5}[0-9]{4}[A-Z]{1}$`), TAN (optional), GST (required)
- EU: VAT (required, regex: `^[A-Z]{2}[0-9A-Z]{2,12}$`)
- Dubai/UAE: BUSINESS_LICENSE (required, regex: `^[0-9]{6,10}$`)

### Initial BankFieldTypes
- IFSC (Indian Financial System Code)
- IBAN (International Bank Account Number)
- SWIFT (SWIFT/BIC Code)
- ROUTING_NUMBER (ABA Routing Number)
- ACCOUNT_NUMBER (Universal)

### Initial CountryBankFieldConfigurations
- India: IFSC (required, regex: `^[A-Z]{4}0[0-9A-Z]{6}$`), ACCOUNT_NUMBER (required)
- Dubai/UAE: IBAN (required, regex: `^[A-Z]{2}[0-9]{2}[A-Z0-9]{4,30}$`), SWIFT (required, regex: `^[A-Z]{4}[A-Z]{2}[0-9A-Z]{2}([0-9A-Z]{3})?$`), ACCOUNT_NUMBER (required)
- US: ROUTING_NUMBER (required, regex: `^[0-9]{9}$`), ACCOUNT_NUMBER (required)

---

## Notes

- **Flexibility**: JSONB storage allows adding new identifier/bank field types per country without schema changes
- **Validation**: Validation rules stored in configuration tables, applied in application code using FluentValidation
- **Performance**: GIN indexes on JSONB columns ensure efficient queries
- **Extensibility**: Master configuration tables allow adding new countries, identifier types, and bank field types without code changes
- **Backward Compatibility**: Existing CompanyDetails and BankDetails tables remain, with JSONB columns added alongside

