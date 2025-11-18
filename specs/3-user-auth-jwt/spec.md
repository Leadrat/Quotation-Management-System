# Spec-003: User Authentication & JWT Token Generation

Project: CRM Quotation Management System
Group: User & Authentication (Group 1 of 11)
Priority: CRITICAL (Foundation - must be completed in Phase 1)
Dependencies: Spec-001 (UserEntity), Spec-002 (UserRegistration)
Related: Spec-004 (RoleManagement), Spec-005 (UserProfileManagement)

## Overview
Defines secure authentication including login/logout, JWT access/refresh tokens, lockout, and audit. All non-public APIs require a valid JWT access token.

## Key Features
- Secure login with email/password
- JWT access token (1h) + refresh token (30d)
- Refresh mechanism and optional rotation
- Failed login tracking and account lockout (≥5)
- Logout (refresh revocation; optional access blacklist)
- CORS-safe token transmission (prefer HttpOnly cookie for refresh)
- Stateless access tokens (no server session)

## JTBD Alignment
- Persona: All (Sales Rep, Manager, Client, Admin)
- Goal: Login quickly and securely to access dashboard (<30s)
- Success: Stay logged in ~8 hours
- Security: Protect from brute force

## Business Value
- Enables secure access to features
- Protects data; supports RBAC
- Auditable sessions and events
- Works for web and mobile

## Login Flow (POST /api/v1/auth/login)
1) Validate input (email RFC 5322, password present) → 400 on fail
2) Find user by email (case-insensitive, not deleted)
3) Check status: active → 403 if inactive (no lockout state)
4) Verify password (BCrypt)
   - If wrong: increment attempts (monitor only); return generic 401 (no lockout)
5) On success: reset attempts, update LastLoginAt
6) Generate tokens
   - Access: JWT with sub/email/name/role/iat/exp (1h), HS256
   - Refresh: JWT with sub/refreshTokenId/iat/exp (30d), HS256; persist row
7) Respond 200 with tokens, expiresIn, and basic user info
8) Emit UserLoggedIn; audit info (IP, UA)

## Refresh Flow (POST /api/v1/auth/refresh-token)
1) Accept refresh token via HttpOnly, Secure, SameSite=strict cookie (preferred). For non-browser clients only, allow JSON body fallback.
2) Validate signature/expiry → 401 on invalid/expired
3) Extract userId; verify DB record (not revoked, not expired)
4) Issue new access token; rotate refresh token on every refresh (revoke old, insert new)
5) Respond 200 with new tokens/expiresIn
6) Emit TokenRefreshed; audit

## Logout Flow (POST /api/v1/auth/logout)
1) Authenticated request (Bearer access token)
2) Revoke refresh token in DB (mark revoked, timestamp)
3) Optional access blacklist until exp
4) Respond 200; emit UserLoggedOut; audit

## Commands & Handlers
- LoginCommand → LoginCommandHandler
- RefreshTokenCommand → RefreshTokenCommandHandler
- LogoutCommand → LogoutCommandHandler

Handlers perform validation, state checks, password verify, token issue/verify, persist/rotate refresh token, and emit events.

## Result Models
- LoginResult: success, message, accessToken, refreshToken, expiresIn, user, timestamp
- RefreshTokenResult: success, message, accessToken, refreshToken?, expiresIn
- LogoutResult: success, message, timestamp

## JWT Specifications
- HS256 signing with JwtSettings.Secret (≥32 chars)
- Access claims: sub, email, firstName, lastName, roleId, roleName, iat, exp, iss, aud
- Refresh claims: sub, refreshTokenId, iat, exp, iss, aud
- Access exp: 3600s; Refresh exp: 2592000s
- Issuer: crm.system; Audience: crm.api

## Database: RefreshTokens
- Columns: RefreshTokenId (PK), UserId (FK), TokenJti (unique), IsRevoked, RevokedAt, ExpiresAt, CreatedAt, LastUsedAt
- Indexes: PK, UNIQUE(TokenJti), IDX(UserId), IDX(ExpiresAt), IDX(IsRevoked)
- Ops: insert/verify/revoke/cleanup

- POST /api/v1/auth/login → 200, errors 400/401
- POST /api/v1/auth/refresh-token → 200, error 401 (refresh token read from HttpOnly cookie; JSON body allowed for non-browser clients)
- POST /api/v1/auth/logout → 200, error 401

## Middleware & Authentication
- Use ASP.NET Core JWT Bearer authentication with TokenValidationParameters (issuer/audience/lifetime/signing key)
- Access token transport: accept Authorization header (Bearer) and HttpOnly cookie (for browser convenience)
- If cookie is used for access token on state-changing routes, enforce CSRF protections (anti-forgery token or double-submit cookie)
- Support secret rotation with dual-key validation (current + previous) during rollout window
- CORS: allowlist frontend origin (e.g., https://app.crm.com) to access API at https://api.crm.com with credentials enabled
- Cookies set with Secure; refresh cookie SameSite=None for cross-site; limit domain to api subdomain as needed
- app.UseAuthentication(); app.UseAuthorization();

## Exceptions → HTTP
- InvalidCredentialsException → 401
- UserNotActiveException → 403
- InvalidTokenException/TokenExpiredException/TokenRevokedException → 401

## Domain Events
- UserLoggedIn, TokenRefreshed, UserLoggedOut, LoginAttemptFailed (with IP, UA, timestamps)

## Security Requirements
- BCrypt.Verify for passwords; never log secrets
- Generic error messages for login
- HTTPS in production; strong secret; HttpOnly cookies for refresh preferred
- Audit all auth events; detect suspicious patterns
- Refresh tokens are rotated on every refresh
- No account lockout; monitor failed attempts and alert on abuse
- Access token can be sent via header or HttpOnly cookie; if cookie is used, CSRF protections MUST be enabled for state-changing endpoints
- JWT signing secret rotated every 90 days with overlap window and dual-key validation
- CORS configured for separate subdomains (app.crm.com ↔ api.crm.com) with strict allowlist and credentials=true; refresh cookie uses SameSite=None; Secure

## Test Cases (summary)
- Successful login; invalid email; wrong password; inactive user (no lockout)
- Token refresh (success, expired, revoked)
- Logout success
- Access with valid/expired/missing tokens

## Expected Deliverables (high level)
- Commands, handlers, results
- JWT service (IJwtTokenGenerator + implementation)
- Exceptions
- AuthController endpoints (login/refresh/logout)
- Domain events
- EF migration CreateRefreshTokensTable
- JwtSettings config
- Authentication setup in Program.cs
- Unit + integration tests (coverage ≥85%)

## Success Criteria
- Functional, technical, security, testing items all satisfied as described above

## Assumptions
- JWTs signed with a single symmetric key managed via environment/config
- Refresh tokens stored in DB for revocation/rotation
- CORS configured per environment; production enforces HTTPS and strict origins

## Dependencies & Risks
- Depends on Spec-001/Spec-002
- Risk: weak secret or misconfigured auth can compromise security; mitigation via checklists and tests

## Clarifications
### Session 2025-11-12
- Q: How should the refresh token be transmitted? → A: HttpOnly cookie (body fallback for non-browser)
- Q: Refresh token rotation policy? → A: Rotate on every refresh
- Q: Account lockout policy? → A: No lockout (monitor only)
- Q: Access token transport? → A: Support both header and cookie
- Q: JWT secret rotation cadence? → A: Rotate every 90 days with overlap (dual-key validation)
- Q: Frontend/API topology? → A: Separate subdomains (app.crm.com, api.crm.com)
