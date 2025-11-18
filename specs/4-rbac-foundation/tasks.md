# Tasks: Spec-004 Role Management & RBAC Foundation

Feature: Role Management & RBAC Foundation (Spec-004)
Branch: rbac-foundation

## Phase 1: Setup

- [X] T001 Ensure Role constants exist in src/Backend/CRM.Shared/Constants/RoleConstants.cs
- [X] T002 Wire JWT to include role and role_id in src/Backend/CRM.Infrastructure/Auth/JwtTokenGenerator.cs
- [X] T003 Update Program.cs auth policies placeholder (if any) in src/Backend/CRM.Api/Program.cs

## Phase 2: Foundational

- [X] T004 Create Role entity (validate existing) in src/Backend/CRM.Domain/Entities/Role.cs
- [X] T005 [P] Create EF configuration for Role in src/Backend/CRM.Infrastructure/EntityConfigurations/RoleEntityConfiguration.cs
- [X] T006 Add DbSet<Role> and apply configuration in src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs
- [X] T007 [P] Create migration CreateRolesTable + Seed roles in src/Backend/CRM.Infrastructure/Migrations/
- [X] T008 Add exceptions in src/Backend/CRM.Shared/Exceptions/{RoleNotFoundException.cs,DuplicateRoleNameException.cs,CannotModifyBuiltInRoleException.cs,CannotDeleteRoleInUseException.cs,InvalidRoleException.cs}
- [X] T009 [P] Add mapping Role -> RoleDto in src/Backend/CRM.Application/Mapping/RoleProfile.cs

## Phase 3: [US1] List Roles (Admin)

- [X] T010 [US1] Create RoleDto in src/Backend/CRM.Application/Roles/Queries/RoleDto.cs
- [X] T011 [P] [US1] Create GetAllRolesQuery in src/Backend/CRM.Application/Roles/Queries/GetAllRolesQuery.cs
- [X] T012 [US1] Implement GetAllRolesQueryHandler with pagination/filter in src/Backend/CRM.Application/Roles/Queries/Handlers/GetAllRolesQueryHandler.cs
- [X] T013 [P] [US1] Add RolesController GET /api/v1/roles (Admin) in src/Backend/CRM.Api/Controllers/RolesController.cs
- [X] T014 [US1] Add endpoint response model (paged) in src/Backend/CRM.Api/Controllers/RolesController.cs
- [X] T015 [US1] Independent test criteria: returns 200 with 4 built-ins

## Phase 4: [US2] Get Role By Id (Admin)

- [X] T016 [US2] Create GetRoleByIdQuery in src/Backend/CRM.Application/Roles/Queries/GetRoleByIdQuery.cs
- [X] T017 [P] [US2] Implement GetRoleByIdQueryHandler (includes user count) in src/Backend/CRM.Application/Roles/Queries/Handlers/GetRoleByIdQueryHandler.cs
- [X] T018 [US2] Add RolesController GET /api/v1/roles/{roleId} in src/Backend/CRM.Api/Controllers/RolesController.cs
- [X] T019 [US2] Independent test criteria: 200 with role details; 404 if not found

## Phase 5: [US3] Create Role (Admin)

- [X] T020 [US3] Create CreateRoleCommand in src/Backend/CRM.Application/Roles/Commands/CreateRoleCommand.cs
- [X] T021 [P] [US3] Implement CreateRoleCommandHandler with CI-trim uniqueness in src/Backend/CRM.Application/Roles/Commands/Handlers/CreateRoleCommandHandler.cs
- [X] T022 [US3] Add RolesController POST /api/v1/roles in src/Backend/CRM.Api/Controllers/RolesController.cs
- [X] T023 [US3] Independent test criteria: 201 returns RoleDto; 409 on duplicate

## Phase 6: [US4] Update Role (Admin)

- [X] T024 [US4] Create UpdateRoleCommand in src/Backend/CRM.Application/Roles/Commands/UpdateRoleCommand.cs
- [X] T025 [P] [US4] Implement UpdateRoleCommandHandler (block built-in mods; CI-trim uniqueness) in src/Backend/CRM.Application/Roles/Commands/Handlers/UpdateRoleCommandHandler.cs
- [X] T026 [US4] Add RolesController PUT /api/v1/roles/{roleId} in src/Backend/CRM.Api/Controllers/RolesController.cs
- [X] T027 [US4] Independent test criteria: 200 on custom; 400 built-in; 409 duplicate; 404 not found

## Phase 7: [US5] Delete (Soft) Role (Admin)

- [X] T028 [US5] Create DeleteRoleCommand in src/Backend/CRM.Application/Roles/Commands/DeleteRoleCommand.cs
- [X] T029 [P] [US5] Implement DeleteRoleCommandHandler (soft delete; block built-in/in-use) in src/Backend/CRM.Application/Roles/Commands/Handlers/DeleteRoleCommandHandler.cs
- [X] T030 [US5] Add RolesController DELETE /api/v1/roles/{roleId} in src/Backend/CRM.Api/Controllers/RolesController.cs
- [X] T031 [US5] Independent test criteria: 200 delete; 400 built-in/in-use; 404 not found

## Final Phase: Polish & Cross-Cutting

- [X] T032 Add audit logging in role handlers using CRM.Infrastructure.Logging in src/Backend/CRM.Application/Roles/*
- [X] T033 [P] Add OpenAPI alignment (roles.openapi.yaml) check in specs/4-rbac-foundation/contracts/roles.openapi.yaml
- [X] T034 Add quickstart verification steps for roles in specs/4-rbac-foundation/quickstart.md
- [X] T035 Security review: verify [Authorize(Roles="Admin")] on all controller actions in src/Backend/CRM.Api/Controllers/RolesController.cs

## Dependencies
- Phase 2 foundational must complete before US1–US5
- US1 is independent
- US2 depends on Role entity and mapping
- US3–US5 depend on foundational + controller scaffolding

## Parallel Execution Examples
- T005, T007, T009 can run in parallel
- T011 and T013 in parallel after foundational
- T021 and T022 in parallel after queries
- T025 and T026 in parallel after US3

## Implementation Strategy
- Deliver MVP via US1 (roles listing) first
- Add US2 (get by id) for completeness
- Implement create/update/delete iteratively with tests per phase
