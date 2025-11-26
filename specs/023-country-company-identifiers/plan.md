# Implementation Plan: Company Details Admin Page – Country-Specific Identifiers & Bank Details (Spec-023)

**Branch**: `023-country-company-identifiers` | **Date**: 2025-01-27 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/023-country-company-identifiers/spec.md`

## Summary

This specification extends the Company Details admin page to support country-specific company identifiers and bank details management. The system enables administrators to configure required identifiers (e.g., PAN for India, VAT for EU, Business License for Dubai) and relevant bank detail fields (e.g., IFSC for India, IBAN/SWIFT for Dubai/UAE, routing codes for US) for each country. These settings automatically flow into quotation creation and editing so only contextually correct data is visible and required.

The technical approach involves creating master configuration tables for identifier types and bank field types with country-specific validation rules, implementing EAV (Entity-Attribute-Value) or JSONB-based storage for company identifier and bank detail values, extending the Company Details admin page to dynamically render fields based on country selection, and integrating country-specific company details into quotation generation based on client country.

## Technical Context

**Language/Version**: C# 12 / .NET 8  
**Primary Dependencies**: 
- ASP.NET Core 8.0
- Entity Framework Core 8.0 (PostgreSQL provider)
- AutoMapper (for DTO mapping)
- FluentValidation (for request validation)
- QuestPDF (for PDF generation - already in use)
- FluentEmail (for email templates - already in use)

**Storage**: PostgreSQL (via Entity Framework Core)  
**Testing**: xUnit, Moq, FluentAssertions  
**Target Platform**: Linux server (ASP.NET Core web API) + Next.js frontend  
**Project Type**: Web application (backend + frontend)  
**Performance Goals**: 
- Master configuration pages load in <2 seconds
- Company Details page loads and renders dynamic fields in <2 seconds
- Country selection change updates form fields in <0.5 seconds
- Quotation PDF generation includes country-specific company details without performance degradation (<3 seconds for standard quotations)
- Company details retrieval for quotations completes in <100ms (cached)

**Constraints**: 
- Must integrate seamlessly with existing Company Details (Spec-022) and quotation creation/update workflows
- Must not break existing quotation functionality
- Master configuration and company detail changes must be auditable
- Historical accuracy: quotations created before updates must preserve original company details
- Validation rules must be stored as configuration data (not hardcoded)
- Must support adding new countries without code changes
- Form field validation must be real-time and responsive

**Scale/Scope**: 
- Support for 10+ countries initially, expandable to 50+
- Multiple identifier types per country (3-5 typical)
- Multiple bank fields per country (4-6 typical)
- Single company details record (singleton pattern) with country-specific values
- Expected 100-1000 quotations per day referencing country-specific company details
- Master configuration changes: 1-5 per month per country

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Based on standard Clean Architecture principles and existing codebase patterns:

### I. Architecture Compliance
- ✅ Clean Architecture pattern maintained (Domain, Application, Infrastructure, API layers)
- ✅ Entity Framework Core used for data access (consistent with existing codebase)
- ✅ DTOs, Commands, Queries pattern maintained (CQRS-lite approach)
- ✅ AutoMapper used for entity-to-DTO mapping (consistent with existing codebase)

### II. Testing Requirements
- ⚠️ Unit tests required for identifier and bank field validation logic per country
- ⚠️ Unit tests required for dynamic field rendering logic
- ⚠️ Integration tests required for master configuration API endpoints
- ⚠️ Integration tests required for company details API endpoints with country-specific fields
- ⚠️ Integration tests required for quotation PDF generation with country-specific company details
- ⚠️ Test coverage target: ≥85% for backend, ≥80% for frontend

### III. Security & Authorization
- ✅ RBAC enforced - Admin role required for all master configuration endpoints
- ✅ RBAC enforced - Admin role required for company details configuration endpoints
- ✅ Audit logging required for all master configuration and company detail changes
- ✅ Input validation required for all identifier values, bank details per country rules

### IV. Data Integrity
- ✅ Foreign key constraints enforced for referential integrity (country references, configuration references)
- ✅ Unique constraints for country-identifier type combinations and country-bank field combinations
- ✅ Validation rules stored as configuration data (not hardcoded)
- ✅ Historical accuracy preserved in quotations (snapshot at creation time)

### V. Performance & Scalability
- ✅ Master configuration cached for frequent quotation generation
- ✅ Country-specific field configurations cached for form rendering
- ✅ Efficient queries for retrieving country-specific company details
- ✅ Database indexes on country, identifier type, bank field type columns

**Gate Status**: ✅ PASSING - All constitutional requirements can be met with standard patterns already in use. Research needed for optimal data model approach (EAV vs JSONB vs normalized tables).

## Project Structure

### Documentation (this feature)

```text
specs/023-country-company-identifiers/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
│   └── country-company-identifiers.openapi.yaml
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/Backend/
├── CRM.Domain/
│   └── Entities/
│       ├── IdentifierType.cs (new)
│       ├── CountryIdentifierConfiguration.cs (new)
│       ├── CompanyIdentifierValue.cs (new)
│       ├── BankFieldType.cs (new)
│       ├── CountryBankFieldConfiguration.cs (new)
│       ├── CompanyBankDetails.cs (modified from Spec-022)
│       └── CompanyDetails.cs (modified from Spec-022)
├── CRM.Application/
│   ├── CompanyIdentifiers/
│   │   ├── Commands/
│   │   │   ├── CreateIdentifierTypeCommand.cs (new)
│   │   │   ├── UpdateIdentifierTypeCommand.cs (new)
│   │   │   ├── ConfigureCountryIdentifierCommand.cs (new)
│   │   │   ├── UpdateCountryIdentifierConfigurationCommand.cs (new)
│   │   │   ├── SaveCompanyIdentifierValuesCommand.cs (new)
│   │   │   └── Handlers/
│   │   ├── Queries/
│   │   │   ├── GetIdentifierTypesQuery.cs (new)
│   │   │   ├── GetCountryIdentifierConfigurationsQuery.cs (new)
│   │   │   ├── GetCompanyIdentifierValuesQuery.cs (new)
│   │   │   └── Handlers/
│   │   ├── DTOs/
│   │   └── Validators/
│   ├── CompanyBankDetails/
│   │   ├── Commands/
│   │   │   ├── CreateBankFieldTypeCommand.cs (new)
│   │   │   ├── UpdateBankFieldTypeCommand.cs (new)
│   │   │   ├── ConfigureCountryBankFieldCommand.cs (new)
│   │   │   ├── UpdateCountryBankFieldConfigurationCommand.cs (new)
│   │   │   ├── SaveCompanyBankDetailsCommand.cs (modified)
│   │   │   └── Handlers/
│   │   ├── Queries/
│   │   │   ├── GetBankFieldTypesQuery.cs (new)
│   │   │   ├── GetCountryBankFieldConfigurationsQuery.cs (new)
│   │   │   ├── GetCompanyBankDetailsQuery.cs (modified)
│   │   │   └── Handlers/
│   │   ├── DTOs/
│   │   └── Validators/
│   └── Quotations/
│       └── Services/
│           └── QuotationCompanyDetailsService.cs (modified - filter by country)
├── CRM.Infrastructure/
│   ├── Persistence/
│   │   ├── Configurations/
│   │   │   ├── IdentifierTypeEntityConfiguration.cs (new)
│   │   │   ├── CountryIdentifierConfigurationEntityConfiguration.cs (new)
│   │   │   ├── CompanyIdentifierValueEntityConfiguration.cs (new)
│   │   │   ├── BankFieldTypeEntityConfiguration.cs (new)
│   │   │   ├── CountryBankFieldConfigurationEntityConfiguration.cs (new)
│   │   │   └── CompanyBankDetailsEntityConfiguration.cs (modified)
│   │   └── AppDbContext.cs (modified - add new DbSets)
│   └── Migrations/
│       └── [timestamp]_AddCountrySpecificIdentifiersAndBankDetails.cs (new)
├── CRM.Api/
│   └── Controllers/
│       ├── IdentifierTypesController.cs (new)
│       ├── CountryIdentifierConfigurationsController.cs (new)
│       ├── CompanyIdentifiersController.cs (new)
│       ├── BankFieldTypesController.cs (new)
│       ├── CountryBankFieldConfigurationsController.cs (new)
│       └── CompanyBankDetailsController.cs (modified)
└── CRM.Shared/
    └── Constants/
        └── ValidationPatterns.cs (new - country-specific regex patterns)

src/Frontend/web/
├── src/
│   ├── app/
│   │   └── admin/
│   │       ├── company-identifiers/
│   │       │   ├── page.tsx (new - master identifier types)
│   │       │   └── [countryId]/
│   │       │       └── page.tsx (new - country identifier configuration)
│   │       ├── company-bank-fields/
│   │       │   ├── page.tsx (new - master bank field types)
│   │       │   └── [countryId]/
│   │       │       └── page.tsx (new - country bank field configuration)
│   │       └── company-details/
│   │           └── page.tsx (modified - dynamic fields based on country)
│   ├── components/
│   │   ├── admin/
│   │   │   ├── IdentifierTypeForm.tsx (new)
│   │   │   ├── CountryIdentifierConfigurationForm.tsx (new)
│   │   │   ├── BankFieldTypeForm.tsx (new)
│   │   │   ├── CountryBankFieldConfigurationForm.tsx (new)
│   │   │   └── DynamicCompanyDetailsForm.tsx (new)
│   │   └── quotations/
│   │       └── QuotationCompanyDetailsDisplay.tsx (modified - country-specific display)
│   └── lib/
│       └── api/
│           ├── identifierTypes.ts (new)
│           ├── countryIdentifierConfigurations.ts (new)
│           ├── bankFieldTypes.ts (new)
│           └── countryBankFieldConfigurations.ts (new)

tests/
├── CRM.Tests/
│   ├── Application/
│   │   ├── CompanyIdentifiers/
│   │   └── CompanyBankDetails/
│   └── Domain/
│       ├── IdentifierTypeTests.cs (new)
│       ├── CountryIdentifierConfigurationTests.cs (new)
│       └── BankFieldTypeTests.cs (new)
└── CRM.Tests.Integration/
    ├── CompanyIdentifiers/
    └── CompanyBankDetails/
```

**Structure Decision**: Web application structure with separate backend (ASP.NET Core) and frontend (Next.js) projects. Clean Architecture maintained with Domain, Application, Infrastructure, and API layers. Master configuration tables stored in normalized relational structure for queryability and referential integrity. Company identifier and bank detail values stored using flexible schema (EAV or JSONB) to support country-specific fields without schema changes.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

No violations identified. Standard patterns from existing codebase will be used.
