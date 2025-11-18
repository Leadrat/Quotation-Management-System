# Spec-016: Implementation Plan - Refund & Adjustment Workflow

## Overview

This plan outlines the phased implementation of the refund and adjustment workflow system. The implementation will be done in parallel for backend and frontend to ensure seamless integration.

## Implementation Phases

### Phase 1: Database & Entities (Week 1)
**Goal**: Set up database schema and domain entities

**Tasks**:
1. Create database migrations for:
   - `Refunds` table
   - `Adjustments` table
   - `RefundTimeline` table
2. Create C# entity classes:
   - `Refund.cs`
   - `Adjustment.cs`
   - `RefundTimeline.cs`
3. Create enums:
   - `RefundStatus` enum
   - `RefundReasonCode` enum
   - `AdjustmentType` enum
   - `AdjustmentStatus` enum
   - `RefundTimelineEventType` enum
4. Configure Entity Framework mappings
5. Update `AppDbContext` and `IAppDbContext`
6. Run migrations and verify schema

**Deliverables**:
- 3 database tables created
- 3 entity classes
- 5 enum definitions
- EF Core configurations

---

### Phase 2: DTOs & Request Models (Week 1)
**Goal**: Define all data transfer objects and request models

**Tasks**:
1. Create DTOs:
   - `RefundDto.cs`
   - `AdjustmentDto.cs`
   - `RefundTimelineDto.cs`
   - `RefundMetricsDto.cs`
2. Create request DTOs:
   - `CreateRefundRequest.cs`
   - `UpdateRefundRequest.cs`
   - `ApproveRefundRequest.cs`
   - `RejectRefundRequest.cs`
   - `ReverseRefundRequest.cs`
   - `CreateAdjustmentRequest.cs`
   - `ApproveAdjustmentRequest.cs`
   - `BulkProcessRefundsRequest.cs`
3. Create response DTOs for all endpoints

**Deliverables**:
- 10+ DTO classes
- Request/Response models for all endpoints

---

### Phase 3: Payment Gateway Integration (Week 2)
**Goal**: Integrate refund APIs with payment gateways

**Tasks**:
1. Extend `IPaymentGatewayService` interface:
   - Add `RefundPaymentAsync` method (already exists, verify implementation)
   - Add `GetRefundStatusAsync` method
   - Add `ReverseRefundAsync` method
2. Update `StripePaymentGatewayService`:
   - Implement refund processing
   - Handle refund webhooks
   - Implement refund status checking
3. Update `RazorpayPaymentGatewayService`:
   - Implement refund processing
   - Handle refund webhooks
   - Implement refund status checking
4. Create refund webhook handler
5. Test gateway integrations

**Deliverables**:
- Gateway refund methods implemented
- Webhook handlers for refunds
- Integration tests

---

### Phase 4: Command Handlers - Refunds (Week 2)
**Goal**: Implement command handlers for refund operations

**Tasks**:
1. `InitiateRefundCommandHandler`:
   - Validate payment exists and is refundable
   - Check refund amount limits
   - Determine approval level based on amount
   - Create refund record
   - Lock payment if threshold met
   - Create timeline entry
   - Publish `RefundRequested` event
2. `ApproveRefundCommandHandler`:
   - Validate approver permissions
   - Update refund status
   - Create timeline entry
   - Trigger refund processing
   - Publish `RefundApproved` event
3. `RejectRefundCommandHandler`:
   - Validate approver permissions
   - Update refund status
   - Create timeline entry
   - Publish `RefundRejected` event
4. `ProcessRefundCommandHandler`:
   - Call payment gateway refund API
   - Update refund status
   - Store gateway reference
   - Create timeline entry
   - Publish `RefundProcessing` / `RefundCompleted` / `RefundFailed` events
5. `ReverseRefundCommandHandler`:
   - Validate refund can be reversed
   - Process reversal via gateway
   - Update refund status
   - Create timeline entry
   - Publish `RefundReversed` event
6. `BulkProcessRefundsCommandHandler`:
   - Process multiple refunds
   - Handle partial failures
   - Return results summary

**Deliverables**:
- 6 command handlers
- Event publishing
- Validation logic

---

### Phase 5: Command Handlers - Adjustments (Week 2)
**Goal**: Implement command handlers for adjustment operations

**Tasks**:
1. `InitiateAdjustmentCommandHandler`:
   - Validate quotation exists
   - Calculate tax impact
   - Determine approval level
   - Create adjustment record
   - Publish `AdjustmentRequested` event
2. `ApproveAdjustmentCommandHandler`:
   - Validate approver permissions
   - Update adjustment status
   - Publish `AdjustmentApproved` event
3. `RejectAdjustmentCommandHandler`:
   - Validate approver permissions
   - Update adjustment status
4. `ApplyAdjustmentCommandHandler`:
   - Update quotation amounts
   - Recalculate taxes
   - Update payment if needed
   - Update adjustment status
   - Publish `AdjustmentApplied` event

**Deliverables**:
- 4 command handlers
- Tax recalculation logic
- Quotation update logic

---

### Phase 6: Query Handlers (Week 3)
**Goal**: Implement query handlers for retrieving refund and adjustment data

**Tasks**:
1. `GetRefundByIdQueryHandler`
2. `GetRefundsByPaymentQueryHandler`
3. `GetRefundsByQuotationQueryHandler`
4. `GetPendingRefundsQueryHandler`
5. `GetRefundTimelineQueryHandler`
6. `GetRefundMetricsQueryHandler`
7. `GetAdjustmentsByQuotationQueryHandler`
8. `GetPendingAdjustmentsQueryHandler`

**Deliverables**:
- 8 query handlers
- Metrics calculations

---

### Phase 7: Event Handlers (Week 3)
**Goal**: Implement event handlers for notifications and audit

**Tasks**:
1. `RefundRequestedEventHandler` - Send notification to approver
2. `RefundApprovedEventHandler` - Send notification to requester
3. `RefundRejectedEventHandler` - Send notification to requester
4. `RefundCompletedEventHandler` - Send notification, update payment
5. `RefundFailedEventHandler` - Send notification, log error
6. `RefundReversedEventHandler` - Send notification, update records
7. `AdjustmentRequestedEventHandler` - Send notification to approver
8. `AdjustmentApprovedEventHandler` - Send notification to requester
9. `AdjustmentAppliedEventHandler` - Send notification, update quotation

**Deliverables**:
- 9 event handlers
- Notification integration (Spec 13)
- Audit logging

---

### Phase 8: API Controllers (Week 3)
**Goal**: Create REST API endpoints

**Tasks**:
1. Create `RefundsController`:
   - POST /api/v1/refunds
   - GET /api/v1/refunds/{refundId}
   - GET /api/v1/payments/{paymentId}/refunds
   - POST /api/v1/refunds/{refundId}/approve
   - POST /api/v1/refunds/{refundId}/reject
   - POST /api/v1/refunds/{refundId}/reverse
   - GET /api/v1/refunds/timeline/{refundId}
   - GET /api/v1/refunds/pending
   - GET /api/v1/refunds/metrics
   - POST /api/v1/refunds/bulk-process
2. Create `AdjustmentsController`:
   - POST /api/v1/adjustments
   - GET /api/v1/adjustments/{adjustmentId}
   - POST /api/v1/adjustments/{adjustmentId}/approve
   - POST /api/v1/adjustments/{adjustmentId}/apply
   - GET /api/v1/quotations/{quotationId}/adjustments
3. Create `RefundWebhookController` for gateway callbacks

**Deliverables**:
- 3 API controllers
- 15+ endpoints
- Authorization and validation

---

### Phase 9: Validators & AutoMapper (Week 3)
**Goal**: Create validators and mapping profiles

**Tasks**:
1. Create validators for all requests:
   - `CreateRefundRequestValidator`
   - `ApproveRefundRequestValidator`
   - `RejectRefundRequestValidator`
   - `ReverseRefundRequestValidator`
   - `CreateAdjustmentRequestValidator`
   - `ApproveAdjustmentRequestValidator`
2. Create AutoMapper profiles:
   - `RefundProfile`
   - `AdjustmentProfile`

**Deliverables**:
- 7 validators
- 2 AutoMapper profiles

---

### Phase 10: Frontend - TypeScript Types & API Client (Week 4)
**Goal**: Create TypeScript types and API client

**Tasks**:
1. Create `src/types/refunds.ts`:
   - Refund, Adjustment, RefundTimeline types
   - Request/Response types
   - Enum types
2. Extend `src/lib/api.ts`:
   - `RefundsApi` methods
   - `AdjustmentsApi` methods

**Deliverables**:
- TypeScript type definitions
- API client methods

---

### Phase 11: Frontend - Refund Components (Week 4)
**Goal**: Create refund UI components

**Tasks**:
1. Create refund request form component
2. Create refund detail page
3. Create refund queue/dashboard
4. Create refund history page
5. Create shared components:
   - RefundStatusBadge
   - RefundTimeline
   - RefundReasonBadge
   - ApprovalDialog
   - RefundAmountDisplay

**Deliverables**:
- 5 page components
- 5 shared components

---

### Phase 12: Frontend - Adjustment Components (Week 4)
**Goal**: Create adjustment UI components

**Tasks**:
1. Create adjustment request form
2. Create adjustment timeline component
3. Create adjustment preview component
4. Integrate into quotation detail page

**Deliverables**:
- 3 components
- Quotation page integration

---

### Phase 13: Frontend - Client Portal (Week 5)
**Goal**: Create client portal refund request

**Tasks**:
1. Create client refund request form
2. Integrate into client portal quotation view
3. Add success/error handling

**Deliverables**:
- Client portal component
- Integration complete

---

### Phase 14: Frontend - Custom Hooks & Utilities (Week 5)
**Goal**: Create reusable hooks and utilities

**Tasks**:
1. Create `useRefund` hook
2. Create `useRefundList` hook
3. Create `useRefundApproval` hook
4. Create `useAdjustment` hook
5. Create `useRefundMetrics` hook
6. Create `useRefundForm` hook

**Deliverables**:
- 6 custom hooks
- Utility functions

---

### Phase 15: Integration & Testing (Week 5)
**Goal**: Integration testing and bug fixes

**Tasks**:
1. Backend integration tests
2. Frontend component tests
3. E2E tests for refund flow
4. E2E tests for adjustment flow
5. Payment gateway integration tests
6. Performance testing
7. Security testing

**Deliverables**:
- Test suite
- Bug fixes
- Performance optimizations

---

### Phase 16: Documentation & Deployment (Week 6)
**Goal**: Finalize documentation and deploy

**Tasks**:
1. API documentation
2. User guides
3. Deployment scripts
4. Migration scripts
5. Monitoring setup

**Deliverables**:
- Complete documentation
- Deployment ready

---

## Risk Mitigation

1. **Payment Gateway API Changes**: Abstract gateway calls behind interface
2. **Concurrent Refunds**: Implement locking mechanism
3. **Partial Refund Edge Cases**: Validate sum of refunds <= payment amount
4. **Tax Recalculation Errors**: Test with various tax scenarios
5. **Approval Workflow Complexity**: Use state machine pattern

---

## Success Metrics

- All refunds processed within 48 hours
- 100% audit trail coverage
- Zero financial discrepancies
- <2% refund failure rate
- Approval workflow reduces manual processing time by 60%

