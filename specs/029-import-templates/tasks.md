# Tasks: Import Templates (Chat + Drag & Drop)

Feature: specs/029-import-templates/spec.md  
Branch: 029-import-templates

Dependencies order by User Story: US1 (P1) → US2 (P1) → US3 (P2)

Parallel opportunities noted with [P].

## Phase 1: Setup

- [X] T001 Create Import feature folders per plan in src/Backend and src/Frontend/web/src  
- [X] T002 Add env placeholders for Gemini in .env.example (done) and document in quickstart.md  
- [X] T003 [P] Add route scaffolds: /imports/new and /imports/session/[id] in src/Frontend/web/src/app/(protected)/imports  
- [X] T004 [P] Create API client file src/Frontend/web/src/lib/api/imports.ts  

## Phase 2: Foundational (Blocking)

- [X] T005 Backend Domain: entities ImportSession, Template in src/Backend/CRM.Domain/Imports/*.cs  
- [X] T006 Backend Infrastructure: EF configs + DbSet in CRM.Infrastructure  
- [X] T007 Create DB migration for ImportSessions and Templates  
- [ ] T008 Storage integration for uploaded source files (local/filesystem placeholder)  
- [ ] T009 [P] Application DTOs for upload, chat, mappings in CRM.Application/Imports/Dtos/*.cs  
- [ ] T010 [P] Mapping + validation services in CRM.Application/Imports/Services/*.cs  
- [ ] T011 [P] Configure Gemini client (server-side) reading GEMINI_* env in CRM.Application/Imports/LLM/GeminiClient.cs  

## Phase 3: [US1] Drag & Drop and Parse (P1)

- [X] T012 [US1] API: POST /api/imports (upload & create session) in CRM.Api/Controllers/ImportTemplatesController.cs  
- [X] T013 [US1] Parse pipeline for docx/pdf/xlsx and store SuggestedMappings preview in CRM.Application/Imports/Services/ParseService.cs  
- [X] T014 [US1] Frontend drag-drop UI in src/Frontend/web/src/app/(protected)/imports/new/page.tsx  
- [X] T015 [US1] Frontend preview of parsed text/tables and regions in components/imports/ParsedPreview.tsx  
- [X] T016 [US1] UI error/loading states and file size/type validation  

## Phase 4: [US2] Variable Mapping Chat (P1)

- [X] T017 [US2] API: POST /api/imports/{id}/chat (Gemini proxy) in CRM.Api/Controllers/ImportChatController.cs  
- [X] T018 [US2] API: POST /api/imports/{id}/mappings (save confirmed mappings) in ImportTemplatesController.cs  
- [X] T019 [US2] Frontend chat panel and message list in components/imports/ChatPanel.tsx  
- [X] T020 [US2] Mapping UI with inline variable pickers in components/imports/MappingEditor.tsx  
- [X] T021 [US2] Client validations for required mappings and totals preview  

## Phase 5: [US3] Generate Lookalike Template (P2)

- [X] T022 [US3] API: POST /api/imports/{id}/generate to produce lookalike template in ImportTemplatesController.cs  
- [X] T023 [US3] API: GET /api/imports/{id}/preview to fetch preview image/PDF  
- [X] T024 [US3] Frontend preview viewer in components/imports/TemplatePreview.tsx  
- [X] T025 [US3] API: POST /api/imports/{id}/save-template to persist template  
- [X] T026 [US3] Frontend save flow from preview and route to Templates list  

## Final Phase: Polish & Cross-Cutting

- [ ] T027 Accessibility + dark mode consistency for import pages/components  
- [ ] T028 Contract docs update at specs/029-import-templates/contracts/openapi.yaml  
- [ ] T029 Sample files for UAT and seed scripts  
- [ ] T030 Add tests: unit (parsing, mapping), integration (API), UI smoke (drag-drop, chat, preview)  
- [ ] T031 Push branch and open PR with links to spec, plan, tasks  

## Parallel Execution Examples

- T003/T004 can run in parallel with backend T005–T011.  
- T017 (chat) and T018 (mappings save) can develop in parallel after T012.  
- T022 (generate) and T023 (preview) can be developed in parallel.

## MVP Scope

- Complete US1 + US2: upload/parse + chat mapping + save mappings.

## Independent Test Criteria per Story

- US1: Drag-drop creates session; parsed preview shows text/tables; errors are clear.  
- US2: Chat suggestions guide mapping; required fields validated; totals preview correct.  
- US3: Generated template preview matches layout; saved template is reusable.
