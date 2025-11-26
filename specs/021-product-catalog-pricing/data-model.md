# Data Model: Product Catalog & Pricing Management (Spec-021)

**Date**: 2025-01-27  
**Spec**: Spec-021  
**Database**: PostgreSQL 14+

## Overview

This document defines the database schema for the Product Catalog & Pricing Management feature. The schema includes three new entities (Product, ProductCategory, ProductPriceHistory) and extends the existing QuotationLineItem entity to support product catalog integration.

## Entity Relationships

```
ProductCategory (1) ──< (N) Product (1) ──< (N) ProductPriceHistory
                                 │
                                 │ (optional)
                                 │
                                 ▼
                        QuotationLineItem
```

## Entities

### 1. Product

Represents a product, add-on service, or custom development charge in the catalog.

**Table**: `Products`  
**Schema**: `public`  
**Primary Key**: `ProductId` (UUID)

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `ProductId` | UUID | PK, NOT NULL, DEFAULT gen_random_uuid() | Unique product identifier |
| `ProductName` | VARCHAR(200) | NOT NULL | Product name |
| `ProductType` | VARCHAR(50) | NOT NULL, CHECK | Product type enum: Subscription, AddOnSubscription, AddOnOneTime, CustomDevelopment |
| `Description` | TEXT | NULL | Product description |
| `CategoryId` | UUID | FK → ProductCategories, NULL | Product category (nullable for uncategorized) |
| `BasePricePerUserPerMonth` | DECIMAL(18,2) | NULL | Base price per user per month (for subscription products) |
| `BillingCycleMultipliers` | JSONB | NULL | Billing cycle multipliers: `{"quarterly": 0.95, "halfYearly": 0.90, "yearly": 0.85, "multiYear": 0.80}` |
| `AddOnPricing` | JSONB | NULL | Add-on pricing: `{"pricingType": "subscription|oneTime", "monthlyPrice": 50.00, "fixedPrice": 500.00}` |
| `CustomDevelopmentPricing` | JSONB | NULL | Custom development pricing: `{"pricingModel": "hourly|fixed|projectBased", "hourlyRate": 100.00, "fixedPrice": 5000.00, "baseProjectPrice": 20000.00, "estimatedHours": 200}` |
| `Currency` | VARCHAR(3) | NOT NULL, DEFAULT 'USD' | ISO 4217 currency code |
| `IsActive` | BOOLEAN | NOT NULL, DEFAULT true | Product active status (soft delete) |
| `CreatedAt` | TIMESTAMPTZ | NOT NULL, DEFAULT now() | Creation timestamp |
| `UpdatedAt` | TIMESTAMPTZ | NOT NULL, DEFAULT now() | Last update timestamp |
| `CreatedByUserId` | UUID | FK → Users, NULL | User who created the product |
| `UpdatedByUserId` | UUID | FK → Users, NULL | User who last updated the product |

**Indexes**:
- `IX_Products_ProductType` ON `ProductType`
- `IX_Products_CategoryId` ON `CategoryId`
- `IX_Products_IsActive` ON `IsActive`
- `IX_Products_ProductName` ON `ProductName` (for search)
- `IX_Products_CreatedAt` ON `CreatedAt` DESC

**Constraints**:
- `CK_Products_BasePricePerUserPerMonth` CHECK (`BasePricePerUserPerMonth` IS NULL OR `BasePricePerUserPerMonth` > 0)
- `CK_Products_ProductType_BasePrice` CHECK (`ProductType` = 'Subscription' AND `BasePricePerUserPerMonth` IS NOT NULL OR `ProductType` != 'Subscription')
- `CK_Products_ProductType_AddOnPricing` CHECK (`ProductType` IN ('AddOnSubscription', 'AddOnOneTime') AND `AddOnPricing` IS NOT NULL OR `ProductType` NOT IN ('AddOnSubscription', 'AddOnOneTime'))
- `CK_Products_ProductType_CustomDevelopmentPricing` CHECK (`ProductType` = 'CustomDevelopment' AND `CustomDevelopmentPricing` IS NOT NULL OR `ProductType` != 'CustomDevelopment')

**Business Rules**:
- Subscription products must have `BasePricePerUserPerMonth` set
- Add-on products must have `AddOnPricing` set
- Custom development products must have `CustomDevelopmentPricing` set
- Products cannot be deleted if used in any quotation (enforced in application layer)

---

### 2. ProductCategory

Represents a category for organizing products. Supports hierarchical organization.

**Table**: `ProductCategories`  
**Schema**: `public`  
**Primary Key**: `CategoryId` (UUID)

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `CategoryId` | UUID | PK, NOT NULL, DEFAULT gen_random_uuid() | Unique category identifier |
| `CategoryName` | VARCHAR(100) | NOT NULL | Category name |
| `CategoryCode` | VARCHAR(50) | NOT NULL, UNIQUE | Category code (for programmatic access) |
| `Description` | TEXT | NULL | Category description |
| `ParentCategoryId` | UUID | FK → ProductCategories, NULL | Parent category (for hierarchy, max 3 levels) |
| `IsActive` | BOOLEAN | NOT NULL, DEFAULT true | Category active status |
| `CreatedAt` | TIMESTAMPTZ | NOT NULL, DEFAULT now() | Creation timestamp |
| `UpdatedAt` | TIMESTAMPTZ | NOT NULL, DEFAULT now() | Last update timestamp |
| `CreatedByUserId` | UUID | FK → Users, NULL | User who created the category |
| `UpdatedByUserId` | UUID | FK → Users, NULL | User who last updated the category |

**Indexes**:
- `IX_ProductCategories_CategoryCode` ON `CategoryCode` (UNIQUE)
- `IX_ProductCategories_ParentCategoryId` ON `ParentCategoryId`
- `IX_ProductCategories_IsActive` ON `IsActive`
- `IX_ProductCategories_CategoryName` ON `CategoryName` (for search)

**Constraints**:
- `CK_ProductCategories_CategoryCode` CHECK (`CategoryCode` ~ '^[A-Z0-9_]+$') -- Alphanumeric and underscore only
- `CK_ProductCategories_ParentCategoryId` CHECK (`ParentCategoryId` IS NULL OR `ParentCategoryId` != `CategoryId`) -- Cannot be own parent

**Business Rules**:
- Maximum nesting depth: 3 levels (enforced in application layer)
- Categories cannot be deleted if products are assigned (enforced in application layer)
- Category codes must be unique and uppercase

---

### 3. ProductPriceHistory

Tracks historical pricing changes for products with effective dates.

**Table**: `ProductPriceHistory`  
**Schema**: `public`  
**Primary Key**: `PriceHistoryId` (UUID)

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `PriceHistoryId` | UUID | PK, NOT NULL, DEFAULT gen_random_uuid() | Unique price history identifier |
| `ProductId` | UUID | FK → Products, NOT NULL | Product identifier |
| `PriceType` | VARCHAR(50) | NOT NULL, CHECK | Price type: BasePrice, AddOnPrice, CustomDevelopmentPrice |
| `OldPriceValue` | DECIMAL(18,2) | NULL | Previous price value (NULL for new products) |
| `NewPriceValue` | DECIMAL(18,2) | NOT NULL | New price value |
| `EffectiveFrom` | DATE | NOT NULL | Effective start date |
| `EffectiveTo` | DATE | NULL | Effective end date (NULL for current price) |
| `ChangedByUserId` | UUID | FK → Users, NOT NULL | User who made the change |
| `ChangedAt` | TIMESTAMPTZ | NOT NULL, DEFAULT now() | Change timestamp |
| `ChangeReason` | TEXT | NULL | Reason for price change |

**Indexes**:
- `IX_ProductPriceHistory_ProductId` ON `ProductId`
- `IX_ProductPriceHistory_EffectiveFrom` ON `EffectiveFrom`
- `IX_ProductPriceHistory_EffectiveTo` ON `EffectiveTo`
- `IX_ProductPriceHistory_ProductId_EffectiveFrom` ON (`ProductId`, `EffectiveFrom`)

**Constraints**:
- `CK_ProductPriceHistory_EffectiveDates` CHECK (`EffectiveTo` IS NULL OR `EffectiveTo` >= `EffectiveFrom`)
- `CK_ProductPriceHistory_NewPriceValue` CHECK (`NewPriceValue` > 0)

**Business Rules**:
- Price history entries are immutable (append-only)
- Effective dates cannot overlap for same product and price type (enforced in application layer)
- When price changes, previous entry's `EffectiveTo` is set to new `EffectiveFrom` - 1 day

---

### 4. QuotationLineItem (Extended)

Extends existing QuotationLineItem entity to support product catalog integration.

**Table**: `QuotationLineItems` (existing table, adding columns)  
**Schema**: `public`

**New Columns**:

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `ProductId` | UUID | FK → Products, NULL | Product identifier (NULL for custom line items) |
| `BillingCycle` | VARCHAR(50) | NULL, CHECK | Billing cycle: Monthly, Quarterly, HalfYearly, Yearly, MultiYear (for subscription products) |
| `Hours` | DECIMAL(10,2) | NULL | Hours (for hourly custom development charges) |
| `OriginalProductPrice` | DECIMAL(18,2) | NULL | Original product price at time of addition (for historical accuracy) |
| `DiscountAmount` | DECIMAL(18,2) | NULL | Product-level discount amount |
| `TaxCategoryId` | UUID | FK → ProductServiceCategories (Spec-020), NULL | Tax category for tax calculation |

**New Indexes**:
- `IX_QuotationLineItems_ProductId` ON `ProductId`
- `IX_QuotationLineItems_TaxCategoryId` ON `TaxCategoryId`

**New Constraints**:
- `CK_QuotationLineItems_Hours` CHECK (`Hours` IS NULL OR `Hours` > 0)
- `CK_QuotationLineItems_DiscountAmount` CHECK (`DiscountAmount` IS NULL OR `DiscountAmount` >= 0)
- `CK_QuotationLineItems_ProductId_BillingCycle` CHECK (`ProductId` IS NULL OR (`BillingCycle` IS NOT NULL AND `ProductId` IN (SELECT `ProductId` FROM `Products` WHERE `ProductType` = 'Subscription')) OR `BillingCycle` IS NULL)

**Business Rules**:
- If `ProductId` is set, line item is from product catalog; if NULL, it's a custom/manual entry
- `OriginalProductPrice` preserves historical pricing (product price at quotation creation time)
- `BillingCycle` is required for subscription products
- `Hours` is required for hourly custom development charges
- Product pricing changes do not affect existing quotations (historical pricing preserved)

---

## Enums

### ProductType

```csharp
public enum ProductType
{
    Subscription = 1,           // Subscription products (per user per month)
    AddOnSubscription = 2,      // Add-on services (subscription)
    AddOnOneTime = 3,           // Add-on services (one-time)
    CustomDevelopment = 4        // Custom development charges
}
```

### BillingCycle

```csharp
public enum BillingCycle
{
    Monthly = 1,        // Monthly billing
    Quarterly = 2,      // Quarterly billing (3 months)
    HalfYearly = 3,    // Half-yearly billing (6 months)
    Yearly = 4,         // Yearly billing (12 months)
    MultiYear = 5       // Multi-year billing (2-5 years)
}
```

### PriceType (for ProductPriceHistory)

```csharp
public enum PriceType
{
    BasePrice = 1,              // Base price per user per month
    AddOnPrice = 2,             // Add-on service price
    CustomDevelopmentPrice = 3  // Custom development price
}
```

---

## JSONB Schema Definitions

### BillingCycleMultipliers

```json
{
  "quarterly": 0.95,
  "halfYearly": 0.90,
  "yearly": 0.85,
  "multiYear": 0.80
}
```

**Validation**:
- All values must be > 0 and ≤ 1
- Keys must be: "quarterly", "halfYearly", "yearly", "multiYear"
- All keys are optional (if missing, use 1.0)

### AddOnPricing

```json
{
  "pricingType": "subscription",  // or "oneTime"
  "monthlyPrice": 50.00,           // for subscription (required if pricingType = "subscription")
  "fixedPrice": 500.00             // for one-time (required if pricingType = "oneTime")
}
```

**Validation**:
- `pricingType` must be "subscription" or "oneTime"
- `monthlyPrice` required if `pricingType` = "subscription", must be > 0
- `fixedPrice` required if `pricingType` = "oneTime", must be > 0

### CustomDevelopmentPricing

```json
{
  "pricingModel": "hourly",           // or "fixed" or "projectBased"
  "hourlyRate": 100.00,                // for hourly (required if pricingModel = "hourly")
  "fixedPrice": 5000.00,               // for fixed (required if pricingModel = "fixed")
  "baseProjectPrice": 20000.00,        // for project-based (required if pricingModel = "projectBased")
  "estimatedHours": 200                 // for project-based (optional)
}
```

**Validation**:
- `pricingModel` must be "hourly", "fixed", or "projectBased"
- `hourlyRate` required if `pricingModel` = "hourly", must be > 0
- `fixedPrice` required if `pricingModel` = "fixed", must be > 0
- `baseProjectPrice` required if `pricingModel` = "projectBased", must be > 0
- `estimatedHours` optional, must be > 0 if provided

---

## Migration Strategy

1. **Create new tables**: Products, ProductCategories, ProductPriceHistory
2. **Alter existing table**: Add new columns to QuotationLineItems
3. **Create indexes**: All indexes listed above
4. **Create foreign keys**: All foreign key relationships
5. **Create check constraints**: All check constraints
6. **Data migration**: N/A (new feature, no existing data to migrate)

---

## Data Integrity Rules

1. **Product Deletion**: Products cannot be deleted if `ProductId` exists in any `QuotationLineItem` (enforced in application layer)
2. **Category Deletion**: Categories cannot be deleted if `CategoryId` exists in any `Product` (enforced in application layer)
3. **Price History**: Price history entries are immutable (append-only)
4. **Billing Cycle**: Billing cycle is required for subscription products in quotations
5. **Hours**: Hours are required for hourly custom development charges in quotations
6. **Historical Pricing**: `OriginalProductPrice` in `QuotationLineItem` preserves product price at quotation creation time

---

## Performance Considerations

1. **Indexes**: All foreign keys and frequently queried columns are indexed
2. **JSONB Queries**: Use GIN indexes on JSONB columns if needed for complex queries
3. **Product Catalog Queries**: Optimized with indexes on ProductType, CategoryId, IsActive
4. **Price Calculations**: Cached in application layer for frequently accessed products

---

**Status**: ✅ Data model complete. Ready for implementation.

