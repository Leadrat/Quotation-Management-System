# Feature Specification: Document Template Upload & Conversion

**Feature Branch**: `001-template-upload`  
**Created**: 2025-01-27  
**Status**: Draft  
**Input**: User description: "User can upload a pdf or word document in to templete generation flow and the application should make it editable word template with placeholders for user's company details and customer's company details. There will be Quotations, Proforma Invoice templates."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Upload and Convert Document to Template (Priority: P1)

A user (Admin or SalesRep) uploads a PDF or Word document containing a quotation or proforma invoice format. The system analyzes the document, identifies company information (company name, address, city, country, postal code, bank account details, tax identifiers), and converts it into an editable Word template with placeholders replacing the identified company details. The converted template retains all original formatting, styles, fonts, colors, and layout.

**Why this priority**: This is the core functionality that enables users to transform existing documents into reusable templates. Without this, users cannot leverage their existing document formats, making this the highest priority.

**Independent Test**: Can be fully tested by uploading a Word document with company details, verifying the system identifies and replaces company information with placeholders, and confirming the output Word template is editable and preserves original styles. This delivers immediate value by enabling template creation from existing documents.

**Acceptance Scenarios**:

1. **Given** a user is on the template upload page, **When** they select a PDF or Word document file, **Then** the system accepts the file and displays a preview
2. **Given** a user has uploaded a Word document containing company name "ABC Corp", **When** the system processes the document, **Then** it identifies "ABC Corp" and replaces it with placeholder `{{CompanyName}}`
3. **Given** a user has uploaded a document containing a full address "123 Main St, New York, NY 10001, USA", **When** the system processes the document, **Then** it identifies the address components and replaces them with placeholders like `{{CompanyAddress}}`, `{{CompanyCity}}`, `{{CompanyState}}`, `{{CompanyPostalCode}}`, `{{CompanyCountry}}`
4. **Given** a user has uploaded a document containing bank account details "Account: 123456789, IFSC: ABCD0123456", **When** the system processes the document, **Then** it identifies bank details and replaces them with placeholders like `{{BankAccountNumber}}`, `{{BankIFSC}}`
5. **Given** a user has uploaded a document with company tax identifiers (PAN, TAN, GST), **When** the system processes the document, **Then** it identifies tax identifiers and replaces them with placeholders like `{{CompanyPAN}}`, `{{CompanyTAN}}`, `{{CompanyGST}}`
6. **Given** a user has uploaded a document with formatted text (bold, italic, colors, fonts), **When** the system converts it to a template, **Then** all formatting, styles, fonts, and colors are preserved in the output Word template
7. **Given** a user has uploaded a document, **When** the conversion completes successfully, **Then** the system provides a downloadable Word template file with placeholders
8. **Given** a user uploads an invalid file format, **When** they attempt to upload, **Then** the system displays an error message indicating only PDF and Word documents are supported

---

### User Story 2 - Identify Customer Company Details (Priority: P1)

When processing an uploaded document, the system identifies customer/client company information (customer company name, address, city, state, postal code, country, GSTIN) and replaces them with appropriate placeholders for customer details, distinguishing them from the user's company details.

**Why this priority**: Templates must support both company details (sender) and customer details (recipient) to be useful for quotations and invoices. This is essential for the template to function correctly when generating documents.

**Independent Test**: Can be fully tested by uploading a document containing both sender and customer company information, verifying the system correctly distinguishes between them, and confirming customer details are replaced with customer-specific placeholders. This delivers value by ensuring templates can handle both parties' information correctly.

**Acceptance Scenarios**:

1. **Given** a user uploads a document containing "Bill To: XYZ Company, 456 Oak Ave, Mumbai, MH 400001, India", **When** the system processes the document, **Then** it identifies this as customer information and replaces it with placeholders like `{{CustomerCompanyName}}`, `{{CustomerAddress}}`, `{{CustomerCity}}`, `{{CustomerState}}`, `{{CustomerPostalCode}}`, `{{CustomerCountry}}`
2. **Given** a user uploads a document with customer GSTIN "27ABCDE1234F1Z5", **When** the system processes the document, **Then** it identifies the GSTIN and replaces it with placeholder `{{CustomerGSTIN}}`
3. **Given** a user uploads a document with both sender and customer company details, **When** the system processes the document, **Then** it correctly distinguishes between sender (user's company) and customer company details and applies appropriate placeholders
4. **Given** a user uploads a document where customer details are not clearly labeled, **When** the system processes the document, **Then** it provides a review interface allowing the user to confirm which sections are customer vs company details

---

### User Story 3 - Template Type Selection and Categorization (Priority: P2)

When uploading a document, users can specify the template type (Quotation or Proforma Invoice) and provide metadata (name, description). The system categorizes and stores the template accordingly, making it available for use in the appropriate document generation workflows.

**Why this priority**: Templates need to be categorized and organized for easy retrieval and use. While the conversion functionality is more critical, template organization ensures users can efficiently find and use templates, making this important but secondary to core conversion.

**Independent Test**: Can be fully tested by uploading a document, selecting "Quotation" as template type, providing a name and description, and verifying the template is saved and appears in the Quotation templates list. This delivers value by enabling template organization and reuse.

**Acceptance Scenarios**:

1. **Given** a user has uploaded and converted a document, **When** they are prompted to save the template, **Then** they can select template type (Quotation or Proforma Invoice) from a dropdown
2. **Given** a user is saving a template, **When** they provide a template name and description, **Then** the system saves this metadata with the template
3. **Given** a user has saved a Quotation template, **When** they navigate to the Quotation templates list, **Then** the template appears in the list with the provided name and description
4. **Given** a user has saved a Proforma Invoice template, **When** they navigate to the Proforma Invoice templates list, **Then** the template appears in the list with the provided name and description
5. **Given** a user attempts to save a template without providing a name, **When** they click save, **Then** the system displays a validation error requiring a template name

---

### User Story 4 - Manual Review and Correction of Placeholder Identification (Priority: P2)

After the system automatically identifies and replaces company details with placeholders, users can review the converted template, manually adjust placeholder positions, add additional placeholders, or correct misidentified text before finalizing the template.

**Why this priority**: Automatic identification may not be 100% accurate, especially for complex documents or unusual formats. Allowing manual review and correction ensures users can create accurate templates even when automatic detection has limitations.

**Independent Test**: Can be fully tested by uploading a document, reviewing the automatically generated placeholders, manually editing placeholder text, and saving the corrected template. This delivers value by ensuring template accuracy and user control.

**Acceptance Scenarios**:

1. **Given** a user has uploaded and processed a document, **When** the conversion completes, **Then** the system displays a preview of the template with highlighted placeholders
2. **Given** a user is reviewing a converted template, **When** they click on a placeholder, **Then** they can edit the placeholder name or remove it
3. **Given** a user is reviewing a converted template, **When** they select text that wasn't identified as a placeholder, **Then** they can manually convert it to a placeholder
4. **Given** a user has made manual corrections to placeholders, **When** they save the template, **Then** the corrected template is saved with all manual adjustments

---

### User Story 5 - Apply Document Template During Quotation Creation (Priority: P1)

When a Sales Rep creates a quotation, they can select an uploaded document template (uploaded by Admin). Upon clicking "Apply", the system automatically converts the document to a template, identifies placeholders, and populates the quotation form with company details, bank details, and client information based on the identified placeholders.

**Why this priority**: This is the core workflow that enables Sales Reps to use uploaded documents for quotation generation. Without this, uploaded documents cannot be utilized in the quotation creation process, making this critical for the feature's value.

**Independent Test**: Can be fully tested by a Sales Rep creating a quotation, selecting an uploaded document, clicking apply, and verifying that the quotation form is automatically populated with company details, bank details, and client information. This delivers immediate value by streamlining quotation creation.

**Acceptance Scenarios**:

1. **Given** a Sales Rep is creating a quotation, **When** they select an uploaded document template from the list, **Then** the system displays the document preview and an "Apply Template" button
2. **Given** a Sales Rep has selected a document template, **When** they click "Apply Template", **Then** the system processes the document in the backend (converts to template, identifies placeholders)
3. **Given** the system has processed the document template, **When** placeholders are identified, **Then** the quotation form is automatically populated with:
   - Company details (name, address, phone, email, website) from Spec-022
   - Bank details (account name, account number, IFSC code, bank name) from Spec-022
   - Office addresses (multiple addresses if present in document) from Spec-022
   - Client details (company name, address, GSTIN) from selected client (Spec-006)
4. **Given** a Sales Rep has applied a document template, **When** the quotation form is populated, **Then** all identified placeholders are mapped to corresponding form fields
5. **Given** a document contains quotation-specific placeholders (e.g., `{{QuotationNumber}}`, `{{QuotationDate}}`, `{{LineItems}}`), **When** the template is applied, **Then** these placeholders are prepared for dynamic content during final generation

---

### User Story 6 - Generate Final Quotation Matching Document Structure (Priority: P1)

After a Sales Rep completes the quotation form and generates the quotation, the system creates a final quotation document that matches the structure, layout, formatting, and design of the original uploaded document. All placeholders are replaced with actual data, and the document is available for client viewing and PDF download.

**Why this priority**: The final output must match the uploaded document's appearance to maintain brand consistency and professional presentation. This is essential for client-facing documents and ensures the feature delivers complete value.

**Independent Test**: Can be fully tested by creating a quotation with an applied template, generating the final document, and verifying it matches the original document structure with all placeholders replaced by actual data. This delivers value by producing professional, branded quotation documents.

**Acceptance Scenarios**:

1. **Given** a Sales Rep has created a quotation with an applied document template, **When** they complete the quotation form and click "Generate Quotation", **Then** the system processes the template and replaces all placeholders with actual data
2. **Given** the system is generating the final quotation, **When** placeholders are replaced, **Then** the following data is inserted:
   - `{{CompanyName}}` → User's company name from Spec-022
   - `{{CompanyAddress}}` → User's company address from Spec-022
   - `{{CompanyPhone}}` → User's company phone from Spec-022
   - `{{CompanyEmail}}` → User's company email from Spec-022
   - `{{CompanyWebsite}}` → User's company website from Spec-022
   - `{{BankAccountName}}` → Bank account name from Spec-022
   - `{{BankAccountNumber}}` → Bank account number from Spec-022
   - `{{BankIFSC}}` → IFSC code from Spec-022
   - `{{BankName}}` → Bank name from Spec-022
   - `{{OfficeAddress1}}`, `{{OfficeAddress2}}`, `{{OfficeAddress3}}` → Office addresses from Spec-022
   - `{{CustomerCompanyName}}` → Selected client's company name from Spec-006
   - `{{CustomerAddress}}` → Selected client's address from Spec-006
   - `{{CustomerGSTIN}}` → Selected client's GSTIN from Spec-006
   - `{{QuotationNumber}}` → Generated quotation number
   - `{{QuotationDate}}` → Quotation creation date
   - `{{LineItems}}` → Quotation line items with pricing, taxes, totals
3. **Given** the final quotation is generated, **When** the document is created, **Then** all original formatting is preserved (fonts, colors, styles, layout, tables, images, QR codes, headers, footers)
4. **Given** a quotation has been generated, **When** a client accesses the quotation via web page, **Then** they see the quotation matching the original document structure with all data filled in
5. **Given** a client is viewing a quotation, **When** they click "Download PDF", **Then** a PDF version of the quotation is generated and downloaded, matching the original document layout
6. **Given** the original document contains complex elements (tables, images, QR codes, multi-column layout), **When** the final quotation is generated, **Then** all these elements are preserved and properly formatted with actual data

---

### Edge Cases

- What happens when a document contains no identifiable company information? (System should still convert the document to a template, allowing users to manually add placeholders)
- How does the system handle documents with multiple company names or addresses? (System should identify all instances and allow user to specify which is company vs customer)
- What happens when a document has corrupted formatting or unsupported styles? (System should preserve what it can and log warnings for unsupported elements)
- How does the system handle very large documents (e.g., >50MB)? (System should validate file size and reject or process with size limits)
- What happens when a document contains images, tables, or complex layouts? (System should preserve these elements while identifying text placeholders)
- How does the system handle documents in languages other than English? (System should support identification of company details in common languages, with fallback to manual placeholder creation)
- What happens when bank account details use different formats or terminology? (System should recognize common variations and patterns, with manual correction option)
- How does the system handle documents that are already in template format with placeholders? (System should detect existing placeholders and preserve them, only identifying and replacing actual company data)
- What happens when a user uploads a password-protected document? (System should prompt for password or reject with clear error message)
- How does the system handle documents with embedded fonts or special characters? (System should preserve fonts and handle special characters appropriately)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow users (Admin and SalesRep) to upload PDF documents for template conversion
- **FR-002**: System MUST allow users (Admin and SalesRep) to upload Word documents (.doc, .docx) for template conversion
- **FR-003**: System MUST validate uploaded files are PDF or Word format before processing
- **FR-004**: System MUST validate uploaded file size does not exceed maximum allowed size
- **FR-005**: System MUST analyze uploaded documents to identify company name patterns
- **FR-006**: System MUST analyze uploaded documents to identify company address information (street address, city, state, postal code, country)
- **FR-007**: System MUST analyze uploaded documents to identify company postal/pin codes
- **FR-008**: System MUST analyze uploaded documents to identify bank account details (account number, IFSC code, IBAN, SWIFT code, bank name, branch name)
- **FR-009**: System MUST analyze uploaded documents to identify company tax identifiers (PAN, TAN, GST numbers)
- **FR-010**: System MUST analyze uploaded documents to identify company identifiers (registration numbers, license numbers, etc.)
- **FR-011**: System MUST replace identified user company details with placeholders (e.g., `{{CompanyName}}`, `{{CompanyAddress}}`, `{{CompanyCity}}`, `{{CompanyState}}`, `{{CompanyPostalCode}}`, `{{CompanyCountry}}`, `{{CompanyPAN}}`, `{{CompanyTAN}}`, `{{CompanyGST}}`, `{{BankAccountNumber}}`, `{{BankIFSC}}`, `{{BankIBAN}}`, `{{BankSWIFT}}`, `{{BankName}}`, `{{BankBranch}}`)
- **FR-012**: System MUST replace identified customer company details with placeholders (e.g., `{{CustomerCompanyName}}`, `{{CustomerAddress}}`, `{{CustomerCity}}`, `{{CustomerState}}`, `{{CustomerPostalCode}}`, `{{CustomerCountry}}`, `{{CustomerGSTIN}}`)
- **FR-013**: System MUST preserve all original document formatting including fonts, font sizes, colors, bold, italic, underline, alignment, spacing, and paragraph styles
- **FR-014**: System MUST preserve document layout including margins, headers, footers, page breaks, and section breaks
- **FR-015**: System MUST preserve images, tables, charts, and other non-text elements in the converted template
- **FR-016**: System MUST generate an editable Word document (.docx) as output
- **FR-017**: System MUST allow users to specify template type (Quotation or Proforma Invoice) during template creation
- **FR-018**: System MUST allow users to provide template name and description during template creation
- **FR-019**: System MUST provide a preview interface showing the converted template with highlighted placeholders
- **FR-020**: System MUST allow users to manually edit placeholder names in the preview interface
- **FR-021**: System MUST allow users to manually add placeholders by selecting text in the preview
- **FR-022**: System MUST allow users to remove incorrectly identified placeholders
- **FR-023**: System MUST distinguish between user's company details and customer company details in the document
- **FR-024**: System MUST handle documents that are already in template format by preserving existing placeholders
- **FR-025**: System MUST provide clear error messages when file upload fails or file format is invalid
- **FR-026**: System MUST save converted templates with all placeholders and formatting intact
- **FR-027**: System MUST make saved templates available in the appropriate template list (Quotation or Proforma Invoice) based on template type
- **FR-028**: System MUST allow Sales Reps to select and apply uploaded document templates during quotation creation
- **FR-029**: System MUST automatically convert uploaded documents to templates when Sales Rep applies them during quotation creation
- **FR-030**: System MUST automatically populate quotation form fields when a document template is applied, mapping placeholders to form fields
- **FR-031**: System MUST populate company details (name, address, phone, email, website) in quotation form from Spec-022 when template is applied
- **FR-032**: System MUST populate bank details (account name, account number, IFSC code, bank name) in quotation form from Spec-022 when template is applied
- **FR-033**: System MUST populate office addresses (multiple addresses if present) in quotation form from Spec-022 when template is applied
- **FR-034**: System MUST populate client details (company name, address, GSTIN) in quotation form from selected client (Spec-006) when template is applied
- **FR-035**: System MUST generate final quotation document matching the original uploaded document structure, layout, and formatting
- **FR-036**: System MUST replace all placeholders in template with actual data when generating final quotation (company details, bank details, client details, quotation data)
- **FR-037**: System MUST preserve all document elements in final quotation (tables, images, QR codes, headers, footers, multi-column layouts)
- **FR-038**: System MUST make generated quotations available for client viewing on web page matching the document structure
- **FR-039**: System MUST provide PDF download functionality for generated quotations, maintaining original document formatting
- **FR-040**: System MUST support quotation-specific placeholders (QuotationNumber, QuotationDate, LineItems, SubTotal, TaxAmount, TotalAmount) in document templates

### Key Entities *(include if feature involves data)*

- **DocumentTemplate**: Represents a converted template with placeholders, including template ID, name, description, template type (Quotation/Proforma Invoice), file reference, original file name, created date, updated date, and creator user ID
- **TemplatePlaceholder**: Represents identified placeholders in a template, including placeholder name, placeholder type (company/customer/quotation), position/location in document, mapped form field name, and associated metadata
- **QuotationTemplateMapping**: Represents the mapping between template placeholders and quotation form fields, including placeholder name, form field name, data source (CompanyDetails, BankDetails, Client, Quotation), and default value

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can successfully convert 90% of uploaded PDF and Word documents to editable Word templates with placeholders on first attempt
- **SC-002**: System correctly identifies and replaces company details (name, address, city, country, postal code, bank details, tax identifiers) in 85% of uploaded documents without manual intervention
- **SC-003**: Converted templates preserve 100% of original formatting (fonts, colors, styles, layout) as verified by visual comparison
- **SC-004**: Users can complete the template upload and conversion process (upload → conversion → review → save) in under 5 minutes for documents under 10 pages
- **SC-005**: System processes and converts documents up to 50 pages in under 2 minutes
- **SC-006**: 95% of users who upload templates successfully save and use them for generating quotations or proforma invoices
- **SC-007**: System handles documents with complex layouts (tables, images, multi-column) while preserving structure in 90% of cases
- **SC-008**: Users report 80% satisfaction with automatic placeholder identification accuracy
- **SC-009**: Sales Reps can apply document templates and populate quotation forms automatically in under 10 seconds
- **SC-010**: Final quotation documents match original uploaded document structure and formatting in 100% of cases
- **SC-011**: All placeholders are correctly replaced with actual data in 95% of generated quotations
- **SC-012**: Clients can view and download PDF quotations matching document structure in under 3 seconds

## Assumptions

- Users have existing PDF or Word documents they want to convert to templates
- Documents contain identifiable company information in standard formats
- Users are familiar with basic template concepts and placeholders
- The system has access to company details configuration (from Spec-022) to understand what placeholders should be available
- The system has access to client/customer data structure (from Spec-006) to understand customer detail placeholders
- Word documents may contain macros, but macros will not be preserved in converted templates (only formatting and content)
- PDF documents may be scanned images, but OCR capabilities are assumed for text extraction
- Users will review and correct automatically identified placeholders when needed
- Template storage and management infrastructure exists (from Spec-011: Quotation Template Management)

## Dependencies

- **Spec-006**: Client Entity (for customer company detail structure and client data)
- **Spec-009**: Quotation Entity & CRUD Operations (for quotation data structure and line items)
- **Spec-010**: Quotation Management (for quotation generation and client sharing)
- **Spec-011**: Quotation Template Management (for template storage and management)
- **Spec-022**: Company Details Admin Configuration (for user company detail structure, bank details, and available placeholders)

## Out of Scope

- Automatic generation of templates from scratch (templates must be uploaded)
- Support for other document formats (Excel, PowerPoint, etc.)
- Real-time collaborative editing of templates
- Version control for templates (handled by Spec-011)
- Template approval workflows (handled by Spec-011)
- Automatic translation of placeholders to other languages
- Support for complex document structures like forms with fillable fields
- Integration with external document storage services beyond basic file upload
- Manual template editing by Sales Reps (templates are applied automatically during quotation creation)
- Template preview before application (conversion happens automatically on apply)
