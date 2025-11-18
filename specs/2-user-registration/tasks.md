# Tasks: Spec-002 User Registration & Account Creation

Feature: User Registration & Account Creation (Spec-002)
Branch: 2-user-registration

## Phase 1: Setup

- [ ] T001 Ensure EF Core migrator runs against target DB (CRM.Migrator) using POSTGRES_CONNECTION
- [ ] T002 Add feature flag placeholders for email verification flow (Spec-003 alignment) in README/quickstart

## Phase 2: Foundational

- [X] T003 Create EmailVerificationToken entity and configuration (draft, to be finalized in Spec-003) at /src/Backend/CRM.Domain/Entities/EmailVerificationToken.cs
- [X] T004 [P] Add DbSet and configuration hook for EmailVerificationToken in AppDbContext at /src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs
- [ ] T005 [P] Add mapping/profile placeholders for any new DTOs (if needed later) at /src/Backend/CRM.Application/Mapping/

## Phase 3: US1 Client Self-Registration (P1)

- [X] T006 [US1] Create RegisterClientCommand.cs at /src/Backend/CRM.Application/Users/Commands/RegisterClientCommand.cs
- [X] T007 [P] [US1] Create RegisterResult.cs at /src/Backend/CRM.Application/Users/Commands/Results/RegisterResult.cs
- [X] T008 [US1] Create RegisterClientCommandHandler.cs at /src/Backend/CRM.Application/Users/Commands/Handlers/RegisterClientCommandHandler.cs
- [X] T009 [P] [US1] Implement email uniqueness check (citext) in handler using AppDbContext
- [X] T010 [US1] Hash password via PasswordHelper and create User with RoleId=Client, IsActive=true
- [X] T011 [P] [US1] Persist user and emit UserCreated domain event at /src/Backend/CRM.Domain/Events/UserCreated.cs
- [X] T012 [US1] Add AuthController.cs with POST /api/v1/auth/register at /src/Backend/CRM.Api/Controllers/AuthController.cs
- [X] T013 [P] [US1] Add rate limiting (≤5/hour per IP) middleware/filter placeholder at /src/Backend/CRM.Api/
- [X] T014 [US1] Return 201 with RegisterResult (no password) per contract

### Independent Test Criteria (US1)
- [X] T015 [US1] Duplicate email returns 409 (citext uniqueness)
- [X] T016 [US1] Weak password returns 400
- [X] T017 [US1] Successful register returns 201 and creates Client user

## Phase 4: US2 Admin User Creation (P1)

- [X] T018 [US2] Create CreateUserCommand.cs at /src/Backend/CRM.Application/Users/Commands/CreateUserCommand.cs
- [X] T019 [P] [US2] Create UserCreatedResult.cs at /src/Backend/CRM.Application/Users/Commands/Results/UserCreatedResult.cs
- [X] T020 [US2] Create CreateUserCommandHandler.cs at /src/Backend/CRM.Application/Users/Commands/Handlers/CreateUserCommandHandler.cs
- [X] T021 [P] [US2] Validate RoleId exists and not Client; manager validation for SalesRep
- [X] T022 [US2] Hash password, create user, persist, emit AdminUserCreated at /src/Backend/CRM.Domain/Events/AdminUserCreated.cs
- [X] T023 [P] [US2] Add UsersController.cs with POST /api/v1/users (Authorize Admin) at /src/Backend/CRM.Api/Controllers/UsersController.cs
- [X] T024 [US2] Return 201 with UserCreatedResult and emailSent flag

### Independent Test Criteria (US2)
- [X] T025 [US2] Non-admin returns 403
- [X] T026 [US2] SalesRep without manager returns 422
- [X] T027 [US2] SalesRep with valid manager returns 201 and associates manager

## Final Phase: Polish & Cross-Cutting
- [X] T028 Add audit logging for registration attempts (success/failure) at /src/Backend/CRM.Infrastructure/Logging/
- [X] T029 [P] Ensure sanitization (trim/lowercase email; strip HTML names) in handlers
- [X] T030 Verify API contracts align with openapi.yaml; adjust responses as needed
- [X] T031 Update quickstart.md with curl examples and troubleshooting

## Dependencies
- US1 → US2 (shared validation and persistence patterns)

## Parallel Execution Examples
- T007 and T009 can proceed while T008 is scaffolded
- T019 and T021 can proceed while T020 is scaffolded

## Implementation Strategy (MVP)
- MVP Scope: Complete US1 and US2 endpoints and handlers without email delivery; emit events and return expected responses.
- Defer email token mechanics and delivery pipeline details to Spec-003.
