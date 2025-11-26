# Research & Technical Decisions: Multi-Country & Jurisdiction Tax Management (Spec-020)

**Date**: 2025-01-27  
**Spec**: [spec.md](./spec.md)  
**Plan**: [plan.md](./plan.md)

## Overview

This document captures technical research and decisions for implementing multi-country and multi-jurisdiction tax management. The system currently supports hardcoded GST (India) tax calculation with CGST/SGST for intra-state and IGST for inter-state. This feature will extend it to support multiple countries, jurisdictions, tax frameworks, and category-based tax rules.

## Research Areas

### 1. Existing Tax Calculation Implementation

**Decision**: Migrate existing hardcoded tax calculation to flexible tax framework system

**Current Implementation**:
- `TaxCalculationService` in `CRM.Application.Quotations.Services` handles only India GST
- Hardcoded rates: 18% GST (9% CGST + 9% SGST for intra-state, 18% IGST for inter-state)
- Quotation entity has `TaxAmount`, `CgstAmount`, `SgstAmount`, `IgstAmount` fields
- Calculation based on client state code comparison with company state code

**Migration Strategy**:
1. Create new `TaxCalculationService` that uses tax framework configuration
2. Maintain backward compatibility - existing quotations retain their tax amounts
3. Gradually migrate to framework-based calculation for new quotations
4. Support both old (GST-specific) and new (framework-based) tax structures during transition

**Rationale**: 
- Backward compatibility ensures existing quotations remain unchanged
- Framework-based approach enables multi-country support
- Gradual migration reduces risk and allows testing

**Alternatives Considered**:
- **Immediate hard break**: Reject - would break existing quotations
- **Dual calculation**: Accept - new framework calculates, old code validates (temporary)
- **Tax migration script**: Consider for future - bulk recalculate historical quotations if needed

---

### 2. Product/Service Category Management

**Decision**: Create new `ProductServiceCategory` entity for tax rule assignment

**Research Findings**:
- No existing category entity found in the codebase
- `QuotationLineItem` entity does not have category field
- Categories needed for category-based tax rates (e.g., services at 5% VAT, products at 5% VAT in Dubai)

**Implementation Approach**:
1. Create `ProductServiceCategory` entity with: CategoryId, CategoryName, CategoryCode, Description, IsActive
2. Add `ProductServiceCategoryId` (nullable FK) to `QuotationLineItem` entity
3. Categories are admin-managed (similar to roles)
4. Categories can be assigned to line items during quotation creation/editing
5. Tax rates can be configured per category per jurisdiction

**Rationale**:
- Flexible - allows different tax rates for different item types
- Required for UAE VAT compliance (some categories may have different rates)
- Extensible - can add more category properties later if needed

**Alternatives Considered**:
- **Hardcoded category names**: Reject - not flexible, cannot add new categories
- **Category as string field**: Reject - no referential integrity, harder to query
- **Category as enum**: Reject - not configurable by admin, requires code changes

---

### 3. Multi-Component Tax Framework Structure

**Decision**: Use JSONB column to store tax components configuration

**Tax Framework Requirements**:
- **GST (India)**: Has 2 components (CGST, SGST) for intra-state, 1 component (IGST) for inter-state
- **VAT (UAE/Dubai)**: Has 1 component (VAT) for all transactions
- Future: May need 3+ components for other countries

**Storage Design**:
- `TaxFramework.TaxComponents` as JSONB array: `[{ "name": "CGST", "code": "CGST", "isCentrallyGoverned": false }, { "name": "SGST", "code": "SGST", "isCentrallyGoverned": false }, { "name": "IGST", "code": "IGST", "isCentrallyGoverned": true }]`
- Each tax rate record specifies which component(s) it applies to
- For GST intra-state: Apply both CGST and SGST rates
- For GST inter-state: Apply IGST rate
- For VAT: Apply single VAT rate

**Rationale**:
- JSONB provides flexibility for varying tax structures
- Allows querying and indexing in PostgreSQL
- Easy to extend for future tax frameworks
- No need to create separate tables for each component type

**Alternatives Considered**:
- **Separate TaxComponent table**: Considered but rejected - adds complexity, harder to query
- **Fixed columns per component**: Reject - limits flexibility, cannot handle varying component counts
- **Enum + fixed structure**: Reject - not flexible enough for future requirements

---

### 4. Jurisdiction Hierarchy Structure

**Decision**: Self-referencing FK for jurisdiction hierarchy (up to 3 levels)

**Research Findings**:
- India: Country → State (2 levels, or Country → State → City for 3 levels)
- UAE: Country → Emirate (2 levels, or Country → Emirate → City for 3 levels)
- Need to support: Country → Jurisdiction → Sub-Jurisdiction

**Implementation**:
- `Jurisdiction.ParentJurisdictionId` (nullable FK to Jurisdiction)
- Hierarchy: `Country` → `Jurisdiction` (parent=null) → `SubJurisdiction` (parent=JurisdictionId)
- Tax rates configured at any level in hierarchy
- Lookup algorithm: Check jurisdiction → check parent → check country default

**Example**:
```
Country: India (IN)
├── Jurisdiction: Maharashtra (parent=null, level=1)
│   ├── Sub-Jurisdiction: Mumbai (parent=Maharashtra, level=2)
│   └── Sub-Jurisdiction: Pune (parent=Maharashtra, level=2)
└── Jurisdiction: Karnataka (parent=null, level=1)
    └── Sub-Jurisdiction: Bangalore (parent=Karnataka, level=2)
```

**Rationale**:
- Supports real-world tax jurisdictions (states, cities)
- Flexible depth (can add more levels if needed)
- Simple to query with recursive CTEs or application-level traversal
- Matches tax authority structure (state-level GST, city-level surcharges)

**Alternatives Considered**:
- **Fixed 2-level structure**: Reject - too limiting, doesn't support cities
- **Adjacency list with materialized path**: Considered but rejected - adds complexity, current needs don't justify
- **Separate tables per level**: Reject - too rigid, hard to query across levels

---

### 5. Client Location Storage Enhancement

**Decision**: Add `CountryId` and `JurisdictionId` FK fields to `Client` entity

**Current State**:
- `Client` entity has: `State`, `City`, `StateCode`, `PinCode` (all string fields)
- No country or jurisdiction relationship

**Enhancement**:
- Add `CountryId` (FK to Country, nullable initially for migration)
- Add `JurisdictionId` (FK to Jurisdiction, nullable)
- Keep existing `State`, `City`, `StateCode` fields for backward compatibility
- Migration: Populate CountryId/JurisdictionId from existing State/StateCode data where possible

**Rationale**:
- Enables automatic tax determination from client location
- Maintains referential integrity (client must reference valid country/jurisdiction)
- Backward compatible - old clients without CountryId/JurisdictionId still work (default to company default)

**Alternatives Considered**:
- **Replace string fields with FKs only**: Reject - breaks backward compatibility
- **Keep string fields only**: Reject - no referential integrity, harder to query tax rules
- **Separate Address table**: Consider for future - overkill for current requirements

---

### 6. Tax Rate Lookup Algorithm

**Decision**: Priority-based lookup: Category+Jurisdiction → Jurisdiction → Country default

**Lookup Priority** (highest to lowest):
1. **Category-specific rate in jurisdiction**: `TaxRate` with `ProductServiceCategoryId` AND `JurisdictionId` AND `EffectiveFrom <= now <= EffectiveTo`
2. **Jurisdiction base rate**: `TaxRate` with `JurisdictionId` AND `ProductServiceCategoryId IS NULL` AND `EffectiveFrom <= now <= EffectiveTo`
3. **Parent jurisdiction rate**: If jurisdiction has parent, check parent (recursive)
4. **Country default rate**: `TaxRate` with `JurisdictionId IS NULL` AND `CountryId` AND `EffectiveFrom <= now <= EffectiveTo`

**Effective Date Handling**:
- Only rates where `EffectiveFrom <= current_date` and (`EffectiveTo IS NULL` OR `EffectiveTo >= current_date`) are considered
- If multiple rates match, use the most recent `EffectiveFrom` date
- Historical quotations retain original tax rates (no recalculation)

**Rationale**:
- Supports category-specific rates (e.g., services vs products)
- Supports jurisdiction hierarchy (city → state → country)
- Supports historical rates (effective dates)
- Predictable and testable lookup logic

**Alternatives Considered**:
- **Single rate per jurisdiction**: Reject - doesn't support category-based rates
- **Always use country default**: Reject - doesn't support jurisdiction-specific rates
- **Complex rule engine**: Reject - overkill, current requirements don't justify complexity

---

### 7. UAE VAT Structure

**Decision**: Implement VAT as single-component tax framework (5% standard rate)

**Research Findings**:
- UAE VAT rate: 5% standard rate (as of 2024)
- Dubai is an emirate within UAE
- VAT applies uniformly across UAE (no state-level variations)
- Some items may be zero-rated or exempt (configurable via tax rate `IsZeroRated` or `IsExempt` flags)

**Implementation**:
- Create UAE country with VAT framework
- Create Dubai as jurisdiction under UAE
- Configure 5% VAT rate for Dubai
- Configure category-specific rates if needed (e.g., zero-rated for exports)

**Tax Components**:
```json
[{ "name": "VAT", "code": "VAT", "isCentrallyGoverned": true }]
```

**Rationale**:
- Matches real-world UAE VAT structure
- Simpler than GST (single component vs multiple)
- Extensible for future zero-rated/exempt categories

**Alternatives Considered**:
- **Hardcode UAE rates**: Reject - not flexible, cannot adjust rates without code changes
- **Separate VAT framework type**: Accept - use TaxFrameworkType enum to distinguish

---

### 8. Tax Calculation Service Architecture

**Decision**: Create new `ITaxCalculationService` interface with framework-based implementation

**Service Interface**:
```csharp
public interface ITaxCalculationService
{
    Task<TaxCalculationResult> CalculateTaxAsync(
        Guid clientId,
        IEnumerable<LineItemTaxInput> lineItems,
        DateTime calculationDate,
        CancellationToken cancellationToken = default);
}
```

**Input**:
- `clientId`: To determine country/jurisdiction
- `lineItems`: Each with `ProductServiceCategoryId`, `Amount` (after discount)
- `calculationDate`: For effective date lookup

**Output**:
- `TaxCalculationResult` with tax breakdown by component (CGST, SGST, IGST, VAT, etc.)
- Total tax amount
- Calculation details for audit log

**Caching Strategy**:
- Cache tax rates by jurisdiction+category+date (key: `{jurisdictionId}:{categoryId}:{date}`)
- Cache invalidation: When tax rate created/updated/deleted
- Cache TTL: 1 hour (tax rates don't change frequently)
- Use `IMemoryCache` for in-memory caching

**Rationale**:
- Interface enables testing and future implementations
- Async for database queries (jurisdiction lookup, rate lookup)
- Caching improves performance for frequent lookups
- Framework-based approach supports multiple countries

**Alternatives Considered**:
- **Synchronous service**: Reject - database queries should be async
- **No caching**: Reject - performance impact for high-volume scenarios
- **Distributed cache (Redis)**: Consider for future - current volume doesn't justify

---

### 9. Quotation Entity Tax Field Migration

**Decision**: Keep existing tax fields AND add new framework-agnostic fields

**Current Fields** (GST-specific):
- `TaxAmount` (decimal)
- `CgstAmount` (decimal?)
- `SgstAmount` (decimal?)
- `IgstAmount` (decimal?)

**New Fields** (framework-agnostic):
- `TaxCountryId` (Guid?, FK to Country)
- `TaxJurisdictionId` (Guid?, FK to Jurisdiction)
- `TaxFrameworkId` (Guid?, FK to TaxFramework)
- `TaxBreakdown` (JSONB) - stores component-wise breakdown: `[{ "component": "CGST", "rate": 9.0, "amount": 4050.00 }, { "component": "SGST", "rate": 9.0, "amount": 4050.00 }]`

**Migration Strategy**:
- For GST quotations: Populate both old and new fields (dual-write during transition)
- For new VAT quotations: Populate only new fields
- Old fields deprecated but retained for backward compatibility
- Frontend displays using new fields, falls back to old if new fields empty

**Rationale**:
- Backward compatibility for existing quotations
- Framework-agnostic design supports any tax structure
- JSONB breakdown enables flexible tax component display
- Gradual migration path

**Alternatives Considered**:
- **Remove old fields immediately**: Reject - breaks existing quotations
- **Keep old fields only**: Reject - doesn't support multi-country
- **Separate TaxDetails table**: Consider for future - current needs don't justify extra table

---

### 10. ISO 3166-1 Alpha-2 Country Code Validation

**Decision**: Use regex validation for ISO 3166-1 alpha-2 format

**Validation Rule**:
- Format: Exactly 2 uppercase letters (A-Z)
- Regex: `^[A-Z]{2}$`
- Examples: `IN`, `AE`, `US`, `GB`

**Implementation**:
- Validate in `CreateCountryRequestValidator` and `UpdateCountryRequestValidator`
- Validate uniqueness in database (unique constraint on `CountryCode`)
- Case-insensitive uniqueness check (store uppercase, compare case-insensitive)

**Rationale**:
- ISO 3166-1 alpha-2 is standard for country codes
- Simple validation, no need for full ISO list (admin can enter any valid code)
- Uniqueness ensures one country per code

**Alternatives Considered**:
- **Full ISO 3166-1 database**: Consider for future - adds complexity, current needs don't justify
- **No validation**: Reject - data quality issues, duplicate codes possible
- **Case-sensitive uniqueness**: Reject - IN and in should be treated as same

---

### 11. Database Indexing Strategy

**Decision**: Index on FK columns and frequently queried columns

**Required Indexes**:
1. `Countries.CountryCode` - UNIQUE INDEX (for lookup and uniqueness)
2. `Jurisdictions.CountryId` - INDEX (for country → jurisdictions query)
3. `Jurisdictions.ParentJurisdictionId` - INDEX (for hierarchy traversal)
4. `TaxRates.JurisdictionId` - INDEX (for rate lookup by jurisdiction)
5. `TaxRates.ProductServiceCategoryId` - INDEX (for rate lookup by category)
6. `TaxRates.EffectiveFrom, EffectiveTo` - INDEX (for effective date filtering)
7. `TaxRates(JurisdictionId, ProductServiceCategoryId, EffectiveFrom, EffectiveTo)` - COMPOSITE INDEX (for rate lookup query)
8. `TaxCalculationLog.QuotationId` - INDEX (for audit log queries)
9. `TaxCalculationLog.ChangedAt` - INDEX (for date range filtering)
10. `Clients.CountryId` - INDEX (for client location lookup)
11. `Clients.JurisdictionId` - INDEX (for client location lookup)
12. `QuotationLineItems.ProductServiceCategoryId` - INDEX (for category-based queries)

**Rationale**:
- FK indexes improve join performance
- Composite index on TaxRates optimizes rate lookup query
- Date indexes enable efficient range queries for audit logs
- Category indexes support category-based tax rule queries

**Alternatives Considered**:
- **No indexes**: Reject - poor query performance
- **Index all columns**: Reject - unnecessary overhead, slower writes
- **Materialized views**: Consider for future - current volume doesn't justify

---

### 12. Admin UI Component Library

**Decision**: Use existing TailAdmin Next.js components and patterns

**Research Findings**:
- Project uses TailAdmin theme (Next.js)
- Existing admin pages follow consistent patterns (tables, forms, modals)
- Uses shadcn/ui components
- Responsive design required

**Implementation**:
- Follow existing admin page patterns (e.g., `/admin/settings`, `/admin/users`)
- Reuse existing table, form, and modal components
- Use consistent styling and layout
- Implement tree view for jurisdiction hierarchy (custom component or library)

**Rationale**:
- Consistency with existing admin interface
- Faster development using existing components
- Familiar UX for admins
- Maintainable codebase

**Alternatives Considered**:
- **Custom UI library**: Reject - unnecessary, TailAdmin already available
- **Different admin theme**: Reject - inconsistent with existing design

---

## Summary of Key Decisions

| Area | Decision | Rationale |
|------|----------|-----------|
| Tax Calculation Migration | Framework-based with backward compatibility | Enables multi-country while preserving existing quotations |
| Category Management | New ProductServiceCategory entity | Flexible, admin-configurable category-based tax rates |
| Tax Components Storage | JSONB array in TaxFramework | Flexible, supports varying tax structures |
| Jurisdiction Hierarchy | Self-referencing FK (up to 3 levels) | Supports real-world tax jurisdiction structure |
| Client Location | Add CountryId/JurisdictionId FKs | Enables automatic tax determination with referential integrity |
| Tax Rate Lookup | Priority: Category+Jurisdiction → Jurisdiction → Country | Predictable, supports category and jurisdiction hierarchy |
| UAE VAT | Single-component 5% rate | Matches real-world structure |
| Tax Service | Async interface with caching | Performance and testability |
| Quotation Tax Fields | Keep old + add new framework-agnostic fields | Backward compatibility + future-proof |
| Country Code Validation | Regex + uniqueness constraint | ISO 3166-1 compliance |
| Database Indexing | Indexes on FKs and frequently queried columns | Optimized query performance |
| Admin UI | TailAdmin components and patterns | Consistency and faster development |

## Open Questions / Future Enhancements

1. **Tax Migration Tool**: Bulk recalculate historical quotations with new tax rates? (Future)
2. **Exchange Rate Integration**: How to handle tax calculations in different currencies? (Spec-017 dependency)
3. **Reverse Charge Mechanism**: Support for B2B reverse charge? (Future)
4. **Tax-on-Tax (Compound Tax)**: Support for taxes calculated on other taxes? (Future)
5. **Third-Party Tax APIs**: Integration with tax authority APIs for rate updates? (Future)

## References

- [ISO 3166-1 Alpha-2 Country Codes](https://en.wikipedia.org/wiki/ISO_3166-1_alpha-2)
- [India GST Structure](https://www.gst.gov.in/)
- [UAE VAT Regulations](https://www.mof.gov.ae/en/strategiesandpolicies/vat)
- [PostgreSQL JSONB Documentation](https://www.postgresql.org/docs/current/datatype-json.html)
- Existing codebase: `src/Backend/CRM.Application/Quotations/Services/TaxCalculationService.cs`
- Existing codebase: `src/Backend/CRM.Domain/Entities/Quotation.cs`

