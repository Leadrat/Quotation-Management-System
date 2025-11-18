# Quickstart: Implement Spec-001 (User Entity & DTO)

Branch: 1-user-entity

## Steps

1) Domain Entity
- Add file: /src/Backend/CRM.Domain/Entities/User.cs
- Properties and navigation as in data-model.md

2) Infrastructure
- Add: /src/Backend/CRM.Infrastructure/EntityConfigurations/UserEntityConfiguration.cs
- Configure CITEXT for Email, indexes, FKs
- Migration snippet:
  - CREATE EXTENSION IF NOT EXISTS citext;
  - Email column type = citext

3) Validators
- Add:
  - /src/Backend/CRM.Application/Validators/UserValidator.cs
  - /src/Backend/CRM.Application/Validators/CreateUserRequestValidator.cs
  - /src/Backend/CRM.Application/Validators/UpdateUserRequestValidator.cs

4) Shared
- Add:
  - /src/Backend/CRM.Shared/Constants/ValidationConstants.cs
  - /src/Backend/CRM.Shared/Helpers/PasswordHelper.cs (BCrypt, cost 12)

5) Migration & Seed
- Create migration: CreateUsersTable
- Seed 4 demo users (GUIDs provided)

6) Tests
- Add unit tests for validators and password helper
- Target: ≥80% coverage

## PR Milestones (per constitution)
- PR-1: Design files (plan/, contracts/, data-model.md, quickstart.md)
- PR-2: Entity + configuration + migration compiles
- PR-3: Validators + helpers + seed
- PR-4: Tests passing (≥80%)
