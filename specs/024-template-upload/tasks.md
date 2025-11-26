# Tasks: Document Template Upload & Conversion

**Input**: Design documents from `/specs/024-template-upload/`
**Prerequisites**: plan.md, spec.md, data-model.md, contracts/, research.md, quickstart.md

**Tests**: Tests are OPTIONAL - not explicitly requested in specification, so test tasks are excluded. Focus on implementation tasks.

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

**Purpose**: Project initialization, dependency installation, and basic structure

- [x] T001 Install DocumentFormat.OpenXml NuGet package (version 3.0.0) in `src/Backend/CRM.Application/CRM.Application.csproj`
- [x] T002 Install PdfSharpCore NuGet package (version 1.3.0) in `src/Backend/CRM.Infrastructure/CRM.Infrastructure.csproj`
- [x] T003 [P] Create directory structure `src/Backend/CRM.Application/DocumentTemplates/` with subdirectories: Commands, Queries, Dtos, Services, Validators
- [x] T004 [P] Create directory structure `src/Backend/CRM.Infrastructure/Services/DocumentProcessing/`
- [x] T005 [P] Create directory structure `src/Frontend/web/src/components/templates/`
- [x] T006 [P] Create directory structure `src/Frontend/web/src/components/quotations/`
- [x] T007 [P] Add file upload configuration to `src/Backend/CRM.Api/appsettings.json` (MaxFileSizeBytes: 52428800, AllowedExtensions, UploadPath, TemplatePath)
- [x] T008 [P] Add document processing configuration to `src/Backend/CRM.Api/appsettings.json` (MaxProcessingTimeSeconds, EnableAsyncProcessing, AsyncProcessingThresholdPages)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T009 Create database migration `src/Backend/CRM.Infrastructure/Migrations/<timestamp>_AddDocumentTemplateSupport.cs` to extend QuotationTemplates table with IsFileBased, TemplateFilePath, TemplateType, OriginalFileName, FileSizeBytes, ProcessingStatus, ProcessingErrorMessage columns
- [x] T010 Create database migration to create TemplatePlaceholders table with PlaceholderId, TemplateId, PlaceholderName, PlaceholderType, OriginalText, PositionInDocument, IsManuallyAdded, CreatedAt, UpdatedAt columns
- [x] T011 Create database migration to add indexes: IX_QuotationTemplates_IsFileBased_TemplateType, IX_QuotationTemplates_ProcessingStatus, IX_TemplatePlaceholders_TemplateId, IX_TemplatePlaceholders_TemplateId_Type, IX_TemplatePlaceholders_TemplateId_PlaceholderName (unique)
- [x] T012 Update `src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs` to add DbSet<TemplatePlaceholder> property
- [x] T013 Update `src/Backend/CRM.Application/Common/Persistence/IAppDbContext.cs` to add DbSet<TemplatePlaceholder> property
- [x] T014 [P] Create entity configuration `src/Backend/CRM.Infrastructure/EntityConfigurations/TemplatePlaceholderEntityConfiguration.cs` with relationships and constraints
- [x] T015 [P] Register document processing services in `src/Backend/CRM.Api/Program.cs`: IDocumentProcessingService, IPlaceholderIdentificationService, ITemplateConversionService, IPdfParserService, IWordDocumentService
- [x] T016 [P] Configure file upload limits in `src/Backend/CRM.Api/Program.cs` (FormOptions.MultipartBodyLengthLimit: 52428800)

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Upload and Convert Document to Template (Priority: P1) 🎯 MVP

**Goal**: Enable Admin users to upload PDF or Word documents and automatically convert them to editable Word templates with placeholders, preserving all formatting.

**Independent Test**: Upload a Word document with company details, verify system identifies and replaces company information with placeholders, confirm output Word template is editable and preserves original styles.

### Implementation for User Story 1

- [x] T017 [P] [US1] Create DTO `src/Backend/CRM.Application/DocumentTemplates/Dtos/UploadDocumentRequest.cs` with File, TemplateType, Name, Description properties
- [x] T018 [P] [US1] Create DTO `src/Backend/CRM.Application/DocumentTemplates/Dtos/DocumentTemplateDto.cs` with all template properties including file-based fields
- [x] T019 [P] [US1] Create validator `src/Backend/CRM.Application/DocumentTemplates/Validators/UploadDocumentRequestValidator.cs` for file type (.pdf, .doc, .docx), file size (max 50MB), and required fields
- [x] T020 [P] [US1] Create service interface `src/Backend/CRM.Infrastructure/Services/DocumentProcessing/IPdfParserService.cs` for PDF text extraction
- [x] T021 [P] [US1] Create service implementation `src/Backend/CRM.Infrastructure/Services/DocumentProcessing/PdfParserService.cs` using PdfSharpCore for PDF parsing
- [x] T022 [P] [US1] Create service interface `src/Backend/CRM.Infrastructure/Services/DocumentProcessing/IWordDocumentService.cs` for Word document manipulation
- [x] T023 [P] [US1] Create service implementation `src/Backend/CRM.Infrastructure/Services/DocumentProcessing/WordDocumentService.cs` using DocumentFormat.OpenXml for Word processing
- [x] T024 [US1] Create service interface `src/Backend/CRM.Application/DocumentTemplates/Services/IDocumentProcessingService.cs` for document processing orchestration
- [x] T025 [US1] Create service implementation `src/Backend/CRM.Application/DocumentTemplates/Services/DocumentProcessingService.cs` that coordinates PDF and Word parsing
- [x] T026 [US1] Create service interface `src/Backend/CRM.Application/DocumentTemplates/Services/IPlaceholderIdentificationService.cs` for identifying company details in documents
- [x] T027 [US1] Create service implementation `src/Backend/CRM.Application/DocumentTemplates/Services/PlaceholderIdentificationService.cs` with pattern matching for company name, address, bank details, tax identifiers
- [x] T028 [US1] Create service interface `src/Backend/CRM.Application/DocumentTemplates/Services/ITemplateConversionService.cs` for converting documents to templates with placeholders
- [x] T029 [US1] Create service implementation `src/Backend/CRM.Application/DocumentTemplates/Services/TemplateConversionService.cs` that replaces identified text with placeholders while preserving formatting
- [x] T030 [US1] Create command `src/Backend/CRM.Application/DocumentTemplates/Commands/UploadDocumentCommand.cs` with handler that validates file, saves to storage, creates QuotationTemplate record with IsFileBased=true, ProcessingStatus='Pending'
- [x] T031 [US1] Create command handler `src/Backend/CRM.Application/DocumentTemplates/Commands/UploadDocumentCommandHandler.cs` that processes upload, stores file, creates template record
- [x] T032 [US1] Create command `src/Backend/CRM.Application/DocumentTemplates/Commands/ConvertDocumentCommand.cs` with handler that processes document, identifies placeholders, converts to template, updates ProcessingStatus
- [x] T033 [US1] Create command handler `src/Backend/CRM.Application/DocumentTemplates/Commands/ConvertDocumentCommandHandler.cs` that orchestrates document processing, placeholder identification, and template conversion
- [x] T034 [US1] Create AutoMapper profile `src/Backend/CRM.Application/Mapping/DocumentTemplateProfile.cs` for DocumentTemplate entity to DTO mappings
- [x] T035 [US1] Create controller `src/Backend/CRM.Api/Controllers/DocumentTemplatesController.cs` with POST /document-templates/upload endpoint
- [x] T036 [US1] Create controller endpoint POST /document-templates/{templateId}/convert in `src/Backend/CRM.Api/Controllers/DocumentTemplatesController.cs`
- [ ] T037 [US1] Create frontend component `src/Frontend/web/src/components/templates/DocumentUploader.tsx` with file input, drag-and-drop, and file validation
- [ ] T038 [US1] Create frontend page `src/Frontend/web/src/app/(protected)/templates/upload/page.tsx` that uses DocumentUploader component and handles file upload

**Checkpoint**: At this point, User Story 1 should be fully functional - Admin can upload documents and system converts them to templates with placeholders

---

## Phase 4: User Story 2 - Identify Customer Company Details (Priority: P1)

**Goal**: System identifies customer/client company information in uploaded documents and distinguishes it from user's company details, replacing with customer-specific placeholders.

**Independent Test**: Upload a document containing both sender and customer company information, verify system correctly distinguishes between them, confirm customer details are replaced with customer-specific placeholders.

### Implementation for User Story 2

- [x] T039 [US2] Extend `src/Backend/CRM.Application/DocumentTemplates/Services/PlaceholderIdentificationService.cs` to identify customer company details using context analysis (Bill To, Ship To, Customer sections)
- [x] T040 [US2] Update placeholder identification logic to distinguish between company (sender) and customer (recipient) sections based on document structure and keywords
- [x] T041 [US2] Update `src/Backend/CRM.Application/DocumentTemplates/Services/TemplateConversionService.cs` to replace customer details with `{{CustomerCompanyName}}`, `{{CustomerAddress}}`, `{{CustomerCity}}`, `{{CustomerState}}`, `{{CustomerPostalCode}}`, `{{CustomerCountry}}`, `{{CustomerGSTIN}}` placeholders
- [x] T042 [US2] Update TemplatePlaceholder entity to support PlaceholderType='Customer' in addition to 'Company'
- [x] T043 [US2] Update `src/Backend/CRM.Application/DocumentTemplates/Commands/ConvertDocumentCommandHandler.cs` to create TemplatePlaceholder records with correct PlaceholderType (Company vs Customer)

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently - system can identify and distinguish between company and customer details

---

## Phase 5: User Story 3 - Template Type Selection and Categorization (Priority: P2)

**Goal**: Users can specify template type (Quotation or Proforma Invoice) and provide metadata (name, description) when uploading documents. Templates are categorized and stored accordingly.

**Independent Test**: Upload a document, select "Quotation" as template type, provide name and description, verify template is saved and appears in Quotation templates list.

### Implementation for User Story 3

- [x] T044 [US3] Update `src/Backend/CRM.Application/DocumentTemplates/Dtos/UploadDocumentRequest.cs` to include TemplateType enum (Quotation, ProformaInvoice) validation
- [x] T045 [US3] Update `src/Backend/CRM.Application/DocumentTemplates/Commands/UploadDocumentCommandHandler.cs` to save TemplateType to QuotationTemplate record
- [x] T046 [US3] Create query `src/Backend/CRM.Application/DocumentTemplates/Queries/ListTemplatesQuery.cs` with filter by TemplateType
- [x] T047 [US3] Create query handler `src/Backend/CRM.Application/DocumentTemplates/Queries/ListTemplatesQueryHandler.cs` that filters templates by IsFileBased=true and TemplateType
- [x] T048 [US3] Create controller endpoint GET /document-templates in `src/Backend/CRM.Api/Controllers/DocumentTemplatesController.cs` with templateType query parameter
- [x] T049 [US3] Update frontend component `src/Frontend/web/src/components/templates/DocumentUploader.tsx` to include template type dropdown (Quotation/Proforma Invoice)
- [x] T050 [US3] Update frontend page `src/Frontend/web/src/app/(protected)/templates/page.tsx` to filter and display file-based templates by type

**Checkpoint**: At this point, User Stories 1, 2, AND 3 should work - templates can be categorized and filtered by type

---

## Phase 6: User Story 4 - Manual Review and Correction of Placeholder Identification (Priority: P2)

**Goal**: Users can review converted templates, manually adjust placeholder positions, add additional placeholders, or correct misidentified text before finalizing.

**Independent Test**: Upload a document, review automatically generated placeholders, manually edit placeholder text, save corrected template.

### Implementation for User Story 4

- [ ] T051 [US4] Create DTO `src/Backend/CRM.Application/DocumentTemplates/Dtos/TemplatePlaceholderDto.cs` with PlaceholderId, PlaceholderName, PlaceholderType, OriginalText, PositionInDocument, IsManuallyAdded properties
- [ ] T052 [US4] Create query `src/Backend/CRM.Application/DocumentTemplates/Queries/GetTemplateQuery.cs` to retrieve template with placeholders
- [ ] T053 [US4] Create query handler `src/Backend/CRM.Application/DocumentTemplates/Queries/GetTemplateQueryHandler.cs` that returns template with all placeholders
- [ ] T054 [US4] Create command `src/Backend/CRM.Application/DocumentTemplates/Commands/UpdateTemplatePlaceholdersCommand.cs` for updating placeholders
- [ ] T055 [US4] Create command handler `src/Backend/CRM.Application/DocumentTemplates/Commands/UpdateTemplatePlaceholdersCommandHandler.cs` that updates placeholder names, adds new placeholders, removes placeholders
- [ ] T056 [US4] Create controller endpoint GET /document-templates/{templateId}/preview in `src/Backend/CRM.Api/Controllers/DocumentTemplatesController.cs`
- [ ] T057 [US4] Create controller endpoint GET /document-templates/{templateId}/placeholders in `src/Backend/CRM.Api/Controllers/DocumentTemplatesController.cs`
- [ ] T058 [US4] Create controller endpoint POST /document-templates/{templateId}/placeholders in `src/Backend/CRM.Api/Controllers/DocumentTemplatesController.cs`
- [ ] T059 [US4] Create controller endpoint DELETE /document-templates/{templateId}/placeholders in `src/Backend/CRM.Api/Controllers/DocumentTemplatesController.cs`
- [ ] T060 [US4] Create frontend component `src/Frontend/web/src/components/templates/TemplatePreview.tsx` that displays template with highlighted placeholders
- [ ] T061 [US4] Create frontend component `src/Frontend/web/src/components/templates/PlaceholderEditor.tsx` for editing placeholder names, adding/removing placeholders
- [ ] T062 [US4] Update frontend page `src/Frontend/web/src/app/(protected)/templates/upload/page.tsx` to show preview and placeholder editor after conversion

**Checkpoint**: At this point, User Stories 1-4 should work - users can review and manually correct placeholders

---

## Phase 7: User Story 5 - Apply Document Template During Quotation Creation (Priority: P1)

**Goal**: Sales Reps can select uploaded document templates during quotation creation. System automatically converts document, identifies placeholders, and populates quotation form with company details, bank details, and client information.

**Independent Test**: Sales Rep creates quotation, selects uploaded document, clicks apply, verifies quotation form is automatically populated with company details, bank details, and client information.

### Implementation for User Story 5

- [ ] T063 [US5] Create DTO `src/Backend/CRM.Application/DocumentTemplates/Dtos/ApplyTemplateResponse.cs` with populated form field mappings
- [ ] T064 [US5] Create DTO `src/Backend/CRM.Application/DocumentTemplates/Dtos/PlaceholderMappingDto.cs` with PlaceholderName, FormFieldName, DataSource, Value properties
- [ ] T065 [US5] Create service interface `src/Backend/CRM.Application/DocumentTemplates/Services/IPlaceholderMappingService.cs` for mapping placeholders to form fields
- [ ] T066 [US5] Create service implementation `src/Backend/CRM.Application/DocumentTemplates/Services/PlaceholderMappingService.cs` that maps placeholders to quotation form fields and data sources (CompanyDetails, BankDetails, Client, Quotation)
- [ ] T067 [US5] Create command `src/Backend/CRM.Application/DocumentTemplates/Commands/ApplyDocumentTemplateCommand.cs` with TemplateId and ClientId parameters
- [ ] T068 [US5] Create command handler `src/Backend/CRM.Application/DocumentTemplates/Commands/ApplyDocumentTemplateCommandHandler.cs` that processes document, identifies placeholders, retrieves company details (Spec-022), bank details (Spec-022), client details (Spec-006), and returns populated form data
- [ ] T069 [US5] Create controller endpoint POST /document-templates/{templateId}/apply in `src/Backend/CRM.Api/Controllers/DocumentTemplatesController.cs` (or integrate into QuotationsController)
- [ ] T070 [US5] Create frontend component `src/Frontend/web/src/components/templates/TemplateSelector.tsx` that displays list of available templates for selection
- [ ] T071 [US5] Create frontend component `src/Frontend/web/src/components/quotations/QuotationFormWithTemplate.tsx` that extends existing quotation form with template selection and auto-population
- [ ] T072 [US5] Update frontend page `src/Frontend/web/src/app/(protected)/quotations/create/page.tsx` to include TemplateSelector and handle template application
- [ ] T073 [US5] Integrate template application API call in quotation creation flow to populate form fields automatically

**Checkpoint**: At this point, User Story 5 should work - Sales Reps can apply templates and forms auto-populate

---

## Phase 8: User Story 6 - Generate Final Quotation Matching Document Structure (Priority: P1)

**Goal**: After Sales Rep completes quotation form, system generates final quotation document matching original uploaded document structure with all placeholders replaced by actual data. Document available for client viewing and PDF download.

**Independent Test**: Create quotation with applied template, generate final document, verify it matches original document structure with all placeholders replaced by actual data, verify client can view and download PDF.

### Implementation for User Story 6

- [ ] T074 [US6] Create service interface `src/Backend/CRM.Application/Quotations/Services/IQuotationDocumentGenerationService.cs` for generating final quotation documents
- [ ] T075 [US6] Create service implementation `src/Backend/CRM.Application/Quotations/Services/QuotationDocumentGenerationService.cs` that loads template, replaces placeholders with actual data (company details, bank details, client details, quotation data), preserves formatting
- [ ] T076 [US6] Extend placeholder replacement logic to support quotation-specific placeholders: `{{QuotationNumber}}`, `{{QuotationDate}}`, `{{LineItems}}`, `{{SubTotal}}`, `{{TaxAmount}}`, `{{TotalAmount}}`
- [ ] T077 [US6] Create command `src/Backend/CRM.Application/Quotations/Commands/GenerateQuotationWithTemplateCommand.cs` with QuotationId parameter
- [ ] T078 [US6] Create command handler `src/Backend/CRM.Application/Quotations/Commands/GenerateQuotationWithTemplateCommandHandler.cs` that generates final document using template, replaces all placeholders, saves generated document
- [ ] T079 [US6] Integrate document generation into existing quotation generation flow in `src/Backend/CRM.Application/Quotations/Commands/CreateQuotationCommandHandler.cs` or create separate endpoint
- [ ] T080 [US6] Create controller endpoint GET /quotations/{quotationId}/document in `src/Backend/CRM.Api/Controllers/QuotationsController.cs` for retrieving generated document
- [ ] T081 [US6] Create controller endpoint GET /quotations/{quotationId}/document/pdf in `src/Backend/CRM.Api/Controllers/QuotationsController.cs` for PDF download (using QuestPDF or Word to PDF conversion)
- [ ] T082 [US6] Create frontend component `src/Frontend/web/src/components/quotations/QuotationDocumentViewer.tsx` for displaying quotation document to clients
- [ ] T083 [US6] Create frontend component `src/Frontend/web/src/components/quotations/QuotationPDFDownload.tsx` for PDF download functionality
- [ ] T084 [US6] Create frontend page `src/Frontend/web/src/app/(public)/quotations/[quotationId]/page.tsx` for client-facing quotation view with document display and PDF download
- [ ] T085 [US6] Ensure document generation preserves all formatting: fonts, colors, styles, layout, tables, images, QR codes, headers, footers, multi-column layouts

**Checkpoint**: At this point, all user stories should work - complete flow from upload to final quotation generation with document matching original structure

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T086 [P] Add error handling and logging throughout document processing services
- [ ] T087 [P] Add validation for placeholder names (PascalCase pattern) in `src/Backend/CRM.Application/DocumentTemplates/Validators/`
- [ ] T088 [P] Add file size and type validation in upload endpoints
- [ ] T089 [P] Add authorization checks (Admin for upload, SalesRep for apply) in controllers
- [ ] T090 [P] Add error messages for unsupported file formats, file size limits, processing failures
- [ ] T091 [P] Add progress indicators for document processing in frontend components
- [ ] T092 [P] Add loading states for template application and document generation
- [ ] T093 [P] Update API documentation in Swagger with new endpoints
- [ ] T094 [P] Add unit tests for PlaceholderIdentificationService in `tests/CRM.Tests.Unit/DocumentTemplates/PlaceholderIdentificationServiceTests.cs`
- [ ] T095 [P] Add unit tests for TemplateConversionService in `tests/CRM.Tests.Unit/DocumentTemplates/TemplateConversionServiceTests.cs`
- [ ] T096 [P] Add integration tests for document upload and conversion flow in `tests/CRM.Tests.Integration/DocumentTemplates/DocumentTemplateIntegrationTests.cs`
- [ ] T097 [P] Run quickstart.md validation to ensure all setup steps work
- [ ] T098 [P] Performance optimization for large document processing (async processing for >10 pages)
- [ ] T099 [P] Add caching for processed templates to avoid re-processing
- [ ] T100 [P] Add monitoring and metrics for document processing times and success rates

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-8)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 stories first: US1, US2, US5, US6 → then P2: US3, US4)
- **Polish (Phase 9)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P1)**: Depends on US1 (extends PlaceholderIdentificationService) - Should follow US1
- **User Story 3 (P2)**: Can start after Foundational (Phase 2) - Minimal dependency on US1 (uses template structure)
- **User Story 4 (P2)**: Depends on US1 (needs converted templates) - Should follow US1
- **User Story 5 (P1)**: Depends on US1, US2 (needs templates with placeholders) - Should follow US1 and US2
- **User Story 6 (P1)**: Depends on US5 (needs applied templates) - Should follow US5

### Within Each User Story

- Services before commands
- Commands before controllers
- Backend before frontend
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational phase completes:
  - US1 and US3 can start in parallel (different components)
  - After US1 completes, US2 and US4 can start in parallel
  - After US1 and US2 complete, US5 can start
  - After US5 completes, US6 can start
- All tasks marked [P] within a story can run in parallel
- Different user stories can be worked on in parallel by different team members (respecting dependencies)

---

## Parallel Example: User Story 1

```bash
# Launch all DTOs and validators in parallel:
Task: "Create DTO UploadDocumentRequest.cs"
Task: "Create DTO DocumentTemplateDto.cs"
Task: "Create validator UploadDocumentRequestValidator.cs"

# Launch all service interfaces in parallel:
Task: "Create service interface IPdfParserService.cs"
Task: "Create service interface IWordDocumentService.cs"
Task: "Create service interface IDocumentProcessingService.cs"
Task: "Create service interface IPlaceholderIdentificationService.cs"
Task: "Create service interface ITemplateConversionService.cs"

# Launch service implementations in parallel (after interfaces):
Task: "Create service implementation PdfParserService.cs"
Task: "Create service implementation WordDocumentService.cs"
```

---

## Implementation Strategy

### MVP First (User Stories 1, 2, 5, 6 Only - P1 Stories)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1 (Upload and Convert)
4. Complete Phase 4: User Story 2 (Identify Customer Details)
5. Complete Phase 7: User Story 5 (Apply Template)
6. Complete Phase 8: User Story 6 (Generate Final Document)
7. **STOP and VALIDATE**: Test complete flow independently
8. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (Basic upload works!)
3. Add User Story 2 → Test independently → Deploy/Demo (Customer identification works!)
4. Add User Story 5 → Test independently → Deploy/Demo (Template application works!)
5. Add User Story 6 → Test independently → Deploy/Demo (Complete flow works! MVP!)
6. Add User Story 3 → Test independently → Deploy/Demo (Template categorization)
7. Add User Story 4 → Test independently → Deploy/Demo (Manual correction)
8. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (Upload and Convert)
   - Developer B: User Story 3 (Template Type Selection) - can start in parallel
3. After User Story 1 completes:
   - Developer A: User Story 2 (Customer Details)
   - Developer B: User Story 4 (Manual Review) - can start in parallel
   - Developer C: User Story 5 (Apply Template) - waits for US1 and US2
4. After User Story 5 completes:
   - Developer A: User Story 6 (Generate Final Document)
5. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
- File paths use absolute paths from repository root
- All tasks include specific file paths for clarity
- Tests are optional - excluded as not explicitly requested in specification

