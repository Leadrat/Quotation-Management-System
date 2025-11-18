# Spec-012: Quotation Discount Approval Workflow (Manager & Admin)

## Overview

This spec introduces a robust discount approval workflow for quotations. Large discounts must be reviewed and approved by the Sales Manager (or Admin) based on configurable thresholds (e.g., 10–20%: manager; >20%: admin). The workflow covers submitting discounts requiring approval, automatic locking of the quotation, escalation processes, reasons/comments for all actions, audit logging, and real-time notifications. This ensures compliance, reduces risk of unauthorized offers, and keeps management informed.

## Project Information

- **PROJECT_NAME**: CRM Quotation Management System
- **SPEC_NUMBER**: Spec-012
- **SPEC_NAME**: Quotation Discount Approval Workflow (Manager & Admin)
- **GROUP**: Quotation Management (Group 3 of 11)
- **PRIORITY**: CRITICAL (Phase 1, after Spec 11)
- **DEPENDENCIES**: Spec-009 (QuotationEntity), Spec-010 (QuotationManagement), Spec-011 (QuotationTemplateManagement), Spec-004 (RBAC)
- **RELATED_SPECS**: Spec-013 (NotificationSystem), Spec-014 (PaymentProcessing)

## Key Features

- Configurable discount approval thresholds (by role and percentage)
- Submit quotation for discount approval (locks editing until approved/rejected)
- Real-time workflow: "Pending Approval" → "Approved" → "Rejected" with reasons
- Approval escalation path (e.g., if manager exceeds threshold, admins notified)
- Audit trail for all steps (who requested, approved, rejected, comments, timestamps)
- Notification/alert at each stage (sales rep, manager/admin, and optionally client)
- Comment requirement on approval/rejection (reason field, mandatory)
- Pending approval queue for each approver
- Approval status and timeline visible in quotation detail view
- Dashboard for managers/admins to review, approve, or reject pending quotations in batch or individually
- Uneditable while pending approval (quotations "locked" until decision)
- Activity metrics: approval TAT, rejection rate, average discount %
- Option for re-submit after rejection (with reason/history tracked)
- Bulk approval for low-value requests (manager discretion, via batch UI)

## JTBD Alignment

### Persona: Sales Rep
**JTBD**: "I want to offer discounts to clients but follow company approval processes"  
**Success Metric**: "Quick response to my discount requests, and clear communication"

### Persona: Manager/Admin (Approver)
**JTBD**: "I want to review, approve, or reject discounts efficiently and ensure compliance"  
**Success Metric**: "No discounts processed without my sign-off, easy workload management"

## Business Value

- Controls unnecessary or risky discounting
- Maintains margin and compliance with approval policy
- Enables transparency (audit who and why for each action)
- Lowers risk (prevents unauthorized quotations/issues)
- Builds trust (client can be shown approval notes if needed)
- Provides analytics on discount trends, team behaviors

## Database Schema

### Table: DiscountApprovals

**Purpose**: Track all discount approval requests for quotations.

**Columns (16 total)**:
- `ApprovalId` (UUID, PK, NOT NULL)
- `QuotationId` (UUID, FK → Quotations, NOT NULL)
- `RequestedByUserId` (UUID, FK → Users, NOT NULL)
- `ApproverUserId` (UUID, FK → Users, NULLABLE if manager auto-assigned later)
- `Status` (VARCHAR(50): "PENDING", "APPROVED", "REJECTED", NOT NULL)
- `RequestDate` (TIMESTAMPTZ, NOT NULL)
- `ApprovalDate` (TIMESTAMPTZ, NULLABLE)
- `RejectionDate` (TIMESTAMPTZ, NULLABLE)
- `CurrentDiscountPercentage` (DECIMAL(5,2), NOT NULL)
- `Threshold` (DECIMAL(5,2), NOT NULL)
- `ApprovalLevel` (VARCHAR(50): "Manager", "Admin", NOT NULL)
- `Reason` (TEXT, NOT NULL) // Required for both approval and rejection
- `Comments` (TEXT, NULLABLE)
- `UpdatedAt` (TIMESTAMPTZ, NOT NULL)
- `CreatedAt` (TIMESTAMPTZ, NOT NULL)
- `EscalatedToAdmin` (BOOLEAN, NOT NULL, DEFAULT FALSE)

**Indexes**:
- INDEX(`ApproverUserId`, `Status`)
- INDEX(`QuotationId`)
- INDEX(`RequestedByUserId`)
- INDEX(`CurrentDiscountPercentage`)
- INDEX(`Status`)
- INDEX(`CreatedAt`, `Status`)

## C# Domain Entities

### DiscountApproval.cs

```csharp
namespace CRM.Domain.Entities
{
    public class DiscountApproval
    {
        public Guid ApprovalId { get; set; }
        public Guid QuotationId { get; set; }
        public Guid RequestedByUserId { get; set; }
        public Guid? ApproverUserId { get; set; }
        public ApprovalStatus Status { get; set; }
        public DateTimeOffset RequestDate { get; set; }
        public DateTimeOffset? ApprovalDate { get; set; }
        public DateTimeOffset? RejectionDate { get; set; }
        public decimal CurrentDiscountPercentage { get; set; }
        public decimal Threshold { get; set; }
        public ApprovalLevel ApprovalLevel { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Comments { get; set; }
        public bool EscalatedToAdmin { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        // Navigation properties
        public virtual Quotation Quotation { get; set; } = null!;
        public virtual User RequestedByUser { get; set; } = null!;
        public virtual User? ApproverUser { get; set; }
    }
}
```

### ApprovalStatus Enum

```csharp
namespace CRM.Domain.Enums
{
    public enum ApprovalStatus
    {
        Pending,
        Approved,
        Rejected
    }
}
```

### ApprovalLevel Enum

```csharp
namespace CRM.Domain.Enums
{
    public enum ApprovalLevel
    {
        Manager,
        Admin
    }
}
```

## DTOs

### DiscountApprovalDto
- ApprovalId, QuotationId, RequestedByUserId, ApproverUserId, Status, Reason, Comments, ApprovalLevel, RequestDate, ApprovalDate, RejectionDate, EscalatedToAdmin, QuotationNumber, ClientName, RequestedByUserName, ApproverUserName

### CreateDiscountApprovalRequest
- QuotationId, DiscountPercentage, Reason, Comments

### ApproveDiscountApprovalRequest
- Reason (required), Comments

### RejectDiscountApprovalRequest
- Reason (required), Comments

### ResubmitDiscountApprovalRequest
- Reason (required), Comments

## API Endpoints

1. **POST** `/api/v1/discount-approvals/request` - Submit quotation for discount approval
2. **GET** `/api/v1/discount-approvals/pending` - Get pending approvals (filterable by approver/status/discount %/date/user)
3. **POST** `/api/v1/discount-approvals/{approvalId}/approve` - Approve discount request
4. **POST** `/api/v1/discount-approvals/{approvalId}/reject` - Reject discount request
5. **GET** `/api/v1/discount-approvals/{approvalId}` - Get approval detail and history
6. **POST** `/api/v1/discount-approvals/{approvalId}/escalate` - Escalate to admin (optional)
7. **GET** `/api/v1/quotations/{quotationId}/approvals` - Get approval history for a quotation
8. **GET** `/api/v1/discount-approvals/reports` - Get approval metrics and analytics (admin)
9. **GET** `/api/v1/discount-approvals/timeline` - Get approval timeline for audit/reporting
10. **POST** `/api/v1/discount-approvals/{approvalId}/resubmit` - Resubmit after rejection
11. **POST** `/api/v1/discount-approvals/bulk-approve` - Bulk approve multiple requests

## Business Logic

- **Manager Approval**: Default threshold 10–20%
- **Admin Approval**: >20%
- Configurable by admin (exposed via system settings, not MVP)
- **Bulk-approve**: Any manager may approve multiple low% discounts in one UI action
- **Auto-escalation**: If manager does not act in N hours (e.g., 24h), system escalates to admin
- **Quotation Locking**: Quotation becomes read-only while approval is pending
- **Email/SMS/real-time notification** on every step

## Frontend UI Components

### Sales Rep Pages

**SR-P26: Quotation Approval Submission Dialog**
- Location: Quotation create/edit form, when discount >= threshold
- Modal dialog opens before save
- Form: Reason (textarea, required), Optional Comments
- Confirm button triggers submit API call, disables form on submit
- Shows "Awaiting Manager/Admin approval" banner with status if pending
- UI disables all form fields while locked
- Error/Success feedback, auto-scroll to status

**SR-P27: Quotation List with Approval Status**
- Approval status column: "Pending/Mgr Approved/Admin Approved/Rejected"
- Filter: Show only "Quotations pending my approval"
- Status badge color-coded

**SR-P28: Quotation Detail Approval Timeline**
- New section in `/dashboard/quotations/{id}/view`
- Vertical timeline: Request → Pending → Approved/Rejected
- Each entry: Who (name/role), when, reason/comments, icon
- Comments expandable, full audit trail

### Manager/Admin Pages

**A-P19: Discount Approval Dashboard**
- Location: `/admin/approvals`, `/manager/approvals`
- Tabs: "Pending Approvals", "Approved", "Rejected", "All"
- Table: Quotation #, Client, Discount %, SalesRep, Reason, Status, Date, Actions
- Bulk select & approve/reject (with single reason/comment for all)
- Card: Stats (pending count, average approval time, rejection %, escalations)
- Row actions: "Approve", "Reject", "Escalate"
- Expand row: View full quotation (in modal), comments, history
- Filters: By sales rep, client, date, discount %, approval level
- Toast/notification for every action

**A-P20: Approval Reports/Trends** (optional)
- Charts/graphs on discounts approved, rejection rates, etc.

### Shared/Reusable Components

- `ApprovalStatusBadge` - Shows status
- `ApprovalTimeline` - Vertical timeline/history
- `ApprovalDecisionModal` - Manager/Admin decision: approve/reject, reason required
- `ApprovalCommentsPanel` - View all comments/reasons chronologically
- `LockedFormOverlay` - Shows lock overlay when pending

## Domain Events

- `DiscountApprovalRequested`
- `DiscountApprovalApproved`
- `DiscountApprovalRejected`
- `DiscountApprovalEscalated`
- `DiscountApprovalResubmitted`

Audit log/notification/metrics handlers for each. All actions timestamped and user-identified.

## Test Cases

### Backend Tests
1. Submission for >10% discount triggers approval flow, quotation locked
2. Pending approval disables edit/delete on quotation
3. Manager approves: quotation unlocks, approved badge/status in list/tracker
4. Manager rejects: sales rep sees rejection + comment, can resubmit
5. Admin escalation for >20% or if manager delay
6. Approval timeline shows all actions with comments
7. Bulk approval works (with reason for all selected)
8. Notifications/emails sent at each step
9. Sales rep cannot bypass approval (API and UI lock enforced)
10. All audit events logged with correct user/time/reason/comment
11. Approvers can filter/sort pending queue by date, rep, % etc.
12. Resubmission after rejection works, prior reason preserved

### Frontend Tests
13. Mobile friendly tables and modals
14. UI tests: status overlays, banners, toast work as expected
15. Data privacy: only relevant approvers see each request

## Security Requirements

- Quotation locked from edits during pending approval (set flag; UI disables editing and delete)
- Only assigned manager/admin can approve/reject/escalate
- All actions create audit log entries
- Comments and reasons required for all actions

## Acceptance Criteria

✅ Every approval workflow step is visible + actionable in the UI  
✅ Approvers can review, approve, reject, comment, escalate from dashboard  
✅ Sales rep cannot bypass approval (no create/update if pending)  
✅ All backend audit, events, notifications correct  
✅ All frontend banners, badges, modals display status, block edits as required  
✅ All actions (approve/reject) require reason, tracked in audit comments  
✅ Timeline visible in quotation detail page  
✅ Accessible, responsive UI

## Deliverables

### Backend (35+ files)
- Entities, DTOs, queries, commands, events, background escalation, validators, API controllers, audit/metrics event handlers, migration scripts, notification/email templates

### Backend Tests (15+)
- Unit, integration, and event tests

### Frontend (40+ files)
- All pages/components/hooks/types for approval flows, badges, modals, banners, E2E flows, context/providers as needed, TailAdmin integration

### Frontend Tests (20+)
- Jest/RTL for components/views, E2E test for full approval lifecycle, notification states, mobile rendering

