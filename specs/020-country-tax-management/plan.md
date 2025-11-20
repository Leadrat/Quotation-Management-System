# Implementation Plan: Multi-Country & Jurisdiction Tax Management (Spec-020)

**Branch**: `020-country-tax-management` | **Date**: 2025-01-27 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/020-country-tax-management/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

This specification extends the quotation management system to support multi-country and multi-jurisdiction tax management. Administrators can configure tax rules per country and jurisdiction (e.g., states, provinces, cities), with support for different tax types and rates based on product or service categories. Initially supporting India (GST) and UAE (Dubai VAT), the system provides flexible tax configuration capabilities to expand to additional countries and jurisdictions over time.

The technical approach involves creating new entities for Country, Jurisdiction, TaxFramework, TaxRate, ProductServiceCategory, and TaxCalculationLog; implementing a tax calculation service that automatically determines applicable taxes based on client location and item categories; and building admin interfaces for configuration management.

## Technical Context

**Language/Version**: C# 12 / .NET 8  
**Primary Dependencies**: 
- ASP.NET Core 8.0
- Entity Framework Core 8.0 (PostgreSQL provider)
- AutoMapper (for DTO mapping)
- FluentValidation (for request validation)
- QuestPDF (for PDF generation)
- BCrypt.Net (for password hashing)

**Storage**: PostgreSQL (via Entity Framework Core)  
**Testing**: xUnit, Moq, FluentAssertions  
**Target Platform**: Linux server (ASP.NET Core web API) + Next.js frontend  
**Project Type**: Web application (backend + frontend)  
**Performance Goals**: 
- Tax calculation completes in <1 second for quotations with up to 50 line items
- Admin tax configuration pages load in <2 seconds
- Tax rate lookups cached for frequently accessed jurisdictions and categories

**Constraints**: 
- Must integrate seamlessly with existing quotation creation/update workflows
- Must not break existing quotation functionality
- Tax calculations must be auditable and reversible
- Support for multi-component tax frameworks (GST with CGST/SGST, VAT with single component)
- Historical tax rate tracking with effective dates

**Scale/Scope**: 
- Support for unlimited countries and jurisdictions (with proper indexing)
- Support for unlimited tax rates and category rules
- Expected 100-1000 quotations per day with tax calculations
- 10-50 countries initially, expandable to 100+

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Based on the constitution file at `.specify/memory/constitution.md`, the following principles must be verified:

### I. Architecture Compliance
- ✅ Clean Architecture pattern maintained (Domain, Application, Infrastructure, API layers)
- ✅ Entity Framework Core used for data access (consistent with existing codebase)
- ✅ DTOs, Commands, Queries pattern maintained (CQRS-lite approach)
- ✅ AutoMapper used for entity-to-DTO mapping (consistent with existing codebase)

### II. Testing Requirements
- ⚠️ Unit tests required for tax calculation logic
- ⚠️ Integration tests required for tax calculation service and API endpoints
- ⚠️ Test coverage target: ≥85% for backend, ≥80% for frontend

### III. Security & Authorization
- ✅ RBAC enforced - Admin role required for all tax configuration endpoints
- ✅ Audit logging required for all tax configuration changes and calculations
- ✅ Input validation required for all tax rates, country codes, jurisdiction codes

### IV. Data Integrity
- ✅ Soft delete for countries/jurisdictions in use to maintain quotation data integrity
- ✅ Foreign key constraints enforced for referential integrity
- ✅ Unique constraints for country codes (ISO 3166-1 alpha-2) and jurisdiction codes

### V. Performance & Scalability
- ✅ Database indexes required on CountryId, JurisdictionId, ProductServiceCategoryId
- ✅ Tax rate lookups cached for frequently accessed jurisdictions
- ✅ Efficient queries for jurisdiction hierarchy traversal

**Gate Status**: ✅ PASSING - All constitutional requirements can be met with standard patterns already in use.

## Project Structure

### Documentation (this feature)

```text
specs/020-country-tax-management/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/Backend/
├── CRM.Domain/
│   ├── Entities/
│   │   ├── Country.cs                      # NEW: Country entity
│   │   ├── Jurisdiction.cs                 # NEW: Jurisdiction entity
│   │   ├── TaxFramework.cs                 # NEW: TaxFramework entity
│   │   ├── TaxRate.cs                      # NEW: TaxRate entity
│   │   ├── ProductServiceCategory.cs       # NEW: ProductServiceCategory entity
│   │   ├── TaxCalculationLog.cs            # NEW: TaxCalculationLog entity
│   │   ├── QuotationLineItem.cs            # MODIFY: Add CategoryId FK
│   │   └── Client.cs                       # MODIFY: Add CountryId FK, JurisdictionId FK
│   └── Enums/
│       ├── TaxFrameworkType.cs             # NEW: Enum for GST, VAT, etc.
│       └── TaxCalculationActionType.cs     # NEW: Enum for audit log
│
├── CRM.Application/
│   ├── TaxManagement/
│   │   ├── Commands/
│   │   │   ├── CreateCountryCommand.cs
│   │   │   ├── UpdateCountryCommand.cs
│   │   │   ├── DeleteCountryCommand.cs
│   │   │   ├── CreateJurisdictionCommand.cs
│   │   │   ├── UpdateJurisdictionCommand.cs
│   │   │   ├── DeleteJurisdictionCommand.cs
│   │   │   ├── CreateTaxFrameworkCommand.cs
│   │   │   ├── UpdateTaxFrameworkCommand.cs
│   │   │   ├── CreateTaxRateCommand.cs
│   │   │   ├── UpdateTaxRateCommand.cs
│   │   │   ├── DeleteTaxRateCommand.cs
│   │   │   ├── CreateProductServiceCategoryCommand.cs
│   │   │   └── UpdateProductServiceCategoryCommand.cs
│   │   ├── Commands/Handlers/
│   │   │   ├── CreateCountryCommandHandler.cs
│   │   │   ├── UpdateCountryCommandHandler.cs
│   │   │   ├── DeleteCountryCommandHandler.cs
│   │   │   ├── CreateJurisdictionCommandHandler.cs
│   │   │   ├── UpdateJurisdictionCommandHandler.cs
│   │   │   ├── DeleteJurisdictionCommandHandler.cs
│   │   │   ├── CreateTaxFrameworkCommandHandler.cs
│   │   │   ├── UpdateTaxFrameworkCommandHandler.cs
│   │   │   ├── CreateTaxRateCommandHandler.cs
│   │   │   ├── UpdateTaxRateCommandHandler.cs
│   │   │   ├── DeleteTaxRateCommandHandler.cs
│   │   │   ├── CreateProductServiceCategoryCommandHandler.cs
│   │   │   └── UpdateProductServiceCategoryCommandHandler.cs
│   │   ├── Queries/
│   │   │   ├── GetAllCountriesQuery.cs
│   │   │   ├── GetCountryByIdQuery.cs
│   │   │   ├── GetJurisdictionsByCountryQuery.cs
│   │   │   ├── GetJurisdictionByIdQuery.cs
│   │   │   ├── GetAllTaxFrameworksQuery.cs
│   │   │   ├── GetTaxFrameworkByIdQuery.cs
│   │   │   ├── GetAllTaxRatesQuery.cs
│   │   │   ├── GetTaxRatesByJurisdictionQuery.cs
│   │   │   ├── GetAllProductServiceCategoriesQuery.cs
│   │   │   ├── GetProductServiceCategoryByIdQuery.cs
│   │   │   ├── GetTaxCalculationLogQuery.cs
│   │   │   └── PreviewTaxCalculationQuery.cs
│   │   ├── Queries/Handlers/
│   │   │   ├── GetAllCountriesQueryHandler.cs
│   │   │   ├── GetCountryByIdQueryHandler.cs
│   │   │   ├── GetJurisdictionsByCountryQueryHandler.cs
│   │   │   ├── GetJurisdictionByIdQueryHandler.cs
│   │   │   ├── GetAllTaxFrameworksQueryHandler.cs
│   │   │   ├── GetTaxFrameworkByIdQueryHandler.cs
│   │   │   ├── GetAllTaxRatesQueryHandler.cs
│   │   │   ├── GetTaxRatesByJurisdictionQueryHandler.cs
│   │   │   ├── GetAllProductServiceCategoriesQueryHandler.cs
│   │   │   ├── GetProductServiceCategoryByIdQueryHandler.cs
│   │   │   ├── GetTaxCalculationLogQueryHandler.cs
│   │   │   └── PreviewTaxCalculationQueryHandler.cs
│   │   ├── Dtos/
│   │   │   ├── CountryDto.cs
│   │   │   ├── JurisdictionDto.cs
│   │   │   ├── TaxFrameworkDto.cs
│   │   │   ├── TaxRateDto.cs
│   │   │   ├── ProductServiceCategoryDto.cs
│   │   │   ├── TaxCalculationResultDto.cs
│   │   │   └── TaxCalculationLogDto.cs
│   │   ├── Services/
│   │   │   ├── ITaxCalculationService.cs
│   │   │   └── TaxCalculationService.cs      # Core tax calculation engine
│   │   ├── Requests/
│   │   │   ├── CreateCountryRequest.cs
│   │   │   ├── UpdateCountryRequest.cs
│   │   │   ├── CreateJurisdictionRequest.cs
│   │   │   ├── UpdateJurisdictionRequest.cs
│   │   │   ├── CreateTaxFrameworkRequest.cs
│   │   │   ├── UpdateTaxFrameworkRequest.cs
│   │   │   ├── CreateTaxRateRequest.cs
│   │   │   ├── UpdateTaxRateRequest.cs
│   │   │   ├── CreateProductServiceCategoryRequest.cs
│   │   │   ├── UpdateProductServiceCategoryRequest.cs
│   │   │   ├── PreviewTaxCalculationRequest.cs
│   │   │   └── ImportTaxConfigurationRequest.cs
│   │   ├── Validators/
│   │   │   ├── CreateCountryRequestValidator.cs
│   │   │   ├── UpdateCountryRequestValidator.cs
│   │   │   ├── CreateJurisdictionRequestValidator.cs
│   │   │   ├── UpdateJurisdictionRequestValidator.cs
│   │   │   ├── CreateTaxFrameworkRequestValidator.cs
│   │   │   ├── UpdateTaxFrameworkRequestValidator.cs
│   │   │   ├── CreateTaxRateRequestValidator.cs
│   │   │   ├── UpdateTaxRateRequestValidator.cs
│   │   │   ├── CreateProductServiceCategoryRequestValidator.cs
│   │   │   └── UpdateProductServiceCategoryRequestValidator.cs
│   │   └── Mapping/
│   │       └── TaxManagementProfile.cs
│   │
│   └── Quotations/
│       ├── Services/
│       │   └── QuotationService.cs          # MODIFY: Integrate tax calculation
│       └── Commands/Handlers/
│           ├── CreateQuotationCommandHandler.cs    # MODIFY: Use tax calculation
│           └── UpdateQuotationCommandHandler.cs    # MODIFY: Recalculate tax on changes
│
├── CRM.Infrastructure/
│   ├── EntityConfigurations/
│   │   ├── CountryEntityConfiguration.cs
│   │   ├── JurisdictionEntityConfiguration.cs
│   │   ├── TaxFrameworkEntityConfiguration.cs
│   │   ├── TaxRateEntityConfiguration.cs
│   │   ├── ProductServiceCategoryEntityConfiguration.cs
│   │   ├── TaxCalculationLogEntityConfiguration.cs
│   │   ├── QuotationLineItemEntityConfiguration.cs  # MODIFY: Add CategoryId FK
│   │   └── ClientEntityConfiguration.cs              # MODIFY: Add CountryId, JurisdictionId FKs
│   └── Persistence/
│       └── AppDbContext.cs                 # MODIFY: Add DbSets for new entities
│
├── CRM.Api/
│   ├── Controllers/
│   │   ├── CountriesController.cs          # NEW: Country management endpoints
│   │   ├── JurisdictionsController.cs      # NEW: Jurisdiction management endpoints
│   │   ├── TaxFrameworksController.cs      # NEW: Tax framework endpoints
│   │   ├── TaxRatesController.cs           # NEW: Tax rate endpoints
│   │   ├── ProductServiceCategoriesController.cs  # NEW: Category endpoints
│   │   ├── TaxCalculationController.cs     # NEW: Tax calculation endpoints
│   │   └── TaxAuditLogController.cs        # NEW: Audit log endpoints
│   └── Program.cs                          # MODIFY: Register new services
│
└── CRM.Tests.Integration/
    └── TaxManagement/
        ├── TaxCalculationServiceTests.cs
        ├── CountriesControllerTests.cs
        ├── JurisdictionsControllerTests.cs
        └── TaxRatesControllerTests.cs

src/Frontend/web/
├── src/
│   ├── app/
│   │   └── (protected)/
│   │       └── admin/
│   │           └── tax/
│   │               ├── countries/
│   │               │   ├── page.tsx                 # NEW: Country management page
│   │               │   ├── [countryId]/
│   │               │   │   └── jurisdictions/
│   │               │   │       └── page.tsx         # NEW: Jurisdiction management
│   │               │   └── new/
│   │               │       └── page.tsx             # NEW: Create country form
│   │               ├── frameworks/
│   │               │   └── page.tsx                 # NEW: Tax framework config
│   │               ├── rates/
│   │               │   └── page.tsx                 # NEW: Tax rate management
│   │               ├── categories/
│   │               │   └── page.tsx                 # NEW: Category tax rules
│   │               ├── audit-log/
│   │               │   └── page.tsx                 # NEW: Audit log view
│   │               └── import-export/
│   │                   └── page.tsx                 # NEW: Import/export page
│   ├── components/
│   │   └── tax/
│   │       ├── CountryManagementTable.tsx   # NEW: Country table component
│   │       ├── CountryForm.tsx              # NEW: Country form component
│   │       ├── JurisdictionTree.tsx         # NEW: Jurisdiction tree view
│   │       ├── JurisdictionForm.tsx         # NEW: Jurisdiction form
│   │       ├── TaxFrameworkForm.tsx         # NEW: Tax framework form
│   │       ├── TaxRateTable.tsx             # NEW: Tax rate table
│   │       ├── TaxRateForm.tsx              # NEW: Tax rate form
│   │       ├── CategoryTaxRulesTable.tsx    # NEW: Category rules table
│   │       ├── TaxCalculationPreview.tsx    # NEW: Tax calculation preview
│   │       └── TaxAuditLogTable.tsx         # NEW: Audit log table
│   └── lib/
│       └── api.ts                           # MODIFY: Add tax management API methods
└── tests/
    └── tax/
        ├── TaxCalculation.test.tsx
        └── CountryManagement.test.tsx
```

**Structure Decision**: Web application structure with backend (ASP.NET Core) and frontend (Next.js). Backend follows Clean Architecture with Domain, Application, Infrastructure, and API layers. Frontend uses Next.js App Router with protected admin routes for tax management. All new entities follow existing patterns (Commands/Queries, DTOs, Validators, AutoMapper profiles).

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Multiple new entities (6 entities) | Tax management requires country, jurisdiction, framework, rate, category, and audit log entities. Each serves distinct purpose. | Single monolithic entity would violate normalization and make queries/updates inefficient |
| Tax calculation service abstraction | Complex tax logic with multiple components (CGST/SGST for GST, single VAT) requires dedicated service for testability and maintainability | Inline calculation logic would duplicate code and make testing difficult |
| Historical tax rate tracking (effective dates) | Compliance requirement - need to know which rates applied at quotation creation time | Current rates only would break historical quotation accuracy |

