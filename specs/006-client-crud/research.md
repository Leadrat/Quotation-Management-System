# Research: Spec-006 Client Entity & CRUD

Created: 2025-11-13
Branch: 006-client-crud

## Decisions, Rationale, Alternatives

### 1) Email uniqueness scope and index strategy
- Decision: Global, case-insensitive uniqueness for active (non-deleted) clients via partial unique index WHERE DeletedAt IS NULL. Normalize email to lowercase on write; compare in lowercase.
- Rationale: Prevents duplicates across org, aligns with dedupe needs, supports soft delete reuse, deterministic across environments.
- Alternatives:
  - Per-owner uniqueness (CreatedByUserId, Email) — rejected (cross-team duplicates permitted inadvertently).
  - App-layer-only checks — rejected (race conditions, non-atomic).
  - DB collation-based case-insensitive unique index — rejected (cross-env collation variance risk).

### 2) Pagination defaults and OOB handling
- Decision: Default pageSize=10; max=100; clamp pageNumber<1→1 and pageSize>100→100; return 200 with corrected meta.
- Rationale: User-friendly, prevents heavy queries, consistent UX.
- Alternatives:
  - 400 on OOB — noisier UX; acceptable but less friendly.
  - No max — risk of expensive scans.

### 3) Email normalization policy
- Decision: Store lowercase; compare lowercase everywhere.
- Rationale: Simple, fast, avoids collation issues.
- Alternatives: Store-as-entered + case-insensitive collations — environment sensitive.

### 4) StateCode validation (India GST)
- Decision: In-code constants list; reviewed quarterly.
- Rationale: Small, rarely changing list; zero DB round-trips; simple deployment.
- Alternatives: DB lookup table (adds migrations/seeding), external service (adds latency/dependency).

### 5) GSTIN requirement policy
- Decision: Required for India B2B; optional for B2C and non-India. If provided, must pass format regex.
- Rationale: Matches invoicing compliance needs without over-restricting other clients.
- Alternatives: Always optional (weak compliance), always required for India (too strict).

### 6) Soft delete model
- Decision: Use DeletedAt nullable timestamp; all queries exclude DeletedAt IS NOT NULL.
- Rationale: Preserve audit/history; reversible.
- Alternatives: Hard delete — rejected for compliance/audit.

### 7) Ownership and RBAC
- Decision: CreatedByUserId sets ownership; SalesRep limited to own clients; Admin can access all.
- Rationale: Least-privilege; matches business control.
- Alternatives: Team-based ownership (future enhancement in Spec-008/roles).

### 8) Indexes
- Decision:
  - Unique partial index on lower(Email) WHERE DeletedAt IS NULL
  - Index(Gstin) for invoicing lookups
  - Index(CreatedAt), Index(UpdatedAt) for sorting
  - Index(DeletedAt) for soft-delete filters
  - Composite Index(CreatedByUserId, DeletedAt) for fast active-by-owner
- Rationale: Query patterns and constraints from requirements.
- Alternatives: Full-text search on companyName/email deferred to Spec-007.

## Open Questions (none)
All critical clarifications resolved in spec.
