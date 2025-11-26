# Research: Product Catalog & Pricing Management (Spec-021)

**Date**: 2025-01-27  
**Spec**: Spec-021  
**Purpose**: Resolve technical decisions and clarify implementation approach

## Research Questions & Decisions

### 1. Product Pricing Calculation Algorithm

**Question**: How should subscription product pricing be calculated for different billing cycles?

**Decision**: 
- Base price is always per user per month
- Calculation formula: `BasePrice × BillingCycleMultiplier × NumberOfMonthsInCycle × Quantity(Users)`
- Example: Base price $10/user/month, Yearly billing (multiplier 0.85, 12 months), 10 users
  - Calculation: $10 × 0.85 × 12 × 10 = $1,020.00 per year
- Monthly equivalent for display: `(BasePrice × BillingCycleMultiplier × NumberOfMonthsInCycle) / NumberOfMonthsInCycle`

**Rationale**: 
- Consistent pricing model across all billing cycles
- Multipliers provide discount incentives for longer commitments
- Clear calculation logic that can be validated and tested

**Alternatives Considered**:
- Separate base prices per billing cycle: Rejected - too complex, harder to maintain
- Percentage-based discounts: Rejected - multipliers are more flexible and explicit

---

### 2. JSONB Schema Design for Flexible Pricing

**Question**: How should billing cycle multipliers, add-on pricing, and custom development pricing be stored?

**Decision**: Use PostgreSQL JSONB columns with structured schemas:

**BillingCycleMultipliers (JSONB)**:
```json
{
  "quarterly": 0.95,
  "halfYearly": 0.90,
  "yearly": 0.85,
  "multiYear": 0.80
}
```

**AddOnPricing (JSONB)**:
```json
{
  "pricingType": "subscription|oneTime",
  "monthlyPrice": 50.00,  // for subscription
  "fixedPrice": 500.00    // for one-time
}
```

**CustomDevelopmentPricing (JSONB)**:
```json
{
  "pricingModel": "hourly|fixed|projectBased",
  "hourlyRate": 100.00,      // for hourly
  "fixedPrice": 5000.00,     // for fixed
  "baseProjectPrice": 20000.00,  // for project-based
  "estimatedHours": 200      // for project-based
}
```

**Rationale**: 
- Flexible schema supports all product types without multiple tables
- JSONB allows efficient querying and indexing in PostgreSQL
- Easy to extend with new pricing models in the future
- Type-safe access via C# classes with JSON serialization

**Alternatives Considered**:
- Separate tables for each product type: Rejected - too many tables, complex relationships
- Single table with nullable columns: Rejected - too many nullable fields, harder to validate

---

### 3. Billing Cycle Multiplier Logic

**Question**: How should billing cycle multipliers be validated and applied?

**Decision**:
- Multipliers must be between 0 and 1 (representing discounts from base price)
- Multipliers are optional - if not specified, use 1.0 (no discount)
- Multipliers can be configured per product
- Default multipliers can be set in appsettings.json for consistency
- Validation: Each multiplier must be > 0 and ≤ 1

**Rationale**:
- Prevents invalid pricing (negative or zero multipliers)
- Allows products to have custom discount structures
- Provides system-wide defaults for consistency

**Alternatives Considered**:
- Fixed multipliers across all products: Rejected - need flexibility for different product types
- Percentage-based (e.g., 5% discount): Rejected - multipliers are more intuitive and consistent

---

### 4. Product Integration with QuotationLineItem

**Question**: How should products be linked to quotation line items while maintaining backward compatibility?

**Decision**:
- Extend QuotationLineItem entity with optional ProductId FK
- If ProductId is null, line item is a custom/manual entry (existing behavior)
- If ProductId is set, line item is from product catalog
- Store original product price at time of addition (OriginalProductPrice)
- Store billing cycle, hours, and other product-specific data in QuotationLineItem
- Product pricing changes do not affect existing quotations (historical pricing preserved)

**Rationale**:
- Maintains backward compatibility with existing quotations
- Preserves historical pricing for audit and accuracy
- Clear separation between catalog products and custom line items
- Supports both workflows (catalog selection and manual entry)

**Alternatives Considered**:
- Separate table for product line items: Rejected - adds complexity, harder to query
- Always use latest product price: Rejected - breaks historical accuracy, unfair to clients

---

### 5. Product Price History Tracking

**Question**: How should product pricing changes be tracked and audited?

**Decision**:
- Create ProductPriceHistory entity with effective dates
- Track price type (BasePrice, AddOnPrice, CustomDevelopmentPrice)
- Store old price, new price, effective from/to dates
- Log user who made the change and timestamp
- Use effective dates to determine which price was active at quotation creation time
- ProductPriceHistory entries are immutable (append-only)

**Rationale**:
- Complete audit trail for pricing changes
- Supports historical price lookups for quotations
- Enables pricing analytics and reporting
- Compliance with financial audit requirements

**Alternatives Considered**:
- Version entire product entity: Rejected - too complex, only price changes matter
- No history tracking: Rejected - required for audit and historical accuracy

---

### 6. Product Category Hierarchy

**Question**: Should product categories support hierarchical organization?

**Decision**:
- ProductCategory entity supports optional ParentCategoryId for hierarchy
- Categories can be flat (no parent) or nested (with parent)
- Maximum nesting depth: 3 levels (Category → Subcategory → Sub-subcategory)
- Category hierarchy used for:
  - Product organization and filtering
  - Tax calculation (Spec-020 integration)
  - Reporting and analytics
- Categories can be enabled/disabled independently

**Rationale**:
- Flexible organization for large product catalogs
- Supports tax calculation by category (Spec-020 requirement)
- Better user experience for browsing and filtering
- Prevents excessive nesting (performance and UX)

**Alternatives Considered**:
- Flat categories only: Rejected - insufficient for large catalogs and tax integration
- Unlimited nesting: Rejected - performance concerns and UX complexity

---

### 7. Product Usage Statistics Calculation

**Question**: How should product usage statistics be calculated efficiently?

**Decision**:
- Query QuotationLineItems where ProductId matches
- Aggregate statistics:
  - Count of quotations using product
  - Total quantity sold
  - Total revenue (sum of line item amounts)
  - Average quantity per quotation
  - Most recent usage date
- Cache statistics for frequently accessed products
- Refresh cache on product price changes or quotation updates
- Statistics calculated on-demand (not stored in Product entity)

**Rationale**:
- Real-time statistics based on actual quotation data
- No data duplication or sync issues
- Efficient queries with proper indexes
- Caching improves performance for admin dashboard

**Alternatives Considered**:
- Store statistics in Product entity: Rejected - data duplication, sync issues
- Background job to calculate: Rejected - unnecessary complexity, on-demand is sufficient

---

### 8. Multi-Currency Product Pricing

**Question**: How should products support multi-currency pricing (Spec-017 integration)?

**Decision**:
- Product entity has Currency field (ISO 4217 code)
- Products can have different prices in different currencies
- When adding product to quotation:
  - If product currency matches quotation currency: use product price directly
  - If currencies differ: convert using exchange rates (Spec-017)
  - Store converted price in QuotationLineItem (OriginalProductPrice)
- Product catalog can filter by currency
- Admin can set product prices in multiple currencies (future enhancement)

**Rationale**:
- Supports international clients with different currencies
- Integrates with existing multi-currency system (Spec-017)
- Clear conversion logic at quotation creation time
- Historical prices preserved in quotation currency

**Alternatives Considered**:
- Single currency with conversion at display: Rejected - inaccurate for financial records
- Separate product per currency: Rejected - too many duplicate products

---

### 9. Product Disabling vs Deletion

**Question**: Should products be soft-deleted or hard-deleted when no longer needed?

**Decision**:
- Products use soft delete pattern (IsActive flag)
- Disabled products:
  - Cannot be added to new quotations
  - Remain visible in existing quotations (historical accuracy)
  - Can be re-enabled by admin
  - Appear in admin product management with "Inactive" status
- Products cannot be deleted if used in any quotation (enforced by validation)
- Product deletion requires:
  - No quotations using the product
  - Admin confirmation
  - Audit log entry

**Rationale**:
- Maintains data integrity for existing quotations
- Allows products to be temporarily disabled
- Prevents accidental deletion of products in use
- Supports product lifecycle management

**Alternatives Considered**:
- Hard delete with cascade: Rejected - breaks historical quotation data
- Always allow deletion: Rejected - data integrity risk

---

### 10. Product Price Calculation Caching

**Question**: Should product price calculations be cached for performance?

**Decision**:
- Cache product price calculations for frequently accessed products
- Cache key: `ProductId_BillingCycle_Quantity`
- Cache duration: 5 minutes (short enough for price updates, long enough for performance)
- Invalidate cache on:
  - Product price update
  - Product disable/enable
  - Billing cycle multiplier changes
- Use in-memory cache (IMemoryCache) for simplicity
- Cache only for product catalog queries, not quotation calculations (always use latest)

**Rationale**:
- Improves performance for product selector and catalog browsing
- Short cache duration ensures price accuracy
- Simple implementation with existing .NET caching
- Quotation calculations always use latest prices (no cache)

**Alternatives Considered**:
- No caching: Rejected - performance concerns with large catalogs
- Redis distributed cache: Rejected - unnecessary complexity for current scale
- Cache quotation calculations: Rejected - must always use latest prices for accuracy

---

## Integration Points

### Spec-009 (Quotation CRUD) Integration
- Extend QuotationLineItem entity with ProductId, BillingCycle, Hours, OriginalProductPrice
- Update quotation creation/update workflows to support product selection
- Maintain backward compatibility with custom line items (ProductId = null)

### Spec-012 (Discount Approval Workflow) Integration
- Product-level discounts supported in QuotationLineItem.DiscountAmount
- Discount approval workflow applies to product-level discounts
- Quotation-level discounts apply after product-level discounts

### Spec-020 (Multi-Country Tax Management) Integration
- Product categories map to tax categories for tax calculation
- Tax calculation uses product category to determine applicable tax rates
- Tax breakdown displayed per product category in quotations

### Spec-017 (Multi-Currency) Integration
- Product prices stored in product currency
- Currency conversion at quotation creation time
- Exchange rates from Spec-017 currency service

---

## Performance Considerations

1. **Product Catalog Queries**: Index on ProductType, CategoryId, IsActive for fast filtering
2. **Product Price Calculations**: Cache frequently accessed calculations
3. **Quotation Integration**: Efficient queries for product lookup and price calculation
4. **Product Usage Statistics**: Aggregate queries with proper indexes on QuotationLineItem.ProductId

---

## Security Considerations

1. **RBAC Enforcement**: All product management endpoints require Admin role
2. **Price Validation**: Prevent negative or invalid prices
3. **Audit Logging**: Log all product pricing changes
4. **Data Integrity**: Prevent deletion of products in use

---

## Testing Strategy

1. **Unit Tests**: Product pricing calculation logic, billing cycle multipliers
2. **Integration Tests**: Product CRUD operations, quotation integration, tax calculation
3. **E2E Tests**: Complete workflow: Create product → Add to quotation → Calculate totals → Apply discount → Calculate tax

---

**Status**: ✅ All technical decisions resolved. Ready for Phase 1 (Design & Contracts).

