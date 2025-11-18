# Implementation Plan: Quotation Entity & CRUD Operations (Spec-009)

**Branch**: `009-quotation-crud` | **Date**: 2025-11-15 | **Spec**: [`specs/009-quotation-crud/spec.md`](./spec.md)  
**Input**: Feature specification and artifacts under `specs/009-quotation-crud/`

## Summary

Deliver complete CRUD operations for Quotations including entity creation, line items management, automatic tax calculation (GST - CGST/SGST/IGST), discount support, status lifecycle management, and full authorization. Implementation builds atop existing Clean Architecture backend (.NET 8, EF Core, MediatR) and TailAdmin frontend (Next.js 16, React 19, TypeScript). This spec is CRITICAL and includes both backend and frontend implementation.

## Technical Context

**Language/Version**: C# 12 / .NET 8 Web API (Backend), TypeScript / Next.js 16 (Frontend)  
**Primary Dependencies**: ASP.NET Core, MediatR, EF Core, AutoMapper, FluentValidation, Serilog (Backend); React Query, React Hook Form, Tailwind CSS, TailAdmin (Frontend)  
**Storage**: PostgreSQL 14+ with UUID PKs, DECIMAL for financial amounts, proper indexes  
**Testing**: xUnit + FluentAssertions (backend unit), WebApplicationFactory (integration), React Testing Library + Jest (frontend), Cypress/Playwright (E2E)  
**Target Platform**: Containerized Linux backend API (Kestrel) + Next.js frontend (Node.js)  
**Project Type**: Multi-project Clean Architecture backend + Next.js frontend mono-repo  
**Performance Goals**: Create quotation ≤30s, list load ≤2s @100 items, tax calculation <100ms, real-time frontend updates <300ms debounce  
**Constraints**: Only DRAFT quotations editable, immutable quotation numbers, tax calculation automatic, authorization enforced, soft delete (CANCELLED status)  
**Scale/Scope**: Thousands of quotations per month, 1-50 line items per quotation, tax calculation on every create/update

## Constitution Check

*GATE STATUS: PASS (all mandatory principles satisfied pre-design)*

| Principle | Evidence |
|-----------|----------|
| Spec-driven delivery | Feature maps directly to Spec-009 with traceable artifacts (spec, research, plan, contracts). |
| Clean Architecture boundaries | Changes limited to CRM.Domain/Application/Infrastructure/Api with MediatR handlers & EF migrations; no bypassing layers. |
| RBAC enforcement | Every endpoint enforces SalesRep/Admin scope; SalesReps see only own quotations, Admins see all. |
| Security & compliance | Tax calculation ensures GST compliance, authorization protects pricing data, audit trail for all operations. |
| Testing & quality | Plan includes unit + integration + E2E coverage for handlers, queries, commands, and frontend components; performance budgets noted. |
| Observability/auditability | Domain events for all operations, Serilog integration, quotation number immutability for traceability. |
| Frontend integration | TailAdmin components used, React Query for state, real-time tax calculation, responsive design. |

## Project Structure

### Documentation (this feature)

```text
specs/009-quotation-crud/
├── plan.md              # This file
├── research.md          # Technical decisions
├── data-model.md        # Database schema
├── quickstart.md        # Setup instructions
├── contracts/           # OpenAPI specification
│   └── quotations.openapi.yaml
└── tasks.md             # Detailed task breakdown (generated)
```

### Source Code (repository root)
```text
src/
├── Backend/
│   ├── CRM.Api/
│   │   └── Controllers/QuotationsController.cs
│   ├── CRM.Application/
│   │   ├── Quotations/ (Commands, Queries, Validators, Dtos, Services)
│   │   └── Common/, Auth/, Clients/
│   ├── CRM.Domain/
│   │   ├── Entities/ (Quotation.cs, QuotationLineItem.cs)
│   │   ├── Enums/ (QuotationStatus.cs)
│   │   └── Events/ (QuotationCreated.cs, QuotationUpdated.cs, QuotationDeleted.cs)
│   ├── CRM.Infrastructure/
│   │   ├── Persistence/ (EntityConfigurations, Migrations)
│   │   └── Services/ (TaxCalculationService, QuotationNumberGenerator)
│   └── CRM.Shared/ (DTOs, Constants, Helpers)
├── Frontend/
│   └── web/ (Next.js 16 TailAdmin app)
│       ├── src/app/(protected)/quotations/
│       │   ├── page.tsx (list)
│       │   ├── create/page.tsx
│       │   └── [id]/
│       │       ├── edit/page.tsx
│       │       ├── view/page.tsx
│       │       └── timeline/page.tsx
│       ├── src/components/quotations/
│       │   ├── QuotationTable.tsx
│       │   ├── QuotationForm.tsx
│       │   ├── QuotationViewer.tsx
│       │   ├── LineItemRepeater.tsx
│       │   ├── TaxCalculationPreview.tsx
│       │   └── ClientSelector.tsx
│       └── src/lib/api.ts (QuotationsApi methods)
```

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | All principles satisfied | N/A |

**Structure Decision**: Continue leveraging existing Clean Architecture solution (CRM.Api ⇄ CRM.Application ⇄ CRM.Domain ⇄ CRM.Infrastructure) with contracts/tests mirroring backend modules. Frontend implementation is CRITICAL for this spec and must be built alongside backend using TailAdmin components, React Query for state management, and real-time tax calculation.

## Implementation Phases

1. **Phase 1: Setup & Foundational** - Entities, DTOs, migrations, validators, tax calculation service
2. **Phase 2: Backend CRUD** - Commands, queries, handlers, quotation number generator
3. **Phase 3: API Endpoints** - Controller, authorization, error handling, validation
4. **Phase 4: Frontend API Integration** - API service methods, React Query hooks
5. **Phase 5: Frontend Pages** - List, create, edit, view, timeline pages
6. **Phase 6: Frontend Components** - Reusable components, tax calculation preview, line item repeater
7. **Phase 7: Testing** - Unit, integration, E2E tests (backend + frontend)
8. **Phase 8: Polish** - Documentation, performance optimization, UX improvements

## Key Implementation Details

### Backend Services

- **TaxCalculationService**: Determines intra-state vs inter-state, calculates CGST/SGST/IGST
- **QuotationNumberGenerator**: Generates unique quotation numbers with retry on collision
- **QuotationTotalsCalculator**: Calculates subtotal, discount, tax, and total amount

### Frontend Services

- **TaxCalculator** (utils): Mirrors backend tax calculation for real-time updates
- **QuotationFormState**: Manages multi-step form state with React Hook Form
- **LineItemManager**: Handles add/remove/update line items with sequence numbers

### Authorization Rules

- SalesRep: Can create, view own quotations, edit/delete own DRAFT quotations
- Admin: Can view all quotations, edit/delete any DRAFT quotations
- All operations require JWT authentication

### Tax Calculation Logic

- **Intra-State** (same state): CGST = 9%, SGST = 9% (total 18%)
- **Inter-State** (different state): IGST = 18%
- Tax applied to: (SubTotal - DiscountAmount)
- Company state code from system settings, client state code from Client entity

### Status Lifecycle

- **DRAFT**: Can be edited/deleted
- **SENT**: Immutable (cannot edit/delete)
- **VIEWED/ACCEPTED/REJECTED/EXPIRED**: Immutable
- **CANCELLED**: Can be deleted (soft delete)

### Performance Considerations

- Eager load line items with quotation (avoid N+1 queries)
- Index on (ClientId, Status) for fast filtering
- Index on (CreatedByUserId, Status, CreatedAt DESC) for dashboard queries
- Pagination default 10, max 100 per page
- Frontend tax calculation debounced 300ms for performance

