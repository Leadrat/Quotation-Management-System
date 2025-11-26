# Research: Import Templates (Chat + Drag & Drop)

## Decisions

- Decision: Variable syntax = `{{variable}}` with dot-paths and array notation (e.g., `{{items[i].description}}` with repeat sections handled by template engine)
  - Rationale: Human-readable, aligns with common templating, easy to scan in preview.
  - Alternatives: DOCX content controls; Liquid-style `{% %}` blocks.

- Decision: Parse limits = max 10MB per file, timeout 30s per parse stage
  - Rationale: Performance guardrails, matches Success Criteria; larger docs can be split.
  - Alternatives: Larger/streaming parsing; queued async processing (future).

- Decision: Branding strategy = preserve source formatting by default; optional brand pass later
  - Rationale: "lookalike" is primary goal; brand theming can be opt-in post-MVP.
  - Alternatives: Force TailAdmin typography/colors; migrate styles aggressively.

- Decision: Gemini model = `gemini-2.5-flash` via backend proxy
  - Rationale: User preference; latency target p95 ≤ 2.5s.
  - Alternatives: 1.5-pro for higher quality at higher latency.

## Patterns

- PDF parsing: attempt text layer via PDF parser; fallback to heuristic extraction; user-guided mapping resolves ambiguities.
- XLSX parsing: sheet selector + header inference; allow manual header selection.
- OpenXML fidelity: preserve table/grid layout; map placeholders inline; maintain margins and font sizes.

## Open Questions (tracked)

- None blocking MVP; questions in spec marked as clarified above.
