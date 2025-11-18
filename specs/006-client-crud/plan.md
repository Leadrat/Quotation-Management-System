# Implementation Plan: Client Entity & CRUD Operations (Spec-006)

**Branch**: `006-client-crud` | **Date**: 2025-11-13 | **Spec**: specs/006-client-crud/spec.md
**Input**: Feature specification from `/specs/006-client-crud/spec.md`

## Summary

Implement the Client entity and full CRUD with ownership (SalesRep vs Admin), global case-insensitive
email uniqueness for active records, soft delete via DeletedAt, pagination with clamping, and
validations (E.164 mobile, RFC5322 email, GSTIN format, India GST StateCode constants). Publish domain
events (created/updated/deleted) and record audit entries.

## Technical Context

**Language/Version**: C# 12, .NET 8 Web API  
**Primary Dependencies**: MediatR, Ardalis.Specification, EF Core, FluentValidation, AutoMapper, Serilog  
**Storage**: PostgreSQL (UUID PKs, TIMESTAMPTZ); EF Core migrations  
**Testing**: xUnit for unit/integration tests  
**Target Platform**: Backend API (Windows/Linux), Frontend later via TailAdmin (Spec 3+)  
**Project Type**: Web (backend API), frontend later  
**Performance Goals**: API p90 <200ms; list queries paginated and indexed  
**Constraints**: Strict RBAC; partial unique index on Email WHERE DeletedAt IS NULL; clamp pagination  
**Scale/Scope**: Thousands of clients per tenant; list endpoints must paginate reliably

## Constitution Check

Gates (must pass):
- Spec-driven delivery: This plan references Spec-006 and will generate design artifacts before coding.
- Clean Architecture & RBAC: Commands/Queries via MediatR; Admin/SalesRep enforced at endpoints.
- Security & Data Integrity: Validation via FluentValidation; UUID PKs; indexes; audit logging via Serilog.
- Testing & Quality Gates: Unit + integration tests required; Swagger contracts produced.
- Observability: Structured logs for CRUD and authorization denials.

Status: PASS. No violations identified.

## Project Structure

### Documentation (this feature)

```text
specs/006-client-crud/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
└── contracts/
```

### Source Code (repository root)

```text
src/Backend/
├── CRM.Domain/Entities/Client.cs
├── CRM.Infrastructure/EntityConfigurations/ClientEntityConfiguration.cs
├── CRM.Infrastructure/Migrations/<timestamp>_CreateClients.cs
├── CRM.Application/Clients/
│   ├── Commands/{Create,Update,Delete}ClientCommand*.cs
│   ├── Commands/Handlers/{Create,Update,Delete}ClientCommandHandler.cs
│   ├── Queries/{GetClientById,GetAllClients,GetClientsByCreatedByUser,GetClientByEmail}*.cs
│   ├── Queries/Handlers/*.cs
│   ├── Dtos/ClientDto.cs
│   ├── Validators/{ClientValidator,CreateClientRequestValidator}.cs
│   └── Exceptions/{ClientNotFoundException,DuplicateEmailException,InvalidStateCodeException}.cs
├── CRM.Api/Controllers/ClientsController.cs
└── CRM.Application/Common/Validation/StateCodeConstants.cs

tests/
├── CRM.Tests/Clients/*.cs
└── CRM.Tests.Integration/Clients/*.cs
```

**Structure Decision**: Backend API in existing Clean Architecture layout; documentation under
specs/006-client-crud.

## Complexity Tracking

None; plan conforms to constitution gates.
