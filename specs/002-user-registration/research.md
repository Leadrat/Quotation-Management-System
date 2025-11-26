# Spec-002 Research & Decisions

Created: 2025-11-12
Feature: ./spec.md

## Decisions

- Decision: Require client email verification before first login
  - Rationale: Reduce spam/fraud; aligns with best practices
  - Alternatives: No verification; limited actions until verified

- Decision: No captcha for public registration in Spec-002
  - Rationale: Keep UX simple; rely on rate limiting (≤5/hour)
  - Alternatives: Checkbox captcha; invisible/score-based captcha

- Decision: Allow disposable email domains in Spec-002
  - Rationale: Lower friction; revisit if abuse observed
  - Alternatives: Denylist disposable domains; flag-only

## Best Practices References (summarized)
- Password hashing: bcrypt cost 12 (OWASP ASVS)
- Case-insensitive email: PostgreSQL citext (done in Spec-001)
- Rate limiting: IP-based, server enforced, log blocked attempts
- Verification tokens: time-bound, single-use, hashed at rest (details deferred to Spec-003)

## Open Items
- None (all clarifications resolved in this session)
