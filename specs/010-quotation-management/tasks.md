# Tasks: Quotation Management (Spec-010)

**Input**: Design documents from `/specs/010-quotation-management/`  
**Prerequisites**: `spec.md`, `plan.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md`

**Tests**: Critical flows include unit and integration coverage per user story (send, track views, client response, PDF generation, email delivery). Frontend E2E tests for complete send → view → respond workflow.

## Phase 1: Setup & Foundational (Blocking Prerequisites)

**Purpose**: Core schema, entities, DTOs, migrations, PDF/Email services, and configuration consumed by all user stories.

- [ ] T001 Confirm feature branch `010-quotation-management` is active and linked to Spec-010 artifacts.
- [ ] T002 Install dependencies: QuestPDF, FluentEmail.Core, FluentEmail.Smtp, FluentEmail.Razor in `src/Backend/CRM.Application/CRM.Application.csproj`.
- [ ] T003 Add `Email` config section (SMTP settings) to `src/Backend/CRM.Api/appsettings.json`.
- [ ] T004 Add `QuotationManagement` config section (access link expiration, PDF cache, base URL) to `src/Backend/CRM.Api/appsettings.json`.
- [ ] T005 Add `QuotationAccessLink` entity to `src/Backend/CRM.Domain/Entities/QuotationAccessLink.cs` per data-model definitions (12 properties).
- [ ] T006 Add `QuotationStatusHistory` entity to `src/Backend/CRM.Domain/Entities/QuotationStatusHistory.cs` per data-model definitions (8 properties).
- [ ] T007 Add `QuotationResponse` entity to `src/Backend/CRM.Domain/Entities/QuotationResponse.cs` per data-model definitions (10 properties).
- [ ] T008 Create EF configurations (`QuotationAccessLinkEntityConfiguration.cs`, `QuotationStatusHistoryEntityConfiguration.cs`, `QuotationResponseEntityConfiguration.cs`) under `src/Backend/CRM.Infrastructure/EntityConfigurations/`.
- [ ] T009 Add migration `src/Backend/CRM.Infrastructure/Migrations/<timestamp>_CreateQuotationManagementTables.cs` with tables, indexes, foreign keys, and constraints.
- [ ] T010 Update `src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs` to register DbSets for `QuotationAccessLinks`, `QuotationStatusHistory`, and `QuotationResponses`.
- [ ] T011 Update `src/Backend/CRM.Application/Common/Persistence/IAppDbContext.cs` to add DbSet properties for new entities.
- [ ] T012 Create DTOs (`QuotationAccessLinkDto`, `QuotationStatusHistoryDto`, `QuotationResponseDto`, `SendQuotationRequest`, `SubmitQuotationResponseRequest`, `PublicQuotationDto`) in `src/Backend/CRM.Application/Quotations/Dtos/`.
- [ ] T013 Add AutoMapper profile `src/Backend/CRM.Application/Mapping/QuotationManagementProfile.cs` for new entity/DTO mappings.
- [ ] T014 Create `QuotationPdfGenerationService` in `src/Backend/CRM.Application/Quotations/Services/QuotationPdfGenerationService.cs` using QuestPDF.
- [ ] T015 Create `QuotationEmailService` in `src/Backend/CRM.Application/Quotations/Services/QuotationEmailService.cs` using FluentEmail.
- [ ] T016 Create `AccessTokenGenerator` helper in `src/Backend/CRM.Application/Quotations/Services/AccessTokenGenerator.cs` with cryptographically secure token generation.
- [ ] T017 Create email templates in `src/Backend/CRM.Infrastructure/Notifications/Templates/`:
  - `QuotationSentEmail.html`
  - `QuotationAcceptedNotification.html`
  - `QuotationRejectedNotification.html`
  - `QuotationExpirationReminder.html`
- [ ] T018 Register FluentEmail services in `src/Backend/CRM.Api/Program.cs` with SMTP configuration.
- [ ] T019 Register QuestPDF and PDF generation service in `src/Backend/CRM.Api/Program.cs`.
- [ ] T020 Wire contracts into Swagger by referencing `specs/010-quotation-management/contracts/quotation-management.openapi.yaml` inside `src/Backend/CRM.Api/Program.cs`.

**Checkpoint**: Quotation management schema, DTOs, services, and config ready—user stories can proceed independently.

---

## Phase 2: Backend Commands - Send & Track

**Purpose**: Implement commands for sending quotations, tracking views, and managing status transitions.

### User Story 1 - Send Quotation to Client (Priority: P1) 🎯 MVP

**Goal**: SalesReps send professional quotations via email with PDF and secure access link.

**Independent Test**: `POST /api/v1/quotations/{id}/send` sends email, generates access link, updates status to SENT.

- [ ] T021 [P] [US1] Add `SendQuotationCommand`, validator, and handler in `src/Backend/CRM.Application/Quotations/Commands/SendQuotationCommand*.cs`.
- [ ] T022 [US1] Implement handler logic: verify quotation status is DRAFT, generate access link, generate PDF, send email, update status to SENT, create status history.
- [ ] T023 [US1] Add domain event `QuotationSent` in `src/Backend/CRM.Domain/Events/QuotationSent.cs` and publish in handler.
- [ ] T024 [US1] Add validation: reject sending non-DRAFT quotations, validate email addresses, ensure quotation has line items.
- [ ] T025 [P] [US1] Add unit tests for `SendQuotationCommandHandler` in `tests/CRM.Tests/Quotations/SendQuotationCommandHandlerTests.cs` (8-10 tests).
- [ ] T026 [US1] Add unit tests for `AccessTokenGenerator` in `tests/CRM.Tests/Quotations/AccessTokenGeneratorTests.cs` (token uniqueness, security).

**Checkpoint**: Send quotation endpoint operational with email delivery.

---

### User Story 2 - Track Quotation Views (Priority: P1)

**Goal**: System tracks when client opens access link and updates status automatically.

**Independent Test**: Client opening access link triggers view tracking and status change SENT → VIEWED.

- [ ] T027 [P] [US2] Add `MarkQuotationAsViewedCommand`, validator, and handler in `src/Backend/CRM.Application/Quotations/Commands/MarkQuotationAsViewedCommand*.cs`.
- [ ] T028 [US2] Implement handler logic: verify access link exists and is valid, update FirstViewedAt/LastViewedAt, increment ViewCount, update IP address, change status SENT → VIEWED if needed.
- [ ] T029 [US2] Add domain event `QuotationViewed` in `src/Backend/CRM.Domain/Events/QuotationViewed.cs` and publish in handler.
- [ ] T030 [US2] Create status history record when status changes to VIEWED.
- [ ] T031 [P] [US2] Add unit tests for `MarkQuotationAsViewedCommandHandler` in `tests/CRM.Tests/Quotations/MarkQuotationAsViewedCommandHandlerTests.cs` (6-8 tests).

**Checkpoint**: View tracking operational with automatic status updates.

---

### User Story 3 - Client Responds to Quotation (Priority: P1)

**Goal**: Clients can accept, reject, or request modification via public portal.

**Independent Test**: `POST /api/v1/client-portal/quotations/{id}/{token}/respond` records response and updates status.

- [ ] T032 [P] [US3] Add `SubmitQuotationResponseCommand`, validator, and handler in `src/Backend/CRM.Application/Quotations/Commands/SubmitQuotationResponseCommand*.cs`.
- [ ] T033 [US3] Implement handler logic: verify access link, prevent duplicate responses, create QuotationResponse record, update quotation status based on response type, create status history, send notification email to sales rep.
- [ ] T034 [US3] Add domain event `QuotationResponseReceived` in `src/Backend/CRM.Domain/Events/QuotationResponseReceived.cs` and publish in handler.
- [ ] T035 [US3] Add validation: response type must be ACCEPTED/REJECTED/NEEDS_MODIFICATION, prevent duplicate responses.
- [ ] T036 [P] [US3] Add unit tests for `SubmitQuotationResponseCommandHandler` in `tests/CRM.Tests/Quotations/SubmitQuotationResponseCommandHandlerTests.cs` (8-10 tests).

**Checkpoint**: Client response handling operational with notifications.

---

### User Story 4 - Automatic Expiration (Priority: P2)

**Goal**: System automatically marks expired quotations via background job.

**Independent Test**: Background job marks quotations past ValidUntil date as EXPIRED.

- [ ] T037 [P] [US4] Add `MarkQuotationAsExpiredCommand`, validator, and handler in `src/Backend/CRM.Application/Quotations/Commands/MarkQuotationAsExpiredCommand*.cs`.
- [ ] T038 [US4] Implement handler logic: verify ValidUntil < today, set status to EXPIRED, create status history, emit event.
- [ ] T039 [US4] Add domain event `QuotationExpired` in `src/Backend/CRM.Domain/Events/QuotationExpired.cs` and publish in handler.
- [ ] T040 [P] [US4] Add unit tests for `MarkQuotationAsExpiredCommandHandler` in `tests/CRM.Tests/Quotations/MarkQuotationAsExpiredCommandHandlerTests.cs` (4-6 tests).

**Checkpoint**: Expiration command operational (background job in Phase 5).

---

### User Story 5 - Resend Quotation (Priority: P2)

**Goal**: SalesReps can resend quotations to same or different email.

**Independent Test**: `POST /api/v1/quotations/{id}/resend` generates new access link and sends email.

- [ ] T041 [P] [US5] Add `ResendQuotationCommand`, validator, and handler in `src/Backend/CRM.Application/Quotations/Commands/ResendQuotationCommand*.cs`.
- [ ] T042 [US5] Implement handler logic: verify quotation status allows resend, generate new access link (or reuse if same email), send email, create status history.
- [ ] T043 [US5] Add domain event `QuotationResent` in `src/Backend/CRM.Domain/Events/QuotationResent.cs` and publish in handler.
- [ ] T044 [P] [US5] Add unit tests for `ResendQuotationCommandHandler` in `tests/CRM.Tests/Quotations/ResendQuotationCommandHandlerTests.cs` (4-6 tests).

**Checkpoint**: Resend functionality operational.

---

## Phase 3: Backend Queries

**Purpose**: Implement queries for status history, responses, and access link information.

### User Story 6 - View Status History (Priority: P1)

**Goal**: SalesReps/Admins view complete timeline of quotation status changes.

**Independent Test**: `GET /api/v1/quotations/{id}/status-history` returns ordered list of status changes.

- [ ] T045 [P] [US6] Add `GetQuotationStatusHistoryQuery`, validator, and handler in `src/Backend/CRM.Application/Quotations/Queries/GetQuotationStatusHistoryQuery*.cs`.
- [ ] T046 [US6] Implement handler logic: query QuotationStatusHistory by QuotationId, order by ChangedAt DESC, map to DTOs with user names.
- [ ] T047 [US6] Add authorization: SalesRep sees only own quotations, Admin sees all.
- [ ] T048 [P] [US6] Add unit tests for `GetQuotationStatusHistoryQueryHandler` in `tests/CRM.Tests/Quotations/GetQuotationStatusHistoryQueryHandlerTests.cs` (4-6 tests).

**Checkpoint**: Status history query operational.

---

### User Story 7 - Get Client Response (Priority: P1)

**Goal**: SalesReps view client's response to quotation.

**Independent Test**: `GET /api/v1/quotations/{id}/response` returns response or 204 if none.

- [ ] T049 [P] [US7] Add `GetQuotationResponseQuery`, validator, and handler in `src/Backend/CRM.Application/Quotations/Queries/GetQuotationResponseQuery*.cs`.
- [ ] T050 [US7] Implement handler logic: query QuotationResponses by QuotationId, return DTO or null.
- [ ] T051 [US7] Add authorization: SalesRep sees only own quotations, Admin sees all.
- [ ] T052 [P] [US7] Add unit tests for `GetQuotationResponseQueryHandler` in `tests/CRM.Tests/Quotations/GetQuotationResponseQueryHandlerTests.cs` (3-4 tests).

**Checkpoint**: Response query operational.

---

### User Story 8 - Get Access Link Info (Priority: P2)

**Goal**: SalesReps can view and share access link information.

**Independent Test**: `GET /api/v1/quotations/{id}/access-link` returns access link details.

- [ ] T053 [P] [US8] Add `GetQuotationAccessLinkQuery`, validator, and handler in `src/Backend/CRM.Application/Quotations/Queries/GetQuotationAccessLinkQuery*.cs`.
- [ ] T054 [US8] Implement handler logic: query QuotationAccessLinks by QuotationId, build view URL, return DTO.
- [ ] T055 [US8] Add authorization: SalesRep sees only own quotations, Admin sees all.
- [ ] T056 [P] [US8] Add unit tests for `GetQuotationAccessLinkQueryHandler` in `tests/CRM.Tests/Quotations/GetQuotationAccessLinkQueryHandlerTests.cs` (3-4 tests).

**Checkpoint**: Access link query operational.

---

### User Story 9 - Get Quotation by Access Token (Priority: P1)

**Goal**: Public endpoint to retrieve quotation for client portal without authentication.

**Independent Test**: `GET /api/v1/client-portal/quotations/{id}/{token}` returns public quotation data.

- [ ] T057 [P] [US9] Add `GetQuotationByAccessTokenQuery`, validator, and handler in `src/Backend/CRM.Application/Quotations/Queries/GetQuotationByAccessTokenQuery*.cs`.
- [ ] T058 [US9] Implement handler logic: verify access link exists and is valid (IsActive, not expired), load quotation, return public DTO (no internal notes/creator info), trigger view tracking (background).
- [ ] T059 [US9] Add validation: reject expired or inactive tokens, verify QuotationId matches.
- [ ] T060 [P] [US9] Add unit tests for `GetQuotationByAccessTokenQueryHandler` in `tests/CRM.Tests/Quotations/GetQuotationByAccessTokenQueryHandlerTests.cs` (6-8 tests).

**Checkpoint**: Public quotation retrieval operational.

---

## Phase 4: API Endpoints

**Purpose**: Expose commands and queries via REST API with proper authorization.

### Sales Rep Endpoints (Authenticated)

- [ ] T061 [P] [US1] Extend `QuotationsController` in `src/Backend/CRM.Api/Controllers/QuotationsController.cs` with `POST /quotations/{id}/send` endpoint.
- [ ] T062 [P] [US5] Extend `QuotationsController` with `POST /quotations/{id}/resend` endpoint.
- [ ] T063 [P] [US6] Extend `QuotationsController` with `GET /quotations/{id}/status-history` endpoint.
- [ ] T064 [P] [US7] Extend `QuotationsController` with `GET /quotations/{id}/response` endpoint.
- [ ] T065 [P] [US8] Extend `QuotationsController` with `GET /quotations/{id}/access-link` endpoint.
- [ ] T066 [P] [US1] Extend `QuotationsController` with `GET /quotations/{id}/download-pdf` endpoint.
- [ ] T067 [US1-US8] Add authorization checks: SalesRep sees only own quotations, Admin sees all.
- [ ] T068 [US1-US8] Add error handling: proper status codes, error messages, logging.

**Checkpoint**: Sales rep endpoints operational with authorization.

---

### Client Portal Endpoints (Public, No Auth)

- [ ] T069 [P] [US9] Create `ClientPortalController` in `src/Backend/CRM.Api/Controllers/ClientPortalController.cs` with `GET /client-portal/quotations/{id}/{token}` endpoint.
- [ ] T070 [P] [US3] Extend `ClientPortalController` with `POST /client-portal/quotations/{id}/{token}/respond` endpoint.
- [ ] T071 [US9, US3] Configure public routes to bypass authentication middleware (token serves as authorization).
- [ ] T072 [US9, US3] Add rate limiting on public endpoints to prevent abuse.
- [ ] T073 [US9, US3] Add IP tracking and logging for security.
- [ ] T074 [US9, US3] Add error handling: proper status codes, no sensitive data in errors.

**Checkpoint**: Client portal endpoints operational (public access).

---

## Phase 5: Background Jobs

**Purpose**: Scheduled tasks for expiration checks and reminders.

### Expiration Check Job

- [ ] T075 [P] [US4] Create `QuotationExpirationCheckJob` in `src/Backend/CRM.Infrastructure/Jobs/QuotationExpirationCheckJob.cs`.
- [ ] T076 [US4] Implement job logic: find quotations with ValidUntil < today and Status not ACCEPTED/REJECTED/EXPIRED, call MarkQuotationAsExpiredCommand for each.
- [ ] T077 [US4] Schedule job to run daily at midnight using Quartz.NET.
- [ ] T078 [US4] Add error handling and logging for job execution.
- [ ] T079 [P] [US4] Add unit tests for `QuotationExpirationCheckJob` in `tests/CRM.Tests/Quotations/QuotationExpirationCheckJobTests.cs` (4-6 tests).

**Checkpoint**: Expiration job operational.

---

### Reminder Jobs

- [ ] T080 [P] Create `UnviewedQuotationReminderJob` in `src/Backend/CRM.Infrastructure/Jobs/UnviewedQuotationReminderJob.cs`.
- [ ] T081 Implement job logic: find quotations sent 3+ days ago, never viewed, send reminder email to sales rep.
- [ ] T082 Schedule job to run daily at 9 AM using Quartz.NET.
- [ ] T083 [P] Create `PendingResponseFollowUpJob` in `src/Backend/CRM.Infrastructure/Jobs/PendingResponseFollowUpJob.cs`.
- [ ] T084 Implement job logic: find quotations viewed but no response for 7+ days, send follow-up email to sales rep.
- [ ] T085 Schedule job to run daily at 3 PM using Quartz.NET.
- [ ] T086 [P] Add unit tests for reminder jobs in `tests/CRM.Tests/Quotations/ReminderJobTests.cs` (4-6 tests).

**Checkpoint**: Reminder jobs operational.

---

## Phase 6: Frontend Sales Rep Pages

**Purpose**: Build UI for sales reps to send quotations, view status, and track analytics.

### Send Quotation Modal

- [ ] T087 [P] [US1] Create `SendQuotationModal.tsx` in `src/Frontend/web/src/components/quotations/SendQuotationModal.tsx`.
- [ ] T088 [US1] Implement modal with three sections: Configure Recipients (To, CC, BCC), Compose Message (subject, custom message), Preview & Send (email preview, PDF preview, CTA link).
- [ ] T089 [US1] Add email validation: real-time format checking, suggest common typos.
- [ ] T090 [US1] Add form validation: disable send until valid, character limits enforced.
- [ ] T091 [US1] Add loading states: "Sending..." with spinner during API call.
- [ ] T092 [US1] Add success/error handling: toast notifications, error messages.
- [ ] T093 [P] [US1] Add unit tests for `SendQuotationModal` in `src/Frontend/web/src/components/quotations/__tests__/SendQuotationModal.test.tsx` (6-8 tests).

**Checkpoint**: Send quotation modal operational.

---

### Status Timeline Component

- [ ] T094 [P] [US6] Create `QuotationStatusTimeline.tsx` in `src/Frontend/web/src/components/quotations/QuotationStatusTimeline.tsx`.
- [ ] T095 [US6] Implement timeline UI: vertical timeline showing status changes with dates, user names, reasons, icons.
- [ ] T096 [US6] Add color coding: DRAFT=gray, SENT=blue, VIEWED=yellow, ACCEPTED=green, REJECTED=red, EXPIRED=orange.
- [ ] T097 [US6] Add loading state: skeleton while fetching history.
- [ ] T098 [P] [US6] Add unit tests for `QuotationStatusTimeline` in `src/Frontend/web/src/components/quotations/__tests__/QuotationStatusTimeline.test.tsx` (4-6 tests).

**Checkpoint**: Status timeline component operational.

---

### Client Response Card

- [ ] T099 [P] [US7] Create `ClientResponseCard.tsx` in `src/Frontend/web/src/components/quotations/ClientResponseCard.tsx`.
- [ ] T100 [US7] Implement card UI: response type with icon, client name/email, response message, response date, IP address, notification timestamp.
- [ ] T101 [US7] Add action buttons: "Reply to Client" (opens email composer), "Accept Response" (if needed).
- [ ] T102 [P] [US7] Add unit tests for `ClientResponseCard` in `src/Frontend/web/src/components/quotations/__tests__/ClientResponseCard.test.tsx` (3-4 tests).

**Checkpoint**: Client response card operational.

---

### Extended Quotation Detail Page

- [ ] T103 [P] [US1, US6, US7] Extend `src/Frontend/web/src/app/(protected)/quotations/[id]/page.tsx` with:
  - Send Quotation Panel (if Status = DRAFT)
  - Quotation Status & Timeline section
  - Client Response section (if exists)
  - Action buttons based on status
- [ ] T104 [US1] Integrate `SendQuotationModal` into detail page.
- [ ] T105 [US6] Integrate `QuotationStatusTimeline` into detail page.
- [ ] T106 [US7] Integrate `ClientResponseCard` into detail page.
- [ ] T107 [US5] Add "Resend" button and `ResendQuotationModal` integration.

**Checkpoint**: Extended quotation detail page operational.

---

### Analytics Page

- [ ] T108 [P] Create `src/Frontend/web/src/app/(protected)/quotations/[id]/analytics/page.tsx`.
- [ ] T109 Implement analytics UI: engagement metrics cards (sent, first viewed, view count, last viewed, response status), view history table (timestamp, IP, device), client activity timeline.
- [ ] T110 Add actions: "Download PDF", "Send Reminder", "Revoke Access Link".
- [ ] T111 [P] Add unit tests for analytics page in `src/Frontend/web/src/app/(protected)/quotations/[id]/analytics/__tests__/page.test.tsx` (4-6 tests).

**Checkpoint**: Analytics page operational.

---

## Phase 7: Frontend Client Portal (Public)

**Purpose**: Build public client portal for viewing and responding to quotations.

### Client Quotation View Page

- [ ] T112 [P] [US9] Create `src/Frontend/web/src/app/(public)/client-portal/quotations/[quotationId]/[token]/page.tsx`.
- [ ] T113 [US9] Implement professional quotation display: header (company logo, name), quotation details (number, date, valid until), client section, line items table, summary (subtotal, discount, tax, total), notes/terms section.
- [ ] T114 [US9] Add CTA buttons: "Accept Quotation" (green), "Request Modification" (yellow), "Decline" (red), "Download PDF" (secondary), "Contact Salesperson" (link).
- [ ] T115 [US9] Add responsive design: full page on desktop, scrollable on mobile.
- [ ] T116 [US9] Add print-friendly CSS for PDF-like appearance.
- [ ] T117 [US9] Implement automatic view tracking on page load (no user action needed).
- [ ] T118 [US9] Add loading state: skeleton while fetching quotation.
- [ ] T119 [US9] Add error handling: invalid/expired token, 404 page.
- [ ] T120 [P] [US9] Add unit tests for client portal page in `src/Frontend/web/src/app/(public)/client-portal/quotations/[quotationId]/[token]/__tests__/page.test.tsx` (6-8 tests).

**Checkpoint**: Client portal view page operational.

---

### Client Response Form Modal

- [ ] T121 [P] [US3] Create `ClientResponseModal.tsx` in `src/Frontend/web/src/components/quotations/ClientResponseModal.tsx`.
- [ ] T122 [US3] Implement modal UI: title "Please confirm your decision", choice buttons (Accept/Decline/Modification pre-selected), form fields (name, email, comments, terms checkbox), action buttons (Submit, Cancel).
- [ ] T123 [US3] Add form validation: required fields, email format, character limits, terms checkbox required.
- [ ] T124 [US3] Add success message: "Thank you! Your response has been received."
- [ ] T125 [US3] Add error handling: validation errors inline, network errors, duplicate response error.
- [ ] T126 [US3] Integrate modal into client portal page (opens on CTA button click).
- [ ] T127 [P] [US3] Add unit tests for `ClientResponseModal` in `src/Frontend/web/src/components/quotations/__tests__/ClientResponseModal.test.tsx` (6-8 tests).

**Checkpoint**: Client response modal operational.

---

### API Integration

- [ ] T128 [P] Extend `QuotationsApi` in `src/Frontend/web/src/lib/api.ts` with:
  - `send(quotationId, payload)`
  - `resend(quotationId, payload)`
  - `getStatusHistory(quotationId)`
  - `getResponse(quotationId)`
  - `getAccessLink(quotationId)`
  - `downloadPdf(quotationId)`
- [ ] T129 [P] Create `ClientPortalApi` in `src/Frontend/web/src/lib/api.ts` with:
  - `getQuotationByToken(quotationId, token)`
  - `submitResponse(quotationId, token, payload)`
- [ ] T130 Add error handling: network errors, validation errors, proper error messages.

**Checkpoint**: API integration complete.

---

## Phase 8: Testing & Polish

**Purpose**: Comprehensive testing, documentation, and polish.

### Backend Integration Tests

- [ ] T131 [P] Create `tests/CRM.Tests.Integration/Quotations/SendQuotationEndpointTests.cs` covering:
  - Send quotation successfully
  - Cannot send non-draft quotation
  - Authorization (SalesRep sees only own)
  - Email delivery verification
- [ ] T132 [P] Create `tests/CRM.Tests.Integration/Quotations/ClientPortalEndpointTests.cs` covering:
  - View quotation by token (public, no auth)
  - Submit response successfully
  - Cannot respond twice
  - Invalid/expired token rejection
- [ ] T133 [P] Create `tests/CRM.Tests.Integration/Quotations/QuotationStatusHistoryEndpointTests.cs` covering:
  - Get status history
  - Authorization checks
- [ ] T134 [P] Create `tests/CRM.Tests.Integration/Quotations/QuotationResponseEndpointTests.cs` covering:
  - Get response
  - No response returns 204

**Checkpoint**: Integration tests complete.

---

### Frontend E2E Tests

- [ ] T135 [P] Create E2E test `tests/e2e/quotation-send-flow.spec.ts` covering:
  - Sales rep sends quotation
  - Email received (mock)
  - Client views quotation
  - Client responds
  - Sales rep sees response
- [ ] T136 [P] Create E2E test `tests/e2e/client-portal-flow.spec.ts` covering:
  - Client accesses quotation via link
  - Client views quotation
  - Client submits response
  - Response recorded correctly

**Checkpoint**: E2E tests complete.

---

### PDF Generation Tests

- [ ] T137 [P] Add unit tests for `QuotationPdfGenerationService` in `tests/CRM.Tests/Quotations/QuotationPdfGenerationServiceTests.cs` covering:
  - PDF generation with line items
  - PDF generation with tax breakdown
  - PDF caching
  - Error handling
- [ ] T138 [P] Add integration test for PDF download endpoint in `tests/CRM.Tests.Integration/Quotations/QuotationPdfDownloadTests.cs`.

**Checkpoint**: PDF generation tests complete.

---

### Email Service Tests

- [ ] T139 [P] Add unit tests for `QuotationEmailService` in `tests/CRM.Tests/Quotations/QuotationEmailServiceTests.cs` covering:
  - Email composition
  - Template rendering
  - Attachment handling
  - Error handling
- [ ] T140 [P] Add integration test for email delivery (mock SMTP) in `tests/CRM.Tests.Integration/Quotations/QuotationEmailDeliveryTests.cs`.

**Checkpoint**: Email service tests complete.

---

### Documentation & Polish

- [ ] T141 Update `specs/010-quotation-management/quickstart.md` with actual setup steps and verification.
- [ ] T142 Create `specs/010-quotation-management/checklists/validation-results.md` with manual testing checklist.
- [ ] T143 Update Swagger documentation with new endpoints and examples.
- [ ] T144 Add performance monitoring: log PDF generation time, email send time, view tracking time.
- [ ] T145 Add error boundaries for client portal pages.
- [ ] T146 Add loading skeletons for all async operations.
- [ ] T147 Add toast notifications for all user actions.
- [ ] T148 Ensure mobile responsive design for all pages.
- [ ] T149 Add keyboard shortcuts where applicable.
- [ ] T150 Verify all security measures: token validation, rate limiting, IP tracking.

**Checkpoint**: Documentation and polish complete.

---

## Dependencies & Execution Order

### Phase Dependencies
- Setup & Foundational → Backend Commands → Backend Queries → API Endpoints → Background Jobs → Frontend Sales Rep → Frontend Client Portal → Testing & Polish.
- Foundational tasks must be complete before any user story work.
- PDF/Email services must be ready before send command.
- Client portal endpoints must be ready before frontend client portal.

### User Story Dependencies
- **US1 (Send)**: Depends on Foundational (entities, PDF service, email service).
- **US2 (Track Views)**: Depends on US1 (access links created on send).
- **US3 (Client Response)**: Depends on US1 (access links) and US2 (view tracking).
- **US4 (Expiration)**: Can run parallel with other stories (background job).
- **US5 (Resend)**: Depends on US1 (send functionality).
- **US6-US9 (Queries)**: Can run parallel after Foundational.

### Parallel Opportunities
- Tasks marked `[P]` can run simultaneously.
- Backend Commands can run parallel with Backend Queries after Foundational.
- Frontend Sales Rep and Frontend Client Portal can be built in parallel after API Endpoints.
- Background Jobs can be implemented in parallel with other phases.

---

## Implementation Strategy

1. Complete Setup & Foundational to establish schema, DTOs, PDF/Email services, and configs.
2. Deliver MVP via US1 (Send) with email delivery; validate independently before proceeding.
3. Layer US2 (Track Views) for engagement tracking.
4. Add US3 (Client Response) for complete client interaction.
5. Build API endpoints with proper authorization (sales rep) and public access (client portal).
6. Implement background jobs for automation (expiration, reminders).
7. Build frontend sales rep pages (send modal, status timeline, analytics).
8. Build frontend client portal (public view and response).
9. Complete testing (unit, integration, E2E).
10. Finish with polish tasks (performance, UX, documentation).

---

## Success Metrics

- Send quotation: <1 minute end-to-end
- PDF generation: <5 seconds
- Email delivery: <30 seconds
- Client portal load: <2 seconds
- View tracking: Real-time (<1 second)
- Status updates: Real-time (<1 second)
- Test coverage: >80% for critical paths

---

## Notes

- Access tokens must be cryptographically secure (use RNGCryptoServiceProvider).
- PDF caching: 24-hour cache to avoid regeneration.
- Email queue: Use background processing to avoid blocking.
- Public endpoints: Rate limiting required to prevent abuse.
- All status transitions must be logged in QuotationStatusHistory.
- Client portal must work without authentication (token-based authorization only).

