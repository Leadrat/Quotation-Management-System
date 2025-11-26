# Spec-023: Company Details Admin Page – Country-Specific Identifiers & Bank Details

**Feature Branch**: `023-country-company-identifiers`  
**Created**: 2025-01-27  
**Status**: Draft  
**Input**: User description: "Enhance the existing 'Company Details' admin page so that all company information (identifiers and bank details) can be managed per country."

## Overview

This specification extends the Company Details admin page to support country-specific company identifiers and bank details management. The system enables administrators to configure required identifiers (e.g., PAN for India, VAT for EU, Business License for Dubai) and relevant bank detail fields (e.g., IFSC for India, IBAN/SWIFT for Dubai/UAE, routing codes for US) for each country. These settings automatically flow into quotation creation and editing so only contextually correct data is visible and required.

## Project Information

- **Project Name**: CRM Quotation Management System
- **Spec Number**: Spec-023
- **Spec Name**: Company Details Admin Page – Country-Specific Identifiers & Bank Details
- **Group**: Administration & Settings
- **Priority**: HIGH
- **Dependencies**: 
  - Spec-009 (User Management)
  - Spec-018 (System Administration)
  - Spec-022 (Company Details Admin Configuration)
  - Spec-020 (Country Tax Management) - for country reference
- **Related Specs**: 
  - Spec-010 (Quotation Management)
  - Spec-017 (Multi-Currency & Localization)

---

## Key Features

### Master Configuration (Admin-Only)

- Define required company identifiers for each country (e.g., PAN for India, VAT for EU, Business License for Dubai)
- Configure relevant bank detail fields for each country (e.g., IFSC for India, IBAN/SWIFT for Dubai/UAE, routing codes for US)
- Set validation rules (format, regex, min/max length, required flag) per field and country
- Enable/disable identifier types and bank fields per country
- Extend configuration to new countries without code changes

### Company Details Management (Admin)

- Assign values to each company for only the fields relevant in the company's country
- View and edit company identifiers based on country configuration
- Manage bank details with country-specific fields dynamically displayed
- Country selection drives what fields are shown and required
- All edits are context-aware and save correctly

### Quotation Integration

- When a sales rep creates/edits a quotation, dynamically inject only those company identifier and bank details relevant for the client's country
- Quotation PDFs, emails, and UI show only the country-appropriate details
- Form fields automatically adjust based on client country selection
- Validation rules apply per country configuration

### Data Validation

- Strong data validation (regex/type, min/max length, required flag) per field/country
- Format validation for identifiers (e.g., PAN format for India, VAT format for EU)
- Real-time validation feedback during data entry
- Clear error messages indicating which fields are invalid and why

---

## JTBD Alignment

**Persona**: Administrator, Sales Representative

**JTBD**: "I want to configure company identifiers and bank details per country, and have quotations automatically show the correct information based on the client's country, so that we can operate globally while maintaining compliance and accuracy."

**Success Metric**: "Admins can configure country-specific identifiers and bank fields; quotations automatically display correct company information based on client country; zero configuration errors when adding new countries."

---

## Business Value

- Enables global business expansion with country-specific compliance
- Reduces manual errors by showing only relevant fields per country
- Ensures regulatory compliance across different jurisdictions
- Improves quotation accuracy with contextually correct information
- Supports extensibility for new countries without development work
- Enhances user experience with country-aware forms
- Reduces training time with intuitive, context-aware interfaces

---

## User Scenarios & Testing

### User Story 1 - Admin Configures Country-Specific Identifiers (Priority: P1)

An admin user accesses the master configuration to define which company identifiers are required for each country (e.g., PAN for India, VAT for EU, Business License for Dubai) and sets validation rules for each identifier type per country.

**Why this priority**: This is the foundation of the feature. Without master configuration, country-specific fields cannot be determined. This must be implemented first to enable all other functionality.

**Independent Test**: Can be fully tested by an admin logging in, navigating to identifier configuration, adding PAN identifier for India with format validation, adding VAT identifier for EU countries with format validation, and verifying both configurations are saved and displayed correctly.

**Acceptance Scenarios**:

1. **Given** I am logged in as an admin, **When** I navigate to the master identifier configuration page, **Then** I see a list of countries and can configure identifiers for each country

2. **Given** I am configuring identifiers for India, **When** I add PAN identifier with:
   - Display name: "PAN Number"
   - Format: "^[A-Z]{5}[0-9]{4}[A-Z]{1}$"
   - Required: Yes
   - Help text: "10-character alphanumeric Permanent Account Number"
   - Then click "Save", **Then** PAN is configured for India and appears in the identifier list

3. **Given** I am configuring identifiers for EU countries, **When** I add VAT identifier with:
   - Display name: "VAT Number"
   - Format: "^[A-Z]{2}[0-9A-Z]{2,12}$"
   - Required: Yes
   - Help text: "Country code followed by alphanumeric VAT number"
   - Then click "Save", **Then** VAT is configured for EU and appears in the identifier list

4. **Given** I am configuring identifiers for Dubai, **When** I add Business License identifier with:
   - Display name: "Trade License Number"
   - Format: "^[0-9]{6,10}$"
   - Required: Yes
   - Help text: "6-10 digit trade license number issued by DED"
   - Then click "Save", **Then** Business License is configured for Dubai and appears in the identifier list

5. **Given** I have configured identifiers for a country, **When** I edit the validation rules or display properties, **Then** the changes are saved and reflected immediately

6. **Given** I am a non-admin user, **When** I try to access the master identifier configuration, **Then** I receive an access denied error

---

### User Story 2 - Admin Configures Country-Specific Bank Fields (Priority: P1)

An admin user accesses the master configuration to define which bank detail fields are relevant for each country (e.g., IFSC for India, IBAN/SWIFT for Dubai/UAE, routing codes for US) and sets validation rules for each field per country.

**Why this priority**: Bank fields must be configured alongside identifiers to enable complete country-specific company details. This is essential for accurate quotation generation and must be configured after identifiers are set up.

**Independent Test**: Can be fully tested by an admin configuring IFSC field for India, IBAN and SWIFT fields for Dubai, routing number field for US, setting validation rules for each, and verifying configurations are saved correctly.

**Acceptance Scenarios**:

1. **Given** I am logged in as an admin, **When** I navigate to the master bank fields configuration page, **Then** I see a list of countries and can configure bank fields for each country

2. **Given** I am configuring bank fields for India, **When** I add IFSC field with:
   - Display name: "IFSC Code"
   - Format: "^[A-Z]{4}0[0-9A-Z]{6}$"
   - Required: Yes
   - Help text: "11-character alphanumeric Indian Financial System Code"
   - Then click "Save", **Then** IFSC is configured for India and appears in the bank fields list

3. **Given** I am configuring bank fields for Dubai/UAE, **When** I add IBAN and SWIFT fields with:
   - IBAN: Display name "IBAN", Format "^[A-Z]{2}[0-9]{2}[A-Z0-9]{4,30}$", Required: Yes
   - SWIFT: Display name "SWIFT Code", Format "^[A-Z]{4}[A-Z]{2}[0-9A-Z]{2}([0-9A-Z]{3})?$", Required: Yes
   - Then click "Save", **Then** both fields are configured for Dubai and appear in the bank fields list

4. **Given** I am configuring bank fields for US, **When** I add Routing Number field with:
   - Display name: "Routing Number"
   - Format: "^[0-9]{9}$"
   - Required: Yes
   - Help text: "9-digit ABA routing number"
   - Then click "Save", **Then** Routing Number is configured for US and appears in the bank fields list

5. **Given** I have configured bank fields for a country, **When** I edit the validation rules or field properties, **Then** the changes are saved and reflected immediately

6. **Given** I am a non-admin user, **When** I try to access the master bank fields configuration, **Then** I receive an access denied error

---

### User Story 3 - Admin Manages Company Identifiers by Country (Priority: P1)

An admin user accesses the Company Details page to add or edit company identifier values. The form dynamically displays only the identifier fields configured for the selected company country, with appropriate validation and help text.

**Why this priority**: Admins need to enter actual company identifier values. This delivers immediate value by allowing configuration of company details per country. This must work correctly after master configuration is set up.

**Independent Test**: Can be fully tested by an admin selecting India as company country, seeing only PAN field (if configured), entering a PAN value, verifying validation, saving, then changing country to EU and seeing only VAT field appear.

**Acceptance Scenarios**:

1. **Given** I am logged in as an admin, **When** I navigate to the Company Details page and select "India" as the company country, **Then** I see only the identifier fields configured for India (e.g., PAN) with appropriate labels and help text

2. **Given** I am on the Company Details page with India selected, **When** I enter a PAN number in the correct format, **Then** the field validates successfully and shows no error

3. **Given** I am on the Company Details page with India selected, **When** I enter an invalid PAN number format, **Then** I see a validation error with the expected format and the save button is disabled

4. **Given** I am on the Company Details page and change the country from India to "EU", **When** the country changes, **Then** the PAN field disappears and VAT field appears (if configured for EU)

5. **Given** I have entered valid company identifier values, **When** I click save, **Then** the values are saved and I see a success message

6. **Given** I am viewing company details for a company in India, **When** I edit the PAN number, **Then** the new value is validated before saving

7. **Given** I am required to provide an identifier for the selected country, **When** I try to save without entering it, **Then** I see a clear error message indicating which required fields are missing

---

### User Story 4 - Admin Manages Bank Details by Country (Priority: P1)

An admin user accesses the Company Details page to add or edit bank details. The form dynamically displays only the bank fields configured for the selected company country, with appropriate validation and help text.

**Why this priority**: Bank details are essential for quotations. Admins need to enter country-specific bank information. This delivers immediate value by allowing accurate bank details per country. This must work correctly after master configuration is set up.

**Independent Test**: Can be fully tested by an admin selecting India as company country, seeing only IFSC field, entering IFSC code, verifying validation, saving, then changing country to Dubai and seeing IBAN and SWIFT fields appear.

**Acceptance Scenarios**:

1. **Given** I am logged in as an admin, **When** I navigate to the Bank Details section on the Company Details page and select "India" as the bank country, **Then** I see only the bank fields configured for India (e.g., Account Number, IFSC Code, Bank Name, Branch Name)

2. **Given** I am on the Bank Details section with India selected, **When** I enter an IFSC code in the correct format, **Then** the field validates successfully and shows no error

3. **Given** I am on the Bank Details section with India selected, **When** I enter an invalid IFSC code format, **Then** I see a validation error with the expected format and the save button is disabled

4. **Given** I am on the Bank Details section and change the bank country from India to "Dubai", **When** the country changes, **Then** the IFSC field disappears and IBAN and SWIFT fields appear (if configured for Dubai)

5. **Given** I have entered valid bank details, **When** I click save, **Then** the values are saved and I see a success message

6. **Given** I am viewing bank details for a company in Dubai, **When** I edit the IBAN or SWIFT code, **Then** the new values are validated before saving

7. **Given** I am required to provide bank fields for the selected country, **When** I try to save without entering required fields, **Then** I see a clear error message indicating which required fields are missing

---

### User Story 5 - Sales Rep Sees Country-Specific Company Details in Quotations (Priority: P1)

When a sales representative creates or edits a quotation, the system automatically includes only those company identifiers and bank details relevant for the client's country. The quotation form and PDF display only the appropriate information.

**Why this priority**: This is the core value proposition - quotations must include accurate, country-appropriate company information. This must work correctly from the first quotation created after company details are configured.

**Independent Test**: Can be fully tested by configuring company details with India identifiers and bank details, creating a quotation for a client in India, verifying India-specific details appear, then creating a quotation for a Dubai client and verifying Dubai-specific details appear.

**Acceptance Scenarios**:

1. **Given** company details are configured with PAN for India and VAT for EU, and bank details with IFSC for India and IBAN/SWIFT for Dubai, **When** a sales rep creates a quotation for a client in India, **Then** the quotation includes PAN number and India bank details (Account Number, IFSC Code, Bank Name, Branch Name)

2. **Given** company details are configured with PAN for India and VAT for EU, and bank details with IFSC for India and IBAN/SWIFT for Dubai, **When** a sales rep creates a quotation for a client in Dubai, **Then** the quotation includes relevant identifiers (if configured for Dubai) and Dubai bank details (Account Number, IBAN, SWIFT Code, Bank Name, Branch Name)

3. **Given** company details are configured with VAT for EU, **When** a sales rep creates a quotation for a client in Germany (EU country), **Then** the quotation includes VAT number and relevant EU bank details

4. **Given** a quotation is being created or edited, **When** the sales rep selects or changes the client's country, **Then** the quotation preview updates to show only the company identifiers and bank details relevant for that country

5. **Given** company details are updated by an admin, **When** a sales rep creates a new quotation after the update, **Then** the new quotation reflects the updated company details for the client's country

6. **Given** a quotation PDF is generated, **When** the PDF is created, **Then** it includes only the company identifiers and bank details relevant for the client's country

7. **Given** a quotation is sent via email, **When** the email is sent, **Then** the email content includes only the company identifiers and bank details relevant for the client's country

8. **Given** a quotation was created before company details were updated, **When** the quotation is viewed, **Then** it displays the company details that were current at the time of quotation creation (historical accuracy)

---

### Edge Cases

- What happens when company details are not yet configured for a country and a sales rep tries to create a quotation? (System should handle gracefully - either show available fields or show a message indicating configuration is needed)
- How does the system handle invalid identifier or bank field format during data entry? (System should validate and show clear error messages with expected format)
- What happens when a client's country is not yet configured in the master configuration? (System should either show a default set of fields or prompt admin to configure the country first)
- How does the system handle cases where required identifiers are missing for a country? (System should prevent quotation creation and show clear message about missing required information)
- What happens when an admin tries to configure duplicate identifier types for the same country? (System should prevent duplicates and show appropriate error message)
- How does the system handle country configuration changes after company details are already entered? (System should update validation rules but preserve existing values if they still pass validation)
- What happens when bank details for a country are not configured but a quotation is created for a client in that country? (System should either show a warning, use default/empty bank details, or prevent quotation creation depending on configuration)
- How does the system handle concurrent updates to master configuration by multiple admins? (System should use appropriate concurrency control to prevent data loss)
- What happens when a country configuration is disabled after company details are already entered? (System should handle gracefully - either show inactive fields or prompt admin to update country)

---

## Requirements

### Functional Requirements

- **FR-001**: System MUST provide an admin-only master configuration page for managing country-specific identifier types

- **FR-002**: System MUST allow admins to define which identifier types are required for each country (e.g., PAN for India, VAT for EU, Business License for Dubai)

- **FR-003**: System MUST allow admins to configure validation rules (format, regex, min/max length, required flag) per identifier type and country

- **FR-004**: System MUST allow admins to set display names and help text per identifier type and country

- **FR-005**: System MUST provide an admin-only master configuration page for managing country-specific bank fields

- **FR-006**: System MUST allow admins to define which bank fields are relevant for each country (e.g., IFSC for India, IBAN/SWIFT for Dubai/UAE, routing codes for US)

- **FR-007**: System MUST allow admins to configure validation rules (format, regex, min/max length, required flag) per bank field and country

- **FR-008**: System MUST allow admins to set display names and help text per bank field and country

- **FR-009**: System MUST dynamically display only identifier fields configured for the selected company country on the Company Details page

- **FR-010**: System MUST dynamically display only bank fields configured for the selected bank country on the Company Details page

- **FR-011**: System MUST validate company identifier values according to the configured rules for the selected country

- **FR-012**: System MUST validate bank detail values according to the configured rules for the selected country

- **FR-013**: System MUST show clear validation error messages with expected format when validation fails

- **FR-014**: System MUST prevent saving company details when required fields are missing or invalid

- **FR-015**: System MUST update the Company Details form fields when country selection changes

- **FR-016**: System MUST automatically include only relevant company identifiers in quotations based on the client's country

- **FR-017**: System MUST automatically include only relevant bank details in quotations based on the client's country

- **FR-018**: System MUST update quotation preview when client country changes during quotation creation/editing

- **FR-019**: System MUST include only country-relevant company details in quotation PDFs

- **FR-020**: System MUST include only country-relevant company details in quotation emails

- **FR-021**: System MUST preserve historical accuracy - quotations created before company details updates must display the company details that were current at creation time

- **FR-022**: System MUST reflect updated company details in all new quotations created after an update

- **FR-023**: System MUST log all changes to master configuration and company details for audit purposes

- **FR-024**: System MUST restrict access to master configuration pages to admin role only

- **FR-025**: System MUST handle cases where company details are not configured for a country gracefully (show appropriate messages or default behavior)

- **FR-026**: System MUST prevent duplicate identifier type configurations for the same country

- **FR-027**: System MUST allow admins to enable/disable identifier types and bank fields per country

- **FR-028**: System MUST support extending configuration to new countries without code changes

- **FR-029**: System MUST validate all identifier and bank field values according to configured rules before saving

### Key Entities *(include if feature involves data)*

- **IdentifierType**: Represents a type of company identifier (e.g., PAN, VAT, Business License) with configuration per country including validation rules, display properties, and required flag

- **CountryIdentifierConfiguration**: Represents the configuration of which identifier types are required/optional for each country, including validation rules and display properties

- **CompanyIdentifierValue**: Represents the actual identifier values assigned to a company for their country (e.g., PAN number "ABCDE1234F" for India)

- **BankFieldType**: Represents a type of bank field (e.g., IFSC, IBAN, SWIFT, Routing Number) with configuration per country including validation rules, display properties, and required flag

- **CountryBankFieldConfiguration**: Represents the configuration of which bank fields are required/optional for each country, including validation rules and display properties

- **CompanyBankDetails**: Represents the actual bank detail values assigned to a company for their country (e.g., IFSC code "HDFC0001234" for India)

- **CompanyDetails**: Represents the company's overall details configuration, now linked to country-specific identifier and bank detail values

- **Quotation**: Represents a quotation document that references company details filtered by client country to display only relevant identifiers and bank details

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Admins can complete master identifier configuration for a country (adding 3+ identifier types with validation rules) in under 10 minutes

- **SC-002**: Admins can complete master bank fields configuration for a country (adding 4+ bank fields with validation rules) in under 10 minutes

- **SC-003**: Admins can complete company details entry for a country (entering all required identifiers and bank details) in under 5 minutes

- **SC-004**: 100% of quotations created after company details are configured include only the country-relevant company identifiers and bank details based on the client's country

- **SC-005**: Company details configuration updates are reflected in new quotations within 1 minute of saving changes

- **SC-006**: All master configuration and company detail changes are logged and auditable with 100% accuracy

- **SC-007**: System validates identifier and bank field formats with 100% accuracy according to configured rules per country

- **SC-008**: Non-admin users are prevented from accessing master configuration with 100% success rate

- **SC-009**: Quotation PDFs and emails include only country-relevant company details when generated, with 100% consistency across all document types

- **SC-010**: System supports configuration for 5+ countries without code changes, demonstrating extensibility

- **SC-011**: Form validation errors are displayed within 1 second of invalid data entry, providing immediate feedback

- **SC-012**: Country selection changes update form fields within 0.5 seconds, ensuring responsive user experience

---

## Assumptions

- Master configuration for identifier types and bank fields is managed separately from company detail values
- Company details are stored per country, allowing different values for the same company in different countries if needed
- Validation rules are stored as configuration data, not hardcoded in application logic
- The system can reference country data from existing country/jurisdiction management (Spec-020)
- Historical quotation accuracy is maintained through snapshots or timestamp-based queries
- Admin users have appropriate permissions to configure master settings
- The system supports adding new countries without requiring code deployment
- Identifier types and bank field types can be reused across multiple countries with country-specific validation rules
- Company can operate in multiple countries simultaneously, requiring country-specific configurations for each
- Quotation creation can proceed even if some optional identifiers are not configured, as long as required ones are present

---

## Out of Scope

- Automated validation of identifier values against external government databases (only format validation is included)
- Multi-language support for identifier field labels and help text (future enhancement)
- Bulk import/export of identifier and bank detail configurations
- Version history or rollback of master configuration changes
- Custom identifier or bank field types created by admins (only predefined types with country-specific configuration)
- Identifier or bank detail expiration dates and renewal reminders
- Integration with third-party payment processors for bank account verification
- Automatic identifier number generation
- Support for multi-jurisdiction identifiers within the same country (handled at country level only)

---

## Dependencies

### Prerequisites

- Spec-009 (User Management) - Required for user authentication and role-based access control
- Spec-018 (System Administration) - Required for admin interface foundation
- Spec-022 (Company Details Admin Configuration) - Required for base company details functionality
- Spec-020 (Country Tax Management) - Required for country reference data

### Integration Points

- Quotation creation/editing workflow (Spec-010) - Must integrate country-specific company details display
- Quotation PDF generation (Spec-010) - Must include only relevant company details per country
- Quotation email templates (Spec-010) - Must include only relevant company details per country
- Country/jurisdiction management (Spec-020) - Must reference country data for field configuration

---

## Risks & Mitigations

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Complex validation rules may cause performance issues | High | Medium | Implement client-side validation for immediate feedback, server-side validation for security; cache validation rules |
| Country configuration errors may break quotation generation | High | Low | Implement configuration validation before activation; provide preview/test mode for configurations |
| Migration of existing company details to country-specific model may lose data | High | Medium | Create comprehensive migration script with data validation and rollback capability; maintain backup |
| Inconsistent validation rules across countries may confuse admins | Medium | Medium | Provide clear documentation and examples for each country; implement configuration templates |
| Performance degradation when querying country-specific details for quotations | Medium | Low | Implement efficient database queries with proper indexing; cache frequently accessed configurations |

