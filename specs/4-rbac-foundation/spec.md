---
feature: Spec-004 Role Management & RBAC Foundation
project: CRM Quotation Management System
priority: CRITICAL
owner_group: User & Authentication (Group 1 of 11)
dependencies:
  - Spec-001 User Entity
  - Spec-003 User Authentication (JWT)
related_specs:
  - Spec-002 User Registration
  - Spec-005 User Profile Management
status: Draft v1.0
---

# Overview
This specification establishes the Role-Based Access Control (RBAC) foundation for the system. It defines four built-in roles (Admin, Manager, SalesRep, Client), their responsibilities, and authorization rules. Admins can manage roles (CRUD) for future extensibility, while Phase 1 enforces authorization via role claims in JWT and controller attributes.

# Goals & Business Value
- Ensure only authorized users can access sensitive features and data.
- Provide clear, auditable separation of duties across Admin, Manager, SalesRep, Client.
- Form a durable foundation that future features can build upon without redesign.

# Scope
In-scope (Phase 1):
- Built-in roles and role constants
- Role CRUD endpoints (Admin-only)
- Authorization via `[Authorize(Roles="...")]`
- Role seed data on startup
- Audit trail of role operations

Out-of-scope (future phases):
- Dynamic permissions (RolePermissions table)
- Custom authorization policies beyond simple role lists

# Actors
- Admin (primary)
- Manager
- SalesRep
- Client
- System (backend services)

# Assumptions
- Users have exactly one role at a time (RoleId required).
- JWT contains role claim for fast authorization checks.
- Built-in roles are immutable (cannot be renamed or deleted).

# Role Definitions (Built-in)
- Admin: Full access; system configuration; audit; templates; user and role management.
- Manager: Team-scoped approvals and data; cannot manage users, roles, or system settings.
- SalesRep: Create/manage own quotations and clients; submit approvals.
- Client: External quotation viewer via portal; no API access.

# Role Hierarchy & Business Rules
1) Every user must have exactly one role.
2) Manager can approve only for direct reports (ReportingManagerId).
3) SalesRep must have a valid Manager as ReportingManagerId.
4) Managers do not report to other managers (ReportingManagerId null or Admin).
5) Admin has no ReportingManagerId.
6) Client has no ReportingManagerId.
7) Built-in roles cannot be deleted, renamed, or deactivated (IsActive toggle not allowed). Only custom roles may be deactivated.

# Non-Functional Requirements
- Authorization checks enforced at controller layer with role claims.
- All role operations write audit events (who, what, when).
- Seed four roles at initialization with fixed GUIDs.

# Functional Requirements
FR1. System seeds four built-in roles with specified IDs and names.
FR2. Admin can list roles with user counts and active flag.
FR3. Admin can create custom roles with unique name and description.
FR4. Admin can update custom roles (rename, description, IsActive) but not built-ins.
FR5. Admin can soft-delete custom roles not in use; built-ins cannot be deleted.
FR6. All protected endpoints use `[Authorize(Roles="...")]` consistent with RoleConstants.
FR7. All role operations are audited with actor ID and timestamp.
FR8. RoleName uniqueness is case-insensitive and whitespace-trimmed; comparisons use normalized value while display preserves original casing.
FR9. JWT includes both RoleName and RoleId claims; RoleName used for authorization attributes, RoleId available for data filtering/auditing.
FR10. Role delete or deactivation is blocked if any active users are assigned to the role; reassignment of those users is required first.
FR11. Roles listing pagination: default pageNumber=1, default pageSize=10, maximum pageSize=100.

# Success Criteria
- Admin retrieves all 4 built-in roles from API within 500 ms.
- Creating a custom role returns 201 and persists; duplicate name returns 409.
- Updating built-in role returns 400; deleting built-in returns 400.
- Non-admin access to role endpoints returns 403.
- JWT for logged-in users includes RoleName and RoleId claims.

# Key Entities
Entity: Role
- RoleId (Guid, PK)
- RoleName (string, unique, 3–100)
- Description (string?, ≤500)
- IsActive (bool, default true)
- CreatedAt (DateTime)
- UpdatedAt (DateTime)
Navigation: Users (ICollection<User>)
Method: GetDisplayName() => RoleName

# DTOs
RoleDto (response)
- RoleId, RoleName, Description, IsActive, UserCount, CreatedAt, UpdatedAt

CreateRoleRequest (request)
- RoleName (required, unique, 3–100)
- Description (optional, ≤500)

UpdateRoleRequest (request)
- RoleId (path)
- RoleName? (unique)
- Description?
- IsActive?

Validation rules:
- RoleName uniqueness is case-insensitive and trimmed; normalized for comparison, original casing preserved for display.

# Queries & Commands
Queries (Admin-only):
- GetAllRolesQuery(isActive?, pageNumber?, pageSize?) → List<RoleDto>
- GetRoleByIdQuery(RoleId) → RoleDto?
- GetRoleByNameQuery(RoleName) → RoleDto?
- GetUserCountByRoleQuery(RoleId) → int

Commands (Admin-only):
- CreateRoleCommand(RoleName, Description?) → RoleDto
  - Errors: DuplicateRoleNameException
- UpdateRoleCommand(RoleId, RoleName?, Description?, IsActive?) → RoleDto
  - Errors: RoleNotFoundException, CannotModifyBuiltInRoleException, DuplicateRoleNameException
- DeleteRoleCommand(RoleId) → Success
  - Soft delete (IsActive=false)
  - Errors: RoleNotFoundException, CannotDeleteRoleInUseException, CannotModifyBuiltInRoleException

# API Endpoints (Admin-only)
- GET /api/v1/roles
  - Query: `isActive?` (bool), `pageNumber?` (int, default 1), `pageSize?` (int, default 10, max 100)
- GET /api/v1/roles/{roleId}
- POST /api/v1/roles
- PUT /api/v1/roles/{roleId}
- DELETE /api/v1/roles/{roleId}
All require Authentication + `[Authorize(Roles="Admin")]`.

# Authorization Implementation
- Use `RoleConstants` for names and IDs.
- Controllers decorate endpoints with `[Authorize(Roles="Admin")]`.
- Future: policies like `ManagerOrAdmin` as needed.
- JWT carries two claims: `role` (RoleName) for attribute checks and `role_id` (RoleId) for downstream filtering/audit.

# Exception Mapping
- RoleNotFoundException → 404
- DuplicateRoleNameException → 409
- CannotModifyBuiltInRoleException → 400
- CannotDeleteRoleInUseException → 400
- InvalidRoleException → 422

# Domain Events (for audit)
- RoleCreated(RoleId, RoleName, Description, CreatedAt, CreatedByUserId)
- RoleUpdated(RoleId, RoleName, OldName, UpdatedAt, UpdatedByUserId)
- RoleDeleted(RoleId, RoleName, DeletedAt, DeletedByUserId)
Handlers: Audit logging.

# User Scenarios & Testing
- Admin lists all roles and sees the four built-ins plus any custom roles.
- Admin creates a custom role; duplicate name rejected with 409.
- Admin updates a custom role; attempts to update built-in rejected with 400.
- Admin deletes a custom role with no active users; attempts to delete built-in or role-in-use rejected with 400.
- Manager attempts to access roles API → 403.

# Clarifications
### Session 2025-11-12
- Q: Should role updates (IsActive toggle) be allowed for built-in roles in emergencies?
  → A: Disallow (built-in roles fully immutable, no deactivation)
- Q: RoleName uniqueness semantics (case sensitivity/whitespace)?
  → A: Case-insensitive, trimmed uniqueness; normalize for comparison, preserve casing for display
- Q: Which role claims are embedded in JWT?
  → A: Both RoleName (`role`) and RoleId (`role_id`)
- Q: Can roles be deleted/deactivated while assigned to active users?
  → A: No; block both operations until all assigned users are reassigned
- Q: Pagination defaults and max limits for roles listing?
  → A: Default pageSize=10 (max 100); default pageNumber=1

# Acceptance Criteria
- All Functional Requirements met and verifiable via API.
- Built-in roles seeded; JWT includes role claim; role endpoints protected by Admin role.
- Unit tests cover queries, commands, and authorization checks; integration tests verify endpoints.

# Delivery
- Spec file (this) and quality checklist.
- Planning artifacts to follow in subsequent workflow.
