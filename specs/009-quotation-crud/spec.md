# Feature Specification: Quotation Entity & CRUD Operations (Spec-009)

**Feature Branch**: `009-quotation-crud`  
**Created**: 2025-11-15  
**Status**: Draft  
**Input**: User description captured in request for Spec-009 (Quotation Entity & CRUD Operations)

## Overview

Sales Representatives require a complete quotation management system to create, view, update, and delete professional price quotations for clients. A quotation includes itemized products/services with quantities, rates, automatic tax calculation (GST - CGST/SGST/IGST), discounts, and total amounts. Quotations have a status lifecycle (Draft, Sent, Viewed, Accepted, Rejected, Expired, Cancelled) and support full CRUD operations with proper authorization and audit trails.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create quotation with line items (Priority: P1)

As a Sales Representative, I need to create a professional quotation with multiple line items so I can send accurate price offers to clients.

**Why this priority**: Core functionality - without this, the quotation system cannot function.

**Independent Test**: `POST /api/v1/quotations` creates a quotation with line items, calculates subtotal, tax, and total correctly.

**Acceptance Scenarios**:

1. **Given** a SalesRep viewing a client they own, **When** they create a quotation with 3 line items, **Then** the system calculates subtotal, applies discount, calculates tax (CGST/SGST for intra-state or IGST for inter-state), and returns the complete quotation with total amount.
2. **Given** a SalesRep creating a quotation, **When** they provide invalid data (no line items, negative quantity, etc.), **Then** the system returns validation errors before saving.

---

### User Story 2 - View and list quotations (Priority: P1)

As a Sales Representative, I need to view my quotations and filter them by status, client, or date so I can track my sales pipeline.

**Why this priority**: Essential for managing quotations and tracking progress.

**Independent Test**: `GET /api/v1/quotations` returns paginated quotations with filters, and `GET /api/v1/quotations/{id}` returns full quotation details with line items.

**Acceptance Scenarios**:

1. **Given** a SalesRep with 10 quotations, **When** they request the list with pageSize=5, **Then** they receive 5 quotations per page with pagination metadata.
2. **Given** a SalesRep filtering by status=SENT, **When** they request quotations, **Then** only SENT quotations are returned.
3. **Given** a SalesRep viewing a quotation, **When** they request by ID, **Then** they see all line items, tax breakdown, and totals.

---

### User Story 3 - Update draft quotations (Priority: P1)

As a Sales Representative, I need to edit draft quotations to correct errors or add items before sending.

**Why this priority**: Prevents errors from being sent to clients and improves efficiency.

**Independent Test**: `PUT /api/v1/quotations/{id}` updates a DRAFT quotation and recalculates totals; attempts to update SENT quotations are rejected.

**Acceptance Scenarios**:

1. **Given** a DRAFT quotation, **When** a SalesRep updates line items or discount, **Then** the system recalculates totals and saves changes.
2. **Given** a SENT quotation, **When** a SalesRep attempts to update it, **Then** the system returns 400 Bad Request with "Only DRAFT quotations can be edited."

---

### User Story 4 - Delete draft quotations (Priority: P2)

As a Sales Representative, I need to delete draft quotations that are no longer needed.

**Why this priority**: Keeps the quotation list clean and prevents clutter.

**Independent Test**: `DELETE /api/v1/quotations/{id}` soft-deletes a DRAFT quotation; attempts to delete SENT quotations are rejected.

**Acceptance Scenarios**:

1. **Given** a DRAFT quotation, **When** a SalesRep deletes it, **Then** the quotation is soft-deleted and removed from active list.
2. **Given** a SENT quotation, **When** a SalesRep attempts to delete it, **Then** the system returns 400 Bad Request with "Only DRAFT quotations can be deleted."

---

### User Story 5 - Automatic tax calculation (Priority: P1)

As a Sales Representative, I need the system to automatically calculate GST (CGST/SGST for intra-state, IGST for inter-state) based on client location so I don't make calculation errors.

**Why this priority**: Ensures tax compliance and accuracy - critical for legal and financial correctness.

**Independent Test**: Creating quotations for intra-state and inter-state clients calculates correct tax amounts automatically.

**Acceptance Scenarios**:

1. **Given** a client in the same state as the company, **When** a quotation is created, **Then** CGST and SGST are calculated (9% each, totaling 18%).
2. **Given** a client in a different state, **When** a quotation is created, **Then** IGST is calculated (18% total).
3. **Given** a quotation with discount, **When** tax is calculated, **Then** tax is applied to (SubTotal - DiscountAmount).

---

### Edge Cases

- Quotation with zero line items → validation error before save.
- Discount exceeds subtotal → validation error.
- Client state code missing → use default tax calculation or error.
- Quotation number collision → retry with new number.
- Concurrent updates to same quotation → optimistic locking or last-write-wins.
- Very large line item lists (>100 items) → pagination or performance optimization.
- Invalid date ranges (ValidUntil before QuotationDate) → validation error.
- Negative quantities or rates → validation error.
- Tax calculation with very large amounts → precision handling (decimal vs float).
- Quotation created by user who no longer exists → handle gracefully (preserve CreatedByUserId).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST support creating quotations with multiple line items (minimum 1, no maximum).
- **FR-002**: System MUST automatically calculate subtotal as sum of (Quantity × UnitRate) for all line items.
- **FR-003**: System MUST support discount as percentage (0-100%) applied to subtotal.
- **FR-004**: System MUST automatically calculate tax based on client location:
  - Intra-state (same state): CGST = 9%, SGST = 9% (total 18%)
  - Inter-state (different state): IGST = 18%
- **FR-005**: System MUST calculate total as: SubTotal - DiscountAmount + TaxAmount.
- **FR-006**: System MUST generate unique quotation numbers in format configurable via settings (e.g., QT-2025-001234).
- **FR-007**: System MUST enforce quotation status lifecycle: DRAFT → SENT → VIEWED → ACCEPTED/REJECTED/EXPIRED.
- **FR-008**: System MUST allow editing only DRAFT quotations; SENT/VIEWED/ACCEPTED quotations are immutable.
- **FR-009**: System MUST allow deletion only of DRAFT or CANCELLED quotations.
- **FR-010**: System MUST enforce authorization: SalesReps see only own quotations; Admins see all quotations.
- **FR-011**: System MUST support filtering quotations by: status, client, date range, created by user.
- **FR-012**: System MUST support pagination for quotation lists (default 10, max 100 per page).
- **FR-013**: System MUST track quotation expiration (ValidUntil date, default +30 days from creation).
- **FR-014**: System MUST persist line items with sequence numbers for proper ordering.
- **FR-015**: System MUST validate all inputs: quantities > 0, rates > 0, discount 0-100%, dates valid.

### Key Entities

- **Quotation**: Main quotation header with client reference, status, dates, totals, and notes.
- **QuotationLineItem**: Individual items/services within a quotation with quantity, rate, and calculated amount.
- **QuotationStatus**: Enum representing lifecycle states (Draft, Sent, Viewed, Accepted, Rejected, Expired, Cancelled).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: SalesRep can create a quotation with 5 line items in ≤30 seconds (including tax calculation).
- **SC-002**: Quotation list loads with pagination in ≤2 seconds for 100 quotations.
- **SC-003**: Tax calculation accuracy: 100% correct for both intra-state and inter-state scenarios.
- **SC-004**: Authorization enforcement: 0% unauthorized access (SalesReps cannot see other users' quotations).
- **SC-005**: Validation coverage: 100% of invalid inputs rejected before database save.
- **SC-006**: Quotation number uniqueness: 0 collisions in production (with retry mechanism).

## Assumptions

- Company state code is stored in system settings (not in this spec).
- GST rate is 18% (9% CGST + 9% SGST for intra-state, 18% IGST for inter-state) - configurable in future.
- Quotation expiration default is 30 days from creation - configurable.
- Quotation numbers are auto-generated and immutable after creation.
- Line items are ordered by SequenceNumber for display.
- Soft delete is used for quotations (preserve audit trail).

## Dependencies

- **Spec-006**: Provides Client entity and CRUD operations (quotations reference clients).
- **Spec-003**: Provides UserAuthentication and JWT (for authorization).
- **Spec-008**: Provides audit trail patterns (can be extended for quotation history).

## Frontend Requirements

- **FE-001**: Quotation list page with search, filter, and pagination.
- **FE-002**: Create/Edit quotation form with multi-step workflow (Client → Details → Line Items → Review).
- **FE-003**: Quotation viewer displaying professional PDF-like layout.
- **FE-004**: Real-time tax calculation as user types (updates instantly).
- **FE-005**: Line item repeater component (add/remove items dynamically).
- **FE-006**: Status badges with color coding (Draft=gray, Sent=blue, Accepted=green, etc.).
- **FE-007**: Mobile responsive design for all quotation pages.
- **FE-008**: Form validation with inline error messages.
- **FE-009**: Loading states and error handling for all API calls.
- **FE-010**: Tax breakdown preview showing CGST/SGST or IGST based on client selection.

