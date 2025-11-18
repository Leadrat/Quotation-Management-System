# Task Breakdown: Spec-011 Quotation Template Management

**Spec**: Spec-011  
**Last Updated**: 2025-11-15

## Overview

This document provides a detailed task breakdown for implementing Spec-011: Quotation Template Management. Tasks are organized by phase and include priority markers `[P]` for parallelizable work.

---

## Phase 1: Setup & Foundational

**Purpose**: Establish database schema, entities, DTOs, and basic infrastructure.

### Database & Migrations

- [ ] T1 [P] Create migration `CreateQuotationTemplatesTables`:
  - Create `QuotationTemplates` table with all 16 columns
  - Create `QuotationTemplateLineItems` table with all 9 columns
  - Add all foreign keys and constraints
  - Add all indexes (including partial indexes for soft delete)
  - Add check constraints for Visibility enum

**Checkpoint**: Migration runs successfully, tables created.

---

### Domain Entities

- [ ] T2 [P] Create `src/Backend/CRM.Domain/Entities/QuotationTemplate.cs`:
  - All 16 properties with correct types
  - Navigation property to `LineItems`
  - Domain methods: `IsDeleted()`, `CanBeEdited()`, `IncrementVersion()`, `MarkAsApproved()`
  - Enum for `TemplateVisibility` (Public, Team, Private)

- [ ] T3 [P] Create `src/Backend/CRM.Domain/Entities/QuotationTemplateLineItem.cs`:
  - All 9 properties with correct types
  - Navigation property to `Template`
  - Domain method: `CalculateAmount()`

- [ ] T4 [P] Create `src/Backend/CRM.Domain/Enums/TemplateVisibility.cs`:
  - Public, Team, Private values

**Checkpoint**: Entities compile and pass basic validation.

---

### Entity Framework Configuration

- [ ] T5 [P] Create `src/Backend/CRM.Infrastructure/EntityConfigurations/QuotationTemplateEntityConfiguration.cs`:
  - Table name mapping
  - Primary key configuration
  - Property constraints (max lengths, required, defaults)
  - Enum to string conversion for Visibility
  - Relationships (OwnerUser, ApprovedByUser, PreviousVersion)
  - Indexes configuration

- [ ] T6 [P] Create `src/Backend/CRM.Infrastructure/EntityConfigurations/QuotationTemplateLineItemEntityConfiguration.cs`:
  - Table name mapping
  - Primary key configuration
  - Property constraints
  - Foreign key to QuotationTemplate (CASCADE delete)
  - Indexes configuration

- [ ] T7 [P] Update `src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs`:
  - Add `DbSet<QuotationTemplate> QuotationTemplates`
  - Add `DbSet<QuotationTemplateLineItem> QuotationTemplateLineItems`

- [ ] T8 [P] Update `src/Backend/CRM.Application/Common/Persistence/IAppDbContext.cs`:
  - Add `DbSet<QuotationTemplate> QuotationTemplates`
  - Add `DbSet<QuotationTemplateLineItem> QuotationTemplateLineItems`

**Checkpoint**: EF Core configuration complete, migrations generate correctly.

---

### DTOs

- [ ] T9 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Dtos/QuotationTemplateDto.cs`:
  - All template properties
  - `OwnerUserName` (mapped from User)
  - `LineItems` (list of TemplateLineItemDto)
  - `IsActive` (computed: DeletedAt == null)
  - `IsEditable` (computed: based on owner/approval status)

- [ ] T10 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Dtos/TemplateLineItemDto.cs`:
  - All line item properties

- [ ] T11 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Dtos/CreateQuotationTemplateRequest.cs`:
  - Name, Description, Visibility
  - LineItems (list of CreateTemplateLineItemRequest)
  - Notes, DiscountDefault (optional)

- [ ] T12 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Dtos/CreateTemplateLineItemRequest.cs`:
  - ItemName, Description, Quantity, UnitRate

- [ ] T13 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Dtos/UpdateQuotationTemplateRequest.cs`:
  - Same as Create, all fields optional

- [ ] T14 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Dtos/UpdateTemplateLineItemRequest.cs`:
  - LineItemId (optional, for updates), ItemName, Description, Quantity, UnitRate

- [ ] T15 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Dtos/QuotationTemplateVersionDto.cs`:
  - TemplateId, Version, Name, UpdatedAt, UpdatedByUserId, UpdatedByUserName, PreviousVersionId

- [ ] T16 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Dtos/TemplateUsageStatsDto.cs`:
  - TotalTemplates, TotalUsage, MostUsedTemplates, TemplatesByVisibility, TemplatesByRole

**Checkpoint**: All DTOs created and compile.

---

### AutoMapper Profiles

- [ ] T17 [P] Create `src/Backend/CRM.Application/Mapping/QuotationTemplateProfile.cs`:
  - Map QuotationTemplate → QuotationTemplateDto
  - Map CreateQuotationTemplateRequest → QuotationTemplate
  - Map UpdateQuotationTemplateRequest → QuotationTemplate
  - Map QuotationTemplateLineItem → TemplateLineItemDto
  - Map CreateTemplateLineItemRequest → QuotationTemplateLineItem
  - Map UpdateTemplateLineItemRequest → QuotationTemplateLineItem
  - Resolve OwnerUserName from User navigation

**Checkpoint**: AutoMapper profiles configured.

---

### Exceptions

- [ ] T18 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Exceptions/QuotationTemplateNotFoundException.cs`

- [ ] T19 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Exceptions/InvalidTemplateVisibilityException.cs`

- [ ] T20 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Exceptions/TemplateNotEditableException.cs`

- [ ] T21 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Exceptions/UnauthorizedTemplateAccessException.cs`

**Checkpoint**: Exception classes created.

---

## Phase 2: Backend Commands

**Purpose**: Implement CQRS commands for template CRUD operations.

### Create Template Command

- [ ] T22 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Commands/CreateQuotationTemplateCommand.cs`:
  - Request (CreateQuotationTemplateRequest)
  - CreatedByUserId (Guid)

- [ ] T23 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Commands/Handlers/CreateQuotationTemplateCommandHandler.cs`:
  - Validate request
  - Check name uniqueness (per owner)
  - Create template entity (Version = 1)
  - Create line items
  - Calculate amounts
  - Save to database
  - Publish QuotationTemplateCreated event
  - Return QuotationTemplateDto

- [ ] T24 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Validators/CreateQuotationTemplateRequestValidator.cs`:
  - Name: required, 3-100 chars, unique per user
  - Description: max 255 chars
  - Visibility: required, valid enum
  - LineItems: required, min 1, each validated
  - Notes: max 2000 chars
  - DiscountDefault: 0-100 if provided

**Checkpoint**: Create template command works end-to-end.

---

### Update Template Command

- [ ] T25 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Commands/UpdateQuotationTemplateCommand.cs`:
  - TemplateId (Guid)
  - Request (UpdateQuotationTemplateRequest)
  - UpdatedByUserId (Guid)

- [ ] T26 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Commands/Handlers/UpdateQuotationTemplateCommandHandler.cs`:
  - Find template (throw if not found or deleted)
  - Check authorization (owner or admin)
  - Check if editable (not deleted, not approved if not owner)
  - Create new version (increment Version, set PreviousVersionId)
  - Update properties
  - Update line items (add/update/delete)
  - Recalculate amounts
  - Save to database
  - Publish QuotationTemplateUpdated event
  - Return QuotationTemplateDto

- [ ] T27 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Validators/UpdateQuotationTemplateRequestValidator.cs`:
  - Same rules as Create, all fields optional

**Checkpoint**: Update template command works with versioning.

---

### Delete Template Command

- [ ] T28 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Commands/DeleteQuotationTemplateCommand.cs`:
  - TemplateId (Guid)
  - DeletedByUserId (Guid)

- [ ] T29 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Commands/Handlers/DeleteQuotationTemplateCommandHandler.cs`:
  - Find template (throw if not found)
  - Check authorization (owner or admin)
  - Set DeletedAt = DateTimeOffset.UtcNow (soft delete)
  - Save to database
  - Publish QuotationTemplateDeleted event

**Checkpoint**: Delete template command works (soft delete).

---

### Restore Template Command

- [ ] T30 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Commands/RestoreQuotationTemplateCommand.cs`:
  - TemplateId (Guid)
  - RestoredByUserId (Guid)

- [ ] T31 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Commands/Handlers/RestoreQuotationTemplateCommandHandler.cs`:
  - Find template (must be deleted)
  - Check authorization (owner or admin)
  - Set DeletedAt = null
  - Save to database
  - Return QuotationTemplateDto

**Checkpoint**: Restore template command works.

---

### Approve Template Command

- [ ] T32 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Commands/ApproveQuotationTemplateCommand.cs`:
  - TemplateId (Guid)
  - ApprovedByUserId (Guid) - must be Admin

- [ ] T33 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Commands/Handlers/ApproveQuotationTemplateCommandHandler.cs`:
  - Find template (throw if not found or deleted)
  - Verify user is Admin
  - Set IsApproved = true
  - Set ApprovedByUserId and ApprovedAt
  - Save to database
  - Publish QuotationTemplateApproved event
  - Return QuotationTemplateDto

- [ ] T34 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Validators/ApproveQuotationTemplateCommandValidator.cs`:
  - TemplateId required
  - User must be Admin (checked in handler)

**Checkpoint**: Approve template command works (admin only).

---

### Apply Template Command

- [ ] T35 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Commands/ApplyTemplateToQuotationCommand.cs`:
  - TemplateId (Guid)
  - ClientId (Guid) - for quotation
  - AppliedByUserId (Guid)

- [ ] T36 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Commands/Handlers/ApplyTemplateToQuotationCommandHandler.cs`:
  - Find template (throw if not found, deleted, or not visible to user)
  - Check visibility rules (Public/Team/Private)
  - Increment UsageCount
  - Set LastUsedAt = DateTimeOffset.UtcNow
  - Convert template to CreateQuotationRequest format
  - Map line items
  - Include default discount and notes
  - Save template usage update
  - Publish QuotationTemplateApplied event
  - Return CreateQuotationRequest

- [ ] T37 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Validators/ApplyTemplateToQuotationCommandValidator.cs`:
  - TemplateId required
  - ClientId required

**Checkpoint**: Apply template command works, usage tracking updates.

---

## Phase 3: Backend Queries

**Purpose**: Implement CQRS queries for template retrieval.

### Get Template By ID

- [ ] T38 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Queries/GetTemplateByIdQuery.cs`:
  - TemplateId (Guid)
  - RequestorUserId (Guid)
  - RequestorRole (string)

- [ ] T39 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Queries/Handlers/GetTemplateByIdQueryHandler.cs`:
  - Find template (include line items, owner user)
  - Check visibility and authorization
  - Throw if not found or not accessible
  - Return QuotationTemplateDto

- [ ] T40 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Validators/GetTemplateByIdQueryValidator.cs`:
  - TemplateId required

**Checkpoint**: Get by ID query works with authorization.

---

### Get All Templates

- [ ] T41 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Queries/GetAllTemplatesQuery.cs`:
  - PageNumber, PageSize
  - Search (string, optional)
  - Visibility (enum, optional)
  - IsApproved (bool, optional)
  - OwnerUserId (Guid, optional)
  - RequestorUserId (Guid)
  - RequestorRole (string)

- [ ] T42 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Queries/Handlers/GetAllTemplatesQueryHandler.cs`:
  - Build query with filters
  - Apply visibility rules (Public/Team/Private)
  - Apply search (name, description)
  - Apply pagination
  - Order by UpdatedAt DESC
  - Return PagedResult<QuotationTemplateDto>

- [ ] T43 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Validators/GetAllTemplatesQueryValidator.cs`:
  - PageNumber >= 1
  - PageSize 1-100

**Checkpoint**: List query works with filtering and pagination.

---

### Get Template Versions

- [ ] T44 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Queries/GetTemplateVersionsQuery.cs`:
  - TemplateId (Guid)
  - RequestorUserId (Guid)

- [ ] T45 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Queries/Handlers/GetTemplateVersionsQueryHandler.cs`:
  - Find current template
  - Traverse version chain via PreviousVersionId
  - Collect all versions
  - Order by Version DESC
  - Return list of QuotationTemplateVersionDto

- [ ] T46 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Validators/GetTemplateVersionsQueryValidator.cs`:
  - TemplateId required

**Checkpoint**: Version history query works.

---

### Get Public Templates

- [ ] T47 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Queries/GetPublicTemplatesQuery.cs`:
  - RequestorUserId (Guid)
  - RequestorRole (string)

- [ ] T48 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Queries/Handlers/GetPublicTemplatesQueryHandler.cs`:
  - Query templates visible to user:
    - Public AND IsApproved = true
    - Team AND OwnerRole = RequestorRole
    - Private AND OwnerUserId = RequestorUserId
  - Exclude deleted
  - Order by UsageCount DESC, Name ASC
  - Return list of QuotationTemplateDto

**Checkpoint**: Public templates query works for quotation creation.

---

### Get Template Usage Stats

- [ ] T49 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Queries/GetTemplateUsageStatsQuery.cs`:
  - StartDate (DateTime?, optional)
  - EndDate (DateTime?, optional)
  - GroupBy (string, optional: owner, role, visibility)
  - RequestorUserId (Guid) - must be Admin

- [ ] T50 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Queries/Handlers/GetTemplateUsageStatsQueryHandler.cs`:
  - Verify user is Admin
  - Aggregate usage statistics
  - Calculate totals, most used, breakdowns
  - Return TemplateUsageStatsDto

- [ ] T51 [P] Create `src/Backend/CRM.Application/QuotationTemplates/Validators/GetTemplateUsageStatsQueryValidator.cs`:
  - User must be Admin
  - Date range valid if provided

**Checkpoint**: Usage stats query works (admin only).

---

## Phase 4: API Endpoints

**Purpose**: Expose REST API endpoints.

- [ ] T52 [P] Create `src/Backend/CRM.Api/Controllers/QuotationTemplatesController.cs`:
  - POST `/api/v1/quotation-templates` - Create
  - PUT `/api/v1/quotation-templates/{id}` - Update
  - GET `/api/v1/quotation-templates` - List
  - GET `/api/v1/quotation-templates/{id}` - Get by ID
  - DELETE `/api/v1/quotation-templates/{id}` - Delete
  - POST `/api/v1/quotation-templates/{id}/restore` - Restore
  - POST `/api/v1/quotation-templates/{id}/apply` - Apply
  - GET `/api/v1/quotation-templates/{id}/versions` - Versions
  - POST `/api/v1/quotation-templates/{id}/approve` - Approve (admin)
  - GET `/api/v1/quotation-templates/usage-stats` - Stats (admin)

- [ ] T53 [P] Register handlers and validators in `Program.cs`

- [ ] T54 [P] Add Swagger documentation for all endpoints

**Checkpoint**: All 10 endpoints working, Swagger documented.

---

## Phase 5: Frontend API Integration

- [ ] T55 [P] Extend `src/Frontend/web/src/lib/api.ts`:
  - Add `TemplatesApi` object with all methods
  - Add TypeScript interfaces
  - Add error handling

**Checkpoint**: API integration complete.

---

## Phase 6: Frontend Pages - Sales Rep

- [ ] T56 [P] Create `src/Frontend/web/src/app/(protected)/templates/page.tsx` (SR-P21)
- [ ] T57 [P] Create `src/Frontend/web/src/app/(protected)/templates/create/page.tsx` (SR-P22)
- [ ] T58 [P] Create `src/Frontend/web/src/app/(protected)/templates/[id]/edit/page.tsx` (SR-P22)
- [ ] T59 [P] Create `src/Frontend/web/src/app/(protected)/templates/[id]/versions/page.tsx` (SR-P24)
- [ ] T60 [P] Integrate Apply Template in `src/Frontend/web/src/app/(protected)/quotations/new/page.tsx` (SR-P23)
- [ ] T61 [P] Create components:
  - `TemplateListTable.tsx`
  - `TemplateForm.tsx`
  - `LineItemsEditor.tsx`
  - `TemplatePreview.tsx`
  - `ApplyTemplateModal.tsx`
  - `VersionHistoryTimeline.tsx`

**Checkpoint**: All sales rep pages functional.

---

## Phase 7: Frontend Pages - Admin

- [ ] T62 [P] Create `src/Frontend/web/src/app/(protected)/admin/templates/pending/page.tsx` (A-P17)
- [ ] T63 [P] Create `src/Frontend/web/src/app/(protected)/admin/templates/stats/page.tsx` (A-P18)
- [ ] T64 [P] Create components:
  - `AdminApprovalActions.tsx`
  - `UsageStatsWidgets.tsx`
  - `TemplateUsageChart.tsx`

**Checkpoint**: All admin pages functional.

---

## Phase 8: Testing & Polish

- [ ] T65 [P] Create backend unit tests (15+ tests)
- [ ] T66 [P] Create backend integration tests (10+ tests)
- [ ] T67 [P] Create frontend component tests (20+ tests)
- [ ] T68 [P] Update documentation
- [ ] T69 [P] Add error boundaries
- [ ] T70 [P] Add loading skeletons
- [ ] T71 [P] Add toast notifications
- [ ] T72 [P] Verify mobile responsiveness
- [ ] T73 [P] Verify accessibility

**Checkpoint**: Testing complete, polish done.

---

## Dependencies & Execution Order

### Phase Dependencies
- Setup → Commands → Queries → API → Frontend Integration → Frontend Pages → Testing

### Task Dependencies
- T1 (migration) must complete before T2-T8
- T2-T8 (entities/config) must complete before T9-T21 (DTOs/mapping)
- T9-T21 must complete before T22+ (commands/queries)
- T22-T37 (commands) can run parallel with T38-T51 (queries) after foundational
- T52-T54 (API) depends on commands and queries
- T55 (frontend API) depends on API endpoints
- T56-T64 (frontend pages) depends on frontend API
- T65-T73 (testing) depends on all previous phases

### Parallel Opportunities
- Tasks marked `[P]` can run simultaneously
- Commands and Queries can be built in parallel after foundational
- Frontend pages can be built in parallel after API integration

---

## Implementation Strategy

1. Complete Phase 1 to establish schema and entities
2. Deliver MVP via Create/Get/Apply commands and queries
3. Add Update/Delete/Restore for full CRUD
4. Add Approval and Version History
5. Build API endpoints with proper authorization
6. Build frontend pages starting with templates list
7. Integrate template application into quotation creation
8. Add admin pages for approval and statistics
9. Comprehensive testing and polish

---

**Total Tasks**: 73  
**Estimated Duration**: 18 days  
**Last Updated**: 2025-11-15

