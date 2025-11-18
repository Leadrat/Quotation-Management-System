# Data Model: Spec-003 User Authentication & JWT

Created: 2025-11-12

## Entities

### User (existing from Spec-001)
- UserId (Guid, PK)
- Email (citext, unique)
- PasswordHash (string, 255)
- FirstName (string, 100)
- LastName (string, 100)
- IsActive (bool)
- LoginAttempts (int) // monitored only, no lockout
- LastLoginAt (timestamp?)
- IsLockedOut (bool) // unused for lockout in Spec-003
- CreatedAt, UpdatedAt, DeletedAt

### RefreshToken (new)
- RefreshTokenId (Guid, PK)
- UserId (Guid, FK → User)
- TokenJti (string, 255 unique)
- IsRevoked (bool, default false)
- RevokedAt (timestamp, null)
- ExpiresAt (timestamp)
- CreatedAt (timestamp)
- LastUsedAt (timestamp, null)

Constraints:
- UNIQUE(TokenJti)
- INDEX(UserId), INDEX(ExpiresAt), INDEX(IsRevoked)
- ExpiresAt > CreatedAt

## Relationships
- User 1..* RefreshToken (active tokens per device/session)

## Validation Rules
- On insert: ExpiresAt = now + 30d
- On revoke: set IsRevoked=true and RevokedAt=now
- On rotate: revoke old; insert new with fresh JTI and ExpiresAt

## State Transitions
- RefreshToken: Active → (Revoked | Expired)

## Notes
- Access token is stateless; not stored
- Refresh token transmitted via HttpOnly cookie (SameSite=None; Secure) with JSON fallback for non-browser clients
