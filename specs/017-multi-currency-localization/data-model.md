# Spec-017: Data Model - Multi-Currency, Multi-Language & Localization

## Overview

This document defines the database schema for multi-currency support, multi-language localization, and user/company preferences.

---

## Database Tables

### 1. Currency

Stores supported currencies in the system.

**Table Name**: `Currencies`

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| CurrencyCode | string(3) | PK, NOT NULL, UNIQUE | ISO 4217 currency code (e.g., "INR", "USD", "EUR") |
| DisplayName | string(100) | NOT NULL | Human-readable currency name (e.g., "Indian Rupee") |
| Symbol | string(10) | NOT NULL | Currency symbol (e.g., "₹", "$", "€") |
| DecimalPlaces | int | NOT NULL, DEFAULT 2 | Number of decimal places for display |
| IsDefault | bool | NOT NULL, DEFAULT false | Whether this is the default system currency |
| IsActive | bool | NOT NULL, DEFAULT true | Whether currency is active and available |
| CreatedAt | DateTimeOffset | NOT NULL | Creation timestamp |
| UpdatedAt | DateTimeOffset | NOT NULL | Last update timestamp |
| CreatedByUserId | Guid | NULL | User who created the currency |
| UpdatedByUserId | Guid | NULL | User who last updated the currency |

**Indexes**:
- Primary Key: `CurrencyCode`
- Index: `IsDefault` (for quick default lookup)
- Index: `IsActive` (for filtering active currencies)

**Default Data**:
- INR (Indian Rupee, ₹, 2 decimals, default)
- USD (US Dollar, $, 2 decimals)
- EUR (Euro, €, 2 decimals)
- GBP (British Pound, £, 2 decimals)

---

### 2. ExchangeRate

Stores exchange rates between currencies with historical tracking.

**Table Name**: `ExchangeRates`

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| ExchangeRateId | Guid | PK, NOT NULL | Unique identifier |
| FromCurrencyCode | string(3) | FK → Currencies, NOT NULL | Source currency code |
| ToCurrencyCode | string(3) | FK → Currencies, NOT NULL | Target currency code |
| Rate | decimal(18,6) | NOT NULL | Exchange rate (1 FromCurrency = Rate ToCurrency) |
| EffectiveDate | DateTime | NOT NULL | Date when rate becomes effective |
| ExpiryDate | DateTime | NULL | Date when rate expires (NULL = current) |
| Source | string(50) | NULL | Source of rate (e.g., "Manual", "API", "Fixer.io") |
| IsActive | bool | NOT NULL, DEFAULT true | Whether rate is currently active |
| CreatedAt | DateTimeOffset | NOT NULL | Creation timestamp |
| UpdatedAt | DateTimeOffset | NOT NULL | Last update timestamp |
| CreatedByUserId | Guid | NULL | User who created the rate |

**Indexes**:
- Primary Key: `ExchangeRateId`
- Unique Index: `(FromCurrencyCode, ToCurrencyCode, EffectiveDate)`
- Index: `FromCurrencyCode`
- Index: `ToCurrencyCode`
- Index: `EffectiveDate`
- Index: `IsActive`

**Constraints**:
- `FromCurrencyCode` ≠ `ToCurrencyCode`
- `Rate` > 0
- `EffectiveDate` ≤ `ExpiryDate` (if ExpiryDate is not NULL)

---

### 3. UserPreferences

Stores user-specific locale and currency preferences.

**Table Name**: `UserPreferences`

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| UserId | Guid | PK, FK → Users, NOT NULL | User identifier |
| LanguageCode | string(5) | NOT NULL, DEFAULT 'en' | ISO 639-1 language code (e.g., "en", "hi", "ar") |
| CurrencyCode | string(3) | FK → Currencies, NULL | Preferred currency (NULL = use company default) |
| DateFormat | string(20) | NOT NULL, DEFAULT 'dd/MM/yyyy' | Date format pattern |
| TimeFormat | string(10) | NOT NULL, DEFAULT '24h' | Time format (12h/24h) |
| NumberFormat | string(50) | NOT NULL, DEFAULT 'en-IN' | Number format locale (e.g., "en-IN", "en-US") |
| Timezone | string(50) | NULL | Timezone identifier (e.g., "Asia/Kolkata") |
| FirstDayOfWeek | int | NOT NULL, DEFAULT 1 | First day of week (1=Monday, 0=Sunday) |
| CreatedAt | DateTimeOffset | NOT NULL | Creation timestamp |
| UpdatedAt | DateTimeOffset | NOT NULL | Last update timestamp |

**Indexes**:
- Primary Key: `UserId`
- Index: `LanguageCode`
- Index: `CurrencyCode`

**Default Values**:
- LanguageCode: 'en'
- DateFormat: 'dd/MM/yyyy'
- TimeFormat: '24h'
- NumberFormat: 'en-IN'
- FirstDayOfWeek: 1

---

### 4. CompanyPreferences

Stores company-level default locale and currency settings.

**Table Name**: `CompanyPreferences`

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| CompanyId | Guid | PK, FK → Companies, NOT NULL | Company identifier |
| DefaultLanguageCode | string(5) | NOT NULL, DEFAULT 'en' | Default language for company |
| DefaultCurrencyCode | string(3) | FK → Currencies, NOT NULL | Default currency for company |
| DateFormat | string(20) | NOT NULL, DEFAULT 'dd/MM/yyyy' | Default date format |
| TimeFormat | string(10) | NOT NULL, DEFAULT '24h' | Default time format |
| NumberFormat | string(50) | NOT NULL, DEFAULT 'en-IN' | Default number format locale |
| Timezone | string(50) | NULL | Default timezone |
| FirstDayOfWeek | int | NOT NULL, DEFAULT 1 | Default first day of week |
| CreatedAt | DateTimeOffset | NOT NULL | Creation timestamp |
| UpdatedAt | DateTimeOffset | NOT NULL | Last update timestamp |
| UpdatedByUserId | Guid | NULL | User who last updated preferences |

**Indexes**:
- Primary Key: `CompanyId`
- Index: `DefaultCurrencyCode`

---

### 5. LocalizationResources

Stores translation strings for UI and system messages.

**Table Name**: `LocalizationResources`

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| ResourceId | Guid | PK, NOT NULL | Unique identifier |
| LanguageCode | string(5) | NOT NULL | ISO 639-1 language code |
| ResourceKey | string(200) | NOT NULL | Resource key (e.g., "common.save", "quotation.title") |
| ResourceValue | string(1000) | NOT NULL | Translated text |
| Category | string(50) | NULL | Resource category (e.g., "UI", "Email", "Notification") |
| IsActive | bool | NOT NULL, DEFAULT true | Whether resource is active |
| CreatedAt | DateTimeOffset | NOT NULL | Creation timestamp |
| UpdatedAt | DateTimeOffset | NOT NULL | Last update timestamp |
| CreatedByUserId | Guid | NULL | User who created the resource |
| UpdatedByUserId | Guid | NULL | User who last updated the resource |

**Indexes**:
- Primary Key: `ResourceId`
- Unique Index: `(LanguageCode, ResourceKey)`
- Index: `LanguageCode`
- Index: `ResourceKey`
- Index: `Category`
- Index: `IsActive`

**Resource Key Naming Convention**:
- Use dot notation: `{module}.{component}.{key}`
- Examples:
  - `common.save`
  - `common.cancel`
  - `quotation.title`
  - `quotation.create`
  - `payment.status.success`
  - `email.quotation.subject`

---

### 6. SupportedLanguages

Stores supported languages in the system.

**Table Name**: `SupportedLanguages`

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| LanguageCode | string(5) | PK, NOT NULL, UNIQUE | ISO 639-1 language code |
| DisplayName | string(100) | NOT NULL | Language name in native script |
| DisplayNameEn | string(100) | NOT NULL | Language name in English |
| NativeName | string(100) | NOT NULL | Language name in its own script |
| IsRTL | bool | NOT NULL, DEFAULT false | Whether language is right-to-left |
| IsActive | bool | NOT NULL, DEFAULT true | Whether language is available |
| FlagIcon | string(50) | NULL | Flag icon identifier (optional) |
| CreatedAt | DateTimeOffset | NOT NULL | Creation timestamp |
| UpdatedAt | DateTimeOffset | NOT NULL | Last update timestamp |

**Indexes**:
- Primary Key: `LanguageCode`
- Index: `IsActive`

**Default Data**:
- en (English, English, English, false, true)
- hi (हिंदी, Hindi, हिंदी, false, true)
- ar (العربية, Arabic, العربية, true, false) - if needed

---

## Entity Relationships

```
Companies
  └── CompanyPreferences (1:1)
       └── DefaultCurrencyCode → Currencies

Users
  └── UserPreferences (1:1)
       └── CurrencyCode → Currencies (nullable)

Currencies
  ├── ExchangeRates (FromCurrencyCode) (1:N)
  └── ExchangeRates (ToCurrencyCode) (1:N)

SupportedLanguages
  └── LocalizationResources (1:N)
```

---

## Data Migration Considerations

### Initial Data Setup

1. **Currencies**: Insert default currencies (INR, USD, EUR, GBP)
2. **SupportedLanguages**: Insert default languages (en, hi)
3. **ExchangeRates**: Insert initial exchange rates (1:1 for same currency, manual rates for others)
4. **LocalizationResources**: Seed English translations for all resource keys
5. **CompanyPreferences**: Set defaults for existing companies
6. **UserPreferences**: Create preferences for existing users with defaults

### Migration Strategy

1. Create new tables without breaking existing functionality
2. Set default values for all existing records
3. Migrate existing data to new schema
4. Update application code to use new schema
5. Validate data integrity

---

## Data Validation Rules

### Currency
- CurrencyCode must be valid ISO 4217 code (3 uppercase letters)
- Symbol must not be empty
- DecimalPlaces must be between 0 and 6
- Only one currency can be marked as IsDefault = true

### ExchangeRate
- FromCurrencyCode and ToCurrencyCode must be different
- Rate must be positive
- EffectiveDate must be in the past or present
- ExpiryDate must be after EffectiveDate (if not NULL)
- Only one active rate per currency pair at a time

### UserPreferences
- LanguageCode must exist in SupportedLanguages
- CurrencyCode must exist in Currencies (if not NULL)
- DateFormat must be valid .NET date format pattern
- NumberFormat must be valid locale identifier

### LocalizationResources
- LanguageCode must exist in SupportedLanguages
- ResourceKey must follow naming convention
- ResourceValue must not be empty
- ResourceKey must be unique per LanguageCode

---

## Performance Optimization

### Indexing Strategy
- Index frequently queried columns (LanguageCode, CurrencyCode, ResourceKey)
- Composite indexes for common query patterns
- Covering indexes for read-heavy operations

### Caching Strategy
- Cache currency list (rarely changes)
- Cache exchange rates (update on schedule)
- Cache localization resources (update on resource change)
- Cache user preferences (update on preference change)

### Query Optimization
- Use efficient joins for currency conversions
- Batch load localization resources by language
- Lazy load exchange rates when needed
- Use materialized views for reporting (if needed)

---

## Audit & Compliance

### Audit Fields
- CreatedAt, UpdatedAt on all tables
- CreatedByUserId, UpdatedByUserId where applicable
- Track exchange rate changes for compliance
- Log preference changes for audit trail

### Data Retention
- Keep historical exchange rates for reporting
- Archive old localization resources
- Maintain audit trail for preference changes

---

**End of Data Model Specification**

