# Tasks: Spec-003 User Authentication & JWT

Feature: User Authentication & JWT (Spec-003)
Branch: 3-user-auth-jwt

## Phase 1: Setup

- [X] T001 Add JwtSettings config bindings and environment mapping in src/Backend/CRM.Api/Program.cs
- [X] T002 Add auth packages (Microsoft.AspNetCore.Authentication.JwtBearer) in src/Backend/CRM.Api/CRM.Api.csproj
- [X] T003 Configure Authentication/Authorization middleware in src/Backend/CRM.Api/Program.cs
- [X] T004 [P] Create configuration class JwtSettings in src/Backend/CRM.Shared/Config/JwtSettings.cs

## Phase 2: Foundational

- [X] T005 Create RefreshToken entity in src/Backend/CRM.Domain/Entities/RefreshToken.cs
- [X] T006 [P] Add DbSet and configuration in src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs
- [X] T007 [P] Create EF configuration for RefreshToken in src/Backend/CRM.Infrastructure/EntityConfigurations/RefreshTokenEntityConfiguration.cs
- [X] T008 Create migration CreateRefreshTokensTable in src/Backend/CRM.Infrastructure/Migrations/
- [X] T009 [P] Create exceptions in src/Backend/CRM.Shared/Exceptions/{InvalidCredentialsException.cs,InvalidTokenException.cs,TokenExpiredException.cs,TokenRevokedException.cs,UserNotActiveException.cs}
- [X] T010 Create domain events in src/Backend/CRM.Domain/Events/{UserLoggedIn.cs,TokenRefreshed.cs,UserLoggedOut.cs,LoginAttemptFailed.cs}
- [X] T011 [P] Create IJwtTokenGenerator interface in src/Backend/CRM.Application/Auth/Services/IJwtTokenGenerator.cs
- [X] T012 Implement JwtTokenGenerator in src/Backend/CRM.Infrastructure/Auth/JwtTokenGenerator.cs
- [X] T013 [P] Add dual-key validation support (current + previous secret) in JwtTokenGenerator

## Phase 3: [US1] Login (P1)

- [X] T014 [US1] Create LoginCommand.cs in src/Backend/CRM.Application/Auth/Commands/LoginCommand.cs
- [X] T015 [P] [US1] Create LoginResult.cs in src/Backend/CRM.Application/Auth/Commands/Results/LoginResult.cs
- [X] T016 [US1] Implement LoginCommandHandler.cs in src/Backend/CRM.Application/Auth/Commands/Handlers/LoginCommandHandler.cs
- [X] T017 [P] [US1] Add AuthController Login endpoint POST /api/v1/auth/login in src/Backend/CRM.Api/Controllers/AuthController.cs
- [X] T018 [US1] Update Program.cs exception mapping → 400/401/403 for auth errors
- [X] T019 [P] [US1] Emit UserLoggedIn and audit logging

### Independent Test Criteria (US1)
- [ ] T020 [US1] 401 on invalid email or wrong password (generic message)
- [ ] T021 [US1] 403 on inactive user
- [ ] T022 [US1] 200 returns access/refresh tokens and user info

## Phase 4: [US2] Refresh Token (P1)

- [X] T023 [US2] Create RefreshTokenCommand.cs in src/Backend/CRM.Application/Auth/Commands/RefreshTokenCommand.cs
- [X] T024 [P] [US2] Create RefreshTokenResult.cs in src/Backend/CRM.Application/Auth/Commands/Results/RefreshTokenResult.cs
- [X] T025 [US2] Implement RefreshTokenCommandHandler.cs with rotation in src/Backend/CRM.Application/Auth/Commands/Handlers/RefreshTokenCommandHandler.cs
- [X] T026 [P] [US2] Add AuthController Refresh endpoint POST /api/v1/auth/refresh-token (cookie preferred; JSON fallback) in src/Backend/CRM.Api/Controllers/AuthController.cs
- [X] T027 [US2] Persist new RefreshToken, revoke previous (rotation), set HttpOnly; Secure; SameSite=None cookie for browser clients

### Independent Test Criteria (US2)
- [ ] T028 [US2] 200 returns new access token (and rotated refresh if returned)
- [ ] T029 [US2] 401 on invalid/expired/revoked refresh token

## Phase 5: [US3] Logout (P1)

- [X] T030 [US3] Create LogoutCommand.cs in src/Backend/CRM.Application/Auth/Commands/LogoutCommand.cs
- [X] T031 [P] [US3] Create LogoutResult.cs in src/Backend/CRM.Application/Auth/Commands/Results/LogoutResult.cs
- [X] T032 [US3] Implement LogoutCommandHandler.cs in src/Backend/CRM.Application/Auth/Commands/Handlers/LogoutCommandHandler.cs
- [X] T033 [P] [US3] Add AuthController Logout endpoint POST /api/v1/auth/logout in src/Backend/CRM.Api/Controllers/AuthController.cs
- [ ] T034 [US3] Revoke refresh token; optionally add access token to blacklist placeholder service

### Independent Test Criteria (US3)
- [ ] T035 [US3] 200 on logout; refresh token revoked
- [ ] T036 [US3] 401 when not authenticated

## Final Phase: Polish & Cross-Cutting

- [X] T037 Configure CORS for app.crm.com ↔ api.crm.com in src/Backend/CRM.Api/Program.cs
- [ ] T038 [P] Add CSRF guidance and (optional) anti-forgery plumbing when access token via cookie in src/Backend/CRM.Api/
- [X] T039 Add security headers middleware (HSTS, X-Content-Type-Options, etc.) in src/Backend/CRM.Api/
- [X] T040 Verify openapi.yaml alignment and update if needed in specs/3-user-auth-jwt/contracts/openapi.yaml
- [X] T041 Update quickstart.md with cookie examples and env var notes in specs/3-user-auth-jwt/quickstart.md

## Dependencies
- Phase 2 foundational must complete before US1–US3
- US1 independent of US2/US3; US2 and US3 depend on RefreshToken entity and JwtTokenGenerator

## Parallel Execution Examples
- T011–T013 can proceed in parallel with T005–T008
- T015 and T017 can proceed while T016 is scaffolded
- T024 and T026 can proceed while T025 is scaffolded

## Implementation Strategy (MVP)
- Deliver US1 login first with token issuance
- Add US2 refresh with rotation next
- Add US3 logout with revocation; blacklist placeholder optional
