# Tasks: Manual Payment Update Management & Dashboard Integration

Feature: specs/028-manual-payments/spec.md
Branch: 028-manual-payments

Dependencies order by User Story: US1 (P1) → US2 (P2) → US3 (P3). US4 (Edge) woven into phases.

Parallel opportunities noted with [P].

## Phase 1: Setup

- [ ] T001 Create feature work branch if not already: 028-manual-payments (git)  
- [ ] T002 Create backend Payments area folder structure in src/Backend (Domain, Application, Infrastructure, Api)  
- [ ] T003 Add shared enums and constants for PaymentStatus and PaymentMethod in src/Backend/CRM.Domain/Payments/PaymentEnums.cs  
- [ ] T004 Add TailAdmin UI helpers for status badges if missing in src/Frontend/web/src/components/tailadmin/ui/badges/PaymentStatusBadge.tsx  
- [ ] T005 Configure frontend environment for API base URL in src/Frontend/web/.env.local and src/Frontend/web/src/lib/api/config.ts  

## Phase 2: Foundational (Blocking)

- [ ] T006 Add Payments entity to domain: src/Backend/CRM.Domain/Payments/Payment.cs  
- [ ] T007 Add Payment configuration (EF mapping): src/Backend/CRM.Infrastructure/Persistence/Configurations/PaymentConfiguration.cs  
- [ ] T008 Create DB migration for Payments table: src/Backend/CRM.Infrastructure/Migrations/<timestamp>_AddPayments.cs  
- [ ] T009 Update DbContext to include DbSet<Payment>: src/Backend/CRM.Infrastructure/Persistence/CRMDbContext.cs  
- [ ] T010 Seed demo data (sample payments and quotations): src/Backend/CRM.Infrastructure/Seeds/PaymentsSeed.cs  
- [ ] T011 Add quotation payment aggregates (computed service): src/Backend/CRM.Application/Payments/Services/PaymentAggregationService.cs  
- [ ] T012 [P] Add DTOs: PaymentDto, CreatePaymentRequest, PaymentHistoryItem in src/Backend/CRM.Application/Payments/Dtos/*.cs  
- [ ] T013 [P] Add validation rules for payments in src/Backend/CRM.Application/Payments/Validation/CreatePaymentValidator.cs  
- [ ] T014 [P] Add mapping profiles for payments in src/Backend/CRM.Application/Common/Mapping/PaymentProfile.cs  
- [ ] T015 Implement RBAC policy allowing SalesRep/Manager/Admin to create/update/delete payments in src/Backend/CRM.Api/Security/Policies.cs  

## Phase 3: US1 (P1) Record Manual Payment

- [ ] T016 [US1] Add application command: AddPaymentCommand + handler to insert payment and recalc aggregates in src/Backend/CRM.Application/Payments/Commands/AddPayment/*.cs  
- [ ] T017 [US1] Add application command: UpdatePaymentCommand + handler (allow edit) in src/Backend/CRM.Application/Payments/Commands/UpdatePayment/*.cs  
- [ ] T018 [US1] Domain/service logic: status derivation Paid/Partial/Unpaid (block overpayment) in src/Backend/CRM.Application/Payments/Services/PaymentDomainService.cs  
- [ ] T019 [US1] API endpoint POST /quotations/{id}/payments in src/Backend/CRM.Api/Controllers/PaymentsController.cs  
- [ ] T020 [US1] API endpoint PUT /payments/{paymentId} in src/Backend/CRM.Api/Controllers/PaymentsController.cs  
- [ ] T021 [US1] Add unit tests for domain math: src/Backend/CRM.Tests/Unit/Payments/PaymentDomainServiceTests.cs  
- [ ] T022 [US1] Add integration tests for POST/PUT happy paths + validation: src/Backend/CRM.Tests/Integration/Payments/PaymentsApiTests.cs  
- [ ] T023 [P] [US1] Frontend API client: add payments endpoints in src/Frontend/web/src/lib/api/payments.ts  
- [ ] T024 [P] [US1] Quotation detail: add "Add Payment" button and modal in src/Frontend/web/src/app/(protected)/quotations/[id]/components/PaymentModal.tsx  
- [ ] T025 [US1] Implement modal fields (Amount Received, Method, Date default today, Remarks) with inline validation in src/Frontend/web/src/app/(protected)/quotations/[id]/components/PaymentModal.tsx  
- [ ] T026 [US1] Show computed Paid and Balance preview on input change (client-side) in PaymentModal.tsx  
- [ ] T027 [US1] Submit payment to API and refresh quotation detail and aggregates in src/Frontend/web/src/app/(protected)/quotations/[id]/page.tsx  
- [ ] T028 [US1] UI smoke tests (if present): modal open/validate/submit in src/Frontend/web/src/tests/payments/PaymentModal.spec.tsx  

## Phase 4: US2 (P2) Payment Management List & Filters

- [ ] T029 [US2] Backend query: GET /quotations/payments with pagination/filters in src/Backend/CRM.Api/Controllers/QuotationsPaymentsController.cs  
- [ ] T030 [US2] Application query handler to return totals (total, totalPaid, balance, status) in src/Backend/CRM.Application/Payments/Queries/GetQuotationsWithPayments/*.cs  
- [ ] T031 [US2] Frontend API client for list endpoint in src/Frontend/web/src/lib/api/payments.ts  
- [ ] T032 [US2] TailAdmin-styled table page in src/Frontend/web/src/app/(protected)/payments/page.tsx  
- [ ] T033 [US2] Add quick search and filters (client name, date range, status, overdue toggle) in payments/page.tsx  
- [ ] T034 [US2] Status badges (green paid, yellow partial, red unpaid) component usage in payments/page.tsx  
- [ ] T035 [US2] Error/loading states + empty state in payments/page.tsx  
- [ ] T036 [US2] Route guard + RBAC check UI (hide actions if unauthorized) in payments/page.tsx  

## Phase 5: US3 (P3) Dashboard Widgets

- [ ] T037 [US3] Backend stats: aggregate counts per status endpoint GET /payments/stats in src/Backend/CRM.Api/Controllers/PaymentsStatsController.cs  
- [ ] T038 [US3] Frontend API client for stats in src/Frontend/web/src/lib/api/payments.ts  
- [ ] T039 [US3] Dashboard cards (Paid, Partial, Unpaid) in src/Frontend/web/src/app/(protected)/dashboard/components/PaymentsSummaryCards.tsx  
- [ ] T040 [US3] Optional small list or chart of recent payments in src/Frontend/web/src/app/(protected)/dashboard/components/RecentPayments.tsx  
- [ ] T041 [US3] Wire filters (all/overdue/date range) and refresh behavior in dashboard page at src/Frontend/web/src/app/(protected)/dashboard/page.tsx  

## Phase 6: History & Management UX

- [ ] T042 [US1] Backend: GET /quotations/{id}/payments/history endpoint in src/Backend/CRM.Api/Controllers/PaymentsController.cs  
- [ ] T043 [US1] Frontend: Payment History panel/modal in src/Frontend/web/src/app/(protected)/quotations/[id]/components/PaymentHistory.tsx  
- [ ] T044 [US1] Allow edit/delete actions for payments per RBAC in PaymentHistory.tsx and PaymentsController.cs  

## Final Phase: Polish & Cross-Cutting

- [ ] T045 Ensure all new pages and modals follow TailAdmin dark mode and accessibility in respective TSX files  
- [ ] T046 Add API docs (request/response) in src/Backend/CRM.Api/Docs/payments.md  
- [ ] T047 Add seed scenario for UAT (1000 total → 400 paid → +600) in seeds file  
- [ ] T048 UAT script and test report in specs/028-manual-payments/uat.md  
- [ ] T049 Push branch and open PR referencing spec and tasks (git)  

## Parallel Execution Examples

- T012/T013/T014 can run in parallel.  
- T023 (frontend client) can run in parallel with T019/T020 once contracts are known.  
- T029 (list endpoint) and T037 (stats) can run in parallel.  

## MVP Scope

- Complete Phase 3 (US1) end-to-end: Add payment from quotation detail, recalc, show updated status/balance.

## Independent Test Criteria per Story

- US1: Add/Update payment results in immediate recalculation; invalid amounts blocked.  
- US2: Filters return correct subsets; badges reflect status accurately.  
- US3: Dashboard counts change after a new payment.
