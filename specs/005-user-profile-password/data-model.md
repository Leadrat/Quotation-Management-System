# Data Model: Spec-005 User Profile & Password

## Entities

### User (existing)
- Id: UUID (PK)
- Email: citext, unique
- PasswordHash: string
- FirstName: varchar(100)
- LastName: varchar(100)
- Mobile: varchar(16) nullable (E.164)
- PhoneCode: varchar(8) nullable
- RoleId: UUID (FK Roles)
- IsActive: bool
- LoginAttempts: int (default 0)
- IsLockedOut: bool (derived flag or computed from attempts/lockout policy)
- CreatedAt, UpdatedAt: timestamptz

### PasswordResetToken (new)
- Id: UUID (PK)
- UserId: UUID (FK Users, cascade delete)
- TokenHash: bytea (HMAC-SHA-256)
- ExpiresAt: timestamptz (UTC)
- UsedAt: timestamptz nullable (UTC)
- CreatedAt: timestamptz (UTC)

Constraints and Indexes:
- IX_PasswordResetToken_User_Active: unique where UserId and UsedAt is null (enforce only one active token per user).
- IX_PasswordResetToken_ExpiresAt: nonclustered for cleanup sweeps.

## Validation Rules
- FirstName/LastName: ^[a-zA-Z\s\-']{2,100}$
- Mobile: ^\+[1-9]\d{1,14}$ (E.164)
- Password strength: min 8, upper, lower, digit, special.
- newPassword == confirmPassword.

## State Transitions
- Password change (self-service): verify current hash → set new hash → reset attempts → revoke refresh tokens → emit PasswordChanged.
- Admin reset: create token (HMAC hash) → email link → on consumption: set new hash → mark token UsedAt → reset attempts → revoke refresh tokens → emit PasswordReset.
- Lockout: after 5 failed current-password verifications during change, set account to locked and require admin unlock.

## Security Considerations
- Never persist raw reset token; only HMAC hash with server key.
- All timestamps in UTC; audit record includes actor and IP/UA when available.
