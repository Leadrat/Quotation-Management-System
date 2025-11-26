---
feature: Spec-005 User Profile Management & Password Management
project: CRM Quotation Management System
priority: HIGH (Phase 1, after Specs 1–4)
owner_group: User & Authentication (Group 1 of 11)
dependencies:
  - Spec-001 User Entity
  - Spec-003 User Authentication (JWT)
  - Spec-004 Role Management & RBAC
related_specs:
  - Spec-002 User Registration
status: Draft v1.0
---

# Overview
This specification covers user self-service profile updates and secure password management. Users can update personal details (first/last name, mobile, phone code) and change passwords (with current password verification and strength checks). Admins can reset user passwords when required. All actions are authorized, audited, and notify users via email.

# Goals & Business Value
- Keep user contact information current without admin intervention.
- Improve account security via strong password policies and verified changes.
- Reduce support load through self-service.
- Provide a compliant audit trail and proactive notifications for security events.

# Scope
In-scope (MVP - Phase 1):
- Self-service profile update for authenticated users.
- Self-service password change with current password verification and strength requirements.
- Admin-initiated password reset with temporary password and force-change requirement.
- Email notifications and audit logging for all profile/password changes.

Out-of-scope (future phases):
- Forgotten password email flows and reset links.
- 2FA and password rotation policies.
- Session management UI (view/logout all sessions).

# Actors
- Authenticated User (SalesRep, Manager, Admin, Client* if applicable)
- Admin
- System (email service, audit logger)

# Assumptions
- Email and Role are immutable through self-service endpoints.
- JWT contains user identity and role claims (Spec-003/004).
- Passwords are stored hashed (bcrypt); no plaintext storage or logs.
 - Emails are delivered via a transactional email provider asynchronously through a background queue (e.g., SES/SendGrid).

# User Flows
## Profile Update (Self-Service)
- Endpoint: PUT /api/v1/users/{userId}/profile (Auth required)
- Authorization: User can update own profile; Admin can update any user.
- Validates names (2–100, letters/space/hyphen/apostrophe) and mobile (E.164) if provided.
- Updates User fields and timestamps; emits UserProfileUpdated; sends confirmation email.

## Password Change (Self-Service)
- Endpoint: POST /api/v1/auth/change-password (Auth required)
- Authorization: User can change own password only.
- Verifies current password; enforces strength; prevents reuse; updates hash and timestamps.
- Resets login attempts/lockout; emits PasswordChanged; sends confirmation email; invalidates refresh tokens (logout everywhere).

## Admin Password Reset
- Endpoint: POST /api/v1/users/{userId}/reset-password (Admin only)
- Sends a one-time password reset link via email; user must set a new password on first login; link expires in 24h; no temporary password is displayed to admin or stored.
- On reset completion, updates password hash; resets attempts/lockout; emits PasswordReset; emails user confirmation.

# Functional Requirements
FR1. Authenticated users can update profile fields: FirstName, LastName, Mobile, PhoneCode.
FR2. Email, RoleId, ReportingManagerId are immutable for self-service.
FR3. Admins can update any user's profile with same validation rules.
FR4. Self-service password change requires current password verification.
FR5. New password must meet strength: ≥8 chars, ≥1 uppercase, ≥1 lowercase, ≥1 digit, ≥1 special.
FR6. New password cannot match current password.
FR7. After password change, all refresh tokens are invalidated; user must re‑login.
FR8. Admin can trigger a one-time password reset link emailed to the user; link expires in 24 hours and requires setting a new password (no temporary password exposure).
FR8a. Reset link is single-use and invalid after first successful password set or expiry.
FR8b. Reset token is stored server-side (DB), single-use, 24h expiry, and invalidated on use or re-issue.
FR8c. Issuing a new reset invalidates any previous active reset tokens for that user.
FR9. All operations write audit events and send email notifications.
FR10. Global per-account lockout after 5 failed current password verifications during change; admin can unlock.
FR11. Profile/password change update UpdatedAt; return updated minimal user info.

# Success Criteria
- Profile update completes < 500 ms on average and returns updated data.
- Password change validates and succeeds with proper notifications; weak or reused passwords rejected.
- Admin password reset sends a one-time reset link via email; link expires in 24h; user must set a new password.
- All related actions appear in audit logs with actor, timestamp, IP, and user agent (where available).

# Data Model (Impacts)
- User.PasswordHash (existing)
- User.LoginAttempts, User.IsLockedOut (existing)
- User.UpdatedAt (existing updates)
- Optional flag for temp password enforcement (not persisted for MVP; behavior driven by policy/event handlers)
 - PasswordResetToken (new): { Id (GUID), UserId (FK), TokenHash (HMAC-SHA256), ExpiresAt (UTC), UsedAt (UTC? null), CreatedAt (UTC) }

# API Endpoints
- PUT /api/v1/users/{userId}/profile (Auth; self or Admin)
- POST /api/v1/auth/change-password (Auth; self)
- POST /api/v1/users/{userId}/reset-password (Admin)

# Validation Rules
- Names: ^[a-zA-Z\s\-']{2,100}$
- Mobile: ^\+[1-9]\d{1,14}$ (E.164)
- Password strength: must include uppercase, lowercase, digit, special; min length 8.
- Confirmation: newPassword == confirmPassword.

# Exceptions (mapped to HTTP status)
- InvalidCurrentPassword → 401
- WeakPassword → 400
- PasswordMismatch → 400
- PasswordReuse → 400
- AccountLocked → 403
- UserNotFound → 404
- Unauthorized → 403
- Validation errors → 400/422 with field messages

# Domain Events
- UserProfileUpdated: { UserId, Email, FirstName, LastName, UpdatedAt, UpdatedByUserId, Changes }
- PasswordChanged: { UserId, Email, ChangedAt, ChangedByUserId, IPAddress, UserAgent }
- PasswordReset: { UserId, Email, ResetByAdminId, ResetAt, TemporaryPasswordExpiry }

# Security & Audit
- Always use bcrypt for verification/storage; never log secrets.
- Use generic error messages for authentication failures.
- Lock out after 5 wrong current-password attempts during change.
- Invalidate all refresh tokens on password change.
- Audit all actions with actor, target, timestamp, IP, user agent (if available).
- Email notifications are enqueued to a background worker and sent via a transactional email provider API to avoid blocking API requests.
- Temporary passwords are never displayed or stored; admin-initiated reset uses a one-time link only.
- Lockout scope is global per-account (not session-scoped) to prevent distributed guessing.
- BCrypt work factor: 12.
 - Reset token generation: 32-byte cryptographic random value; only a keyed HMAC/SHA-256 hash is persisted. Token is single-use, 24h TTL, invalidated on consumption or re-issue; previous tokens are revoked.

# Success Measurement
- Reduction in profile support tickets by >50% in first month.
- 0 critical security findings in review of password flows.
- Email notification delivery rate ≥ 99%.

# Clarifications
- [RESOLVED 2025-11-13] Email provider and delivery: Transactional email API via background queue (e.g., SES/SendGrid).
- [RESOLVED 2025-11-13] Temporary password delivery: Email one-time reset link; no temporary password shown.
- [RESOLVED 2025-11-13] Lockout counter scope: Global per-account.
- [RESOLVED 2025-11-13] Password hashing work factor: BCrypt cost 12.
 - [RESOLVED 2025-11-13] Reset token mechanism: DB-backed, random value with HMAC/SHA-256 hash, single-use, 24h expiry, revoke on re-issue.

### Session 2025-11-13
- Q: What is the email delivery approach (provider and sync vs async)? → A: Transactional email API via background queue (e.g., SES/SendGrid), asynchronous.
- Q: How should admin-initiated reset deliver credentials? → A: Email one-time reset link; user sets new password; no temp password exposure; 24h expiry.
- Q: Lockout counter scope? → A: Global per-account (not session-scoped).
- Q: What hashing work factor to use? → A: BCrypt cost 12.
 - Q: Which reset token mechanism to use? → A: DB-backed single-use token; random 32 bytes; HMAC/SHA-256 persisted; 24h expiry; revoke on re-issue.

# Acceptance Criteria
- Functional Requirements FR1–FR11 demonstrably met via API.
- Endpoints protected by correct authorization rules; non-admin blocked appropriately.
- Emails sent on profile update and password change/reset events.
- Admin password reset sends a one-time reset link via email; link expires in 24h; user must set a new password.

# Delivery
- Spec file (this) and quality checklist.
- Planning and tasks artifacts to follow in subsequent workflows.
