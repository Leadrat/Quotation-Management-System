# Tasks: Multi-Country & Jurisdiction Tax Management (Spec-020)

**Input**: Design documents from `/specs/020-country-tax-management/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅

**Tests**: Tests are OPTIONAL - not explicitly requested in spec. Include basic integration tests for critical flows only.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., [US1], [US2], [US3])
- Include exact file paths in descriptions

## Path Conventions

- **Backend**: `src/Backend/CRM.Domain/`, `src/Backend/CRM.Application/`, `src/Backend/CRM.Infrastructure/`, `src/Backend/CRM.Api/`
- **Frontend**: `src/Frontend/web/src/`
- **Tests**: `src/Backend/CRM.Tests.Integration/`, `src/Frontend/web/tests/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project structure setup and dependency configuration

- [ ] T001 Create TaxManagement folder structure in src/Backend/CRM.Application/TaxManagement/
- [ ] T002 [P] Create Commands, Commands/Handlers, Queries, Queries/Handlers, Dtos, Services, Requests, Validators, Mapping folders in src/Backend/CRM.Application/TaxManagement/
- [ ] T003 [P] Add AutoMapper profile registration in src/Backend/CRM.Application/Mapping/
- [ ] T004 [P] Register FluentValidation validators in src/Backend/CRM.Api/Program.cs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T005 Create TaxFrameworkType enum in src/Backend/CRM.Domain/Enums/TaxFrameworkType.cs (GST, VAT)
- [X] T006 Create TaxCalculationActionType enum in src/Backend/CRM.Domain/Enums/TaxCalculationActionType.cs (Calculation, ConfigurationChange)
- [X] T007 [P] Modify AppDbContext to add DbSets for new tax entities in src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs
- [X] T008 [P] Create database migration for tax management tables in src/Backend/CRM.Infrastructure/Migrations/
- [X] T009 Configure entity relationships and indexes in entity configurations per data-model.md

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Admin Configures Country and Tax Framework (Priority: P1) 🎯 MVP

**Goal**: Admins can add countries (India, UAE) and configure their tax frameworks (GST, VAT) so the system can calculate taxes correctly for quotations in these countries.

**Independent Test**: Admin logs in, navigates to `/admin/tax/countries`, adds India with GST framework and UAE with VAT framework, verifies both countries are saved and displayed in configuration list.

### Implementation for User Story 1

- [ ] T010 [P] [US1] Create Country entity in src/Backend/CRM.Domain/Entities/Country.cs
- [ ] T011 [P] [US1] Create TaxFramework entity in src/Backend/CRM.Domain/Entities/TaxFramework.cs
- [ ] T012 [P] [US1] Create CountryEntityConfiguration in src/Backend/CRM.Infrastructure/EntityConfigurations/CountryEntityConfiguration.cs
- [ ] T013 [P] [US1] Create TaxFrameworkEntityConfiguration in src/Backend/CRM.Infrastructure/EntityConfigurations/TaxFrameworkEntityConfiguration.cs
- [ ] T014 [US1] Create CountryDto in src/Backend/CRM.Application/TaxManagement/Dtos/CountryDto.cs
- [ ] T015 [US1] Create TaxFrameworkDto in src/Backend/CRM.Application/TaxManagement/Dtos/TaxFrameworkDto.cs
- [ ] T016 [P] [US1] Create CreateCountryRequest in src/Backend/CRM.Application/TaxManagement/Requests/CreateCountryRequest.cs
- [ ] T017 [P] [US1] Create UpdateCountryRequest in src/Backend/CRM.Application/TaxManagement/Requests/UpdateCountryRequest.cs
- [ ] T018 [P] [US1] Create CreateTaxFrameworkRequest in src/Backend/CRM.Application/TaxManagement/Requests/CreateTaxFrameworkRequest.cs
- [ ] T019 [P] [US1] Create UpdateTaxFrameworkRequest in src/Backend/CRM.Application/TaxManagement/Requests/UpdateTaxFrameworkRequest.cs
- [ ] T020 [P] [US1] Create CreateCountryRequestValidator in src/Backend/CRM.Application/TaxManagement/Validators/CreateCountryRequestValidator.cs
- [ ] T021 [P] [US1] Create UpdateCountryRequestValidator in src/Backend/CRM.Application/TaxManagement/Validators/UpdateCountryRequestValidator.cs
- [ ] T022 [P] [US1] Create CreateTaxFrameworkRequestValidator in src/Backend/CRM.Application/TaxManagement/Validators/CreateTaxFrameworkRequestValidator.cs
- [ ] T023 [P] [US1] Create UpdateTaxFrameworkRequestValidator in src/Backend/CRM.Application/TaxManagement/Validators/UpdateTaxFrameworkRequestValidator.cs
- [ ] T024 [P] [US1] Create CreateCountryCommand in src/Backend/CRM.Application/TaxManagement/Commands/CreateCountryCommand.cs
- [ ] T025 [P] [US1] Create UpdateCountryCommand in src/Backend/CRM.Application/TaxManagement/Commands/UpdateCountryCommand.cs
- [ ] T026 [P] [US1] Create DeleteCountryCommand in src/Backend/CRM.Application/TaxManagement/Commands/DeleteCountryCommand.cs
- [ ] T027 [P] [US1] Create CreateTaxFrameworkCommand in src/Backend/CRM.Application/TaxManagement/Commands/CreateTaxFrameworkCommand.cs
- [ ] T028 [P] [US1] Create UpdateTaxFrameworkCommand in src/Backend/CRM.Application/TaxManagement/Commands/UpdateTaxFrameworkCommand.cs
- [ ] T029 [US1] Create CreateCountryCommandHandler in src/Backend/CRM.Application/TaxManagement/Commands/Handlers/CreateCountryCommandHandler.cs (depends on T024)
- [ ] T030 [US1] Create UpdateCountryCommandHandler in src/Backend/CRM.Application/TaxManagement/Commands/Handlers/UpdateCountryCommandHandler.cs (depends on T025)
- [ ] T031 [US1] Create DeleteCountryCommandHandler in src/Backend/CRM.Application/TaxManagement/Commands/Handlers/DeleteCountryCommandHandler.cs (depends on T026)
- [ ] T032 [US1] Create CreateTaxFrameworkCommandHandler in src/Backend/CRM.Application/TaxManagement/Commands/Handlers/CreateTaxFrameworkCommandHandler.cs (depends on T027)
- [ ] T033 [US1] Create UpdateTaxFrameworkCommandHandler in src/Backend/CRM.Application/TaxManagement/Commands/Handlers/UpdateTaxFrameworkCommandHandler.cs (depends on T028)
- [ ] T034 [P] [US1] Create GetAllCountriesQuery in src/Backend/CRM.Application/TaxManagement/Queries/GetAllCountriesQuery.cs
- [ ] T035 [P] [US1] Create GetCountryByIdQuery in src/Backend/CRM.Application/TaxManagement/Queries/GetCountryByIdQuery.cs
- [ ] T036 [P] [US1] Create GetAllTaxFrameworksQuery in src/Backend/CRM.Application/TaxManagement/Queries/GetAllTaxFrameworksQuery.cs
- [ ] T037 [P] [US1] Create GetTaxFrameworkByIdQuery in src/Backend/CRM.Application/TaxManagement/Queries/GetTaxFrameworkByIdQuery.cs
- [ ] T038 [US1] Create GetAllCountriesQueryHandler in src/Backend/CRM.Application/TaxManagement/Queries/Handlers/GetAllCountriesQueryHandler.cs (depends on T034)
- [ ] T039 [US1] Create GetCountryByIdQueryHandler in src/Backend/CRM.Application/TaxManagement/Queries/Handlers/GetCountryByIdQueryHandler.cs (depends on T035)
- [ ] T040 [US1] Create GetAllTaxFrameworksQueryHandler in src/Backend/CRM.Application/TaxManagement/Queries/Handlers/GetAllTaxFrameworksQueryHandler.cs (depends on T036)
- [ ] T041 [US1] Create GetTaxFrameworkByIdQueryHandler in src/Backend/CRM.Application/TaxManagement/Queries/Handlers/GetTaxFrameworkByIdQueryHandler.cs (depends on T037)
- [ ] T042 [US1] Create TaxManagementProfile AutoMapper profile in src/Backend/CRM.Application/TaxManagement/Mapping/TaxManagementProfile.cs
- [ ] T043 [US1] Create CountriesController in src/Backend/CRM.Api/Controllers/CountriesController.cs with [AdminOnly] authorization
- [ ] T044 [US1] Create TaxFrameworksController in src/Backend/CRM.Api/Controllers/TaxFrameworksController.cs with [AdminOnly] authorization
- [ ] T045 [US1] Register CountriesController and TaxFrameworksController routes in src/Backend/CRM.Api/Program.cs
- [X] T046 [P] [US1] Create CountryManagementTable component in src/Frontend/web/src/components/tax/CountryManagementTable.tsx
- [X] T047 [P] [US1] Create CountryForm component in src/Frontend/web/src/components/tax/CountryForm.tsx
- [X] T048 [P] [US1] Create TaxFrameworkForm component in src/Frontend/web/src/components/tax/TaxFrameworkForm.tsx
- [X] T049 [US1] Create countries page in src/Frontend/web/src/app/(protected)/admin/tax/countries/page.tsx (depends on T046, T047)
- [X] T050 [US1] Create new country page in src/Frontend/web/src/app/(protected)/admin/tax/countries/new/page.tsx (depends on T047) - Integrated into main page
- [X] T051 [US1] Create frameworks page in src/Frontend/web/src/app/(protected)/admin/tax/frameworks/page.tsx (depends on T048)
- [X] T052 [US1] Add CountriesApi and TaxFrameworksApi methods to src/Frontend/web/src/lib/api.ts

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently - admins can configure countries and tax frameworks

---

## Phase 4: User Story 2 - Admin Configures Jurisdictions (Priority: P1)

**Goal**: Admins can configure jurisdictions (states for India, emirates/cities for UAE) with their tax rates so quotations can calculate taxes based on client location.

**Independent Test**: Admin adds Indian states (e.g., Maharashtra, Karnataka) and UAE emirates (Dubai, Abu Dhabi), verifies jurisdictions appear in configuration with correct hierarchy.

### Implementation for User Story 2

- [X] T053 [P] [US2] Create Jurisdiction entity in src/Backend/CRM.Domain/Entities/Jurisdiction.cs
- [X] T054 [US2] Create JurisdictionEntityConfiguration in src/Backend/CRM.Infrastructure/EntityConfigurations/JurisdictionEntityConfiguration.cs (depends on T007, T053)
- [X] T055 [US2] Create JurisdictionDto in src/Backend/CRM.Application/TaxManagement/Dtos/JurisdictionDto.cs
- [X] T056 [P] [US2] Create CreateJurisdictionRequest in src/Backend/CRM.Application/TaxManagement/Requests/CreateJurisdictionRequest.cs
- [X] T057 [P] [US2] Create UpdateJurisdictionRequest in src/Backend/CRM.Application/TaxManagement/Requests/UpdateJurisdictionRequest.cs
- [X] T058 [P] [US2] Create CreateJurisdictionRequestValidator in src/Backend/CRM.Application/TaxManagement/Validators/CreateJurisdictionRequestValidator.cs
- [X] T059 [P] [US2] Create UpdateJurisdictionRequestValidator in src/Backend/CRM.Application/TaxManagement/Validators/UpdateJurisdictionRequestValidator.cs
- [X] T060 [P] [US2] Create CreateJurisdictionCommand in src/Backend/CRM.Application/TaxManagement/Commands/CreateJurisdictionCommand.cs
- [X] T061 [P] [US2] Create UpdateJurisdictionCommand in src/Backend/CRM.Application/TaxManagement/Commands/UpdateJurisdictionCommand.cs
- [X] T062 [P] [US2] Create DeleteJurisdictionCommand in src/Backend/CRM.Application/TaxManagement/Commands/DeleteJurisdictionCommand.cs
- [X] T063 [US2] Create CreateJurisdictionCommandHandler in src/Backend/CRM.Application/TaxManagement/Commands/Handlers/CreateJurisdictionCommandHandler.cs (depends on T060)
- [X] T064 [US2] Create UpdateJurisdictionCommandHandler in src/Backend/CRM.Application/TaxManagement/Commands/Handlers/UpdateJurisdictionCommandHandler.cs (depends on T061)
- [X] T065 [US2] Create DeleteJurisdictionCommandHandler in src/Backend/CRM.Application/TaxManagement/Commands/Handlers/DeleteJurisdictionCommandHandler.cs (depends on T062)
- [X] T066 [P] [US2] Create GetJurisdictionsByCountryQuery in src/Backend/CRM.Application/TaxManagement/Queries/GetJurisdictionsByCountryQuery.cs
- [X] T067 [P] [US2] Create GetJurisdictionByIdQuery in src/Backend/CRM.Application/TaxManagement/Queries/GetJurisdictionByIdQuery.cs
- [X] T068 [US2] Create GetJurisdictionsByCountryQueryHandler in src/Backend/CRM.Application/TaxManagement/Queries/Handlers/GetJurisdictionsByCountryQueryHandler.cs (depends on T066)
- [X] T069 [US2] Create GetJurisdictionByIdQueryHandler in src/Backend/CRM.Application/TaxManagement/Queries/Handlers/GetJurisdictionByIdQueryHandler.cs (depends on T067)
- [X] T070 [US2] Update TaxManagementProfile with Jurisdiction mappings in src/Backend/CRM.Application/TaxManagement/Mapping/TaxManagementProfile.cs
- [X] T071 [US2] Create JurisdictionsController in src/Backend/CRM.Api/Controllers/JurisdictionsController.cs with [AdminOnly] authorization
- [X] T072 [US2] Register JurisdictionsController routes in src/Backend/CRM.Api/Program.cs
- [X] T073 [P] [US2] Create JurisdictionTree component in src/Frontend/web/src/components/tax/JurisdictionTree.tsx
- [X] T074 [P] [US2] Create JurisdictionForm component in src/Frontend/web/src/components/tax/JurisdictionForm.tsx
- [X] T075 [US2] Create jurisdictions page in src/Frontend/web/src/app/(protected)/admin/tax/countries/[countryId]/jurisdictions/page.tsx (depends on T073, T074)
- [X] T076 [US2] Add JurisdictionsApi methods to src/Frontend/web/src/lib/api.ts

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently - admins can configure countries, frameworks, and jurisdictions

---

## Phase 5: User Story 3 - Admin Configures Tax Rates by Product/Service Category (Priority: P1)

**Goal**: Admins can configure different tax rates based on product or service categories so items in the same jurisdiction can have different tax rates (e.g., services at 5% VAT in Dubai, products at 5% VAT).

**Independent Test**: Admin configures tax rates for Dubai: 5% VAT for "Services" category and 5% VAT for "Products" category, verifies category tax rates are configured correctly.

### Implementation for User Story 3

- [X] T077 [P] [US3] Create ProductServiceCategory entity in src/Backend/CRM.Domain/Entities/ProductServiceCategory.cs
- [X] T078 [P] [US3] Create TaxRate entity in src/Backend/CRM.Domain/Entities/TaxRate.cs
- [X] T079 [US3] Create ProductServiceCategoryEntityConfiguration in src/Backend/CRM.Infrastructure/EntityConfigurations/ProductServiceCategoryEntityConfiguration.cs (depends on T077)
- [X] T080 [US3] Create TaxRateEntityConfiguration in src/Backend/CRM.Infrastructure/EntityConfigurations/TaxRateEntityConfiguration.cs (depends on T078, T007)
- [X] T081 [US3] Create ProductServiceCategoryDto in src/Backend/CRM.Application/TaxManagement/Dtos/ProductServiceCategoryDto.cs
- [X] T082 [US3] Create TaxRateDto in src/Backend/CRM.Application/TaxManagement/Dtos/TaxRateDto.cs
- [X] T083 [P] [US3] Create CreateProductServiceCategoryRequest in src/Backend/CRM.Application/TaxManagement/Requests/CreateProductServiceCategoryRequest.cs
- [X] T084 [P] [US3] Create UpdateProductServiceCategoryRequest in src/Backend/CRM.Application/TaxManagement/Requests/UpdateProductServiceCategoryRequest.cs
- [X] T085 [P] [US3] Create CreateTaxRateRequest in src/Backend/CRM.Application/TaxManagement/Requests/CreateTaxRateRequest.cs
- [X] T086 [P] [US3] Create UpdateTaxRateRequest in src/Backend/CRM.Application/TaxManagement/Requests/UpdateTaxRateRequest.cs
- [X] T087 [P] [US3] Create CreateProductServiceCategoryRequestValidator in src/Backend/CRM.Application/TaxManagement/Validators/CreateProductServiceCategoryRequestValidator.cs
- [X] T088 [P] [US3] Create UpdateProductServiceCategoryRequestValidator in src/Backend/CRM.Application/TaxManagement/Validators/UpdateProductServiceCategoryRequestValidator.cs
- [X] T089 [P] [US3] Create CreateTaxRateRequestValidator in src/Backend/CRM.Application/TaxManagement/Validators/CreateTaxRateRequestValidator.cs
- [X] T090 [P] [US3] Create UpdateTaxRateRequestValidator in src/Backend/CRM.Application/TaxManagement/Validators/UpdateTaxRateRequestValidator.cs
- [X] T091 [P] [US3] Create CreateProductServiceCategoryCommand in src/Backend/CRM.Application/TaxManagement/Commands/CreateProductServiceCategoryCommand.cs
- [X] T092 [P] [US3] Create UpdateProductServiceCategoryCommand in src/Backend/CRM.Application/TaxManagement/Commands/UpdateProductServiceCategoryCommand.cs
- [X] T093 [P] [US3] Create CreateTaxRateCommand in src/Backend/CRM.Application/TaxManagement/Commands/CreateTaxRateCommand.cs
- [X] T094 [P] [US3] Create UpdateTaxRateCommand in src/Backend/CRM.Application/TaxManagement/Commands/UpdateTaxRateCommand.cs
- [X] T095 [P] [US3] Create DeleteTaxRateCommand in src/Backend/CRM.Application/TaxManagement/Commands/DeleteTaxRateCommand.cs
- [X] T096 [US3] Create CreateProductServiceCategoryCommandHandler in src/Backend/CRM.Application/TaxManagement/Commands/Handlers/CreateProductServiceCategoryCommandHandler.cs (depends on T091)
- [X] T097 [US3] Create UpdateProductServiceCategoryCommandHandler in src/Backend/CRM.Application/TaxManagement/Commands/Handlers/UpdateProductServiceCategoryCommandHandler.cs (depends on T092)
- [X] T098 [US3] Create CreateTaxRateCommandHandler in src/Backend/CRM.Application/TaxManagement/Commands/Handlers/CreateTaxRateCommandHandler.cs (depends on T093)
- [X] T099 [US3] Create UpdateTaxRateCommandHandler in src/Backend/CRM.Application/TaxManagement/Commands/Handlers/UpdateTaxRateCommandHandler.cs (depends on T094)
- [X] T100 [US3] Create DeleteTaxRateCommandHandler in src/Backend/CRM.Application/TaxManagement/Commands/Handlers/DeleteTaxRateCommandHandler.cs (depends on T095)
- [X] T101 [P] [US3] Create GetAllProductServiceCategoriesQuery in src/Backend/CRM.Application/TaxManagement/Queries/GetAllProductServiceCategoriesQuery.cs
- [X] T102 [P] [US3] Create GetProductServiceCategoryByIdQuery in src/Backend/CRM.Application/TaxManagement/Queries/GetProductServiceCategoryByIdQuery.cs
- [X] T103 [P] [US3] Create GetAllTaxRatesQuery in src/Backend/CRM.Application/TaxManagement/Queries/GetAllTaxRatesQuery.cs
- [X] T104 [P] [US3] Create GetTaxRatesByJurisdictionQuery in src/Backend/CRM.Application/TaxManagement/Queries/GetTaxRatesByJurisdictionQuery.cs
- [X] T105 [US3] Create GetAllProductServiceCategoriesQueryHandler in src/Backend/CRM.Application/TaxManagement/Queries/Handlers/GetAllProductServiceCategoriesQueryHandler.cs (depends on T101)
- [X] T106 [US3] Create GetProductServiceCategoryByIdQueryHandler in src/Backend/CRM.Application/TaxManagement/Queries/Handlers/GetProductServiceCategoryByIdQueryHandler.cs (depends on T102)
- [X] T107 [US3] Create GetAllTaxRatesQueryHandler in src/Backend/CRM.Application/TaxManagement/Queries/Handlers/GetAllTaxRatesQueryHandler.cs (depends on T103)
- [X] T108 [US3] Create GetTaxRatesByJurisdictionQueryHandler in src/Backend/CRM.Application/TaxManagement/Queries/Handlers/GetTaxRatesByJurisdictionQueryHandler.cs (depends on T104)
- [X] T109 [US3] Update TaxManagementProfile with ProductServiceCategory and TaxRate mappings in src/Backend/CRM.Application/TaxManagement/Mapping/TaxManagementProfile.cs
- [X] T110 [US3] Create ProductServiceCategoriesController in src/Backend/CRM.Api/Controllers/ProductServiceCategoriesController.cs with [AdminOnly] authorization
- [X] T111 [US3] Create TaxRatesController in src/Backend/CRM.Api/Controllers/TaxRatesController.cs with [AdminOnly] authorization
- [X] T112 [US3] Register ProductServiceCategoriesController and TaxRatesController routes in src/Backend/CRM.Api/Program.cs
- [X] T113 [P] [US3] Create TaxRateTable component in src/Frontend/web/src/components/tax/TaxRateTable.tsx
- [X] T114 [P] [US3] Create TaxRateForm component in src/Frontend/web/src/components/tax/TaxRateForm.tsx
- [X] T115 [P] [US3] Create CategoryTaxRulesTable component in src/Frontend/web/src/components/tax/CategoryTaxRulesTable.tsx
- [X] T116 [US3] Create rates page in src/Frontend/web/src/app/(protected)/admin/tax/rates/page.tsx (depends on T113, T114)
- [X] T117 [US3] Create categories page in src/Frontend/web/src/app/(protected)/admin/tax/categories/page.tsx (depends on T115)
- [X] T118 [US3] Add ProductServiceCategoriesApi and TaxRatesApi methods to src/Frontend/web/src/lib/api.ts

**Checkpoint**: At this point, User Stories 1, 2, AND 3 should all work independently - admins can configure countries, frameworks, jurisdictions, categories, and tax rates

---

## Phase 6: User Story 4 - System Calculates Tax Based on Client Location and Item Categories (Priority: P1)

**Goal**: System automatically calculates taxes when creating quotations based on client's country/jurisdiction and the categories of items being quoted, so sales reps don't have to manually calculate or research tax rates.

**Independent Test**: Create quotation with client in Maharashtra, India, add line items categorized as "Services", verify system automatically calculates CGST and SGST at configured rates without manual intervention.

### Implementation for User Story 4

- [X] T119 [P] [US4] Create TaxCalculationLog entity in src/Backend/CRM.Domain/Entities/TaxCalculationLog.cs
- [X] T120 [US4] Create TaxCalculationLogEntityConfiguration in src/Backend/CRM.Infrastructure/EntityConfigurations/TaxCalculationLogEntityConfiguration.cs (depends on T119, T007)
- [X] T121 [US4] Modify Client entity to add CountryId and JurisdictionId FKs in src/Backend/CRM.Domain/Entities/Client.cs
- [X] T122 [US4] Modify QuotationLineItem entity to add ProductServiceCategoryId FK in src/Backend/CRM.Domain/Entities/QuotationLineItem.cs
- [X] T123 [US4] Modify Quotation entity to add TaxCountryId, TaxJurisdictionId, TaxFrameworkId, TaxBreakdown fields in src/Backend/CRM.Domain/Entities/Quotation.cs
- [X] T124 [US4] Update ClientEntityConfiguration to add CountryId and JurisdictionId FKs in src/Backend/CRM.Infrastructure/EntityConfigurations/ClientEntityConfiguration.cs (depends on T121)
- [X] T125 [US4] Update QuotationLineItemEntityConfiguration to add ProductServiceCategoryId FK in src/Backend/CRM.Infrastructure/EntityConfigurations/QuotationLineItemEntityConfiguration.cs (depends on T122)
- [X] T126 [US4] Update QuotationEntityConfiguration to add new tax fields in src/Backend/CRM.Infrastructure/EntityConfigurations/QuotationEntityConfiguration.cs (depends on T123)
- [X] T127 [US4] Create database migration for entity modifications in src/Backend/CRM.Infrastructure/Migrations/
- [X] T128 [US4] Create ITaxCalculationService interface in src/Backend/CRM.Application/TaxManagement/Services/ITaxCalculationService.cs
- [X] T129 [US4] Create TaxCalculationService implementation in src/Backend/CRM.Application/TaxManagement/Services/TaxCalculationService.cs
- [X] T130 [US4] Create TaxCalculationResultDto in src/Backend/CRM.Application/TaxManagement/Dtos/TaxCalculationResultDto.cs
- [X] T131 [US4] Create TaxCalculationLogDto in src/Backend/CRM.Application/TaxManagement/Dtos/TaxCalculationLogDto.cs
- [X] T132 [P] [US4] Create PreviewTaxCalculationQuery in src/Backend/CRM.Application/TaxManagement/Queries/PreviewTaxCalculationQuery.cs
- [X] T133 [US4] Create PreviewTaxCalculationQueryHandler in src/Backend/CRM.Application/TaxManagement/Queries/Handlers/PreviewTaxCalculationQueryHandler.cs (depends on T129, T132)
- [X] T134 [US4] Register ITaxCalculationService in src/Backend/CRM.Api/Program.cs
- [X] T135 [US4] Create TaxCalculationController in src/Backend/CRM.Api/Controllers/TaxCalculationController.cs
- [X] T136 [US4] Register TaxCalculationController routes in src/Backend/CRM.Api/Program.cs
- [X] T137 [US4] Modify CreateQuotationCommandHandler to use ITaxCalculationService in src/Backend/CRM.Application/Quotations/Commands/Handlers/CreateQuotationCommandHandler.cs
- [X] T138 [US4] Modify UpdateQuotationCommandHandler to recalculate taxes using ITaxCalculationService in src/Backend/CRM.Application/Quotations/Commands/Handlers/UpdateQuotationCommandHandler.cs
- [X] T139 [US4] Update TaxManagementProfile with TaxCalculationResultDto and TaxCalculationLogDto mappings in src/Backend/CRM.Application/TaxManagement/Mapping/TaxManagementProfile.cs
- [X] T140 [P] [US4] Create TaxCalculationPreview component in src/Frontend/web/src/components/tax/TaxCalculationPreview.tsx
- [X] T141 [US4] Update quotation create form to show tax breakdown in src/Frontend/web/src/app/(protected)/quotations/create/page.tsx (depends on T140)
- [X] T142 [US4] Update quotation edit form to recalculate taxes on client/item changes in src/Frontend/web/src/app/(protected)/quotations/[id]/edit/page.tsx (depends on T140)
- [X] T143 [US4] Update quotation view to display detailed tax breakdown in src/Frontend/web/src/app/(protected)/quotations/[id]/view/page.tsx
- [X] T144 [US4] Update quotation PDF generation to include tax breakdown in src/Backend/CRM.Application/Quotations/Services/QuotationPdfGenerationService.cs
- [X] T145 [US4] Add TaxCalculationApi methods to src/Frontend/web/src/lib/api.ts

**Checkpoint**: At this point, User Story 4 should be fully functional - system automatically calculates taxes based on client location and item categories when creating/updating quotations

---

## Phase 7: User Story 5 - Admin Views Tax Calculation Audit Trail (Priority: P2)

**Goal**: Admins can view a history of tax calculations and configuration changes to audit tax compliance and troubleshoot calculation issues.

**Independent Test**: Admin views tax audit log, sees entries for tax configuration changes and quotations that used tax calculations, verifies log shows dates, users, and details of each action.

### Implementation for User Story 5

- [X] T146 [P] [US5] Create GetTaxCalculationLogQuery in src/Backend/CRM.Application/TaxManagement/Queries/GetTaxCalculationLogQuery.cs
- [X] T147 [US5] Create GetTaxCalculationLogQueryHandler in src/Backend/CRM.Application/TaxManagement/Queries/Handlers/GetTaxCalculationLogQueryHandler.cs (depends on T146)
- [X] T148 [US5] Update TaxManagementProfile with TaxCalculationLogDto mappings in src/Backend/CRM.Application/TaxManagement/Mapping/TaxManagementProfile.cs
- [X] T149 [US5] Create TaxAuditLogController in src/Backend/CRM.Api/Controllers/TaxAuditLogController.cs with [AdminOnly] authorization
- [X] T150 [US5] Register TaxAuditLogController routes in src/Backend/CRM.Api/Program.cs (auto-registered via MapControllers)
- [X] T151 [US5] Update all tax configuration command handlers to log changes in TaxCalculationLog (CountriesController, JurisdictionsController, TaxFrameworksController, TaxRatesController, ProductServiceCategoriesController)
- [X] T152 [US5] Update TaxCalculationService to log calculations in TaxCalculationLog in src/Backend/CRM.Application/TaxManagement/Services/TaxCalculationService.cs (logged in quotation handlers)
- [X] T153 [P] [US5] Create TaxAuditLogTable component in src/Frontend/web/src/components/tax/TaxAuditLogTable.tsx
- [X] T154 [US5] Create audit-log page in src/Frontend/web/src/app/(protected)/admin/tax/audit-log/page.tsx (depends on T153)
- [X] T155 [US5] Add TaxAuditLogApi methods to src/Frontend/web/src/lib/api.ts

**Checkpoint**: At this point, all user stories should be independently functional - admins can view tax calculation audit trail

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T156 [P] Add caching for tax rate lookups in TaxCalculationService using IMemoryCache
- [X] T157 [P] Update database indexes per data-model.md specifications
- [X] T158 [P] Add validation for country code ISO 3166-1 alpha-2 format in validators
- [X] T159 [P] Add validation for tax rate effective dates (no overlaps) in CreateTaxRateCommandHandler
- [ ] T160 [P] Add error handling and logging for all tax management endpoints
- [ ] T161 [P] Update Swagger/OpenAPI documentation with tax management endpoints
- [ ] T162 [P] Add integration tests for TaxCalculationService in src/Backend/CRM.Tests.Integration/TaxManagement/TaxCalculationServiceTests.cs
- [ ] T163 [P] Add integration tests for CountriesController in src/Backend/CRM.Tests.Integration/TaxManagement/CountriesControllerTests.cs
- [ ] T164 [P] Add integration tests for JurisdictionsController in src/Backend/CRM.Tests.Integration/TaxManagement/JurisdictionsControllerTests.cs
- [ ] T165 [P] Add integration tests for TaxRatesController in src/Backend/CRM.Tests.Integration/TaxManagement/TaxRatesControllerTests.cs
- [ ] T166 [P] Update documentation with tax management feature in README.md
- [ ] T167 Run quickstart.md validation to ensure all examples work

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-7)**: All depend on Foundational phase completion
  - US1 (P1): Can start after Foundational - No dependencies on other stories
  - US2 (P1): Can start after Foundational - Depends on US1 (needs Country entity)
  - US3 (P1): Can start after Foundational - Depends on US1 (needs TaxFramework) and US2 (needs Jurisdiction)
  - US4 (P1): Can start after Foundational - Depends on US1, US2, US3 (needs all tax configuration entities)
  - US5 (P2): Can start after Foundational - Depends on US4 (needs TaxCalculationService to log calculations)
- **Polish (Phase 8)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P1)**: Can start after Foundational (Phase 2) - Depends on US1 (needs Country entity for Jurisdiction FK)
- **User Story 3 (P1)**: Can start after Foundational (Phase 2) - Depends on US1 (needs TaxFramework for TaxRate FK) and US2 (needs Jurisdiction for TaxRate FK)
- **User Story 4 (P1)**: Can start after Foundational (Phase 2) - Depends on US1, US2, US3 (needs Country, Jurisdiction, TaxFramework, TaxRate, ProductServiceCategory entities for tax calculation)
- **User Story 5 (P2)**: Can start after Foundational (Phase 2) - Depends on US4 (needs TaxCalculationService to log calculations)

### Within Each User Story

- Models/Entities before DTOs
- DTOs before Commands/Queries
- Commands/Queries before Handlers
- Handlers before Controllers
- Backend before Frontend
- Core implementation before integration

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational phase completes, User Stories 1, 2, 3 can start in parallel (if US2 waits for US1 Country entity)
- All tests marked [P] can run in parallel
- Models/Entities within a story marked [P] can run in parallel
- DTOs/Requests/Validators marked [P] can run in parallel
- Queries marked [P] can run in parallel
- Frontend components marked [P] can run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch all entities for User Story 1 together:
- Create Country entity in src/Backend/CRM.Domain/Entities/Country.cs
- Create TaxFramework entity in src/Backend/CRM.Domain/Entities/TaxFramework.cs

# Launch all entity configurations together:
- Create CountryEntityConfiguration in src/Backend/CRM.Infrastructure/EntityConfigurations/CountryEntityConfiguration.cs
- Create TaxFrameworkEntityConfiguration in src/Backend/CRM.Infrastructure/EntityConfigurations/TaxFrameworkEntityConfiguration.cs

# Launch all requests/validators together:
- Create CreateCountryRequest, UpdateCountryRequest, CreateTaxFrameworkRequest, UpdateTaxFrameworkRequest
- Create all validators for above requests

# Launch all commands/queries together:
- Create CreateCountryCommand, UpdateCountryCommand, DeleteCountryCommand, CreateTaxFrameworkCommand, UpdateTaxFrameworkCommand
- Create GetAllCountriesQuery, GetCountryByIdQuery, GetAllTaxFrameworksQuery, GetTaxFrameworkByIdQuery
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1 (Country and Tax Framework configuration)
4. **STOP and VALIDATE**: Test User Story 1 independently
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 → Test independently → Deploy/Demo
4. Add User Story 3 → Test independently → Deploy/Demo
5. Add User Story 4 → Test independently → Deploy/Demo (Full tax calculation)
6. Add User Story 5 → Test independently → Deploy/Demo (Audit trail)
7. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (Countries + Frameworks)
   - Developer B: (waits for US1 Country entity) → User Story 2 (Jurisdictions)
   - Developer C: (waits for US1 TaxFramework + US2 Jurisdiction) → User Story 3 (Categories + Rates)
3. Once US1, US2, US3 complete:
   - Developer A: User Story 4 (Tax Calculation Engine)
   - Developer B: User Story 5 (Audit Log)
4. Stories complete and integrate independently

---

## Summary

**Total Task Count**: 167 tasks

**Task Count Per User Story**:
- Phase 1 (Setup): 4 tasks
- Phase 2 (Foundational): 5 tasks
- Phase 3 (US1): 43 tasks
- Phase 4 (US2): 24 tasks
- Phase 5 (US3): 42 tasks
- Phase 6 (US4): 27 tasks
- Phase 7 (US5): 10 tasks
- Phase 8 (Polish): 12 tasks

**Parallel Opportunities Identified**:
- 85+ tasks can run in parallel (marked with [P])
- Models/Entities can be created in parallel within each story
- DTOs/Requests/Validators can be created in parallel
- Frontend components can be created in parallel
- User Stories 1-3 can be worked on in parallel (with dependency management)

**Independent Test Criteria**:
- US1: Admin can configure countries and tax frameworks independently
- US2: Admin can configure jurisdictions for existing countries independently
- US3: Admin can configure categories and tax rates independently (requires US1/US2 entities)
- US4: System calculates taxes automatically based on client location and item categories (requires US1/US2/US3 configuration)
- US5: Admin can view tax calculation audit trail (requires US4 calculations to log)

**Suggested MVP Scope**: User Story 1 (Phase 3) - Country and Tax Framework Configuration
- Minimal viable product allows admins to configure countries and tax frameworks
- Subsequent stories build on this foundation
- Can be tested and deployed independently

**Format Validation**: ✅ All tasks follow checklist format with checkbox, ID, optional [P] marker, optional [Story] label, and exact file paths

