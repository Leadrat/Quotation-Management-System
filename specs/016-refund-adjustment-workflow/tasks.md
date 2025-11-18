# Spec-016: Task Breakdown - Refund & Adjustment Workflow

## Phase 1: Database & Entities (13 tasks)

### 1.1 Database Migrations
- [ ] Create migration: `CreateRefundsTable`
- [ ] Create migration: `CreateAdjustmentsTable`
- [ ] Create migration: `CreateRefundTimelineTable`
- [ ] Verify migrations run successfully

### 1.2 Entity Classes
- [ ] Create `Refund.cs` entity class
- [ ] Create `Adjustment.cs` entity class
- [ ] Create `RefundTimeline.cs` entity class

### 1.3 Enums
- [ ] Create `RefundStatus.cs` enum
- [ ] Create `RefundReasonCode.cs` enum
- [ ] Create `AdjustmentType.cs` enum
- [ ] Create `AdjustmentStatus.cs` enum
- [ ] Create `RefundTimelineEventType.cs` enum

### 1.4 Entity Configurations
- [ ] Create `RefundEntityConfiguration.cs`
- [ ] Create `AdjustmentEntityConfiguration.cs`
- [ ] Create `RefundTimelineEntityConfiguration.cs`
- [ ] Update `AppDbContext` and `IAppDbContext`

---

## Phase 2: DTOs & Request Models (10 tasks)

### 2.1 Refund DTOs
- [ ] Create `RefundDto.cs`
- [ ] Create `RefundTimelineDto.cs`
- [ ] Create `RefundMetricsDto.cs`

### 2.2 Refund Request DTOs
- [ ] Create `CreateRefundRequest.cs`
- [ ] Create `UpdateRefundRequest.cs`
- [ ] Create `ApproveRefundRequest.cs`
- [ ] Create `RejectRefundRequest.cs`
- [ ] Create `ReverseRefundRequest.cs`
- [ ] Create `BulkProcessRefundsRequest.cs`

### 2.3 Adjustment DTOs
- [ ] Create `AdjustmentDto.cs`
- [ ] Create `CreateAdjustmentRequest.cs`
- [ ] Create `ApproveAdjustmentRequest.cs`

---

## Phase 3: Payment Gateway Integration (8 tasks)

### 3.1 Interface Updates
- [ ] Verify `IPaymentGatewayService.RefundPaymentAsync` exists
- [ ] Add `GetRefundStatusAsync` method to interface
- [ ] Add `ReverseRefundAsync` method to interface

### 3.2 Stripe Integration
- [ ] Implement refund processing in `StripePaymentGatewayService`
- [ ] Implement refund status checking
- [ ] Handle Stripe refund webhooks

### 3.3 Razorpay Integration
- [ ] Implement refund processing in `RazorpayPaymentGatewayService`
- [ ] Implement refund status checking
- [ ] Handle Razorpay refund webhooks

### 3.4 Webhook Handler
- [ ] Create refund webhook handler in `PaymentWebhookController`

---

## Phase 4: Command Handlers - Refunds (12 tasks)

### 4.1 Initiate Refund
- [ ] Create `InitiateRefundCommand.cs`
- [ ] Create `InitiateRefundCommandHandler.cs`
- [ ] Validate payment and amount
- [ ] Determine approval level
- [ ] Create timeline entry
- [ ] Publish `RefundRequested` event

### 4.2 Approve Refund
- [ ] Create `ApproveRefundCommand.cs`
- [ ] Create `ApproveRefundCommandHandler.cs`
- [ ] Validate permissions
- [ ] Trigger processing
- [ ] Publish `RefundApproved` event

### 4.3 Reject Refund
- [ ] Create `RejectRefundCommand.cs`
- [ ] Create `RejectRefundCommandHandler.cs`
- [ ] Publish `RefundRejected` event

### 4.4 Process Refund
- [ ] Create `ProcessRefundCommand.cs`
- [ ] Create `ProcessRefundCommandHandler.cs`
- [ ] Call gateway API
- [ ] Handle success/failure
- [ ] Publish events

### 4.5 Reverse Refund
- [ ] Create `ReverseRefundCommand.cs`
- [ ] Create `ReverseRefundCommandHandler.cs`
- [ ] Process reversal
- [ ] Publish `RefundReversed` event

### 4.6 Bulk Process
- [ ] Create `BulkProcessRefundsCommand.cs`
- [ ] Create `BulkProcessRefundsCommandHandler.cs`

---

## Phase 5: Command Handlers - Adjustments (8 tasks)

### 5.1 Initiate Adjustment
- [ ] Create `InitiateAdjustmentCommand.cs`
- [ ] Create `InitiateAdjustmentCommandHandler.cs`
- [ ] Calculate tax impact
- [ ] Publish `AdjustmentRequested` event

### 5.2 Approve Adjustment
- [ ] Create `ApproveAdjustmentCommand.cs`
- [ ] Create `ApproveAdjustmentCommandHandler.cs`
- [ ] Publish `AdjustmentApproved` event

### 5.3 Reject Adjustment
- [ ] Create `RejectAdjustmentCommand.cs`
- [ ] Create `RejectAdjustmentCommandHandler.cs`

### 5.4 Apply Adjustment
- [ ] Create `ApplyAdjustmentCommand.cs`
- [ ] Create `ApplyAdjustmentCommandHandler.cs`
- [ ] Update quotation
- [ ] Recalculate taxes
- [ ] Publish `AdjustmentApplied` event

---

## Phase 6: Query Handlers (8 tasks)

- [ ] Create `GetRefundByIdQuery` and handler
- [ ] Create `GetRefundsByPaymentQuery` and handler
- [ ] Create `GetRefundsByQuotationQuery` and handler
- [ ] Create `GetPendingRefundsQuery` and handler
- [ ] Create `GetRefundTimelineQuery` and handler
- [ ] Create `GetRefundMetricsQuery` and handler
- [ ] Create `GetAdjustmentsByQuotationQuery` and handler
- [ ] Create `GetPendingAdjustmentsQuery` and handler

---

## Phase 7: Event Handlers (9 tasks)

- [ ] Create `RefundRequestedEventHandler`
- [ ] Create `RefundApprovedEventHandler`
- [ ] Create `RefundRejectedEventHandler`
- [ ] Create `RefundCompletedEventHandler`
- [ ] Create `RefundFailedEventHandler`
- [ ] Create `RefundReversedEventHandler`
- [ ] Create `AdjustmentRequestedEventHandler`
- [ ] Create `AdjustmentApprovedEventHandler`
- [ ] Create `AdjustmentAppliedEventHandler`

---

## Phase 8: API Controllers (15 tasks)

### 8.1 Refunds Controller
- [ ] POST /api/v1/refunds
- [ ] GET /api/v1/refunds/{refundId}
- [ ] GET /api/v1/payments/{paymentId}/refunds
- [ ] POST /api/v1/refunds/{refundId}/approve
- [ ] POST /api/v1/refunds/{refundId}/reject
- [ ] POST /api/v1/refunds/{refundId}/reverse
- [ ] GET /api/v1/refunds/timeline/{refundId}
- [ ] GET /api/v1/refunds/pending
- [ ] GET /api/v1/refunds/metrics
- [ ] POST /api/v1/refunds/bulk-process

### 8.2 Adjustments Controller
- [ ] POST /api/v1/adjustments
- [ ] GET /api/v1/adjustments/{adjustmentId}
- [ ] POST /api/v1/adjustments/{adjustmentId}/approve
- [ ] POST /api/v1/adjustments/{adjustmentId}/apply
- [ ] GET /api/v1/quotations/{quotationId}/adjustments

---

## Phase 9: Validators & AutoMapper (9 tasks)

### 9.1 Validators
- [ ] Create `CreateRefundRequestValidator`
- [ ] Create `ApproveRefundRequestValidator`
- [ ] Create `RejectRefundRequestValidator`
- [ ] Create `ReverseRefundRequestValidator`
- [ ] Create `CreateAdjustmentRequestValidator`
- [ ] Create `ApproveAdjustmentRequestValidator`

### 9.2 AutoMapper
- [ ] Create `RefundProfile.cs`
- [ ] Create `AdjustmentProfile.cs`
- [ ] Register profiles in `Program.cs`

---

## Phase 10: Frontend - TypeScript Types & API Client (2 tasks)

- [ ] Create `src/types/refunds.ts` with all types
- [ ] Extend `src/lib/api.ts` with `RefundsApi` and `AdjustmentsApi`

---

## Phase 11: Frontend - Refund Components (10 tasks)

- [ ] Create refund request form component
- [ ] Create refund detail page
- [ ] Create refund queue/dashboard
- [ ] Create refund history page
- [ ] Create `RefundStatusBadge` component
- [ ] Create `RefundTimeline` component
- [ ] Create `RefundReasonBadge` component
- [ ] Create `ApprovalDialog` component
- [ ] Create `RefundAmountDisplay` component
- [ ] Create `TimelineEvent` component

---

## Phase 12: Frontend - Adjustment Components (4 tasks)

- [ ] Create adjustment request form
- [ ] Create adjustment timeline component
- [ ] Create adjustment preview component
- [ ] Integrate into quotation detail page

---

## Phase 13: Frontend - Client Portal (3 tasks)

- [ ] Create client refund request form
- [ ] Integrate into client portal
- [ ] Add success/error handling

---

## Phase 14: Frontend - Custom Hooks (6 tasks)

- [ ] Create `useRefund` hook
- [ ] Create `useRefundList` hook
- [ ] Create `useRefundApproval` hook
- [ ] Create `useAdjustment` hook
- [ ] Create `useRefundMetrics` hook
- [ ] Create `useRefundForm` hook

---

## Phase 15: Integration & Testing (15 tasks)

### 15.1 Backend Tests
- [ ] Unit tests for command handlers
- [ ] Unit tests for query handlers
- [ ] Integration tests for refund flow
- [ ] Integration tests for adjustment flow
- [ ] Gateway integration tests (mocked)

### 15.2 Frontend Tests
- [ ] Component tests for refund form
- [ ] Component tests for refund timeline
- [ ] Component tests for approval dialog
- [ ] Hook tests
- [ ] E2E test: Request → Approve → Process
- [ ] E2E test: Adjustment flow

### 15.3 Performance & Security
- [ ] Performance testing
- [ ] Security testing
- [ ] Load testing

---

## Phase 16: Documentation & Deployment (5 tasks)

- [ ] API documentation
- [ ] User guides
- [ ] Deployment scripts
- [ ] Migration verification
- [ ] Monitoring setup

---

## Total Tasks: 141+

**Estimated Timeline**: 6 weeks  
**Team Size**: 2-3 developers (1 backend, 1-2 frontend)

