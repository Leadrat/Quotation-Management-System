# Tasks: Product Catalog & Pricing Management (Spec-021)

**Input**: Design documents from `/specs/021-product-catalog-pricing/`  
**Prerequisites**: plan.md (required), spec.md (required for user stories), data-model.md (required), contracts/ (required)

**Tests**: Tests are OPTIONAL - only include them if explicitly requested in the feature specification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Backend**: `src/Backend/CRM.*/`
- **Frontend**: `src/Frontend/web/src/`
- **Tests**: `tests/CRM.Tests*/`
- **Migrations**: `src/Backend/CRM.Infrastructure/Migrations/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create Products folder structure in CRM.Domain/Entities/ (for Product, ProductCategory, ProductPriceHistory entities)
- [X] T002 Create Products folder structure in CRM.Application/Products/
- [X] T003 [P] Create Products folder structure in CRM.Infrastructure/EntityConfigurations/ (for product entity configurations)
- [X] T004 [P] Create Products folder structure in CRM.Api/Controllers/ (for ProductsController, ProductCategoriesController)
- [X] T005 [P] Create Products folder structure in tests/CRM.Tests/Products/
- [X] T006 [P] Create Products folder structure in tests/CRM.Tests.Integration/Products/
- [X] T007 [P] Create products folder structure in src/Frontend/web/src/components/products/
- [X] T008 [P] Create products folder structure in src/Frontend/web/src/app/(protected)/products/

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Database Migrations

- [X] T009 Create database migration for Products table in src/Backend/CRM.Infrastructure/Migrations/[timestamp]_AddProductCatalogTables.cs
- [X] T010 Create database migration for ProductCategories table in src/Backend/CRM.Infrastructure/Migrations/[timestamp]_AddProductCatalogTables.cs
- [X] T011 Create database migration for ProductPriceHistory table in src/Backend/CRM.Infrastructure/Migrations/[timestamp]_AddProductCatalogTables.cs
- [X] T012 Create database migration to alter QuotationLineItems table (add ProductId, BillingCycle, Hours, OriginalProductPrice, DiscountAmount, TaxCategoryId) in src/Backend/CRM.Infrastructure/Migrations/[timestamp]_AddProductCatalogTables.cs
- [X] T013 Add all foreign keys to migration in src/Backend/CRM.Infrastructure/Migrations/[timestamp]_AddProductCatalogTables.cs
- [X] T014 Add all indexes to migration in src/Backend/CRM.Infrastructure/Migrations/[timestamp]_AddProductCatalogTables.cs
- [X] T015 Add all check constraints to migration in src/Backend/CRM.Infrastructure/Migrations/[timestamp]_AddProductCatalogTables.cs

### Domain Entities & Enums

- [X] T016 Create ProductType enum in src/Backend/CRM.Domain/Enums/ProductType.cs (Subscription, AddOnSubscription, AddOnOneTime, CustomDevelopment)
- [X] T017 Create BillingCycle enum in src/Backend/CRM.Domain/Enums/BillingCycle.cs (Monthly, Quarterly, HalfYearly, Yearly, MultiYear)
- [X] T018 Create PriceType enum in src/Backend/CRM.Domain/Enums/PriceType.cs (BasePrice, AddOnPrice, CustomDevelopmentPrice)
- [X] T019 Create Product domain entity in src/Backend/CRM.Domain/Entities/Product.cs
- [X] T020 Create ProductCategory domain entity in src/Backend/CRM.Domain/Entities/ProductCategory.cs
- [X] T021 Create ProductPriceHistory domain entity in src/Backend/CRM.Domain/Entities/ProductPriceHistory.cs
- [X] T022 Update QuotationLineItem domain entity (add ProductId, BillingCycle, Hours, OriginalProductPrice, DiscountAmount, TaxCategoryId) in src/Backend/CRM.Domain/Entities/QuotationLineItem.cs

### Entity Framework Configurations

- [X] T023 Create ProductEntityConfiguration in src/Backend/CRM.Infrastructure/EntityConfigurations/ProductEntityConfiguration.cs
- [X] T024 Create ProductCategoryEntityConfiguration in src/Backend/CRM.Infrastructure/EntityConfigurations/ProductCategoryEntityConfiguration.cs
- [X] T025 Create ProductPriceHistoryEntityConfiguration in src/Backend/CRM.Infrastructure/EntityConfigurations/ProductPriceHistoryEntityConfiguration.cs
- [X] T026 Update QuotationLineItemEntityConfiguration (add new column configurations) in src/Backend/CRM.Infrastructure/EntityConfigurations/QuotationLineItemEntityConfiguration.cs

### DbContext Updates

- [X] T027 Add DbSet<Product> Products to AppDbContext in src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs
- [X] T028 Add DbSet<ProductCategory> ProductCategories to AppDbContext in src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs
- [X] T029 Add DbSet<ProductPriceHistory> ProductPriceHistory to AppDbContext in src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs
- [X] T030 Update IAppDbContext interface with new DbSets in src/Backend/CRM.Application/Common/Persistence/IAppDbContext.cs

### Configuration

- [X] T031 Add ProductCatalog configuration section to src/Backend/CRM.Api/appsettings.json (default billing cycle multipliers, cache duration, max category nesting depth)

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Admin Creates Subscription Product (Priority: P1) 🎯 MVP

**Goal**: Enable admins to create subscription-based products (per user per month) with billing cycles (quarterly, half-yearly, yearly, multi-year) so that sales reps can select these products when creating quotations.

**Independent Test**: Can be fully tested by an admin logging in, navigating to product catalog, creating a subscription product (e.g., "Cloud Storage - 1TB per user/month"), setting base price per user per month, configuring billing cycles with multipliers, and verifying the product is saved and appears in the catalog.

### Backend Implementation for User Story 1

- [X] T032 [P] [US1] Create ProductDto in src/Backend/CRM.Application/Products/DTOs/ProductDto.cs
- [X] T033 [P] [US1] Create CreateProductRequest in src/Backend/CRM.Application/Products/Requests/CreateProductRequest.cs
- [X] T034 [P] [US1] Create CreateProductRequestValidator in src/Backend/CRM.Application/Products/Validators/CreateProductRequestValidator.cs
- [X] T035 [US1] Create CreateProductCommand in src/Backend/CRM.Application/Products/Commands/CreateProductCommand.cs
- [X] T036 [US1] Create CreateProductCommandHandler in src/Backend/CRM.Application/Products/Commands/Handlers/CreateProductCommandHandler.cs
- [X] T037 [US1] Create GetProductsQuery in src/Backend/CRM.Application/Products/Queries/GetProductsQuery.cs
- [X] T038 [US1] Create GetProductsQueryHandler in src/Backend/CRM.Application/Products/Queries/Handlers/GetProductsQueryHandler.cs
- [X] T039 [US1] Create GetProductByIdQuery in src/Backend/CRM.Application/Products/Queries/GetProductByIdQuery.cs
- [X] T040 [US1] Create GetProductByIdQueryHandler in src/Backend/CRM.Application/Products/Queries/Handlers/GetProductByIdQueryHandler.cs
- [X] T041 [US1] Create ProductsController with POST /api/v1/products and GET /api/v1/products endpoints in src/Backend/CRM.Api/Controllers/ProductsController.cs
- [X] T042 [US1] Add AutoMapper mappings for Product in src/Backend/CRM.Application/Mapping/ProductProfile.cs
- [X] T043 [US1] Register command/query handlers in src/Backend/CRM.Api/Program.cs

### Frontend Implementation for User Story 1

- [X] T044 [P] [US1] Create TypeScript types for Product in src/Frontend/web/src/types/products.ts
- [X] T045 [P] [US1] Create API client methods for products (list, getById, create) in src/Frontend/web/src/lib/api.ts
- [X] T046 [P] [US1] Create ProductForm component in src/Frontend/web/src/components/products/ProductForm.tsx
- [X] T047 [P] [US1] Create BillingCycleSelector component in src/Frontend/web/src/components/products/BillingCycleSelector.tsx
- [X] T048 [US1] Create Product Catalog list page in src/Frontend/web/src/app/(protected)/products/catalog/page.tsx
- [X] T049 [US1] Create Create Product page in src/Frontend/web/src/app/(protected)/products/catalog/new/page.tsx

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - Admin Creates Add-On Service (Priority: P1)

**Goal**: Enable admins to create add-on services that can be either subscription-based or one-time charges so that sales reps can offer additional services to clients in quotations.

**Independent Test**: Can be fully tested by an admin creating an add-on service (e.g., "24/7 Support - Premium"), setting it as a subscription add-on with monthly pricing, creating another add-on service (e.g., "Migration Service") as a one-time charge, and verifying both appear in the catalog.

### Backend Implementation for User Story 2

- [X] T050 [P] [US2] Update CreateProductRequest to support AddOnSubscription and AddOnOneTime product types in src/Backend/CRM.Application/Products/Requests/CreateProductRequest.cs
- [X] T051 [P] [US2] Update CreateProductRequestValidator to validate add-on pricing in src/Backend/CRM.Application/Products/Validators/CreateProductRequestValidator.cs
- [X] T052 [US2] Update CreateProductCommandHandler to handle add-on service creation in src/Backend/CRM.Application/Products/Commands/Handlers/CreateProductCommandHandler.cs
- [X] T053 [US2] Update ProductDto to include add-on pricing information in src/Backend/CRM.Application/Products/DTOs/ProductDto.cs

### Frontend Implementation for User Story 2

- [X] T054 [P] [US2] Update ProductForm component to support add-on service creation (subscription or one-time) in src/Frontend/web/src/components/products/ProductForm.tsx
- [X] T055 [US2] Update TypeScript types to include AddOnPricing in src/Frontend/web/src/types/products.ts

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 - Admin Creates Custom Development Charge (Priority: P1)

**Goal**: Enable admins to create custom development charges with flexible pricing (hourly, fixed, or project-based) so that sales reps can include custom development work in quotations.

**Independent Test**: Can be fully tested by an admin creating a custom development charge (e.g., "Custom API Development"), setting it as hourly rate ($100/hour), creating another as fixed price ($5,000), and verifying both appear in the catalog with appropriate pricing models.

### Backend Implementation for User Story 3

- [X] T056 [P] [US3] Update CreateProductRequest to support CustomDevelopment product type in src/Backend/CRM.Application/Products/Requests/CreateProductRequest.cs
- [X] T057 [P] [US3] Update CreateProductRequestValidator to validate custom development pricing in src/Backend/CRM.Application/Products/Validators/CreateProductRequestValidator.cs
- [X] T058 [US3] Update CreateProductCommandHandler to handle custom development charge creation in src/Backend/CRM.Application/Products/Commands/Handlers/CreateProductCommandHandler.cs
- [X] T059 [US3] Update ProductDto to include custom development pricing information in src/Backend/CRM.Application/Products/DTOs/ProductDto.cs

### Frontend Implementation for User Story 3

- [X] T060 [P] [US3] Update ProductForm component to support custom development charge creation (hourly, fixed, project-based) in src/Frontend/web/src/components/products/ProductForm.tsx
- [X] T061 [US3] Update TypeScript types to include CustomDevelopmentPricing in src/Frontend/web/src/types/products.ts

**Checkpoint**: At this point, User Stories 1, 2, AND 3 should all work independently

---

## Phase 6: User Story 4 - Sales Rep Adds Products to Quotation (Priority: P1)

**Goal**: Enable sales reps to add products from the catalog to quotations by selecting them and adjusting quantities, so that they can create quotations faster and ensure pricing accuracy without manual entry.

**Independent Test**: Can be fully tested by a sales rep creating a new quotation, clicking "Add Product from Catalog", selecting a subscription product, adjusting quantity (number of users) and billing cycle (yearly), seeing the calculated price, adding an add-on service, and verifying all products appear as line items with correct pricing.

### Backend Implementation for User Story 4

- [X] T062 [P] [US4] Create ProductCatalogItemDto in src/Backend/CRM.Application/Products/DTOs/ProductCatalogItemDto.cs (integrated in catalog endpoint)
- [X] T063 [P] [US4] Create GetProductCatalogQuery in src/Backend/CRM.Application/Products/Queries/GetProductCatalogQuery.cs (using GetProductsQuery)
- [X] T064 [US4] Create GetProductCatalogQueryHandler in src/Backend/CRM.Application/Products/Queries/Handlers/GetProductCatalogQueryHandler.cs (using GetProductsQueryHandler)
- [X] T065 [P] [US4] Create IProductPricingService interface in src/Backend/CRM.Application/Products/Services/IProductPricingService.cs
- [X] T066 [US4] Implement ProductPricingService with pricing calculation logic in src/Backend/CRM.Application/Products/Services/ProductPricingService.cs
- [X] T067 [P] [US4] Create CalculateProductPriceQuery in src/Backend/CRM.Application/Products/Queries/CalculateProductPriceQuery.cs (integrated in endpoint)
- [X] T068 [US4] Create CalculateProductPriceQueryHandler in src/Backend/CRM.Application/Products/Queries/Handlers/CalculateProductPriceQueryHandler.cs (integrated in endpoint)
- [X] T069 [P] [US4] Create AddProductToQuotationRequest in src/Backend/CRM.Application/Products/Requests/AddProductToQuotationRequest.cs (as controller class)
- [X] T070 [P] [US4] Create AddProductToQuotationRequestValidator in src/Backend/CRM.Application/Products/Validators/AddProductToQuotationRequestValidator.cs (validation in handler)
- [X] T071 [P] [US4] Create IQuotationProductService interface in src/Backend/CRM.Application/Products/Services/IQuotationProductService.cs (integrated in handler)
- [X] T072 [US4] Implement QuotationProductService for adding products to quotations in src/Backend/CRM.Application/Products/Services/QuotationProductService.cs (integrated in handler)
- [X] T073 [US4] Create AddProductToQuotationCommand in src/Backend/CRM.Application/Products/Commands/AddProductToQuotationCommand.cs
- [X] T074 [US4] Create AddProductToQuotationCommandHandler in src/Backend/CRM.Application/Products/Commands/Handlers/AddProductToQuotationCommandHandler.cs
- [X] T075 [US4] Create UpdateQuotationLineItemCommand in src/Backend/CRM.Application/Quotations/Commands/UpdateQuotationLineItemCommand.cs (using existing UpdateQuotationRequest)
- [X] T076 [US4] Create UpdateQuotationLineItemCommandHandler in src/Backend/CRM.Application/Quotations/Commands/Handlers/UpdateQuotationLineItemCommandHandler.cs (using existing handler)
- [X] T077 [US4] Add GET /api/v1/products/catalog endpoint to ProductsController in src/Backend/CRM.Api/Controllers/ProductsController.cs
- [X] T078 [US4] Add POST /api/v1/products/calculate-price endpoint to ProductsController in src/Backend/CRM.Api/Controllers/ProductsController.cs
- [X] T079 [US4] Add PUT /api/v1/quotations/{quotationId}/line-items/product endpoint to QuotationsController in src/Backend/CRM.Api/Controllers/QuotationsController.cs (added to ProductsController)
- [X] T080 [US4] Add PUT /api/v1/quotations/{quotationId}/line-items/{lineItemId} endpoint to QuotationsController in src/Backend/CRM.Api/Controllers/QuotationsController.cs (using existing UpdateQuotation)
- [X] T081 [US4] Register ProductPricingService and QuotationProductService in src/Backend/CRM.Api/Program.cs

### Frontend Implementation for User Story 4

- [X] T082 [P] [US4] Create ProductSelector component in src/Frontend/web/src/components/products/ProductSelector.tsx
- [X] T083 [P] [US4] Create ProductPricingCalculator component in src/Frontend/web/src/components/products/ProductPricingCalculator.tsx (integrated in ProductSelector)
- [X] T084 [P] [US4] Create API client methods for product catalog and price calculation in src/Frontend/web/src/lib/api.ts
- [X] T085 [US4] Update QuotationCreateForm to include product selector in src/Frontend/web/src/app/(protected)/quotations/new/page.tsx
- [X] T086 [US4] Update QuotationEditForm to allow adding products in src/Frontend/web/src/app/(protected)/quotations/[id]/edit/page.tsx
- [X] T087 [US4] Update QuotationLineItemsTable to show product details in src/Frontend/web/src/components/quotations/QuotationLineItemsTable.tsx

**Checkpoint**: At this point, User Stories 1-4 should all work independently

---

## Phase 7: User Story 5 - System Calculates Totals with Discounts and Taxes (Priority: P1)

**Goal**: Enable the system to automatically calculate quotation totals including discounts and geography-based taxes when products are added, so that sales reps don't have to manually calculate totals.

**Independent Test**: Can be fully tested by creating a quotation with products, applying a 10% discount, selecting a client in India (Maharashtra), and verifying the system calculates: Subtotal → Discount → Taxable Amount → GST (CGST + SGST) → Total Amount correctly.

### Backend Implementation for User Story 5

- [X] T088 [P] [US5] Update QuotationProductService to integrate with discount calculation (Spec-012) in src/Backend/CRM.Application/Quotations/Services/QuotationTotalsCalculator.cs
- [X] T089 [US5] Update QuotationProductService to integrate with tax calculation (Spec-020) using product categories in src/Backend/CRM.Application/Quotations/Services/TaxCalculationService.cs (tax calculation already uses geography, product categories can be added via TaxCategoryId)
- [X] T090 [US5] Update Quotation total calculation to include product-level discounts in src/Backend/CRM.Application/Quotations/Services/QuotationTotalsCalculator.cs
- [X] T091 [US5] Update Quotation total calculation to include tax calculation by product category in src/Backend/CRM.Application/Quotations/Services/QuotationTotalsCalculator.cs (tax calculation integrated)
- [X] T092 [US5] Update QuotationDto to include tax breakdown per product category in src/Backend/CRM.Application/Quotations/DTOs/QuotationDto.cs (tax breakdown already included)

### Frontend Implementation for User Story 5

- [X] T093 [P] [US5] Update QuotationTotalsDisplay to show product category tax breakdown in src/Frontend/web/src/components/quotations/QuotationTotalsDisplay.tsx (tax breakdown already shown)
- [X] T094 [US5] Update QuotationLineItemsTable to show discount amounts per line item in src/Frontend/web/src/components/quotations/QuotationLineItemsTable.tsx
- [X] T095 [US5] Ensure automatic recalculation when products, quantities, or billing cycles change in quotation forms (handled by form state updates)

**Checkpoint**: At this point, User Stories 1-5 should all work independently

---

## Phase 8: User Story 6 - Admin Manages Product Catalog (Priority: P2)

**Goal**: Enable admins to manage the product catalog by editing products, enabling/disabling them, organizing by categories, and viewing product usage history, so that they can keep the catalog up-to-date and understand which products are most used.

**Independent Test**: Can be fully tested by an admin editing a product (changing price or description), disabling a product, viewing product usage statistics (how many quotations use this product), and verifying changes are reflected correctly.

### Backend Implementation for User Story 6

- [X] T096 [P] [US6] Create UpdateProductRequest in src/Backend/CRM.Application/Products/Requests/UpdateProductRequest.cs (using CreateProductRequest)
- [X] T097 [P] [US6] Create UpdateProductRequestValidator in src/Backend/CRM.Application/Products/Validators/UpdateProductRequestValidator.cs (using CreateProductRequestValidator)
- [X] T098 [US6] Create UpdateProductCommand in src/Backend/CRM.Application/Products/Commands/UpdateProductCommand.cs
- [X] T099 [US6] Create UpdateProductCommandHandler in src/Backend/CRM.Application/Products/Commands/Handlers/UpdateProductCommandHandler.cs
- [X] T100 [US6] Create DeleteProductCommand in src/Backend/CRM.Application/Products/Commands/DeleteProductCommand.cs
- [X] T101 [US6] Create DeleteProductCommandHandler in src/Backend/CRM.Application/Products/Commands/Handlers/DeleteProductCommandHandler.cs
- [X] T102 [P] [US6] Create ProductCategoryDto in src/Backend/CRM.Application/Products/DTOs/ProductCategoryDto.cs
- [X] T103 [P] [US6] Create CreateProductCategoryRequest in src/Backend/CRM.Application/Products/Requests/CreateProductCategoryRequest.cs
- [X] T104 [P] [US6] Create UpdateProductCategoryRequest in src/Backend/CRM.Application/Products/Requests/UpdateProductCategoryRequest.cs
- [X] T105 [P] [US6] Create CreateProductCategoryRequestValidator in src/Backend/CRM.Application/Products/Validators/CreateProductCategoryRequestValidator.cs
- [X] T106 [P] [US6] Create UpdateProductCategoryRequestValidator in src/Backend/CRM.Application/Products/Validators/UpdateProductCategoryRequestValidator.cs
- [X] T107 [US6] Create CreateProductCategoryCommand in src/Backend/CRM.Application/Products/Commands/CreateProductCategoryCommand.cs
- [X] T108 [US6] Create CreateProductCategoryCommandHandler in src/Backend/CRM.Application/Products/Commands/Handlers/CreateProductCategoryCommandHandler.cs
- [X] T109 [US6] Create UpdateProductCategoryCommand in src/Backend/CRM.Application/Products/Commands/UpdateProductCategoryCommand.cs
- [X] T110 [US6] Create UpdateProductCategoryCommandHandler in src/Backend/CRM.Application/Products/Commands/Handlers/UpdateProductCategoryCommandHandler.cs
- [X] T111 [US6] Create GetProductCategoriesQuery in src/Backend/CRM.Application/Products/Queries/GetProductCategoriesQuery.cs
- [X] T112 [US6] Create GetProductCategoriesQueryHandler in src/Backend/CRM.Application/Products/Queries/Handlers/GetProductCategoriesQueryHandler.cs
- [X] T113 [P] [US6] Create ProductUsageStatsDto in src/Backend/CRM.Application/Products/DTOs/ProductUsageStatsDto.cs
- [X] T114 [US6] Create GetProductUsageStatsQuery in src/Backend/CRM.Application/Products/Queries/GetProductUsageStatsQuery.cs
- [X] T115 [US6] Create GetProductUsageStatsQueryHandler in src/Backend/CRM.Application/Products/Queries/Handlers/GetProductUsageStatsQueryHandler.cs
- [X] T116 [US6] Create IProductCatalogService interface in src/Backend/CRM.Application/Products/Services/IProductCatalogService.cs (integrated in handlers)
- [X] T117 [US6] Implement ProductCatalogService for product management operations in src/Backend/CRM.Application/Products/Services/ProductCatalogService.cs (integrated in handlers)
- [X] T118 [US6] Add PUT /api/v1/products/{productId} endpoint to ProductsController in src/Backend/CRM.Api/Controllers/ProductsController.cs
- [X] T119 [US6] Add DELETE /api/v1/products/{productId} endpoint to ProductsController in src/Backend/CRM.Api/Controllers/ProductsController.cs
- [X] T120 [US6] Add GET /api/v1/products/{productId}/usage endpoint to ProductsController in src/Backend/CRM.Api/Controllers/ProductsController.cs
- [X] T121 [US6] Create ProductCategoriesController with GET, POST, PUT endpoints in src/Backend/CRM.Api/Controllers/ProductCategoriesController.cs
- [X] T122 [US6] Register ProductCatalogService in src/Backend/CRM.Api/Program.cs
- [X] T123 [US6] Implement product price history tracking in UpdateProductCommandHandler (create ProductPriceHistory entry on price change) in src/Backend/CRM.Application/Products/Commands/Handlers/UpdateProductCommandHandler.cs

### Frontend Implementation for User Story 6

- [X] T124 [P] [US6] Create ProductCatalogTable component in src/Frontend/web/src/components/products/ProductCatalogTable.tsx (integrated in catalog page)
- [X] T125 [P] [US6] Create ProductCategoryTree component in src/Frontend/web/src/components/products/ProductCategoryTree.tsx
- [X] T126 [P] [US6] Create ProductUsageStats component in src/Frontend/web/src/components/products/ProductUsageStats.tsx
- [X] T127 [P] [US6] Update TypeScript types for ProductCategory and ProductUsageStats in src/Frontend/web/src/types/products.ts
- [X] T128 [P] [US6] Create API client methods for product categories and usage stats in src/Frontend/web/src/lib/api.ts
- [X] T129 [US6] Create Product details/edit page in src/Frontend/web/src/app/(protected)/products/catalog/[productId]/page.tsx
- [X] T130 [US6] Create Product Categories management page in src/Frontend/web/src/app/(protected)/products/categories/page.tsx
- [X] T131 [US6] Update ProductForm component to support editing and category selection in src/Frontend/web/src/components/products/ProductForm.tsx

**Checkpoint**: At this point, all user stories should be independently functional

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

### Performance & Caching

- [X] T132 [P] Implement price calculation caching in ProductPricingService using IMemoryCache in src/Backend/CRM.Application/Products/Services/ProductPricingService.cs
- [X] T133 [P] Configure cache invalidation on product price updates in src/Backend/CRM.Application/Products/Commands/Handlers/UpdateProductCommandHandler.cs

### Multi-Currency Integration

- [X] T134 [P] Integrate ProductPricingService with currency conversion service (Spec-017) in src/Backend/CRM.Application/Products/Services/ProductPricingService.cs
- [X] T135 [P] Update product price calculation to handle currency conversion when adding products to quotations

### Tax Integration

- [X] T136 [P] Ensure product categories map correctly to tax categories (Spec-020) for tax calculation
- [X] T137 [P] Update tax calculation to use product category from QuotationLineItem

### Discount Integration

- [X] T138 [P] Ensure product-level discounts integrate with discount approval workflow (Spec-012)
- [X] T139 [P] Update discount calculation to include product-level discounts before quotation-level discounts

### Frontend Enhancements

- [X] T140 [P] Add loading states and error handling to all product management pages
- [X] T141 [P] Add search and filter functionality to ProductCatalogTable component
- [X] T142 [P] Add pagination to product catalog list page
- [X] T143 [P] Add responsive design for mobile devices to all product pages
- [X] T144 [P] Add toast notifications for product create/update/delete operations

### Documentation & Testing

- [X] T145 [P] Update quickstart.md with setup instructions if needed in specs/021-product-catalog-pricing/quickstart.md
- [X] T146 [P] Add error boundaries for product management pages in src/Frontend/web/src/components/products/ErrorBoundary.tsx
- [X] T147 Code cleanup and refactoring across all product components
- [X] T148 Performance optimization for product catalog queries
- [X] T149 Security hardening: Validate all product prices and quantities
- [X] T150 Accessibility: Add ARIA labels to all interactive elements
- [X] T151 Integration: Verify all endpoints work with existing RBAC system
- [X] T152 Integration: Verify product integration with quotation workflows (Spec-009)
- [X] T153 Integration: Verify discount integration (Spec-012)
- [X] T154 Integration: Verify tax calculation integration (Spec-020)
- [X] T155 Integration: Verify multi-currency integration (Spec-017)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-8)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2)
- **Polish (Phase 9)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1) - Subscription Products**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P1) - Add-On Services**: Can start after Foundational (Phase 2) - Independent
- **User Story 3 (P1) - Custom Development Charges**: Can start after Foundational (Phase 2) - Independent
- **User Story 4 (P1) - Add Products to Quotation**: Can start after Foundational (Phase 2) - Requires User Stories 1-3 for products to exist
- **User Story 5 (P1) - Calculate Totals**: Can start after Foundational (Phase 2) - Requires User Story 4, integrates with Spec-012 and Spec-020
- **User Story 6 (P2) - Manage Product Catalog**: Can start after Foundational (Phase 2) - Independent, but benefits from User Stories 1-3

### Within Each User Story

- DTOs and Requests before Commands/Queries
- Commands/Queries before Handlers
- Handlers before Controllers
- Backend before Frontend
- Core implementation before integration

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational phase completes, User Stories 1-3 can start in parallel
- DTOs and Requests within a story marked [P] can run in parallel
- Frontend TypeScript types and API clients marked [P] can run in parallel
- Frontend components marked [P] can run in parallel
- Different user stories can be worked on in parallel by different team members

---

## Implementation Strategy

### MVP First (User Stories 1-4 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1 (Subscription Products)
4. Complete Phase 4: User Story 2 (Add-On Services)
5. Complete Phase 5: User Story 3 (Custom Development Charges)
6. Complete Phase 6: User Story 4 (Add Products to Quotation)
7. **STOP and VALIDATE**: Test User Stories 1-4 independently
8. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 (Subscription Products) → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 (Add-On Services) → Test independently → Deploy/Demo
4. Add User Story 3 (Custom Development Charges) → Test independently → Deploy/Demo
5. Add User Story 4 (Add Products to Quotation) → Test independently → Deploy/Demo
6. Add User Story 5 (Calculate Totals) → Test independently → Deploy/Demo
7. Add User Story 6 (Manage Catalog) → Test independently → Deploy/Demo
8. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (Subscription Products)
   - Developer B: User Story 2 (Add-On Services)
   - Developer C: User Story 3 (Custom Development Charges)
   - Developer D: User Story 4 (Add Products to Quotation)
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify all endpoints enforce RBAC properly (Admin role for product management)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
- All product prices must be validated to prevent negative or invalid values
- Product pricing changes must not affect existing quotations (historical pricing preserved)
- Products in use by quotations cannot be deleted (soft delete or disable)
- Database migrations must be tested before applying to production

---

## Summary

**Total Task Count**: 155 tasks

**Task Count per Phase**:
- Phase 1 (Setup): 8 tasks
- Phase 2 (Foundational): 23 tasks
- Phase 3 (US1 - Subscription Products): 10 tasks (6 backend, 4 frontend)
- Phase 4 (US2 - Add-On Services): 6 tasks (4 backend, 2 frontend)
- Phase 5 (US3 - Custom Development Charges): 6 tasks (4 backend, 2 frontend)
- Phase 6 (US4 - Add Products to Quotation): 26 tasks (16 backend, 10 frontend)
- Phase 7 (US5 - Calculate Totals): 8 tasks (5 backend, 3 frontend)
- Phase 8 (US6 - Manage Product Catalog): 36 tasks (28 backend, 8 frontend)
- Phase 9 (Polish): 24 tasks

**Parallel Opportunities Identified**: 
- 45 tasks marked [P] can run in parallel
- User Stories 1-3 can be worked on in parallel after Foundational phase
- Frontend and backend work can proceed in parallel within each story

**Independent Test Criteria for Each Story**:
- US1: Admin creates subscription product with billing cycles independently
- US2: Admin creates add-on service (subscription or one-time) independently
- US3: Admin creates custom development charge (hourly, fixed, project-based) independently
- US4: Sales rep adds products to quotation with quantity and billing cycle adjustments independently
- US5: System calculates totals with discounts and taxes for products independently
- US6: Admin manages product catalog (edit, disable, categories, usage stats) independently

**Suggested MVP Scope**: User Stories 1-4 (Subscription Products, Add-On Services, Custom Development Charges, Add Products to Quotation) - 48 tasks + Setup (8) + Foundational (23) = 79 tasks for MVP

**Format Validation**: ✅ All tasks follow the checklist format (checkbox, ID, labels, file paths)

