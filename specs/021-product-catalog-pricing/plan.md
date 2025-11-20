# Implementation Plan: Product Catalog & Pricing Management (Spec-021)

**Branch**: `021-product-catalog-pricing` | **Date**: 2025-01-27 | **Spec**: [spec.md](./spec.md)  
**Input**: Feature specification from `/specs/021-product-catalog-pricing/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

This specification introduces a comprehensive product catalog and pricing management system that enables administrators to create and manage products, add-on services, and custom development charges. The system supports subscription-based products with flexible billing cycles (per user per month, billed quarterly, half-yearly, yearly, or multi-year), add-on services available as subscriptions or one-time charges, and custom development charges with flexible pricing models (hourly, fixed, or project-based). Products from the catalog can be added to quotations with quantity adjustments, automatic price calculations, discounts, and geography-based tax calculations (GST or local taxes).

The technical approach involves creating new entities for Product, ProductCategory, and ProductPriceHistory; extending QuotationLineItem to support product catalog integration; implementing pricing calculation services for all product types and billing cycles; building admin interfaces for product management; and integrating product selection into quotation creation workflows.

## Technical Context

**Language/Version**: C# 12 / .NET 8 Web API (Backend), TypeScript / Next.js 16 (Frontend)  
**Primary Dependencies**: 
- ASP.NET Core 8.0
- Entity Framework Core 8.0 (PostgreSQL provider)
- MediatR (CQRS pattern)
- AutoMapper (for DTO mapping)
- FluentValidation (for request validation)
- Serilog (for logging)
- React Query, React Hook Form, Tailwind CSS, TailAdmin (Frontend)

**Storage**: PostgreSQL 14+ with UUID PKs, DECIMAL for financial amounts, JSONB for flexible pricing configurations, proper indexes  
**Testing**: xUnit + FluentAssertions (backend unit), WebApplicationFactory (integration), React Testing Library + Jest (frontend), Cypress/Playwright (E2E)  
**Target Platform**: Containerized Linux backend API (Kestrel) + Next.js frontend (Node.js)  
**Project Type**: Multi-project Clean Architecture backend + Next.js frontend mono-repo  
**Performance Goals**: 
- Product catalog queries load in <2s @1000 products
- Product price calculation completes in <100ms
- Product selector search/filter completes in <500ms
- Quotation total recalculation completes in <200ms

**Constraints**: 
- Must integrate seamlessly with existing quotation creation/update workflows (Spec-009)
- Must not break existing quotation functionality
- Product pricing changes must not affect existing quotations (historical pricing preserved)
- Must support discount integration (Spec-012)
- Must support tax calculation integration (Spec-020)
- Must support multi-currency pricing (Spec-017)
- Only admins can manage products (RBAC enforced)
- Products in use by quotations cannot be deleted (soft delete or disable)

**Scale/Scope**: 
- Support for unlimited products, add-ons, and custom development charges
- Expected 100-1000 products in catalog
- Expected 1000-10000 quotations per month using products from catalog
- 1-50 line items per quotation, each potentially from product catalog
- Product price calculations on every quotation create/update with products

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Based on the architecture patterns established in the CRM project, the following principles must be verified:

### I. Architecture Compliance
- ✅ Clean Architecture pattern maintained (Domain, Application, Infrastructure, API layers)
- ✅ Entity Framework Core used for data access (consistent with existing codebase)
- ✅ CQRS pattern maintained (Commands, Queries, Handlers via MediatR)
- ✅ AutoMapper used for entity-to-DTO mapping (consistent with existing codebase)
- ✅ FluentValidation used for request validation (consistent with existing codebase)

### II. Testing Requirements
- ⚠️ Unit tests required for product pricing calculation logic
- ⚠️ Integration tests required for product catalog service and API endpoints
- ⚠️ Integration tests required for quotation integration with products
- ⚠️ Test coverage target: ≥85% for backend, ≥80% for frontend

### III. Security & Authorization
- ✅ RBAC enforced - Admin role required for all product management endpoints
- ✅ Audit logging required for all product pricing changes
- ✅ Input validation required for all product prices, quantities, billing cycles
- ✅ Product prices validated to prevent negative or invalid values

### IV. Data Integrity
- ✅ Soft delete for products in use to maintain quotation data integrity
- ✅ Foreign key constraints enforced for referential integrity
- ✅ Product pricing history tracked with effective dates
- ✅ Historical pricing preserved for existing quotations

### V. Performance & Scalability
- ✅ Database indexes required on ProductType, CategoryId, IsActive, ProductId
- ✅ Product price calculations cached for frequently accessed products
- ✅ Product catalog queries optimized with proper indexes
- ✅ Efficient queries for product selector search and filter

### VI. Integration Requirements
- ✅ Must integrate with Spec-009 (Quotation CRUD) - extend QuotationLineItem
- ✅ Must integrate with Spec-012 (Discount Approval Workflow) - product-level discounts
- ✅ Must integrate with Spec-020 (Multi-Country Tax Management) - tax calculation by product category
- ✅ Must integrate with Spec-017 (Multi-Currency) - multi-currency pricing support

**Gate Status**: ✅ PASSING - All constitutional requirements can be met with standard patterns already in use. Integration points are clearly defined in dependencies.

## Project Structure

### Documentation (this feature)

```text
specs/021-product-catalog-pricing/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
│   └── products.openapi.yaml
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/Backend/
├── CRM.Domain/
│   ├── Entities/
│   │   ├── Product.cs                      # NEW: Product entity
│   │   ├── ProductCategory.cs              # NEW: ProductCategory entity
│   │   ├── ProductPriceHistory.cs          # NEW: ProductPriceHistory entity
│   │   └── QuotationLineItem.cs            # MODIFY: Add ProductId FK, BillingCycle, Hours, OriginalProductPrice, DiscountAmount, TaxCategoryId
│   └── Enums/
│       ├── ProductType.cs                  # NEW: Subscription, AddOnSubscription, AddOnOneTime, CustomDevelopment
│       └── BillingCycle.cs                 # NEW: Monthly, Quarterly, HalfYearly, Yearly, MultiYear
│
├── CRM.Application/
│   ├── Products/
│   │   ├── Commands/
│   │   │   ├── CreateProductCommand.cs
│   │   │   ├── UpdateProductCommand.cs
│   │   │   ├── DeleteProductCommand.cs
│   │   │   ├── CreateProductCategoryCommand.cs
│   │   │   ├── UpdateProductCategoryCommand.cs
│   │   │   └── AddProductToQuotationCommand.cs
│   │   ├── Commands/Handlers/
│   │   │   ├── CreateProductCommandHandler.cs
│   │   │   ├── UpdateProductCommandHandler.cs
│   │   │   ├── DeleteProductCommandHandler.cs
│   │   │   ├── CreateProductCategoryCommandHandler.cs
│   │   │   ├── UpdateProductCategoryCommandHandler.cs
│   │   │   └── AddProductToQuotationCommandHandler.cs
│   │   ├── Queries/
│   │   │   ├── GetProductsQuery.cs
│   │   │   ├── GetProductByIdQuery.cs
│   │   │   ├── GetProductCatalogQuery.cs
│   │   │   ├── GetProductCategoriesQuery.cs
│   │   │   ├── GetProductUsageStatsQuery.cs
│   │   │   └── CalculateProductPriceQuery.cs
│   │   ├── Queries/Handlers/
│   │   │   ├── GetProductsQueryHandler.cs
│   │   │   ├── GetProductByIdQueryHandler.cs
│   │   │   ├── GetProductCatalogQueryHandler.cs
│   │   │   ├── GetProductCategoriesQueryHandler.cs
│   │   │   ├── GetProductUsageStatsQueryHandler.cs
│   │   │   └── CalculateProductPriceQueryHandler.cs
│   │   ├── DTOs/
│   │   │   ├── ProductDto.cs
│   │   │   ├── ProductCategoryDto.cs
│   │   │   ├── ProductCatalogItemDto.cs
│   │   │   ├── ProductPriceCalculationDto.cs
│   │   │   └── ProductUsageStatsDto.cs
│   │   ├── Requests/
│   │   │   ├── CreateProductRequest.cs
│   │   │   ├── UpdateProductRequest.cs
│   │   │   ├── CreateProductCategoryRequest.cs
│   │   │   ├── UpdateProductCategoryRequest.cs
│   │   │   └── AddProductToQuotationRequest.cs
│   │   ├── Validators/
│   │   │   ├── CreateProductRequestValidator.cs
│   │   │   ├── UpdateProductRequestValidator.cs
│   │   │   ├── CreateProductCategoryRequestValidator.cs
│   │   │   ├── UpdateProductCategoryRequestValidator.cs
│   │   │   └── AddProductToQuotationRequestValidator.cs
│   │   └── Services/
│   │       ├── IProductCatalogService.cs
│   │       ├── ProductCatalogService.cs
│   │       ├── IProductPricingService.cs
│   │       ├── ProductPricingService.cs
│   │       ├── IQuotationProductService.cs
│   │       └── QuotationProductService.cs
│   │
│   └── Quotations/
│       └── Commands/
│           └── UpdateQuotationLineItemCommand.cs  # MODIFY: Support product updates
│
├── CRM.Infrastructure/
│   ├── EntityConfigurations/
│   │   ├── ProductEntityConfiguration.cs
│   │   ├── ProductCategoryEntityConfiguration.cs
│   │   └── ProductPriceHistoryEntityConfiguration.cs
│   ├── Persistence/
│   │   └── AppDbContext.cs                    # MODIFY: Add DbSets for Products, ProductCategories, ProductPriceHistory
│   └── Migrations/
│       └── [timestamp]_AddProductCatalogTables.cs
│
└── CRM.Api/
    └── Controllers/
        ├── ProductsController.cs              # NEW: Product management endpoints
        └── ProductCategoriesController.cs    # NEW: Product category management endpoints

src/Frontend/web/
├── src/app/(protected)/
│   ├── products/
│   │   ├── catalog/
│   │   │   ├── page.tsx                       # NEW: Product catalog list
│   │   │   ├── new/page.tsx                   # NEW: Create product
│   │   │   └── [productId]/page.tsx            # NEW: Product details/edit
│   │   └── categories/
│   │       └── page.tsx                       # NEW: Product category management
│   └── quotations/
│       └── [quotationId]/
│           └── edit/page.tsx                  # MODIFY: Add product selector
│
├── src/components/
│   ├── products/
│   │   ├── ProductCatalogTable.tsx            # NEW: Product listing table
│   │   ├── ProductForm.tsx                    # NEW: Product create/edit form
│   │   ├── ProductSelector.tsx                # NEW: Product selector for quotations
│   │   ├── ProductPricingCalculator.tsx       # NEW: Price breakdown display
│   │   ├── BillingCycleSelector.tsx           # NEW: Billing cycle selector
│   │   ├── ProductCategoryTree.tsx            # NEW: Category tree view
│   │   └── ProductUsageStats.tsx              # NEW: Usage statistics
│   └── quotations/
│       ├── QuotationLineItemsTable.tsx        # MODIFY: Show product details
│       └── QuotationTotalsDisplay.tsx         # MODIFY: Show product category tax breakdown
│
└── src/lib/
    └── api.ts                                 # MODIFY: Add ProductsApi methods
```

**Structure Decision**: Continue leveraging existing Clean Architecture solution (CRM.Api ⇄ CRM.Application ⇄ CRM.Domain ⇄ CRM.Infrastructure) with contracts/tests mirroring backend modules. Frontend implementation is CRITICAL for this spec and must be built alongside backend using TailAdmin components, React Query for state management, and real-time price calculation. Product catalog integration extends existing quotation workflows without breaking changes.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | All principles satisfied | N/A |

**Structure Decision**: Continue leveraging existing Clean Architecture solution with product catalog as a new feature module. JSONB storage for flexible pricing configurations (billing cycle multipliers, add-on pricing, custom development pricing) is necessary to support the variety of product types without creating multiple tables. Integration with existing quotation system requires extending QuotationLineItem entity, which is a standard pattern for feature extensions.

## Implementation Phases

1. **Phase 0: Research & Technical Decisions** - Resolve pricing calculation algorithms, JSONB schema design, billing cycle multiplier logic, integration patterns with quotations
2. **Phase 1: Design & Contracts** - Data model design, API contracts, quickstart guide
3. **Phase 2: Backend Foundation** - Entities, DTOs, migrations, validators, pricing calculation services
4. **Phase 3: Backend CRUD** - Commands, queries, handlers, controllers for product management
5. **Phase 4: Quotation Integration** - Extend QuotationLineItem, integrate product selection, update quotation services
6. **Phase 5: Frontend Implementation** - Product management pages, product selector component, quotation integration
7. **Phase 6: Testing & Validation** - Unit tests, integration tests, E2E tests, performance validation

---

**Next Steps**: Proceed to Phase 0 (Research) to resolve technical decisions, then Phase 1 (Design & Contracts) to generate data model, API contracts, and quickstart guide.

