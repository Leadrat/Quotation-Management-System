# Implementation Plan: Spec-011 Quotation Template Management

**Spec**: Spec-011  
**Last Updated**: 2025-11-15

## Overview

This plan outlines the phased implementation of Quotation Template Management, building on Spec-009 (Quotation Entity) and Spec-010 (Quotation Management).

---

## Implementation Phases

### Phase 1: Setup & Foundational (Days 1-2)

**Goal**: Establish database schema, entities, and basic infrastructure.

#### Step 1.1: Database Migration
**File**: `src/Backend/CRM.Infrastructure/Migrations/YYYYMMDDHHMMSS_CreateQuotationTemplatesTables.cs`

**Tasks**:
- Create `QuotationTemplates` table with 16 columns
- Create `QuotationTemplateLineItems` table with 9 columns
- Add foreign keys (OwnerUserId, ApprovedByUserId, PreviousVersionId, TemplateId)
- Add check constraints (Visibility enum, Quantity > 0, UnitRate > 0)
- Add all indexes (including partial indexes for soft delete)
- Add unique constraint on (Name, OwnerUserId) WHERE DeletedAt IS NULL

**Verification**:
```sql
SELECT table_name FROM information_schema.tables 
WHERE table_name IN ('QuotationTemplates', 'QuotationTemplateLineItems');
```

#### Step 1.2: Domain Entities
**Files**:
- `src/Backend/CRM.Domain/Entities/QuotationTemplate.cs`
- `src/Backend/CRM.Domain/Entities/QuotationTemplateLineItem.cs`
- `src/Backend/CRM.Domain/Enums/TemplateVisibility.cs`

**Tasks**:
- Create `QuotationTemplate` entity with all 16 properties
- Add navigation property `LineItems`
- Add domain methods: `IsDeleted()`, `CanBeEdited()`, `IncrementVersion()`, `MarkAsApproved()`
- Create `QuotationTemplateLineItem` entity with all 9 properties
- Add navigation property `Template`
- Add domain method `CalculateAmount()`
- Create `TemplateVisibility` enum (Public, Team, Private)

#### Step 1.3: Entity Framework Configuration
**Files**:
- `src/Backend/CRM.Infrastructure/EntityConfigurations/QuotationTemplateEntityConfiguration.cs`
- `src/Backend/CRM.Infrastructure/EntityConfigurations/QuotationTemplateLineItemEntityConfiguration.cs`

**Tasks**:
- Configure table names, primary keys, property constraints
- Configure enum to string conversion for Visibility
- Configure relationships and foreign keys
- Configure indexes (including partial indexes)
- Configure cascade delete for line items

#### Step 1.4: Update DbContext
**Files**:
- `src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs`
- `src/Backend/CRM.Application/Common/Persistence/IAppDbContext.cs`

**Tasks**:
- Add `DbSet<QuotationTemplate> QuotationTemplates`
- Add `DbSet<QuotationTemplateLineItem> QuotationTemplateLineItems`
- Update interface with same properties

#### Step 1.5: DTOs
**Files** (in `src/Backend/CRM.Application/QuotationTemplates/Dtos/`):
- `QuotationTemplateDto.cs`
- `TemplateLineItemDto.cs`
- `CreateQuotationTemplateRequest.cs`
- `CreateTemplateLineItemRequest.cs`
- `UpdateQuotationTemplateRequest.cs`
- `UpdateTemplateLineItemRequest.cs`
- `QuotationTemplateVersionDto.cs`
- `TemplateUsageStatsDto.cs`

**Tasks**:
- Create all 8 DTO classes with proper properties
- Add validation attributes where needed
- Include computed properties (IsActive, IsEditable)

#### Step 1.6: AutoMapper Profile
**File**: `src/Backend/CRM.Application/Mapping/QuotationTemplateProfile.cs`

**Tasks**:
- Map QuotationTemplate → QuotationTemplateDto
- Map CreateQuotationTemplateRequest → QuotationTemplate
- Map UpdateQuotationTemplateRequest → QuotationTemplate
- Map line items (both directions)
- Resolve OwnerUserName from User navigation

#### Step 1.7: Custom Exceptions
**Files** (in `src/Backend/CRM.Application/QuotationTemplates/Exceptions/`):
- `QuotationTemplateNotFoundException.cs`
- `InvalidTemplateVisibilityException.cs`
- `TemplateNotEditableException.cs`
- `UnauthorizedTemplateAccessException.cs`

**Tasks**:
- Create 4 exception classes inheriting from appropriate base exceptions
- Add meaningful error messages

**Deliverables**:
- ✅ Migration file
- ✅ 2 entity classes + 1 enum
- ✅ 2 entity configuration classes
- ✅ 8 DTO classes
- ✅ 1 AutoMapper profile
- ✅ 4 exception classes
- ✅ DbContext updated

---

### Phase 2: Backend Commands (Days 3-5)

**Goal**: Implement CQRS commands for template CRUD operations.

#### Step 2.1: Create Template Command
**Files**:
- `src/Backend/CRM.Application/QuotationTemplates/Commands/CreateQuotationTemplateCommand.cs`
- `src/Backend/CRM.Application/QuotationTemplates/Commands/Handlers/CreateQuotationTemplateCommandHandler.cs`
- `src/Backend/CRM.Application/QuotationTemplates/Validators/CreateQuotationTemplateRequestValidator.cs`

**Tasks**:
- Create command with Request and CreatedByUserId
- Handler: Validate name uniqueness (per owner), create template (Version=1), create line items, calculate amounts, save, return DTO
- Validator: Name (3-100 chars, unique), Description (max 255), Visibility (required enum), LineItems (min 1), Notes (max 2000), DiscountDefault (0-100)

**Test**: Create template succeeds, name uniqueness enforced, line items created

#### Step 2.2: Update Template Command
**Files**:
- `src/Backend/CRM.Application/QuotationTemplates/Commands/UpdateQuotationTemplateCommand.cs`
- `src/Backend/CRM.Application/QuotationTemplates/Commands/Handlers/UpdateQuotationTemplateCommandHandler.cs`
- `src/Backend/CRM.Application/QuotationTemplates/Validators/UpdateQuotationTemplateRequestValidator.cs`

**Tasks**:
- Create command with TemplateId, Request, UpdatedByUserId
- Handler: Find template, check authorization, check if editable, create new version (increment Version, set PreviousVersionId), update properties, update line items, recalculate, save, return DTO
- Validator: Same as Create, all fields optional

**Test**: Update increments version, previous version preserved, authorization enforced

#### Step 2.3: Delete Template Command
**Files**:
- `src/Backend/CRM.Application/QuotationTemplates/Commands/DeleteQuotationTemplateCommand.cs`
- `src/Backend/CRM.Application/QuotationTemplates/Commands/Handlers/DeleteQuotationTemplateCommandHandler.cs`

**Tasks**:
- Create command with TemplateId, DeletedByUserId
- Handler: Find template, check authorization, set DeletedAt = UtcNow (soft delete), save
- No validator needed (simple operation)

**Test**: Delete sets DeletedAt, template hidden from queries, authorization enforced

#### Step 2.4: Restore Template Command
**Files**:
- `src/Backend/CRM.Application/QuotationTemplates/Commands/RestoreQuotationTemplateCommand.cs`
- `src/Backend/CRM.Application/QuotationTemplates/Commands/Handlers/RestoreQuotationTemplateCommandHandler.cs`

**Tasks**:
- Create command with TemplateId, RestoredByUserId
- Handler: Find template (must be deleted), check authorization, set DeletedAt = null, save, return DTO

**Test**: Restore clears DeletedAt, template visible again

#### Step 2.5: Approve Template Command
**Files**:
- `src/Backend/CRM.Application/QuotationTemplates/Commands/ApproveQuotationTemplateCommand.cs`
- `src/Backend/CRM.Application/QuotationTemplates/Commands/Handlers/ApproveQuotationTemplateCommandHandler.cs`
- `src/Backend/CRM.Application/QuotationTemplates/Validators/ApproveQuotationTemplateCommandValidator.cs`

**Tasks**:
- Create command with TemplateId, ApprovedByUserId
- Handler: Find template, verify user is Admin, set IsApproved=true, ApprovedByUserId, ApprovedAt=UtcNow, save, return DTO
- Validator: TemplateId required, user must be Admin (checked in handler)

**Test**: Approval works (admin only), IsApproved set, timestamp recorded

#### Step 2.6: Apply Template Command
**Files**:
- `src/Backend/CRM.Application/QuotationTemplates/Commands/ApplyTemplateToQuotationCommand.cs`
- `src/Backend/CRM.Application/QuotationTemplates/Commands/Handlers/ApplyTemplateToQuotationCommandHandler.cs`
- `src/Backend/CRM.Application/QuotationTemplates/Validators/ApplyTemplateToQuotationCommandValidator.cs`

**Tasks**:
- Create command with TemplateId, ClientId, AppliedByUserId
- Handler: Find template, check visibility rules, increment UsageCount, set LastUsedAt=UtcNow, convert to CreateQuotationRequest format, map line items, include defaults, save usage update, return CreateQuotationRequest
- Validator: TemplateId and ClientId required

**Test**: Apply increments usage, returns correct format, visibility enforced

**Deliverables**:
- ✅ 6 command classes
- ✅ 6 handler classes
- ✅ 5 validator classes (Delete doesn't need one)
- ✅ Unit tests for each command

---

### Phase 3: Backend Queries (Days 6-7)

**Goal**: Implement CQRS queries for template retrieval.

#### Step 3.1: Get Template By ID Query
**Files**:
- `src/Backend/CRM.Application/QuotationTemplates/Queries/GetTemplateByIdQuery.cs`
- `src/Backend/CRM.Application/QuotationTemplates/Queries/Handlers/GetTemplateByIdQueryHandler.cs`
- `src/Backend/CRM.Application/QuotationTemplates/Validators/GetTemplateByIdQueryValidator.cs`

**Tasks**:
- Query: TemplateId, RequestorUserId, RequestorRole
- Handler: Find template (include line items, owner user), check visibility/authorization, throw if not accessible, return DTO
- Validator: TemplateId required

**Test**: Get by ID works, authorization enforced, visibility rules applied

#### Step 3.2: Get All Templates Query
**Files**:
- `src/Backend/CRM.Application/QuotationTemplates/Queries/GetAllTemplatesQuery.cs`
- `src/Backend/CRM.Application/QuotationTemplates/Queries/Handlers/GetAllTemplatesQueryHandler.cs`
- `src/Backend/CRM.Application/QuotationTemplates/Validators/GetAllTemplatesQueryValidator.cs`

**Tasks**:
- Query: PageNumber, PageSize, Search, Visibility, IsApproved, OwnerUserId, RequestorUserId, RequestorRole
- Handler: Build query with filters, apply visibility rules (Public/Team/Private), apply search (name/description), apply pagination, order by UpdatedAt DESC, return PagedResult
- Validator: PageNumber >= 1, PageSize 1-100

**Test**: List works with filters, pagination, search, visibility rules

#### Step 3.3: Get Template Versions Query
**Files**:
- `src/Backend/CRM.Application/QuotationTemplates/Queries/GetTemplateVersionsQuery.cs`
- `src/Backend/CRM.Application/QuotationTemplates/Queries/Handlers/GetTemplateVersionsQueryHandler.cs`
- `src/Backend/CRM.Application/QuotationTemplates/Validators/GetTemplateVersionsQueryValidator.cs`

**Tasks**:
- Query: TemplateId, RequestorUserId
- Handler: Find current template, traverse version chain via PreviousVersionId (recursive or loop), collect all versions, order by Version DESC, return list of VersionDto
- Validator: TemplateId required

**Test**: Version history returns all versions, ordered correctly

#### Step 3.4: Get Public Templates Query
**Files**:
- `src/Backend/CRM.Application/QuotationTemplates/Queries/GetPublicTemplatesQuery.cs`
- `src/Backend/CRM.Application/QuotationTemplates/Queries/Handlers/GetPublicTemplatesQueryHandler.cs`

**Tasks**:
- Query: RequestorUserId, RequestorRole
- Handler: Query templates visible to user (Public AND IsApproved, Team AND OwnerRole match, Private AND OwnerUserId match), exclude deleted, order by UsageCount DESC then Name ASC, return list of DTOs
- No validator needed (uses requestor info from auth)

**Test**: Public templates query returns correct templates for quotation creation

#### Step 3.5: Get Template Usage Stats Query
**Files**:
- `src/Backend/CRM.Application/QuotationTemplates/Queries/GetTemplateUsageStatsQuery.cs`
- `src/Backend/CRM.Application/QuotationTemplates/Queries/Handlers/GetTemplateUsageStatsQueryHandler.cs`
- `src/Backend/CRM.Application/QuotationTemplates/Validators/GetTemplateUsageStatsQueryValidator.cs`

**Tasks**:
- Query: StartDate, EndDate, GroupBy, RequestorUserId (must be Admin)
- Handler: Verify user is Admin, aggregate usage statistics (totals, most used, breakdowns by visibility/role), return UsageStatsDto
- Validator: User must be Admin, date range valid if provided

**Test**: Usage stats works (admin only), aggregates correctly

**Deliverables**:
- ✅ 5 query classes
- ✅ 5 handler classes
- ✅ 4 validator classes (GetPublicTemplates doesn't need one)
- ✅ Unit tests for each query

---

### Phase 4: API Endpoints (Days 8-9)

**Goal**: Expose REST API endpoints for template management.

#### Step 4.1: Create Controller
**File**: `src/Backend/CRM.Api/Controllers/QuotationTemplatesController.cs`

**Endpoints to Implement**:

1. **POST `/api/v1/quotation-templates`** - Create
   - Inject: CreateQuotationTemplateCommandHandler, CreateQuotationTemplateRequestValidator
   - Validate request, create command, call handler, return 201 with DTO
   - Authorization: `[Authorize(Roles = "SalesRep,Admin")]`

2. **PUT `/api/v1/quotation-templates/{id}`** - Update
   - Inject: UpdateQuotationTemplateCommandHandler, UpdateQuotationTemplateRequestValidator
   - Validate request, create command, call handler, return 200 with DTO
   - Authorization: `[Authorize]` (owner or admin)

3. **GET `/api/v1/quotation-templates`** - List
   - Inject: GetAllTemplatesQueryHandler, GetAllTemplatesQueryValidator
   - Build query from query params, validate, call handler, return 200 with PagedResult
   - Authorization: `[Authorize]`

4. **GET `/api/v1/quotation-templates/{id}`** - Get by ID
   - Inject: GetTemplateByIdQueryHandler, GetTemplateByIdQueryValidator
   - Build query, validate, call handler, return 200 with DTO
   - Authorization: `[Authorize]`

5. **DELETE `/api/v1/quotation-templates/{id}`** - Delete
   - Inject: DeleteQuotationTemplateCommandHandler
   - Create command, call handler, return 200
   - Authorization: `[Authorize]` (owner or admin)

6. **POST `/api/v1/quotation-templates/{id}/restore`** - Restore
   - Inject: RestoreQuotationTemplateCommandHandler
   - Create command, call handler, return 200 with DTO
   - Authorization: `[Authorize]` (owner or admin)

7. **POST `/api/v1/quotation-templates/{id}/apply`** - Apply
   - Inject: ApplyTemplateToQuotationCommandHandler, ApplyTemplateToQuotationCommandValidator
   - Query param: clientId (required)
   - Validate, create command, call handler, return 200 with CreateQuotationRequest
   - Authorization: `[Authorize(Roles = "SalesRep,Admin")]`

8. **GET `/api/v1/quotation-templates/{id}/versions`** - Versions
   - Inject: GetTemplateVersionsQueryHandler, GetTemplateVersionsQueryValidator
   - Build query, validate, call handler, return 200 with list
   - Authorization: `[Authorize]`

9. **POST `/api/v1/quotation-templates/{id}/approve`** - Approve (admin)
   - Inject: ApproveQuotationTemplateCommandHandler, ApproveQuotationTemplateCommandValidator
   - Create command, call handler, return 200 with DTO
   - Authorization: `[Authorize(Roles = "Admin")]`

10. **GET `/api/v1/quotation-templates/usage-stats`** - Stats (admin)
    - Inject: GetTemplateUsageStatsQueryHandler, GetTemplateUsageStatsQueryValidator
    - Build query from query params, validate, call handler, return 200 with stats
    - Authorization: `[Authorize(Roles = "Admin")]`

#### Step 4.2: Register Services
**File**: `src/Backend/CRM.Api/Program.cs`

**Tasks**:
- Register all command handlers
- Register all query handlers
- Register all validators
- Add AutoMapper profile to configuration

#### Step 4.3: Swagger Documentation
**Tasks**:
- Add XML comments to controller methods
- Configure Swagger to include template endpoints
- Add example requests/responses

**Deliverables**:
- ✅ QuotationTemplatesController with 10 endpoints
- ✅ All handlers/validators registered in Program.cs
- ✅ Integration tests for all endpoints
- ✅ Swagger documentation complete

---

### Phase 5: Frontend API Integration (Day 10)

**Goal**: Create frontend API service layer.

#### Step 5.1: Extend API Service
**File**: `src/Frontend/web/src/lib/api.ts`

**Tasks**:
- Add `TemplatesApi` object with 10 methods:
  - `list(params)` - GET with query params
  - `get(templateId)` - GET by ID
  - `create(payload)` - POST
  - `update(templateId, payload)` - PUT
  - `delete(templateId)` - DELETE
  - `restore(templateId)` - POST restore
  - `apply(templateId, clientId)` - POST apply
  - `getVersions(templateId)` - GET versions
  - `approve(templateId)` - POST approve
  - `getUsageStats()` - GET stats
- Add error handling for all methods
- Use existing `apiFetch` utility

#### Step 5.2: TypeScript Types
**File**: `src/Frontend/web/src/types/templates.ts` (new file)

**Tasks**:
- Define interfaces:
  - `QuotationTemplate`
  - `TemplateLineItem`
  - `CreateQuotationTemplateRequest`
  - `UpdateQuotationTemplateRequest`
  - `TemplateVersion`
  - `TemplateUsageStats`
- Match backend DTOs exactly

**Deliverables**:
- ✅ Updated `api.ts` with TemplatesApi
- ✅ TypeScript type definitions file
- ✅ All methods tested manually

---

### Phase 6: Frontend Pages - Sales Rep (Days 11-14)

**Goal**: Build sales rep UI for template management.

#### Step 6.1: Templates List Page (SR-P21)
**File**: `src/Frontend/web/src/app/(protected)/templates/page.tsx`

**Tasks**:
- DataTable component with columns: Name, Owner, Visibility, Last Updated, Usage Count, Actions
- Search box (name/description)
- Filter dropdown (My, Team, Public, All)
- Pagination controls
- Action buttons: View, Apply, Edit, Delete, Clone
- Badge for approved templates
- Loading skeleton
- Error boundary
- Toast notifications

#### Step 6.2: Create/Edit Template Pages (SR-P22)
**Files**:
- `src/Frontend/web/src/app/(protected)/templates/create/page.tsx`
- `src/Frontend/web/src/app/(protected)/templates/[id]/edit/page.tsx`

**Tasks**:
- Form with fields: Name, Description, Visibility dropdown
- Line items editor (add/remove/edit rows)
- Notes/terms textarea
- Default discount input
- Preview button (opens TemplatePreview modal)
- Save button, Save-as-New button, Cancel button
- Form validation (client-side)
- Auto-calculation of line item amounts
- Loading states

#### Step 6.3: Apply Template Flow (SR-P23)
**File**: `src/Frontend/web/src/app/(protected)/quotations/new/page.tsx` (modify existing)

**Tasks**:
- Add "Apply Template" button at top of form
- Create `ApplyTemplateModal` component
- Modal shows searchable/filterable template list
- Preview button in modal
- On select + confirm: Pre-fill quotation form with template data
- User can modify before submitting

#### Step 6.4: Version History Page (SR-P24)
**File**: `src/Frontend/web/src/app/(protected)/templates/[id]/versions/page.tsx`

**Tasks**:
- Timeline or table showing all versions
- Columns: Version, Name, Updated By, Updated At, Actions
- Restore button for each version
- Confirmation dialog before restore
- Link back to template detail

#### Step 6.5: Template Preview Modal (SR-P25)
**Component**: `src/Frontend/web/src/components/templates/TemplatePreview.tsx`

**Tasks**:
- Modal component
- Format template as quotation preview
- Show line items, totals, tax breakdown
- Match quotation PDF format
- Close button

#### Step 6.6: Reusable Components
**Files** (in `src/Frontend/web/src/components/templates/`):
- `TemplateListTable.tsx` - Data table with actions
- `TemplateForm.tsx` - Create/edit form
- `LineItemsEditor.tsx` - Add/remove/edit line items
- `ApplyTemplateModal.tsx` - Template selection modal
- `VersionHistoryTimeline.tsx` - Version history display
- `TemplateStatusBadge.tsx` - Visibility/approval badges

**Deliverables**:
- ✅ 5 pages (list, create, edit, versions, + modify quotation create)
- ✅ 6+ reusable components
- ✅ Responsive design (mobile-friendly)
- ✅ Error handling and validation
- ✅ Toast notifications

---

### Phase 7: Frontend Pages - Admin (Days 15-16)

**Goal**: Build admin UI for template approval and statistics.

#### Step 7.1: Template Approval Queue (A-P17)
**File**: `src/Frontend/web/src/app/(protected)/admin/templates/pending/page.tsx`

**Tasks**:
- List of unapproved templates (filter: IsApproved = false)
- Columns: Name, Creator, Created Date, Usage Count, Actions
- Preview button (opens TemplatePreview)
- Version history link
- Approve button (calls approve API)
- Reject button (optional, can just delete)
- Confirmation dialogs
- Toast notifications

#### Step 7.2: Template Usage Statistics (A-P18)
**File**: `src/Frontend/web/src/app/(protected)/admin/templates/stats/page.tsx`

**Tasks**:
- Cards with key metrics:
  - Total Templates
  - Total Usage
  - Most Used Template
  - Templates by Visibility (pie chart)
  - Templates by Role (bar chart)
- Table with detailed statistics:
  - Template name, owner, visibility, usage count, last used
- Export to CSV/Excel button
- Date range filter (optional)
- Refresh button

#### Step 7.3: Admin Components
**Files** (in `src/Frontend/web/src/components/templates/admin/`):
- `AdminApprovalActions.tsx` - Approve/Reject buttons with confirmation
- `UsageStatsWidgets.tsx` - Metric cards
- `TemplateUsageChart.tsx` - Charts (pie, bar) using Chart.js or similar

**Deliverables**:
- ✅ 2 admin pages (approval queue, usage stats)
- ✅ 3+ admin components
- ✅ Charts and analytics
- ✅ Export functionality

---

### Phase 8: Testing & Polish (Days 17-18)

**Goal**: Comprehensive testing and final polish.

#### Step 8.1: Backend Unit Tests
**Files** (in `tests/CRM.Tests/QuotationTemplates/`):
- `CreateQuotationTemplateCommandHandlerTests.cs`
- `UpdateQuotationTemplateCommandHandlerTests.cs`
- `DeleteQuotationTemplateCommandHandlerTests.cs`
- `RestoreQuotationTemplateCommandHandlerTests.cs`
- `ApproveQuotationTemplateCommandHandlerTests.cs`
- `ApplyTemplateToQuotationCommandHandlerTests.cs`
- `GetTemplateByIdQueryHandlerTests.cs`
- `GetAllTemplatesQueryHandlerTests.cs`
- `GetTemplateVersionsQueryHandlerTests.cs`
- `GetTemplateUsageStatsQueryHandlerTests.cs`

**Test Coverage**:
- All CRUD operations
- Versioning (increment, history preservation)
- Authorization (owner, admin, visibility rules)
- Usage tracking (increment, timestamp)
- Soft delete and restore
- Approval workflow

#### Step 8.2: Backend Integration Tests
**Files** (in `tests/CRM.Tests.Integration/QuotationTemplates/`):
- `QuotationTemplatesControllerTests.cs` - All 10 endpoints
- Test authorization, validation, error handling
- Test versioning end-to-end
- Test apply template flow

#### Step 8.3: Frontend Component Tests
**Files** (in `src/Frontend/web/src/components/templates/__tests__/`):
- `TemplateListTable.test.tsx`
- `TemplateForm.test.tsx`
- `LineItemsEditor.test.tsx`
- `ApplyTemplateModal.test.tsx`
- `VersionHistoryTimeline.test.tsx`

**Test Coverage**:
- Component rendering
- Form validation
- User interactions
- API integration

#### Step 8.4: Frontend E2E Tests
**Files** (in `tests/e2e/`):
- `template-management-flow.spec.ts`
  - Create template → Apply to quotation → Quotation created
  - Update template → Version history → Restore version
  - Admin approval → Template becomes public

#### Step 8.5: Polish
**Tasks**:
- Add error boundaries to all pages
- Add loading skeletons for async operations
- Add toast notifications for all user actions
- Verify mobile responsiveness (test on mobile devices)
- Verify accessibility (keyboard navigation, screen readers)
- Add helpful error messages
- Update documentation (quickstart, README)
- Create validation checklist

**Deliverables**:
- ✅ 30+ backend unit tests
- ✅ 10+ backend integration tests
- ✅ 20+ frontend component tests
- ✅ 3+ E2E tests
- ✅ Error boundaries and loading states
- ✅ Toast notifications
- ✅ Mobile responsive verified
- ✅ Accessibility verified
- ✅ Documentation updated

---

## Dependencies

### External Dependencies
- Spec-009: Quotation Entity (must be complete)
- Spec-010: Quotation Management (must be complete)
- Spec-006: Client Entity (must be complete)

### Internal Dependencies
- Phase 1 → Phase 2 (entities before commands)
- Phase 2 → Phase 3 (commands before queries)
- Phase 3 → Phase 4 (queries before API)
- Phase 4 → Phase 5 (API before frontend integration)
- Phase 5 → Phase 6 (API integration before pages)
- Phase 6 → Phase 7 (sales rep pages before admin)
- All phases → Phase 8 (testing)

---

## Risk Mitigation

### Risk 1: Version History Complexity
**Mitigation**: Use simple linked list structure (PreviousVersionId). Test thoroughly with multiple versions.

### Risk 2: Visibility Rules Complexity
**Mitigation**: Create clear authorization helper methods. Test all visibility combinations.

### Risk 3: Template Application Performance
**Mitigation**: Optimize queries with indexes. Cache frequently used templates if needed.

### Risk 4: Frontend Form Complexity
**Mitigation**: Use existing form components. Break into smaller sub-components.

---

## Success Criteria

### Backend
- ✅ All 10 API endpoints working
- ✅ Versioning preserves history
- ✅ Authorization rules enforced
- ✅ Usage tracking accurate
- ✅ 90%+ test coverage

### Frontend
- ✅ All 7 pages functional
- ✅ Templates usable in quotation creation
- ✅ Mobile responsive
- ✅ Accessible (WCAG 2.1 AA)
- ✅ All user flows tested

---

## Timeline Summary

| Phase | Duration | Days |
|-------|----------|------|
| Phase 1: Setup & Foundational | 2 days | 1-2 |
| Phase 2: Backend Commands | 3 days | 3-5 |
| Phase 3: Backend Queries | 2 days | 6-7 |
| Phase 4: API Endpoints | 2 days | 8-9 |
| Phase 5: Frontend API Integration | 1 day | 10 |
| Phase 6: Frontend Pages - Sales Rep | 4 days | 11-14 |
| Phase 7: Frontend Pages - Admin | 2 days | 15-16 |
| Phase 8: Testing & Polish | 2 days | 17-18 |
| **Total** | **18 days** | **1-18** |

---

## Next Steps After Completion

1. **Spec-012**: Approval Workflow (enhance template approval)
2. **Spec-013**: Notification System (notify on template approval)
3. **Spec-014**: Template Categories/Tags (organize templates)
4. **Spec-015**: Template Variables (dynamic placeholders)

---

## Execution Summary

### Quick Start Checklist

**Before Starting**:
- [ ] Verify Spec-009 (Quotation Entity) is complete
- [ ] Verify Spec-010 (Quotation Management) is complete
- [ ] Verify Spec-006 (Client Entity) is complete
- [ ] Database is accessible and migrations can run
- [ ] Frontend development environment is set up

**Phase 1 (Days 1-2) - Foundation**:
1. Create migration → Run migration → Verify tables
2. Create entities → Create enum → Verify compilation
3. Create EF configurations → Verify relationships
4. Update DbContext → Verify DbSets
5. Create all DTOs → Verify types
6. Create AutoMapper profile → Verify mappings
7. Create exceptions → Verify inheritance

**Phase 2 (Days 3-5) - Commands**:
1. Create command (T22) → Handler (T23) → Validator (T24) → Test
2. Update command (T25) → Handler (T26) → Validator (T27) → Test
3. Delete command (T28) → Handler (T29) → Test
4. Restore command (T30) → Handler (T31) → Test
5. Approve command (T32) → Handler (T33) → Validator (T34) → Test
6. Apply command (T35) → Handler (T36) → Validator (T37) → Test

**Phase 3 (Days 6-7) - Queries**:
1. GetById query → Handler → Validator → Test
2. GetAll query → Handler → Validator → Test
3. GetVersions query → Handler → Validator → Test
4. GetPublic query → Handler → Test
5. GetUsageStats query → Handler → Validator → Test

**Phase 4 (Days 8-9) - API**:
1. Create controller with all 10 endpoints
2. Register handlers/validators in Program.cs
3. Add Swagger documentation
4. Create integration tests

**Phase 5 (Day 10) - Frontend API**:
1. Extend api.ts with TemplatesApi
2. Create TypeScript types file
3. Test all API methods manually

**Phase 6 (Days 11-14) - Frontend Sales Rep**:
1. Templates list page
2. Create/edit template pages
3. Apply template flow (modify quotation create)
4. Version history page
5. Preview modal
6. Reusable components

**Phase 7 (Days 15-16) - Frontend Admin**:
1. Approval queue page
2. Usage statistics page
3. Admin components

**Phase 8 (Days 17-18) - Testing & Polish**:
1. Backend unit tests
2. Backend integration tests
3. Frontend component tests
4. E2E tests
5. Polish (error boundaries, loading, toasts, mobile, accessibility)

### Critical Path

**Must Complete Sequentially**:
1. Phase 1 (Foundation) → Phase 2 (Commands) → Phase 3 (Queries) → Phase 4 (API)
2. Phase 4 (API) → Phase 5 (Frontend API) → Phase 6 (Frontend Pages)

**Can Run in Parallel**:
- Phase 2 and Phase 3 (after Phase 1)
- Phase 6 and Phase 7 (after Phase 5)
- Individual commands/queries within phases

### Key Milestones

- **Milestone 1** (End of Day 2): Database schema and entities complete
- **Milestone 2** (End of Day 5): All commands working, versioning tested
- **Milestone 3** (End of Day 7): All queries working, visibility rules tested
- **Milestone 4** (End of Day 9): All API endpoints working, Swagger documented
- **Milestone 5** (End of Day 10): Frontend API integration complete
- **Milestone 6** (End of Day 14): Sales rep pages complete, apply flow working
- **Milestone 7** (End of Day 16): Admin pages complete
- **Milestone 8** (End of Day 18): Testing complete, ready for production

### Risk Areas

1. **Version History**: Test thoroughly with multiple versions, ensure PreviousVersionId links correctly
2. **Visibility Rules**: Test all combinations (Public/Team/Private × Owner/Admin/Other)
3. **Template Application**: Ensure CreateQuotationRequest format matches exactly
4. **Frontend Form Complexity**: Break into smaller components, test incrementally

### Success Metrics

- ✅ All 10 API endpoints return correct responses
- ✅ Version history preserved on every update
- ✅ Usage tracking increments correctly
- ✅ Templates can be applied to quotation creation
- ✅ Admin approval workflow functional
- ✅ All pages mobile responsive
- ✅ 90%+ test coverage

---

**Last Updated**: 2025-11-15

