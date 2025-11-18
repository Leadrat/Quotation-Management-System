# Spec-002: User Registration & Account Creation

Feature: User Registration & Account Creation (Spec-002)
Group: User & Authentication (Group 1 of 11)
Priority: CRITICAL (Foundation - Phase 1)
Dependencies: Spec-001 (User Entity)
Related: Spec-003 (UserAuthentication), Spec-004 (RoleManagement), Spec-005 (UserProfileManagement)

## Overview
Defines the end-to-end flows to create new user accounts:
- Client self-registration (public sign-up)
- Admin-controlled user creation (internal users)

Both flows validate email uniqueness (case-insensitive), password strength, and role constraints. Passwords are stored only as bcrypt hashes.

## Clarifications

### Session 2025-11-12
- Q: Should client email be verified? → A: Require email verification link before first login.
- Q: Should public registration require captcha? → A: No captcha; rely on rate limiting only.
- Q: Enforce disposable-email domain checks now or defer? → A: Allow all domains (no checks) in Spec-002.

## Actors & JTBD
- Admin / Sales Manager: Adds internal users quickly with correct role/manager.
- Client: Signs up with minimal fields to access quotations.

Success metric: New internal user can log in the same day; admin completes creation in <5 minutes.

## Scopes & Flows

### Flow 1: Client Self-Registration (Public)
- Endpoint: POST /api/v1/auth/register (public)
- Role assigned: Client
- Input: Email, Password, FirstName, LastName; optional Mobile, PhoneCode
- Validation:
  - Email format (RFC 5322) and uniqueness (case-insensitive)
  - Password strength (≥8, upper, lower, digit, special)
  - Names: 2–100 chars, letters/spaces/hyphens
- Processing:
  - Hash password via bcrypt (cost=12)
  - Create User with RoleId=Client, ReportingManagerId=NULL
  - Persist to DB
  - Emit UserCreated event
  - Send verification email with time-bound token
  - Login requires verified email; unverified accounts cannot authenticate
- Output (201): success message with redirect to /login

### Flow 2: Admin User Creation (Internal)
- Endpoint: POST /api/v1/users (Admin-only)
- Authorization: Requires Admin role
- Input: Email, Password, FirstName, LastName, RoleId; optional Mobile, PhoneCode, ReportingManagerId
- Validation:
  - Email format + uniqueness (case-insensitive)
  - Password strength if provided
  - RoleId exists and is one of: Admin, Manager, SalesRep
  - If RoleId=SalesRep: ReportingManagerId is required, must be active user with role=Manager
- Processing:
  - Hash password via bcrypt (cost=12)
  - Create User with specified RoleId and ReportingManagerId
  - Persist to DB
  - Emit AdminUserCreated event (includes temp password metadata)
  - Enqueue welcome email with credentials/reset guidance
- Output (201): success with created user summary and emailSent flag

## Functional Requirements
1. Email uniqueness enforced against Users (including soft-deleted), case-insensitive.
2. Password strength: at least 1 uppercase, 1 lowercase, 1 digit, 1 special, length ≥8.
3. Client flow must always assign Role=Client; Admin flow cannot assign Client.
4. SalesRep must have a valid, active Manager as ReportingManagerId.
5. Passwords are hashed with bcrypt cost 12; never returned or logged.
6. On success, emit domain events:
   - UserCreated (for client flow)
   - AdminUserCreated (for admin flow)
7. Welcome emails are sent asynchronously using templates:
   - Client: generic welcome and login link
   - Admin-created: include temporary password instructions and reset link
8. Public endpoint rate limiting: ≤5 registrations per IP per hour.
9. Audit all attempts (success/failure) with timestamp, IP, actor (admin if applicable).
10. Client email verification required before first login; system issues verification token and validates it prior to authentication.
11. Public client registration does not require captcha in Spec-002; bot mitigation relies on IP rate limiting (≤5/hour) and server-side validation.
12. Disposable email domains are permitted in Spec-002; no denylist enforcement.

## Non-Functional & Constraints
- Case-insensitive email matching (citext in DB already provisioned by Spec-001).
- Async I/O for email sending; user creation path remains responsive.
- Avoid N+1 queries (use eager loading as needed for role/manager validation).

## Validation & Security
- Input validation using existing validators from Spec-001 where applicable.
- Sanitize inputs: trim whitespace; strip HTML from names; email to lowercase for checks.
- Authorization enforced for Admin endpoint (role=Admin).
- Prevent privilege escalation: users cannot self-assign Admin.

## Domain Events
- UserCreated: UserId, Email, FirstName, LastName, RoleId, RoleName, CreatedAt, CreatedBy(optional)
- AdminUserCreated: extends UserCreated with ReportingManagerId/Name and TemporaryPassword metadata

## API Contracts

### POST /api/v1/auth/register (public)
Request:
{
  "email": "client1@company.com",
  "password": "SecurePass@123",
  "firstName": "John",
  "lastName": "Doe",
  "mobile": "+91...", // optional
  "phoneCode": "+91"   // optional
}

Responses:
- 201 Created: { success, message, userId, email, redirectUrl }
- 400 Bad Request: validation errors
- 409 Conflict: duplicate email

### POST /api/v1/users (Admin-only)
Headers: Authorization: Bearer <token>
Request: { email, password, firstName, lastName, mobile?, phoneCode?, roleId, reportingManagerId? }

Responses:
- 201 Created: { success, message, user, emailSent, temporaryPasswordExpiry }
- 400/401/403/409/422 appropriate to validation/authorization failures

## Success Criteria
- Client registration completes < 2s p95 (excluding email sending).
- Admin creation completes < 2s p95.
- Duplicate email requests return 409 consistently.
- Emails delivered for ≥99% of successful creations (asynchronously).
- Rate limit enforced for public endpoint.

## Assumptions
- Client email verification is required before first login; verification transport and token TTL will be aligned with Spec-003.
- Email delivery uses an abstracted service; provider choice is out of scope.
- Temporary passwords (admin flow) expire in 24 hours.

## Out of Scope
- Login/JWT (Spec-003), role CRUD (Spec-004), profile edits (Spec-005).

## Acceptance Tests (high level)
- Client registers successfully → User with Role=Client exists and IsActive=true.
- Duplicate client email → 409 Conflict.
- Weak password → 400 with validation error.
- Admin creates SalesRep with valid Manager → 201; user has SalesRep role with manager.
- Admin creates SalesRep without Manager → 422 Unprocessable.
- Non-admin calls POST /users → 403 Forbidden.

## [NEEDS CLARIFICATION]
None for this session.
