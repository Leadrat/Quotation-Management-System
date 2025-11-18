# Research: Spec-007 Client Search, Filtering & Advanced Queries

Created: 2025-11-14

## Decisions

- Hybrid search: PostgreSQL FTS (TSVECTOR + GIN) primary; ILIKE fallback for very short terms (<3) or when FTS 0 results.
- Filter options caching: Daily refresh.
- Export: Default CSV, streamed, hard cap 10k rows.
- Saved searches: Private per user; Admin can view all (optional userId param), owner-only delete unless Admin.
- Sorting tiebreakers: After primary SortBy, use CreatedAt DESC, then ClientId ASC.
- Search history: Keep last 20 searches per user; visible only to the user.
- API param naming: API uses userId; map internally to CreatedByUserId.

## Rationale

- Hybrid FTS: Combines relevance and resilience. FTS performs well with proper GIN indexes; fallback ensures short-term queries still yield results.
- Daily cached facets: State/City facet cardinalities don’t churn rapidly; daily cache reduces DB load.
- CSV default: Universal, small footprint; streaming with 10k cap mitigates resource spikes.
- Privacy defaults: Saved searches and history are personal by default; admin override preserves operational control.
- Deterministic sorting: Stable tiebreakers avoid pagination inconsistencies.

## Alternatives Considered

- Pure FTS only: Higher precision, but poor UX for short terms and zero-hit scenarios.
- External search (Elasticsearch): Powerful, but adds operational complexity; not needed for Phase 1 scale.
- No export cap: Risks long-running queries, memory pressure, poor UX.
- Organization/shared saved searches: Useful, but deferred to Phase 2+ to limit scope.

## Open Questions (Resolved)

- None. Clarification loop closed for Spec-007.
