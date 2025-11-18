# Tasks: System Administration & Configuration Console

**Input**: Design documents from `/specs/018-system-administration-configuration/`
**Prerequisites**: plan.md ✓, spec.md ✓, research.md ✓, data-model.md ✓, contracts/ ✓

**Tests**: Tests are included as they are critical for security and compliance features. All admin endpoints must be tested for RBAC enforcement.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Backend**: `src/Backend/CRM.{Layer}/Admin/`
- **Frontend**: `src/Frontend/web/src/`
- **Tests**: `tests/CRM.Tests/Admin/` and `tests/CRM.Tests.Integration/Admin/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [ ] T001 Create Admin feature folder structure in `src/Backend/CRM.Domain/Admin/`
- [ ] T002 Create Admin feature folder structure in `src/Backend/CRM.Application/Admin/`
- [ ] T003 Create Admin feature folder structure in `src/Backend/CRM.Infrastructure/Admin/`
- [ ] T004 Create Admin feature folder structure in `src/Backend/CRM.Api/Controllers/`
- [ ] T005 [P] Create Admin feature folder structure in `src/Frontend/web/src/app/admin/`
- [ ] T006 [P] Create Admin components folder structure in `src/Frontend/web/src/components/admin/`
- [ ] T007 [P] Create Admin API client folder in `src/Frontend/web/src/lib/api/`
- [ ] T008 [P] Create Admin hooks folder in `src/Frontend/web/src/hooks/`
- [ ] T009 [P] Create Admin test folder structure in `tests/CRM.Tests/Admin/`
- [ ] T010 [P] Create Admin integration test folder structure in `tests/CRM.Tests.Integration/Admin/`
- [ ] T011 Install NuGet package `Ganss.Xss` (HtmlSanitizer) in `CRM.Infrastructure/CRM.Infrastructure.csproj`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Encryption Infrastructure

- [ ] T012 Create `IDataEncryptionService` interface in `src/Backend/CRM.Infrastructure/Admin/Encryption/IDataEncryptionService.cs`
- [ ] T013 Implement `AesDataEncryptionService` in `src/Backend/CRM.Infrastructure/Admin/Encryption/AesDataEncryptionService.cs` using AES-256-GCM
- [ ] T014 Register `IDataEncryptionService` in `src/Backend/CRM.Api/Program.cs` dependency injection

### File Storage Infrastructure

- [ ] T015 Create `IFileStorageService` interface in `src/Backend/CRM.Infrastructure/Admin/FileStorage/IFileStorageService.cs`
- [ ] T016 Implement `LocalFileStorageService` in `src/Backend/CRM.Infrastructure/Admin/FileStorage/LocalFileStorageService.cs`
- [ ] T017 Register `IFileStorageService` in `src/Backend/CRM.Api/Program.cs` dependency injection
- [ ] T018 Create upload directory `wwwroot/uploads/branding/` and ensure it exists at startup

### HTML Sanitization Infrastructure

- [ ] T019 Create `IHtmlSanitizer` interface in `src/Backend/CRM.Infrastructure/Admin/HtmlSanitization/IHtmlSanitizer.cs`
- [ ] T020 Implement `HtmlSanitizerService` in `src/Backend/CRM.Infrastructure/Admin/HtmlSanitization/HtmlSanitizerService.cs` using Ganss.Xss
- [ ] T021 Register `IHtmlSanitizer` in `src/Backend/CRM.Api/Program.cs` dependency injection

### Audit Log Infrastructure

- [ ] T022 Create `AuditLog` entity in `src/Backend/CRM.Domain/Admin/AuditLog.cs`
- [ ] T023 Create `IAuditLogService` interface in `src/Backend/CRM.Application/Admin/Services/IAuditLogService.cs`
- [ ] T024 Implement `AuditLogService` in `src/Backend/CRM.Application/Admin/Services/AuditLogService.cs`
- [ ] T025 Create database migration for `AuditLog` table in `src/Backend/CRM.Migrator/Migrations/`
- [ ] T026 Register `IAuditLogService` in `src/Backend/CRM.Api/Program.cs` dependency injection
- [ ] T027 Configure `AuditLog` entity in `AppDbContext` in `src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs`

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - System Settings Management (Priority: P1) 🎯 MVP

**Goal**: Enable admins to view and update system-wide settings (company name, date formats, currencies, notification preferences) with immediate UI updates and audit logging.

**Independent Test**: Admin logs in, navigates to `/admin/settings`, updates company name, saves, and verifies change is reflected immediately in UI and persisted after refresh. Non-admin users receive 403 Forbidden.

### Tests for User Story 1

- [ ] T028 [P] [US1] Create unit test for `UpdateSystemSettingsCommandHandler` in `tests/CRM.Tests/Admin/Commands/UpdateSystemSettingsCommandHandlerTests.cs`
- [ ] T029 [P] [US1] Create unit test for `GetSystemSettingsQueryHandler` in `tests/CRM.Tests/Admin/Queries/GetSystemSettingsQueryHandlerTests.cs`
- [ ] T030 [P] [US1] Create integration test for `GET /api/v1/admin/settings` in `tests/CRM.Tests.Integration/Admin/Controllers/AdminSettingsControllerTests.cs`
- [ ] T031 [P] [US1] Create integration test for `POST /api/v1/admin/settings` in `tests/CRM.Tests.Integration/Admin/Controllers/AdminSettingsControllerTests.cs`
- [ ] T032 [P] [US1] Create integration test for RBAC enforcement (403 for non-admin) in `tests/CRM.Tests.Integration/Admin/Controllers/AdminSettingsControllerTests.cs`

### Implementation for User Story 1

- [ ] T033 [P] [US1] Create `SystemSettings` entity in `src/Backend/CRM.Domain/Admin/SystemSettings.cs`
- [ ] T034 [P] [US1] Create `SettingsUpdated` domain event in `src/Backend/CRM.Domain/Admin/Events/SettingsUpdated.cs`
- [ ] T035 [US1] Create database migration for `SystemSettings` table in `src/Backend/CRM.Migrator/Migrations/`
- [ ] T036 [US1] Configure `SystemSettings` entity in `AppDbContext` in `src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs`
- [ ] T037 [US1] Seed default system settings in migration or seed data script
- [ ] T038 [P] [US1] Create `SystemSettingsDto` in `src/Backend/CRM.Application/Admin/DTOs/SystemSettingsDto.cs`
- [ ] T039 [P] [US1] Create `UpdateSystemSettingsRequest` in `src/Backend/CRM.Application/Admin/Requests/UpdateSystemSettingsRequest.cs`
- [ ] T040 [P] [US1] Create `UpdateSystemSettingsRequestValidator` in `src/Backend/CRM.Application/Admin/Validators/UpdateSystemSettingsRequestValidator.cs`
- [ ] T041 [US1] Create `ISystemSettingsService` interface in `src/Backend/CRM.Application/Admin/Services/ISystemSettingsService.cs`
- [ ] T042 [US1] Implement `SystemSettingsService` in `src/Backend/CRM.Application/Admin/Services/SystemSettingsService.cs` with caching
- [ ] T043 [US1] Create `GetSystemSettingsQuery` in `src/Backend/CRM.Application/Admin/Queries/GetSystemSettingsQuery.cs`
- [ ] T044 [US1] Create `GetSystemSettingsQueryHandler` in `src/Backend/CRM.Application/Admin/Queries/Handlers/GetSystemSettingsQueryHandler.cs`
- [ ] T045 [US1] Create `UpdateSystemSettingsCommand` in `src/Backend/CRM.Application/Admin/Commands/UpdateSystemSettingsCommand.cs`
- [ ] T046 [US1] Create `UpdateSystemSettingsCommandHandler` in `src/Backend/CRM.Application/Admin/Commands/Handlers/UpdateSystemSettingsCommandHandler.cs` with audit logging
- [ ] T047 [US1] Create AutoMapper profile `AdminProfile` in `src/Backend/CRM.Application/Admin/Mapping/AdminProfile.cs`
- [ ] T048 [US1] Create `AdminSettingsController` in `src/Backend/CRM.Api/Controllers/AdminSettingsController.cs` with `[Authorize(Roles = "Admin")]`
- [ ] T049 [US1] Register services and handlers in `src/Backend/CRM.Api/Program.cs`
- [ ] T050 [P] [US1] Create API client function `getSystemSettings` in `src/Frontend/web/src/lib/api/admin.ts`
- [ ] T051 [P] [US1] Create API client function `updateSystemSettings` in `src/Frontend/web/src/lib/api/admin.ts`
- [ ] T052 [P] [US1] Create React hook `useAdminSettings` in `src/Frontend/web/src/hooks/useAdminSettings.ts`
- [ ] T053 [P] [US1] Create `SettingsForm` component in `src/Frontend/web/src/components/admin/SettingsForm.tsx`
- [ ] T054 [P] [US1] Create Admin Console Home page in `src/Frontend/web/src/app/admin/settings/page.tsx`
- [ ] T055 [P] [US1] Create System Settings Editor page in `src/Frontend/web/src/app/admin/settings/system/page.tsx`
- [ ] T056 [US1] Integrate settings updates with audit log service in command handler

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - Integration Keys Management (Priority: P1)

**Goal**: Enable admins to securely manage API keys and credentials for third-party services with encryption, masking, and audit logging.

**Independent Test**: Admin adds new integration key, views it (masked), updates it, verifies it's encrypted in database. Non-admins are blocked. Audit log entries created.

### Tests for User Story 2

- [ ] T057 [P] [US2] Create unit test for encryption service in `tests/CRM.Tests/Admin/Services/AesDataEncryptionServiceTests.cs`
- [ ] T058 [P] [US2] Create unit test for `ManageIntegrationKeyCommandHandler` in `tests/CRM.Tests/Admin/Commands/ManageIntegrationKeyCommandHandlerTests.cs`
- [ ] T059 [P] [US2] Create unit test for `GetIntegrationKeysQueryHandler` in `tests/CRM.Tests/Admin/Queries/GetIntegrationKeysQueryHandlerTests.cs`
- [ ] T060 [P] [US2] Create integration test for `GET /api/v1/admin/integrations` in `tests/CRM.Tests.Integration/Admin/Controllers/AdminIntegrationsControllerTests.cs`
- [ ] T061 [P] [US2] Create integration test for `POST /api/v1/admin/integrations` in `tests/CRM.Tests.Integration/Admin/Controllers/AdminIntegrationsControllerTests.cs`
- [ ] T062 [P] [US2] Create integration test for `GET /api/v1/admin/integrations/{id}/show` in `tests/CRM.Tests.Integration/Admin/Controllers/AdminIntegrationsControllerTests.cs`
- [ ] T063 [P] [US2] Create integration test for RBAC enforcement in `tests/CRM.Tests.Integration/Admin/Controllers/AdminIntegrationsControllerTests.cs`

### Implementation for User Story 2

- [ ] T064 [P] [US2] Create `IntegrationKey` entity in `src/Backend/CRM.Domain/Admin/IntegrationKey.cs`
- [ ] T065 [P] [US2] Create `IntegrationKeyChanged` domain event in `src/Backend/CRM.Domain/Admin/Events/IntegrationKeyChanged.cs`
- [ ] T066 [US2] Create database migration for `IntegrationKeys` table in `src/Backend/CRM.Migrator/Migrations/`
- [ ] T067 [US2] Configure `IntegrationKey` entity in `AppDbContext` in `src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs`
- [ ] T068 [P] [US2] Create `IntegrationKeyDto` in `src/Backend/CRM.Application/Admin/DTOs/IntegrationKeyDto.cs` with masked key value
- [ ] T069 [P] [US2] Create `CreateIntegrationKeyRequest` in `src/Backend/CRM.Application/Admin/Requests/CreateIntegrationKeyRequest.cs`
- [ ] T070 [P] [US2] Create `UpdateIntegrationKeyRequest` in `src/Backend/CRM.Application/Admin/Requests/UpdateIntegrationKeyRequest.cs`
- [ ] T071 [P] [US2] Create `CreateIntegrationKeyRequestValidator` in `src/Backend/CRM.Application/Admin/Validators/CreateIntegrationKeyRequestValidator.cs`
- [ ] T072 [US2] Create `IIntegrationKeyService` interface in `src/Backend/CRM.Application/Admin/Services/IIntegrationKeyService.cs`
- [ ] T073 [US2] Implement `IntegrationKeyService` in `src/Backend/CRM.Application/Admin/Services/IntegrationKeyService.cs` with encryption/decryption
- [ ] T074 [US2] Create `GetIntegrationKeysQuery` in `src/Backend/CRM.Application/Admin/Queries/GetIntegrationKeysQuery.cs`
- [ ] T075 [US2] Create `GetIntegrationKeysQueryHandler` in `src/Backend/CRM.Application/Admin/Queries/Handlers/GetIntegrationKeysQueryHandler.cs`
- [ ] T076 [US2] Create `ManageIntegrationKeyCommand` in `src/Backend/CRM.Application/Admin/Commands/ManageIntegrationKeyCommand.cs`
- [ ] T077 [US2] Create `ManageIntegrationKeyCommandHandler` in `src/Backend/CRM.Application/Admin/Commands/Handlers/ManageIntegrationKeyCommandHandler.cs` with audit logging
- [ ] T078 [US2] Update AutoMapper profile `AdminProfile` for IntegrationKey mappings
- [ ] T079 [US2] Create `AdminIntegrationsController` in `src/Backend/CRM.Api/Controllers/AdminIntegrationsController.cs` with `[Authorize(Roles = "Admin")]`
- [ ] T080 [US2] Register integration key services in `src/Backend/CRM.Api/Program.cs`
- [ ] T081 [P] [US2] Create API client functions for integration keys in `src/Frontend/web/src/lib/api/admin.ts`
- [ ] T082 [P] [US2] Create React hook `useIntegrationKeys` in `src/Frontend/web/src/hooks/useIntegrationKeys.ts`
- [ ] T083 [P] [US2] Create `ApiKeyList` component in `src/Frontend/web/src/components/admin/ApiKeyList.tsx`
- [ ] T084 [P] [US2] Create `ApiKeyEditDialog` component in `src/Frontend/web/src/components/admin/ApiKeyEditDialog.tsx`
- [ ] T085 [P] [US2] Create Integration Keys Manager page in `src/Frontend/web/src/app/admin/integrations/page.tsx`

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 - Audit Log Viewing (Priority: P2)

**Goal**: Enable admins to view, search, filter, and export audit logs of all system actions for security monitoring and compliance.

**Independent Test**: Admin performs actions (update settings, manage keys), views audit log, filters by user/date/action, views details, exports CSV. All actions are logged correctly.

### Tests for User Story 3

- [ ] T086 [P] [US3] Create unit test for `GetAuditLogsQueryHandler` in `tests/CRM.Tests/Admin/Queries/GetAuditLogsQueryHandlerTests.cs`
- [ ] T087 [P] [US3] Create integration test for `GET /api/v1/admin/audit-logs` with filters in `tests/CRM.Tests.Integration/Admin/Controllers/AdminAuditLogControllerTests.cs`
- [ ] T088 [P] [US3] Create integration test for `GET /api/v1/admin/audit-logs/{id}` in `tests/CRM.Tests.Integration/Admin/Controllers/AdminAuditLogControllerTests.cs`
- [ ] T089 [P] [US3] Create integration test for `GET /api/v1/admin/audit-logs/export` in `tests/CRM.Tests.Integration/Admin/Controllers/AdminAuditLogControllerTests.cs`

### Implementation for User Story 3

- [ ] T090 [P] [US3] Create `AuditLogDto` in `src/Backend/CRM.Application/Admin/DTOs/AuditLogDto.cs`
- [ ] T091 [US3] Create `GetAuditLogsQuery` in `src/Backend/CRM.Application/Admin/Queries/GetAuditLogsQuery.cs` with filter parameters
- [ ] T092 [US3] Create `GetAuditLogsQueryHandler` in `src/Backend/CRM.Application/Admin/Queries/Handlers/GetAuditLogsQueryHandler.cs` with pagination
- [ ] T093 [US3] Create `GetAuditLogByIdQuery` in `src/Backend/CRM.Application/Admin/Queries/GetAuditLogByIdQuery.cs`
- [ ] T094 [US3] Create `GetAuditLogByIdQueryHandler` in `src/Backend/CRM.Application/Admin/Queries/Handlers/GetAuditLogByIdQueryHandler.cs`
- [ ] T095 [US3] Update AutoMapper profile `AdminProfile` for AuditLog mappings
- [ ] T096 [US3] Create `AdminAuditLogController` in `src/Backend/CRM.Api/Controllers/AdminAuditLogController.cs` with `[Authorize(Roles = "Admin")]`
- [ ] T097 [US3] Implement CSV export functionality in `AdminAuditLogController.ExportAuditLogs` method
- [ ] T098 [P] [US3] Create API client functions for audit logs in `src/Frontend/web/src/lib/api/admin.ts`
- [ ] T099 [P] [US3] Create React hook `useAuditLogs` in `src/Frontend/web/src/hooks/useAuditLogs.ts`
- [ ] T100 [P] [US3] Create `AuditLogTable` component in `src/Frontend/web/src/components/admin/AuditLogTable.tsx`
- [ ] T101 [P] [US3] Create `AuditLogFilter` component in `src/Frontend/web/src/components/admin/AuditLogFilter.tsx`
- [ ] T102 [P] [US3] Create `LogDetailModal` component in `src/Frontend/web/src/components/admin/LogDetailModal.tsx`
- [ ] T103 [P] [US3] Create Audit Log Browser page in `src/Frontend/web/src/app/admin/audit-logs/page.tsx`

**Checkpoint**: At this point, User Stories 1, 2, AND 3 should all work independently

---

## Phase 6: User Story 4 - Custom Branding Management (Priority: P2)

**Goal**: Enable admins to customize application branding (logo, colors, footer) with file upload, live preview, and immediate application-wide updates.

**Independent Test**: Admin uploads logo, changes colors, updates footer HTML, verifies changes appear immediately in preview and across application.

### Tests for User Story 4

- [ ] T104 [P] [US4] Create unit test for `UpdateBrandingCommandHandler` in `tests/CRM.Tests/Admin/Commands/UpdateBrandingCommandHandlerTests.cs`
- [ ] T105 [P] [US4] Create unit test for file storage service in `tests/CRM.Tests/Admin/Services/LocalFileStorageServiceTests.cs`
- [ ] T106 [P] [US4] Create unit test for HTML sanitization in `tests/CRM.Tests/Admin/Services/HtmlSanitizerServiceTests.cs`
- [ ] T107 [P] [US4] Create integration test for `POST /api/v1/admin/branding/logo` in `tests/CRM.Tests.Integration/Admin/Controllers/AdminBrandingControllerTests.cs`
- [ ] T108 [P] [US4] Create integration test for `POST /api/v1/admin/branding` in `tests/CRM.Tests.Integration/Admin/Controllers/AdminBrandingControllerTests.cs`

### Implementation for User Story 4

- [ ] T109 [P] [US4] Create `CustomBranding` entity in `src/Backend/CRM.Domain/Admin/CustomBranding.cs`
- [ ] T110 [P] [US4] Create `BrandingChanged` domain event in `src/Backend/CRM.Domain/Admin/Events/BrandingChanged.cs`
- [ ] T111 [US4] Create database migration for `CustomBranding` table in `src/Backend/CRM.Migrator/Migrations/`
- [ ] T112 [US4] Configure `CustomBranding` entity in `AppDbContext` in `src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs`
- [ ] T113 [US4] Seed default branding configuration in migration or seed data script
- [ ] T114 [P] [US4] Create `BrandingDto` in `src/Backend/CRM.Application/Admin/DTOs/BrandingDto.cs`
- [ ] T115 [P] [US4] Create `UpdateBrandingRequest` in `src/Backend/CRM.Application/Admin/Requests/UpdateBrandingRequest.cs`
- [ ] T116 [P] [US4] Create `UpdateBrandingRequestValidator` in `src/Backend/CRM.Application/Admin/Validators/UpdateBrandingRequestValidator.cs`
- [ ] T117 [US4] Create `IBrandingService` interface in `src/Backend/CRM.Application/Admin/Services/IBrandingService.cs`
- [ ] T118 [US4] Implement `BrandingService` in `src/Backend/CRM.Application/Admin/Services/BrandingService.cs` with HTML sanitization
- [ ] T119 [US4] Create `GetBrandingQuery` in `src/Backend/CRM.Application/Admin/Queries/GetBrandingQuery.cs`
- [ ] T120 [US4] Create `GetBrandingQueryHandler` in `src/Backend/CRM.Application/Admin/Queries/Handlers/GetBrandingQueryHandler.cs`
- [ ] T121 [US4] Create `UpdateBrandingCommand` in `src/Backend/CRM.Application/Admin/Commands/UpdateBrandingCommand.cs`
- [ ] T122 [US4] Create `UpdateBrandingCommandHandler` in `src/Backend/CRM.Application/Admin/Commands/Handlers/UpdateBrandingCommandHandler.cs` with audit logging
- [ ] T123 [US4] Update AutoMapper profile `AdminProfile` for Branding mappings
- [ ] T124 [US4] Create `AdminBrandingController` in `src/Backend/CRM.Api/Controllers/AdminBrandingController.cs` with `[Authorize(Roles = "Admin")]`
- [ ] T125 [US4] Implement logo upload endpoint in `AdminBrandingController.UploadLogo` with file validation
- [ ] T126 [US4] Register branding services in `src/Backend/CRM.Api/Program.cs`
- [ ] T127 [P] [US4] Create API client functions for branding in `src/Frontend/web/src/lib/api/admin.ts`
- [ ] T128 [P] [US4] Create React hook `useBranding` in `src/Frontend/web/src/hooks/useBranding.ts`
- [ ] T129 [P] [US4] Create `BrandingUploader` component in `src/Frontend/web/src/components/admin/BrandingUploader.tsx`
- [ ] T130 [P] [US4] Create `ColorPicker` component in `src/Frontend/web/src/components/admin/ColorPicker.tsx`
- [ ] T131 [P] [US4] Create `LivePreview` component in `src/Frontend/web/src/components/admin/LivePreview.tsx`
- [ ] T132 [P] [US4] Create Branding Editor page in `src/Frontend/web/src/app/admin/branding/page.tsx`

**Checkpoint**: At this point, User Stories 1, 2, 3, AND 4 should all work independently

---

## Phase 7: User Story 5 - Data Retention & Compliance Settings (Priority: P3)

**Goal**: Enable admins to configure data retention policies for different entity types with warnings for destructive actions.

**Independent Test**: Admin sets retention periods, enables auto-purge, verifies warnings appear, confirms, and policy is saved.

### Tests for User Story 5

- [ ] T133 [P] [US5] Create unit test for `UpdateRetentionPolicyCommandHandler` in `tests/CRM.Tests/Admin/Commands/UpdateRetentionPolicyCommandHandlerTests.cs`
- [ ] T134 [P] [US5] Create integration test for `GET /api/v1/admin/data-retention` in `tests/CRM.Tests.Integration/Admin/Controllers/AdminRetentionControllerTests.cs`
- [ ] T135 [P] [US5] Create integration test for `POST /api/v1/admin/data-retention` in `tests/CRM.Tests.Integration/Admin/Controllers/AdminRetentionControllerTests.cs`

### Implementation for User Story 5

- [ ] T136 [P] [US5] Create `DataRetentionPolicy` entity in `src/Backend/CRM.Domain/Admin/DataRetentionPolicy.cs`
- [ ] T137 [P] [US5] Create `RetentionPolicyChanged` domain event in `src/Backend/CRM.Domain/Admin/Events/RetentionPolicyChanged.cs`
- [ ] T138 [US5] Create database migration for `DataRetentionPolicy` table in `src/Backend/CRM.Migrator/Migrations/`
- [ ] T139 [US5] Configure `DataRetentionPolicy` entity in `AppDbContext` in `src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs`
- [ ] T140 [US5] Seed default retention policies in migration or seed data script
- [ ] T141 [P] [US5] Create `RetentionPolicyDto` in `src/Backend/CRM.Application/Admin/DTOs/RetentionPolicyDto.cs`
- [ ] T142 [P] [US5] Create `UpdateRetentionPolicyRequest` in `src/Backend/CRM.Application/Admin/Requests/UpdateRetentionPolicyRequest.cs`
- [ ] T143 [P] [US5] Create `UpdateRetentionPolicyRequestValidator` in `src/Backend/CRM.Application/Admin/Validators/UpdateRetentionPolicyRequestValidator.cs`
- [ ] T144 [US5] Create `IRetentionPolicyService` interface in `src/Backend/CRM.Application/Admin/Services/IRetentionPolicyService.cs`
- [ ] T145 [US5] Implement `RetentionPolicyService` in `src/Backend/CRM.Application/Admin/Services/RetentionPolicyService.cs`
- [ ] T146 [US5] Create `GetRetentionPoliciesQuery` in `src/Backend/CRM.Application/Admin/Queries/GetRetentionPoliciesQuery.cs`
- [ ] T147 [US5] Create `GetRetentionPoliciesQueryHandler` in `src/Backend/CRM.Application/Admin/Queries/Handlers/GetRetentionPoliciesQueryHandler.cs`
- [ ] T148 [US5] Create `UpdateRetentionPolicyCommand` in `src/Backend/CRM.Application/Admin/Commands/UpdateRetentionPolicyCommand.cs`
- [ ] T149 [US5] Create `UpdateRetentionPolicyCommandHandler` in `src/Backend/CRM.Application/Admin/Commands/Handlers/UpdateRetentionPolicyCommandHandler.cs` with audit logging
- [ ] T150 [US5] Update AutoMapper profile `AdminProfile` for RetentionPolicy mappings
- [ ] T151 [US5] Create `AdminRetentionController` in `src/Backend/CRM.Api/Controllers/AdminRetentionController.cs` with `[Authorize(Roles = "Admin")]`
- [ ] T152 [US5] Register retention policy services in `src/Backend/CRM.Api/Program.cs`
- [ ] T153 [P] [US5] Create API client functions for retention policies in `src/Frontend/web/src/lib/api/admin.ts`
- [ ] T154 [P] [US5] Create React hook `useRetentionPolicies` in `src/Frontend/web/src/hooks/useRetentionPolicies.ts`
- [ ] T155 [P] [US5] Create `RetentionPolicyTable` component in `src/Frontend/web/src/components/admin/RetentionPolicyTable.tsx`
- [ ] T156 [P] [US5] Create `RetentionEditDialog` component in `src/Frontend/web/src/components/admin/RetentionEditDialog.tsx`
- [ ] T157 [P] [US5] Create Data Retention Settings page in `src/Frontend/web/src/app/admin/data-retention/page.tsx`

**Checkpoint**: At this point, User Stories 1-5 should all work independently

---

## Phase 8: User Story 6 - Global System Messages (Priority: P3)

**Goal**: Enable admins to set global banner messages that appear to all users with preview and toggle visibility.

**Independent Test**: Admin sets banner message, toggles visibility, verifies it appears/disappears for all users immediately.

### Tests for User Story 6

- [ ] T158 [P] [US6] Create unit test for `UpdateNotificationSettingsCommandHandler` in `tests/CRM.Tests/Admin/Commands/UpdateNotificationSettingsCommandHandlerTests.cs`
- [ ] T159 [P] [US6] Create integration test for `GET /api/v1/admin/notification-settings` in `tests/CRM.Tests.Integration/Admin/Controllers/AdminNotificationsControllerTests.cs`
- [ ] T160 [P] [US6] Create integration test for `POST /api/v1/admin/notification-settings` in `tests/CRM.Tests.Integration/Admin/Controllers/AdminNotificationsControllerTests.cs`

### Implementation for User Story 6

- [ ] T161 [P] [US6] Create `NotificationSettings` entity in `src/Backend/CRM.Domain/Admin/NotificationSettings.cs` (or add to existing table)
- [ ] T162 [US6] Create database migration for `NotificationSettings` table in `src/Backend/CRM.Migrator/Migrations/`
- [ ] T163 [US6] Configure `NotificationSettings` entity in `AppDbContext` in `src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs`
- [ ] T164 [P] [US6] Create `NotificationSettingsDto` in `src/Backend/CRM.Application/Admin/DTOs/NotificationSettingsDto.cs`
- [ ] T165 [P] [US6] Create `UpdateNotificationSettingsRequest` in `src/Backend/CRM.Application/Admin/Requests/UpdateNotificationSettingsRequest.cs`
- [ ] T166 [P] [US6] Create `UpdateNotificationSettingsRequestValidator` in `src/Backend/CRM.Application/Admin/Validators/UpdateNotificationSettingsRequestValidator.cs`
- [ ] T167 [US6] Create `INotificationSettingsService` interface in `src/Backend/CRM.Application/Admin/Services/INotificationSettingsService.cs`
- [ ] T168 [US6] Implement `NotificationSettingsService` in `src/Backend/CRM.Application/Admin/Services/NotificationSettingsService.cs` with HTML sanitization
- [ ] T169 [US6] Create `GetNotificationSettingsQuery` in `src/Backend/CRM.Application/Admin/Queries/GetNotificationSettingsQuery.cs`
- [ ] T170 [US6] Create `GetNotificationSettingsQueryHandler` in `src/Backend/CRM.Application/Admin/Queries/Handlers/GetNotificationSettingsQueryHandler.cs`
- [ ] T171 [US6] Create `UpdateNotificationSettingsCommand` in `src/Backend/CRM.Application/Admin/Commands/UpdateNotificationSettingsCommand.cs`
- [ ] T172 [US6] Create `UpdateNotificationSettingsCommandHandler` in `src/Backend/CRM.Application/Admin/Commands/Handlers/UpdateNotificationSettingsCommandHandler.cs` with audit logging
- [ ] T173 [US6] Update AutoMapper profile `AdminProfile` for NotificationSettings mappings
- [ ] T174 [US6] Create `AdminNotificationsController` in `src/Backend/CRM.Api/Controllers/AdminNotificationsController.cs` with `[Authorize(Roles = "Admin")]`
- [ ] T175 [US6] Register notification settings services in `src/Backend/CRM.Api/Program.cs`
- [ ] T176 [P] [US6] Create API client functions for notification settings in `src/Frontend/web/src/lib/api/admin.ts`
- [ ] T177 [P] [US6] Create React hook `useNotificationSettings` in `src/Frontend/web/src/hooks/useNotificationSettings.ts`
- [ ] T178 [P] [US6] Create `SystemBannerSet` component in `src/Frontend/web/src/components/admin/SystemBannerSet.tsx`
- [ ] T179 [P] [US6] Create `PreviewBanner` component in `src/Frontend/web/src/components/admin/PreviewBanner.tsx`
- [ ] T180 [P] [US6] Create Global Message Settings page in `src/Frontend/web/src/app/admin/notifications/page.tsx`
- [ ] T181 [US6] Create global banner component to display active banner on all pages in `src/Frontend/web/src/components/admin/GlobalBanner.tsx`

**Checkpoint**: All user stories should now be independently functional

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T182 [P] Update Admin Console Home page with summary statistics and quick links in `src/Frontend/web/src/app/admin/settings/page.tsx`
- [ ] T183 [P] Add loading states and error handling to all admin pages
- [ ] T184 [P] Add toast notifications for all save operations
- [ ] T185 [P] Implement React Query cache invalidation on settings updates
- [ ] T186 [P] Add responsive design for mobile/tablet access
- [ ] T187 [P] Add accessibility compliance (WCAG 2.1 AA) to all admin components
- [ ] T188 [P] Create background job for data retention policy enforcement in `src/Backend/CRM.Infrastructure/Admin/Jobs/DataRetentionJob.cs`
- [ ] T189 [P] Register background job in `src/Backend/CRM.Api/Program.cs`
- [ ] T190 [P] Add rate limiting to admin endpoints
- [ ] T191 [P] Add CSRF protection to state-changing admin operations
- [ ] T192 [P] Update API documentation (Swagger) with admin endpoints
- [ ] T193 [P] Run quickstart.md validation scenarios
- [ ] T194 [P] Code cleanup and refactoring
- [ ] T195 [P] Performance optimization (caching, query optimization)
- [ ] T196 [P] Security hardening review

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - **BLOCKS all user stories**
- **User Stories (Phase 3-8)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Polish (Phase 9)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories (uses shared encryption infrastructure)
- **User Story 3 (P2)**: Can start after Foundational (Phase 2) - Uses audit log infrastructure from Phase 2
- **User Story 4 (P2)**: Can start after Foundational (Phase 2) - Uses file storage and HTML sanitization from Phase 2
- **User Story 5 (P3)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 6 (P3)**: Can start after Foundational (Phase 2) - Uses HTML sanitization from Phase 2

### Within Each User Story

- Tests (if included) MUST be written and FAIL before implementation
- Domain entities before DTOs/Requests
- DTOs/Requests before Validators
- Validators before Commands/Queries
- Commands/Queries before Handlers
- Handlers before Controllers
- Backend before Frontend
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational phase completes, all user stories can start in parallel (if team capacity allows)
- All tests for a user story marked [P] can run in parallel
- Domain entities within a story marked [P] can run in parallel
- DTOs/Requests within a story marked [P] can run in parallel
- Frontend components marked [P] can run in parallel
- Different user stories can be worked on in parallel by different team members

---

## Parallel Example: User Story 1

```bash
# Launch all tests for User Story 1 together:
Task: "Create unit test for UpdateSystemSettingsCommandHandler"
Task: "Create unit test for GetSystemSettingsQueryHandler"
Task: "Create integration test for GET /api/v1/admin/settings"
Task: "Create integration test for POST /api/v1/admin/settings"
Task: "Create integration test for RBAC enforcement"

# Launch all domain entities together:
Task: "Create SystemSettings entity"
Task: "Create SettingsUpdated domain event"

# Launch all DTOs/Requests together:
Task: "Create SystemSettingsDto"
Task: "Create UpdateSystemSettingsRequest"
Task: "Create UpdateSystemSettingsRequestValidator"

# Launch all frontend components together:
Task: "Create API client functions"
Task: "Create React hook useAdminSettings"
Task: "Create SettingsForm component"
Task: "Create Admin Console Home page"
Task: "Create System Settings Editor page"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1 (System Settings Management)
4. **STOP and VALIDATE**: Test User Story 1 independently
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 → Test independently → Deploy/Demo
4. Add User Story 3 → Test independently → Deploy/Demo
5. Add User Story 4 → Test independently → Deploy/Demo
6. Add User Story 5 → Test independently → Deploy/Demo
7. Add User Story 6 → Test independently → Deploy/Demo
8. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (System Settings)
   - Developer B: User Story 2 (Integration Keys)
   - Developer C: User Story 3 (Audit Logs)
3. Next iteration:
   - Developer A: User Story 4 (Branding)
   - Developer B: User Story 5 (Data Retention)
   - Developer C: User Story 6 (Global Messages)
4. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
- All admin endpoints MUST enforce RBAC with `[Authorize(Roles = "Admin")]`
- All sensitive operations MUST be logged to audit log
- Integration keys MUST be encrypted at rest
- HTML content MUST be sanitized before storage

---

## Summary

- **Total Tasks**: 196
- **Tasks per User Story**:
  - User Story 1 (P1): 29 tasks
  - User Story 2 (P1): 23 tasks
  - User Story 3 (P2): 18 tasks
  - User Story 4 (P2): 29 tasks
  - User Story 5 (P3): 25 tasks
  - User Story 6 (P3): 24 tasks
  - Setup: 11 tasks
  - Foundational: 16 tasks
  - Polish: 15 tasks

- **Parallel Opportunities**: High - Most tasks within each phase can run in parallel
- **Independent Test Criteria**: Each user story has clear acceptance scenarios and can be tested independently
- **Suggested MVP Scope**: Phase 1 (Setup) + Phase 2 (Foundational) + Phase 3 (User Story 1 - System Settings Management)
- **Format Validation**: ✅ All tasks follow checklist format with checkbox, ID, labels, and file paths

