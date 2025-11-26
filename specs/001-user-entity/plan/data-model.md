# Data Model: User (Spec-001)

## Entities

### User
- UserId (UUID, PK)
- Email (citext, UNIQUE, NOT NULL)
- PasswordHash (varchar(255), NOT NULL)
- FirstName (varchar(100), NOT NULL)
- LastName (varchar(100), NOT NULL)
- Mobile (varchar(20), NULL)
- PhoneCode (varchar(5), NULL)
- IsActive (bool, NOT NULL, default true)
- RoleId (UUID, NOT NULL, FK -> Roles.RoleId)
- ReportingManagerId (UUID, NULL, FK -> Users.UserId)
- LastLoginAt (timestamptz, NULL)
- LoginAttempts (int, NOT NULL, default 0)
- IsLockedOut (bool, NOT NULL, default false)
- CreatedAt (timestamptz, NOT NULL)
- UpdatedAt (timestamptz, NOT NULL)
- DeletedAt (timestamptz, NULL)

## Relationships
- User.RoleId → Roles.RoleId (many users to one role)
- User.ReportingManagerId → Users.UserId (self-reference: manager to many direct reports)

## Constraints
- Email case-insensitive uniqueness via citext
- DeletedAt canonical active-state; IsActive must be false when DeletedAt is set
- SalesRep must have ReportingManagerId referencing a Manager
- PhoneCode must match country code derived from Mobile when both provided

## Indexes
- PK(UserId)
- UNIQUE(Email)
- IDX(RoleId)
- IDX(ReportingManagerId)
- IDX(IsActive)
- IDX(CreatedAt)
- IDX(UpdatedAt)
- IDX(DeletedAt)

## State
- Active: DeletedAt IS NULL
- Inactive (soft-deleted): DeletedAt NOT NULL (IsActive forced false)
- Locked: IsLockedOut true

## Seed
- 4 demo users with GUIDs and roles as specified
