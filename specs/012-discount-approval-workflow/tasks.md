# Task Breakdown: Spec-012 Discount Approval Workflow

**Spec**: Spec-012  
**Last Updated**: 2025-01-XX

## Overview

This document provides a detailed task breakdown for implementing Spec-012: Quotation Discount Approval Workflow. Tasks are organized by phase and include priority markers `[P]` for parallelizable work.

---

## Phase 1: Setup & Foundational

**Purpose**: Establish database schema, entities, DTOs, and basic infrastructure.

### Database & Migrations

- [ ] T1 [P] Create migration `CreateDiscountApprovalsTable`:
  - Create `DiscountApprovals` table with all 16 columns
  - Add foreign keys (QuotationId → Quotations, RequestedByUserId → Users, ApproverUserId → Users)
  - Add check constraints (Status enum, CurrentDiscountPercentage >= 0, Threshold >= 0)
  - Add all indexes (ApproverUserId+Status, QuotationId, RequestedByUserId, CurrentDiscountPercentage, Status, CreatedAt+Status)
  - Add default values (EscalatedToAdmin = false, CreatedAt/UpdatedAt = CURRENT_TIMESTAMP)

- [ ] T2 [P] Create migration `AddQuotationApprovalLocking`:
  - Add `IsPendingApproval` column (BOOLEAN, NOT NULL, DEFAULT FALSE)
  - Add `PendingApprovalId` column (UUID, NULLABLE, FK → DiscountApprovals)
  - Add index on `IsPendingApproval`
  - Add index on `PendingApprovalId`

**Checkpoint**: Migrations run successfully, tables created.

---

### Domain Entities

- [ ] T3 [P] Create `src/Backend/CRM.Domain/Enums/ApprovalStatus.cs`:
  - Pending, Approved, Rejected values

- [ ] T4 [P] Create `src/Backend/CRM.Domain/Enums/ApprovalLevel.cs`:
  - Manager, Admin values

- [ ] T5 [P] Create `src/Backend/CRM.Domain/Entities/DiscountApproval.cs`:
  - All 16 properties with correct types
  - Navigation properties: Quotation, RequestedByUser, ApproverUser
  - Domain methods: `Approve()`, `Reject()`, `Escalate()`, `CanBeApprovedBy()`, `IsPending()`, `IsApproved()`, `IsRejected()`

- [ ] T6 [P] Update `src/Backend/CRM.Domain/Entities/Quotation.cs`:
  - Add `IsPendingApproval` property (bool)
  - Add `PendingApprovalId` property (Guid?, nullable)
  - Add navigation property to `DiscountApproval` (optional)
  - Add domain method: `IsLockedForEditing()` that returns true if IsPendingApproval
  - Add domain method: `LockForApproval(Guid approvalId)`
  - Add domain method: `UnlockFromApproval()`

**Checkpoint**: Entities compile and pass basic validation.

---

### Entity Framework Configuration

- [ ] T7 [P] Create `src/Backend/CRM.Infrastructure/EntityConfigurations/DiscountApprovalEntityConfiguration.cs`:
  - Table name mapping
  - Primary key configuration
  - Property constraints (max lengths, required, defaults)
  - Enum to string conversion for Status and ApprovalLevel
  - Relationships (Quotation, RequestedByUser, ApproverUser)
  - Indexes configuration (all 6 indexes)

- [ ] T8 [P] Update `src/Backend/CRM.Infrastructure/EntityConfigurations/QuotationEntityConfiguration.cs`:
  - Add `IsPendingApproval` property configuration
  - Add `PendingApprovalId` property configuration
  - Add optional relationship to DiscountApproval
  - Add index on `IsPendingApproval`
  - Add index on `PendingApprovalId`

- [ ] T9 [P] Update `src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs`:
  - Add `DbSet<DiscountApproval> DiscountApprovals`

- [ ] T10 [P] Update `src/Backend/CRM.Application/Common/Persistence/IAppDbContext.cs`:
  - Add `DbSet<DiscountApproval> DiscountApprovals`

**Checkpoint**: EF Core configuration complete, migrations generate correctly.

---

### DTOs

- [ ] T11 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Dtos/DiscountApprovalDto.cs`:
  - All approval properties
  - `QuotationNumber` (mapped from Quotation)
  - `ClientName` (mapped from Quotation.Client)
  - `RequestedByUserName` (mapped from RequestedByUser)
  - `ApproverUserName` (mapped from ApproverUser, nullable)
  - Computed properties: `IsPending`, `IsApproved`, `IsRejected`

- [ ] T12 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Dtos/CreateDiscountApprovalRequest.cs`:
  - QuotationId (required)
  - DiscountPercentage (required, decimal)
  - Reason (required, string)
  - Comments (optional, string)

- [ ] T13 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Dtos/ApproveDiscountApprovalRequest.cs`:
  - Reason (required, string)
  - Comments (optional, string)

- [ ] T14 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Dtos/RejectDiscountApprovalRequest.cs`:
  - Reason (required, string)
  - Comments (optional, string)

- [ ] T15 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Dtos/ResubmitDiscountApprovalRequest.cs`:
  - Reason (required, string)
  - Comments (optional, string)

- [ ] T16 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Dtos/BulkApproveRequest.cs`:
  - ApprovalIds (required, List<Guid>)
  - Reason (required, string)
  - Comments (optional, string)

- [ ] T17 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Dtos/ApprovalTimelineDto.cs`:
  - ApprovalId, QuotationId, EventType, Status, UserId, UserName, UserRole
  - Reason, Comments, Timestamp
  - PreviousStatus (for state transitions)

- [ ] T18 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Dtos/ApprovalMetricsDto.cs`:
  - PendingCount, ApprovedCount, RejectedCount
  - AverageApprovalTime (TimeSpan)
  - RejectionRate (decimal)
  - AverageDiscountPercentage (decimal)
  - EscalationCount
  - DateRange (DateFrom, DateTo)

**Checkpoint**: All DTOs created and compile.

---

### AutoMapper Profiles

- [ ] T19 [P] Create `src/Backend/CRM.Application/Mapping/DiscountApprovalProfile.cs`:
  - Map DiscountApproval → DiscountApprovalDto
  - Map CreateDiscountApprovalRequest → DiscountApproval
  - Resolve QuotationNumber from Quotation navigation
  - Resolve ClientName from Quotation.Client navigation
  - Resolve RequestedByUserName from RequestedByUser navigation
  - Resolve ApproverUserName from ApproverUser navigation (nullable)

**Checkpoint**: AutoMapper profiles configured.

---

### Exceptions

- [ ] T20 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Exceptions/DiscountApprovalNotFoundException.cs`

- [ ] T21 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Exceptions/QuotationLockedException.cs`

- [ ] T22 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Exceptions/UnauthorizedApprovalActionException.cs`

- [ ] T23 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Exceptions/InvalidApprovalStatusException.cs`

**Checkpoint**: Exception classes created.

---

### Domain Events

- [ ] T24 [P] Create `src/Backend/CRM.Domain/Events/DiscountApprovalRequested.cs`:
  - ApprovalId, QuotationId, RequestedByUserId, ApproverUserId, DiscountPercentage, Reason

- [ ] T25 [P] Create `src/Backend/CRM.Domain/Events/DiscountApprovalApproved.cs`:
  - ApprovalId, QuotationId, ApprovedByUserId, Reason, Comments

- [ ] T26 [P] Create `src/Backend/CRM.Domain/Events/DiscountApprovalRejected.cs`:
  - ApprovalId, QuotationId, RejectedByUserId, Reason, Comments

- [ ] T27 [P] Create `src/Backend/CRM.Domain/Events/DiscountApprovalEscalated.cs`:
  - ApprovalId, QuotationId, EscalatedByUserId, Reason

- [ ] T28 [P] Create `src/Backend/CRM.Domain/Events/DiscountApprovalResubmitted.cs`:
  - ApprovalId, QuotationId, PreviousApprovalId, ResubmittedByUserId, Reason

**Checkpoint**: Domain events created.

---

## Phase 2: Backend Commands

**Purpose**: Implement CQRS commands for approval workflow actions.

### Request Discount Approval Command

- [ ] T29 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Commands/RequestDiscountApprovalCommand.cs`:
  - Request (CreateDiscountApprovalRequest)
  - RequestedByUserId (Guid)
  - Response (DiscountApprovalDto)

- [ ] T30 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Commands/Handlers/RequestDiscountApprovalCommandHandler.cs`:
  - Validate quotation exists and is not already pending
  - Calculate discount percentage from quotation
  - Determine approval level based on threshold (10-20% = Manager, >20% = Admin)
  - Find appropriate approver (manager or admin) based on role
  - Create DiscountApproval record
  - Lock quotation (set IsPendingApproval = true, PendingApprovalId)
  - Publish DiscountApprovalRequested event
  - Return DiscountApprovalDto

- [ ] T31 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Validators/RequestDiscountApprovalCommandValidator.cs`:
  - Validate QuotationId is provided and exists
  - Validate DiscountPercentage >= threshold
  - Validate Reason is not empty
  - Validate quotation is not already pending approval

**Checkpoint**: Request command works, quotation locks correctly.

---

### Approve Discount Command

- [ ] T32 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Commands/ApproveDiscountApprovalCommand.cs`:
  - ApprovalId (Guid)
  - Request (ApproveDiscountApprovalRequest)
  - ApprovedByUserId (Guid)
  - Response (DiscountApprovalDto)

- [ ] T33 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Commands/Handlers/ApproveDiscountApprovalCommandHandler.cs`:
  - Validate approval exists and is pending
  - Verify user has permission to approve (is assigned approver or admin)
  - Update approval status to Approved
  - Set ApprovalDate
  - Update quotation discount percentage
  - Unlock quotation (set IsPendingApproval = false, clear PendingApprovalId)
  - Publish DiscountApprovalApproved event
  - Return DiscountApprovalDto

- [ ] T34 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Validators/ApproveDiscountApprovalCommandValidator.cs`:
  - Validate ApprovalId is provided
  - Validate Reason is not empty
  - Validate approval is in Pending status
  - Validate user is authorized (approver or admin)

**Checkpoint**: Approve command works, quotation unlocks and discount applied.

---

### Reject Discount Command

- [ ] T35 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Commands/RejectDiscountApprovalCommand.cs`:
  - ApprovalId (Guid)
  - Request (RejectDiscountApprovalRequest)
  - RejectedByUserId (Guid)
  - Response (DiscountApprovalDto)

- [ ] T36 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Commands/Handlers/RejectDiscountApprovalCommandHandler.cs`:
  - Validate approval exists and is pending
  - Verify user has permission to reject
  - Update approval status to Rejected
  - Set RejectionDate
  - Revert quotation discount to previous value (or 0)
  - Unlock quotation
  - Publish DiscountApprovalRejected event
  - Return DiscountApprovalDto

- [ ] T37 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Validators/RejectDiscountApprovalCommandValidator.cs`:
  - Validate ApprovalId is provided
  - Validate Reason is not empty
  - Validate approval is in Pending status
  - Validate user is authorized

**Checkpoint**: Reject command works, quotation unlocks and discount reverted.

---

### Escalate to Admin Command

- [ ] T38 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Commands/EscalateDiscountApprovalCommand.cs`:
  - ApprovalId (Guid)
  - EscalatedByUserId (Guid)
  - Reason (string, optional)
  - Response (DiscountApprovalDto)

- [ ] T39 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Commands/Handlers/EscalateDiscountApprovalCommandHandler.cs`:
  - Validate approval exists and is pending
  - Verify escalation is allowed (manager can escalate, or auto-escalation)
  - Update EscalatedToAdmin = true
  - Change ApprovalLevel to Admin
  - Reassign ApproverUserId to admin user
  - Publish DiscountApprovalEscalated event
  - Return DiscountApprovalDto

- [ ] T40 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Validators/EscalateDiscountApprovalCommandValidator.cs`:
  - Validate ApprovalId is provided
  - Validate approval is in Pending status
  - Validate escalation is allowed (not already escalated, or manager/admin)

**Checkpoint**: Escalate command works, approval reassigned to admin.

---

### Resubmit Approval Command

- [ ] T41 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Commands/ResubmitDiscountApprovalCommand.cs`:
  - ApprovalId (Guid, the rejected approval)
  - Request (ResubmitDiscountApprovalRequest)
  - ResubmittedByUserId (Guid)
  - Response (DiscountApprovalDto)

- [ ] T42 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Commands/Handlers/ResubmitDiscountApprovalCommandHandler.cs`:
  - Validate original approval exists and is rejected
  - Verify user is original requester
  - Get quotation and current discount
  - Create new approval record (linked to previous via history)
  - Lock quotation again
  - Publish DiscountApprovalResubmitted event
  - Return new DiscountApprovalDto

- [ ] T43 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Validators/ResubmitDiscountApprovalCommandValidator.cs`:
  - Validate ApprovalId is provided
  - Validate Reason is not empty
  - Validate approval is in Rejected status
  - Validate user is the original requester

**Checkpoint**: Resubmit command works, new approval created and quotation locked.

---

### Bulk Approve Command

- [ ] T44 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Commands/BulkApproveDiscountApprovalsCommand.cs`:
  - Request (BulkApproveRequest)
  - ApprovedByUserId (Guid)
  - Response (List<DiscountApprovalDto>)

- [ ] T45 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Commands/Handlers/BulkApproveDiscountApprovalsCommandHandler.cs`:
  - Validate all approvals exist and are pending
  - Verify user has permission to approve all (admin or assigned approver)
  - Process each approval (similar to single approve)
  - Publish DiscountApprovalApproved event for each
  - Return list of DiscountApprovalDto

- [ ] T46 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Validators/BulkApproveDiscountApprovalsCommandValidator.cs`:
  - Validate ApprovalIds list is not empty
  - Validate Reason is not empty
  - Validate all approvals are in Pending status
  - Validate user is authorized for all approvals

**Checkpoint**: Bulk approve command works, multiple approvals processed.

---

## Phase 3: Backend Queries

**Purpose**: Implement CQRS queries for retrieving approval data.

### Get Pending Approvals Query

- [ ] T47 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Queries/GetPendingApprovalsQuery.cs`:
  - ApproverUserId (Guid?, optional filter)
  - Status (ApprovalStatus?, optional filter)
  - DiscountPercentageMin (decimal?, optional)
  - DiscountPercentageMax (decimal?, optional)
  - DateFrom (DateTimeOffset?, optional)
  - DateTo (DateTimeOffset?, optional)
  - RequestedByUserId (Guid?, optional)
  - PageNumber (int, default 1)
  - PageSize (int, default 20)
  - Response (PagedResult<DiscountApprovalDto>)

- [ ] T48 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Queries/Handlers/GetPendingApprovalsQueryHandler.cs`:
  - Filter by approver (if manager, show manager-level; if admin, show all)
  - Apply all filters (status, discount %, date range, requester)
  - Include navigation properties (Quotation, Users)
  - Paginate results
  - Return paginated DTOs

- [ ] T49 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Validators/GetPendingApprovalsQueryValidator.cs`:
  - Validate PageNumber >= 1
  - Validate PageSize between 1 and 100
  - Validate DateFrom <= DateTo if both provided
  - Validate DiscountPercentageMin <= DiscountPercentageMax if both provided

**Checkpoint**: Query returns correct pending approvals with filters.

---

### Get Approval By Id Query

- [ ] T50 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Queries/GetApprovalByIdQuery.cs`:
  - ApprovalId (Guid)
  - RequestorUserId (Guid, for authorization)
  - Response (DiscountApprovalDto)

- [ ] T51 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Queries/Handlers/GetApprovalByIdQueryHandler.cs`:
  - Find approval by ID
  - Verify user has access (requester, approver, or admin)
  - Include full details and navigation properties
  - Return DiscountApprovalDto

- [ ] T52 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Validators/GetApprovalByIdQueryValidator.cs`:
  - Validate ApprovalId is provided

**Checkpoint**: Query returns approval detail with authorization check.

---

### Get Approval Timeline Query

- [ ] T53 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Queries/GetApprovalTimelineQuery.cs`:
  - ApprovalId (Guid?, optional)
  - QuotationId (Guid?, optional)
  - Response (List<ApprovalTimelineDto>)

- [ ] T54 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Queries/Handlers/GetApprovalTimelineQueryHandler.cs`:
  - Get all approval records for quotation (if QuotationId provided)
  - Or get single approval timeline (if ApprovalId provided)
  - Sort by CreatedAt descending
  - Map to ApprovalTimelineDto with all events
  - Include resubmissions and escalations

- [ ] T55 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Validators/GetApprovalTimelineQueryValidator.cs`:
  - Validate either ApprovalId or QuotationId provided (not both, not neither)

**Checkpoint**: Query returns complete timeline with all events.

---

### Get Quotation Approvals Query

- [ ] T56 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Queries/GetQuotationApprovalsQuery.cs`:
  - QuotationId (Guid)
  - Response (List<DiscountApprovalDto>)

- [ ] T57 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Queries/Handlers/GetQuotationApprovalsQueryHandler.cs`:
  - Get all approval records for quotation
  - Include current and historical approvals
  - Sort by CreatedAt descending
  - Return list of DiscountApprovalDto

- [ ] T58 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Validators/GetQuotationApprovalsQueryValidator.cs`:
  - Validate QuotationId is provided

**Checkpoint**: Query returns all approvals for a quotation.

---

### Get Approval Metrics Query

- [ ] T59 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Queries/GetApprovalMetricsQuery.cs`:
  - DateFrom (DateTimeOffset?, optional)
  - DateTo (DateTimeOffset?, optional)
  - ApproverUserId (Guid?, optional filter)
  - Response (ApprovalMetricsDto)

- [ ] T60 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Queries/Handlers/GetApprovalMetricsQueryHandler.cs`:
  - Calculate metrics: pending count, approved count, rejected count
  - Calculate average approval time (TAT) from RequestDate to ApprovalDate
  - Calculate rejection rate (rejected / total)
  - Calculate average discount percentage
  - Calculate escalation count
  - Return ApprovalMetricsDto
  - Admin-only access

- [ ] T61 [P] Create `src/Backend/CRM.Application/DiscountApprovals/Validators/GetApprovalMetricsQueryValidator.cs`:
  - Validate DateFrom <= DateTo if both provided
  - Validate user is admin (authorization)

**Checkpoint**: Query returns accurate metrics for reporting.

---

## Phase 4: API Endpoints

**Purpose**: Create REST API endpoints for all approval operations.

### DiscountApprovalsController

- [ ] T62 [P] Create `src/Backend/CRM.Api/Controllers/DiscountApprovalsController.cs`:
  - `POST /api/v1/discount-approvals/request` - Request approval
  - `GET /api/v1/discount-approvals/pending` - Get pending approvals
  - `POST /api/v1/discount-approvals/{approvalId}/approve` - Approve
  - `POST /api/v1/discount-approvals/{approvalId}/reject` - Reject
  - `GET /api/v1/discount-approvals/{approvalId}` - Get approval detail
  - `POST /api/v1/discount-approvals/{approvalId}/escalate` - Escalate
  - `GET /api/v1/quotations/{quotationId}/approvals` - Get quotation approvals
  - `GET /api/v1/discount-approvals/reports` - Get metrics (admin only)
  - `GET /api/v1/discount-approvals/timeline` - Get timeline
  - `POST /api/v1/discount-approvals/{approvalId}/resubmit` - Resubmit
  - `POST /api/v1/discount-approvals/bulk-approve` - Bulk approve
  - Add authorization attributes ([Authorize(Roles = "SalesRep,Manager,Admin")])
  - Add Swagger documentation
  - Handle exceptions and return appropriate HTTP status codes

- [ ] T63 [P] Register all handlers and validators in `src/Backend/CRM.Api/Program.cs`:
  - Register 6 command handlers
  - Register 5 query handlers
  - Register 6 validators
  - Ensure AutoMapper profile is registered

**Checkpoint**: All 11 endpoints functional, Swagger documentation complete.

---

## Phase 5: Background Jobs & Events

**Purpose**: Implement auto-escalation and event handlers.

### Auto-Escalation Background Job

- [ ] T64 [P] Create `src/Backend/CRM.Infrastructure/Jobs/DiscountApprovalEscalationJob.cs`:
  - Run every hour (cron: "0 * * * *")
  - Find approvals pending > 24 hours at manager level
  - Auto-escalate to admin (call EscalateDiscountApprovalCommand)
  - Send notifications
  - Log escalation events

- [ ] T65 [P] Register background job in `src/Backend/CRM.Api/Program.cs`:
  - Add to CronBackgroundService registration

**Checkpoint**: Auto-escalation works, runs hourly.

---

### Event Handlers

- [ ] T66 [P] Create `src/Backend/CRM.Application/DiscountApprovals/EventHandlers/DiscountApprovalRequestedEventHandler.cs`:
  - Create audit log entry
  - Send email notification to approver
  - Update metrics/analytics
  - Trigger real-time notification (if WebSocket/SignalR available)

- [ ] T67 [P] Create `src/Backend/CRM.Application/DiscountApprovals/EventHandlers/DiscountApprovalApprovedEventHandler.cs`:
  - Create audit log entry
  - Send email notification to sales rep (and optionally client)
  - Update metrics/analytics
  - Trigger real-time notification

- [ ] T68 [P] Create `src/Backend/CRM.Application/DiscountApprovals/EventHandlers/DiscountApprovalRejectedEventHandler.cs`:
  - Create audit log entry
  - Send email notification to sales rep
  - Update metrics/analytics
  - Trigger real-time notification

- [ ] T69 [P] Create `src/Backend/CRM.Application/DiscountApprovals/EventHandlers/DiscountApprovalEscalatedEventHandler.cs`:
  - Create audit log entry
  - Send email notification to admin
  - Update metrics/analytics
  - Trigger real-time notification

- [ ] T70 [P] Create `src/Backend/CRM.Application/DiscountApprovals/EventHandlers/DiscountApprovalResubmittedEventHandler.cs`:
  - Create audit log entry
  - Send email notification to approver
  - Update metrics/analytics
  - Trigger real-time notification

- [ ] T71 [P] Register all event handlers in `src/Backend/CRM.Api/Program.cs`:
  - Register 5 event handlers
  - Configure event bus/dispatcher

**Checkpoint**: All events trigger handlers, notifications sent.

---

## Phase 6: Frontend API Integration

**Purpose**: Create TypeScript types and API service methods.

### TypeScript Types

- [ ] T72 [P] Create `src/Frontend/web/src/types/discount-approvals.ts`:
  - DiscountApproval interface
  - DiscountApprovalDto interface
  - CreateDiscountApprovalRequest interface
  - ApproveDiscountApprovalRequest interface
  - RejectDiscountApprovalRequest interface
  - ResubmitDiscountApprovalRequest interface
  - BulkApproveRequest interface
  - ApprovalTimelineDto interface
  - ApprovalMetricsDto interface
  - ApprovalStatus enum (TypeScript)
  - ApprovalLevel enum (TypeScript)

### API Service

- [ ] T73 [P] Extend `src/Frontend/web/src/lib/api.ts`:
  - Add `DiscountApprovalsApi` object with 11 methods:
    - `request(quotationId, request)` - POST /api/v1/discount-approvals/request
    - `getPending(filters)` - GET /api/v1/discount-approvals/pending
    - `approve(approvalId, request)` - POST /api/v1/discount-approvals/{id}/approve
    - `reject(approvalId, request)` - POST /api/v1/discount-approvals/{id}/reject
    - `getById(approvalId)` - GET /api/v1/discount-approvals/{id}
    - `escalate(approvalId, reason)` - POST /api/v1/discount-approvals/{id}/escalate
    - `getQuotationApprovals(quotationId)` - GET /api/v1/quotations/{id}/approvals
    - `getReports(filters)` - GET /api/v1/discount-approvals/reports
    - `getTimeline(approvalId?, quotationId?)` - GET /api/v1/discount-approvals/timeline
    - `resubmit(approvalId, request)` - POST /api/v1/discount-approvals/{id}/resubmit
    - `bulkApprove(request)` - POST /api/v1/discount-approvals/bulk-approve
  - Use existing `apiFetch` pattern
  - Handle errors appropriately

**Checkpoint**: API integration complete, types match backend DTOs.

---

## Phase 7: Frontend Sales Rep Pages

**Purpose**: Implement sales rep UI for approval workflow.

### Approval Submission Modal

- [ ] T74 [P] Create `src/Frontend/web/src/components/approvals/ApprovalSubmissionModal.tsx`:
  - Modal component with form
  - Form fields: Reason (required textarea), Comments (optional textarea)
  - Submit button with loading state
  - Error handling and success feedback
  - Auto-close on success
  - Display discount percentage and threshold

### Update Quotation Pages

- [ ] T75 [P] Update `src/Frontend/web/src/app/(protected)/quotations/new/page.tsx`:
  - Add discount threshold check (10% for manager, 20% for admin)
  - Show approval submission modal when discount >= threshold
  - Disable form fields when quotation is locked (IsPendingApproval)
  - Show "Pending Approval" banner with status

- [ ] T76 [P] Update `src/Frontend/web/src/app/(protected)/quotations/[id]/edit/page.tsx`:
  - Add discount threshold check
  - Show approval submission modal when discount >= threshold
  - Disable form fields when quotation is locked
  - Show "Pending Approval" banner
  - Prevent save if pending approval

### Update Quotation List

- [ ] T77 [P] Update `src/Frontend/web/src/app/(protected)/quotations/page.tsx`:
  - Add "Approval Status" column
  - Add ApprovalStatusBadge component
  - Add filter for "Pending Approval"
  - Color-code status badges (Pending=Yellow, Approved=Green, Rejected=Red)

### Approval Timeline Component

- [ ] T78 [P] Create `src/Frontend/web/src/components/approvals/ApprovalTimeline.tsx`:
  - Vertical timeline component
  - Display all approval events (request, approve, reject, escalate, resubmit)
  - Show user name, role, timestamp, reason, comments
  - Expandable comments section
  - Icons for each event type
  - Responsive design

### Update Quotation Detail Page

- [ ] T79 [P] Update `src/Frontend/web/src/app/(protected)/quotations/[id]/page.tsx`:
  - Add "Approval Timeline" section
  - Display ApprovalTimeline component
  - Show current approval status prominently
  - Show lock overlay if pending

### Locked Form Overlay Component

- [ ] T80 [P] Create `src/Frontend/web/src/components/approvals/LockedFormOverlay.tsx`:
  - Overlay component that shows when quotation is locked
  - Display message: "This quotation is pending approval and cannot be edited"
  - Show approval details (who, when, reason)
  - Disable all form interactions
  - Styled with TailAdmin theme

### Approval Status Badge Component

- [ ] T81 [P] Create `src/Frontend/web/src/components/approvals/ApprovalStatusBadge.tsx`:
  - Badge component for approval status
  - Color-coded (Pending=Yellow, Approved=Green, Rejected=Red)
  - Shows status text
  - Accessible (ARIA labels)

**Checkpoint**: Sales rep pages functional, approval workflow visible.

---

## Phase 8: Frontend Manager/Admin Pages

**Purpose**: Implement manager/admin dashboard for approvals.

### Approval Dashboard Page

- [ ] T82 [P] Create `src/Frontend/web/src/app/(protected)/approvals/page.tsx`:
  - Main approval dashboard
  - Tabs: "Pending Approvals", "Approved", "Rejected", "All"
  - Table with columns: Quotation #, Client, Discount %, SalesRep, Reason, Status, Date, Actions
  - Row actions: Approve, Reject, Escalate, View Details
  - Expandable rows for quotation preview
  - Filters: Sales rep, client, date range, discount %, approval level
  - Pagination
  - Bulk selection checkbox

### Approval Stats Widgets

- [ ] T83 [P] Create `src/Frontend/web/src/components/approvals/ApprovalStatsWidgets.tsx`:
  - Stats cards:
    - Pending count
    - Average approval time (TAT)
    - Rejection rate
    - Escalation count
  - Display in dashboard header
  - Real-time updates

### Approval Decision Modal

- [ ] T84 [P] Create `src/Frontend/web/src/components/approvals/ApprovalDecisionModal.tsx`:
  - Modal for approve/reject actions
  - Form fields: Reason (required), Comments (optional)
  - Action buttons: Approve, Reject, Cancel
  - Show quotation summary in modal
  - Loading states and error handling
  - Confirmation before submit

### Bulk Approval Component

- [ ] T85 [P] Create `src/Frontend/web/src/components/approvals/BulkApprovalModal.tsx`:
  - Modal for bulk approval
  - Checkbox selection for multiple approvals
  - Single reason/comment field for all selected
  - Confirm action with count
  - Process all selected approvals
  - Show progress and results
  - Error handling for partial failures

### Approval Reports Page (Optional)

- [ ] T86 [P] Create `src/Frontend/web/src/app/(protected)/admin/approvals/reports/page.tsx`:
  - Reports page with charts
  - Discount approval trends (line chart)
  - Rejection rate over time (bar chart)
  - Average approval time by approver (bar chart)
  - Export functionality (CSV, PDF)
  - Date range filters

### Approval Comments Panel

- [ ] T87 [P] Create `src/Frontend/web/src/components/approvals/ApprovalCommentsPanel.tsx`:
  - View all comments/reasons chronologically
  - Expandable sections
  - User attribution (name, role, timestamp)
  - Styled with TailAdmin theme

**Checkpoint**: Manager/admin dashboard functional, bulk operations work.

---

## Phase 9: Testing & Polish

**Purpose**: Comprehensive testing and UI polish.

### Backend Unit Tests

- [ ] T88 [P] Create `tests/CRM.Tests/DiscountApprovals/RequestDiscountApprovalCommandHandlerTests.cs`:
  - Test successful request
  - Test quotation locking
  - Test threshold determination
  - Test approver assignment
  - Test validation errors

- [ ] T89 [P] Create `tests/CRM.Tests/DiscountApprovals/ApproveDiscountApprovalCommandHandlerTests.cs`:
  - Test successful approval
  - Test quotation unlocking
  - Test discount application
  - Test authorization
  - Test validation errors

- [ ] T90 [P] Create `tests/CRM.Tests/DiscountApprovals/RejectDiscountApprovalCommandHandlerTests.cs`:
  - Test successful rejection
  - Test quotation unlocking
  - Test discount reversion
  - Test authorization
  - Test validation errors

- [ ] T91 [P] Create `tests/CRM.Tests/DiscountApprovals/EscalateDiscountApprovalCommandHandlerTests.cs`:
  - Test successful escalation
  - Test approver reassignment
  - Test authorization
  - Test validation errors

- [ ] T92 [P] Create `tests/CRM.Tests/DiscountApprovals/ResubmitDiscountApprovalCommandHandlerTests.cs`:
  - Test successful resubmission
  - Test new approval creation
  - Test quotation locking
  - Test validation errors

- [ ] T93 [P] Create `tests/CRM.Tests/DiscountApprovals/BulkApproveDiscountApprovalsCommandHandlerTests.cs`:
  - Test successful bulk approval
  - Test partial failures
  - Test authorization
  - Test validation errors

- [ ] T94 [P] Create `tests/CRM.Tests/DiscountApprovals/GetPendingApprovalsQueryHandlerTests.cs`:
  - Test filtering
  - Test pagination
  - Test authorization
  - Test sorting

- [ ] T95 [P] Create `tests/CRM.Tests/DiscountApprovals/GetApprovalByIdQueryHandlerTests.cs`:
  - Test successful retrieval
  - Test authorization
  - Test not found

- [ ] T96 [P] Create `tests/CRM.Tests/DiscountApprovals/GetApprovalMetricsQueryHandlerTests.cs`:
  - Test metrics calculation
  - Test date filtering
  - Test admin-only access

### Backend Integration Tests

- [ ] T97 [P] Create `tests/CRM.Tests.Integration/DiscountApprovals/DiscountApprovalsControllerTests.cs`:
  - Test all 11 API endpoints
  - Test authorization (sales rep, manager, admin)
  - Test quotation locking behavior
  - Test bulk operations
  - Test error scenarios
  - Test event publishing

### Frontend Component Tests

- [ ] T98 [P] Create component tests for:
  - ApprovalSubmissionModal
  - ApprovalTimeline
  - ApprovalDecisionModal
  - LockedFormOverlay
  - ApprovalStatusBadge
  - ApprovalStatsWidgets
  - BulkApprovalModal

**Note**: Component tests may be skipped if Jest/React Testing Library is not configured.

### Error Boundaries & Loading States

- [ ] T99 [P] Add error boundaries to approval pages:
  - Approval dashboard page
  - Quotation detail page (approval section)
  - Approval reports page

- [ ] T100 [P] Add loading skeletons:
  - Approval list skeleton
  - Approval detail skeleton
  - Timeline skeleton

- [ ] T101 [P] Add toast notifications:
  - Approval requested
  - Approval approved
  - Approval rejected
  - Approval escalated
  - Bulk approval results
  - Error notifications

- [ ] T102 [P] Verify mobile responsiveness:
  - Tables are scrollable
  - Modals are full-screen on mobile
  - Forms are accessible
  - Buttons are tappable

- [ ] T103 [P] Verify accessibility:
  - ARIA labels on all interactive elements
  - Keyboard navigation works
  - Screen reader compatibility
  - Color contrast meets WCAG standards

### Documentation

- [ ] T104 [P] Update API documentation (Swagger):
  - All 11 endpoints documented
  - Request/response examples
  - Error responses

- [ ] T105 [P] Create quickstart guide:
  - `specs/012-discount-approval-workflow/quickstart.md`
  - Setup instructions
  - Configuration
  - Verification steps

- [ ] T106 [P] Update requirements checklist:
  - `specs/012-discount-approval-workflow/checklists/requirements.md`
  - All acceptance criteria listed
  - Verification steps

**Checkpoint**: All tests pass, UI polished, documentation complete.

---

## Summary

**Total Tasks**: 106  
**Estimated Time**: 17 days  
**Critical Path**: Phases 1-4 (Backend foundation) must complete before frontend work

**Key Deliverables**:
- 35+ backend files (entities, DTOs, commands, queries, controllers, handlers, validators, events, jobs)
- 40+ frontend files (pages, components, types, API integration)
- 15+ test files (unit, integration, component)
- Documentation (quickstart, checklists, API docs)

