# Implementation Plan: Import Templates (Chat + Drag & Drop)

**Branch**: `029-import-templates` | **Date**: 2025-11-25 | **Spec**: specs/029-import-templates/spec.md
**Input**: Feature specification from `/specs/029-import-templates/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Deliver a chat-like import flow where users drag & drop documents (pdf, docx, xlsx, xslt, dotx), the system parses text/tables, guides variable mapping via Gemini LLM, generates a lookalike Word-merge template for preview, and saves it as a reusable template.

## Technical Context

**Language/Version**: Backend C# (.NET), Frontend Next.js/TypeScript  
**Primary Dependencies**: OpenXML SDK, PDF/XLSX parsers, Gemini LLM (server-side)  
**Storage**: PostgreSQL (existing) for templates/import sessions; blob/file storage for uploads  
**Testing**: xUnit for .NET, Playwright/RTL for frontend  
**Target Platform**: Web app (backend API + Next.js frontend)  
**Project Type**: Web (frontend + backend)  
**Performance Goals**: LLM p95 ≤ 2.5s; import-to-preview ≤ 90s for 1-page DOCX/XLSX  
**Constraints**: No API keys in frontend; file size ≤ 10MB; parse timeout ≤ 30s  
**Scale/Scope**: Single-user flow per session; queue/streaming optional later

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- Test-first encouraged; add unit tests for parsing/mapping and contract tests for API.  
- Security: Keys must remain server-side; validate file types/sizes.  
- Simplicity: MVP first (US1+US2), then generation (US3).

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
src/
├── Backend/
│   ├── CRM.Api/                      # Controllers: ImportTemplatesController, ChatController
│   ├── CRM.Application/              # Import sessions, mapping services, template gen
│   ├── CRM.Domain/                   # Entities: ImportSession, Template, Mapping
│   └── CRM.Infrastructure/           # EF configs, migrations, storage
└── Frontend/web/
    └── src/
        ├── app/(protected)/imports/  # pages: new (drag-drop), session/[id]
        ├── components/imports/       # chat panel, mapping UI, previews
        └── lib/api/imports.ts        # API client
```

**Structure Decision**: Use existing backend/frontend layout; add Import feature modules as above.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |

---

## Phase 0: Outline & Research

- Extract unresolved questions from spec and decide defaults: variable syntax, parse limits, branding.  
- Evaluate PDF/XLSX parsing options and OpenXML fidelity tactics.  
- Establish LLM prompt patterns for mapping and conflict resolution.

Deliverable: specs/029-import-templates/research.md

## Phase 1: Design & Contracts

- Data model: ImportSession, Mapping, Template; validation rules.  
- API contracts: upload/parse, chat, mapping save, generate, preview, save template.  
- Quickstart: env, run, sample flows.

Deliverables: data-model.md, contracts/openapi.yaml, quickstart.md

## Phase 2: Implementation (MVP US1+US2)

- Backend endpoints: upload/parse, chat (Gemini), mapping CRUD.  
- Frontend: drag-drop page, chat panel, mapping UI, validations.  
- Tests: unit (parsing), contract (API), UI smoke for drag-drop + chat.

## Phase 3: Generation & Preview (US3)

- Backend: lookalike template generation and preview rendering.  
- Frontend: preview viewer, save-as-template flow.

## Gates & Done Criteria

- US1/US2 independently demoable; US3 produces preview and save.  
- LLM latency p95 ≤ 2.5s; no secrets in client; dark-mode consistent UI.
