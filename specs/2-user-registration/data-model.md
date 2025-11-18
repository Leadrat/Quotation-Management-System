# Data Model: Spec-002 User Registration & Account Creation

Created: 2025-11-12

## Entities

### User (from Spec-001; used here)
- UserId (Guid, PK)
- Email (citext, unique)
- PasswordHash (string, 255)
- FirstName (string, 100)
- LastName (string, 100)
- Mobile (string?, 20)
- PhoneCode (string?, 5)
- IsActive (bool)
- RoleId (Guid, FK → Role)
- ReportingManagerId (Guid?, FK → User)
- LastLoginAt (DateTime?)
- LoginAttempts (int)
- IsLockedOut (bool)
- CreatedAt (DateTime)
- UpdatedAt (DateTime)
- DeletedAt (DateTime?)

### Role (from Spec-001)
- RoleId (Guid, PK)
- RoleName (string, 100 unique)
- Description (string?, 500)

### EmailVerificationToken (new)
- TokenId (Guid, PK)
- UserId (Guid, FK → User)
- TokenHash (string, 255)  // store a hash of the token
- ExpiresAt (DateTime, UTC)
- ConsumedAt (DateTime?, UTC)
- CreatedAt (DateTime, UTC)
- CreatedBy (string) // system or actor email

Constraints:
- Unique active token per user (ConsumedAt IS NULL AND ExpiresAt > now())
- Token length (raw) ≥ 32 bytes before hashing

## Relationships
- User 1..* EmailVerificationToken (time-bound, at most one active)
- User (SalesRep) → User (Manager) via ReportingManagerId (if Role=SalesRep)

## Validation Rules
- Email unique (case-insensitive) and RFC 5322 format
- Password: ≥8, at least 1 upper, 1 lower, 1 digit, 1 special
- Names: 2–100, letters/spaces/hyphens
- SalesRep requires ReportingManagerId; manager must have Role=Manager and IsActive=true
- Client registration → Role=Client; Admin creation → Role in {Admin, Manager, SalesRep}
- Email verification required for client before first login

## State Transitions
- EmailVerificationToken: New → (Consumed | Expired)
- User: New → Active (client flow pending verification; login allowed only after verification)

## Notes
- Disposable domains allowed (no denylist in Spec-002)
- No captcha; rely on rate-limiting ≤5/hour per IP
