# Research for Spec-005: User Profile & Password Management

## Decisions and Rationale

- Decision: Transactional email via async queue (SES/SendGrid)
  Rationale: Non-blocking API, high deliverability, retry policies.
  Alternatives: SMTP sync (blocks), SMTP async (fragile), service bus + microservice (overkill now).

- Decision: Admin reset uses one-time email reset link (no temp password exposure), 24h expiry
  Rationale: Minimizes insider risk and ensures user selects a strong final password.
  Alternatives: Show temp password to admin, email temp password, admin sets final password.

- Decision: Lockout is global per-account (5 failed current-password attempts)
  Rationale: Prevents distributed guessing; simple to reason about.
  Alternatives: Per-session only; per-IP throttle without lockout.

- Decision: BCrypt cost 12
  Rationale: Strong security with acceptable latency on .NET 8.
  Alternatives: Cost 10 (weaker), 14 (slower), Argon2id (good but new dependency and migration).

- Decision: DB-backed reset token, random 32 bytes, HMAC/SHA-256 stored, single-use, revoke on re-issue
  Rationale: Simple invalidation, auditability, avoids self-contained token misuse.
  Alternatives: JWT reset token; DPAPI opaque token; Redis TTL token.

## Best Practices & Patterns

- Password hashing: BCrypt.Net-Next with work factor 12; avoid rehash unless cost changes.
- Validation: FluentValidation for request DTOs; regexes for names and E.164; explicit messages.
- EF Core: Unique composite index (UserId, UsedAt null) for active reset tokens; FK to Users; UTC timestamps.
- Audit: Domain events (UserProfileUpdated, PasswordChanged, PasswordReset) → handlers write structured logs/DB audit.
- Email: Queue event on domain event; background worker sends via provider; implement retry with exponential backoff.
- Security headers: Already enforced at API; continue using generic error messages.
- Token invalidation: On password change, revoke all refresh tokens for the user and clear server state as applicable.
- Testing: Unit tests for validators and handlers; integration tests for endpoints and DB effects.

## Open Questions (none)
All critical clarifications resolved in spec.
