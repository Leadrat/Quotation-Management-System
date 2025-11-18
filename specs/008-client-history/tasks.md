# Tasks: Client History & Activity Log (Spec-008)

**Input**: Design documents from `/specs/008-client-history/`  
**Prerequisites**: `spec.md`, `plan.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md`

**Tests**: Critical flows include unit and integration coverage per user story (timeline, restore, activity, export, suspicious monitoring).

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare branch, configuration, and documentation touchpoints required before coding.

- [x] T001 Confirm feature branch `008-client-history` is active and linked to Spec-008 artifacts.
- [x] T002 Add `History` and `SuspiciousActivity` config sections (retention years, restore window, thresholds) to `src/Backend/CRM.Api/appsettings.json`.
- [x] T003 Document new environment keys and quickstart steps in `specs/008-client-history/quickstart.md` (retention/cron settings).

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core schema, entities, DTOs, and mapping consumed by all user stories.

- [x] T004 Add `ClientHistory` entity to `src/Backend/CRM.Domain/Entities/ClientHistory.cs` per data-model definitions.
- [x] T005 Add `SuspiciousActivityFlag` entity to `src/Backend/CRM.Domain/Entities/SuspiciousActivityFlag.cs`.
- [x] T006 Create EF configurations (`ClientHistoryEntityConfiguration.cs`, `SuspiciousActivityFlagEntityConfiguration.cs`) under `src/Backend/CRM.Infrastructure/EntityConfigurations/`.
- [x] T007 Add migration `src/Backend/CRM.Infrastructure/Migrations/<timestamp>_AddClientHistoryTables.cs` with tables, indexes, triggers, and retention metadata.
- [x] T008 Update `src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs` to register DbSets, configure shadow properties, and seed extension guards.
- [x] T009 Create DTOs (`ClientHistoryEntryDto`, `ClientTimelineSummaryDto`, `SuspiciousActivityDto`) in `src/Backend/CRM.Application/Clients/Dtos/`.
- [x] T010 Add AutoMapper profile `src/Backend/CRM.Application/Mapping/ClientHistoryProfile.cs` for history/timeline projection mapping.
- [x] T011 Add FluentValidation helpers (restore reason length, pagination clamp reuse) in `src/Backend/CRM.Application/Common/Validation/HistoryValidationRules.cs`.
- [x] T012 Wire contracts into Swagger by referencing `specs/008-client-history/contracts/client-history.openapi.yaml` inside `src/Backend/CRM.Api/Program.cs`.
- [x] T013 Extend `DbSeeder` and sample data utilities in `src/Backend/CRM.Api/Utilities/DbSeeder.cs` to optionally seed demo history rows.

**Checkpoint**: ClientHistory schema, DTOs, and config ready—user stories can proceed independently.

---

## Phase 3: User Story 1 - Inspect full client timeline (Priority: P1) 🎯 MVP

**Goal**: SalesReps/Admins view chronological client history with CRUD + optional access logs.

**Independent Test**: `GET /api/v1/clients/{clientId}/history?includeAccessLogs=false` returns owner-visible entries with pagination; Admin including access logs sees merged feed.

### Implementation

- [x] T014 [P] [US1] Implement diff builder + masking helper in `src/Backend/CRM.Application/Clients/Services/ClientHistoryDiffBuilder.cs`.
- [x] T015 [P] [US1] Add `GetClientHistoryQuery`, validator, and handler under `src/Backend/CRM.Application/Clients/Queries/GetClientHistoryQuery*.cs`.
- [x] T016 [P] [US1] Add `GetClientTimelineQuery` (summary plus restoration window) in `src/Backend/CRM.Application/Clients/Queries/GetClientTimelineQuery*.cs`.
- [x] T017 [US1] Create `ClientsHistoryController` in `src/Backend/CRM.Api/Controllers/ClientsHistoryController.cs` exposing `/clients/{id}/history` and `/clients/{id}/timeline`.
- [x] T018 [US1] Add caching policy (memory cache or distributed) for timeline summaries inside `ClientsHistoryController`.
- [x] T019 [US1] Add AutoMapper projections + EF includes to support before/after snapshots in `GetClientHistoryQueryHandler`.
- [x] T020 [P] [US1] Add unit tests for diff builder and query validators in `tests/CRM.Tests/Clients/History`.
- [x] T021 [US1] Add integration tests `tests/CRM.Tests.Integration/Clients/ClientHistoryEndpointTests.cs` covering owner vs admin, pagination, access log toggle.

**Checkpoint**: Timeline endpoints operational, satisfying MVP.

---

## Phase 4: User Story 2 - Restore a deleted client safely (Priority: P1)

**Goal**: Admin-only restore workflow within 30-day window, logging RESTORED history entry.

**Independent Test**: `POST /api/v1/clients/{clientId}/restore` reinstates a client deleted ≤30 days ago and appends RESTORED log; attempts outside window rejected with 400.

### Implementation

- [x] T022 [P] [US2] Add `RestoreClientCommand`, validator, and handler in `src/Backend/CRM.Application/Clients/Commands/RestoreClientCommand*.cs`.
- [x] T023 [US2] Persist restoration history entry + before/after snapshots in handler (reuse diff builder).
- [x] T024 [US2] Add controller action for restore in `src/Backend/CRM.Api/Controllers/ClientsHistoryController.cs` with Admin-only authorization.
- [x] T025 [P] [US2] Update `CRM.Infrastructure.Security/ResetTokenGenerator.cs` or relevant services to prevent restoration when `DeletedAt` null or >30 days.
- [x] T026 [US2] Add event `ClientRestored` in `src/Backend/CRM.Domain/Events/ClientRestored.cs` and audit logging hook.
- [x] T027 [US2] Integration tests `tests/CRM.Tests.Integration/Clients/ClientRestoreEndpointTests.cs` covering success, expired window, already active client.

---

## Phase 5: User Story 3 - Monitor user activity (Priority: P2)

**Goal**: View per-user action feed for coaching/compliance.

**Independent Test**: `GET /api/v1/users/{userId}/activity` returns paginated entries filtered by actor and optionally action type/date.

### Implementation

- [x] T028 [P] [US3] Implement `GetUserActivityQuery`, validator, and handler in `src/Backend/CRM.Application/Clients/Queries/GetUserActivityQuery*.cs`.
- [x] T029 [US3] Extend controller with `/users/{userId}/activity` endpoint enforcing self-view or Admin/Manager override.
- [x] T030 [P] [US3] Add filter + sorting indexes (actor/date) to migration or new follow-up migration if needed.
- [x] T031 [US3] Add unit tests for query filters and authorization in `tests/CRM.Tests/Clients/UserActivityQueryTests.cs`.
- [x] T032 [US3] Add integration tests `tests/CRM.Tests.Integration/Clients/UserActivityEndpointTests.cs` for self-view vs admin view and filter combinations.

---

## Phase 6: User Story 4 - Export history for audits (Priority: P2)

**Goal**: Provide CSV (and future PDF) exports for selected clients/time ranges respecting RBAC and size caps.

**Independent Test**: `GET /api/v1/clients/history/export?clientIds=A&format=csv` streams CSV with ≤5k rows and correct headers; format=pdf returns placeholder message until implemented.

### Implementation

- [x] T033 [P] [US4] Implement `ExportClientHistoryQuery` with streaming iterator in `src/Backend/CRM.Application/Clients/Queries/ExportClientHistoryQuery*.cs`.
- [x] T034 [US4] Add CSV writer utility in `src/Backend/CRM.Application/Clients/Services/ClientHistoryCsvWriter.cs` enforcing row cap and escaping.
- [x] T035 [US4] Extend controller with `/clients/history/export` action, content negotiation, and format guard in `src/Backend/CRM.Api/Controllers/ClientsHistoryController.cs`.
- [x] T036 [P] [US4] Add admin/manager authorization policies + rate limit metadata in `src/Backend/CRM.Api/Program.cs`.
- [x] T037 [US4] Integration tests `tests/CRM.Tests.Integration/Clients/ClientHistoryExportTests.cs` for CSV success, 0 results, row cap warning, PDF placeholder.

---

## Phase 7: User Story 5 - Detect suspicious behavior (Priority: P3)

**Goal**: Surface high-signal anomalies to Admins using hybrid heuristics and scheduled correlation.

**Independent Test**: Within 5 minutes of rapid changes, `GET /api/v1/admin/suspicious-activity?minScore=7` lists flagged entries with reasons; statuses can be updated later if needed.

### Implementation

- [x] T038 [P] [US5] Implement inline heuristics (rapid changes, odd hours, unknown IP) within `src/Backend/CRM.Application/Clients/Services/SuspiciousActivityScorer.cs`.
- [x] T039 [US5] Create background job `src/Backend/CRM.Infrastructure/Jobs/SuspiciousActivityAggregationJob.cs` scheduled via Quartz/Hangfire (≤5 min) to aggregate scores and insert `SuspiciousActivityFlag` rows.
- [x] T040 [US5] Add `GetSuspiciousActivityQuery` + handler under `src/Backend/CRM.Application/Clients/Queries/GetSuspiciousActivityQuery*.cs`.
- [x] T041 [US5] Add `/admin/suspicious-activity` controller action with Admin-only authorization and pagination.
- [x] T042 [P] [US5] Create unit tests for scorer heuristics in `tests/CRM.Tests/Clients/SuspiciousActivityScorerTests.cs`.
- [x] T043 [US5] Integration tests `tests/CRM.Tests.Integration/Clients/SuspiciousActivityEndpointTests.cs` verifying flags appear within SLA and respect status filter.

---

## Phase 8: Frontend UI (TailAdmin Next.js)

**Purpose**: Build user-facing pages and components for history, timeline, restore, user activity, export, and suspicious activity monitoring using TailAdmin components.

- [x] T049 [Frontend] Add `ClientHistoryApi` methods to `src/Frontend/web/src/lib/api.ts` for all history endpoints (history, timeline, restore, user activity, export, suspicious activity).
- [x] T050 [Frontend] Create client history/timeline page at `src/Frontend/web/src/app/(protected)/clients/[id]/history/page.tsx` with paginated history entries, timeline summary, and before/after diff views.
- [x] T051 [Frontend] Add "View History" link/button to client details page (`src/Frontend/web/src/app/(protected)/clients/[id]/page.tsx`) and integrate restore button/modal for Admin users.
- [x] T052 [Frontend] Create user activity page at `src/Frontend/web/src/app/(protected)/users/[userId]/activity/page.tsx` with filters (action type, date range) and pagination.
- [x] T053 [Frontend] Create suspicious activity dashboard at `src/Frontend/web/src/app/(protected)/admin/suspicious-activity/page.tsx` (Admin-only) with score filters, status management, and pagination.
- [x] T054 [Frontend] Add export button to history page with CSV download functionality and filter options.
- [x] T055 [Frontend] Create reusable history entry component showing action type, actor, timestamp, changed fields, and before/after snapshots in `src/Frontend/web/src/components/crm/HistoryEntry.tsx`.
- [x] T056 [Frontend] Add TailAdmin styling and responsive layout to all history-related pages following existing client pages pattern.

---

## Final Phase: Polish & Cross-Cutting

**Purpose**: Hardening, observability, docs, and validation across stories.

- [x] T044 [P] Add Serilog structured logging + audit correlation IDs for history endpoints in `src/Backend/CRM.Api/Controllers/ClientsHistoryController.cs`.
- [x] T045 Add caching/batching metrics + histogram for suspicious job in `src/Backend/CRM.Infrastructure/Jobs/SuspiciousActivityAggregationJob.cs`.
- [x] T046 Update `docs/CHANGELOG.md` (or project log) referencing Spec-008 deliverables and configuration steps.
- [x] T047 [P] Run manual validation per `specs/008-client-history/quickstart.md` and capture evidence in `specs/008-client-history/checklists/validation-results.md`.
- [x] T048 Ensure Swagger + README reference new endpoints/contracts (`src/Backend/CRM.Api/Program.cs`, root README.md).

---

## Dependencies & Execution Order

### Phase Dependencies
- Setup → Foundational → User Story phases (US1–US5) → Polish.
- Foundational tasks must be complete before any user story work.

### User Story Dependencies
- **US1 (Timeline)**: First deliverable (MVP); no other story dependencies.
- **US2 (Restore)**: Depends on US1 diff builder/history plumbing.
- **US3 (User Activity)**: Depends on Foundational only; can run parallel with US2 after diff builder ready.
- **US4 (Export)**: Depends on US1 queries for data shape.
- **US5 (Suspicious)**: Depends on schema + US1 history ingestion to supply data.

### Parallel Opportunities
- Tasks marked `[P]` can run simultaneously (e.g., diff builder + query validators, scorer tests vs job implementation).
- After Foundational, US2–US5 can be staffed in parallel once their prerequisites from earlier stories are satisfied.

---

## Implementation Strategy

1. Complete Setup + Foundational to establish schema, DTOs, and configs.
2. Deliver MVP via US1 timeline endpoints; validate independently before proceeding.
3. Layer restoration (US2) and user activity (US3) next for operational readiness.
4. Add exports (US4) for compliance, then suspicious monitoring (US5) for proactive alerts.
5. Finish with polish tasks (logging, docs, validation) prior to opening `/speckit.implement`.

