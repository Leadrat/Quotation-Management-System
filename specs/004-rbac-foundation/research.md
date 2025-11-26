# Research: Spec-004 Role Management & RBAC Foundation

Date: 2025-11-12

## Decisions

- Built-in roles immutable (no deactivate/delete/rename)
  - Rationale: Prevent privilege regression and policy drift
  - Alternatives: Emergency toggle with audit (rejected for risk)

- RoleName uniqueness: case-insensitive, trimmed; preserve casing for display
  - Rationale: Avoid duplicate semantics ("Admin" vs "admin") while keeping UX-friendly labels
  - Alternatives: Case-sensitive only (rejected), normalized stored value (defer)

- JWT carries both RoleName (`role`) and RoleId (`role_id`)
  - Rationale: Name supports attribute checks; Id supports data filtering/audit consistency
  - Alternatives: Only name or only id (rejected due to limitations)

- Block delete/deactivate when role is in use
  - Rationale: Avoid orphaned permissions and inconsistent access
  - Alternatives: Auto-reassign to fallback role (adds hidden side effects)

- Pagination for roles: default pageSize=10, max=100
  - Rationale: Balance usability and API performance
  - Alternatives: No max (rejected), different defaults (acceptable)

## Best Practices considered

- Use constants for role names and IDs to prevent typos and drift
- Keep authorization at controller layer with role attributes for clarity
- Surface user count in role listings to guide safe deactivation decisions
- Audit all role mutations with actor identity and timestamp

## Open Risks

- Future dynamic permissions: will require RolePermissions table and policy engine migration
- Cross-service propagation of role changes (out of scope for Phase 1)
