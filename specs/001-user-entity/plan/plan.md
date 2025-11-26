# Implementation Plan: Spec-001 User Entity & DTO

Created: 2025-11-12
Branch: 1-user-entity (per constitution v1.1.0)

## Technical Context
- Backend: .NET Core Web API, Clean Architecture, EF Core (PostgreSQL)
- Domain: User entity with 16 columns, RBAC with Roles, self-reference for ReportingManager
- Validation: FluentValidation
- Hashing: BCrypt (work factor 12)
- Email: Postgres citext (enable extension)
- Soft delete: DeletedAt canonical; IsActive subordinate
- Consistency: PhoneCode must match Mobile country code when both present

## Constitution Check
- Spec-driven: This plan derives from Spec-001 ✔
- Clean Architecture boundaries: Domain entity, Infrastructure configuration, Application validators ✔
- Security/Data integrity: bcrypt, citext, FK constraints, indexes ✔
- Testing & quality gates: unit tests (validators/helper) ≥ 80% ✔
- Observability: Not applicable for entity-only scope (no runtime) — N/A
- Branch-per-spec & cadence: Use `1-user-entity`; milestone PRs after design, migration, validators, tests ✔

Gate Result: PASS (no violations)

## Phase 0: Outline & Research
- Consolidated in plan/research.md (decisions on DeletedAt, citext, BCrypt, role changes, phone code)

## Phase 1: Design & Contracts
- Data model documented in plan/data-model.md
- API contracts (admin user mgmt) drafted in contracts/users.openapi.yaml (CRUD subset)
- Quickstart created for implementers in plan/quickstart.md

## Phase 2: Implementation Preparation
- Tasks:
  1) Add Domain entity User.cs
  2) Add EF configuration (UserEntityConfiguration.cs), citext extension migration
  3) Create validators (UserValidator.cs, CreateUserRequestValidator.cs, UpdateUserRequestValidator.cs)
  4) Add helpers/constants (PasswordHelper.cs, ValidationConstants.cs)
  5) Seed demo users in migration
  6) Tests: validators, password helper, entity basics

## Milestone PR Cadence (per constitution)
- PR-1: Design artifacts (data-model.md, contracts, quickstart, spec updates)
- PR-2: Entity + configuration + migration (compiles, up migration)
- PR-3: Validators + helpers + seed data
- PR-4: Unit tests passing (≥80% coverage for this spec)

## Risks & Mitigations
- citext extension absent in some envs → guard with `CREATE EXTENSION IF NOT EXISTS citext;`
- Strict name/phone regex → QA review and adjust if required
- Role GUID dependence → ensure Roles seeded before or within test harness

## Done Criteria
- All Acceptance Criteria in Spec-001 marked PASS with tests
- Migration applied successfully; seed present
- Contracts and docs published to repo
