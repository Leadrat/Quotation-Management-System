# Tasks: Country-Specific Company Identifiers & Bank Details (Spec-023)

**Input**: Design documents from `/specs/023-country-company-identifiers/`  
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Tests are included as per constitution requirements (≥85% backend, ≥80% frontend coverage).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Backend**: `src/Backend/CRM.{Layer}/`
- **Frontend**: `src/Frontend/web/src/`
- **Tests**: `tests/CRM.Tests.{Type}/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [x] T001 Create CompanyIdentifiers feature module directory structure in `src/Backend/CRM.Application/CompanyIdentifiers/`
- [x] T002 [P] Create CompanyBankDetails feature module directory structure in `src/Backend/CRM.Application/CompanyBankDetails/`
- [x] T003 [P] Create feature module directory structures in `src/Backend/CRM.Domain/Entities/`
- [x] T004 [P] Create feature module directory structures in `src/Backend/CRM.Infrastructure/EntityConfigurations/`
- [x] T005 [P] Create feature module directory structures in `src/Frontend/web/src/app/admin/company-identifiers/`
- [x] T006 [P] Create feature module directory structures in `src/Frontend/web/src/app/admin/company-bank-fields/`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T007 Create IdentifierType entity in `src/Backend/CRM.Domain/Entities/IdentifierType.cs`
- [x] T008 [P] Create CountryIdentifierConfiguration entity in `src/Backend/CRM.Domain/Entities/CountryIdentifierConfiguration.cs`
- [x] T009 [P] Create BankFieldType entity in `src/Backend/CRM.Domain/Entities/BankFieldType.cs`
- [x] T010 [P] Create CountryBankFieldConfiguration entity in `src/Backend/CRM.Domain/Entities/CountryBankFieldConfiguration.cs`
- [x] T011 Modify CompanyDetails entity to add IdentifierValues JSONB column in `src/Backend/CRM.Domain/Entities/CompanyDetails.cs`
- [x] T012 [P] Modify CompanyBankDetails entity to add FieldValues JSONB column in `src/Backend/CRM.Domain/Entities/BankDetails.cs`
- [x] T013 Create IdentifierTypeEntityConfiguration in `src/Backend/CRM.Infrastructure/EntityConfigurations/IdentifierTypeEntityConfiguration.cs`
- [x] T014 [P] Create CountryIdentifierConfigurationEntityConfiguration in `src/Backend/CRM.Infrastructure/EntityConfigurations/CountryIdentifierConfigurationEntityConfiguration.cs`
- [x] T015 [P] Create BankFieldTypeEntityConfiguration in `src/Backend/CRM.Infrastructure/EntityConfigurations/BankFieldTypeEntityConfiguration.cs`
- [x] T016 [P] Create CountryBankFieldConfigurationEntityConfiguration in `src/Backend/CRM.Infrastructure/EntityConfigurations/CountryBankFieldConfigurationEntityConfiguration.cs`
- [x] T017 Modify CompanyDetailsEntityConfiguration to configure IdentifierValues JSONB column in `src/Backend/CRM.Infrastructure/EntityConfigurations/CompanyDetailsEntityConfiguration.cs`
- [x] T018 [P] Modify BankDetailsEntityConfiguration to configure FieldValues JSONB column in `src/Backend/CRM.Infrastructure/EntityConfigurations/BankDetailsEntityConfiguration.cs`
- [x] T019 Register all entity configurations in `src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs`
- [x] T020 Create EF Core migration for IdentifierTypes, CountryIdentifierConfigurations, BankFieldTypes, CountryBankFieldConfigurations tables and JSONB column additions in `src/Backend/CRM.Infrastructure/Migrations/`
- [x] T021 Apply migration to database

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Admin Configures Country-Specific Identifiers (Priority: P1) 🎯 MVP

**Goal**: Admin user accesses the master configuration to define which company identifiers are required for each country (e.g., PAN for India, VAT for EU, Business License for Dubai) and sets validation rules for each identifier type per country.

**Independent Test**: Admin logs in, navigates to identifier configuration, adds PAN identifier for India with format validation, adds VAT identifier for EU countries with format validation, and verifies both configurations are saved and displayed correctly.

**Dependencies**: Phase 2 must be complete

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T022 [P] [US1] Unit test for IdentifierType entity validation in `tests/CRM.Tests/Domain/IdentifierTypeTests.cs`
- [ ] T023 [P] [US1] Unit test for CountryIdentifierConfiguration entity validation in `tests/CRM.Tests/Domain/CountryIdentifierConfigurationTests.cs`
- [ ] T024 [P] [US1] Unit test for identifier value validation against regex patterns in `tests/CRM.Tests/Application/CompanyIdentifiers/IdentifierValidationTests.cs`
- [ ] T025 [US1] Integration test for GET /api/v1/identifier-types endpoint in `tests/CRM.Tests.Integration/CompanyIdentifiers/IdentifierTypesControllerTests.cs`
- [ ] T026 [US1] Integration test for POST /api/v1/identifier-types endpoint in `tests/CRM.Tests.Integration/CompanyIdentifiers/IdentifierTypesControllerTests.cs`
- [ ] T027 [US1] Integration test for PUT /api/v1/identifier-types/{id} endpoint in `tests/CRM.Tests.Integration/CompanyIdentifiers/IdentifierTypesControllerTests.cs`
- [ ] T028 [US1] Integration test for GET /api/v1/countries/{countryId}/identifier-configurations endpoint in `tests/CRM.Tests.Integration/CompanyIdentifiers/CountryIdentifierConfigurationsControllerTests.cs`
- [ ] T029 [US1] Integration test for POST /api/v1/countries/{countryId}/identifier-configurations endpoint in `tests/CRM.Tests.Integration/CompanyIdentifiers/CountryIdentifierConfigurationsControllerTests.cs`
- [ ] T030 [US1] Integration test for duplicate identifier type configuration prevention in `tests/CRM.Tests.Integration/CompanyIdentifiers/CountryIdentifierConfigurationsControllerTests.cs`
- [ ] T031 [US1] Integration test for admin-only access enforcement in `tests/CRM.Tests.Integration/CompanyIdentifiers/IdentifierTypesControllerTests.cs`

### Implementation for User Story 1

#### DTOs and Requests

- [x] T032 [P] [US1] Create IdentifierTypeDto in `src/Backend/CRM.Application/CompanyIdentifiers/DTOs/IdentifierTypeDto.cs`
- [x] T033 [P] [US1] Create CountryIdentifierConfigurationDto in `src/Backend/CRM.Application/CompanyIdentifiers/DTOs/CountryIdentifierConfigurationDto.cs`
- [x] T034 [P] [US1] Create CreateIdentifierTypeRequest in `src/Backend/CRM.Application/CompanyIdentifiers/DTOs/CreateIdentifierTypeRequest.cs`
- [x] T035 [P] [US1] Create UpdateIdentifierTypeRequest in `src/Backend/CRM.Application/CompanyIdentifiers/DTOs/UpdateIdentifierTypeRequest.cs`
- [x] T036 [P] [US1] Create ConfigureCountryIdentifierRequest in `src/Backend/CRM.Application/CompanyIdentifiers/DTOs/ConfigureCountryIdentifierRequest.cs`
- [x] T037 [P] [US1] Create UpdateCountryIdentifierConfigurationRequest in `src/Backend/CRM.Application/CompanyIdentifiers/DTOs/UpdateCountryIdentifierConfigurationRequest.cs`

#### Validators

- [x] T038 [P] [US1] Create CreateIdentifierTypeRequestValidator in `src/Backend/CRM.Application/CompanyIdentifiers/Validators/CreateIdentifierTypeRequestValidator.cs`
- [x] T039 [P] [US1] Create UpdateIdentifierTypeRequestValidator in `src/Backend/CRM.Application/CompanyIdentifiers/Validators/UpdateIdentifierTypeRequestValidator.cs`
- [x] T040 [US1] Create ConfigureCountryIdentifierRequestValidator in `src/Backend/CRM.Application/CompanyIdentifiers/Validators/ConfigureCountryIdentifierRequestValidator.cs`
- [x] T041 [US1] Create UpdateCountryIdentifierConfigurationRequestValidator in `src/Backend/CRM.Application/CompanyIdentifiers/Validators/UpdateCountryIdentifierConfigurationRequestValidator.cs`

#### Commands and Handlers

- [x] T042 [US1] Create CreateIdentifierTypeCommand in `src/Backend/CRM.Application/CompanyIdentifiers/Commands/CreateIdentifierTypeCommand.cs`
- [x] T043 [US1] Create CreateIdentifierTypeCommandHandler in `src/Backend/CRM.Application/CompanyIdentifiers/Commands/Handlers/CreateIdentifierTypeCommandHandler.cs`
- [x] T044 [US1] Create UpdateIdentifierTypeCommand in `src/Backend/CRM.Application/CompanyIdentifiers/Commands/UpdateIdentifierTypeCommand.cs`
- [x] T045 [US1] Create UpdateIdentifierTypeCommandHandler in `src/Backend/CRM.Application/CompanyIdentifiers/Commands/Handlers/UpdateIdentifierTypeCommandHandler.cs`
- [x] T046 [US1] Create ConfigureCountryIdentifierCommand in `src/Backend/CRM.Application/CompanyIdentifiers/Commands/ConfigureCountryIdentifierCommand.cs`
- [x] T047 [US1] Create ConfigureCountryIdentifierCommandHandler in `src/Backend/CRM.Application/CompanyIdentifiers/Commands/Handlers/ConfigureCountryIdentifierCommandHandler.cs`
- [x] T048 [US1] Create UpdateCountryIdentifierConfigurationCommand in `src/Backend/CRM.Application/CompanyIdentifiers/Commands/UpdateCountryIdentifierConfigurationCommand.cs`
- [x] T049 [US1] Create UpdateCountryIdentifierConfigurationCommandHandler in `src/Backend/CRM.Application/CompanyIdentifiers/Commands/Handlers/UpdateCountryIdentifierConfigurationCommandHandler.cs`

#### Queries and Handlers

- [x] T050 [US1] Create GetIdentifierTypesQuery in `src/Backend/CRM.Application/CompanyIdentifiers/Queries/GetIdentifierTypesQuery.cs`
- [x] T051 [US1] Create GetIdentifierTypesQueryHandler in `src/Backend/CRM.Application/CompanyIdentifiers/Queries/Handlers/GetIdentifierTypesQueryHandler.cs`
- [x] T052 [US1] Create GetCountryIdentifierConfigurationsQuery in `src/Backend/CRM.Application/CompanyIdentifiers/Queries/GetCountryIdentifierConfigurationsQuery.cs`
- [x] T053 [US1] Create GetCountryIdentifierConfigurationsQueryHandler in `src/Backend/CRM.Application/CompanyIdentifiers/Queries/Handlers/GetCountryIdentifierConfigurationsQueryHandler.cs`

#### AutoMapper Configuration

- [x] T054 [US1] Create CompanyIdentifiersProfile AutoMapper configuration in `src/Backend/CRM.Application/Mapping/CompanyIdentifiersProfile.cs`
- [x] T055 [US1] Register CompanyIdentifiersProfile in `src/Backend/CRM.Api/Program.cs`

#### Controllers

- [x] T056 [US1] Create IdentifierTypesController in `src/Backend/CRM.Api/Controllers/IdentifierTypesController.cs`
- [x] T057 [US1] Create CountryIdentifierConfigurationsController in `src/Backend/CRM.Api/Controllers/CountryIdentifierConfigurationsController.cs`
- [x] T058 [US1] Register handlers in dependency injection in `src/Backend/CRM.Api/Program.cs`

#### Frontend - API Client

- [x] T059 [P] [US1] Create identifierTypes API client methods in `src/Frontend/web/src/lib/api/identifierTypes.ts`
- [x] T060 [P] [US1] Create countryIdentifierConfigurations API client methods in `src/Frontend/web/src/lib/api/countryIdentifierConfigurations.ts`

#### Frontend - Components

- [ ] T061 [P] [US1] Create IdentifierTypeForm component in `src/Frontend/web/src/components/admin/IdentifierTypeForm.tsx`
- [ ] T062 [P] [US1] Create CountryIdentifierConfigurationForm component in `src/Frontend/web/src/components/admin/CountryIdentifierConfigurationForm.tsx`
- [ ] T063 [US1] Create IdentifierTypesList component in `src/Frontend/web/src/components/admin/IdentifierTypesList.tsx`
- [ ] T064 [US1] Create CountryIdentifierConfigurationsList component in `src/Frontend/web/src/components/admin/CountryIdentifierConfigurationsList.tsx`

#### Frontend - Pages

- [x] T065 [US1] Create admin Identifier Types master page in `src/Frontend/web/src/app/admin/company-identifiers/page.tsx`
- [x] T066 [US1] Create admin Country Identifier Configuration page in `src/Frontend/web/src/app/admin/company-identifiers/config/[countryId]/page.tsx`
- [x] T067 [US1] Add navigation items to AppSidebar in `src/Frontend/web/src/layout/AppSidebar.tsx` (admin-only)
- [x] T068 [US1] Add route protection for admin-only access in frontend pages (via admin route protection)

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently. Admin can configure identifier types and country-specific identifier configurations.

---

## Phase 4: User Story 2 - Admin Configures Country-Specific Bank Fields (Priority: P1)

**Goal**: Admin user accesses the master configuration to define which bank detail fields are relevant for each country (e.g., IFSC for India, IBAN/SWIFT for Dubai/UAE, routing codes for US) and sets validation rules for each field per country.

**Independent Test**: Admin configures IFSC field for India, IBAN and SWIFT fields for Dubai, routing number field for US, sets validation rules for each, and verifies configurations are saved correctly.

**Dependencies**: Phase 2 must be complete (can be parallel with US1 after Phase 2)

### Tests for User Story 2

- [ ] T069 [P] [US2] Unit test for BankFieldType entity validation in `tests/CRM.Tests/Domain/BankFieldTypeTests.cs`
- [ ] T070 [P] [US2] Unit test for CountryBankFieldConfiguration entity validation in `tests/CRM.Tests/Domain/CountryBankFieldConfigurationTests.cs`
- [ ] T071 [P] [US2] Unit test for bank field value validation against regex patterns in `tests/CRM.Tests/Application/CompanyBankDetails/BankFieldValidationTests.cs`
- [ ] T072 [US2] Integration test for GET /api/v1/bank-field-types endpoint in `tests/CRM.Tests.Integration/CompanyBankDetails/BankFieldTypesControllerTests.cs`
- [ ] T073 [US2] Integration test for POST /api/v1/bank-field-types endpoint in `tests/CRM.Tests.Integration/CompanyBankDetails/BankFieldTypesControllerTests.cs`
- [ ] T074 [US2] Integration test for PUT /api/v1/bank-field-types/{id} endpoint in `tests/CRM.Tests.Integration/CompanyBankDetails/BankFieldTypesControllerTests.cs`
- [ ] T075 [US2] Integration test for GET /api/v1/countries/{countryId}/bank-field-configurations endpoint in `tests/CRM.Tests.Integration/CompanyBankDetails/CountryBankFieldConfigurationsControllerTests.cs`
- [ ] T076 [US2] Integration test for POST /api/v1/countries/{countryId}/bank-field-configurations endpoint in `tests/CRM.Tests.Integration/CompanyBankDetails/CountryBankFieldConfigurationsControllerTests.cs`
- [ ] T077 [US2] Integration test for duplicate bank field configuration prevention in `tests/CRM.Tests.Integration/CompanyBankDetails/CountryBankFieldConfigurationsControllerTests.cs`
- [ ] T078 [US2] Integration test for admin-only access enforcement in `tests/CRM.Tests.Integration/CompanyBankDetails/BankFieldTypesControllerTests.cs`

### Implementation for User Story 2

#### DTOs and Requests

- [x] T079 [P] [US2] Create BankFieldTypeDto in `src/Backend/CRM.Application/CompanyBankDetails/DTOs/BankFieldTypeDto.cs`
- [x] T080 [P] [US2] Create CountryBankFieldConfigurationDto in `src/Backend/CRM.Application/CompanyBankDetails/DTOs/CountryBankFieldConfigurationDto.cs`
- [x] T081 [P] [US2] Create CreateBankFieldTypeRequest in `src/Backend/CRM.Application/CompanyBankDetails/DTOs/CreateBankFieldTypeRequest.cs`
- [x] T082 [P] [US2] Create UpdateBankFieldTypeRequest in `src/Backend/CRM.Application/CompanyBankDetails/DTOs/UpdateBankFieldTypeRequest.cs`
- [x] T083 [P] [US2] Create ConfigureCountryBankFieldRequest in `src/Backend/CRM.Application/CompanyBankDetails/DTOs/ConfigureCountryBankFieldRequest.cs`
- [x] T084 [P] [US2] Create UpdateCountryBankFieldConfigurationRequest in `src/Backend/CRM.Application/CompanyBankDetails/DTOs/UpdateCountryBankFieldConfigurationRequest.cs`

#### Validators

- [x] T085 [P] [US2] Create CreateBankFieldTypeRequestValidator in `src/Backend/CRM.Application/CompanyBankDetails/Validators/CreateBankFieldTypeRequestValidator.cs`
- [x] T086 [P] [US2] Create UpdateBankFieldTypeRequestValidator in `src/Backend/CRM.Application/CompanyBankDetails/Validators/UpdateBankFieldTypeRequestValidator.cs`
- [x] T087 [US2] Create ConfigureCountryBankFieldRequestValidator in `src/Backend/CRM.Application/CompanyBankDetails/Validators/ConfigureCountryBankFieldRequestValidator.cs`
- [x] T088 [US2] Create UpdateCountryBankFieldConfigurationRequestValidator in `src/Backend/CRM.Application/CompanyBankDetails/Validators/UpdateCountryBankFieldConfigurationRequestValidator.cs`

#### Commands and Handlers

- [x] T089 [US2] Create CreateBankFieldTypeCommand in `src/Backend/CRM.Application/CompanyBankDetails/Commands/CreateBankFieldTypeCommand.cs`
- [x] T090 [US2] Create CreateBankFieldTypeCommandHandler in `src/Backend/CRM.Application/CompanyBankDetails/Commands/Handlers/CreateBankFieldTypeCommandHandler.cs`
- [x] T091 [US2] Create UpdateBankFieldTypeCommand in `src/Backend/CRM.Application/CompanyBankDetails/Commands/UpdateBankFieldTypeCommand.cs`
- [x] T092 [US2] Create UpdateBankFieldTypeCommandHandler in `src/Backend/CRM.Application/CompanyBankDetails/Commands/Handlers/UpdateBankFieldTypeCommandHandler.cs`
- [x] T093 [US2] Create ConfigureCountryBankFieldCommand in `src/Backend/CRM.Application/CompanyBankDetails/Commands/ConfigureCountryBankFieldCommand.cs`
- [x] T094 [US2] Create ConfigureCountryBankFieldCommandHandler in `src/Backend/CRM.Application/CompanyBankDetails/Commands/Handlers/ConfigureCountryBankFieldCommandHandler.cs`
- [x] T095 [US2] Create UpdateCountryBankFieldConfigurationCommand in `src/Backend/CRM.Application/CompanyBankDetails/Commands/UpdateCountryBankFieldConfigurationCommand.cs`
- [x] T096 [US2] Create UpdateCountryBankFieldConfigurationCommandHandler in `src/Backend/CRM.Application/CompanyBankDetails/Commands/Handlers/UpdateCountryBankFieldConfigurationCommandHandler.cs`

#### Queries and Handlers

- [x] T097 [US2] Create GetBankFieldTypesQuery in `src/Backend/CRM.Application/CompanyBankDetails/Queries/GetBankFieldTypesQuery.cs`
- [x] T098 [US2] Create GetBankFieldTypesQueryHandler in `src/Backend/CRM.Application/CompanyBankDetails/Queries/Handlers/GetBankFieldTypesQueryHandler.cs`
- [x] T099 [US2] Create GetCountryBankFieldConfigurationsQuery in `src/Backend/CRM.Application/CompanyBankDetails/Queries/GetCountryBankFieldConfigurationsQuery.cs`
- [x] T100 [US2] Create GetCountryBankFieldConfigurationsQueryHandler in `src/Backend/CRM.Application/CompanyBankDetails/Queries/Handlers/GetCountryBankFieldConfigurationsQueryHandler.cs`

#### AutoMapper Configuration

- [x] T101 [US2] Create CompanyBankDetailsProfile AutoMapper configuration in `src/Backend/CRM.Application/Mapping/CompanyBankDetailsProfile.cs`
- [x] T102 [US2] Register CompanyBankDetailsProfile in `src/Backend/CRM.Api/Program.cs`

#### Controllers

- [x] T103 [US2] Create BankFieldTypesController in `src/Backend/CRM.Api/Controllers/BankFieldTypesController.cs`
- [x] T104 [US2] Create CountryBankFieldConfigurationsController in `src/Backend/CRM.Api/Controllers/CountryBankFieldConfigurationsController.cs`
- [x] T105 [US2] Register handlers in dependency injection in `src/Backend/CRM.Api/Program.cs`

#### Frontend - API Client

- [x] T106 [P] [US2] Create bankFieldTypes API client methods in `src/Frontend/web/src/lib/api/bankFieldTypes.ts`
- [x] T107 [P] [US2] Create countryBankFieldConfigurations API client methods in `src/Frontend/web/src/lib/api/countryBankFieldConfigurations.ts`

#### Frontend - Components

- [x] T108 [P] [US2] Create BankFieldTypeForm component in `src/Frontend/web/src/components/admin/BankFieldTypeForm.tsx` (integrated into page)
- [x] T109 [P] [US2] Create CountryBankFieldConfigurationForm component in `src/Frontend/web/src/components/admin/CountryBankFieldConfigurationForm.tsx` (integrated into page)
- [x] T110 [US2] Create BankFieldTypesList component in `src/Frontend/web/src/components/admin/BankFieldTypesList.tsx` (integrated into page)
- [x] T111 [US2] Create CountryBankFieldConfigurationsList component in `src/Frontend/web/src/components/admin/CountryBankFieldConfigurationsList.tsx` (integrated into page)

#### Frontend - Pages

- [x] T112 [US2] Create admin Bank Field Types master page in `src/Frontend/web/src/app/admin/company-bank-fields/page.tsx`
- [x] T113 [US2] Create admin Country Bank Field Configuration page in `src/Frontend/web/src/app/admin/company-bank-fields/config/[countryId]/page.tsx`
- [x] T114 [US2] Add navigation items to AppSidebar in `src/Frontend/web/src/layout/AppSidebar.tsx` (admin-only)

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently. Admin can configure identifier types and bank field types with country-specific configurations.

---

## Phase 5: User Story 3 - Admin Manages Company Identifiers by Country (Priority: P1)

**Goal**: Admin user accesses the Company Details page to add or edit company identifier values. The form dynamically displays only the identifier fields configured for the selected company country, with appropriate validation and help text.

**Independent Test**: Admin selects India as company country, sees only PAN field (if configured), enters a PAN value, verifies validation, saves, then changes country to EU and sees only VAT field appear.

**Dependencies**: Phase 2 and Phase 3 (US1) must be complete

### Tests for User Story 3

- [ ] T115 [P] [US3] Unit test for identifier value JSONB serialization/deserialization in `tests/CRM.Tests/Application/CompanyIdentifiers/CompanyIdentifierValueTests.cs`
- [ ] T116 [P] [US3] Unit test for country-specific identifier value validation in `tests/CRM.Tests/Application/CompanyIdentifiers/CompanyIdentifierValueValidationTests.cs`
- [ ] T117 [US3] Integration test for GET /api/v1/company-details/{countryId}/identifiers endpoint in `tests/CRM.Tests.Integration/CompanyIdentifiers/CompanyIdentifiersControllerTests.cs`
- [ ] T118 [US3] Integration test for PUT /api/v1/company-details/{countryId}/identifiers endpoint in `tests/CRM.Tests.Integration/CompanyIdentifiers/CompanyIdentifiersControllerTests.cs`
- [ ] T119 [US3] Integration test for required field validation in `tests/CRM.Tests.Integration/CompanyIdentifiers/CompanyIdentifiersControllerTests.cs`
- [ ] T120 [US3] Integration test for regex pattern validation in `tests/CRM.Tests.Integration/CompanyIdentifiers/CompanyIdentifiersControllerTests.cs`

### Implementation for User Story 3

#### DTOs and Requests

- [x] T121 [US3] Create CompanyIdentifierValuesDto in `src/Backend/CRM.Application/CompanyIdentifiers/DTOs/CompanyIdentifierValuesDto.cs`
- [x] T122 [US3] Create SaveCompanyIdentifierValuesRequest in `src/Backend/CRM.Application/CompanyIdentifiers/DTOs/SaveCompanyIdentifierValuesRequest.cs`

#### Validators

- [x] T123 [US3] Create SaveCompanyIdentifierValuesRequestValidator that validates against country configuration in `src/Backend/CRM.Application/CompanyIdentifiers/Validators/SaveCompanyIdentifierValuesRequestValidator.cs`

#### Commands and Handlers

- [x] T124 [US3] Create SaveCompanyIdentifierValuesCommand in `src/Backend/CRM.Application/CompanyIdentifiers/Commands/SaveCompanyIdentifierValuesCommand.cs`
- [x] T125 [US3] Create SaveCompanyIdentifierValuesCommandHandler that updates JSONB column in `src/Backend/CRM.Application/CompanyIdentifiers/Commands/Handlers/SaveCompanyIdentifierValuesCommandHandler.cs`

#### Queries and Handlers

- [x] T126 [US3] Create GetCompanyIdentifierValuesQuery in `src/Backend/CRM.Application/CompanyIdentifiers/Queries/GetCompanyIdentifierValuesQuery.cs`
- [x] T127 [US3] Create GetCompanyIdentifierValuesQueryHandler that retrieves from JSONB and merges with configurations in `src/Backend/CRM.Application/CompanyIdentifiers/Queries/Handlers/GetCompanyIdentifierValuesQueryHandler.cs`

#### Services

- [x] T128 [US3] Create ICompanyIdentifierValidationService interface in `src/Backend/CRM.Application/CompanyIdentifiers/Services/ICompanyIdentifierValidationService.cs`
- [x] T129 [US3] Create CompanyIdentifierValidationService implementation in `src/Backend/CRM.Application/CompanyIdentifiers/Services/CompanyIdentifierValidationService.cs`
- [x] T130 [US3] Register CompanyIdentifierValidationService in dependency injection in `src/Backend/CRM.Api/Program.cs`

#### Controllers

- [x] T131 [US3] Create CompanyIdentifiersController in `src/Backend/CRM.Api/Controllers/CompanyIdentifiersController.cs`
- [x] T132 [US3] Register handlers in dependency injection in `src/Backend/CRM.Api/Program.cs`

#### Frontend - API Client

- [x] T133 [US3] Create companyIdentifiers API client methods in `src/Frontend/web/src/lib/api/companyIdentifiers.ts`

#### Frontend - Components

- [x] T134 [US3] Create DynamicCompanyIdentifiersForm component that loads configurations based on country selection in `src/Frontend/web/src/components/admin/DynamicCompanyIdentifiersForm.tsx`
- [x] T135 [US3] Implement real-time validation in DynamicCompanyIdentifiersForm component

#### Frontend - Pages

- [x] T136 [US3] Modify Company Details page to include dynamic identifier fields section in `src/Frontend/web/src/app/admin/company-details/page.tsx`
- [x] T137 [US3] Implement country selection change handler to reload identifier fields dynamically

**Checkpoint**: At this point, User Story 3 should be fully functional. Admin can enter company identifier values that dynamically adjust based on country selection.

---

## Phase 6: User Story 4 - Admin Manages Bank Details by Country (Priority: P1)

**Goal**: Admin user accesses the Company Details page to add or edit bank details. The form dynamically displays only the bank fields configured for the selected company country, with appropriate validation and help text.

**Independent Test**: Admin selects India as company country, sees only IFSC field, enters IFSC code, verifies validation, saves, then changes country to Dubai and sees IBAN and SWIFT fields appear.

**Dependencies**: Phase 2 and Phase 4 (US2) must be complete

### Tests for User Story 4

- [ ] T138 [P] [US4] Unit test for bank field value JSONB serialization/deserialization in `tests/CRM.Tests/Application/CompanyBankDetails/CompanyBankDetailsValueTests.cs`
- [ ] T139 [P] [US4] Unit test for country-specific bank field value validation in `tests/CRM.Tests/Application/CompanyBankDetails/CompanyBankDetailsValueValidationTests.cs`
- [ ] T140 [US4] Integration test for GET /api/v1/company-details/{countryId}/bank-details endpoint in `tests/CRM.Tests.Integration/CompanyBankDetails/CompanyBankDetailsControllerTests.cs`
- [ ] T141 [US4] Integration test for PUT /api/v1/company-details/{countryId}/bank-details endpoint in `tests/CRM.Tests.Integration/CompanyBankDetails/CompanyBankDetailsControllerTests.cs`
- [ ] T142 [US4] Integration test for required field validation in `tests/CRM.Tests.Integration/CompanyBankDetails/CompanyBankDetailsControllerTests.cs`
- [ ] T143 [US4] Integration test for regex pattern validation in `tests/CRM.Tests.Integration/CompanyBankDetails/CompanyBankDetailsControllerTests.cs`

### Implementation for User Story 4

#### DTOs and Requests

- [x] T144 [US4] Create CompanyBankDetailsDto in `src/Backend/CRM.Application/CompanyBankDetails/DTOs/CompanyBankDetailsDto.cs`
- [x] T145 [US4] Create SaveCompanyBankDetailsRequest in `src/Backend/CRM.Application/CompanyBankDetails/DTOs/SaveCompanyBankDetailsRequest.cs`

#### Validators

- [x] T146 [US4] Create SaveCompanyBankDetailsRequestValidator that validates against country configuration in `src/Backend/CRM.Application/CompanyBankDetails/Validators/SaveCompanyBankDetailsRequestValidator.cs`

#### Commands and Handlers

- [x] T147 [US4] Modify SaveCompanyBankDetailsCommand to update JSONB FieldValues column in `src/Backend/CRM.Application/CompanyBankDetails/Commands/SaveCompanyBankDetailsCommand.cs`
- [x] T148 [US4] Modify SaveCompanyBankDetailsCommandHandler to update JSONB column in `src/Backend/CRM.Application/CompanyBankDetails/Commands/Handlers/SaveCompanyBankDetailsCommandHandler.cs`

#### Queries and Handlers

- [x] T149 [US4] Modify GetCompanyBankDetailsQuery to retrieve from JSONB and merge with configurations in `src/Backend/CRM.Application/CompanyBankDetails/Queries/GetCompanyBankDetailsQuery.cs`
- [x] T150 [US4] Modify GetCompanyBankDetailsQueryHandler in `src/Backend/CRM.Application/CompanyBankDetails/Queries/Handlers/GetCompanyBankDetailsQueryHandler.cs`

#### Services

- [x] T151 [US4] Create ICompanyBankDetailsValidationService interface in `src/Backend/CRM.Application/CompanyBankDetails/Services/ICompanyBankDetailsValidationService.cs`
- [x] T152 [US4] Create CompanyBankDetailsValidationService implementation in `src/Backend/CRM.Application/CompanyBankDetails/Services/CompanyBankDetailsValidationService.cs`
- [x] T153 [US4] Register CompanyBankDetailsValidationService in dependency injection in `src/Backend/CRM.Api/Program.cs`

#### Controllers

- [x] T154 [US4] Modify CompanyBankDetailsController to use new JSONB-based endpoints in `src/Backend/CRM.Api/Controllers/CompanyBankDetailsController.cs`

#### Frontend - Components

- [x] T155 [US4] Create DynamicCompanyBankDetailsForm component that loads configurations based on country selection in `src/Frontend/web/src/components/admin/DynamicCompanyBankDetailsForm.tsx`
- [x] T156 [US4] Implement real-time validation in DynamicCompanyBankDetailsForm component

#### Frontend - Pages

- [x] T157 [US4] Modify Company Details page to include dynamic bank details fields section in `src/Frontend/web/src/app/admin/company-details/page.tsx`
- [x] T158 [US4] Implement country selection change handler to reload bank detail fields dynamically

**Checkpoint**: At this point, User Stories 3 AND 4 should both work independently. Admin can enter company identifier and bank detail values that dynamically adjust based on country selection.

---

## Phase 7: User Story 5 - Sales Rep Sees Country-Specific Company Details in Quotations (Priority: P1)

**Goal**: When a sales representative creates or edits a quotation, the system automatically includes only those company identifiers and bank details relevant for the client's country. The quotation form and PDF display only the appropriate information.

**Independent Test**: Configure company details with India identifiers and bank details, create a quotation for a client in India, verify India-specific details appear, then create a quotation for a Dubai client and verify Dubai-specific details appear.

**Dependencies**: Phase 3 (US1), Phase 4 (US2), Phase 5 (US3), and Phase 6 (US4) must be complete

### Tests for User Story 5

- [ ] T159 [P] [US5] Unit test for company details filtering by country in `tests/CRM.Tests/Application/Quotations/QuotationCompanyDetailsServiceTests.cs`
- [ ] T160 [P] [US5] Unit test for identifier values extraction from JSONB in `tests/CRM.Tests/Application/Quotations/QuotationCompanyDetailsServiceTests.cs`
- [ ] T161 [P] [US5] Unit test for bank details extraction from JSONB in `tests/CRM.Tests/Application/Quotations/QuotationCompanyDetailsServiceTests.cs`
- [ ] T162 [US5] Integration test for quotation creation with country-specific company details in `tests/CRM.Tests.Integration/Quotations/CreateQuotationCommandHandlerTests.cs`
- [ ] T163 [US5] Integration test for quotation PDF generation with country-specific company details in `tests/CRM.Tests.Integration/Quotations/QuotationPdfGenerationServiceTests.cs`
- [ ] T164 [US5] Integration test for quotation email with country-specific company details in `tests/CRM.Tests.Integration/Quotations/QuotationEmailServiceTests.cs`
- [ ] T165 [US5] Integration test for historical accuracy (old quotations preserve original details) in `tests/CRM.Tests.Integration/Quotations/QuotationPdfGenerationServiceTests.cs`

### Implementation for User Story 5

#### Services

- [x] T166 [US5] Modify QuotationCompanyDetailsService to filter identifiers by client country in `src/Backend/CRM.Application/Quotations/Services/QuotationCompanyDetailsService.cs`
- [x] T167 [US5] Modify QuotationCompanyDetailsService to filter bank details by client country in `src/Backend/CRM.Application/Quotations/Services/QuotationCompanyDetailsService.cs`
- [x] T168 [US5] Implement JSONB extraction logic for identifier values in QuotationCompanyDetailsService
- [x] T169 [US5] Implement JSONB extraction logic for bank field values in QuotationCompanyDetailsService
- [x] T170 [US5] Add caching for country-specific company details in QuotationCompanyDetailsService (IMemoryCache)

#### Quotation Creation

- [x] T171 [US5] Modify CreateQuotationCommandHandler to include country-specific company details snapshot in `src/Backend/CRM.Application/Quotations/Commands/Handlers/CreateQuotationCommandHandler.cs`
- [x] T172 [US5] Modify UpdateQuotationCommandHandler to update company details snapshot if client country changes in `src/Backend/CRM.Application/Quotations/Commands/Handlers/UpdateQuotationCommandHandler.cs`

#### PDF Generation

- [x] T173 [US5] Modify QuotationPdfGenerationService to display only country-specific identifiers in PDF header in `src/Backend/CRM.Application/Quotations/Services/QuotationPdfGenerationService.cs`
- [x] T174 [US5] Modify QuotationPdfGenerationService to display only country-specific bank details in PDF footer in `src/Backend/CRM.Application/Quotations/Services/QuotationPdfGenerationService.cs`

#### Email Generation

- [x] T175 [US5] Modify QuotationEmailService to include only country-specific company details in email template in `src/Backend/CRM.Application/Quotations/Services/QuotationEmailService.cs`

#### Frontend - Components

- [ ] T176 [US5] Modify QuotationCompanyDetailsDisplay component to show only country-specific fields in `src/Frontend/web/src/components/quotations/QuotationCompanyDetailsDisplay.tsx`
- [ ] T177 [US5] Implement country change handler in quotation form to update company details display

#### Frontend - Pages

- [ ] T178 [US5] Modify quotation creation/editing page to dynamically update company details when client country changes in `src/Frontend/web/src/app/(protected)/quotations/[id]/page.tsx`

**Checkpoint**: At this point, all user stories should be complete. Quotations automatically display country-specific company identifiers and bank details based on client country.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Final polish, performance optimization, error handling, and audit logging

- [ ] T179 Add audit logging for master configuration changes in command handlers
- [ ] T180 [P] Add audit logging for company identifier value changes in SaveCompanyIdentifierValuesCommandHandler
- [ ] T181 [P] Add audit logging for company bank details changes in SaveCompanyBankDetailsCommandHandler
- [ ] T182 Implement error handling for missing country configurations in QuotationCompanyDetailsService
- [ ] T183 [P] Add error messages for missing required identifiers/bank fields in quotation creation
- [ ] T184 [P] Implement cache invalidation on configuration updates
- [ ] T185 Add performance monitoring for JSONB queries
- [ ] T186 [P] Add database indexes optimization (GIN indexes on JSONB columns)
- [ ] T187 [P] Implement graceful degradation when country configuration is disabled
- [ ] T188 Add comprehensive error messages for validation failures
- [ ] T189 [P] Add loading states for dynamic form field rendering
- [ ] T190 [P] Implement form field validation debouncing for better UX
- [ ] T191 Add seed data script for initial identifier types and bank field types in `src/Backend/CRM.Infrastructure/Migrations/SeedData/`
- [ ] T192 Add seed data script for country configurations in `src/Backend/CRM.Infrastructure/Migrations/SeedData/`
- [ ] T193 [P] Add migration script to migrate existing CompanyDetails data to JSONB format in `src/Backend/CRM.Infrastructure/Migrations/`
- [ ] T194 [P] Add migration script to migrate existing BankDetails data to JSONB format in `src/Backend/CRM.Infrastructure/Migrations/`

---

## Task Summary

**Total Tasks**: 194

### Task Count by User Story

- **Phase 1 (Setup)**: 6 tasks
- **Phase 2 (Foundational)**: 15 tasks
- **Phase 3 (US1 - Identifier Configuration)**: 47 tasks
- **Phase 4 (US2 - Bank Field Configuration)**: 46 tasks
- **Phase 5 (US3 - Company Identifiers Management)**: 23 tasks
- **Phase 6 (US4 - Bank Details Management)**: 21 tasks
- **Phase 7 (US5 - Quotation Integration)**: 20 tasks
- **Phase 8 (Polish)**: 16 tasks

### Parallel Opportunities

Many tasks within each user story phase can be executed in parallel, particularly:
- DTOs and request models (different files)
- Validators (different files)
- Tests (different test files)
- Frontend components (different components)
- API client methods (different files)

### Independent Test Criteria

Each user story can be tested independently:
- **US1**: Admin can configure identifier types and country configurations
- **US2**: Admin can configure bank field types and country configurations
- **US3**: Admin can enter company identifier values that dynamically adjust by country
- **US4**: Admin can enter company bank details that dynamically adjust by country
- **US5**: Sales rep sees country-specific company details in quotations

### Suggested MVP Scope

**MVP**: Phase 3 (User Story 1) - Admin Configures Country-Specific Identifiers

This provides:
- Master configuration capability for identifier types
- Country-specific identifier configuration with validation rules
- Foundation for all other user stories
- Immediate value: Admins can configure the system for their countries

**Next Steps After MVP**:
1. Phase 4 (US2) - Bank field configuration (parallel to US3)
2. Phase 5 (US3) - Company identifiers management
3. Phase 6 (US4) - Bank details management
4. Phase 7 (US5) - Quotation integration

### Implementation Strategy

**MVP First**: Implement US1 completely before moving to other stories.

**Incremental Delivery**:
1. Complete Phase 2 (Foundational) - required for all stories
2. Complete Phase 3 (US1) - MVP
3. Complete Phase 4 (US2) - enables bank details configuration
4. Complete Phase 5 (US3) and Phase 6 (US4) - enables company details entry (can be parallel after US1 and US2)
5. Complete Phase 7 (US5) - enables quotation integration
6. Complete Phase 8 (Polish) - final optimization

### Dependencies

```
Phase 2 (Foundational)
  ├── Phase 3 (US1) - Identifier Configuration
  │     └── Phase 5 (US3) - Company Identifiers Management
  │           └── Phase 7 (US5) - Quotation Integration
  │
  └── Phase 4 (US2) - Bank Field Configuration
        └── Phase 6 (US4) - Bank Details Management
              └── Phase 7 (US5) - Quotation Integration
```

**Parallel Opportunities**:
- US1 and US2 can be developed in parallel after Phase 2
- US3 can start after US1 is complete
- US4 can start after US2 is complete
- US5 requires both US3 and US4 to be complete

---

## Format Validation

✅ All tasks follow the checklist format: `- [ ] [TaskID] [P?] [Story?] Description with file path`
✅ All user story tasks have story labels ([US1], [US2], etc.)
✅ All tasks include exact file paths
✅ Task IDs are sequential (T001, T002, etc.)
✅ Parallel tasks are marked with [P]
✅ Setup and foundational phases have no story labels

