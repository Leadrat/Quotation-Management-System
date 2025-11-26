# Spec-020: Multi-Country & Jurisdiction Tax Management

**Feature Branch**: `020-country-tax-management`  
**Created**: 2025-11-18  
**Status**: Draft  
**Input**: User description: "We need quotes application to work in different countries and jurisidictions. For now we need to create quotations for India and UAE, specifically Dubai. We can add/manage different countries, local taxes, types of taxes depending on product or service etc. for each of these countries. Admin can manage this."

## Overview

This specification extends the quotation management system to support multi-country and multi-jurisdiction tax management. The system enables administrators to configure tax rules per country and jurisdiction (e.g., states, provinces, cities), with support for different tax types and rates based on product or service categories. Initially supporting India (GST) and UAE (Dubai VAT), the system provides flexible tax configuration capabilities to expand to additional countries and jurisdictions over time.

## Project Information

- **Project Name**: CRM Quotation Management System
- **Spec Number**: Spec-020
- **Spec Name**: Multi-Country & Jurisdiction Tax Management
- **Group**: Advanced Features (Group 10 of 11)
- **Priority**: HIGH (Phase 2, after Multi-Currency & Localization)
- **Dependencies**: 
  - Spec-009 (Quotation Entity & CRUD Operations)
  - Spec-017 (Multi-Currency & Localization)
  - Spec-018 (System Administration)
- **Related Specs**: 
  - Spec-014 (Payment Processing)
  - Spec-015 (Reporting & Analytics)

---

## Key Features

### Country & Jurisdiction Management
- Add and manage countries with their tax frameworks
- Configure jurisdictions within countries (states, provinces, emirates, cities, etc.)
- Set default country and jurisdiction for the company
- Hierarchical jurisdiction support (e.g., Country → State → City)
- Enable/disable countries and jurisdictions

### Tax Configuration
- Configure tax types per country (e.g., GST for India, VAT for UAE)
- Set tax rates per jurisdiction
- Define tax rules based on product/service categories
- Support multiple tax components (e.g., CGST + SGST for India, VAT for UAE)
- Configure tax exemptions and special rates for specific scenarios
- Historical tax rate tracking (effective dates for rate changes)

### Product/Service Category Tax Rules
- Assign tax rates based on product or service category
- Support different tax rates for the same jurisdiction based on item type
- Configure tax exemptions per category (e.g., zero-rated items)
- Override category rates with jurisdiction-specific rates when needed

### Tax Calculation Engine
- Automatically determine applicable tax based on client location and item categories
- Support complex tax structures (multiple components, exemptions, special cases)
- Calculate taxes correctly for multi-jurisdiction scenarios
- Validate tax calculations against configured rules

### Admin Management Interface
- Admin-only interface for managing countries and jurisdictions
- Admin-only interface for configuring tax types, rates, and rules
- Bulk import/export of tax configurations
- View tax calculation history and audit trail
- Preview tax calculations before saving configurations

---

## JTBD Alignment

**Persona**: Administrator, Sales Rep, Accountant

**JTBD**: "I want to create quotations for clients in different countries with accurate tax calculations based on their location and the products/services being sold, so that we can expand our business globally while remaining tax-compliant"

**Success Metric**: "Admins can configure tax rules for India and UAE; quotations automatically calculate correct taxes based on client location and item categories; zero calculation errors reported"

---

## Business Value

- Enables global business expansion with proper tax compliance
- Reduces manual tax calculation errors and compliance risks
- Supports multi-country operations with automated tax handling
- Ensures regulatory compliance across different jurisdictions
- Facilitates accurate financial reporting and accounting
- Improves quotation accuracy and customer trust
- Reduces time spent on tax configuration and calculations

---

## User Scenarios & Testing

### User Story 1 - Admin Configures Country and Tax Framework (Priority: P1)

As an administrator, I want to add countries (India and UAE) and configure their tax frameworks (GST for India, VAT for Dubai) so that the system can calculate taxes correctly for quotations in these countries.

**Why this priority**: This is the foundation of the feature. Without country and tax framework configuration, no tax calculations can occur. This must be implemented first to enable all other functionality.

**Independent Test**: Can be fully tested by an admin logging in, navigating to tax configuration, adding India with GST framework, adding UAE with VAT framework, and verifying both countries are saved and displayed in the configuration list.

**Acceptance Scenarios**:

1. **Given** I am logged in as an admin, **When** I navigate to `/admin/tax/countries`, **Then** I see a list of configured countries (initially empty or showing default countries)

2. **Given** I am on the countries configuration page, **When** I click "Add Country" and fill in:
   - Country name: "India"
   - Country code: "IN"
   - Tax framework: "GST"
   - Default currency: "INR"
   - Then click "Save", **Then** India is added to the list and I can see it in the configuration

3. **Given** I am on the countries configuration page, **When** I add UAE with:
   - Country name: "United Arab Emirates"
   - Country code: "AE"
   - Tax framework: "VAT"
   - Default currency: "AED"
   - Then click "Save", **Then** UAE is added and I can configure Dubai as a jurisdiction

4. **Given** I am viewing a country configuration, **When** I edit the tax framework or other details, **Then** the changes are saved and reflected immediately

5. **Given** I am a non-admin user, **When** I try to access `/admin/tax/countries`, **Then** I receive a 403 Forbidden error

---

### User Story 2 - Admin Configures Jurisdictions (Priority: P1)

As an administrator, I want to configure jurisdictions (states for India, emirates/cities for UAE) with their tax rates so that quotations can calculate taxes based on client location.

**Why this priority**: Jurisdictions are required to determine which tax rules apply based on client location. This is essential for accurate tax calculations and must be configured after countries are set up.

**Independent Test**: Can be fully tested by an admin adding Indian states (e.g., Maharashtra, Karnataka) and UAE emirates (Dubai, Abu Dhabi), setting tax rates for each, and verifying the jurisdictions appear in the configuration with correct rates.

**Acceptance Scenarios**:

1. **Given** I have configured India as a country, **When** I navigate to India's jurisdiction configuration, **Then** I see a list of Indian states/jurisdictions

2. **Given** I am configuring jurisdictions for India, **When** I add "Maharashtra" with state code "27", **Then** Maharashtra is added as a jurisdiction under India

3. **Given** I am configuring jurisdictions for UAE, **When** I add "Dubai" as a jurisdiction, **Then** Dubai is added under UAE and I can configure VAT rates for Dubai

4. **Given** I am viewing a jurisdiction, **When** I edit the tax rates, **Then** the changes are saved and I can see the updated rates

5. **Given** I am configuring multiple jurisdictions, **When** I add hierarchical jurisdictions (e.g., Dubai → Dubai City), **Then** the hierarchy is preserved and I can navigate the nested structure

---

### User Story 3 - Admin Configures Tax Rates by Product/Service Category (Priority: P1)

As an administrator, I want to configure different tax rates based on product or service categories so that items in the same jurisdiction can have different tax rates (e.g., services at 5% VAT in Dubai, products at 5% VAT).

**Why this priority**: Many jurisdictions have different tax rates for different types of goods/services. This is critical for accurate tax calculations and must be supported from the start.

**Independent Test**: Can be fully tested by an admin configuring tax rates for Dubai: 5% VAT for "Services" category and 5% VAT for "Products" category, then creating a quotation with items in both categories and verifying each item has the correct tax rate applied.

**Acceptance Scenarios**:

1. **Given** I am configuring tax rules for Dubai, **When** I navigate to category tax rates, **Then** I see a list of product/service categories with their tax rates

2. **Given** I am on the category tax rates page for Dubai, **When** I set:
   - Category: "Services"
   - Tax rate: 5%
   - Effective from: [current date]
   - Then click "Save", **Then** the category tax rate is configured for Dubai

3. **Given** I have configured category tax rates for Dubai, **When** I create a quotation with line items categorized as "Services", **Then** the tax calculation uses 5% VAT for those items

4. **Given** I am configuring tax rules, **When** I set different rates for different categories in the same jurisdiction, **Then** each category retains its specific tax rate

5. **Given** I am viewing tax rules, **When** I change a category tax rate with an effective date in the future, **Then** the current rate continues to apply until the effective date, after which the new rate applies

---

### User Story 4 - System Calculates Tax Based on Client Location and Item Categories (Priority: P1)

As a sales rep, I want the system to automatically calculate taxes when creating quotations based on the client's country/jurisdiction and the categories of items being quoted, so that I don't have to manually calculate or research tax rates.

**Why this priority**: This is the core user-facing value of the feature. Without automatic tax calculation, users would still need to manually configure taxes, defeating the purpose of this feature.

**Independent Test**: Can be fully tested by creating a quotation with a client in Maharashtra, India, adding line items categorized as "Services", and verifying the system automatically calculates CGST and SGST at the configured rates without manual intervention.

**Acceptance Scenarios**:

1. **Given** I am creating a quotation, **When** I select a client located in Maharashtra, India, **Then** the system determines that GST framework applies and Maharashtra jurisdiction is active

2. **Given** I have selected a client in Maharashtra, **When** I add line items with category "Services", **Then** the system automatically calculates CGST and SGST based on Maharashtra rates for that category

3. **Given** I am creating a quotation for a client in Dubai, **When** I add line items categorized as "Products", **Then** the system automatically calculates 5% VAT for those items

4. **Given** I am editing a quotation, **When** I change the client or add/remove line items, **Then** the tax amounts are recalculated automatically based on the new configuration

5. **Given** I am viewing a quotation, **When** I see the tax breakdown, **Then** I can see which tax components were applied (e.g., CGST, SGST, or VAT) and at what rates

---

### User Story 5 - Admin Views Tax Calculation Audit Trail (Priority: P2)

As an administrator, I want to view a history of tax calculations and configuration changes so that I can audit tax compliance and troubleshoot calculation issues.

**Why this priority**: Important for compliance and troubleshooting, but not critical for initial functionality. Can be implemented after core tax calculation features are working.

**Independent Test**: Can be fully tested by an admin viewing the tax audit log, seeing entries for tax configuration changes and quotations that used tax calculations, and verifying the log shows dates, users, and details of each action.

**Acceptance Scenarios**:

1. **Given** I am an admin, **When** I navigate to `/admin/tax/audit-log`, **Then** I see a list of tax-related actions (configuration changes, calculations performed)

2. **Given** I am viewing the tax audit log, **When** I filter by date range and country, **Then** I see only matching entries

3. **Given** I am viewing a quotation, **When** I click "View Tax Calculation Details", **Then** I see which tax rules were applied, rates used, and the calculation breakdown

4. **Given** I have made tax configuration changes, **When** I view the audit log, **Then** I see entries showing what changed, who made the change, and when

---

## Requirements

### Functional Requirements

#### Country Management
- **FR-001**: System MUST allow admins to add countries with country name, country code (ISO 3166-1 alpha-2), tax framework type, and default currency
- **FR-002**: System MUST allow admins to edit country configurations (name, tax framework, default currency)
- **FR-003**: System MUST allow admins to enable/disable countries (disabled countries cannot be used in new quotations)
- **FR-004**: System MUST validate country codes are unique and follow ISO 3166-1 alpha-2 standard
- **FR-005**: System MUST support setting a default country for the company
- **FR-006**: System MUST enforce RBAC - only admins can manage countries

#### Jurisdiction Management
- **FR-007**: System MUST allow admins to add jurisdictions (states, provinces, emirates, cities) under countries
- **FR-008**: System MUST support hierarchical jurisdictions (e.g., Country → State → City)
- **FR-009**: System MUST allow admins to set jurisdiction codes (e.g., state codes, city codes) for jurisdictions
- **FR-010**: System MUST allow admins to enable/disable jurisdictions (disabled jurisdictions cannot be used in new quotations)
- **FR-011**: System MUST validate jurisdiction codes are unique within their parent country/jurisdiction
- **FR-012**: System MUST enforce RBAC - only admins can manage jurisdictions

#### Tax Framework Configuration
- **FR-013**: System MUST support different tax frameworks per country (e.g., GST for India, VAT for UAE)
- **FR-014**: System MUST allow admins to configure tax framework details (name, description, components like CGST/SGST for GST, or single VAT component)
- **FR-015**: System MUST allow admins to define tax components for each framework (e.g., GST has CGST and SGST components, VAT has a single VAT component)
- **FR-016**: System MUST enforce RBAC - only admins can configure tax frameworks

#### Tax Rate Configuration
- **FR-017**: System MUST allow admins to configure base tax rates per jurisdiction
- **FR-018**: System MUST allow admins to configure tax rates per product/service category within a jurisdiction
- **FR-019**: System MUST allow admins to set effective dates for tax rates (supporting historical and future-dated rates)
- **FR-020**: System MUST allow admins to configure tax exemptions (zero-rated, exempt items) per category or jurisdiction
- **FR-021**: System MUST allow admins to override category tax rates with jurisdiction-specific rates when needed
- **FR-022**: System MUST validate tax rates are between 0% and 100%
- **FR-023**: System MUST enforce RBAC - only admins can configure tax rates

#### Product/Service Category Tax Rules
- **FR-024**: System MUST allow admins to assign tax rates to product/service categories
- **FR-025**: System MUST allow admins to configure different tax rates for the same category in different jurisdictions
- **FR-026**: System MUST allow admins to mark categories as tax-exempt or zero-rated for specific jurisdictions
- **FR-027**: System MUST validate category tax rules are consistent (no conflicting rules)
- **FR-028**: System MUST enforce RBAC - only admins can configure category tax rules

#### Tax Calculation Engine
- **FR-029**: System MUST automatically determine applicable tax framework based on client's country
- **FR-030**: System MUST automatically determine applicable jurisdiction based on client's location (state, city, etc.)
- **FR-031**: System MUST automatically determine tax rate based on jurisdiction and item category
- **FR-032**: System MUST calculate taxes correctly for multi-component tax frameworks (e.g., split CGST/SGST for India)
- **FR-033**: System MUST apply tax exemptions and zero-rated rules when configured
- **FR-034**: System MUST recalculate taxes automatically when quotation line items or client location changes
- **FR-035**: System MUST validate tax calculations match configured rules and rates
- **FR-036**: System MUST log all tax calculations for audit purposes

#### Tax Display and Reporting
- **FR-037**: System MUST display tax breakdown showing all tax components (CGST, SGST, VAT, etc.) separately on quotations
- **FR-038**: System MUST show which tax rates and rules were applied for each line item
- **FR-039**: System MUST display tax calculation details in quotation PDFs and exports
- **FR-040**: System MUST support filtering quotations by country and jurisdiction in reports

#### Admin Management Interface
- **FR-041**: System MUST provide admin-only interface for managing countries and jurisdictions
- **FR-042**: System MUST provide admin-only interface for configuring tax frameworks, rates, and category rules
- **FR-043**: System MUST allow admins to preview tax calculations before saving configurations
- **FR-044**: System MUST allow admins to bulk import/export tax configurations (CSV/Excel format)
- **FR-045**: System MUST provide admin view of tax calculation audit trail
- **FR-046**: System MUST enforce RBAC - all admin tax management features require Admin role

#### Data Validation and Security
- **FR-047**: System MUST validate all country codes against ISO 3166-1 alpha-2 standard
- **FR-048**: System MUST validate tax rates are numeric and within valid ranges (0-100%)
- **FR-049**: System MUST validate effective dates for tax rates are valid dates
- **FR-050**: System MUST prevent deletion of countries/jurisdictions that are in use by existing quotations
- **FR-051**: System MUST require admin confirmation before disabling countries/jurisdictions in use
- **FR-052**: System MUST log all tax configuration changes with user, timestamp, and change details

### Key Entities

#### Country
- Represents a country with its tax framework configuration
- Key attributes: CountryId, CountryName, CountryCode (ISO 3166-1 alpha-2), TaxFrameworkType, DefaultCurrency, IsActive, CreatedAt, UpdatedAt
- Relationships: Has many Jurisdictions, Has one TaxFramework configuration

#### Jurisdiction
- Represents a tax jurisdiction within a country (state, province, emirate, city, etc.)
- Key attributes: JurisdictionId, CountryId (FK), ParentJurisdictionId (FK, nullable for hierarchy), JurisdictionName, JurisdictionCode, IsActive, CreatedAt, UpdatedAt
- Relationships: Belongs to Country, Has optional parent Jurisdiction, Has many TaxRates, Has many child Jurisdictions

#### TaxFramework
- Represents the tax framework for a country (e.g., GST, VAT)
- Key attributes: TaxFrameworkId, CountryId (FK), FrameworkName, FrameworkType (enum: GST, VAT, etc.), Description, TaxComponents (JSONB array), IsActive, CreatedAt, UpdatedAt
- Relationships: Belongs to Country, Has many TaxRates

#### TaxRate
- Represents a tax rate configuration for a jurisdiction and optionally a category
- Key attributes: TaxRateId, JurisdictionId (FK), TaxFrameworkId (FK), ProductServiceCategoryId (FK, nullable), TaxRate (decimal percentage), EffectiveFrom (DATE), EffectiveTo (DATE, nullable), IsExempt (bool), IsZeroRated (bool), CreatedAt, UpdatedAt
- Relationships: Belongs to Jurisdiction, Belongs to TaxFramework, Optionally belongs to ProductServiceCategory

#### ProductServiceCategory
- Represents a product or service category for tax rule assignment
- Key attributes: CategoryId, CategoryName, CategoryCode, Description, IsActive, CreatedAt, UpdatedAt
- Relationships: Has many TaxRates

#### TaxCalculationLog
- Represents an audit log entry for tax calculations and configuration changes
- Key attributes: LogId, QuotationId (FK, nullable), ActionType (enum: Calculation, ConfigurationChange), CountryId (FK, nullable), JurisdictionId (FK, nullable), CalculationDetails (JSONB), ChangedByUserId (FK), ChangedAt (TIMESTAMPTZ)
- Relationships: Optionally belongs to Quotation, Belongs to User (ChangedBy)

---

## Success Criteria

### Measurable Outcomes

- **SC-001**: Admins can configure at least 2 countries (India and UAE) with their tax frameworks (GST and VAT) in under 5 minutes per country
- **SC-002**: Admins can configure at least 5 jurisdictions (states for India, emirates for UAE) with tax rates in under 3 minutes per jurisdiction
- **SC-003**: System automatically calculates taxes correctly for 100% of test quotations across India and UAE jurisdictions without manual intervention
- **SC-004**: Tax calculations complete in under 1 second for quotations with up to 50 line items
- **SC-005**: Admins can configure category-based tax rules for at least 3 product/service categories in under 2 minutes per category
- **SC-006**: System maintains 100% accuracy in tax calculations when client location or item categories change during quotation editing
- **SC-007**: Tax calculation audit trail captures 100% of tax calculations and configuration changes with correct user, timestamp, and details
- **SC-008**: Zero calculation errors reported in production for quotations using configured tax rules
- **SC-009**: Admins can import/export tax configurations for a country with 10+ jurisdictions in under 1 minute

### Backend Success Criteria
- ✅ All entities, DTOs, commands, queries implemented for country, jurisdiction, and tax management
- ✅ Tax calculation engine correctly applies rules based on client location and item categories
- ✅ Support for multi-component tax frameworks (GST with CGST/SGST, VAT with single component)
- ✅ Tax rate history tracking with effective dates functional
- ✅ Tax calculation audit logging captures all calculations and configuration changes
- ✅ All APIs functional with proper RBAC enforcement (admin-only for configuration)
- ✅ Unit and integration tests pass with ≥85% coverage

### Frontend Success Criteria
- ✅ Admin interfaces for managing countries, jurisdictions, and tax configurations built using TailAdmin
- ✅ Tax configuration forms validate inputs and provide clear error messages
- ✅ Tax calculation preview shows correct breakdown before saving quotations
- ✅ Quotation displays show detailed tax breakdown (all components separately)
- ✅ Bulk import/export functionality works for tax configurations
- ✅ Tax audit log view displays all tax-related actions with filtering and search
- ✅ Mobile responsive design for all admin tax management pages
- ✅ Component and E2E tests pass with ≥80% coverage

### Integration Success Criteria
- ✅ Tax calculation integrates seamlessly with existing quotation creation/update workflows
- ✅ Tax calculations appear correctly in quotation PDFs and exports
- ✅ Tax data integrates with reporting and analytics (Spec-015 integration)
- ✅ No existing quotation functionality broken by tax management changes
- ✅ Full E2E workflows tested: Admin configures tax rules → Sales rep creates quotation → Tax calculated correctly → Quotation displays tax breakdown

---

## Assumptions

1. **Initial Countries**: The system will initially support India (GST framework) and UAE/Dubai (VAT framework). Additional countries can be added later through admin configuration.

2. **Tax Framework Types**: The system assumes each country has one primary tax framework (GST, VAT, etc.). Multiple tax frameworks per country are not supported in the initial version.

3. **Jurisdiction Hierarchy**: The system supports up to 3 levels of jurisdiction hierarchy (Country → State → City). More complex hierarchies may require future enhancements.

4. **Product/Service Categories**: Product and service categories are assumed to be pre-configured or manageable through a separate category management feature. This spec does not define category creation, only tax rule assignment to categories.

5. **Currency**: Tax amounts are calculated and stored in the quotation's currency. Multi-currency support is handled by Spec-017 (Multi-Currency & Localization).

6. **Client Location**: Client location (country, state, city) is assumed to be stored in the Client entity. This spec does not modify client data structure, only uses existing location fields.

7. **Historical Quotations**: Existing quotations created before tax configuration will retain their tax amounts. Tax recalculation will only occur for new or edited quotations after tax rules are configured.

8. **Tax Rate Changes**: When tax rates change with effective dates, existing quotations maintain their original tax calculations. Only new or edited quotations after the effective date use the new rates.

9. **Tax Exemptions**: Tax exemptions and zero-rated rules are configured at the category or jurisdiction level. Line-item-specific exemptions are not supported in the initial version.

10. **Admin Role**: Only users with the Admin role can configure tax rules. Sales reps and other users can view but not modify tax configurations.

---

## Technical Requirements

### Backend Requirements

#### Entities & Data Models
- Country entity (CountryId, CountryName, CountryCode, TaxFrameworkType, DefaultCurrency, IsActive, timestamps)
- Jurisdiction entity (JurisdictionId, CountryId FK, ParentJurisdictionId FK, JurisdictionName, JurisdictionCode, IsActive, timestamps)
- TaxFramework entity (TaxFrameworkId, CountryId FK, FrameworkName, FrameworkType, TaxComponents JSONB, IsActive, timestamps)
- TaxRate entity (TaxRateId, JurisdictionId FK, TaxFrameworkId FK, ProductServiceCategoryId FK, TaxRate, EffectiveFrom, EffectiveTo, IsExempt, IsZeroRated, timestamps)
- ProductServiceCategory entity (CategoryId, CategoryName, CategoryCode, Description, IsActive, timestamps)
- TaxCalculationLog entity (LogId, QuotationId FK, ActionType, CountryId FK, JurisdictionId FK, CalculationDetails JSONB, ChangedByUserId FK, ChangedAt)

#### Services
- TaxCalculationService: Core service for calculating taxes based on client location and item categories
- CountryManagementService: Service for managing countries and jurisdictions
- TaxConfigurationService: Service for managing tax frameworks, rates, and category rules
- TaxAuditService: Service for viewing and querying tax calculation audit logs

#### APIs
- `GET /api/v1/admin/tax/countries` - List all countries
- `POST /api/v1/admin/tax/countries` - Create country
- `GET /api/v1/admin/tax/countries/{countryId}` - Get country details
- `PUT /api/v1/admin/tax/countries/{countryId}` - Update country
- `DELETE /api/v1/admin/tax/countries/{countryId}` - Delete country (soft delete if in use)
- `GET /api/v1/admin/tax/countries/{countryId}/jurisdictions` - List jurisdictions for a country
- `POST /api/v1/admin/tax/jurisdictions` - Create jurisdiction
- `PUT /api/v1/admin/tax/jurisdictions/{jurisdictionId}` - Update jurisdiction
- `DELETE /api/v1/admin/tax/jurisdictions/{jurisdictionId}` - Delete jurisdiction (soft delete if in use)
- `GET /api/v1/admin/tax/frameworks` - List tax frameworks
- `POST /api/v1/admin/tax/frameworks` - Create tax framework
- `PUT /api/v1/admin/tax/frameworks/{frameworkId}` - Update tax framework
- `GET /api/v1/admin/tax/rates` - List tax rates (with filters)
- `POST /api/v1/admin/tax/rates` - Create tax rate
- `PUT /api/v1/admin/tax/rates/{rateId}` - Update tax rate
- `DELETE /api/v1/admin/tax/rates/{rateId}` - Delete tax rate
- `POST /api/v1/admin/tax/preview-calculation` - Preview tax calculation for test data
- `GET /api/v1/admin/tax/audit-log` - View tax calculation audit log
- `POST /api/v1/admin/tax/import` - Bulk import tax configurations
- `GET /api/v1/admin/tax/export` - Export tax configurations
- `GET /api/v1/tax/calculate` - Calculate tax for quotation (used by quotation service)

### Frontend Requirements (TailAdmin Next.js Theme - MANDATORY)

#### Pages
- `/admin/tax/countries` - Country management page (list, create, edit, delete)
- `/admin/tax/countries/[countryId]/jurisdictions` - Jurisdiction management for a country
- `/admin/tax/frameworks` - Tax framework configuration page
- `/admin/tax/rates` - Tax rate configuration page (with filters by country, jurisdiction, category)
- `/admin/tax/categories` - Product/service category tax rule assignment page
- `/admin/tax/audit-log` - Tax calculation audit log view
- `/admin/tax/import-export` - Bulk import/export page

#### Components
- CountryManagementTable - Table for listing and managing countries
- CountryForm - Form for creating/editing countries
- JurisdictionTree - Tree view for hierarchical jurisdictions
- JurisdictionForm - Form for creating/editing jurisdictions
- TaxFrameworkForm - Form for configuring tax frameworks
- TaxRateTable - Table for managing tax rates with filters
- TaxRateForm - Form for creating/editing tax rates
- CategoryTaxRulesTable - Table for managing category-based tax rules
- TaxCalculationPreview - Component showing tax calculation breakdown
- TaxAuditLogTable - Table for viewing tax audit logs with filters

#### Integration
- Update QuotationCreateForm to show tax breakdown
- Update QuotationEditForm to recalculate taxes when client or items change
- Update QuotationDisplay to show detailed tax breakdown
- Update QuotationPDF generation to include tax breakdown

---

## Security & Compliance

- All admin tax management endpoints enforce RBAC (Admin role required)
- Tax configuration changes are logged with user, timestamp, and change details
- Tax calculations are auditable through TaxCalculationLog
- Country codes validated against ISO 3166-1 alpha-2 standard
- Tax rates validated to prevent negative or excessive values
- Soft delete for countries/jurisdictions in use to maintain quotation data integrity
- Admin confirmation required for destructive actions (deleting countries/jurisdictions in use)

---

## Performance Considerations

- Tax calculation queries optimized with proper indexes on CountryId, JurisdictionId, ProductServiceCategoryId
- Tax rate lookups cached for frequently accessed jurisdictions and categories
- Jurisdiction hierarchy queries optimized for tree traversal
- Tax calculation preview uses same optimized service as production calculations
- Bulk import/export operations process in batches to avoid timeouts

---

## Scalability

- Support for unlimited countries and jurisdictions (with proper indexing)
- Support for unlimited tax rates and category rules
- Efficient tax rate lookups with indexed queries
- Tax audit log partitioned by date for performance (if volume is high)
- Horizontal scaling support for tax calculation service

---

## Future Enhancements

- Support for multiple tax frameworks per country
- Support for line-item-specific tax exemptions
- Automatic tax rate updates via third-party APIs
- Support for complex tax structures (compound taxes, tax-on-tax)
- Integration with tax compliance reporting tools
- Support for reverse charge mechanisms
- Tax certificate generation per quotation
- Multi-level jurisdiction hierarchies (beyond 3 levels)
- Tax calculation for multi-currency quotations

---

**End of Spec-020: Multi-Country & Jurisdiction Tax Management**

