# Feature Specification: Company Details Admin Configuration & Quotation Integration

**Feature Branch**: `022-company-details-admin`  
**Created**: 2025-01-27  
**Status**: Draft  
**Input**: User description: "Company Details Admin Configuration & Quotation Integration"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Admin Configures Company Details (Priority: P1)

An admin user accesses a dedicated "Company Details" configuration page to enter and manage all company information including tax identification numbers (PAN, TAN, GST), banking details for India and Dubai, company address, contact information, legal disclaimers, and company logo. The admin can update any field, preview the logo, and save changes with confirmation. All changes are immediately available for use in quotations.

**Why this priority**: This is the foundation that enables all other functionality. Without company details configuration, quotations cannot include accurate company information, making this the highest priority.

**Independent Test**: Can be fully tested by an admin user navigating to the Company Details page, entering all required information, uploading a logo, and verifying the data is saved and persisted. This delivers immediate value by centralizing company information management.

**Acceptance Scenarios**:

1. **Given** an admin user is logged in, **When** they navigate to the Company Details configuration page, **Then** they see a form with sections for tax information, bank details (India and Dubai), company address, contact information, branding, and legal disclaimers
2. **Given** an admin user is on the Company Details page, **When** they enter PAN, TAN, and GST numbers, **Then** the system validates the format of each number according to Indian tax regulations
3. **Given** an admin user is entering bank details, **When** they select India as the country, **Then** they see fields for Account Number, IFSC Code, Bank Name, and Branch Name
4. **Given** an admin user is entering bank details, **When** they select Dubai as the country, **Then** they see fields for Account Number, IBAN, SWIFT Code, Bank Name, and Branch Name
5. **Given** an admin user has entered company details, **When** they click save, **Then** a confirmation modal appears asking them to confirm the changes
6. **Given** an admin user confirms saving company details, **When** the save operation completes, **Then** they see a success message and the updated information is persisted
7. **Given** an admin user uploads a company logo, **When** the upload completes, **Then** they see a preview of the logo and can replace it if needed
8. **Given** a non-admin user attempts to access the Company Details page, **When** they navigate to the page, **Then** they are denied access and redirected appropriately

---

### User Story 2 - Company Details Appear in Quotations (Priority: P1)

When a sales representative creates a quotation, the system automatically includes all configured company details (PAN, TAN, GST, company address, contact info, logo) in the quotation document. The quotation displays the bank details corresponding to the client's country (India or Dubai), ensuring clients receive accurate payment instructions for their location.

**Why this priority**: This is the core value proposition - quotations must include accurate company information to be legally compliant and professionally presented. This must work correctly from the first quotation created after company details are configured.

**Independent Test**: Can be fully tested by configuring company details, creating a quotation for a client in India, verifying India bank details appear, then creating a quotation for a Dubai client and verifying Dubai bank details appear. This delivers immediate value by ensuring quotations are complete and accurate.

**Acceptance Scenarios**:

1. **Given** company details are configured with PAN, TAN, GST, and bank details for India and Dubai, **When** a sales rep creates a quotation for a client in India, **Then** the quotation includes PAN, TAN, GST numbers and India bank details (Account Number, IFSC Code, Bank Name, Branch Name)
2. **Given** company details are configured with PAN, TAN, GST, and bank details for India and Dubai, **When** a sales rep creates a quotation for a client in Dubai, **Then** the quotation includes PAN, TAN, GST numbers and Dubai bank details (Account Number, IBAN, SWIFT Code, Bank Name, Branch Name)
3. **Given** a company logo is configured, **When** a sales rep views or generates a quotation PDF, **Then** the company logo appears in the quotation header
4. **Given** company address and contact information are configured, **When** a sales rep views or generates a quotation PDF, **Then** the company address and contact details appear in the quotation header
5. **Given** legal disclaimers are configured, **When** a sales rep views or generates a quotation PDF, **Then** the legal disclaimers appear in the quotation footer
6. **Given** company details are updated by an admin, **When** a sales rep creates a new quotation after the update, **Then** the new quotation reflects the updated company details
7. **Given** a quotation was created before company details were updated, **When** the quotation is viewed, **Then** it displays the company details that were current at the time of quotation creation (historical accuracy)

---

### User Story 3 - Company Details in Email Notifications (Priority: P2)

When quotations are sent via email to clients, the email includes company details (logo, address, contact information) in the email template, and the attached PDF quotation includes all company details as specified in User Story 2.

**Why this priority**: Email is a primary communication channel for quotations. Including company branding and details in emails maintains professional consistency, but this can be implemented after core quotation integration is working.

**Independent Test**: Can be fully tested by configuring company details, creating a quotation, sending it via email, and verifying the email contains company branding and the PDF attachment includes all company details. This delivers value by ensuring consistent branding across all client communications.

**Acceptance Scenarios**:

1. **Given** company details including logo and contact information are configured, **When** a sales rep sends a quotation via email, **Then** the email template includes the company logo and contact information
2. **Given** a quotation PDF is attached to an email, **When** the email is sent, **Then** the PDF includes all company details as specified in User Story 2
3. **Given** company details are updated, **When** new quotations are sent via email after the update, **Then** the emails reflect the updated company details

---

### Edge Cases

- What happens when company details are not yet configured and a sales rep tries to create a quotation? (System should handle gracefully - either show placeholders or prevent quotation creation until details are configured)
- How does the system handle invalid PAN, TAN, or GST number formats? (System should validate and show clear error messages)
- What happens when a client's country is not India or Dubai? (System should either show a default bank detail set or allow admin to configure additional countries)
- How does the system handle logo upload failures or invalid image formats? (System should validate file type and size, show clear error messages)
- What happens when an admin tries to save company details with missing required fields? (System should validate and prevent save, showing which fields are required)
- How does the system handle concurrent updates to company details by multiple admins? (System should use appropriate concurrency control to prevent data loss)
- What happens when bank details for a country are not configured but a quotation is created for a client in that country? (System should either show a warning or use default/empty bank details)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide an admin-only configuration page titled "Company Details" for managing company information
- **FR-002**: System MUST allow admins to enter and update PAN Number (Permanent Account Number) with format validation
- **FR-003**: System MUST allow admins to enter and update TAN Number (Tax Deduction and Collection Account Number) with format validation
- **FR-004**: System MUST allow admins to enter and update GST Number with format validation (if applicable)
- **FR-005**: System MUST allow admins to configure bank details for India including Account Number, IFSC Code, Bank Name, and Branch Name
- **FR-006**: System MUST allow admins to configure bank details for Dubai including Account Number, IBAN, SWIFT Code, Bank Name, and Branch Name
- **FR-007**: System MUST allow admins to enter and update company address information
- **FR-008**: System MUST allow admins to enter and update company contact information
- **FR-009**: System MUST allow admins to upload and update company logo with image preview
- **FR-010**: System MUST allow admins to enter and update legal disclaimers for quotations
- **FR-011**: System MUST validate all tax identification numbers (PAN, TAN, GST) according to Indian tax regulations
- **FR-012**: System MUST require admin confirmation before saving changes to company details
- **FR-013**: System MUST restrict access to Company Details configuration page to admin role only
- **FR-014**: System MUST automatically include configured company details (PAN, TAN, GST, address, contact, logo) in all quotation documents
- **FR-015**: System MUST dynamically include India bank details in quotations when the client's country is India
- **FR-016**: System MUST dynamically include Dubai bank details in quotations when the client's country is Dubai
- **FR-017**: System MUST include company logo in quotation PDF documents when logo is configured
- **FR-018**: System MUST include company address and contact information in quotation PDF documents
- **FR-019**: System MUST include legal disclaimers in quotation PDF documents when configured
- **FR-020**: System MUST include company details in quotation email templates when quotations are sent via email
- **FR-021**: System MUST preserve historical accuracy - quotations created before company details updates must display the company details that were current at creation time
- **FR-022**: System MUST reflect updated company details in all new quotations created after an update
- **FR-023**: System MUST log all changes to company details for audit purposes
- **FR-024**: System MUST validate file type and size for company logo uploads
- **FR-025**: System MUST handle cases where company details are not yet configured gracefully (show appropriate messages or prevent quotation creation)

### Key Entities *(include if feature involves data)*

- **Company Details**: Represents the centralized company information configuration including tax identification numbers (PAN, TAN, GST), banking information for multiple countries (India, Dubai), company address, contact information, legal disclaimers, and branding assets (logo). This is a singleton entity (one record per system instance) that can be updated by admins and is referenced by all quotations.

- **Bank Details**: Represents country-specific banking information associated with Company Details. Each bank detail record includes country identifier, account number, and country-specific fields (IFSC Code for India, IBAN and SWIFT Code for Dubai), bank name, and branch name. Multiple bank detail records can exist for different countries within a single Company Details configuration.

- **Quotation**: Represents a quotation document that references Company Details to include company information. The quotation determines which country-specific bank details to display based on the client's country. The quotation may store a snapshot of company details at creation time for historical accuracy.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Admins can complete company details configuration (all required fields) in under 5 minutes
- **SC-002**: 100% of quotations created after company details are configured include all configured company information (PAN, TAN, GST, address, contact, logo)
- **SC-003**: 100% of quotations display the correct country-specific bank details based on the client's country (India or Dubai)
- **SC-004**: Company details updates are reflected in new quotations within 1 minute of saving changes
- **SC-005**: All company detail changes are logged and auditable with 100% accuracy
- **SC-006**: System validates tax identification number formats with 100% accuracy according to Indian tax regulations
- **SC-007**: Non-admin users are prevented from accessing company details configuration with 100% success rate
- **SC-008**: Quotation PDFs and emails include company branding (logo, address, contact) when configured, with 100% consistency across all document types
