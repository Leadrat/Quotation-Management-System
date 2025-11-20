# Implementation Plan: Company Details Admin Configuration & Quotation Integration (Spec-022)

**Branch**: `022-company-details-admin` | **Date**: 2025-01-27 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/022-company-details-admin/spec.md`

## Summary

This specification enables administrators to centrally configure and manage company information including tax identification numbers (PAN, TAN, GST), banking details for multiple countries (India and Dubai), company address, contact information, legal disclaimers, and branding assets (logo). The configured company details automatically flow into quotation documents, PDFs, and email templates, with country-specific bank details dynamically selected based on the client's country.

The technical approach involves creating a singleton CompanyDetails entity with related BankDetails entities for country-specific banking information, implementing admin-only APIs for configuration management, integrating company details into the quotation PDF generation service and email templates, and building a responsive admin configuration page in the frontend.

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
- Company details configuration page loads in <2 seconds
- Quotation PDF generation includes company details without performance degradation (<3 seconds for standard quotations)
- Company details retrieval for quotations completes in <100ms (cached)

**Constraints**: 
- Must integrate seamlessly with existing quotation creation/update workflows
- Must not break existing quotation functionality
- Company details changes must be auditable
- Historical accuracy: quotations created before updates must preserve original company details
- Logo uploads must be validated (file type, size) and stored securely
- Tax identification number validation must follow Indian tax regulations

**Scale/Scope**: 
- Single company details record (singleton pattern)
- Support for multiple countries' bank details (initially India and Dubai, expandable)
- Expected 100-1000 quotations per day referencing company details
- Logo storage: up to 5MB per logo, common image formats (PNG, JPG, SVG)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Based on the constitution file at `.specify/memory/constitution.md`, the following principles must be verified:

### I. Architecture Compliance
- ✅ Clean Architecture pattern maintained (Domain, Application, Infrastructure, API layers)
- ✅ Entity Framework Core used for data access (consistent with existing codebase)
- ✅ DTOs, Commands, Queries pattern maintained (CQRS-lite approach)
- ✅ AutoMapper used for entity-to-DTO mapping (consistent with existing codebase)

### II. Testing Requirements
- ⚠️ Unit tests required for company details validation logic (PAN, TAN, GST formats)
- ⚠️ Integration tests required for company details API endpoints
- ⚠️ Integration tests required for quotation PDF generation with company details
- ⚠️ Test coverage target: ≥85% for backend, ≥80% for frontend

### III. Security & Authorization
- ✅ RBAC enforced - Admin role required for all company details configuration endpoints
- ✅ Audit logging required for all company details changes
- ✅ Input validation required for all tax identification numbers, bank details
- ✅ File upload validation for logo (type, size)

### IV. Data Integrity
- ✅ Singleton pattern enforced (only one CompanyDetails record)
- ✅ Foreign key constraints enforced for referential integrity
- ✅ Unique constraints for country-specific bank details
- ✅ Historical accuracy preserved in quotations (snapshot at creation time)

### V. Performance & Scalability
- ✅ Company details cached for frequent quotation generation
- ✅ Efficient queries for retrieving company details
- ✅ Logo storage optimized (file system or cloud storage)

**Gate Status**: ✅ PASSING - All constitutional requirements can be met with standard patterns already in use.

## Project Structure

### Documentation (this feature)

```text
specs/022-company-details-admin/
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
│   │   ├── CompanyDetails.cs          # NEW: Singleton company details entity
│   │   └── BankDetails.cs            # NEW: Country-specific bank details entity
│   └── Enums/
│       └── BankCountry.cs             # NEW: Enum for supported countries (India, Dubai)
│
├── CRM.Application/
│   ├── CompanyDetails/               # NEW: Feature module
│   │   ├── Commands/
│   │   │   ├── UpdateCompanyDetailsCommand.cs
│   │   │   └── Handlers/
│   │   │       └── UpdateCompanyDetailsCommandHandler.cs
│   │   ├── Queries/
│   │   │   ├── GetCompanyDetailsQuery.cs
│   │   │   └── Handlers/
│   │   │       └── GetCompanyDetailsQueryHandler.cs
│   │   ├── Dtos/
│   │   │   ├── CompanyDetailsDto.cs
│   │   │   ├── BankDetailsDto.cs
│   │   │   └── UpdateCompanyDetailsRequest.cs
│   │   ├── Validators/
│   │   │   ├── UpdateCompanyDetailsRequestValidator.cs
│   │   │   └── TaxNumberValidators.cs  # PAN, TAN, GST validation
│   │   └── Services/
│   │       └── ICompanyDetailsService.cs  # Service for retrieving company details
│   │
│   └── Quotations/
│       ├── Services/
│       │   └── QuotationPdfGenerationService.cs  # MODIFY: Include company details
│       └── Services/
│           └── QuotationEmailService.cs  # MODIFY: Include company details in email
│
├── CRM.Infrastructure/
│   ├── EntityConfigurations/
│   │   ├── CompanyDetailsEntityConfiguration.cs  # NEW
│   │   └── BankDetailsEntityConfiguration.cs   # NEW
│   ├── Migrations/
│   │   └── YYYYMMDD_CreateCompanyDetailsTables.cs  # NEW
│   └── Services/
│       └── FileStorageService.cs  # NEW or MODIFY: Handle logo uploads
│
└── CRM.Api/
    └── Controllers/
        └── CompanyDetailsController.cs  # NEW: Admin-only endpoints

src/Frontend/web/
├── src/
│   ├── app/
│   │   └── (protected)/
│   │       └── admin/
│   │           └── company-details/
│   │               └── page.tsx  # NEW: Admin configuration page
│   ├── components/
│   │   └── tailadmin/
│   │       └── company-details/  # NEW: Company details form components
│   │           ├── CompanyDetailsForm.tsx
│   │           ├── BankDetailsSection.tsx
│   │           └── LogoUpload.tsx
│   └── lib/
│       └── api.ts  # MODIFY: Add CompanyDetailsApi
```

**Structure Decision**: Web application (backend + frontend) following existing Clean Architecture pattern. Company details stored as singleton entity with related bank details. Logo storage handled via file system or cloud storage service.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

No violations identified. All requirements can be implemented using existing patterns and architecture.

