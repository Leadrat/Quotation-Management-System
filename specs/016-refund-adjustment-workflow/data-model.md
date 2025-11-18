# Spec-016: Data Model - Refund & Adjustment Workflow

## Database Schema

### Refunds

Tracks all refund requests and their processing status.

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
        CHECK ("RefundStatus" IN ('PENDING', 'APPROVED', 'PROCESSING', 'COMPLETED', 'FAILED', 'REVERSED')),
    CONSTRAINT "CK_Refunds_ReasonCode" 
        CHECK ("RefundReasonCode" IN ('CLIENT_REQUEST', 'ERROR', 'DISCOUNT_ADJUSTMENT', 'CANCELLATION', 'DUPLICATE_PAYMENT', 'OTHER'))
);

CREATE INDEX "IX_Refunds_PaymentId" ON "Refunds"("PaymentId");
CREATE INDEX "IX_Refunds_QuotationId" ON "Refunds"("QuotationId");
CREATE INDEX "IX_Refunds_RequestedByUserId" ON "Refunds"("RequestedByUserId");
CREATE INDEX "IX_Refunds_Status" ON "Refunds"("RefundStatus");
CREATE INDEX "IX_Refunds_RequestDate" ON "Refunds"("RequestDate");
CREATE INDEX "IX_Refunds_ApprovedByUserId" ON "Refunds"("ApprovedByUserId");
```

**RefundStatus Values:**
- `PENDING` - Awaiting approval
- `APPROVED` - Approved, ready for processing
- `PROCESSING` - Currently being processed by gateway
- `COMPLETED` - Successfully refunded
- `FAILED` - Processing failed
- `REVERSED` - Refund was reversed

**RefundReasonCode Values:**
- `CLIENT_REQUEST` - Client requested refund
- `ERROR` - System or processing error
- `DISCOUNT_ADJUSTMENT` - Discount adjustment refund
- `CANCELLATION` - Order/service cancellation
- `DUPLICATE_PAYMENT` - Duplicate payment refund
- `OTHER` - Other reason (requires comments)

### Adjustments

Tracks billing adjustments to quotations.

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
        CHECK ("Status" IN ('PENDING', 'APPROVED', 'REJECTED', 'APPLIED')),
    CONSTRAINT "CK_Adjustments_Amount" 
        CHECK ("AdjustedAmount" > 0)
);

CREATE INDEX "IX_Adjustments_QuotationId" ON "Adjustments"("QuotationId");
CREATE INDEX "IX_Adjustments_Status" ON "Adjustments"("Status");
CREATE INDEX "IX_Adjustments_RequestDate" ON "Adjustments"("RequestDate");
CREATE INDEX "IX_Adjustments_RequestedByUserId" ON "Adjustments"("RequestedByUserId");
```

**AdjustmentType Values:**
- `DISCOUNT_CHANGE` - Discount percentage/amount changed
- `AMOUNT_CORRECTION` - Total amount correction
- `TAX_CORRECTION` - Tax amount correction

**AdjustmentStatus Values:**
- `PENDING` - Awaiting approval
- `APPROVED` - Approved, ready to apply
- `REJECTED` - Rejected by approver
- `APPLIED` - Applied to quotation

### RefundTimeline

Audit trail for all refund actions.

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
CREATE INDEX "IX_RefundTimeline_ActedByUserId" ON "RefundTimeline"("ActedByUserId");
```

**EventType Values:**
- `REQUESTED` - Refund request created
- `APPROVED` - Refund approved
- `REJECTED` - Refund rejected
- `PROCESSING` - Refund processing started
- `COMPLETED` - Refund completed successfully
- `FAILED` - Refund processing failed
- `REVERSED` - Refund reversed

## Entity Relationships

```
Payments (1) ──→ (N) Refunds
Quotations (1) ──→ (N) Refunds
Quotations (1) ──→ (N) Adjustments
Users (1) ──→ (N) Refunds (as RequestedBy)
Users (1) ──→ (N) Refunds (as ApprovedBy)
Users (1) ──→ (N) Adjustments (as RequestedBy)
Users (1) ──→ (N) Adjustments (as ApprovedBy)
Refunds (1) ──→ (N) RefundTimeline
Users (1) ──→ (N) RefundTimeline (as ActedBy)
```

## Data Integrity Rules

1. **Refund Amount Validation**: RefundAmount must be > 0 and <= Payment.AmountPaid
2. **Partial Refund Check**: Sum of all refunds for a payment cannot exceed Payment.AmountPaid
3. **Status Transitions**: 
   - PENDING → APPROVED → PROCESSING → COMPLETED/FAILED
   - PENDING → REJECTED
   - COMPLETED → REVERSED
4. **Adjustment Amount**: AdjustedAmount must be different from OriginalAmount
5. **Timeline Consistency**: Every refund status change must have a corresponding timeline entry

## Audit Trail Requirements

- All refund/adjustment actions must be logged in RefundTimeline
- IP addresses captured for security audit
- Comments required for rejections and reversals
- Original values preserved (OriginalAmount in Adjustments)
- Timestamps for all state changes

## Performance Considerations

- Indexes on frequently queried columns (Status, RequestDate, PaymentId, QuotationId)
- Composite indexes for common query patterns (Status + RequestDate)
- Partitioning consideration for RefundTimeline (by year/month if volume is high)

