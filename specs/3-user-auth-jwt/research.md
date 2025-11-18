# Spec-003 Research & Decisions

Created: 2025-11-12
Feature: ./spec.md

## Decisions
- Refresh token transport: HttpOnly, Secure, SameSite=None cookie (JSON body fallback for non-browser)
- Refresh token rotation: rotate on every refresh; revoke old, insert new
- Account lockout: no lockout; monitor failed attempts and alert on abuse
- Access token transport: support Authorization header (Bearer) and HttpOnly cookie; require CSRF protections if cookie used on state-changing routes
- JWT secret rotation: every 90 days with overlap and dual-key validation
- Deployment topology: separate subdomains (app.crm.com ↔ api.crm.com) with strict CORS allowlist and credentials=true

## Best Practices
- JWT: HS256 with ≥32-char secret; validate iss/aud/lifetime; clock skew ≤ 2 min
- Cookies: Secure, HttpOnly; SameSite=None for cross-site; set domain/path narrowly
- Security headers: Strict-Transport-Security, X-Content-Type-Options, X-Frame-Options, Referrer-Policy
- Brute-force monitoring: track attempts by account and IP; alert on patterns
- Refresh storage: DB record with Unique TokenJti; prune expired daily

## Open Items
- None; clarifications resolved
