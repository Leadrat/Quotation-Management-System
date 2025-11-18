# Implementation Plan: Spec-012 Discount Approval Workflow

**Spec**: Spec-012  
**Last Updated**: 2025-01-XX

## Overview

This plan outlines the phased implementation of the Discount Approval Workflow, building on Spec-009 (Quotation Entity), Spec-010 (Quotation Management), Spec-011 (Quotation Template Management), and Spec-004 (RBAC).

---

## Implementation Phases

### Phase 1: Setup & Foundational (Days 1-2)

**Goal**: Establish database schema, entities, and basic infrastructure.

#### Step 1.1: Database Migration
**File**: `src/Backend/CRM.Infrastructure/Migrations/YYYYMMDDHHMMSS_CreateDiscountApprovalsTable.cs`

**Tasks**:
- Create `DiscountApprovals` table with 16 columns
- Add foreign keys (QuotationId, RequestedByUserId, ApproverUserId)
- Add check constraints (Status enum, DiscountPercentage >= 0, Threshold >= 0)
- Add all indexes (ApproverUserId+Status, QuotationId, RequestedByUserId, CurrentDiscountPercentage, Status, CreatedAt+Status)
- Add default values (EscalatedToAdmin = false, CreatedAt/UpdatedAt = CURRENT_TIMESTAMP)

**Verification**:
```sql
SELECT table_name FROM information_schema.tables 
WHERE table_name = 'DiscountApprovals';
```

#### Step 1.2: Update Quotation Entity for Locking
**File**: `src/Backend/CRM.Domain/Entities/Quotation.cs`

**Tasks**:
- Add `IsPendingApproval` property (bool, computed or stored)
- Add `PendingApprovalId` property (Guid?, nullable FK to DiscountApprovals)
- Add domain method `IsLockedForEditing()` that returns true if IsPendingApproval
- Update existing edit/delete logic to check lock status

#### Step 1.3: Domain Entities
**Files**:
- `src/Backend/CRM.Domain/Entities/DiscountApproval.cs`
- `src/Backend/CRM.Domain/Enums/ApprovalStatus.cs`
- `src/Backend/CRM.Domain/Enums/ApprovalLevel.cs`

**Tasks**:
- Create `DiscountApproval` entity with all 16 properties
- Add navigation properties (Quotation, RequestedByUser, ApproverUser)
- Add domain methods: `Approve()`, `Reject()`, `Escalate()`, `CanBeApprovedBy()`
- Create `ApprovalStatus` enum (Pending, Approved, Rejected)
- Create `ApprovalLevel` enum (Manager, Admin)

#### Step 1.4: Entity Framework Configuration
**Files**:
- `src/Backend/CRM.Infrastructure/EntityConfigurations/DiscountApprovalEntityConfiguration.cs`
- Update `QuotationEntityConfiguration.cs` for new properties

**Tasks**:
- Configure table name, primary key, property constraints
- Configure enum to string conversion for Status and ApprovalLevel
- Configure relationships and foreign keys
- Configure indexes (including composite indexes)
- Configure cascade delete behavior

#### Step 1.5: Update DbContext
**Files**:
- `src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs`
- `src/Backend/CRM.Application/Common/Persistence/IAppDbContext.cs`

**Tasks**:
- Add `DbSet<DiscountApproval> DiscountApprovals`
- Update interface with same property

#### Step 1.6: DTOs
**Files** (in `src/Backend/CRM.Application/DiscountApprovals/Dtos/`):
- `DiscountApprovalDto.cs`
- `CreateDiscountApprovalRequest.cs`
- `ApproveDiscountApprovalRequest.cs`
- `RejectDiscountApprovalRequest.cs`
- `ResubmitDiscountApprovalRequest.cs`
- `BulkApproveRequest.cs`
- `ApprovalTimelineDto.cs`
- `ApprovalMetricsDto.cs`

**Tasks**:
- Create all 8 DTO classes with proper properties
- Add validation attributes where needed
- Include computed properties and navigation data (QuotationNumber, ClientName, UserNames)

#### Step 1.7: AutoMapper Profile
**File**: `src/Backend/CRM.Application/Mapping/DiscountApprovalProfile.cs`

**Tasks**:
- Map DiscountApproval → DiscountApprovalDto
- Map CreateDiscountApprovalRequest → DiscountApproval
- Resolve UserNames from User navigation properties
- Map Quotation details for approval context

#### Step 1.8: Custom Exceptions
**Files** (in `src/Backend/CRM.Application/DiscountApprovals/Exceptions/`):
- `DiscountApprovalNotFoundException.cs`
- `QuotationLockedException.cs`
- `UnauthorizedApprovalActionException.cs`
- `InvalidApprovalStatusException.cs`

**Tasks**:
- Create all 4 custom exception classes
- Add appropriate error messages and constructors

#### Step 1.9: Domain Events
**Files** (in `src/Backend/CRM.Domain/Events/`):
- `DiscountApprovalRequested.cs`
- `DiscountApprovalApproved.cs`
- `DiscountApprovalRejected.cs`
- `DiscountApprovalEscalated.cs`
- `DiscountApprovalResubmitted.cs`

**Tasks**:
- Create all 5 domain event classes
- Include all relevant properties (ApprovalId, QuotationId, UserId, Reason, etc.)

---

### Phase 2: Backend Commands (Days 3-4)

**Goal**: Implement all command handlers for approval workflow actions.

#### Step 2.1: Request Discount Approval Command
**Files**:
- `src/Backend/CRM.Application/DiscountApprovals/Commands/RequestDiscountApprovalCommand.cs`
- `src/Backend/CRM.Application/DiscountApprovals/Commands/Handlers/RequestDiscountApprovalCommandHandler.cs`
- `src/Backend/CRM.Application/DiscountApprovals/Validators/RequestDiscountApprovalCommandValidator.cs`

**Tasks**:
- Create command with QuotationId, DiscountPercentage, Reason, Comments
- Handler logic:
  - Validate quotation exists and is not already pending
  - Determine approval level based on discount threshold (10-20% = Manager, >20% = Admin)
  - Find appropriate approver (manager or admin)
  - Create DiscountApproval record
  - Lock quotation (set IsPendingApproval = true, PendingApprovalId)
  - Publish DiscountApprovalRequested event
  - Send notification to approver
- Validator: Ensure reason is provided, discount >= threshold, quotation exists

#### Step 2.2: Approve Discount Command
**Files**:
- `src/Backend/CRM.Application/DiscountApprovals/Commands/ApproveDiscountApprovalCommand.cs`
- `src/Backend/CRM.Application/DiscountApprovals/Commands/Handlers/ApproveDiscountApprovalCommandHandler.cs`
- `src/Backend/CRM.Application/DiscountApprovals/Validators/ApproveDiscountApprovalCommandValidator.cs`

**Tasks**:
- Create command with ApprovalId, Reason, Comments, ApprovedByUserId
- Handler logic:
  - Validate approval exists and is pending
  - Verify user has permission to approve (is assigned approver or admin)
  - Update approval status to Approved
  - Update quotation discount percentage
  - Unlock quotation (set IsPendingApproval = false, clear PendingApprovalId)
  - Publish DiscountApprovalApproved event
  - Send notifications (sales rep, optionally client)
- Validator: Ensure reason is provided, approval is pending, user is authorized

#### Step 2.3: Reject Discount Command
**Files**:
- `src/Backend/CRM.Application/DiscountApprovals/Commands/RejectDiscountApprovalCommand.cs`
- `src/Backend/CRM.Application/DiscountApprovals/Commands/Handlers/RejectDiscountApprovalCommandHandler.cs`
- `src/Backend/CRM.Application/DiscountApprovals/Validators/RejectDiscountApprovalCommandValidator.cs`

**Tasks**:
- Create command with ApprovalId, Reason, Comments, RejectedByUserId
- Handler logic:
  - Validate approval exists and is pending
  - Verify user has permission to reject
  - Update approval status to Rejected
  - Revert quotation discount to previous value (or 0)
  - Unlock quotation
  - Publish DiscountApprovalRejected event
  - Send notification to sales rep
- Validator: Ensure reason is provided, approval is pending, user is authorized

#### Step 2.4: Escalate to Admin Command
**Files**:
- `src/Backend/CRM.Application/DiscountApprovals/Commands/EscalateDiscountApprovalCommand.cs`
- `src/Backend/CRM.Application/DiscountApprovals/Commands/Handlers/EscalateDiscountApprovalCommandHandler.cs`
- `src/Backend/CRM.Application/DiscountApprovals/Validators/EscalateDiscountApprovalCommandValidator.cs`

**Tasks**:
- Create command with ApprovalId, EscalatedByUserId, Reason
- Handler logic:
  - Validate approval exists and is pending
  - Verify escalation is allowed (manager can escalate, or auto-escalation)
  - Update EscalatedToAdmin = true, change ApprovalLevel to Admin
  - Reassign ApproverUserId to admin
  - Publish DiscountApprovalEscalated event
  - Send notification to admin
- Validator: Ensure approval is pending, escalation is allowed

#### Step 2.5: Resubmit Approval Command
**Files**:
- `src/Backend/CRM.Application/DiscountApprovals/Commands/ResubmitDiscountApprovalCommand.cs`
- `src/Backend/CRM.Application/DiscountApprovals/Commands/Handlers/ResubmitDiscountApprovalCommandHandler.cs`
- `src/Backend/CRM.Application/DiscountApprovals/Validators/ResubmitDiscountApprovalCommandValidator.cs`

**Tasks**:
- Create command with ApprovalId, Reason, Comments, ResubmittedByUserId
- Handler logic:
  - Validate approval exists and is rejected
  - Verify user is original requester
  - Create new approval record (linked to previous via history)
  - Lock quotation again
  - Publish DiscountApprovalResubmitted event
  - Send notification to approver
- Validator: Ensure reason is provided, approval is rejected, user is requester

#### Step 2.6: Bulk Approve Command
**Files**:
- `src/Backend/CRM.Application/DiscountApprovals/Commands/BulkApproveDiscountApprovalsCommand.cs`
- `src/Backend/CRM.Application/DiscountApprovals/Commands/Handlers/BulkApproveDiscountApprovalsCommandHandler.cs`
- `src/Backend/CRM.Application/DiscountApprovals/Validators/BulkApproveDiscountApprovalsCommandValidator.cs`

**Tasks**:
- Create command with ApprovalIds array, Reason, Comments, ApprovedByUserId
- Handler logic:
  - Validate all approvals exist and are pending
  - Verify user has permission to approve all
  - Process each approval (similar to single approve)
  - Publish events for each approval
  - Send batch notifications
- Validator: Ensure reason is provided, all approvals are pending, user is authorized

---

### Phase 3: Backend Queries (Days 5-6)

**Goal**: Implement all query handlers for retrieving approval data.

#### Step 3.1: Get Pending Approvals Query
**Files**:
- `src/Backend/CRM.Application/DiscountApprovals/Queries/GetPendingApprovalsQuery.cs`
- `src/Backend/CRM.Application/DiscountApprovals/Queries/Handlers/GetPendingApprovalsQueryHandler.cs`
- `src/Backend/CRM.Application/DiscountApprovals/Validators/GetPendingApprovalsQueryValidator.cs`

**Tasks**:
- Create query with filters: ApproverUserId, Status, DiscountPercentageMin, DiscountPercentageMax, DateFrom, DateTo, RequestedByUserId, PageNumber, PageSize
- Handler logic:
  - Filter by approver (if manager, show manager-level; if admin, show all)
  - Apply all filters
  - Include navigation properties (Quotation, Users)
  - Paginate results
  - Return paginated DTOs
- Validator: Validate filter parameters

#### Step 3.2: Get Approval By Id Query
**Files**:
- `src/Backend/CRM.Application/DiscountApprovals/Queries/GetApprovalByIdQuery.cs`
- `src/Backend/CRM.Application/DiscountApprovals/Queries/Handlers/GetApprovalByIdQueryHandler.cs`
- `src/Backend/CRM.Application/DiscountApprovals/Validators/GetApprovalByIdQueryValidator.cs`

**Tasks**:
- Create query with ApprovalId, RequestorUserId
- Handler logic:
  - Find approval by ID
  - Verify user has access (requester, approver, or admin)
  - Include full details and navigation properties
  - Return DiscountApprovalDto
- Validator: Validate ApprovalId is provided

#### Step 3.3: Get Approval Timeline Query
**Files**:
- `src/Backend/CRM.Application/DiscountApprovals/Queries/GetApprovalTimelineQuery.cs`
- `src/Backend/CRM.Application/DiscountApprovals/Queries/Handlers/GetApprovalTimelineQueryHandler.cs`
- `src/Backend/CRM.Application/DiscountApprovals/Validators/GetApprovalTimelineQueryValidator.cs`

**Tasks**:
- Create query with ApprovalId (or QuotationId for all approvals)
- Handler logic:
  - Get all approval records for quotation (if QuotationId provided)
  - Sort by CreatedAt descending
  - Map to ApprovalTimelineDto with all events
  - Include resubmissions and escalations
- Validator: Validate either ApprovalId or QuotationId provided

#### Step 3.4: Get Quotation Approvals Query
**Files**:
- `src/Backend/CRM.Application/DiscountApprovals/Queries/GetQuotationApprovalsQuery.cs`
- `src/Backend/CRM.Application/DiscountApprovals/Queries/Handlers/GetQuotationApprovalsQueryHandler.cs`
- `src/Backend/CRM.Application/DiscountApprovals/Validators/GetQuotationApprovalsQueryValidator.cs`

**Tasks**:
- Create query with QuotationId
- Handler logic:
  - Get all approval records for quotation
  - Include current and historical approvals
  - Return list of DiscountApprovalDto
- Validator: Validate QuotationId is provided

#### Step 3.5: Get Approval Metrics Query
**Files**:
- `src/Backend/CRM.Application/DiscountApprovals/Queries/GetApprovalMetricsQuery.cs`
- `src/Backend/CRM.Application/DiscountApprovals/Queries/Handlers/GetApprovalMetricsQueryHandler.cs`
- `src/Backend/CRM.Application/DiscountApprovals/Validators/GetApprovalMetricsQueryValidator.cs`

**Tasks**:
- Create query with DateFrom, DateTo, ApproverUserId (optional filters)
- Handler logic:
  - Calculate metrics: pending count, approved count, rejected count
  - Calculate average approval time (TAT)
  - Calculate rejection rate
  - Calculate average discount percentage
  - Calculate escalation count
  - Return ApprovalMetricsDto
- Validator: Validate date range, admin-only access

---

### Phase 4: API Endpoints (Days 7-8)

**Goal**: Create REST API endpoints for all approval operations.

#### Step 4.1: Create DiscountApprovalsController
**File**: `src/Backend/CRM.Api/Controllers/DiscountApprovalsController.cs`

**Tasks**:
- Create controller with 11 endpoints:
  1. `POST /api/v1/discount-approvals/request` - Request approval
  2. `GET /api/v1/discount-approvals/pending` - Get pending approvals
  3. `POST /api/v1/discount-approvals/{approvalId}/approve` - Approve
  4. `POST /api/v1/discount-approvals/{approvalId}/reject` - Reject
  5. `GET /api/v1/discount-approvals/{approvalId}` - Get approval detail
  6. `POST /api/v1/discount-approvals/{approvalId}/escalate` - Escalate
  7. `GET /api/v1/quotations/{quotationId}/approvals` - Get quotation approvals
  8. `GET /api/v1/discount-approvals/reports` - Get metrics (admin only)
  9. `GET /api/v1/discount-approvals/timeline` - Get timeline
  10. `POST /api/v1/discount-approvals/{approvalId}/resubmit` - Resubmit
  11. `POST /api/v1/discount-approvals/bulk-approve` - Bulk approve
- Add authorization attributes ([Authorize(Roles = "SalesRep,Manager,Admin")])
- Add Swagger documentation
- Handle exceptions and return appropriate HTTP status codes

#### Step 4.2: Register Handlers and Validators
**File**: `src/Backend/CRM.Api/Program.cs`

**Tasks**:
- Register all command handlers (6 handlers)
- Register all query handlers (5 handlers)
- Register all validators (6 validators)
- Ensure AutoMapper profile is registered

---

### Phase 5: Background Jobs & Events (Days 9-10)

**Goal**: Implement auto-escalation and event handlers.

#### Step 5.1: Auto-Escalation Background Job
**Files**:
- `src/Backend/CRM.Infrastructure/Jobs/DiscountApprovalEscalationJob.cs`

**Tasks**:
- Create background job that runs every hour
- Find approvals pending > 24 hours at manager level
- Auto-escalate to admin
- Send notifications
- Log escalation events

#### Step 5.2: Event Handlers
**Files** (in `src/Backend/CRM.Application/DiscountApprovals/EventHandlers/`):
- `DiscountApprovalRequestedEventHandler.cs`
- `DiscountApprovalApprovedEventHandler.cs`
- `DiscountApprovalRejectedEventHandler.cs`
- `DiscountApprovalEscalatedEventHandler.cs`
- `DiscountApprovalResubmittedEventHandler.cs`

**Tasks**:
- Create event handlers for all 5 domain events
- Each handler:
  - Create audit log entry
  - Send email notification
  - Update metrics/analytics
  - Trigger real-time notification (if WebSocket/SignalR available)

#### Step 5.3: Register Event Handlers
**File**: `src/Backend/CRM.Api/Program.cs`

**Tasks**:
- Register all event handlers
- Configure event bus/dispatcher

---

### Phase 6: Frontend API Integration (Day 11)

**Goal**: Create TypeScript types and API service methods.

#### Step 6.1: TypeScript Types
**File**: `src/Frontend/web/src/types/discount-approvals.ts`

**Tasks**:
- Create interfaces matching all backend DTOs:
  - DiscountApproval, DiscountApprovalDto
  - CreateDiscountApprovalRequest
  - ApproveDiscountApprovalRequest
  - RejectDiscountApprovalRequest
  - ResubmitDiscountApprovalRequest
  - BulkApproveRequest
  - ApprovalTimelineDto
  - ApprovalMetricsDto

#### Step 6.2: API Service
**File**: `src/Frontend/web/src/lib/api.ts`

**Tasks**:
- Add `DiscountApprovalsApi` object with 11 methods:
  - `request()`, `getPending()`, `approve()`, `reject()`, `getById()`, `escalate()`, `getQuotationApprovals()`, `getReports()`, `getTimeline()`, `resubmit()`, `bulkApprove()`
- Use existing `apiFetch` pattern
- Handle errors appropriately

---

### Phase 7: Frontend Sales Rep Pages (Days 12-13)

**Goal**: Implement sales rep UI for approval workflow.

#### Step 7.1: Approval Submission Dialog Component
**File**: `src/Frontend/web/src/components/approvals/ApprovalSubmissionModal.tsx`

**Tasks**:
- Create modal component
- Form fields: Reason (required textarea), Comments (optional textarea)
- Submit button with loading state
- Error handling and success feedback
- Auto-close on success

#### Step 7.2: Update Quotation Create/Edit Pages
**Files**:
- `src/Frontend/web/src/app/(protected)/quotations/new/page.tsx`
- `src/Frontend/web/src/app/(protected)/quotations/[id]/edit/page.tsx`

**Tasks**:
- Add discount threshold check (10% for manager, 20% for admin)
- Show approval submission modal when discount >= threshold
- Disable form fields when quotation is locked (IsPendingApproval)
- Show "Pending Approval" banner with status
- Prevent save if pending approval

#### Step 7.3: Update Quotation List Page
**File**: `src/Frontend/web/src/app/(protected)/quotations/page.tsx`

**Tasks**:
- Add "Approval Status" column
- Add ApprovalStatusBadge component
- Add filter for "Pending Approval"
- Color-code status badges

#### Step 7.4: Approval Timeline Component
**File**: `src/Frontend/web/src/components/approvals/ApprovalTimeline.tsx`

**Tasks**:
- Create vertical timeline component
- Display all approval events (request, approve, reject, escalate, resubmit)
- Show user name, role, timestamp, reason, comments
- Expandable comments section
- Icons for each event type

#### Step 7.5: Update Quotation Detail Page
**File**: `src/Frontend/web/src/app/(protected)/quotations/[id]/page.tsx`

**Tasks**:
- Add "Approval Timeline" section
- Display ApprovalTimeline component
- Show current approval status prominently
- Show lock overlay if pending

#### Step 7.6: Locked Form Overlay Component
**File**: `src/Frontend/web/src/components/approvals/LockedFormOverlay.tsx`

**Tasks**:
- Create overlay component that shows when quotation is locked
- Display message: "This quotation is pending approval and cannot be edited"
- Show approval details (who, when, reason)
- Disable all form interactions

---

### Phase 8: Frontend Manager/Admin Pages (Days 14-15)

**Goal**: Implement manager/admin dashboard for approvals.

#### Step 8.1: Approval Dashboard Page
**File**: `src/Frontend/web/src/app/(protected)/approvals/page.tsx`

**Tasks**:
- Create main approval dashboard
- Tabs: "Pending Approvals", "Approved", "Rejected", "All"
- Table with columns: Quotation #, Client, Discount %, SalesRep, Reason, Status, Date, Actions
- Row actions: Approve, Reject, Escalate, View Details
- Expandable rows for quotation preview
- Filters: Sales rep, client, date range, discount %, approval level
- Pagination

#### Step 8.2: Approval Stats Widgets
**File**: `src/Frontend/web/src/components/approvals/ApprovalStatsWidgets.tsx`

**Tasks**:
- Create stats cards:
  - Pending count
  - Average approval time (TAT)
  - Rejection rate
  - Escalation count
- Display in dashboard header

#### Step 8.3: Approval Decision Modal
**File**: `src/Frontend/web/src/components/approvals/ApprovalDecisionModal.tsx`

**Tasks**:
- Create modal for approve/reject actions
- Form fields: Reason (required), Comments (optional)
- Action buttons: Approve, Reject, Cancel
- Show quotation summary in modal
- Loading states and error handling

#### Step 8.4: Bulk Approval Component
**File**: `src/Frontend/web/src/components/approvals/BulkApprovalModal.tsx`

**Tasks**:
- Create modal for bulk approval
- Checkbox selection for multiple approvals
- Single reason/comment field for all selected
- Confirm action with count
- Process all selected approvals
- Show progress and results

#### Step 8.5: Approval Reports Page (Optional)
**File**: `src/Frontend/web/src/app/(protected)/admin/approvals/reports/page.tsx`

**Tasks**:
- Create reports page with charts
- Discount approval trends
- Rejection rate over time
- Average approval time by approver
- Export functionality

---

### Phase 9: Testing & Polish (Days 16-17)

**Goal**: Comprehensive testing and UI polish.

#### Step 9.1: Backend Unit Tests
**Files** (in `tests/CRM.Tests/DiscountApprovals/`):
- `RequestDiscountApprovalCommandHandlerTests.cs`
- `ApproveDiscountApprovalCommandHandlerTests.cs`
- `RejectDiscountApprovalCommandHandlerTests.cs`
- `EscalateDiscountApprovalCommandHandlerTests.cs`
- `ResubmitDiscountApprovalCommandHandlerTests.cs`
- `BulkApproveDiscountApprovalsCommandHandlerTests.cs`
- `GetPendingApprovalsQueryHandlerTests.cs`
- `GetApprovalByIdQueryHandlerTests.cs`
- `GetApprovalMetricsQueryHandlerTests.cs`

**Tasks**:
- Create unit tests for all 6 commands
- Create unit tests for all 5 queries
- Test authorization logic
- Test quotation locking/unlocking
- Test threshold determination

#### Step 9.2: Backend Integration Tests
**File**: `tests/CRM.Tests.Integration/DiscountApprovals/DiscountApprovalsControllerTests.cs`

**Tasks**:
- Test all 11 API endpoints
- Test authorization (sales rep, manager, admin)
- Test quotation locking behavior
- Test bulk operations
- Test error scenarios

#### Step 9.3: Frontend Component Tests
**Files** (in `src/Frontend/web/src/components/approvals/__tests__/`):
- Test ApprovalSubmissionModal
- Test ApprovalTimeline
- Test ApprovalDecisionModal
- Test LockedFormOverlay
- Test ApprovalStatusBadge

**Tasks**:
- Test component rendering
- Test form validation
- Test user interactions
- Test error states

#### Step 9.4: Error Boundaries & Loading States
**Tasks**:
- Add error boundaries to approval pages
- Add loading skeletons
- Add toast notifications for all actions
- Verify mobile responsiveness
- Verify accessibility (ARIA labels, keyboard navigation)

#### Step 9.5: Documentation
**Tasks**:
- Update API documentation (Swagger)
- Create quickstart guide
- Update requirements checklist

---

## Dependencies

### External Dependencies
- Spec-009: Quotation Entity (must exist)
- Spec-010: Quotation Management (for quotation operations)
- Spec-011: Quotation Template Management (optional, for template discounts)
- Spec-004: RBAC (for role-based authorization)

### Internal Dependencies
- Notification system (for email/SMS notifications)
- Event bus (for domain events)
- Background job scheduler (for auto-escalation)

---

## Configuration

### Discount Thresholds (appsettings.json)
```json
{
  "DiscountApproval": {
    "ManagerThreshold": 10.0,
    "AdminThreshold": 20.0,
    "AutoEscalationHours": 24,
    "BulkApprovalMaxCount": 50
  }
}
```

---

## Risk Mitigation

1. **Quotation Locking**: Ensure locking mechanism is robust and cannot be bypassed
2. **Concurrent Approvals**: Handle race conditions when multiple approvers act simultaneously
3. **Notification Spam**: Rate limit notifications to prevent email flooding
4. **Performance**: Index all query fields, optimize approval queue queries
5. **Data Integrity**: Ensure approval records are never orphaned

---

## Success Metrics

- All 11 API endpoints functional and tested
- Quotation locking works correctly (no edits while pending)
- Approval workflow completes end-to-end (request → approve/reject)
- Notifications sent at each step
- Dashboard shows accurate pending queue
- Timeline displays complete audit trail
- Bulk operations work efficiently
- Auto-escalation triggers correctly

---

## Deliverables Checklist

### Backend
- [ ] Database migration
- [ ] Domain entities (DiscountApproval, enums)
- [ ] Quotation entity updates (locking)
- [ ] 8 DTOs
- [ ] AutoMapper profile
- [ ] 4 custom exceptions
- [ ] 5 domain events
- [ ] 6 command handlers + validators
- [ ] 5 query handlers + validators
- [ ] API controller (11 endpoints)
- [ ] 5 event handlers
- [ ] Background job (auto-escalation)
- [ ] Unit tests (9+ files)
- [ ] Integration tests (1+ file)

### Frontend
- [ ] TypeScript types file
- [ ] API service (11 methods)
- [ ] 5 reusable components
- [ ] 3 sales rep page updates
- [ ] 2 manager/admin pages
- [ ] Error boundaries
- [ ] Loading states
- [ ] Toast notifications
- [ ] Component tests (5+ files)

---

**Estimated Total Time**: 17 days  
**Critical Path**: Phases 1-4 (Backend foundation) must complete before frontend work

