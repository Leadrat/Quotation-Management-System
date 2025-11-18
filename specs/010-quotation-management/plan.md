# Implementation Plan: Quotation Management (Spec-010)

**Branch**: `010-quotation-management` | **Date**: 2025-11-15 | **Spec**: [`specs/010-quotation-management/spec.md`](./spec.md)  
**Input**: Feature specification and artifacts under `specs/010-quotation-management/`

## Summary

Deliver complete Quotation Management workflow including sending quotations via email, tracking client views and responses, generating PDF files, managing secure access links, and updating quotation status automatically. Implementation builds on Spec-009 (Quotation Entity) and requires email service integration, PDF generation, and public client portal.

## Technical Context

**Language/Version**: C# 12 / .NET 8 Web API (Backend), TypeScript / Next.js 16 (Frontend)  
**Primary Dependencies**: ASP.NET Core, EF Core, QuestPDF (PDF generation), FluentEmail (email), Quartz.NET (background jobs)  
**Storage**: PostgreSQL 14+ with UUID PKs, proper indexes for access token lookup  
**Testing**: xUnit + FluentAssertions (backend unit), WebApplicationFactory (integration), React Testing Library (frontend)  
**Target Platform**: Containerized Linux backend API (Kestrel) + Next.js frontend (Node.js)  
**Performance Goals**: Send quotation ≤1 minute, PDF generation <5 seconds, email delivery <30 seconds, client portal load <2 seconds  
**Constraints**: Access tokens must be cryptographically secure, only one response per quotation, status transitions are immutable  
**Scale/Scope**: Hundreds of quotations sent per month, 1-10 views per quotation, 50-70% response rate

## Constitution Check

*GATE STATUS: PASS (all mandatory principles satisfied pre-design)*

| Principle | Evidence |
|-----------|----------|
| Spec-driven delivery | Feature maps directly to Spec-010 with traceable artifacts (spec, research, plan, contracts). |
| Clean Architecture boundaries | Changes limited to CRM.Domain/Application/Infrastructure/Api with handlers & EF migrations; no bypassing layers. |
| RBAC enforcement | Every endpoint enforces SalesRep/Admin scope; public endpoints use token-based authorization. |
| Security & compliance | Secure token generation, IP tracking, audit trail for all actions, email delivery tracking. |
| Testing & quality | Plan includes unit + integration + E2E coverage for commands, queries, and frontend components. |
| Observability/auditability | Domain events for all operations, status history immutable log, view tracking. |
| Frontend integration | TailAdmin components used, public client portal, real-time status updates. |

## Project Structure

### Documentation (this feature)

```text
specs/010-quotation-management/
├── plan.md              # This file
├── research.md          # Technical decisions
├── data-model.md       # Database schema
├── quickstart.md       # Setup instructions
├── contracts/          # OpenAPI specification
│   └── quotation-management.openapi.yaml
└── tasks.md            # Detailed task breakdown (generated)
```

### Source Code (repository root)

```text
src/
├── Backend/
│   ├── CRM.Api/
│   │   └── Controllers/
│   │       ├── QuotationsController.cs (extended)
│   │       └── ClientPortalController.cs (new, public)
│   ├── CRM.Application/
│   │   ├── Quotations/
│   │   │   ├── Commands/
│   │   │   │   ├── SendQuotationCommand.cs
│   │   │   │   ├── MarkQuotationAsViewedCommand.cs
│   │   │   │   ├── SubmitQuotationResponseCommand.cs
│   │   │   │   ├── MarkQuotationAsExpiredCommand.cs
│   │   │   │   └── ResendQuotationCommand.cs
│   │   │   ├── Queries/
│   │   │   │   ├── GetQuotationStatusHistoryQuery.cs
│   │   │   │   ├── GetQuotationResponseQuery.cs
│   │   │   │   ├── GetQuotationAccessLinkQuery.cs
│   │   │   │   └── GetQuotationByAccessTokenQuery.cs
│   │   │   ├── Services/
│   │   │   │   ├── QuotationPdfGenerationService.cs
│   │   │   │   └── QuotationEmailService.cs
│   │   │   └── Dtos/ (new DTOs)
│   ├── CRM.Domain/
│   │   ├── Entities/
│   │   │   ├── QuotationAccessLink.cs
│   │   │   ├── QuotationStatusHistory.cs
│   │   │   └── QuotationResponse.cs
│   │   └── Events/
│   │       ├── QuotationSent.cs
│   │       ├── QuotationViewed.cs
│   │       ├── QuotationResponseReceived.cs
│   │       ├── QuotationExpired.cs
│   │       └── QuotationResent.cs
│   ├── CRM.Infrastructure/
│   │   ├── EntityConfigurations/ (new configurations)
│   │   ├── Migrations/ (new migration)
│   │   ├── Notifications/
│   │   │   └── Templates/ (HTML email templates)
│   │   └── Jobs/
│   │       ├── QuotationExpirationCheckJob.cs
│   │       ├── UnviewedQuotationReminderJob.cs
│   │       └── PendingResponseFollowUpJob.cs
├── Frontend/
│   └── web/ (Next.js 16 TailAdmin app)
│       ├── src/app/(protected)/quotations/
│       │   └── [id]/
│       │       ├── analytics/page.tsx (new)
│       │       └── view/page.tsx (extended)
│       ├── src/app/(public)/client-portal/
│       │   └── quotations/[quotationId]/[token]/page.tsx (new, public)
│       ├── src/components/quotations/
│       │   ├── SendQuotationModal.tsx (new)
│       │   ├── ResendQuotationModal.tsx (new)
│       │   ├── QuotationStatusTimeline.tsx (new)
│       │   ├── ClientResponseCard.tsx (new)
│       │   └── QuotationAnalytics.tsx (new)
│       └── src/lib/api.ts (extended with new endpoints)
```

## Implementation Phases

1. **Phase 1: Setup & Foundational** - Entities, DTOs, migrations, PDF/Email services
2. **Phase 2: Backend Commands** - Send, mark viewed, submit response, expire, resend
3. **Phase 3: Backend Queries** - Status history, response, access link queries
4. **Phase 4: API Endpoints** - Sales rep endpoints and public client portal
5. **Phase 5: Background Jobs** - Expiration check, reminder jobs
6. **Phase 6: Frontend Sales Rep** - Send modal, status timeline, analytics
7. **Phase 7: Frontend Client Portal** - Public quotation view and response
8. **Phase 8: Testing & Polish** - Unit, integration, E2E tests, documentation

## Key Implementation Details

### Backend Services

- **QuotationPdfGenerationService**: Generates professional PDF using QuestPDF
- **QuotationEmailService**: Sends emails with templates using FluentEmail
- **AccessTokenGenerator**: Generates cryptographically secure tokens

### Frontend Services

- **QuotationStatusTimeline**: Displays status history with timeline UI
- **SendQuotationModal**: Multi-step modal for sending quotations
- **ClientPortalView**: Public page for clients to view and respond

### Authorization Rules

- SalesRep: Can send own quotations, view own status history
- Admin: Can send any quotation, view all status history
- Public: Access via secure token (no authentication)

### Status Lifecycle

- **DRAFT** → SENT (via send command)
- **SENT** → VIEWED (automatic on first view)
- **VIEWED** → ACCEPTED/REJECTED/NEEDS_MODIFICATION (via client response)
- **Any** → EXPIRED (automatic via background job)

### Security Considerations

- Access tokens: 32+ character cryptographically secure random
- Token validation: Check IsActive, ExpiresAt, and QuotationId match
- Public endpoints: Rate limiting, IP tracking, no sensitive data
- Email security: Validate all email addresses, prevent spam

### Performance Considerations

- PDF caching: 24-hour cache to avoid regeneration
- Email queue: Background processing to avoid blocking
- Access token lookup: Index on AccessToken for fast validation
- Status history: Index on (QuotationId, ChangedAt) for timeline queries

