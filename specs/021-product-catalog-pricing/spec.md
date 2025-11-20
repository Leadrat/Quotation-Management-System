# Spec-021: Product Catalog & Pricing Management

**Feature Branch**: `021-product-catalog-pricing`  
**Created**: 2025-11-18  
**Status**: Draft  
**Input**: User description: "There will be subscription based products - usually per user per month, billed quarterly, half yearly yearly or multi-years, add on services either as subscription or one time, and custom development charges. Allow user to create and manage products, add-on services and development charges so that while creating a quote, one can add these, change quantity and calculate. There can be discount, and GST or local taxes applicable to specific geography"

## Overview

This specification introduces a comprehensive product catalog and pricing management system that enables users to create and manage products, add-on services, and custom development charges. The system supports subscription-based products with flexible billing cycles (per user per month, billed quarterly, half-yearly, yearly, or multi-year), add-on services available as subscriptions or one-time charges, and custom development charges. Products from the catalog can be added to quotations with quantity adjustments, automatic price calculations, discounts, and geography-based tax calculations (GST or local taxes).

## Project Information

- **Project Name**: CRM Quotation Management System
- **Spec Number**: Spec-021
- **Spec Name**: Product Catalog & Pricing Management
- **Group**: Advanced Features (Group 11 of 11)
- **Priority**: HIGH (Phase 2, after Multi-Country Tax Management)
- **Dependencies**: 
  - Spec-009 (Quotation Entity & CRUD Operations)
  - Spec-012 (Discount Approval Workflow)
  - Spec-020 (Multi-Country & Jurisdiction Tax Management)
- **Related Specs**: 
  - Spec-010 (Quotation Management)
  - Spec-017 (Multi-Currency & Localization)

---

## Key Features

### Product Catalog Management
- Create and manage subscription-based products (per user per month)
- Configure billing cycles (quarterly, half-yearly, yearly, multi-year)
- Create and manage add-on services (subscription or one-time)
- Create and manage custom development charges
- Organize products by categories
- Enable/disable products
- Track product versions and changes

### Pricing Configuration
- Set base pricing per product (per user per month for subscriptions)
- Configure billing cycle multipliers (discounts for longer commitments)
- Set add-on service pricing (subscription or one-time)
- Configure custom development charge rates (hourly, fixed, or project-based)
- Support multi-currency pricing (integrates with Spec-017)
- Historical price tracking with effective dates

### Product Types
- **Subscription Products**: Recurring products billed per user per month, with billing cycles (quarterly, half-yearly, yearly, multi-year)
- **Add-On Services (Subscription)**: Additional services that can be subscribed to with recurring charges
- **Add-On Services (One-Time)**: Additional services charged once
- **Custom Development Charges**: Custom development work with flexible pricing (hourly, fixed, project-based)

### Quotation Integration
- Select products from catalog when creating quotations
- Add products to quotation line items with quantity adjustments
- Automatic price calculation based on selected products, quantities, and billing cycles
- Apply discounts to products in quotations
- Calculate taxes based on geography and product categories (integrates with Spec-020)
- Display product details and pricing breakdown in quotations

### Discount Management
- Apply quotation-level discounts (integrates with Spec-012)
- Apply product-level discounts
- Configure discount approval workflows for discounts above thresholds
- Display discount breakdown in quotations

### Tax Calculation Integration
- Automatic tax calculation based on client geography (integrates with Spec-020)
- Support for GST (India) and VAT (UAE/Dubai) based on product categories
- Display tax breakdown per product/service category
- Show tax-exempt or zero-rated products where applicable

---

## JTBD Alignment

**Persona**: Sales Rep, Administrator, Account Manager

**JTBD**: "I want to create and manage a catalog of products, add-on services, and development charges with flexible pricing and billing cycles, so that I can quickly create accurate quotations by selecting from the catalog rather than manually entering product details every time"

**Success Metric**: "Sales reps can create quotations 50% faster by selecting from product catalog; zero pricing calculation errors; 100% of quotations use products from catalog"

---

## Business Value

- Reduces quotation creation time by enabling product selection from catalog
- Ensures pricing consistency across all quotations
- Eliminates manual pricing calculation errors
- Supports flexible subscription models and billing cycles
- Enables efficient management of add-on services and custom development charges
- Facilitates accurate tax calculations based on geography and product categories
- Improves sales efficiency and quote accuracy
- Provides audit trail for product pricing changes

---

## User Scenarios & Testing

### User Story 1 - Admin Creates Subscription Product (Priority: P1)

As an administrator, I want to create subscription-based products (per user per month) with billing cycles (quarterly, half-yearly, yearly, multi-year) so that sales reps can select these products when creating quotations.

**Why this priority**: This is the foundation of the product catalog. Without products in the catalog, sales reps cannot use the catalog feature. Subscription products are the primary product type and must be supported first.

**Independent Test**: Can be fully tested by an admin logging in, navigating to product catalog, creating a subscription product (e.g., "Cloud Storage - 1TB per user/month"), setting base price per user per month, configuring billing cycles with multipliers, and verifying the product is saved and appears in the catalog.

**Acceptance Scenarios**:

1. **Given** I am logged in as an admin, **When** I navigate to `/products/catalog`, **Then** I see a list of all products (initially empty or showing existing products)

2. **Given** I am on the product catalog page, **When** I click "Add Product" and select "Subscription Product", **Then** I see a form to create a subscription product

3. **Given** I am creating a subscription product, **When** I fill in:
   - Product name: "Cloud Storage - 1TB per user/month"
   - Description: "Monthly cloud storage subscription"
   - Base price per user per month: $10.00
   - Billing cycles: Quarterly (multiplier: 0.95), Half-yearly (multiplier: 0.90), Yearly (multiplier: 0.85), Multi-year (multiplier: 0.80)
   - Category: "Cloud Services"
   - Then click "Save", **Then** the product is created and appears in the catalog

4. **Given** I have created a subscription product, **When** I view the product details, **Then** I can see all configured billing cycles with their multipliers

5. **Given** I am a non-admin user, **When** I try to access `/products/catalog` (management interface), **Then** I receive a 403 Forbidden error

---

### User Story 2 - Admin Creates Add-On Service (Priority: P1)

As an administrator, I want to create add-on services that can be either subscription-based or one-time charges so that sales reps can offer additional services to clients in quotations.

**Why this priority**: Add-on services are commonly used in quotations and must be available alongside subscription products. This enables comprehensive product offerings from the catalog.

**Independent Test**: Can be fully tested by an admin creating an add-on service (e.g., "24/7 Support - Premium"), setting it as a subscription add-on with monthly pricing, creating another add-on service (e.g., "Migration Service") as a one-time charge, and verifying both appear in the catalog.

**Acceptance Scenarios**:

1. **Given** I am on the product catalog page, **When** I click "Add Product" and select "Add-On Service", **Then** I see options to create subscription or one-time add-on

2. **Given** I am creating an add-on service, **When** I select "Subscription" and fill in:
   - Service name: "24/7 Support - Premium"
   - Description: "Premium support with 24/7 availability"
   - Monthly price: $50.00
   - Category: "Support Services"
   - Then click "Save", **Then** the subscription add-on is created

3. **Given** I am creating an add-on service, **When** I select "One-Time" and fill in:
   - Service name: "Migration Service"
   - Description: "One-time data migration service"
   - Fixed price: $500.00
   - Category: "Professional Services"
   - Then click "Save", **Then** the one-time add-on is created

4. **Given** I have created add-on services, **When** I view the product catalog, **Then** I can see both subscription and one-time add-ons with their pricing clearly indicated

---

### User Story 3 - Admin Creates Custom Development Charge (Priority: P1)

As an administrator, I want to create custom development charges with flexible pricing (hourly, fixed, or project-based) so that sales reps can include custom development work in quotations.

**Why this priority**: Custom development charges are frequently needed in quotations. This must be supported to enable complete quotation creation from the catalog.

**Independent Test**: Can be fully tested by an admin creating a custom development charge (e.g., "Custom API Development"), setting it as hourly rate ($100/hour), creating another as fixed price ($5,000), and verifying both appear in the catalog with appropriate pricing models.

**Acceptance Scenarios**:

1. **Given** I am on the product catalog page, **When** I click "Add Product" and select "Custom Development Charge", **Then** I see options for pricing model (hourly, fixed, project-based)

2. **Given** I am creating a custom development charge, **When** I select "Hourly" and fill in:
   - Service name: "Custom API Development"
   - Description: "Custom API development work"
   - Hourly rate: $100.00
   - Category: "Development Services"
   - Then click "Save", **Then** the hourly development charge is created

3. **Given** I am creating a custom development charge, **When** I select "Fixed Price" and fill in:
   - Service name: "Website Redesign"
   - Description: "Complete website redesign project"
   - Fixed price: $5,000.00
   - Category: "Development Services"
   - Then click "Save", **Then** the fixed-price development charge is created

4. **Given** I am creating a custom development charge, **When** I select "Project-Based" and fill in:
   - Service name: "Enterprise Integration Project"
   - Description: "Multi-phase integration project"
   - Base project price: $20,000.00
   - Estimated hours: 200
   - Hourly rate: $100.00
   - Category: "Development Services"
   - Then click "Save", **Then** the project-based development charge is created

---

### User Story 4 - Sales Rep Adds Products to Quotation (Priority: P1)

As a sales rep, I want to add products from the catalog to quotations by selecting them and adjusting quantities, so that I can create quotations faster and ensure pricing accuracy without manual entry.

**Why this priority**: This is the core user-facing value of the product catalog. Without the ability to add products to quotations, the catalog has no practical use. This must work seamlessly with quotation creation.

**Independent Test**: Can be fully tested by a sales rep creating a new quotation, clicking "Add Product from Catalog", selecting a subscription product, adjusting quantity (number of users) and billing cycle (yearly), seeing the calculated price, adding an add-on service, and verifying all products appear as line items with correct pricing.

**Acceptance Scenarios**:

1. **Given** I am creating a quotation, **When** I click "Add Product from Catalog", **Then** I see a product catalog selector with search and filter options

2. **Given** I am selecting a product from the catalog, **When** I select a subscription product (e.g., "Cloud Storage - 1TB"), **Then** I see options to enter quantity (number of users) and select billing cycle (quarterly, half-yearly, yearly, multi-year)

3. **Given** I have selected a subscription product with quantity 10 users and billing cycle "Yearly", **When** I click "Add to Quotation", **Then** a line item is added showing:
   - Product name: "Cloud Storage - 1TB per user/month"
   - Quantity: 10 users
   - Billing cycle: Yearly
   - Unit rate: $10.00 × 0.85 (yearly multiplier) × 12 months = $102.00 per user per year
   - Amount: $102.00 × 10 = $1,020.00

4. **Given** I am adding products to a quotation, **When** I select an add-on service (one-time or subscription), **Then** it is added as a line item with appropriate pricing

5. **Given** I am adding products to a quotation, **When** I select a custom development charge (hourly), **Then** I can enter hours and the system calculates: Unit rate × Hours = Amount

6. **Given** I have added products to a quotation, **When** I change the quantity or billing cycle of a line item, **Then** the amount is recalculated automatically

---

### User Story 5 - System Calculates Totals with Discounts and Taxes (Priority: P1)

As a sales rep, I want the system to automatically calculate quotation totals including discounts and geography-based taxes when products are added, so that I don't have to manually calculate totals.

**Why this priority**: Automatic calculation is essential for accuracy and speed. Discounts and taxes must work seamlessly with products from the catalog. This integrates with Spec-012 (discounts) and Spec-020 (taxes).

**Independent Test**: Can be fully tested by creating a quotation with products, applying a 10% discount, selecting a client in India (Maharashtra), and verifying the system calculates: Subtotal → Discount → Taxable Amount → GST (CGST + SGST) → Total Amount correctly.

**Acceptance Scenarios**:

1. **Given** I have added products to a quotation with subtotal $10,000, **When** I apply a 10% quotation-level discount, **Then** discount amount is $1,000 and taxable amount is $9,000

2. **Given** I have a quotation with products for a client in India (Maharashtra), **When** the system calculates taxes, **Then** it determines applicable GST rates based on client location and product categories, calculates CGST and SGST, and displays tax breakdown

3. **Given** I have a quotation with products for a client in Dubai, UAE, **When** the system calculates taxes, **Then** it determines applicable VAT rates based on client location and product categories, calculates VAT, and displays tax breakdown

4. **Given** I have applied discounts and taxes to a quotation, **When** I view the quotation totals, **Then** I see:
   - Subtotal: Sum of all line items
   - Discount: Applied discount amount
   - Taxable Amount: Subtotal - Discount
   - Tax Breakdown: CGST, SGST (or VAT) by category
   - Total Tax: Sum of all tax components
   - Total Amount: Taxable Amount + Total Tax

5. **Given** I am editing a quotation, **When** I change products, quantities, discount, or client location, **Then** all totals (subtotal, discount, tax, total) are recalculated automatically

---

### User Story 6 - Admin Manages Product Catalog (Priority: P2)

As an administrator, I want to manage the product catalog by editing products, enabling/disabling them, organizing by categories, and viewing product usage history, so that I can keep the catalog up-to-date and understand which products are most used.

**Why this priority**: Important for maintaining the catalog and business insights, but not critical for initial functionality. Can be implemented after core product creation and quotation integration are working.

**Independent Test**: Can be fully tested by an admin editing a product (changing price or description), disabling a product, viewing product usage statistics (how many quotations use this product), and verifying changes are reflected correctly.

**Acceptance Scenarios**:

1. **Given** I am viewing the product catalog, **When** I click "Edit" on a product, **Then** I can modify product details (name, description, pricing, billing cycles)

2. **Given** I am editing a product, **When** I change the base price per user per month, **Then** the change is saved and existing quotations are not affected (only new quotations use the new price)

3. **Given** I am managing products, **When** I disable a product, **Then** it no longer appears in the product selector for new quotations (existing quotations remain unchanged)

4. **Given** I am viewing product details, **When** I check product usage, **Then** I can see how many quotations use this product and total revenue generated

5. **Given** I am managing products, **When** I organize products by categories, **Then** I can filter and search products by category when adding to quotations

---

## Requirements

### Functional Requirements

#### Product Catalog Management
- **FR-001**: System MUST allow admins to create subscription products with base price per user per month
- **FR-002**: System MUST allow admins to configure billing cycles (quarterly, half-yearly, yearly, multi-year) with multipliers for subscription products
- **FR-003**: System MUST allow admins to create add-on services (subscription or one-time) with pricing
- **FR-004**: System MUST allow admins to create custom development charges with pricing models (hourly, fixed, project-based)
- **FR-005**: System MUST allow admins to organize products by categories
- **FR-006**: System MUST allow admins to enable/disable products (disabled products cannot be added to new quotations)
- **FR-007**: System MUST allow admins to edit product details (name, description, pricing, billing cycles)
- **FR-008**: System MUST track product pricing history with effective dates
- **FR-009**: System MUST enforce RBAC - only admins can manage products
- **FR-010**: System MUST allow admins to view product usage statistics (number of quotations, total revenue)

#### Subscription Product Pricing
- **FR-011**: System MUST calculate subscription product price as: Base Price × Billing Cycle Multiplier × Number of Months in Billing Cycle × Quantity (Users)
- **FR-012**: System MUST support billing cycle multipliers (e.g., 0.95 for quarterly, 0.90 for half-yearly, 0.85 for yearly, 0.80 for multi-year)
- **FR-013**: System MUST allow admins to configure custom multipliers for each billing cycle
- **FR-014**: System MUST calculate monthly equivalent price for display purposes

#### Add-On Service Pricing
- **FR-015**: System MUST support subscription add-ons with recurring monthly pricing
- **FR-016**: System MUST support one-time add-ons with fixed pricing
- **FR-017**: System MUST allow admins to configure whether an add-on is subscription or one-time

#### Custom Development Charge Pricing
- **FR-018**: System MUST support hourly pricing for custom development charges (Unit Rate × Hours = Amount)
- **FR-019**: System MUST support fixed pricing for custom development charges (Fixed Price = Amount)
- **FR-020**: System MUST support project-based pricing for custom development charges (Base Price + (Hours × Hourly Rate) = Amount)
- **FR-021**: System MUST allow admins to configure pricing model per custom development charge

#### Quotation Integration
- **FR-022**: System MUST allow sales reps to select products from catalog when creating quotations
- **FR-023**: System MUST allow sales reps to search and filter products by name, category, or type when adding to quotations
- **FR-024**: System MUST allow sales reps to adjust quantity for subscription products (number of users)
- **FR-025**: System MUST allow sales reps to select billing cycle for subscription products (quarterly, half-yearly, yearly, multi-year)
- **FR-026**: System MUST allow sales reps to enter hours for hourly custom development charges
- **FR-027**: System MUST automatically calculate line item amounts when products are added or quantities changed
- **FR-028**: System MUST display product details (name, description, pricing breakdown) in quotation line items
- **FR-029**: System MUST allow sales reps to modify quantity, billing cycle, or hours for products already added to quotations
- **FR-030**: System MUST recalculate totals automatically when products are added, removed, or modified in quotations
- **FR-031**: System MUST allow sales reps to add custom line items (not from catalog) if needed

#### Discount Integration
- **FR-032**: System MUST support quotation-level discounts (integrates with Spec-012)
- **FR-033**: System MUST support product-level discounts
- **FR-034**: System MUST apply discounts before tax calculation
- **FR-035**: System MUST display discount breakdown in quotations

#### Tax Calculation Integration
- **FR-036**: System MUST automatically determine applicable tax based on client geography (integrates with Spec-020)
- **FR-037**: System MUST apply tax rates based on product/service categories
- **FR-038**: System MUST calculate taxes correctly for GST (India) with CGST and SGST components
- **FR-039**: System MUST calculate taxes correctly for VAT (UAE/Dubai)
- **FR-040**: System MUST display tax breakdown per product category in quotations
- **FR-041**: System MUST support tax-exempt or zero-rated products where configured
- **FR-042**: System MUST recalculate taxes automatically when client location or products change

#### Multi-Currency Support
- **FR-043**: System MUST support multi-currency pricing for products (integrates with Spec-017)
- **FR-044**: System MUST display product prices in quotation currency
- **FR-045**: System MUST convert product prices to quotation currency if different

#### Data Validation and Security
- **FR-046**: System MUST validate product prices are positive numbers
- **FR-047**: System MUST validate billing cycle multipliers are between 0 and 1 (for discounts)
- **FR-048**: System MUST validate quantities are positive numbers
- **FR-049**: System MUST validate hours are positive numbers for hourly development charges
- **FR-050**: System MUST prevent deletion of products that are in use by existing quotations
- **FR-051**: System MUST log all product pricing changes with user, timestamp, and change details
- **FR-052**: System MUST enforce RBAC - product management requires Admin role

### Key Entities

#### Product
- Represents a product, add-on service, or custom development charge in the catalog
- Key attributes: ProductId, ProductName, ProductType (enum: Subscription, AddOnSubscription, AddOnOneTime, CustomDevelopment), Description, CategoryId (FK), BasePricePerUserPerMonth (decimal, nullable), BillingCycleMultipliers (JSONB), AddOnPricing (JSONB), CustomDevelopmentPricing (JSONB), Currency, IsActive, CreatedAt, UpdatedAt
- Relationships: Belongs to ProductCategory, Has many QuotationLineItems, Has many ProductPriceHistory entries

#### ProductCategory
- Represents a category for organizing products
- Key attributes: CategoryId, CategoryName, CategoryCode, Description, ParentCategoryId (FK, nullable for hierarchy), IsActive, CreatedAt, UpdatedAt
- Relationships: Has many Products, Optionally belongs to ParentCategory

#### ProductPriceHistory
- Represents historical pricing for products
- Key attributes: PriceHistoryId, ProductId (FK), PriceType (enum: BasePrice, AddOnPrice, etc.), PriceValue (decimal), EffectiveFrom (DATE), EffectiveTo (DATE, nullable), ChangedByUserId (FK), ChangedAt (TIMESTAMPTZ)
- Relationships: Belongs to Product, Belongs to User (ChangedBy)

#### QuotationLineItem (Extended)
- Extends existing QuotationLineItem entity to support product catalog
- Additional attributes: ProductId (FK, nullable - null if custom line item), BillingCycle (enum: Monthly, Quarterly, HalfYearly, Yearly, MultiYear, nullable), Hours (decimal, nullable for hourly development charges), OriginalProductPrice (decimal, nullable), DiscountAmount (decimal, nullable), TaxCategoryId (FK, nullable)
- Relationships: Optionally belongs to Product, Belongs to Quotation, Optionally belongs to ProductCategory (for tax)

---

## Success Criteria

### Measurable Outcomes

- **SC-001**: Admins can create a subscription product with all billing cycles configured in under 2 minutes
- **SC-002**: Admins can create an add-on service (subscription or one-time) in under 1 minute
- **SC-003**: Admins can create a custom development charge with pricing model in under 1 minute
- **SC-004**: Sales reps can add a product from catalog to quotation in under 30 seconds
- **SC-005**: System automatically calculates subscription product price correctly for all billing cycles (quarterly, half-yearly, yearly, multi-year) with 100% accuracy
- **SC-006**: System automatically calculates quotation totals (subtotal, discount, tax, total) correctly for 100% of test quotations
- **SC-007**: Tax calculations apply correctly based on client geography and product categories for 100% of test cases
- **SC-008**: Sales reps can create quotations 50% faster by using product catalog compared to manual entry
- **SC-009**: Zero pricing calculation errors reported in production for catalog products
- **SC-010**: 90% of quotations use products from catalog within 3 months of feature launch

### Backend Success Criteria
- ✅ All entities, DTOs, commands, queries implemented for product catalog management
- ✅ Product pricing calculation engine correctly handles all product types and billing cycles
- ✅ Quotation integration allows adding products from catalog with quantity and billing cycle adjustments
- ✅ Tax calculation integration works correctly with product categories and client geography
- ✅ Discount integration works correctly with products from catalog
- ✅ Multi-currency support works correctly for products (Spec-017 integration)
- ✅ All APIs functional with proper RBAC enforcement (admin-only for product management)
- ✅ Unit and integration tests pass with ≥85% coverage

### Frontend Success Criteria
- ✅ Admin interfaces for managing products, add-ons, and custom development charges built using TailAdmin
- ✅ Product catalog selector in quotation creation allows search, filter, and selection
- ✅ Product pricing breakdown displays correctly when adding products to quotations
- ✅ Quotation displays show product details, pricing breakdown, discounts, and taxes
- ✅ Automatic calculation and recalculation works correctly when quantities or billing cycles change
- ✅ Mobile responsive design for all product management pages
- ✅ Component and E2E tests pass with ≥80% coverage

### Integration Success Criteria
- ✅ Product catalog integrates seamlessly with quotation creation and editing workflows
- ✅ Discount calculations work correctly with products from catalog (Spec-012 integration)
- ✅ Tax calculations work correctly based on client geography and product categories (Spec-020 integration)
- ✅ Multi-currency pricing works correctly for products (Spec-017 integration)
- ✅ No existing quotation functionality broken by product catalog integration
- ✅ Full E2E workflows tested: Admin creates product → Sales rep adds to quotation → System calculates price → System applies discount → System calculates tax → Quotation displays totals correctly

---

## Assumptions

1. **Product Categories**: Product categories are assumed to be pre-configured or manageable through a separate category management feature. Product categories should align with tax categories (Spec-020) for accurate tax calculations.

2. **Billing Cycles**: Subscription products are billed per user per month, with billing cycles determining the commitment period. The base price is always per user per month, and billing cycle multipliers apply discounts for longer commitments.

3. **Multi-Year Billing**: "Multi-year" billing is assumed to be 2-5 years, with the multiplier applying to the entire commitment period. The exact duration can be configured per product or specified when adding to quotation.

4. **Custom Line Items**: Sales reps can still add custom line items (not from catalog) if needed for one-off services or products not yet in the catalog.

5. **Product Pricing Changes**: When product pricing changes, existing quotations retain their original pricing. Only new quotations use updated prices.

6. **Tax Calculation**: Tax calculation uses product categories to determine applicable tax rates based on client geography. If a product doesn't have a category or category doesn't have tax rules, default tax rules apply.

7. **Currency**: Product prices are stored in the product's currency. When added to a quotation, prices are converted to the quotation's currency using exchange rates (Spec-017).

8. **Discount Approval**: Large discounts may require approval workflows (Spec-012). Product-level discounts follow the same approval rules as quotation-level discounts.

9. **Product Disabling**: Disabled products cannot be added to new quotations but remain in existing quotations for historical accuracy.

10. **Admin Role**: Only users with Admin role can create, edit, or disable products. Sales reps can view products and add them to quotations.

---

## Technical Requirements

### Backend Requirements

#### Entities & Data Models
- Product entity (ProductId, ProductName, ProductType, Description, CategoryId FK, BasePricePerUserPerMonth, BillingCycleMultipliers JSONB, AddOnPricing JSONB, CustomDevelopmentPricing JSONB, Currency, IsActive, timestamps)
- ProductCategory entity (CategoryId, CategoryName, CategoryCode, Description, ParentCategoryId FK, IsActive, timestamps)
- ProductPriceHistory entity (PriceHistoryId, ProductId FK, PriceType, PriceValue, EffectiveFrom, EffectiveTo, ChangedByUserId FK, ChangedAt)
- Extend QuotationLineItem entity: Add ProductId FK, BillingCycle enum, Hours decimal, OriginalProductPrice decimal, DiscountAmount decimal, TaxCategoryId FK

#### Services
- ProductCatalogService: Core service for managing products, add-ons, and custom development charges
- ProductPricingService: Service for calculating product prices based on billing cycles and quantities
- QuotationProductService: Service for integrating products with quotations
- ProductUsageService: Service for tracking product usage statistics

#### APIs
- `GET /api/v1/products` - List all products (with filters)
- `POST /api/v1/products` - Create product (admin only)
- `GET /api/v1/products/{productId}` - Get product details
- `PUT /api/v1/products/{productId}` - Update product (admin only)
- `DELETE /api/v1/products/{productId}` - Delete/disable product (admin only)
- `GET /api/v1/products/catalog` - Get product catalog for quotation selector
- `POST /api/v1/products/calculate-price` - Calculate product price for given quantity and billing cycle
- `GET /api/v1/products/{productId}/usage` - Get product usage statistics (admin only)
- `GET /api/v1/product-categories` - List product categories
- `POST /api/v1/product-categories` - Create product category (admin only)
- `PUT /api/v1/quotations/{quotationId}/line-items/product` - Add product to quotation
- `PUT /api/v1/quotations/{quotationId}/line-items/{lineItemId}` - Update line item (quantity, billing cycle, hours)

### Frontend Requirements (TailAdmin Next.js Theme - MANDATORY)

#### Pages
- `/products/catalog` - Product catalog management page (admin only)
- `/products/catalog/new` - Create product page (admin only)
- `/products/catalog/[productId]` - Product details and edit page (admin only)
- `/products/categories` - Product category management page (admin only)

#### Components
- ProductCatalogTable - Table for listing and managing products
- ProductForm - Form for creating/editing products (supports all product types)
- ProductSelector - Component for selecting products from catalog when creating quotations
- ProductPricingCalculator - Component showing price breakdown when adding products
- BillingCycleSelector - Component for selecting billing cycles for subscription products
- ProductCategoryTree - Tree view for product categories
- ProductUsageStats - Component showing product usage statistics

#### Integration
- Update QuotationCreateForm to include product catalog selector
- Update QuotationEditForm to allow adding/removing products and modifying quantities/billing cycles
- Update QuotationLineItemsTable to show product details and pricing breakdown
- Update QuotationTotalsDisplay to show discount and tax breakdown per product category

---

## Security & Compliance

- All product management endpoints enforce RBAC (Admin role required)
- Product pricing changes are logged with user, timestamp, and change details
- Product usage statistics available for business insights
- Product prices validated to prevent negative or invalid values
- Soft delete for products in use to maintain quotation data integrity
- Admin confirmation required for disabling products in use

---

## Performance Considerations

- Product catalog queries optimized with proper indexes on ProductType, CategoryId, IsActive
- Product price calculations cached for frequently accessed products
- Product selector search and filter optimized for fast results
- Quotation total calculations optimized to recalculate only when necessary

---

## Scalability

- Support for unlimited products, add-ons, and custom development charges
- Efficient product catalog queries with indexed searches
- Product usage statistics computed efficiently with proper aggregation
- Horizontal scaling support for product catalog service

---

## Future Enhancements

- Product variants (e.g., different storage tiers for same product)
- Product bundles (combine multiple products into packages)
- Tiered pricing (different prices based on quantity thresholds)
- Volume discounts (automatic discounts for larger quantities)
- Product recommendations based on client history
- Product approval workflows for new products
- Product versioning and change tracking
- Integration with inventory management (if applicable)
- Product reviews and ratings (if applicable)
- Automated price updates based on market conditions

---

**End of Spec-021: Product Catalog & Pricing Management**

