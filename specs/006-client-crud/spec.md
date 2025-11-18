# Feature Specification: Client Entity & CRUD Operations (Spec-006)

**Feature Branch**: `006-client-crud`  
**Created**: 2025-11-13  
**Status**: Draft  
**Input**: User description captured in request for Spec-006 (Client Entity & CRUD Operations)

## Clarifications

### Session 2025-11-13

- Q: Email uniqueness scope and index strategy → A: Global, case-insensitive uniqueness with a partial
  unique index on active records (WHERE DeletedAt IS NULL).
- Q: Pagination defaults, limits, and out-of-bounds handling → A: Default pageSize=10; max=100; clamp
  pageNumber<1 to 1 and pageSize>100 to 100; return 200 with corrected meta.
- Q: Email normalization policy for storage and comparisons → A: Store email in lowercase on write and
  perform all comparisons using lowercase.
- Q: StateCode validation source and governance → A: Validate against an in-code constants list for India
  GST state codes with a quarterly review note in governance.
- Q: GSTIN requirement policy for India B2B clients → A: GSTIN is required for India B2B clients; optional
  otherwise (e.g., B2C or non-India).

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

### User Story 1 - Create client quickly (Priority: P1)

As a SalesRep, I want to add a new client with minimal required fields so I can start creating
quotations immediately.

**Why this priority**: Enables core CRM flow; creates the foundational entity for all downstream
quotation work.

**Independent Test**: POST /clients with valid data returns 201 and persists client owned by the
current user.

**Acceptance Scenarios**:

1. **Given** a valid SalesRep token, **When** I submit companyName, email, and mobile, **Then** the system
   creates a client, sets CreatedByUserId to me, and returns 201 with ClientDto.
2. **Given** an email already used by an active client, **When** I submit create, **Then** 409 Conflict is
   returned and no new client is created.

---

### User Story 2 - View my clients (Priority: P1)

As a SalesRep, I want to list only the clients I own with pagination so I can manage my book of
business efficiently.

**Why this priority**: Daily workflow; ensures data ownership boundaries.

**Independent Test**: GET /clients returns only my non-deleted clients with page metadata.

**Acceptance Scenarios**:

1. **Given** a SalesRep token, **When** I call GET /clients, **Then** I see only clients where CreatedByUserId
   equals my user and DeletedAt is null.
2. **Given** many clients, **When** I supply pageNumber and pageSize, **Then** results are paginated and sorted
   by CreatedAt DESC.

---

### User Story 3 - View client details (Priority: P2)

As a SalesRep, I want to view a client’s full details so I can confirm contact info before sending a
quotation.

**Independent Test**: GET /clients/{id} returns 200 for owner and 403 for non-owner.

**Acceptance Scenarios**:

1. Owner’s token returns 200 and ClientDto.
2. Non-owner SalesRep token returns 403. Admin token returns 200.

---

### User Story 4 - Update client (Priority: P2)

As a SalesRep, I want to update a client’s details so I can keep records accurate.

**Independent Test**: PUT /clients/{id} updates fields, sets UpdatedAt, enforces uniqueness and
ownership.

**Acceptance Scenarios**:

1. Owner updates contactName and email to a unique email → 200 and updated values in DB.
2. Owner attempts to update email to another client’s email → 409 Conflict.
3. Non-owner SalesRep attempts update → 403.

---

### User Story 5 - Soft delete client (Priority: P2)

As a SalesRep, I want to soft delete a client so inactive records don’t clutter my list but remain in
history.

**Independent Test**: DELETE /clients/{id} sets DeletedAt, returns 200 with timestamp; subsequent
GETs exclude it.

**Acceptance Scenarios**:

1. Owner deletes client → 200 and DeletedAt set; not returned by list/get.
2. Non-owner delete → 403. Admin can delete any client.

---

### User Story 6 - Admin oversight (Priority: P3)

As an Admin, I can view, create, update, and delete any client to support team operations and data
corrections.

**Independent Test**: Admin token bypasses CreatedBy ownership checks while observing soft-delete
rules.

### Edge Cases

<!--
  ACTION REQUIRED: The content in this section represents placeholders.
  Fill them out with the right edge cases.
-->

- Creating with valid optional fields omitted (ContactName, GSTIN, StateCode, Address fields)
- GSTIN provided but invalid format → 400 validation error
- E.164 mobile format strictly enforced
- Email case-insensitivity for uniqueness check
- Email entered with mixed case normalizes to lowercase; duplicates across case variants rejected
- Accessing soft-deleted client by ID → 404 Not Found
- Pagination parameters out of bounds: clamp pageNumber<1 to 1 and pageSize>100 to 100; return 200 with
  corrected metadata in response
- StateCode not in constants list → 400 with specific error
- India B2B client without GSTIN → 400; India B2C or non-India without GSTIN → allowed

## Requirements *(mandatory)*

<!--
  ACTION REQUIRED: The content in this section represents placeholders.
  Fill them out with the right functional requirements.
-->

### Functional Requirements

- **FR-001**: System MUST allow authenticated SalesRep/Admin to create clients.
- **FR-002**: System MUST enforce global, case-insensitive email uniqueness across active (non-deleted)
  clients, backed by a partial unique index WHERE DeletedAt IS NULL.
- **FR-003**: System MUST validate mobile in E.164 format and email in RFC 5322.
- **FR-004**: System SHOULD validate GSTIN and StateCode when provided; both optional.
- **FR-005**: System MUST implement soft delete via DeletedAt and exclude deleted from reads by default.
- **FR-006**: System MUST enforce ownership: SalesRep can access only their clients; Admin can access all.
- **FR-007**: System MUST provide paginated list sorted by CreatedAt DESC with total count.
- **FR-008**: System MUST expose CRUD endpoints with appropriate HTTP semantics and error codes.
- **FR-009**: System MUST publish domain events on create, update, and delete.
- **FR-010**: System MUST provide DTOs that include CreatedByUserName and DisplayName.
- **FR-011**: System MUST return 403 for unauthorized ownership access and 404 for missing/deleted.
- **FR-012**: System MUST record audit trail entries for CRUD operations.
- **FR-013**: Pagination policy: default pageSize=10; maximum pageSize=100; clamp pageNumber<1 to 1 and
  pageSize>100 to 100; responses return 200 with corrected pagination metadata.
- **FR-014**: Email normalization: store email in lowercase on write; perform all email comparisons in
  lowercase to enforce case-insensitive behavior consistently.
- **FR-015**: StateCode validation uses an in-code constants list for India GST state codes; list is
  reviewed quarterly and updated via standard change control.
- **FR-016**: GSTIN requirement: REQUIRED for India B2B clients; OPTIONAL for B2C and non-India clients; if
  provided, MUST pass format validation.

### Key Entities *(include if feature involves data)*

- **Client**: Represents an external organization or person. Attributes include identity fields (ClientId),
  company/contact, communication (Email, Mobile, PhoneCode), compliance (Gstin, StateCode), location
  (Address, City, State, PinCode), ownership (CreatedByUserId), and timestamps (CreatedAt, UpdatedAt,
  DeletedAt).

## Success Criteria *(mandatory)*

<!--
  ACTION REQUIRED: Define measurable success criteria.
  These must be technology-agnostic and measurable.
-->

### Measurable Outcomes

- **SC-001**: SalesRep can create a client with required fields in ≤ 3 minutes.
- **SC-002**: List endpoint returns only owned, active clients with correct pagination meta.
- **SC-003**: Ownership and RBAC rules prevent cross-user access with 0 known regressions.
- **SC-004**: 100% of CRUD operations emit audit entries; deleted clients are hidden from lists and get-by-id.
