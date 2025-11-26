# Feature Specification: Import Templates (Chat + Drag & Drop)

**Feature Branch**: `029-import-templates`  
**Created**: 2025-11-25  
**Status**: Draft  
**Input**: Chat-like interface where user drags & drops a document (pdf, docx, xlsx, xslt, dotx). The app parses content and, using existing variables (my company, customer, identifiers, bank details, invoice items/products/services, quantities, calculations, taxes, grand total, etc.), builds a lookalike Word merge template for user confirmation, then saves as a template.

## User Scenarios & Testing *(mandatory)*

<!--
  IMPORTANT: User stories should be PRIORITIZED as user journeys ordered by importance.
  Each user story/journey must be INDEPENDENTLY TESTABLE - meaning if you implement just ONE of them,
  you should still have a viable MVP (Minimum Viable Product) that delivers value.
  
  Assign priorities (P1, P2, P3, etc.) to each story, where P1 is the most critical.
  Think of each story as a standalone slice of functionality that can be:
  - Developed independently
  - Tested independently
  - Deployed independently
  - Demonstrated to users independently
-->

### User Story 1 - Drag & Drop and Parse (Priority: P1)

As a user, I can drag & drop a document (pdf, docx, xlsx, xslt, dotx) into a chat-like panel, see an upload confirmation, and the system parses detected text/tables and proposes variable mappings.

**Why this priority**: Entry point for conversion flow; must work before anything else.

**Independent Test**: Drop each supported file type; verify parsed preview and extracted fields appear without further steps.

**Acceptance Scenarios**:

1. Given a DOCX invoice, When I drop it, Then text and table regions are parsed and shown, with key fields highlighted and extracted.
2. Given an XLSX price table, When I drop it, Then rows/columns preview and column headers are available for mapping to `items` structure.

---

### User Story 2 - Variable Mapping Chat (Priority: P1)

As a user, I can chat to confirm/adjust mappings from parsed content to existing variables like company, customer, identifiers, bank, items (products/services, quantity, price), taxes, totals.

**Why this priority**: Ensures correctness of generated merge template variables.

**Independent Test**: Without generating a template, confirm mappings via chat prompts and UI toggles; validation catches missing critical mappings.

**Acceptance Scenarios**:

1. Given parsed headers "GSTIN" and "PO Number", When I map them to identifiers, Then the system reflects bindings to our standard variables.
2. Given a line items table, When I map columns to `items[].description`, `items[].qty`, `items[].unitPrice`, `items[].taxRate`, Then a calculated subtotal/tax/total preview appears.

---

### User Story 3 - Generate Lookalike Merge Template (Priority: P2)

As a user, I can preview a Word merge-like template that visually matches my source, using our established variable syntax for dynamic fields and repeating sections for items.

**Why this priority**: Produces the tangible output the business needs.

**Independent Test**: Generate preview only; verify placeholders and repeating sections are correctly placed and styled, without saving.

**Acceptance Scenarios**:

1. Given confirmed mappings, When I generate, Then the preview shows placeholders for company/customer data, identifiers, bank details, items table, taxes, and grand total.
2. Given a multi-page source, When generated, Then page layout and key formatting (headings, table borders) are preserved within reasonable fidelity.

---

### Edge Cases

- Password-protected or corrupted files → show descriptive error and guidance.
- Very large PDFs/XLSX (>10MB) → show progress, limit pages/sheets by prompt; allow user to select subset.
- Ambiguous mappings (e.g., multiple totals) → require user confirmation and highlight conflicts.
- Missing items table → allow template without items, warn user; or user links alternate source.
- Locale/formatting differences (currency, decimal separators, dates) → detect, preview in user locale.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001 Upload**: Support drag & drop and file picker for pdf, docx, xlsx, xslt, dotx with progress and basic validation (type, size).
- **FR-002 Parsing**: Extract text, headings, tables from input; for xlsx select sheet and header row; for pdf use text layer where available.
- **FR-003 Variable Catalog**: Provide a standard variable set: company, customer, identifiers, bank, items (description, qty, unitPrice, taxRate), totals (subtotal, tax, grandTotal), dates, invoice/quotation numbers.
- **FR-004 Mapping UX**: Chat-like flow with inline mapping UI to bind parsed fields/columns to variables; show live preview of computed totals.
- **FR-005 Validation**: Require minimal mappings to proceed: company.name, customer.name, at least one identifier, items mapping or explicit confirmation to skip, tax/total mapping or allow system calculation.
- **FR-006 Template Generation**: Produce a lookalike Word-merge template with placeholders and a repeating items table using our variable syntax.
- **FR-007 Preview**: Render a high-fidelity preview image/PDF of the template to verify layout before saving.
- **FR-008 Save**: On acceptance, save as a new template with metadata (name, type, createdBy, createdAt, sourceFileRef) and version 1.
- **FR-009 Reuse**: Allow re-opening and editing mappings to regenerate the template before final save.
- **FR-010 Audit**: Log mapping actions and final template creation for traceability.
- **FR-011 LLM Chat**: Chat-driven mapping and guidance MUST use Gemini LLM, invoked server-side (no client key exposure). Model, key, and base URL are environment-configured; default model: `gemini-2.5-flash`.

### Key Entities *(include if feature involves data)*

- **ImportSession**: SessionId, SourceType, SourceFileRef, Status, SuggestedMappings, ConfirmedMappings, CreatedBy, CreatedAt, UpdatedAt.
- **Template**: TemplateId, Name, Type, ContentRef/Blob, Version, CreatedBy, CreatedAt, UpdatedAt.
- **VariableCatalog**: Namespaces and keys for company, customer, identifiers, bank, items[*], totals, formatting rules.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A user can import and produce a preview in ≤ 90 seconds for a one-page DOCX/XLSX.
- **SC-002**: Variable mapping completeness check flags missing critical fields with clear guidance ≥ 95% of the time on test corpus.
- **SC-003**: Lookalike preview layout fidelity rated ≥ 80% by reviewers versus source document.
- **SC-004**: Saved templates render correct company, customer, items, and totals using existing data end-to-end in UAT scenarios.
- **SC-005**: Chat responses (LLM) have p95 latency ≤ 2.5s for typical prompts under normal load.

### Assumptions

- PDFs without text layer may need manual assistance; we provide best-effort parsing and allow manual mapping.
- Items repeating section uses our standard `items[]` variable shape.
- Taxes may be computed from mapped rates if explicit totals are absent.
- Gemini is the selected LLM provider for this feature; calls are proxied by our backend using environment variables (GEMINI_API_KEY, GEMINI_MODEL, GEMINI_API_BASE). No keys in frontend. Preferred default model: `gemini-2.5-flash` (overridable per env).

### [NEEDS CLARIFICATION]

1. Preferred variable syntax for the new merge templates (e.g., {{variable}} or existing DOCX content controls)?
2. Maximum file size/time for parsing before timing out? Default assumption: 10MB / 30s.
3. Should we auto-detect and apply brand fonts/colors to the generated template or preserve source exactly?
