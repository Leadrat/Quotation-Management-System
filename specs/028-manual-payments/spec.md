# Feature Specification: Manual Payment Update Management & Dashboard Integration

**Feature Branch**: `028-manual-payments`  
**Created**: 2025-11-25  
**Status**: Draft  
**Input**: User description captured from /speckit.specify invocation

## User Scenarios & Testing *(mandatory)*

<!--
  IMPORTANT: User stories should be PRIORITIZED as user journeys ordered by importance.
  Each user story/journey must be INDEPENDENTLY TESTABLE - meaning if you implement just ONE of them,
  you should still have a viable MVP (Minimum Viable Product) that delivers value.
  
  Assign priorities (P1, P2, P3, etc.) to each story, where P1 is the most critical.
  Think of each story as a standalone slice of functionality that can be:
  - Developed independently
  - Tested independently
  - Deployed independently
  - Demonstrated to users independently
-->

### User Story 1 - Record Manual Payment (Priority: P1)

As a Sales Rep/Manager/Admin, I can open a quotation and record a payment (full or partial) with method and remarks so that the balance and payment status update immediately.

**Why this priority**: Core value delivery; enables cash tracking and status accuracy.

**Independent Test**: From a quotation detail, submit a payment of X; verify payments history, updated paid total, balance due, and status recalculation without other system dependencies.

**Acceptance Scenarios**:

1. Given an Unpaid quotation of 1000, When I record a payment of 400 via UPI, Then PaidAmount total becomes 400, Balance Due becomes 600, and Status becomes Partially Paid.
2. Given a Partially Paid quotation with 900 paid on total 1000, When I add 100, Then PaidAmount becomes 1000, Balance Due 0, Status becomes Paid, and UI shows green status.
 
---

### User Story 2 - Payment Management List & Filters (Priority: P2)

As authorized users, we can view a Payments Management page listing quotations with Client, Date, Total, Amount Paid, Balance Due, and Payment Status, with quick search and filters by client, date range, and status.

**Why this priority**: Daily operations; enables tracking and follow‑up actions.

**Independent Test**: Seed quotations with mixed statuses; verify filtering, search, sorting, and totals display correctly without payment entry.

**Acceptance Scenarios**:

1. Given mixed statuses, When I filter by "Unpaid", Then the table shows only unpaid quotations and reflects correct balances.
2. Given a date range filter, When applied, Then only quotations created in that range are listed.

---

### User Story 3 - Dashboard Widgets (Priority: P3)

As authorized users, we can see widgets and lists on the dashboard for Paid, Partially Paid, and Unpaid quotations with quick filters.

**Why this priority**: Visibility and monitoring; supports managerial oversight.

**Independent Test**: With seeded data, verify the counts, lists, and filter chips update as filters change, independent of payment entry.

**Acceptance Scenarios**:

1. Given quotations across three statuses, When dashboard loads, Then badges show accurate counts and clicking a chip filters the list accordingly.
2. Given an additional payment changes a quotation from Partial to Paid, When dashboard refreshes, Then counts update accordingly.

---

### User Story 4 - Edge Cases Handling (Priority: P2)

As authorized users, the system handles edge cases such as payment value being zero, negative, or non-numeric, percentage inputs, overpayment attempts, timezone/date handling, concurrent updates, and deleted quotation or permission revoked during modal open.

**Why this priority**: Ensures system reliability and prevents errors.

**Independent Test**: Test each edge case scenario to verify the system's behavior.

**Acceptance Scenarios**:

1. Given a payment value of zero, negative, or non-numeric, When I submit the payment, Then the system blocks the submit with an inline message.
2. Given a percentage input, When I submit the payment, Then the system interprets it consistently and shows the computed amount before submit.
3. Given an overpayment attempt, When I submit the payment, Then the system blocks the payment and instructs the user to correct the amount.
4. Given a timezone/date handling scenario, When I submit the payment, Then the system stores the payment date in UTC and displays it in the user's locale.
5. Given a concurrent update scenario, When I submit the payment, Then the system handles the update correctly and prevents data drift.
6. Given a deleted quotation or permission revoked during modal open, When I submit the payment, Then the system returns a 403/404 error and shows an error toast.

---

[Add more user stories as needed, each with an assigned priority]

### Edge Cases

- Payment value is zero, negative, or non-numeric → validation blocks submit with inline message.
- Percentage inputs (e.g., 10%) vs decimals (e.g., 0.1) → interpret consistently: allow either; show computed amount before submit.
- Overpayment attempt (sum of payments > total) → block or cap? We will block and instruct user to correct the amount. [Assumption]
- Timezone/date handling for PaymentDate → store UTC, display in user locale.
- Concurrent updates (two users adding payments) → last write wins at record level; totals are computed server-side from history to avoid drift.
- Deleted quotation or permission revoked during modal open → submission returns 403/404 and UI shows error toast.

## Requirements *(mandatory)*

<!--
  ACTION REQUIRED: The content in this section represents placeholders.
  Fill them out with the right functional requirements.
-->

### Functional Requirements

- **FR-001 UI List**: Provide Payments Management page with table columns: Client, Date, Total Amount, Amount Paid, Balance Due, Payment Status. Quick search and filters (client, date range, status). Sort by date and balance.
- **FR-002 UI Modal**: From list row or quotation view, open a TailAdmin-styled modal to add a payment with fields: Amount Received (supports percent or decimal), Payment Method (enum), Payment Date (default now), Remarks (free text), and intended Status (Paid/Partially Paid/Unpaid) auto-suggested from math.
- **FR-003 Validation**: Inline validations: amount > 0; parsed amount must not exceed remaining balance; percent 0–100; date not in future [Assumption: allow today and past].
- **FR-004 Calculation**: Balance Due = Total − Sum(Partial Payments). UI shows computed Paid Total and Balance before submit and updates after success.
- **FR-005 History**: Display payment history per quotation including PaymentDate, Method, Amount, Remarks, and actor.
- **FR-006 Status Logic**: System updates quotation payment status derived from cumulative payments: Paid if total paid >= total amount; Partially Paid if 0 < total paid < total; Unpaid if total paid = 0.
- **FR-007 API Read**: Endpoint to fetch quotations with payment aggregates: total, totalPaid, balanceDue, status; supports pagination, sorting, and filters.
- **FR-008 API Write**: Endpoint to add a payment record; endpoint to update a payment record (amount, method, date, remarks). Both enforce validation and permissions.
- **FR-009 RBAC**: SalesRep, Manager, and Admin can create/update/delete payments; all authenticated roles can read aggregates they are allowed to see.
- **FR-010 Auditability**: Persist CreatedBy, CreatedAt, UpdatedAt on each payment; log changes to payment records.
- **FR-011 Dashboard**: Dashboard widgets for counts of Paid/Partial/Unpaid and quick lists; filter chips to refine by client/date/overdue.
- **FR-012 Theming/UX**: Follow TailAdmin theme; responsive behavior for mobile/desktop.

### Non-Functional Requirements

- **NFR-001 Performance**: List/API should return first page within 500ms for 1k quotations with indexed filters.
- **NFR-002 Reliability**: Calculations must be idempotent; aggregates computed server-side from payments table.
- **NFR-003 Security**: Enforce authorization per role and ownership rules where applicable.
- **NFR-004 i18n/Locale**: Currency and dates honor tenant/user locale settings.

### Key Entities

- **Payment**: PaymentId (UUID), QuotationId (FK), PaidAmount (decimal), PaymentDate (timestamp UTC), PaymentMethod (enum/string), Remarks (text), Status (enum), CreatedBy, CreatedAt, UpdatedAt.
- **Quotation (augmented)**: QuotationId, TotalAmount, Derived fields: TotalPaid (sum of payments), BalanceDue (TotalAmount − TotalPaid), PaymentStatus (derived state).

## Success Criteria *(mandatory)*

<!--
  ACTION REQUIRED: Define measurable success criteria.
  These must be technology-agnostic and measurable.
-->

### Measurable Outcomes

- **SC-001**: Authorized user can record a payment in ≤ 30s with immediate balance/status update visible in UI and API.
- **SC-002**: Dashboard widgets reflect accurate counts within 5s of a payment update (eventual consistency acceptable).
- **SC-003**: Validation prevents >99.9% of invalid entries (negative, zero, overpayment) in testing.
- **SC-004**: Payments list first-page response p50 ≤ 500ms with filters on a dataset of 1k quotations.
- **SC-005**: RBAC tests confirm SalesRep/Manager/Admin can update; non-privileged roles cannot.

### Assumptions

- Overpayments are disallowed; user must adjust to exact remaining balance or multiple partials that sum to total.
- Payment methods initial enum: Cash, Card, UPI, BankTransfer, Other; extensible later.
- Date validation allows current and past dates; future dates blocked.

### [NEEDS CLARIFICATION]

1. Should overpayment be auto-capped to remaining balance or blocked with an error? (Currently assumed: block.)
2. RESOLVED: All roles (SalesRep, Manager, Admin) may edit and delete posted payment records.
3. Visibility rules: Should users only see quotations they own vs. team vs. org-wide? Define scope per role.
