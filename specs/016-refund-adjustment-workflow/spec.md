# Spec-016: Refund & Adjustment Workflow

**Project**: CRM Quotation Management System  
**Spec Number**: Spec-016  
**Spec Name**: Refund & Adjustment Workflow  
**Group**: Payment & Finance (Group 5 of 11)  
**Priority**: HIGH (Phase 2, after Payment Processing & Reporting)  
**Status**: 📋 Specification Complete

## Dependencies

- **Spec-014**: Payment Processing & Integration (Required)
- **Spec-015**: Reporting, Analytics & Business Intelligence (Required)
- **Spec-013**: Real-time Notification System (Required)

## Related Specs

- **Spec-017**: Advanced Features
- **Spec-018**: System Administration

---

## Overview

This specification defines the refund and adjustment workflow for paid quotations. It covers partial and full refunds initiated by clients or sales/finance teams, adjustment workflows for discounts/corrections, reversal processes, and approval chains. All refunds and adjustments are tracked, audited, and integrated with the payment gateway. The workflow includes notifications, timeline tracking, reason documentation, and comprehensive reporting on refunds and adjustments.

## Key Features

- Initiate full or partial refunds on paid quotations
- Refund reason selection (client request, error, discount adjustment, cancellation, etc.)
- Approval workflow for refunds (by manager/admin based on amount threshold)
- Refund status tracking (pending, approved, processing, completed, failed, reversed)
- Automatic gateway integration (Stripe, Razorpay, PayPal refund API calls)
- Adjustment workflow for billing corrections (override discount, correct amount, etc.)
- Reversal of refunds (if incorrectly processed)
- Refund timeline and history with all actions and comments
- Notifications for refund requests, approvals, processing (Spec 13 integration)
- Finance dashboard for pending refunds and adjustments
- Refund metrics and reporting (refund %, reasons, TAT)
- Audit trail for compliance (who, when, why, amount)
- Client-initiated refund requests (from client portal)
- Bulk refund processing (finance)
- Tax recalculation on adjustments (GST impact)
- Payment gateway reconciliation after refunds
- Reversals and dispute handling

---

## JTBD Alignment

### Persona: Client
**Job to be Done**: "I want an easy process to request a refund if needed"  
**Success Metric**: "Refund requested and processed within 48 hours"

### Persona: Finance/Admin
**Job to be Done**: "I need to manage refunds, track reasons, ensure compliance, and reconcile with payments"  
**Success Metric**: "All refunds accounted for, audited, and reconciled; no discrepancies"

---

## Business Value

- Streamlines refund process, improves customer satisfaction
- Reduces financial disputes and chargebacks
- Enables accurate financial reporting (refunds tracked separately)
- Provides audit trail for compliance and finance reviews
- Supports business agility (quick adjustments to quotations/discounts)
- Prevents revenue leakage through proper approval workflows

---

## Database Schema

### Refunds Table

```sql
CREATE TABLE "Refunds" (
    "RefundId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "PaymentId" UUID NOT NULL,
    "QuotationId" UUID NOT NULL,
    "RefundAmount" DECIMAL(12,2) NOT NULL,
    "RefundReason" VARCHAR(500) NOT NULL,
    "RefundReasonCode" VARCHAR(50) NOT NULL,
    "RequestedByUserId" UUID NOT NULL,
    "ApprovedByUserId" UUID NULL,
    "RefundStatus" VARCHAR(50) NOT NULL,
    "PaymentGatewayReference" VARCHAR(255) NULL,
    "ApprovalLevel" VARCHAR(50) NULL,
    "Comments" TEXT NULL,
    "FailureReason" TEXT NULL,
    "RequestDate" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "ApprovalDate" TIMESTAMPTZ NULL,
    "CompletedDate" TIMESTAMPTZ NULL,
    "ReversedDate" TIMESTAMPTZ NULL,
    "ReversedReason" VARCHAR(500) NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT "FK_Refunds_Payment" 
        FOREIGN KEY ("PaymentId") REFERENCES "Payments"("PaymentId") ON DELETE CASCADE,
    CONSTRAINT "FK_Refunds_Quotation" 
        FOREIGN KEY ("QuotationId") REFERENCES "Quotations"("QuotationId") ON DELETE CASCADE,
    CONSTRAINT "FK_Refunds_RequestedBy" 
        FOREIGN KEY ("RequestedByUserId") REFERENCES "Users"("UserId") ON DELETE CASCADE,
    CONSTRAINT "FK_Refunds_ApprovedBy" 
        FOREIGN KEY ("ApprovedByUserId") REFERENCES "Users"("UserId") ON DELETE SET NULL,
    CONSTRAINT "CK_Refunds_Amount" 
        CHECK ("RefundAmount" > 0),
    CONSTRAINT "CK_Refunds_Status" 
        CHECK ("RefundStatus" IN ('PENDING', 'APPROVED', 'PROCESSING', 'COMPLETED', 'FAILED', 'REVERSED'))
);

CREATE INDEX "IX_Refunds_PaymentId" ON "Refunds"("PaymentId");
CREATE INDEX "IX_Refunds_QuotationId" ON "Refunds"("QuotationId");
CREATE INDEX "IX_Refunds_RequestedByUserId" ON "Refunds"("RequestedByUserId");
CREATE INDEX "IX_Refunds_Status" ON "Refunds"("RefundStatus");
CREATE INDEX "IX_Refunds_RequestDate" ON "Refunds"("RequestDate");
```

### Adjustments Table

```sql
CREATE TABLE "Adjustments" (
    "AdjustmentId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "QuotationId" UUID NOT NULL,
    "AdjustmentType" VARCHAR(50) NOT NULL,
    "OriginalAmount" DECIMAL(12,2) NOT NULL,
    "AdjustedAmount" DECIMAL(12,2) NOT NULL,
    "Reason" VARCHAR(500) NOT NULL,
    "RequestedByUserId" UUID NOT NULL,
    "ApprovedByUserId" UUID NULL,
    "Status" VARCHAR(50) NOT NULL,
    "ApprovalLevel" VARCHAR(50) NULL,
    "RequestDate" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "ApprovalDate" TIMESTAMPTZ NULL,
    "AppliedDate" TIMESTAMPTZ NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT "FK_Adjustments_Quotation" 
        FOREIGN KEY ("QuotationId") REFERENCES "Quotations"("QuotationId") ON DELETE CASCADE,
    CONSTRAINT "FK_Adjustments_RequestedBy" 
        FOREIGN KEY ("RequestedByUserId") REFERENCES "Users"("UserId") ON DELETE CASCADE,
    CONSTRAINT "FK_Adjustments_ApprovedBy" 
        FOREIGN KEY ("ApprovedByUserId") REFERENCES "Users"("UserId") ON DELETE SET NULL,
    CONSTRAINT "CK_Adjustments_Type" 
        CHECK ("AdjustmentType" IN ('DISCOUNT_CHANGE', 'AMOUNT_CORRECTION', 'TAX_CORRECTION')),
    CONSTRAINT "CK_Adjustments_Status" 
        CHECK ("Status" IN ('PENDING', 'APPROVED', 'REJECTED', 'APPLIED'))
);

CREATE INDEX "IX_Adjustments_QuotationId" ON "Adjustments"("QuotationId");
CREATE INDEX "IX_Adjustments_Status" ON "Adjustments"("Status");
CREATE INDEX "IX_Adjustments_RequestDate" ON "Adjustments"("RequestDate");
```

### RefundTimeline Table

```sql
CREATE TABLE "RefundTimeline" (
    "TimelineId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "RefundId" UUID NOT NULL,
    "EventType" VARCHAR(50) NOT NULL,
    "ActedByUserId" UUID NOT NULL,
    "Comments" TEXT NULL,
    "EventDate" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "IpAddress" VARCHAR(50) NULL,
    
    CONSTRAINT "FK_RefundTimeline_Refund" 
        FOREIGN KEY ("RefundId") REFERENCES "Refunds"("RefundId") ON DELETE CASCADE,
    CONSTRAINT "FK_RefundTimeline_ActedBy" 
        FOREIGN KEY ("ActedByUserId") REFERENCES "Users"("UserId") ON DELETE CASCADE,
    CONSTRAINT "CK_RefundTimeline_EventType" 
        CHECK ("EventType" IN ('REQUESTED', 'APPROVED', 'REJECTED', 'PROCESSING', 'COMPLETED', 'FAILED', 'REVERSED'))
);

CREATE INDEX "IX_RefundTimeline_RefundId" ON "RefundTimeline"("RefundId");
CREATE INDEX "IX_RefundTimeline_EventDate" ON "RefundTimeline"("EventDate");
```

---

## Domain Entities

### Refund.cs
- RefundId (Guid, PK)
- PaymentId (Guid, FK)
- QuotationId (Guid, FK)
- RefundAmount (decimal)
- RefundReason (string)
- RefundReasonCode (string)
- RequestedByUserId (Guid, FK)
- ApprovedByUserId (Guid?, FK)
- RefundStatus (enum)
- PaymentGatewayReference (string?)
- ApprovalLevel (string?)
- Comments (string?)
- FailureReason (string?)
- RequestDate (DateTimeOffset)
- ApprovalDate (DateTimeOffset?)
- CompletedDate (DateTimeOffset?)
- ReversedDate (DateTimeOffset?)
- ReversedReason (string?)
- CreatedAt (DateTimeOffset)
- UpdatedAt (DateTimeOffset)

### Adjustment.cs
- AdjustmentId (Guid, PK)
- QuotationId (Guid, FK)
- AdjustmentType (enum)
- OriginalAmount (decimal)
- AdjustedAmount (decimal)
- Reason (string)
- RequestedByUserId (Guid, FK)
- ApprovedByUserId (Guid?, FK)
- Status (enum)
- ApprovalLevel (string?)
- RequestDate (DateTimeOffset)
- ApprovalDate (DateTimeOffset?)
- AppliedDate (DateTimeOffset?)
- CreatedAt (DateTimeOffset)
- UpdatedAt (DateTimeOffset)

### RefundTimeline.cs
- TimelineId (Guid, PK)
- RefundId (Guid, FK)
- EventType (enum)
- ActedByUserId (Guid, FK)
- Comments (string?)
- EventDate (DateTimeOffset)
- IpAddress (string?)

---

## Enums

### RefundStatus
- Pending
- Approved
- Processing
- Completed
- Failed
- Reversed

### RefundReasonCode
- CLIENT_REQUEST
- ERROR
- DISCOUNT_ADJUSTMENT
- CANCELLATION
- DUPLICATE_PAYMENT
- OTHER

### AdjustmentType
- DISCOUNT_CHANGE
- AMOUNT_CORRECTION
- TAX_CORRECTION

### AdjustmentStatus
- PENDING
- APPROVED
- REJECTED
- APPLIED

### RefundTimelineEventType
- REQUESTED
- APPROVED
- REJECTED
- PROCESSING
- COMPLETED
- FAILED
- REVERSED

---

## API Endpoints

### Refund Endpoints

1. `POST /api/v1/refunds` - Initiate refund request
2. `GET /api/v1/refunds/{refundId}` - Get refund detail
3. `GET /api/v1/payments/{paymentId}/refunds` - Get all refunds for payment
4. `POST /api/v1/refunds/{refundId}/approve` - Approve refund
5. `POST /api/v1/refunds/{refundId}/reject` - Reject refund
6. `POST /api/v1/refunds/{refundId}/reverse` - Reverse completed refund
7. `GET /api/v1/refunds/timeline/{refundId}` - Get refund timeline/history
8. `GET /api/v1/refunds/pending` - Get all pending refunds for approver
9. `GET /api/v1/refunds/metrics` - Get refund analytics
10. `POST /api/v1/refunds/bulk-process` - Bulk process refunds (finance)

### Adjustment Endpoints

11. `POST /api/v1/adjustments` - Initiate adjustment request
12. `GET /api/v1/adjustments/{adjustmentId}` - Get adjustment detail
13. `POST /api/v1/adjustments/{adjustmentId}/approve` - Approve adjustment
14. `POST /api/v1/adjustments/{adjustmentId}/apply` - Apply adjustment to quotation
15. `GET /api/v1/quotations/{quotationId}/adjustments` - Get all adjustments for quotation

**Authorization**: All endpoints require authentication. Approval endpoints require manager/admin role.

---

## Domain Events

- `RefundRequested` - When refund request is created
- `RefundApproved` - When refund is approved
- `RefundRejected` - When refund is rejected
- `RefundProcessing` - When refund processing starts
- `RefundCompleted` - When refund is successfully completed
- `RefundFailed` - When refund processing fails
- `RefundReversed` - When refund is reversed
- `AdjustmentRequested` - When adjustment request is created
- `AdjustmentApproved` - When adjustment is approved
- `AdjustmentApplied` - When adjustment is applied to quotation

All events trigger notifications (Spec 13), audit logging, and financial reconciliation.

---

## Frontend Components

### SalesRep/Finance Pages

**FR-P01: Refund Request Form** (Modal/Page)
- Payment/Quotation selector
- Refund amount input (max = payment amount)
- Refund reason dropdown
- Comments textarea
- Submit/Cancel buttons
- Validation and error handling

**FR-P02: Refund Detail Page** (`/refunds/{refundId}`)
- Header with quotation number, amounts, status badge
- Timeline section (vertical timeline)
- Details panel
- Actions based on status

**FR-P03: Refund Queue/Dashboard** (`/refunds/pending`)
- List/table of pending refunds
- Filters and sorting
- Bulk actions
- Approval dialog

**FR-P04: Refund History & Reporting** (`/refunds/history`)
- Filter/search functionality
- Table with export options
- Metrics cards
- Charts for refund reasons

### Adjustment Pages

**FR-P05: Adjustment Request Form** (Modal/Page)
- Adjustment type dropdown
- Original/adjusted amount inputs
- Reason textarea
- Preview of new totals
- Tax recalculation preview

**FR-P06: Adjustment Timeline & Status** (Quotation Detail)
- Adjustment requests display
- Status tracking
- Revert functionality

### Client Portal Pages

**CP-P04: Client Refund Request** (Client Portal)
- Simple refund request form
- Reason dropdown
- Message to finance team
- Success/error handling

### Shared Components

- RefundStatusBadge
- RefundTimeline
- RefundReasonBadge
- ApprovalDialog
- RefundAmountDisplay
- TimelineEvent
- RefundMetricsCard
- AdjustmentPreview

---

## Acceptance Criteria

### Backend
✅ Full and partial refunds initiated, approved, and processed correctly  
✅ Refunds integrated with payment gateways (Stripe, Razorpay, etc.)  
✅ Approval routing by amount/role (manager/admin) enforced  
✅ Reversals work correctly, re-credit accounts  
✅ Adjustments recalculate taxes accurately  
✅ All events logged with timestamps, users, IPs (audit trail)  
✅ Notifications sent at each step  
✅ Reconciliation accurate (payment gateway matches records)  
✅ Client-initiated requests work via portal

### Frontend
✅ Refund request form accessible, validates input  
✅ Refund queue shows pending items, approval intuitive  
✅ Refund detail page shows full timeline and history  
✅ Adjustment form calculates previews correctly  
✅ Client portal refund form simple and functional  
✅ All pages mobile responsive, accessible  
✅ Real-time status updates work  
✅ Error handling graceful and user-friendly

### Integration
✅ Backend and frontend built in parallel  
✅ API and UI seamlessly integrated  
✅ No "backend only" features (all exposed in UI)  
✅ Notifications integrated (Spec 13)  
✅ Payment gateway callbacks processed correctly  
✅ Audit trail complete and queryable  
✅ All test cases pass (unit, integration, E2E)  
✅ Backend coverage ≥85%, Frontend coverage ≥80%

---

## Implementation Notes

### Payment Gateway Integration
- Use Stripe Refund API, Razorpay refund endpoints, PayPal refund APIs
- Handle async callbacks (webhook)
- Retry failed refunds (exponential backoff)
- Idempotent operations (prevent duplicates)

### Approval Workflow
- Configurable thresholds (e.g., <5000 auto-approve, 5000-50000 manager, >50000 admin)
- Escalation if approver offline (timeout → next level)
- Concurrent approvals (if multiple qualified, round-robin)

### Tax Recalculation
- On adjustment, recalculate CGST/SGST/IGST based on new amount
- Update quotation totals
- Show preview before applying
- Audit original vs. adjusted tax

### Performance
- Lazy-load refund lists (pagination)
- Cache refund metrics (daily snapshot)
- Async processing for gateway calls (queue)

### Security
- Encrypt sensitive payment data
- HTTPS only
- Rate limit refund requests (prevent abuse)
- Verify requester identity (JWT)
- Audit all refund/adjustment actions

---

**End of Spec-016: Refund & Adjustment Workflow**

