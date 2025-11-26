# Tasks: Spec-001 User Entity & DTO Specification

Feature: User Entity & DTO (Spec-001)
Branch: 1-user-entity

## Phase 1: Setup

- [X] T001 Create feature branch `1-user-entity` per constitution
- [X] T002 Add planning artifacts to repo (plan/, contracts/, data-model.md, quickstart.md)
- [X] T003 Configure solution folders: /src/Backend/CRM.Domain, CRM.Application, CRM.Infrastructure, CRM.Shared, tests/
- [X] T004 Add NuGet references (EFCore, Npgsql, FluentValidation, BCrypt.Net-Next, AutoMapper)

## Phase 2: Foundational

- [X] T005 Create ValidationConstants.cs at /src/Backend/CRM.Shared/Constants/ValidationConstants.cs
- [X] T006 [P] Create PasswordHelper.cs at /src/Backend/CRM.Shared/Helpers/PasswordHelper.cs (BCrypt cost 12)
- [X] T007 Create EF Core DbContext stub (if not present) at /src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs
- [X] T008 [P] Add citext extension migration snippet (guarded) in upcoming migration

## Phase 3: User Stories (P1)

User stories derived from Spec-001 scenarios.

### US1: Admin creates a SalesRep with ReportingManagerId set to Manager
- [X] T009 [US1] Create Domain entity User.cs at /src/Backend/CRM.Domain/Entities/User.cs
- [X] T010 [P] [US1] Create EF configuration UserEntityConfiguration.cs at /src/Backend/CRM.Infrastructure/EntityConfigurations/UserEntityConfiguration.cs (citext Email, FKs, indexes)
- [X] T011 [US1] Create CreateUsersTable migration at /src/Backend/CRM.Infrastructure/Migrations/[timestamp]_CreateUsersTable.cs (CREATE EXTENSION citext; Email citext; indexes)
- [X] T012 [P] [US1] Create DTO CreateUserRequest.cs at /src/Backend/CRM.Shared/DTOs/CreateUserRequest.cs
- [X] T013 [P] [US1] Create DTO UserDto.cs at /src/Backend/CRM.Shared/DTOs/UserDto.cs
- [X] T014 [US1] Create UserValidator.cs at /src/Backend/CRM.Application/Validators/UserValidator.cs
- [X] T015 [P] [US1] Create CreateUserRequestValidator.cs at /src/Backend/CRM.Application/Validators/CreateUserRequestValidator.cs
- [X] T016 [US1] Seed demo Roles and Users in migration (four users with GUIDs)
- [X] T017 [P] [US1] Add AutoMapper profile mapping User→UserDto at /src/Backend/CRM.Application/Mapping/UserProfile.cs
- [X] T018 [US1] Add unit tests: validators and helper at /tests/CRM.Tests/Application/Validators/

### Independent Test Criteria (US1)
- [ ] T019 [US1] Migration applies with citext extension and constraints
- [ ] T020 [US1] Creating SalesRep without ReportingManagerId is rejected
- [ ] T021 [US1] Creating SalesRep with valid Manager succeeds; returns UserDto (no PasswordHash)

### US2: Admin updates user profile (names, mobile, phone code)
- [ ] T022 [US2] Create UpdateUserRequest.cs at /src/Backend/CRM.Shared/DTOs/UpdateUserRequest.cs
- [ ] T023 [P] [US2] Create UpdateUserRequestValidator.cs at /src/Backend/CRM.Application/Validators/UpdateUserRequestValidator.cs (E.164; PhoneCode consistency)
- [ ] T024 [US2] Add application command/handler stubs (UpdateUser) at /src/Backend/CRM.Application/Users/Commands/UpdateUser/
- [ ] T025 [US2] Unit tests: update validation positive/negative at /tests/CRM.Tests/Application/Validators/

### Independent Test Criteria (US2)
- [ ] T026 [US2] Update with invalid mobile rejected; valid passes
- [ ] T027 [US2] If Mobile and PhoneCode mismatch, rejected

### US3: Role changes and ReportingManager semantics
- [ ] T028 [US3] Add role-change rule enforcement in UpdateUser command handler (to SalesRep require Manager; from SalesRep clear ReportingManagerId)
- [ ] T029 [P] [US3] Unit tests: role change rules at /tests/CRM.Tests/Application/Users/

### Independent Test Criteria (US3)
- [ ] T030 [US3] Client→SalesRep without manager rejected; with manager passes
- [ ] T031 [US3] SalesRep→Manager clears ReportingManagerId

### US4: Deactivate (soft delete) user
- [ ] T032 [US4] Add DeactivateUser command handler to set DeletedAt and force IsActive=false at /src/Backend/CRM.Application/Users/Commands/DeactivateUser/
- [ ] T033 [P] [US4] Ensure repository/query filters active users by DeletedAt IS NULL
- [ ] T034 [US4] Tests: deactivate sets DeletedAt and IsActive=false; filtered queries exclude user

### Independent Test Criteria (US4)
- [ ] T035 [US4] Active list excludes soft-deleted users

### US5: Retrieve user(s) for admin queries
- [ ] T036 [US5] Add GetUserById query and handler at /src/Backend/CRM.Application/Users/Queries/GetUserById/
- [ ] T037 [P] [US5] Add GetUsers paged query with filters (Email, RoleId, IsActive) at /src/Backend/CRM.Application/Users/Queries/GetUsers/
- [ ] T038 [US5] Tests: mapping to UserDto hides sensitive fields

### Independent Test Criteria (US5)
- [ ] T039 [US5] Email filter is case-insensitive (citext)

## Final Phase: Polish & Cross-Cutting
- [ ] T040 Add XML docs to public methods (PasswordHelper, Validators)
- [ ] T041 Verify all files compiled and adhere to naming conventions (TitleCase)
- [ ] T042 Prepare PR-1 (design artifacts); PR-2 (entity+migration); PR-3 (validators+seed); PR-4 (tests) per cadence

## Dependencies
- US1 → US2 → US3 → US4 → US5 (data model extends usage)
- Parallel opportunities: T010/T012/T013/T015/T017; T023/T029; T033/T037

## Parallel Execution Examples
- Example 1: Implement `UserEntityConfiguration` (T010) while DTOs (T012, T013) and CreateUserRequestValidator (T015) proceed in parallel.
- Example 2: Role-change tests (T029) parallel with UpdateUserRequestValidator (T023).
- Example 3: Query filters (T033) in parallel with GetUsers query (T037).

## Implementation Strategy (MVP)
- MVP Scope: Complete US1 (entity, migration, DTOs, validators, seed, basic tests) → open PR-2/PR-3.
- Subsequent: US2–US5 in order, each culminating in a milestone PR with passing tests.
