# Feature Specification: Client History & Activity Log (Spec-008)

**Feature Branch**: `008-client-history`  
**Created**: 2025-11-14  
**Status**: Draft  
**Input**: User description captured in request for Spec-008 (Client History & Activity Log)

## Clarifications

### Session 2025-11-14

- Q: Which actions must create history entries? → A: All client CRUD actions (create, update, delete, restore) plus optional access events when enabled.
- Q: Restoration policy? → A: Admin-only restore permitted within 30 days of deletion; action must itself be logged with reason and metadata.
- Q: Audit storage expectations? → A: History is immutable, retained ≥7 years, and can be archived but never deleted.
- Q: How should suspicious-activity scoring run? → A: Use a hybrid approach: inline heuristics catch obvious anomalies immediately, while a near-real-time background job handles deeper correlation within the 5-minute SLA.

## Overview

Client Management stakeholders (Admins, Sales Managers, Compliance Officers) require a tamper-proof timeline of every client change and user interaction. Spec-008 introduces a dedicated ClientHistory domain object, queries to surface timelines and user activity, restore workflows for deleted clients, suspicious-activity monitoring, and export capabilities for audits. The feature extends Spec-006 (Client CRUD) and Spec-007 (Client Search) by capturing every action and presenting it through purpose-built endpoints and UI experiences.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Inspect full client timeline (Priority: P1)

As an Admin or owning SalesRep, I need to review the complete timeline for a client to answer “what changed, who changed it, and when.”

**Why this priority**: Timely visibility into history underpins compliance, dispute resolution, and customer trust.

**Independent Test**: GET `/clients/{clientId}/history` returns paginated entries showing action type, actor, changed fields, and metadata for an owned client.

**Acceptance Scenarios**:

1. **Given** a SalesRep viewing a client they own, **When** they call the history endpoint without access logs, **Then** they receive only CRUD events ordered by ChangedAt DESC with before/after values.
2. **Given** an Admin reviewing the same client with `includeAccessLogs=true`, **When** they call the endpoint, **Then** access entries appear interleaved chronologically with action type filters available.

---

### User Story 2 - Restore a deleted client safely (Priority: P1)

As an Admin, I want to restore a client that was deleted within the allowed window so we can recover from mistakes without compromising data integrity.

**Why this priority**: Restoration prevents data loss and supports customer retention obligations.

**Independent Test**: POST `/clients/{clientId}/restore` with a valid Admin token reinstates a client deleted less than 30 days ago and appends a RESTORED history record.

**Acceptance Scenarios**:

1. **Given** a client soft-deleted 10 days ago, **When** an Admin submits a restore request with a reason, **Then** the client becomes active, DeletedAt clears, and the response includes updated metadata plus a RESTORED audit entry.
2. **Given** a deletion older than 30 days, **When** restoration is attempted, **Then** the system denies the request with “Restoration window expired” and no changes occur.

---

### User Story 3 - Monitor user activity (Priority: P2)

As a Sales Manager, I need to review actions taken by a team member so I can ensure processes are followed and coach appropriately.

**Why this priority**: Accountability and coaching rely on accurate per-user activity feeds.

**Independent Test**: GET `/users/{userId}/activity` (self or admin view) returns paginated ClientHistory entries filtered by actor and optional action/date parameters.

**Acceptance Scenarios**:

1. **Given** a SalesRep viewing their own activity, **When** they request the feed without filters, **Then** they see all actions they performed ordered by time.
2. **Given** an Admin tracking a rep’s client creations, **When** they filter by `actionType=CREATED` and date range, **Then** only matching entries appear with total counts for reporting.

---

### User Story 4 - Export history for audits (Priority: P2)

As a Compliance Officer, I must export client history for specified clients and time ranges so audits can be satisfied quickly.

**Why this priority**: Regulatory investigations require portable evidence with complete audit trails.

**Independent Test**: GET `/clients/history/export?format=csv` returns a downloadable file limited to authorized roles (Admin/Manager) with requested filters applied.

**Acceptance Scenarios**:

1. **Given** an Admin selects two clients and a date window, **When** they request CSV export, **Then** the file contains all matching entries with columns for actor, action type, changed fields, and reasons.
2. **Given** a Manager requesting PDF format, **When** the export service is available, **Then** the PDF is generated; otherwise the system responds with guidance if PDF is deferred to later phases.

---

### User Story 5 - Detect suspicious behavior (Priority: P3)

As an Admin, I want a dashboard of suspicious change patterns so I can jump on potential fraud or misuse.

**Why this priority**: Proactive detection reduces risk and surfaces issues before damage occurs.

**Independent Test**: GET `/admin/suspicious-activity` with date filters returns entries flagged above the configured suspicion threshold along with reasons.

**Acceptance Scenarios**:

1. **Given** multiple rapid updates to one client within an hour, **When** the detection heuristics run, **Then** the aggregated entry appears with a high suspicion score citing rapid-change rules.
2. **Given** an early-morning change from an unfamiliar IP, **When** the admin queries for the time window, **Then** the response highlights the event with reasons (“Unusual time”, “Unrecognized IP”).

---

### Edge Cases

- Client not found or not owned → return 404/403 before revealing history details.
- Restoration requested when client is already active → return 400 with actionable error.
- Access-log capture disabled → include only CRUD events while still allowing optional future toggle.
- Extremely large history (>10k records) → enforce pagination defaults (pageSize 20, max 100) and indicate total counts.
- Export with overlapping filters that match zero entries → return valid empty file with headers.
- Suspicious-activity heuristics yielding no hits → return empty list with metadata rather than errors.
- History immutability attempts (update/delete) → operations rejected by design to satisfy compliance.
- Archived history (older than retention window) → automatically fetched from cold storage indicators without breaking API contract.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST persist an immutable ClientHistory record for every client create, update, delete, restore, and (when enabled) access event with metadata describing actor, timestamp, and context.
- **FR-002**: System MUST capture before/after values for all fields changed during updates and deletions so that diffs can be reconstructed.
- **FR-003**: System MUST expose GET `/clients/{clientId}/history` returning paginated entries ordered by ChangedAt DESC with optional inclusion of access logs.
- **FR-004**: System MUST enforce authorization such that SalesReps view only their own clients’ history while Admins can view all clients.
- **FR-005**: System MUST provide GET `/clients/{clientId}/timeline` summarizing key milestones (creation, last modification, deletion status, total changes, restoration eligibility).
- **FR-006**: System MUST expose GET `/users/{userId}/activity` so authorized viewers can filter actions by actor, action type, and date range.
- **FR-007**: System MUST provide GET `/admin/suspicious-activity` for Admins only, highlighting entries above the configurable suspicion threshold with explicit reasons.
- **FR-007a**: Suspicious-activity scoring MUST use a hybrid pipeline combining inline lightweight heuristics during history writes with a near-real-time background processor (≤5 minutes) for advanced correlation, ensuring dashboards remain current without impacting CRUD latency.
- **FR-008**: System MUST offer GET `/clients/history/export` supporting CSV (default) and PDF (when available) with filters for clients, action types, and date windows, limited to Admin/Manager roles.
- **FR-009**: System MUST implement POST `/clients/{clientId}/restore` allowing Admins to reinstate clients deleted within 30 days, logging the restoration reason and preventing restores outside the window.
- **FR-010**: System MUST retain ClientHistory data for at least 7 years, permitting archival but prohibiting deletion or mutation of existing records.
- **FR-011**: System MUST store auxiliary metadata (IP address, user agent, automation flag, request IDs) to support forensics and suspicious-activity scoring.
- **FR-012**: System MUST surface change reasons when provided and prompt for them on deletes/restores to maintain narrative context.
- **FR-013**: System MUST provide stable pagination, default pageSize of 20 (max 100), and total counts for all paginated history endpoints.

### Key Entities

- **ClientHistory**: Immutable audit entry linked to a Client and User; captures action type, timestamps, optional change reason, before/after snapshots, metadata such as IP, user agent, automation flag, and extensible JSON payload for future context.
- **ClientTimelineSummary**: Aggregated view per client containing company info, creation date, current status (Active/Deleted), last modified metadata, total change count, deletion/restoration details, and ordered timeline entries.
- **SuspiciousActivityFlag**: Derived representation containing a HistoryId or client reference, suspicion score (0–10), reasons triggered (rapid changes, unusual hours, unknown IP, mass updates), detection timestamp, and review status indicators.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Admins can load a client’s full timeline (first page of history) in ≤5 seconds for 95% of requests at 10k history entries.
- **SC-002**: 100% of client create/update/delete/restore operations automatically produce corresponding immutable history records with actor attribution.
- **SC-003**: Authorized users identify the last change to a client (who/what/when) in ≤10 seconds using the timeline UI or API.
- **SC-004**: At least 90% of restoration attempts that meet policy (admin + ≤30 days) succeed without manual intervention; invalid attempts return precise error messaging.
- **SC-005**: Suspicious-activity endpoint surfaces high-signal events within 5 minutes of occurrence and reduces manual audit time by ≥40% compared to ad-hoc log reviews.
- **SC-006**: Export operations deliver filtered CSV/PDF files under 10 seconds for up to 5k history rows while respecting role-based restrictions.

## Assumptions

- History retention baseline is 7 years with optional archival strategies that remain transparent to API consumers.
- Suspicious-activity heuristics rely on configurable thresholds (e.g., ≥10 changes/hour, unusual IP ranges) defined by Security; initial defaults follow ClientHistoryConstants.
- Access logging is optional and can be toggled without re-ingesting historical data; when disabled, Accessed events simply do not appear.
- PDF export may arrive in a later milestone; until then the endpoint can respond with a descriptive message while CSV remains mandatory.

## Dependencies

- **Spec-006**: Provides Client entity, CRUD workflows, and domain events that trigger history creation.
- **Spec-007**: Supplies Client search, filtering, and SavedSearch infrastructure leveraged by Admin dashboards and exports.

