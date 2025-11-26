# Quickstart: Import Templates

## Prereqs
- Backend env (.env):
  - LLM_PROVIDER=gemini
  - GEMINI_API_KEY=...
  - GEMINI_MODEL=gemini-2.5-flash
  - GEMINI_API_BASE=https://generativelanguage.googleapis.com

## Steps
1. Start backend API and frontend web.
2. Go to /imports/new.
3. Drag & drop a DOCX/XLSX; wait for parse preview.
4. Use chat to confirm mappings (company, customer, identifiers, items, taxes, totals).
5. Generate preview; verify placeholders & layout.
6. Save as template; check it appears in Templates list.

## Test Scenarios
- One-page DOCX with items; verify subtotal/tax/total calculation.
- XLSX table mapping; ensure correct column bindings.
- Error case: password-protected PDF → proper error message.
