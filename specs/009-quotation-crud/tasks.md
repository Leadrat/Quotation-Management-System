# Tasks: Quotation Entity & CRUD Operations (Spec-009)

**Input**: Design documents from `/specs/009-quotation-crud/`  
**Prerequisites**: `spec.md`, `plan.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md`

**Tests**: Critical flows include unit and integration coverage per user story (create, read, update, delete, tax calculation). Frontend E2E tests for complete user workflows.

## Phase 1: Setup & Foundational (Blocking Prerequisites)

**Purpose**: Core schema, entities, DTOs, migrations, validators, and services consumed by all user stories.

- [ ] T001 Confirm feature branch `009-quotation-crud` is active and linked to Spec-009 artifacts.
- [ ] T002 Add `Quotations` config section (number format, default valid days, tax rate) to `src/Backend/CRM.Api/appsettings.json`.
- [ ] T003 Add `Company` config section (state code, state name) to `src/Backend/CRM.Api/appsettings.json` for tax calculation.
- [ ] T004 Add `Quotation` entity to `src/Backend/CRM.Domain/Entities/Quotation.cs` per data-model definitions (18 properties).
- [ ] T005 Add `QuotationLineItem` entity to `src/Backend/CRM.Domain/Entities/QuotationLineItem.cs` per data-model definitions (10 properties).
- [ ] T006 Add `QuotationStatus` enum to `src/Backend/CRM.Domain/Enums/QuotationStatus.cs` (Draft, Sent, Viewed, Accepted, Rejected, Expired, Cancelled).
- [ ] T007 Create EF configurations (`QuotationEntityConfiguration.cs`, `QuotationLineItemEntityConfiguration.cs`) under `src/Backend/CRM.Infrastructure/EntityConfigurations/`.
- [ ] T008 Add migration `src/Backend/CRM.Infrastructure/Migrations/<timestamp>_CreateQuotationsTables.cs` with tables, indexes, foreign keys, and constraints.
- [ ] T009 Update `src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs` to register DbSets for `Quotations` and `QuotationLineItems`.
- [ ] T010 Create DTOs (`QuotationDto`, `LineItemDto`, `CreateQuotationRequest`, `UpdateQuotationRequest`, `CreateLineItemRequest`) in `src/Backend/CRM.Application/Quotations/Dtos/`.
- [ ] T011 Add AutoMapper profile `src/Backend/CRM.Application/Mapping/QuotationProfile.cs` for quotation/line item projection mapping.
- [ ] T012 Create `TaxCalculationService` in `src/Backend/CRM.Application/Quotations/Services/TaxCalculationService.cs` with intra-state and inter-state logic.
- [ ] T013 Create `QuotationNumberGenerator` in `src/Backend/CRM.Application/Quotations/Services/QuotationNumberGenerator.cs` with unique number generation and retry logic.
- [ ] T014 Create `QuotationTotalsCalculator` in `src/Backend/CRM.Application/Quotations/Services/QuotationTotalsCalculator.cs` for subtotal, discount, tax, and total calculation.
- [ ] T015 Add FluentValidation validators (`CreateQuotationRequestValidator`, `CreateLineItemRequestValidator`, `UpdateQuotationRequestValidator`) in `src/Backend/CRM.Application/Quotations/Validators/`.
- [ ] T016 Wire contracts into Swagger by referencing `specs/009-quotation-crud/contracts/quotations.openapi.yaml` inside `src/Backend/CRM.Api/Program.cs`.
- [ ] T017 Extend `DbSeeder` in `src/Backend/CRM.Api/Utilities/DbSeeder.cs` to optionally seed demo quotations with line items.

**Checkpoint**: Quotation schema, DTOs, services, and config ready—user stories can proceed independently.

---

## Phase 2: Backend CRUD - Commands & Queries

**Purpose**: Implement CQRS commands and queries for quotation operations.

### User Story 1 - Create Quotation (Priority: P1) 🎯 MVP

**Goal**: SalesReps create quotations with line items, automatic tax calculation, and totals.

**Independent Test**: `POST /api/v1/quotations` creates quotation with correct totals and tax calculation.

- [ ] T018 [P] [US1] Add `CreateQuotationCommand`, validator, and handler in `src/Backend/CRM.Application/Quotations/Commands/CreateQuotationCommand*.cs`.
- [ ] T019 [US1] Implement handler logic: validate client ownership, generate quotation number, create quotation entity, create line items, calculate totals, apply tax calculation.
- [ ] T020 [US1] Add domain event `QuotationCreated` in `src/Backend/CRM.Domain/Events/QuotationCreated.cs` and publish in handler.
- [ ] T021 [P] [US1] Add unit tests for `CreateQuotationCommandHandler` in `tests/CRM.Tests/Quotations/CreateQuotationCommandHandlerTests.cs` (8-10 tests).
- [ ] T022 [US1] Add unit tests for `TaxCalculationService` in `tests/CRM.Tests/Quotations/TaxCalculationServiceTests.cs` (intra-state, inter-state scenarios).

**Checkpoint**: Create quotation endpoint operational with tax calculation.

---

### User Story 2 - View and List Quotations (Priority: P1)

**Goal**: SalesReps/Admins view quotations with filtering and pagination.

**Independent Test**: `GET /api/v1/quotations` returns paginated quotations; `GET /api/v1/quotations/{id}` returns full details.

- [ ] T023 [P] [US2] Add `GetQuotationByIdQuery`, validator, and handler in `src/Backend/CRM.Application/Quotations/Queries/GetQuotationByIdQuery*.cs`.
- [ ] T024 [P] [US2] Add `GetAllQuotationsQuery` with filters (status, clientId, userId, dateFrom, dateTo) in `src/Backend/CRM.Application/Quotations/Queries/GetAllQuotationsQuery*.cs`.
- [ ] T025 [P] [US2] Add `GetQuotationsByClientQuery` in `src/Backend/CRM.Application/Quotations/Queries/GetQuotationsByClientQuery*.cs`.
- [ ] T026 [US2] Implement authorization in query handlers: SalesRep sees only own quotations, Admin sees all.
- [ ] T027 [US2] Add AutoMapper projections + EF includes to load line items with quotation (avoid N+1).
- [ ] T028 [P] [US2] Add unit tests for query handlers in `tests/CRM.Tests/Quotations/QuotationQueryHandlerTests.cs` (6-8 tests).
- [ ] T029 [US2] Add integration tests `tests/CRM.Tests.Integration/Quotations/QuotationQueryEndpointTests.cs` covering authorization, pagination, filters.

**Checkpoint**: Query endpoints operational with proper authorization.

---

### User Story 3 - Update Draft Quotations (Priority: P1)

**Goal**: SalesReps edit draft quotations to correct errors before sending.

**Independent Test**: `PUT /api/v1/quotations/{id}` updates DRAFT quotation; SENT quotations rejected.

- [ ] T030 [P] [US3] Add `UpdateQuotationCommand`, validator, and handler in `src/Backend/CRM.Application/Quotations/Commands/UpdateQuotationCommand*.cs`.
- [ ] T031 [US3] Implement handler logic: verify status is DRAFT, update fields, recalculate totals, update line items (add/update/delete).
- [ ] T032 [US3] Add domain event `QuotationUpdated` in `src/Backend/CRM.Domain/Events/QuotationUpdated.cs` and publish in handler.
- [ ] T033 [US3] Add validation: reject updates to non-DRAFT quotations with clear error message.
- [ ] T034 [P] [US3] Add unit tests for `UpdateQuotationCommandHandler` in `tests/CRM.Tests/Quotations/UpdateQuotationCommandHandlerTests.cs` (6-8 tests).
- [ ] T035 [US3] Add integration tests `tests/CRM.Tests.Integration/Quotations/UpdateQuotationEndpointTests.cs` covering draft update, sent rejection.

**Checkpoint**: Update quotation endpoint operational with status validation.

---

### User Story 4 - Delete Draft Quotations (Priority: P2)

**Goal**: SalesReps delete draft quotations that are no longer needed.

**Independent Test**: `DELETE /api/v1/quotations/{id}` soft-deletes DRAFT quotation; SENT quotations rejected.

- [ ] T036 [P] [US4] Add `DeleteQuotationCommand`, validator, and handler in `src/Backend/CRM.Application/Quotations/Commands/DeleteQuotationCommand*.cs`.
- [ ] T037 [US4] Implement handler logic: verify status is DRAFT or CANCELLED, set status to CANCELLED (soft delete).
- [ ] T038 [US4] Add domain event `QuotationDeleted` in `src/Backend/CRM.Domain/Events/QuotationDeleted.cs` and publish in handler.
- [ ] T039 [US4] Add validation: reject deletes to non-DRAFT/CANCELLED quotations with clear error message.
- [ ] T040 [P] [US4] Add unit tests for `DeleteQuotationCommandHandler` in `tests/CRM.Tests/Quotations/DeleteQuotationCommandHandlerTests.cs` (4-6 tests).
- [ ] T041 [US4] Add integration tests `tests/CRM.Tests.Integration/Quotations/DeleteQuotationEndpointTests.cs` covering draft delete, sent rejection.

**Checkpoint**: Delete quotation endpoint operational with status validation.

---

## Phase 3: API Endpoints & Controller

**Purpose**: Expose quotation operations via REST API with authorization and error handling.

- [ ] T042 Create `QuotationsController` in `src/Backend/CRM.Api/Controllers/QuotationsController.cs` with all 5 endpoints (GET list, GET by ID, POST create, PUT update, DELETE).
- [ ] T043 Add authorization attributes: `[Authorize(Roles = "SalesRep,Admin")]` on all endpoints, ownership checks in handlers.
- [ ] T044 Add error handling: return 400 for validation errors, 403 for authorization failures, 404 for not found.
- [ ] T045 Add Serilog structured logging for all quotation operations (create, update, delete, view).
- [ ] T046 Add audit logging hooks for quotation operations (who, what, when).
- [ ] T047 Add integration tests `tests/CRM.Tests.Integration/Quotations/QuotationAuthorizationTests.cs` for SalesRep vs Admin access.

**Checkpoint**: API endpoints operational with proper authorization and error handling.

---

## Phase 4: Frontend API Integration

**Purpose**: Set up API service methods and React Query hooks for frontend consumption.

- [ ] T048 [Frontend] Add `QuotationsApi` methods to `src/Frontend/web/src/lib/api.ts` for all endpoints (list, get, create, update, delete, getByClient).
- [ ] T049 [Frontend] Create custom hook `useQuotations` in `src/Frontend/web/src/hooks/useQuotations.ts` using React Query for list, get, create, update, delete operations.
- [ ] T050 [Frontend] Create custom hook `useTaxCalculation` in `src/Frontend/web/src/hooks/useTaxCalculation.ts` for real-time tax calculation (mirrors backend logic).
- [ ] T051 [Frontend] Create `taxCalculator.ts` utility in `src/Frontend/web/src/utils/taxCalculator.ts` with intra-state and inter-state calculation logic.
- [ ] T052 [Frontend] Create TypeScript interfaces in `src/Frontend/web/src/types/quotation.ts` for QuotationDto, LineItemDto, CreateQuotationRequest, etc.

**Checkpoint**: Frontend API integration ready for page components.

---

## Phase 5: Frontend Pages

**Purpose**: Build user-facing pages for quotation management using TailAdmin components.

- [ ] T053 [Frontend] Create quotation list page at `src/Frontend/web/src/app/(protected)/quotations/page.tsx` with table, search, filters (status, client, date), pagination.
- [ ] T054 [Frontend] Create create quotation page at `src/Frontend/web/src/app/(protected)/quotations/create/page.tsx` with multi-step form (Client → Details → Line Items → Review).
- [ ] T055 [Frontend] Create edit quotation page at `src/Frontend/web/src/app/(protected)/quotations/[id]/edit/page.tsx` (only for DRAFT status).
- [ ] T056 [Frontend] Create view quotation page at `src/Frontend/web/src/app/(protected)/quotations/[id]/view/page.tsx` with professional PDF-like display.
- [ ] T057 [Frontend] Create timeline page at `src/Frontend/web/src/app/(protected)/quotations/[id]/timeline/page.tsx` showing status history (future: Spec-010).
- [ ] T058 [Frontend] Add navigation links to quotation pages in sidebar/menu (TailAdmin layout).

**Checkpoint**: All quotation pages operational with TailAdmin styling.

---

## Phase 6: Frontend Components

**Purpose**: Build reusable components for quotation management.

- [ ] T059 [Frontend] Create `QuotationTable.tsx` component in `src/Frontend/web/src/components/quotations/` with sortable columns, status badges, action buttons.
- [ ] T060 [Frontend] Create `QuotationForm.tsx` component in `src/Frontend/web/src/components/quotations/` with React Hook Form, multi-step workflow, validation.
- [ ] T061 [Frontend] Create `QuotationViewer.tsx` component in `src/Frontend/web/src/components/quotations/` with professional PDF-like layout, line items table, tax breakdown.
- [ ] T062 [Frontend] Create `LineItemRepeater.tsx` component in `src/Frontend/web/src/components/quotations/` for add/remove/update line items dynamically.
- [ ] T063 [Frontend] Create `TaxCalculationPreview.tsx` component in `src/Frontend/web/src/components/quotations/` showing real-time CGST/SGST/IGST breakdown.
- [ ] T064 [Frontend] Create `ClientSelector.tsx` component in `src/Frontend/web/src/components/quotations/` with searchable dropdown showing client details (name, email, state).
- [ ] T065 [Frontend] Create `QuotationStatusBadge.tsx` component in `src/Frontend/web/src/components/quotations/` with color-coded status badges.
- [ ] T066 [Frontend] Implement real-time tax calculation: update totals as user types (debounced 300ms), show tax breakdown based on client state.

**Checkpoint**: All reusable components operational with real-time calculations.

---

## Phase 7: Testing

**Purpose**: Comprehensive test coverage for backend and frontend.

### Backend Tests

- [ ] T067 Add unit tests for `QuotationNumberGenerator` in `tests/CRM.Tests/Quotations/QuotationNumberGeneratorTests.cs` (uniqueness, retry logic).
- [ ] T068 Add unit tests for `QuotationTotalsCalculator` in `tests/CRM.Tests/Quotations/QuotationTotalsCalculatorTests.cs` (subtotal, discount, tax, total).
- [ ] T069 Add integration tests `tests/CRM.Tests.Integration/Quotations/QuotationTaxCalculationTests.cs` for intra-state vs inter-state scenarios.
- [ ] T070 Add integration tests `tests/CRM.Tests.Integration/Quotations/QuotationPaginationTests.cs` for pagination with filters.
- [ ] T071 Add integration tests `tests/CRM.Tests.Integration/Quotations/QuotationStatusLifecycleTests.cs` for status transitions and edit restrictions.

### Frontend Tests

- [ ] T072 [Frontend] Add unit tests for `QuotationForm.test.tsx` (form validation, calculations).
- [ ] T073 [Frontend] Add unit tests for `LineItemRepeater.test.tsx` (add/remove items).
- [ ] T074 [Frontend] Add unit tests for `TaxCalculationPreview.test.tsx` (tax calculations).
- [ ] T075 [Frontend] Add unit tests for `useTaxCalculation.test.ts` (hook tests).
- [ ] T076 [Frontend] Add E2E tests `quotations.e2e.test.ts` (create, view, edit, delete flows).
- [ ] T077 [Frontend] Add E2E tests `taxCalculation.e2e.test.ts` (intra-state vs inter-state scenarios).

**Checkpoint**: Test coverage ≥85% backend, ≥80% frontend, all critical flows tested.

---

## Phase 8: Polish & Cross-Cutting

**Purpose**: Hardening, observability, docs, and validation across stories.

- [ ] T078 Add performance monitoring: log quotation creation time, tax calculation time, query execution time.
- [ ] T079 Add caching for quotation number sequence (if high volume detected).
- [ ] T080 Update `docs/CHANGELOG.md` (or project log) referencing Spec-009 deliverables and configuration steps.
- [ ] T081 Run manual validation per `specs/009-quotation-crud/quickstart.md` and capture evidence in `specs/009-quotation-crud/checklists/validation-results.md`.
- [ ] T082 Ensure Swagger + README reference new endpoints/contracts (`src/Backend/CRM.Api/Program.cs`, root README.md).
- [ ] T083 [Frontend] Add loading skeletons for quotation list and form pages.
- [ ] T084 [Frontend] Add error boundaries for quotation pages.
- [ ] T085 [Frontend] Add toast notifications for success/error operations.
- [ ] T086 [Frontend] Ensure mobile responsive design for all quotation pages.
- [ ] T087 [Frontend] Add keyboard shortcuts (Cmd+S/Ctrl+S to save, Esc to cancel).

**Checkpoint**: All polish tasks complete, ready for production.

---

## Dependencies & Execution Order

### Phase Dependencies
- Setup & Foundational → Backend CRUD → API Endpoints → Frontend Integration → Frontend Pages → Frontend Components → Testing → Polish.
- Foundational tasks must be complete before any user story work.
- Frontend API Integration must be complete before Frontend Pages.

### User Story Dependencies
- **US1 (Create)**: First deliverable (MVP); no other story dependencies.
- **US2 (View/List)**: Depends on Foundational only; can run parallel with US1 after entities ready.
- **US3 (Update)**: Depends on US1 (quotation creation) and US2 (quotation retrieval).
- **US4 (Delete)**: Depends on US1 (quotation creation).

### Parallel Opportunities
- Tasks marked `[P]` can run simultaneously (e.g., tax calculation service + quotation number generator, query handlers + command handlers after foundational).
- After Foundational, US2 can run parallel with US1.
- Frontend API Integration can start after Phase 3 (API Endpoints) complete.
- Frontend Components can be built in parallel with Frontend Pages.

---

## Implementation Strategy

1. Complete Setup & Foundational to establish schema, DTOs, services, and configs.
2. Deliver MVP via US1 (Create) with tax calculation; validate independently before proceeding.
3. Layer US2 (View/List) for operational readiness.
4. Add US3 (Update) and US4 (Delete) for complete CRUD.
5. Build API endpoints with proper authorization.
6. Implement frontend API integration and pages.
7. Build reusable frontend components with real-time calculations.
8. Complete testing (unit, integration, E2E).
9. Finish with polish tasks (performance, UX, documentation).

---

## Acceptance Criteria Checklist

### Backend Functional
- [ ] Create quotation with line items and correct tax calculation
- [ ] Intra-state quotation shows CGST + SGST
- [ ] Inter-state quotation shows IGST
- [ ] Quotation totals calculated correctly (SubTotal - Discount + Tax)
- [ ] Update quotation (draft only) works
- [ ] Cannot update sent/viewed/accepted quotations
- [ ] Delete quotation (draft only) works
- [ ] SalesRep sees only own quotations
- [ ] Admin sees all quotations
- [ ] Filter by status, client, date works
- [ ] Pagination works with configurable page size
- [ ] Discount calculation correct (percentage applied to subtotal)
- [ ] Line items persisted and retrieved correctly

### Frontend Functional
- [ ] Quotation list displays all user quotations
- [ ] Search and filter work on frontend + backend
- [ ] Create form validates all inputs before submission
- [ ] Tax calculation updates in real-time
- [ ] Line items can be added/removed
- [ ] Quotation viewer displays like professional PDF
- [ ] Status badges color-coded correctly
- [ ] Edit form loads existing quotation data
- [ ] Delete confirmation dialog before deleting
- [ ] Mobile responsive (all pages work on mobile)

### Testing
- [ ] Backend: All queries, commands, calculations tested
- [ ] Backend: Tax calculation tested (intra-state, inter-state)
- [ ] Backend: Authorization tests
- [ ] Frontend: Form validation tests
- [ ] Frontend: Tax calculation tests
- [ ] Frontend: Component rendering tests
- [ ] E2E: Create, view, edit, delete flows work end-to-end
- [ ] All 15 test cases pass
- [ ] Code coverage ≥85% (backend) + ≥80% (frontend)

