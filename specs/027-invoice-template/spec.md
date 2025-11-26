# Feature Specification: Invoice/Quote Template from Uploaded Document

**Feature Branch**: `027-invoice-template`  
**Created**: 2025-11-24  
**Status**: Draft  
**Input**: User description: "Nextjs + typescript code that allows me to upload a docx or pdf which as invoice or quote and we need to replicate the same thing with dotX template with placeholders that we can use for inserting data in realtime with json. I need both webview and pdf download. I should be able to add multiple line items - subscriptions, add-ons, services to the invoice or quote. Backend with be .net core ,  and do this for all the roles."

## Clarifications

### Session 2025-11-24

- Q: How should roles and permissions for this feature be handled? → A: Reuse existing CRM roles and map feature actions to them (Admin, Sales Rep, Manager), with Admin and Sales Rep both able to upload/manage templates and configure placeholders, and Manager focused on review and history.
- Q: How long should templates and generated documents be retained? → A: Keep templates until manually archived/deleted; retain generated documents for a fixed audit period (for example, 7 years).
- Q: What performance target should apply for rendering a document from a template and data? → A: For typical documents, rendering to web view should complete within 3 seconds.

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

### User Story 1 - Upload invoice/quote and generate reusable template (Priority: P1)

Sales and operations users need to upload an existing invoice or quote document and convert it into a reusable template with placeholders, so that future invoices/quotes can be generated consistently from structured data instead of manually editing documents.

**Why this priority**: This is the foundation of the feature. Without reliable upload and template extraction, no automated generation or downstream flows are possible.

**Independent Test**: Can be fully tested by uploading various invoice/quote files and verifying that a corresponding template is created, with detected placeholders for header fields, parties, totals, and line items.

**Acceptance Scenarios**:

1. **Given** a user with permission to manage templates, **When** they upload a supported invoice/quote file, **Then** the system creates a new template record and shows a preview of the document structure.
2. **Given** a user uploading an unsupported or corrupted file, **When** the upload completes, **Then** the system clearly informs the user that the file cannot be processed and does not create a template.

---

### User Story 2 - Configure placeholders for template fields and line items (Priority: P1)

Template managers need to define and adjust placeholders in the uploaded document (e.g., customer details, totals, taxes, line item fields) so that the system can merge JSON data into the right locations for future invoices and quotes.

**Why this priority**: Correct and flexible placeholder mapping is critical for data-driven document generation and to keep templates maintainable as business requirements evolve.

**Independent Test**: Can be fully tested by opening a template, configuring placeholders, and validating that sample data is rendered in the correct positions in the preview.

**Acceptance Scenarios**:

1. **Given** a created template, **When** the user opens a placeholder configuration view, **Then** they can see and edit mappings for header fields, recipient details, payment terms, totals, and line item fields.
2. **Given** a template with configured placeholders, **When** the user applies sample JSON data, **Then** the preview shows all placeholder-driven values in the correct places or clearly highlights any unmapped fields.

---

### User Story 3 - Generate and view invoice/quote from JSON data (Priority: P1)

Sales users need to generate an invoice or quote instance by selecting a template and providing structured data (including multiple line items), and then view the result in the browser before sending or downloading.

**Why this priority**: This delivers the primary business value: quickly generating accurate invoices/quotes from CRM data without manual copy-paste.

**Independent Test**: Can be fully tested by selecting a template, supplying JSON data, and verifying that the rendered document matches expectations in a browser view.

**Acceptance Scenarios**:

1. **Given** an approved template and valid JSON data, **When** the user generates a document, **Then** the system renders a complete invoice/quote in a web view, including all line items and totals.
2. **Given** invalid or incomplete data, **When** the user attempts to generate a document, **Then** the system clearly indicates missing or invalid fields and prevents generating a final version until issues are resolved.

---

### User Story 4 - Download invoice/quote as PDF (Priority: P2)

Users need to download a finalized invoice/quote instance as a PDF file so that it can be shared externally or archived.

**Why this priority**: PDF is the primary format used for sending invoices/quotes to customers and for internal record-keeping.

**Independent Test**: Can be fully tested by generating a document and downloading a PDF, then visually and functionally validating it (layout, line items, totals, and metadata).

**Acceptance Scenarios**:

1. **Given** a generated invoice/quote in web view, **When** the user chooses to download as PDF, **Then** the system produces a PDF file matching the on-screen layout and values.
2. **Given** a previously generated document, **When** the user reopens it from history and downloads the PDF, **Then** the system returns the same version that was originally generated.

---

### User Story 5 - Manage line items with different types (Priority: P2)

Users need to add, edit, and remove multiple line items of different types (e.g., subscriptions, add-ons, one-time services), with quantity, pricing, discounts, and tax handling, so totals are calculated consistently.

**Why this priority**: Many quotes and invoices involve complex pricing structures; the system must support these structures without manual recalculation.

**Independent Test**: Can be fully tested by configuring line items of different types and verifying that subtotals, discounts, and taxes are calculated and displayed correctly.

**Acceptance Scenarios**:

1. **Given** a template that supports multiple line item types, **When** the user adds subscription, add-on, and service lines with quantities and prices, **Then** the system displays line totals and aggregated totals correctly.
2. **Given** existing line items, **When** the user updates or removes a line, **Then** the system recalculates totals in real time and updates the preview.

---

### User Story 6 - Role-based access and permissions (Priority: P2)

Different user roles (Admin, Sales Rep, Manager) need tailored access to upload documents, manage templates, configure placeholders, generate documents, view history, and download PDFs.

**Why this priority**: Ensures that only authorized users can change templates, while broader roles can safely generate and share documents.

**Independent Test**: Can be fully tested by logging in as users with different roles and verifying available actions and access to templates/documents.

**Acceptance Scenarios**:

1. **Given** an Admin user, **When** they access the system, **Then** they can upload new documents, configure placeholders, manage templates, and view full history.
2. **Given** a Sales Rep user, **When** they access the system, **Then** they can upload new documents, configure placeholders, manage templates, generate documents, and download PDFs, and view history for their templates and quotations.
3. **Given** a Manager user, **When** they access the system, **Then** they can generate and download documents and view history, but may not change template structure unless explicitly granted Admin capabilities.

---

### User Story 7 - Audit trail and history (Priority: P3)

Finance and compliance users need an overview of generated invoices/quotes, including who generated them, when, and from which template and data, to support audits and traceability.

**Why this priority**: Supports compliance and troubleshooting when customers question an invoice/quote.

**Independent Test**: Can be fully tested by generating several documents and then reviewing the history to confirm correct recording and filtering.

**Acceptance Scenarios**:

1. **Given** multiple generated documents, **When** a user opens the history view and filters by template, date range, or customer, **Then** the correct list of documents is shown.
2. **Given** a specific generated document, **When** a user views its details, **Then** they see key metadata such as template used, input data summary, creator, and timestamps.

---

### Edge Cases

- What happens when an uploaded document has a structure that cannot be reliably mapped to placeholders (e.g., scanned image, unusual layout)?
- How does the system handle very large documents or a very high number of line items (e.g., performance, usability of the preview)?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow authorized users to upload invoice/quote documents in supported formats and create a corresponding reusable template record.
- **FR-002**: System MUST analyze uploaded documents and support configuring placeholders for key fields (e.g., parties, dates, totals, payment terms) and line item sections.
- **FR-003**: System MUST support defining placeholders for multiple line item types (e.g., subscriptions, add-ons, services) including quantity, unit price, discounts, and taxes.
- **FR-004**: System MUST accept structured data representing an invoice/quote and merge it into a selected template to produce a rendered document.
- **FR-005**: System MUST provide a browser-based preview of the rendered document before it is finalized or downloaded.
- **FR-006**: System MUST allow users to download a rendered invoice/quote as a PDF file that visually matches the approved template.
- **FR-007**: System MUST store and expose a history of generated documents, including metadata such as template used, creator, timestamps, and key identifiers.
- **FR-008**: System MUST enforce role-based access so that only permitted roles can upload documents, manage templates, generate documents, and download or view history.
- **FR-009**: System SHOULD handle validation errors in uploaded documents and input data with clear, actionable error messages to the user.
- **FR-010**: System SHOULD support versioning of templates so that changes do not retroactively alter documents already generated from older versions.
- **FR-011**: System MUST provide a way to test templates with sample data before they are made available to general users.
- **FR-012**: System MUST ensure that generated documents remain consistent and readable across supported viewing and downloading flows.
- **FR-013**: System MUST reuse existing CRM roles (Admin, Sales Rep, Manager) and clearly document which actions each role can perform within this feature (e.g., Admin: upload/manage templates, configure placeholders, view full history; Sales Rep: upload/manage templates for their work, configure placeholders, generate/download documents, view history; Manager: review and view history, generate/download documents, but no template structure changes by default).
- **FR-014**: System MUST keep templates until they are manually archived or deleted and retain generated documents for a defined audit period (e.g., 7 years), after which they are archived or deleted according to organizational policy.

### Key Entities *(include if feature involves data)*

- **Template**: Represents a reusable invoice/quote layout derived from an uploaded document. Key attributes include identifier, name, type (invoice/quote), status (draft/active/deprecated), version, supported line item types, and placeholder definitions.
- **Generated Document**: Represents a specific invoice/quote instance created from a template and structured data. Key attributes include identifier, template reference, data snapshot, generation timestamp, status (draft/final), and links to stored outputs (e.g., viewable document, downloadable file).
- **Line Item**: Represents a single priced element within a generated document or its source data. Key attributes include type (subscription/add-on/service/other), description, quantity, unit price, discount, tax information, and line total.
- **User Role**: Represents how this feature maps onto the existing CRM role model, specifically Admin, Sales Rep, and Manager. Key attributes include role name and allowed actions in this feature, such as: Admin (upload/manage templates, configure placeholders, view full history), Sales Rep (upload/manage templates for their work, configure placeholders, generate/download documents, view history), Manager (generate/download documents and view history, but no template structure changes by default).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: At least 90% of users with appropriate permissions can successfully upload a valid invoice/quote document and create a template on their first attempt without assistance.
- **SC-002**: For typical documents (up to an agreed size and number of line items), the time from selecting a template and data to seeing a rendered web view does not exceed 3 seconds.
- **SC-003**: At least 90% of generated invoices/quotes pass internal finance review without needing manual layout or calculation corrections.
- **SC-004**: The volume of support tickets or internal complaints related to invoice/quote formatting or calculation errors reduces by at least 50% within a defined period after rollout.
