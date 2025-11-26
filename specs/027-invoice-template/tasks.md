# Tasks: Invoice/Quote Template from Uploaded Document

**Input**: Design documents from `/specs/027-invoice-template/`
**Prerequisites**: plan.md (required), spec.md (required for user stories)

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [ ] T001 Create `backend/` and `frontend/` folders if not present, matching structure in `specs/027-invoice-template/plan.md`
- [ ] T002 Initialize .NET Core API project in `backend/src/Api/` (project file, basic Program/Startup)
- [ ] T003 Initialize Next.js + TypeScript app in `frontend/` (using existing tooling or `create-next-app` as per repo standards)
- [ ] T004 [P] Configure shared `.editorconfig` / formatting and linting for frontend (ESLint/Prettier) and backend (C# style) at repo root

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T005 Configure PostgreSQL connection and EF Core (or chosen ORM) in `backend/src/Infrastructure/`
- [ ] T006 Define initial database migrations for templates and generated documents in `backend/src/Infrastructure/Migrations/`
- [ ] T007 [P] Implement basic logging and error-handling middleware in `backend/src/Api/` (HTTP pipeline)
- [ ] T008 [P] Wire existing CRM authentication/authorization into the API in `backend/src/Api/` (reusing shared auth libraries if present)
- [ ] T009 Setup base API routing structure in `backend/src/Api/` (controllers or minimal APIs)
- [ ] T010 [P] Configure environment-based settings for storage paths (local disk folders) in `backend/src/Infrastructure/`
- [ ] T011 Setup frontend API client base (`frontend/src/services/httpClient.ts`) including auth headers and error handling

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Upload invoice/quote and generate reusable template (Priority: P1) 🎯 MVP

**Goal**: Allow authorized users to upload DOCX/PDF invoice/quote files and create reusable template records with basic preview.

**Independent Test**: A template admin can upload a supported file and see a new template record and preview, or get a clear error for unsupported files.

### Implementation for User Story 1

- [ ] T012 [P] [US1] Define `Template` and `GeneratedDocument` entities and DbContext mappings in `backend/src/Domain/` and `backend/src/Infrastructure/`
- [ ] T013 [US1] Implement file upload & template creation service in `backend/src/Application/TemplateUploadService.cs`
- [ ] T014 [US1] Implement upload API endpoint (DOCX/PDF) in `backend/src/Api/TemplatesController.cs`
- [ ] T015 [US1] Add basic document structure extraction/preview logic (stub if needed) in `backend/src/Application/TemplatePreviewService.cs`
- [X] T016 [P] [US1] Create upload page in `frontend/src/pages/templates/upload.tsx` with file input, role check, and submission to upload API
- [X] T017 [US1] Implement success/error handling and simple preview display on upload page in `frontend/src/pages/templates/upload.tsx`

**Checkpoint**: User Story 1 fully functional and testable independently

---

## Phase 4: User Story 2 - Configure placeholders for template fields and line items (Priority: P1)

**Goal**: Allow template managers to configure and adjust placeholders for header fields and line item sections in a template.

**Independent Test**: From a created template, a manager can open a configuration view, define mappings, and validate them with sample data.

### Implementation for User Story 2

- [ ] T018 [P] [US2] Extend `Template` domain model with placeholder definitions (header fields, line item fields) in `backend/src/Domain/Template.cs`
- [ ] T019 [US2] Implement placeholder configuration service in `backend/src/Application/TemplatePlaceholderService.cs`
- [ ] T020 [US2] Add API endpoints to load and save placeholder mappings in `backend/src/Api/TemplatesController.cs`
- [ ] T021 [P] [US2] Create template details/config page in `frontend/src/pages/templates/[templateId].tsx` showing detected regions and placeholder fields
- [ ] T022 [US2] Implement UI to edit and persist placeholder mappings (header, recipient, payment terms, totals, line items) in `frontend/src/pages/templates/[templateId].tsx`

**Checkpoint**: User Story 2 independently functional with configured placeholders driving preview

---

## Phase 5: User Story 3 - Generate and view invoice/quote from JSON data (Priority: P1)

**Goal**: Allow sales users to select a template, provide JSON data (including line items), and see a rendered web view.

**Independent Test**: Given a template with placeholders and valid JSON, the user can generate and view a full invoice/quote in the browser.

### Implementation for User Story 3

- [ ] T023 [P] [US3] Define input data contract (DTO) for invoice/quote generation including line items in `backend/src/Domain/Dto/InvoiceGenerationRequest.cs`
- [ ] T024 [US3] Implement template merge service (template + JSON → rendered document model) in `backend/src/Application/DocumentGenerationService.cs`
- [ ] T025 [US3] Add API endpoint to generate document for web view in `backend/src/Api/DocumentsController.cs`
- [ ] T026 [P] [US3] Create generation page in `frontend/src/pages/invoices/generate.tsx` with template selection and JSON editor/form for data
- [ ] T027 [US3] Implement web view rendering of generated document in `frontend/src/pages/invoices/generate.tsx` (using returned model/HTML)

**Checkpoint**: User Story 3 provides complete JSON-driven document generation and web preview

---

## Phase 6: User Story 4 - Download invoice/quote as PDF (Priority: P2)

**Goal**: Allow users to download a finalized invoice/quote instance as a PDF.

**Independent Test**: From a generated document, the user can click download and receive a PDF matching the on-screen layout.

### Implementation for User Story 4

- [ ] T028 [P] [US4] Integrate a .NET PDF generation library and adapter in `backend/src/Infrastructure/Pdf/`
- [ ] T029 [US4] Extend `DocumentGenerationService` to support PDF output and persistence in `backend/src/Application/DocumentGenerationService.cs`
- [ ] T030 [US4] Add API endpoint to fetch/download a document as PDF in `backend/src/Api/DocumentsController.cs`
- [ ] T031 [US4] Wire "Download PDF" action from web view in `frontend/src/pages/invoices/generate.tsx` to call the PDF endpoint and trigger browser download

**Checkpoint**: User Story 4 delivers consistent PDF downloads for generated invoices/quotes

---

## Phase 7: User Story 5 - Manage line items with different types (Priority: P2)

**Goal**: Support multiple line item types (subscriptions, add-ons, services) with pricing, discounts, and taxes.

**Independent Test**: User can configure multiple line items of different types and see correct subtotals and totals.

### Implementation for User Story 5

- [ ] T032 [P] [US5] Extend `LineItem` domain model with type, pricing, discount, and tax fields in `backend/src/Domain/LineItem.cs`
- [ ] T033 [US5] Ensure template placeholders and DTOs support multiple line item types in `backend/src/Domain/Dto/InvoiceGenerationRequest.cs`
- [ ] T034 [US5] Implement line item total and aggregate total calculation logic in `backend/src/Application/PricingService.cs`
- [ ] T035 [P] [US5] Enhance generation UI to add/edit/remove different line item types in `frontend/src/pages/invoices/generate.tsx`
- [ ] T036 [US5] Display line-level and overall totals (including discounts and taxes) in the web view in `frontend/src/pages/invoices/generate.tsx`

**Checkpoint**: User Story 5 supports realistic pricing structures for invoices/quotes

---

## Phase 8: User Story 6 - Role-based access and permissions (Priority: P2)

**Goal**: Enforce role-based access using existing CRM roles for template management, document generation, and history viewing.

**Independent Test**: Different CRM roles see only the actions they are allowed to perform.

### Implementation for User Story 6

- [X] T037 [P] [US6] Map existing CRM roles to feature permissions in backend configuration (`backend/src/Api/Auth/AuthorizationPolicies.cs`)
- [X] T038 [US6] Apply authorization attributes/policies to template and document endpoints in `backend/src/Api/TemplatesController.cs` and `backend/src/Api/DocumentsController.cs`
- [X] T039 [P] [US6] Add frontend route guards/conditional rendering based on role claims in `frontend/src/features/invoicing/usePermissions.ts`
- [X] T040 [US6] Hide or disable UI actions (upload, configure placeholders, generate, download, history) based on permissions in the relevant pages/components

**Checkpoint**: User Story 6 enforces correct access per role without breaking other stories

---

## Phase 9: User Story 7 - Audit trail and history (Priority: P3)

**Goal**: Provide history and audit information for generated invoices/quotes.

**Independent Test**: Finance/compliance users can filter and inspect generated documents with key metadata.

### Implementation for User Story 7

- [ ] T041 [P] [US7] Extend `GeneratedDocument` entity to store audit metadata (creator, timestamps, template reference, key identifiers) in `backend/src/Domain/GeneratedDocument.cs`
- [ ] T042 [US7] Implement query service for document history and filters in `backend/src/Application/DocumentHistoryService.cs`
- [ ] T043 [US7] Add API endpoints for listing and viewing history details in `backend/src/Api/DocumentsController.cs`
- [ ] T044 [P] [US7] Create history page with filters in `frontend/src/pages/invoices/history.tsx`
- [ ] T045 [US7] Implement history detail view to show metadata and links to re-open/download in `frontend/src/pages/invoices/history.tsx`

**Checkpoint**: User Story 7 provides auditable history for generated documents

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T046 [P] Add documentation for this feature in `docs/invoicing-templates.md` (overview, flows, roles)
- [ ] T047 Code cleanup and refactoring across `backend/src/` and `frontend/src/` for this feature
- [ ] T048 [P] Add targeted unit tests for core services (upload, merge, pricing, history) in `backend/tests/Unit/`
- [ ] T049 [P] Add targeted component/integration tests for key flows (upload, generate, PDF download) in `frontend/tests/`
- [ ] T050 Run end-to-end validation following `quickstart.md` (once created) and fix any issues found

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - no dependencies on other stories
- **User Story 2 (P1)**: Depends on User Story 1 template creation
- **User Story 3 (P1)**: Depends on User Story 2 placeholder configuration
- **User Story 4 (P2)**: Depends on User Story 3 document generation
- **User Story 5 (P2)**: Depends on User Story 3 data model; can evolve pricing separately
- **User Story 6 (P2)**: Depends on basic endpoints (US1–US3) for applying authorization
- **User Story 7 (P3)**: Depends on document generation (US3) and storage (Foundational)

### Parallel Opportunities

- All tasks marked [P] can be run in parallel with other non-dependent tasks
- Frontend and backend work for the same user story can often proceed in parallel using agreed contracts
- Different user stories (e.g., US5 pricing and US7 history) can be built in parallel once their dependencies are satisfied

---

## Implementation Strategy

### MVP First (P1 Stories)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1 (upload + template creation)
4. Complete Phase 4: User Story 2 (placeholder configuration)
5. Complete Phase 5: User Story 3 (JSON-driven generation + web view)
6. **STOP and VALIDATE**: Ensure P1 flows work end-to-end for a basic template

### Incremental Delivery

1. Deliver P1 stories (US1–US3) as MVP
2. Add P2 stories (US4–US6): PDF downloads, rich line items, role-based controls
3. Add P3 story (US7): History and audit trail
4. Finish with Phase 10 polish

### Parallel Team Strategy

- One developer can focus on backend services and APIs, another on frontend pages/components.
- After foundational work, split by user story: e.g.,
  - Dev A: US1–US3 backend
  - Dev B: US1–US3 frontend
  - Dev C: US5 pricing and US7 history

## Notes

- [P] tasks = different files, no direct dependencies
- [Story] labels map tasks to specific user stories for traceability
- Each user story should be independently completable and testable
- Prefer small, frequent commits aligned to task IDs
