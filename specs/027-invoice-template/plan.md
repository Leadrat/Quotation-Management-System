# Implementation Plan: Invoice/Quote Template from Uploaded Document

**Branch**: `027-invoice-template` | **Date**: 2025-11-24 | **Spec**: `specs/027-invoice-template/spec.md`
**Input**: Feature specification from `/specs/027-invoice-template/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

This feature enables users to upload existing invoice/quote documents (DOCX/PDF), convert them into reusable templates with placeholders, and then generate new invoices/quotes from structured JSON data. Users can preview generated documents in a web view and download them as PDFs. The system supports multiple line item types (subscriptions, add-ons, services) and enforces role-based access using the existing CRM role model.

The implementation will use a **Next.js + TypeScript frontend** for upload UI, template configuration, web preview, and PDF download triggers, backed by a **.NET Core API** responsible for file ingestion, template storage, placeholder mapping, data merge, and PDF generation. Existing CRM authentication/roles will be reused for authorization.

## Technical Context

**Language/Version**: Frontend: TypeScript with Next.js; Backend: .NET Core (latest LTS in this repo).  
**Primary Dependencies**: Next.js (React) for UI, a .NET document-processing/PDF library (specific choice to be made in research), existing CRM authentication/authorization stack.  
**Storage**: PostgreSQL (existing CRM relational database) for templates and generated-document metadata; local disk file storage within the backend environment for uploaded source files and generated outputs.  
**Testing**: Frontend: React/Next.js testing setup (e.g., Jest + Testing Library); Backend: xUnit or equivalent .NET test framework (NEEDS CLARIFICATION: standard in this repo).  
**Target Platform**: Web application (browser clients) backed by .NET services running on the existing CRM deployment environment.  
**Project Type**: Web (separate frontend and backend projects within this repository).  
**Performance Goals**: For typical documents, time from selecting template + data to seeing rendered web view ≤ 3 seconds (from spec SC-002).  
**Constraints**: Must integrate with existing CRM auth/roles model; must support DOCX/PDF uploads for invoices/quotes; must keep templates stable over time while allowing versioning; must respect retention/archival policy (templates long-lived, generated docs ~7 years).  
**Scale/Scope**: Designed for day-to-day CRM usage by sales and finance teams; expected volume in the low thousands of templates and tens of thousands of generated documents over time rather than internet-scale traffic.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

[Gates determined based on constitution file]

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── Api/                 # .NET Core web API for templates, generation, PDFs
│   ├── Domain/              # Template, line item, and document domain models
│   ├── Application/         # Use-cases/services (upload, map placeholders, generate)
│   └── Infrastructure/      # Persistence, file storage, document/PDF adapters
└── tests/
    ├── Unit/
    ├── Integration/
    └── Contract/

frontend/
├── src/
│   ├── pages/               # Next.js pages for templates, generation, history
│   ├── components/          # Reusable UI components (upload controls, editors)
│   ├── features/invoicing/  # Feature-specific hooks, state, and utilities
│   └── services/            # API clients for backend endpoints
└── tests/
    ├── unit/
    └── integration/
```

**Structure Decision**: Use a split `backend/` (.NET Core API) and `frontend/` (Next.js + TypeScript) structure, aligning this feature with a clear web-application separation of concerns.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
