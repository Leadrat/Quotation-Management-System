# Tasks: Spec-005 User Profile & Password Management

Note: Tasks follow strict checklist format. [P] means parallelizable. [USx] is the user story label.

## Phase 1: Setup
- [X] T001 Create feature folder scaffolding for Spec-005 outputs at specs/5-user-profile-password/
- [X] T002 Ensure OpenAPI is referenced in solution build and Swagger loads contracts/user-profile-password.openapi.yaml in src/Backend/CRM.Api/Program.cs
- [X] T003 Add BCrypt.Net-Next package reference to src/Backend/CRM.Application/CRM.Application.csproj
- [X] T004 Add FluentValidation package (if missing) to src/Backend/CRM.Application/CRM.Application.csproj
- [X] T005 Add email provider configuration section to appsettings.json in src/Backend/CRM.Api/appsettings.json

## Phase 2: Foundational
- [X] T006 Create EF Core migration for PasswordResetToken entity in src/Backend/CRM.Infrastructure/Migrations/ (per data-model.md)
- [X] T007 Configure PasswordResetToken entity mapping in src/Backend/CRM.Infrastructure/EntityConfigurations/PasswordResetTokenEntityConfiguration.cs
- [X] T008 Wire PasswordResetToken DbSet in src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs
- [X] T009 Implement IPasswordHasher abstraction (wrap BCrypt) in src/Backend/CRM.Application/Common/Security/IPasswordHasher.cs
- [X] T010 [P] Implement BCryptPasswordHasher (cost 12) in src/Backend/CRM.Infrastructure/Security/BCryptPasswordHasher.cs
- [X] T011 Create password strength validator helper in src/Backend/CRM.Shared/Security/PasswordPolicy.cs
- [X] T012 [P] Add validation rules (regex constants) in src/Backend/CRM.Shared/Validation/ValidationRegex.cs
- [X] T013 Define domain events: UserProfileUpdated, PasswordChanged, PasswordReset in src/Backend/CRM.Domain/Events/*.cs
- [X] T014 [P] Implement email queue interface IEmailQueue in src/Backend/CRM.Application/Common/Notifications/IEmailQueue.cs
- [X] T015 Implement background email sender using provider SDK in src/Backend/CRM.Infrastructure/Notifications/EmailQueueProcessor.cs
- [X] T016 Add DI registrations for hasher, email queue, and event handlers in src/Backend/CRM.Api/Program.cs
- [X] T017 [P] Update global exception mapping for new exceptions in src/Backend/CRM.Api/Program.cs
- [X] T018 Implement RefreshToken invalidation service in src/Backend/CRM.Application/Auth/Services/IRefreshTokenRevoker.cs
- [X] T019 [P] Implement RefreshTokenRevoker using AppDbContext in src/Backend/CRM.Infrastructure/Auth/RefreshTokenRevoker.cs

- [X] T020 [US1] Create UpdateUserProfileCommand DTO in src/Backend/CRM.Application/Users/Commands/UpdateUserProfileCommand.cs
- [X] T021 [P] [US1] Create UpdateUserProfileValidator in src/Backend/CRM.Application/Users/Validators/UpdateUserProfileValidator.cs
- [X] T022 [US1] Implement UpdateUserProfileCommandHandler in src/Backend/CRM.Application/Users/Commands/Handlers/UpdateUserProfileCommandHandler.cs
- [X] T023 [P] [US1] Add AutoMapper profile for User -> UserSummary in src/Backend/CRM.Application/Mapping/UserProfile.cs
- [X] T024 [US1] Add PUT /api/v1/users/{userId}/profile to src/Backend/CRM.Api/Controllers/UsersController.cs
- [X] T025 [P] [US1] Publish UserProfileUpdated event and enqueue confirmation email in handler in src/Backend/CRM.Application/Users/Commands/Handlers/UpdateUserProfileCommandHandler.cs
- [X] T026 [US1] Unit tests for validator and handler in tests/CRM.Tests/Users/UpdateUserProfileTests.cs
- [ ] T027 [P] [US1] Integration test for endpoint success and validation errors in tests/CRM.Tests.Integration/Users/UpdateUserProfileEndpointTests.cs

- [X] T028 [US2] Create ChangePasswordCommand DTO in src/Backend/CRM.Application/Auth/Commands/ChangePasswordCommand.cs
- [X] T029 [P] [US2] Create ChangePasswordValidator in src/Backend/CRM.Application/Auth/Validators/ChangePasswordValidator.cs
- [X] T030 [US2] Implement ChangePasswordCommandHandler (verify current, strength, no reuse, set new hash, reset attempts, revoke refresh tokens) in src/Backend/CRM.Application/Auth/Commands/Handlers/ChangePasswordCommandHandler.cs
- [X] T031 [P] [US2] Map POST /api/v1/auth/change-password in src/Backend/CRM.Api/Controllers/AuthController.cs
- [X] T032 [US2] Publish PasswordChanged event and enqueue email in handler in src/Backend/CRM.Application/Auth/Commands/Handlers/ChangePasswordCommandHandler.cs
- [X] T033 [P] [US2] Add new exceptions: WeakPasswordException, PasswordReuseException, InvalidCurrentPasswordException, AccountLockedException in src/Backend/CRM.Shared/Exceptions/*.cs
- [X] T034 [US2] Unit tests for validator and handler in tests/CRM.Tests/Auth/ChangePasswordTests.cs
- [ ] T035 [P] [US2] Integration test for endpoint flows (success, weak, reuse, invalid current, locked) in tests/CRM.Tests.Integration/Auth/ChangePasswordEndpointTests.cs

## Phase 5: US3 - Admin Password Reset (Send One-Time Link) (P1)
- [X] T036 [US3] Create InitiatePasswordResetCommand DTO in src/Backend/CRM.Application/Auth/Commands/InitiatePasswordResetCommand.cs
- [X] T037 [P] [US3] Implement generator utility for 32-byte token and HMAC hashing in src/Backend/CRM.Infrastructure/Security/ResetTokenGenerator.cs
- [X] T038 [US3] Implement InitiatePasswordResetCommandHandler (create token, persist hash, revoke previous, enqueue email) in src/Backend/CRM.Application/Auth/Commands/Handlers/InitiatePasswordResetCommandHandler.cs
- [X] T039 [P] [US3] Map POST /api/v1/users/{userId}/reset-password (Admin only) in src/Backend/CRM.Api/Controllers/UsersController.cs
- [X] T040 [US3] Publish PasswordReset event and enqueue email in handler in src/Backend/CRM.Application/Auth/Commands/Handlers/InitiatePasswordResetCommandHandler.cs
- [ ] T041 [P] [US3] Unit tests for token generator and handler in tests/CRM.Tests/Auth/InitiatePasswordResetTests.cs
- [ ] T042 [US3] Integration test for endpoint (202 accepted, email enqueued, token persisted) in tests/CRM.Tests.Integration/Auth/InitiatePasswordResetEndpointTests.cs

- [ ] T043 Add Serilog structured logs for security events in src/Backend/CRM.Api/Program.cs and relevant handlers
- [ ] T044 [P] Harden security headers and ensure no sensitive data leaks in logs in src/Backend/CRM.Api/Program.cs
- [ ] T045 Add cleanup job for expired/used reset tokens in src/Backend/CRM.Infrastructure/Jobs/CleanupExpiredResetTokensJob.cs
- [ ] T046 [P] Update Swagger descriptions and response codes per contracts in src/Backend/CRM.Api/Program.cs
- [ ] T047 Review RBAC attributes on endpoints (Admin vs self) in src/Backend/CRM.Api/Controllers/*.cs
- [X] T048 [P] Add sample email templates (profile updated, password changed, password reset link) in src/Backend/CRM.Infrastructure/Notifications/Templates/
- [ ] T049 Update README/quickstart with curl examples and env keys in specs/5-user-profile-password/quickstart.md
- [ ] T050 Final audit: ensure BCrypt cost 12, global lockout policy, and token invalidation implemented across codebase

## Dependencies (Story Order)
1. Phase 1 → Phase 2 → US1 → US2 → US3 → Final Phase
2. US1, US2, US3 depend on Foundational (Phase 2)

## Parallel Execution Examples
- T010, T012, T014, T017, T019 can run in parallel after T009.
- Within US1: T021, T023, T025 can run in parallel after T020.
- Within US2: T029, T031, T033 can run in parallel after T028.
- Within US3: T037, T039, T041 can run in parallel after T036.

## Implementation Strategy
- Deliver MVP by completing US1 and US2 first; US3 can follow.
- Maintain spec traceability in PRs; each PR references Spec-005 and includes test evidence.
