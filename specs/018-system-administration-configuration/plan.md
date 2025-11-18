# Implementation Plan: System Administration & Configuration Console

**Branch**: `018-system-administration-configuration` | **Date**: 2025-11-18 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/018-system-administration-configuration/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

This plan implements a centralized System Administration & Configuration Console that enables admin users to manage system-wide settings, integration keys, custom branding, audit logs, data retention policies, and global system messages. The implementation follows Clean Architecture with CQRS pattern, using .NET 8.0 backend and Next.js 15 frontend with TailAdmin theme. All admin actions are logged to an immutable audit trail, and sensitive data (integration keys) are encrypted at rest.

## Technical Context

**Language/Version**: C# 12+ (.NET 8.0), TypeScript 5.x, React 19, Next.js 15

**Primary Dependencies**: 
- Backend: MediatR, Entity Framework Core 8.0, FluentValidation, AutoMapper, Npgsql.EntityFrameworkCore.PostgreSQL, System.Security.Cryptography (for encryption)
- Frontend: Next.js 15, React 19, Tailwind CSS v4, TailAdmin Next.js template, React Query (TanStack Query), Axios, Zustand

**Storage**: PostgreSQL (SystemSettings, IntegrationKeys, AuditLog, CustomBranding, DataRetentionPolicy tables), File storage for logos (S3 or local filesystem)

**Testing**: 
- Backend: xUnit, Moq, FluentAssertions, Microsoft.AspNetCore.Mvc.Testing
- Frontend: Jest, React Testing Library, Playwright (E2E)

**Target Platform**: 
- Backend: Linux/Windows server (.NET 8.0)
- Frontend: Web browsers (Chrome, Firefox, Safari, Edge - latest 2 versions)

**Project Type**: Web application (backend API + frontend SPA)

**Performance Goals**: 
- API p90 <200ms for all admin endpoints
- Settings retrieval <50ms (cached)
- Audit log queries with pagination <500ms
- File upload (logo) <2s for files up to 5MB
- Frontend LCP <2s

**Constraints**: 
- Admin-only access (RBAC enforced)
- Integration keys must be encrypted at rest
- Audit logs immutable (append-only)
- File uploads: max 5MB, PNG/JPG/SVG only
- HTML sanitization required for user-provided content
- All changes must be logged to audit trail
- Settings changes apply immediately without restart

**Scale/Scope**: 
- Support 1-1000 admin users
- 10-100 integration keys per organization
- 1M+ audit log entries (with archival strategy)
- 1-10 branding configurations
- 10-50 data retention policies

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### ✅ Spec-Driven Delivery
- Spec-018 defines complete scope, entities, APIs, and acceptance criteria
- All deliverables trace back to spec requirements

### ✅ Clean Architecture & RBAC Enforcement
- Follows existing .NET Clean Architecture pattern (Domain, Application, Infrastructure, API layers)
- RBAC enforced at API endpoints using `[Authorize(Roles = "Admin")]` attributes
- CQRS pattern with Commands/Queries and Handlers

### ✅ Security, Compliance, and Data Integrity
- Integration keys encrypted using .NET cryptography (AES-256-GCM)
- JWT authentication required for all admin endpoints
- Input validation with FluentValidation
- Audit trail for all admin actions
- UUID PKs, FK constraints, indexes per schema

### ✅ Testing & Quality Gates
- Unit tests ≥85% backend coverage
- Integration tests for all API endpoints
- E2E tests for admin workflows
- Frontend tests ≥80% coverage

### ✅ Observability, Auditability, and Change Control
- Complete audit trail (AuditLog entity)
- Structured logging via Serilog
- All admin actions logged with user, IP, timestamp, changes

### ✅ Frontend Framework & UI Theme Integration
- Uses TailAdmin Next.js template
- Follows TailAdmin folder structure and conventions
- Tailwind CSS v4 utility classes
- React Query for data fetching and cache invalidation

**Gates Status**: ✅ **PASS** - All constitution principles satisfied

## Project Structure

### Documentation (this feature)

```text
specs/018-system-administration-configuration/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
│   └── admin.openapi.yaml
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/Backend/
├── CRM.Domain/
│   └── Admin/
│       ├── SystemSettings.cs
│       ├── IntegrationKey.cs
│       ├── AuditLog.cs
│       ├── CustomBranding.cs
│       ├── DataRetentionPolicy.cs
│       └── Events/
│           ├── SettingsUpdated.cs
│           ├── IntegrationKeyChanged.cs
│           ├── BrandingChanged.cs
│           ├── RetentionPolicyChanged.cs
│           └── SecurityAlert.cs
├── CRM.Application/
│   └── Admin/
│       ├── Commands/
│       │   ├── UpdateSystemSettingsCommand.cs
│       │   ├── ManageIntegrationKeyCommand.cs
│       │   ├── UpdateBrandingCommand.cs
│       │   ├── UpdateRetentionPolicyCommand.cs
│       │   ├── UpdateNotificationSettingsCommand.cs
│       │   └── Handlers/
│       ├── Queries/
│       │   ├── GetSystemSettingsQuery.cs
│       │   ├── GetIntegrationKeysQuery.cs
│       │   ├── GetAuditLogsQuery.cs
│       │   ├── GetBrandingQuery.cs
│       │   ├── GetRetentionPoliciesQuery.cs
│       │   └── Handlers/
│       ├── DTOs/
│       │   ├── SystemSettingsDto.cs
│       │   ├── IntegrationKeyDto.cs
│       │   ├── AuditLogDto.cs
│       │   ├── BrandingDto.cs
│       │   ├── RetentionPolicyDto.cs
│       │   └── NotificationSettingsDto.cs
│       ├── Requests/
│       │   ├── UpdateSystemSettingsRequest.cs
│       │   ├── CreateIntegrationKeyRequest.cs
│       │   ├── UpdateIntegrationKeyRequest.cs
│       │   ├── UpdateBrandingRequest.cs
│       │   ├── UpdateRetentionPolicyRequest.cs
│       │   └── UpdateNotificationSettingsRequest.cs
│       ├── Validators/
│       │   ├── UpdateSystemSettingsRequestValidator.cs
│       │   ├── CreateIntegrationKeyRequestValidator.cs
│       │   ├── UpdateBrandingRequestValidator.cs
│       │   └── ...
│       ├── Services/
│       │   ├── ISystemSettingsService.cs
│       │   ├── SystemSettingsService.cs
│       │   ├── IIntegrationKeyService.cs
│       │   ├── IntegrationKeyService.cs (with encryption)
│       │   ├── IAuditLogService.cs
│       │   ├── AuditLogService.cs
│       │   ├── IBrandingService.cs
│       │   ├── BrandingService.cs
│       │   ├── IRetentionPolicyService.cs
│       │   └── RetentionPolicyService.cs
│       └── Mapping/
│           └── AdminProfile.cs (AutoMapper)
├── CRM.Infrastructure/
│   └── Admin/
│       ├── Encryption/
│       │   ├── IDataEncryptionService.cs
│       │   └── AesDataEncryptionService.cs
│       ├── FileStorage/
│       │   ├── IFileStorageService.cs
│       │   └── LocalFileStorageService.cs (or S3FileStorageService.cs)
│       └── HtmlSanitization/
│           ├── IHtmlSanitizer.cs
│           └── HtmlSanitizerService.cs
├── CRM.Api/
│   └── Controllers/
│       ├── AdminSettingsController.cs
│       ├── AdminIntegrationsController.cs
│       ├── AdminBrandingController.cs
│       ├── AdminAuditLogController.cs
│       ├── AdminRetentionController.cs
│       └── AdminNotificationsController.cs
└── CRM.Migrator/
    └── Migrations/
        └── [timestamp]_AddAdminConfigurationTables.cs

src/Frontend/web/
├── src/
│   ├── app/
│   │   └── admin/
│   │       ├── settings/
│   │       │   └── page.tsx (Admin Console Home)
│   │       ├── settings/system/
│   │       │   └── page.tsx (System Settings Editor)
│   │       ├── integrations/
│   │       │   └── page.tsx (Integration Keys Manager)
│   │       ├── branding/
│   │       │   └── page.tsx (Branding Editor)
│   │       ├── audit-logs/
│   │       │   └── page.tsx (Audit Log Browser)
│   │       ├── data-retention/
│   │       │   └── page.tsx (Data Retention Settings)
│   │       └── notifications/
│   │           └── page.tsx (Global Message Settings)
│   ├── components/
│   │   └── admin/
│   │       ├── SettingsForm.tsx
│   │       ├── ApiKeyList.tsx
│   │       ├── ApiKeyEditDialog.tsx
│   │       ├── BrandingUploader.tsx
│   │       ├── ColorPicker.tsx
│   │       ├── LivePreview.tsx
│   │       ├── AuditLogTable.tsx
│   │       ├── AuditLogFilter.tsx
│   │       ├── LogDetailModal.tsx
│   │       ├── RetentionPolicyTable.tsx
│   │       ├── RetentionEditDialog.tsx
│   │       ├── SystemBannerSet.tsx
│   │       └── PreviewBanner.tsx
│   ├── lib/
│   │   └── api/
│   │       └── admin.ts (API client for admin endpoints)
│   └── hooks/
│       └── useAdminSettings.ts, useIntegrationKeys.ts, etc.

tests/
├── CRM.Tests/
│   └── Admin/
│       ├── Commands/
│       ├── Queries/
│       └── Services/
└── CRM.Tests.Integration/
    └── Admin/
        └── Controllers/
```

**Structure Decision**: Web application structure (backend API + frontend SPA). Backend follows existing Clean Architecture pattern with Domain, Application, Infrastructure, and API layers. Frontend uses Next.js App Router with TailAdmin theme, organized by feature (admin) with shared components.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

No violations - all requirements align with constitution principles.
