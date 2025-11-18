# Research Notes: Spec-008 Client History & Activity Log

## Decision 1: Immutable ClientHistory table with JSON diff payload
- **Decision**: Store each action in a dedicated `ClientHistories` table using append-only rows plus a `ChangesJson` column for before/after snapshots.
- **Rationale**: Keeps audit trail tamper-proof, aligns with constitution’s immutability requirement, and allows reconstruction of diffs without adding columns for every field.
- **Alternatives considered**:
  - **Soft-update existing Clients rows**: Fails compliance; history would mutate.
  - **Event sourcing for entire client aggregate**: Overkill for current scope and would require large refactor of Specs 006–007.

## Decision 2: Hybrid suspicious-activity scoring pipeline
- **Decision**: Run lightweight heuristics inline (e.g., rapid-change counter) and schedule a background job (≤5 min) for deep correlation such as unusual IP + off-hours changes.
- **Rationale**: Inline detection surfaces obvious issues immediately while the job aggregates across users/clients without adding latency to CRUD endpoints.
- **Alternatives considered**:
  - **Inline-only**: Risks slowing writes and missing cross-entity correlations.
  - **Batch-only**: Delays detection beyond SLA and leaves dashboards stale.

## Decision 3: Export formats & limits
- **Decision**: Implement CSV streaming now with 5k row soft limit per request and reserve PDF generation hooks for later milestone once template engine is ready.
- **Rationale**: CSV satisfies compliance immediately and fits existing infrastructure; PDF blockers can be resolved in future spec without blocking core delivery.
- **Alternatives considered**:
  - **Blocking PDF until template service ready**: Delays entire feature.
  - **Unbounded exports**: Violates performance constraints and risks long-running queries.

