# Spec-001: User Entity & DTO Specification

- Project: CRM Quotation Management System
- Group: User & Authentication (Group 1/5)
- Priority: CRITICAL (Foundation)
- Short Name: user-entity
- Feature Directory: specs/1-user-entity/
- Spec ID: Spec-001

## Objective
Define the complete User entity data model, database table structure, DTOs, validation rules, and request/response models for user management. This is the foundation for authentication and RBAC.

## Scope
- Includes:
  - User entity with all 16 columns
  - User DTOs for API responses
  - CreateUserRequest model for registration
  - UpdateUserRequest model for profile updates
  - FluentValidation rules for all inputs
  - Constants for validation (password strength, phone format, etc.)
- Excludes (separate specs):
  - Authentication/Login (Spec 3: UserAuthentication)
  - Role management (Spec 4: RoleManagement)
  - Password change flow (Spec 5: UserProfileManagement)

## Clarifications

### Session 2025-11-12
- Q: What is the canonical active-state source of truth?
  → A: Use DeletedAt as canonical; enforce IsActive=false when DeletedAt is set.
- Q: Should email uniqueness be case-insensitive at the DB level?
  → A: Yes, use Postgres citext with UNIQUE index; enable citext extension.
- Q: Which password hashing algorithm is standard?
  → A: BCrypt with work factor 12 (BCrypt.Net-Next).
- Q: How to handle ReportingManagerId on role changes?
  → A: When changing to SalesRep, ReportingManagerId is required; when changing from SalesRep, set ReportingManagerId = NULL.
- Q: Must PhoneCode match Mobile's country code?
  → A: Yes. If both provided, PhoneCode must equal the country code derived from Mobile (E.164).

## Actors & Roles
- Admin: create/update/deactivate users, assign roles, assign reporting manager
- Manager: may view team users, receives approval workflows
- SalesRep: standard user, must have a reporting manager
- Client: portal user, limited access

## Database Table Specification
- Table: Users (schema: public)
- Primary Key: UserId (UUID/GUID)
- Naming: TitleCase for columns

### Columns (creation order)
1. UserId — UUID, PK, NOT NULL
   - Description: Unique identifier for user
   - Generation: Guid.NewGuid()
   - Sample: 05948A48-3272-4FB4-8849-796A61D7A6F2

2. Email — CITEXT (Postgres), UNIQUE, NOT NULL
   - RFC 5322 compliant; case-insensitive via Postgres citext; <=255 chars
   - Requires `CREATE EXTENSION IF NOT EXISTS citext;` in migration
   - Sample: admin@crm.com

3. PasswordHash — VARCHAR(255), NOT NULL
   - Bcrypt/Argon2 hashed; never exposed in responses or logs

4. FirstName — VARCHAR(100), NOT NULL
   - 2–100 chars; letters/spaces/hyphens

5. LastName — VARCHAR(100), NOT NULL
   - 2–100 chars; letters/spaces/hyphens

6. Mobile — VARCHAR(20), NULLABLE
   - E.164 format; ^\+[1-9]\d{1,14}$; sample +919876543210

7. PhoneCode — VARCHAR(5), NULLABLE
   - Country code string; e.g., +91, +1, +44

8. IsActive — BOOLEAN, NOT NULL, DEFAULT true

9. RoleId — UUID, NOT NULL, FK -> Roles(RoleId)
   - Must exist in Roles table
   - Sample RoleIds: Admin AA668EE7-79E9-4AF3-B3ED-1A47F104B8EA; Manager 8D38F43B-EB54-4E4A-9582-1C611F7B5DF6; SalesRep FAE6CEDB-42FD-497B-85F6-F2B14ECA0079; Client 00F3CF90-C1A2-4B46-96A2-6A58EF54E8DD

10. ReportingManagerId — UUID, NULLABLE, FK -> Users(UserId)
    - Only for SalesRep; must reference a valid Manager user

11. LastLoginAt — TIMESTAMPTZ, NULLABLE

12. LoginAttempts — INT, NOT NULL, DEFAULT 0
    - Increment on failed login; reset on success; lock when >=5

13. IsLockedOut — BOOLEAN, NOT NULL, DEFAULT false

14. CreatedAt — TIMESTAMPTZ, NOT NULL (DEFAULT CURRENT_TIMESTAMP)

15. UpdatedAt — TIMESTAMPTZ, NOT NULL (DEFAULT CURRENT_TIMESTAMP)

16. DeletedAt — TIMESTAMPTZ, NULLABLE (soft delete)
   - Canonical active-state: DeletedAt IS NULL means active; when NOT NULL, record is inactive and IsActive must be false.

### Indexes
- UNIQUE(Email)
- INDEX(RoleId)
- INDEX(ReportingManagerId)
- INDEX(IsActive)
- INDEX(CreatedAt)
- INDEX(DeletedAt)

Active filter: Queries for active users MUST use `DeletedAt IS NULL`. `IsActive` is an administrative suspension flag but cannot be true when `DeletedAt` is set.

### Foreign Keys
- FK_Users_Roles: RoleId REFERENCES Roles(RoleId) ON DELETE RESTRICT
- FK_Users_ReportingManager: ReportingManagerId REFERENCES Users(UserId) ON DELETE SET NULL

## Entity Definition (C# class layout)
Namespace: CRM.Domain.Entities

Properties:
- Guid UserId
- string Email
- string PasswordHash
- string FirstName
- string LastName
- string Mobile (nullable)
- string PhoneCode (nullable)
- bool IsActive
- Guid RoleId
- Guid? ReportingManagerId
- DateTime? LastLoginAt
- int LoginAttempts
- bool IsLockedOut
- DateTime CreatedAt
- DateTime UpdatedAt
- DateTime? DeletedAt

Navigation:
- Role Role
- User ReportingManager
- ICollection<User> DirectReports

Notes:
- Attributes: Required, StringLength, EmailAddress as appropriate
- Constructor to init defaults and collections

## DTO Models

1) UserDto (Response)
- Includes: UserId, Email, FirstName, LastName, Mobile, PhoneCode, IsActive, RoleId, RoleName, ReportingManagerId, LastLoginAt, CreatedAt, UpdatedAt
- Excludes: PasswordHash, LoginAttempts, IsLockedOut, DeletedAt

2) CreateUserRequest (Request)
- Email (required, unique)
- Password (required, strong)
- FirstName (2–100)
- LastName (2–100)
- Mobile (optional, E.164)
- PhoneCode (optional)
- RoleId (required, must exist)
- ReportingManagerId (optional; required if RoleId=SalesRep)

3) UpdateUserRequest (Request)
- FirstName (optional, 2–100)
- LastName (optional, 2–100)
- Mobile (optional, E.164)
- PhoneCode (optional)
- Immutable via other endpoints: Email, PasswordHash, RoleId, ReportingManagerId, IsActive

## Validation Rules (FluentValidation)

User Entity:
- Email: NotEmpty, MaxLength(255), EmailAddress, Unique
- PasswordHash: NotEmpty, MinLength(60) (bcrypt)
- FirstName/LastName: NotEmpty, 2–100, Matches ^[a-zA-Z\s\-']+$
- Mobile: Matches ^\+[1-9]\d{1,14}$ if provided
- PhoneCode: Matches ^\+\d{1,3}$ if provided
- PhoneCode & Mobile consistency: If Mobile provided, derive country code from Mobile; if PhoneCode also provided, it MUST equal the derived code.
- RoleId: NotEmpty, RoleExists(roleId)
- ReportingManagerId: ValidateReportingManager(); required iff SalesRep; null otherwise
- IsActive: bool; if DeletedAt != null then IsActive must be false

Role change semantics (enforced at command/handler level):
- If RoleId changes to SalesRep → require non-null ReportingManagerId referencing a Manager.
- If RoleId changes from SalesRep to any other role → set ReportingManagerId = NULL.

CreateUserRequest (Password rules):
- NotEmpty; MinLength(8)
- Contains: uppercase, lowercase, digit, special
- NotEqual Email/FirstName

UpdateUserRequest:
- If Mobile provided, must match E.164
 - If both Mobile and PhoneCode provided, PhoneCode must match country code derived from Mobile

## Constants & Helpers

ValidationConstants:
- MinPasswordLength=8; MaxPasswordLength=128; MinNameLength=2; MaxNameLength=100; MaxEmailLength=255; MaxMobileLength=20
- PasswordRegex = ^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*(),.?":{}|<>])[a-zA-Z0-9!@#$%^&*(),.?":{}|<>]{8,128}$
- MobileRegex = ^\+[1-9]\d{1,14}$
- PhoneCodeRegex = ^\+\d{1,3}$
- NameRegex = ^[a-zA-Z\s\-']{2,100}$
- MaxLoginAttempts = 5

PasswordHelper:
- HashPassword(password) using BCrypt cost 12 (standardized)
- VerifyPassword(password, hash)

## Demo Data (Seed)
- Admin: 05948A48-3272-4FB4-8849-796A61D7A6F2, admin@crm.com, +919876543200, +91, Role Admin
- Manager: EB4F2FCA-B9F6-46CE-BB6F-2EA0689ABE9F, manager@crm.com, +919876543201, +91, Role Manager
- Sales Rep: 67B8A7EA-F0D7-46CB-8B9E-F3B2E5EDF336, sales@crm.com, +919876543202, +91, Role SalesRep, ReportingManagerId Manager
- Client: 84762D26-BC7F-4133-AA40-0D15D8F21B84, client1@crm.com, +919876543203, +91, Role Client

## Dependencies & References
NuGet:
- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.Npgsql
- BCrypt.Net-Next
- FluentValidation

Related Entities:
- Roles (Spec 4); Relationship: User.RoleId -> Roles.RoleId

Domain Events (optional):
- UserCreated, UserUpdated, UserDeactivated

## Expected Outputs (Artifacts)
- /src/Backend/CRM.Domain/Entities/User.cs
- /src/Backend/CRM.Shared/DTOs/UserDto.cs
- /src/Backend/CRM.Shared/DTOs/CreateUserRequest.cs
- /src/Backend/CRM.Shared/DTOs/UpdateUserRequest.cs
- /src/Backend/CRM.Shared/Validators/UserValidator.cs
- /src/Backend/CRM.Shared/Validators/CreateUserRequestValidator.cs
- /src/Backend/CRM.Shared/Validators/UpdateUserRequestValidator.cs
- /src/Backend/CRM.Shared/Helpers/PasswordHelper.cs
- /src/Backend/CRM.Shared/Constants/ValidationConstants.cs

## Database Migration
- Migration: CreateUsersTable.cs
- Enable Postgres extension: `CREATE EXTENSION IF NOT EXISTS citext;`
- Use `citext` type for `Email` column with UNIQUE index (case-insensitive uniqueness)
- Seed: Insert 4 demo users with GUIDs above and role references

## Unit Tests
- UserValidatorTests.cs
- PasswordHelperTests.cs
- UserEntityTests.cs

## Documentation
- User.md (entity documentation)
- UserDTO.md (DTO definitions)

## Acceptance Criteria
- User entity created with 16 columns (GUID PK, TitleCase)
- DTOs implemented (UserDto, CreateUserRequest, UpdateUserRequest)
- FluentValidation rules for email, password strength, phone format, etc.
- Password hashing helper using bcrypt (cost 12)
- 4 demo users seeded with real GUIDs (Admin, Manager, SalesRep, Client)
- EF Core migration created and tested
- Indexes created on Email, RoleId, ReportingManagerId, IsActive
- Foreign key constraints configured
- Navigation properties working (Role, ReportingManager)
- Soft delete implemented (DeletedAt)
- Unit tests for validators and password helper (>=80% coverage)
- Naming conventions enforced (TitleCase properties/columns)
- No hardcoded strings (use constants)

## User Scenarios & Testing
- Admin creates a SalesRep with ReportingManagerId set to Manager user → succeeds; email unique enforced
- Admin updates user profile (names, mobile, phone code) → validation enforced
- Attempt to create user with duplicate email → rejected with clear message
- Attempt to create SalesRep without ReportingManagerId → rejected
- Change role from Client to SalesRep without ReportingManagerId → rejected; with valid Manager → succeeds
- Change role from SalesRep to Manager → ReportingManagerId automatically cleared (NULL)
- Lockout behavior: simulate 5 failed logins → IsLockedOut true, LoginAttempts >= 5
- Soft delete: set DeletedAt; system forces IsActive=false; verify filters use DeletedAt IS NULL
- Seed verification: all 4 demo users present with correct roles
 - Create/Update with Mobile + mismatched PhoneCode → rejected; matching PhoneCode → accepted

## Assumptions
- Roles table contains the four roles with specified GUIDs
- Email uniqueness is case-insensitive via Postgres `citext` type and UNIQUE index
- Timestamps stored in TIMESTAMPTZ (UTC)
 - Password hashing standardized on BCrypt (cost 12)

## Success Criteria
- Data model supports all required auth and RBAC flows
- Validations prevent invalid or insecure data states
- Performance: indexed lookups on Email/RoleId/ReportingManagerId
- Test coverage commitments achieved

## Dependencies & Risks
- Dependency on Roles spec for role GUIDs
- Risk: strict regex may reject some valid international names/mobiles → review in QA

## Next Specs
- Spec 2: UserRegistration (uses User entity)
- Spec 3: UserAuthentication
- Spec 4: RoleManagement
