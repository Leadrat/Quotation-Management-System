# Data Model: Import Templates

## Entities

- ImportSession
  - ImportSessionId (UUID)
  - SourceType (enum: pdf, docx, xlsx, xslt, dotx)
  - SourceFileRef (string/blob ref)
  - Status (enum: Uploaded, Parsed, Mapped, Generated, Saved)
  - SuggestedMappings (json)
  - ConfirmedMappings (json)
  - CreatedBy (string)
  - CreatedAt (timestamp)
  - UpdatedAt (timestamp)

- Template
  - TemplateId (UUID)
  - Name (string)
  - Type (enum: quotation, invoice, generic)
  - ContentRef (blob/docx path)
  - Version (int)
  - CreatedBy, CreatedAt, UpdatedAt

- VariableCatalog (logical)
  - Namespaces: company, customer, identifiers, bank, items[], totals, dates, numbers

## Validation Rules

- Upload: type ∈ {pdf, docx, xlsx, xslt, dotx}, size ≤ 10MB
- Mapping: required fields company.name, customer.name; either items[] mapped or explicitly skipped; totals derived if tax rates provided.
- Status transitions: Uploaded→Parsed→Mapped→Generated→Saved
