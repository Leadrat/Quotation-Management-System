# Data Model: Spec-004 Role Management & RBAC Foundation

Date: 2025-11-12

## Entities

### Role
- RoleId: Guid (PK)
- RoleName: string (3–100, unique CI, trimmed)
- Description: string? (≤500)
- IsActive: bool (default true)
- CreatedAt: DateTime (UTC)
- UpdatedAt: DateTime (UTC)

Constraints:
- Uniqueness: RoleName unique, case-insensitive and trimmed
- Built-ins (Admin, Manager, SalesRep, Client) are immutable (no delete/rename/deactivate)
- Delete/Deactivate blocked if any active users assigned (requires reassignment first)

Indexes:
- PK(RoleId)
- UX(RoleName_normalized)
- IX(IsActive)

### User (reference)
- RoleId: Guid (FK → Role.RoleId, required)
- ReportingManagerId rules per Spec-004 business rules

## Relationships
- Role 1..* Users

## Seed Data (built-in roles)
- Admin: AA668EE7-79E9-4AF3-B3ED-1A47F104B8EA
- Manager: 8D38F43B-EB54-4E4A-9582-1C611F7B5DF6
- SalesRep: FAE6CEDB-42FD-497B-85F6-F2B14ECA0079
- Client: 00F3CF90-C1A2-4B46-96A2-6A58EF54E8DD

## Validation Rules
- Create/Update RoleName: trim, 3–100, enforce CI uniqueness
- Update built-in: reject rename/deactivate/delete
- Delete custom role: only if IsActive=false and user count=0

## JWT Claims
- role: RoleName (string)
- role_id: RoleId (Guid)

## Pagination Defaults
- pageNumber default 1
- pageSize default 10, max 100
