<!--
Sync Impact Report
Version change: 1.1.0 → 1.2.0
Modified principles:
- Development Workflow: clarified frontend starts from Spec 3 using TailAdmin
Added sections:
- Frontend Framework & UI Theme Integration (TailAdmin Next.js)
Removed sections: None
Templates requiring updates (⚠ pending):
- .specify/templates/plan-template.md (add frontend/TailAdmin references) ⚠
- .specify/templates/spec-template.md (frontend module guidance) ⚠
- .specify/templates/tasks-template.md (frontend tasks scaffolding) ⚠
- .specify/templates/commands/* (ensure agent-agnostic wording) ⚠
Follow-up TODOs:
- Add PR template section for milestone cadence and branch-naming pattern
- Add frontend onboarding notes and TailAdmin quickstart snippet in frontend specs
-->

# CRM Quotation Management System Constitution

## Core Principles

### I. Spec-Driven Delivery (NON-NEGOTIABLE)
All features MUST originate from approved specs under Spec Kit (sections 1–12 of the
Project Constitution). Each spec defines scope, DTOs, commands/queries, validators,
and acceptance tests. Code, tests, and docs MUST trace back to a spec ID.
Rationale: Ensures clarity, testability, and alignment across backend, frontend, and DB.

### II. Clean Architecture & RBAC Enforcement
Use .NET Clean Architecture with MediatR, Ardalis.Specification, and EF Core.
Boundaries MUST be respected: Domain, Application, Infrastructure, API.
RBAC MUST be enforced at API endpoints using roles Admin, Manager, SalesRep, Client.

### III. Security, Compliance, and Data Integrity
JWT with 1-hour access tokens and 30-day refresh tokens. Passwords hashed with bcrypt.
Inputs validated with FluentValidation. CORS restricted to frontend origins.
All tables use UUID PKs, FK constraints, and indexes per schema. Sensitive actions audited.

### IV. Testing & Quality Gates
Unit coverage ≥80% backend; integration tests for API + DB; E2E tests for critical flows.
CI enforces build, tests, linters, SonarQube checks, and spec compliance.
Performance budgets: API p90 <200ms, frontend LCP <2s.

### V. Observability, Auditability, and Change Control
Serilog structured logs, Sentry error tracking, Datadog APM. Complete audit trail for
entity changes and critical reads. Semantic versioning for constitution and API.
Breaking changes require migration and stakeholder sign-off.

## Additional Constraints & Standards

- Backend: .NET Core Web API (C# 12+), MediatR, Ardalis.Specification, EF Core.
- Frontend: Next.js 15 (React 19, TypeScript), Tailwind CSS v4, TailAdmin base template; Axios, Zustand/Context.
- Database: PostgreSQL, UUID PKs, TIMESTAMPTZ, no enums/JSONB; statuses/types via tables.
- Auth: JWT; OAuth2 optional; Client OTP (email + SMS) for portal access.
- Notifications: WebSocket (Socket.IO), Email (SMTP), SMS.
- Files: S3 for templates/PDFs; PDF generation via service in Shared utilities.
- DevOps: Docker, GitHub Actions; environments (Dev/Staging/Prod); daily backups 30d.
- Security: HTTPS-only, strict CORS, rate limiting, CSRF, secrets via env/Key Vault.
- Performance: Indexed queries, pagination by default, cache common lookups.
- RBAC: Admin, Manager, SalesRep, Client with least-privilege access.

## Frontend Framework & UI Theme Integration

- Template: TailAdmin Free Next.js Admin Dashboard
- Repository: https://github.com/TailAdmin/free-nextjs-admin-dashboard
- Technology stack: Next.js 15, React 19, TypeScript, Tailwind CSS v4
- Design system: Tailwind CSS utility classes; branding colors: white + forest green
- Start point: Frontend delivery begins with Spec 3 (User Authentication & JWT). Subsequent specs
  (Spec 4 RBAC dashboards, Spec 5 user profile, Spec 6 client management, etc.) extend TailAdmin
  layout/components and follow its folder structure and conventions.
- Environment prerequisites: Node.js ≥18 (20.x recommended), yarn or npm per TailAdmin docs
- Rules:
  - Developers MUST use TailAdmin components/layouts unless a deviation is approved and documented.
  - Tailwind utility classes MUST align to the project theme; custom CSS kept minimal and documented.
  - Frontend specs MUST include TailAdmin usage details and customization guidance.

## Development Workflow & Review Process

- Spec-first planning using the 45-spec set; tasks generated via Spec Kit templates.
- Branch-per-spec is MANDATORY: create a feature branch per spec using pattern `{number}-{short-name}` (e.g., `1-user-entity`).
- Milestone cadence: after every milestone on a spec (design complete, skeleton code, validations, migrations, tests pass), run tests, commit, push, and raise a PR to `main`.
- Branching: main, develop, feature/{number}-{short-name}; PRs require 2 approvals.
- Checks: unit + integration tests, linting, SonarQube, Swagger docs updated.
- DB: EF Core migrations reviewed; rollback plan for breaking changes.
- Releases: CI/CD with staging UAT, observability checks, and rollback strategy.

- Frontend workflow additions:
  - From Spec 3 onward, frontend work MUST be scaffolded from TailAdmin. New pages use TailAdmin
    layouts and components; authentication UI integrates JWT flow per backend contracts.
  - Frontend PRs MUST pass lint/build and include screenshots of key views.

## Governance

- Authority: This constitution is the source of truth across specs, code, and reviews.
- Amendments: Propose via PR referencing impacted specs; require architect + PM + QA
  approval and a migration/communication plan.
- Versioning: Semantic versioning. MAJOR for incompatible changes; MINOR for new
  principles/sections; PATCH for clarifications.
- Compliance: PR template includes Constitution Check. CI blocks merges on violations.
- PR cadence: Each spec MUST progress via milestone PRs to `main`; do not accumulate oversized PRs. Each PR references its Spec ID and includes test evidence.
- Reviews: Quarterly governance review; security and performance revisited each cycle.

**Version**: 1.2.0 | **Ratified**: 2025-11-12 | **Last Amended**: 2025-11-13
