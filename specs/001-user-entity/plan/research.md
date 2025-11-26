# Research: Spec-001 User Entity & DTO

Date: 2025-11-12

## Decisions

- Active-state source of truth: DeletedAt is canonical; IsActive must be false when DeletedAt is set.
- Email case-insensitivity: Use Postgres `citext` column with UNIQUE index (enable extension).
- Password hashing: BCrypt with work factor 12 (BCrypt.Net-Next).
- Role changes & ReportingManagerId: To SalesRep → require ReportingManagerId (Manager). From SalesRep → set NULL.
- PhoneCode consistency: If Mobile and PhoneCode provided, PhoneCode must match country code derived from Mobile (E.164).

## Rationale

- DeletedAt canonical: Standard soft-delete pattern improves auditability and query consistency.
- citext: Ensures DB-level correctness; avoids app-only uniqueness gaps.
- BCrypt: Mature, battle-tested; cost 12 balances security/perf.
- Role transitions: Prevents stale manager links and enforces workflow assumptions.
- Phone metadata: Avoids divergent data and simplifies analytics/filtering.

## Alternatives Considered

- Email lower() functional index vs citext → chose citext for simpler semantics.
- Argon2id vs BCrypt → selected BCrypt for library maturity and current stack alignment.
- Drop IsActive and only use DeletedAt → retained IsActive for admin suspension but subordinate to DeletedAt.

## Open Questions

- None critical for this spec.
