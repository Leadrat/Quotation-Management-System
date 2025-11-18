# Spec-014: Payment Processing & Integration - Detailed Tasks

**Last Updated**: 2025-11-16  
**Total Tasks**: 150+  
**Completed**: 45  
**Remaining**: 105+

---

## Phase 1: Setup & Foundational ✅ COMPLETE

### ✅ Task 1.1: Database Migrations
- [x] Create Payments table migration
- [x] Create PaymentGatewayConfigs table migration
- [x] Add all foreign keys and constraints
- [x] Add all indexes

**Files**: 
- `src/Backend/CRM.Infrastructure/Migrations/20251116060752_CreatePaymentsTable.cs`

### ✅ Task 1.2: Domain Entities
- [x] Create Payment entity
- [x] Create PaymentGatewayConfig entity
- [x] Create PaymentStatus enum
- [x] Create PaymentGateway enum
- [x] Add domain methods to Payment entity
- [x] Add domain methods to PaymentGatewayConfig entity

**Files**:
- `src/Backend/CRM.Domain/Entities/Payment.cs`
- `src/Backend/CRM.Domain/Entities/PaymentGatewayConfig.cs`
- `src/Backend/CRM.Domain/Enums/PaymentStatus.cs`
- `src/Backend/CRM.Domain/Enums/PaymentGateway.cs`

### ✅ Task 1.3: Entity Framework Configuration
- [x] Create PaymentEntityConfiguration
- [x] Create PaymentGatewayConfigEntityConfiguration
- [x] Update AppDbContext
- [x] Update IAppDbContext interface

**Files**:
- `src/Backend/CRM.Infrastructure/EntityConfigurations/PaymentEntityConfiguration.cs`
- `src/Backend/CRM.Infrastructure/EntityConfigurations/PaymentGatewayConfigEntityConfiguration.cs`
- `src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs`
- `src/Backend/CRM.Application/Common/Persistence/IAppDbContext.cs`

---

## Phase 2: Domain Events ✅ COMPLETE

### ✅ Task 2.1: Payment Domain Events
- [x] Create PaymentInitiated event
- [x] Create PaymentSuccess event
- [x] Create PaymentFailed event
- [x] Create PaymentRefunded event
- [x] Create PaymentCancelled event
- [x] Create PaymentGatewayConfigUpdated event

**Files**:
- `src/Backend/CRM.Domain/Events/PaymentInitiated.cs`
- `src/Backend/CRM.Domain/Events/PaymentSuccess.cs`
- `src/Backend/CRM.Domain/Events/PaymentFailed.cs`
- `src/Backend/CRM.Domain/Events/PaymentRefunded.cs`
- `src/Backend/CRM.Domain/Events/PaymentCancelled.cs`
- `src/Backend/CRM.Domain/Events/PaymentGatewayConfigUpdated.cs`

---

## Phase 3: Payment Gateway Services ✅ COMPLETE

### ✅ Task 3.1: Service Abstraction
- [x] Create IPaymentGatewayService interface
- [x] Create IPaymentGatewayFactory interface
- [x] Create PaymentGatewayRequest DTO
- [x] Create PaymentGatewayResponse DTO
- [x] Create RefundGatewayResponse DTO
- [x] Create PaymentVerificationResponse DTO

**Files**:
- `src/Backend/CRM.Application/Payments/Services/IPaymentGatewayService.cs`
- `src/Backend/CRM.Application/Payments/Services/IPaymentGatewayFactory.cs`
- `src/Backend/CRM.Application/Payments/Services/PaymentGatewayRequest.cs`
- `src/Backend/CRM.Application/Payments/Services/PaymentGatewayResponse.cs`

### ✅ Task 3.2: Gateway Implementations
- [x] Create StripePaymentGatewayService (stub)
- [x] Create RazorpayPaymentGatewayService (stub)
- [x] Create PaymentGatewayFactory implementation
- [x] Create IPaymentGatewayEncryptionService interface
- [x] Create PaymentGatewayEncryptionService implementation

**Files**:
- `src/Backend/CRM.Application/Payments/Services/StripePaymentGatewayService.cs`
- `src/Backend/CRM.Application/Payments/Services/RazorpayPaymentGatewayService.cs`
- `src/Backend/CRM.Infrastructure/Services/PaymentGatewayFactory.cs`
- `src/Backend/CRM.Infrastructure/Services/IPaymentGatewayEncryptionService.cs`
- `src/Backend/CRM.Infrastructure/Services/PaymentGatewayEncryptionService.cs`

**TODO**: Install SDK packages and implement actual gateway calls

---

## Phase 4: Commands & Handlers 🟡 IN PROGRESS (25%)

### ✅ Task 4.1: InitiatePaymentCommand
- [x] Create InitiatePaymentCommand
- [x] Create InitiatePaymentCommandHandler
- [x] Create InitiatePaymentRequestValidator
- [x] Add quotation validation
- [x] Add duplicate payment check
- [x] Integrate with gateway service
- [x] Publish PaymentInitiated event

**Files**:
- `src/Backend/CRM.Application/Payments/Commands/InitiatePaymentCommand.cs`
- `src/Backend/CRM.Application/Payments/Commands/Handlers/InitiatePaymentCommandHandler.cs`
- `src/Backend/CRM.Application/Payments/Validators/InitiatePaymentRequestValidator.cs`

### ⏳ Task 4.2: UpdatePaymentStatusCommand (CRITICAL)
- [ ] Create UpdatePaymentStatusCommand
- [ ] Create UpdatePaymentStatusCommandHandler
- [ ] Create UpdatePaymentStatusRequest DTO
- [ ] Create UpdatePaymentStatusRequestValidator
- [ ] Add payment verification logic
- [ ] Update quotation status if needed
- [ ] Publish PaymentSuccess or PaymentFailed event
- [ ] Handle idempotency

**Files**:
- `src/Backend/CRM.Application/Payments/Commands/UpdatePaymentStatusCommand.cs`
- `src/Backend/CRM.Application/Payments/Commands/Handlers/UpdatePaymentStatusCommandHandler.cs`
- `src/Backend/CRM.Application/Payments/Dtos/UpdatePaymentStatusRequest.cs`
- `src/Backend/CRM.Application/Payments/Validators/UpdatePaymentStatusRequestValidator.cs`

**Priority**: 🔴 HIGH - Required for webhook processing

### ⏳ Task 4.3: RefundPaymentCommand
- [ ] Create RefundPaymentCommand
- [ ] Create RefundPaymentCommandHandler
- [ ] Create RefundPaymentRequest DTO (already exists)
- [ ] Create RefundPaymentRequestValidator
- [ ] Add refund validation (amount, status)
- [ ] Integrate with gateway refund service
- [ ] Update payment entity
- [ ] Publish PaymentRefunded event

**Files**:
- `src/Backend/CRM.Application/Payments/Commands/RefundPaymentCommand.cs`
- `src/Backend/CRM.Application/Payments/Commands/Handlers/RefundPaymentCommandHandler.cs`
- `src/Backend/CRM.Application/Payments/Validators/RefundPaymentRequestValidator.cs`

**Priority**: 🟡 MEDIUM

### ⏳ Task 4.4: CancelPaymentCommand
- [ ] Create CancelPaymentCommand
- [ ] Create CancelPaymentCommandHandler
- [ ] Create CancelPaymentRequestValidator
- [ ] Add cancellation validation
- [ ] Integrate with gateway cancel service (if needed)
- [ ] Update payment entity
- [ ] Publish PaymentCancelled event

**Files**:
- `src/Backend/CRM.Application/Payments/Commands/CancelPaymentCommand.cs`
- `src/Backend/CRM.Application/Payments/Commands/Handlers/CancelPaymentCommandHandler.cs`
- `src/Backend/CRM.Application/Payments/Validators/CancelPaymentCommandValidator.cs`

**Priority**: 🟡 MEDIUM

### ⏳ Task 4.5: Payment Gateway Config Commands
- [ ] Create CreatePaymentGatewayConfigCommand
- [ ] Create CreatePaymentGatewayConfigCommandHandler
- [ ] Create UpdatePaymentGatewayConfigCommand
- [ ] Create UpdatePaymentGatewayConfigCommandHandler
- [ ] Create DeletePaymentGatewayConfigCommand
- [ ] Create DeletePaymentGatewayConfigCommandHandler
- [ ] Create validators for all config commands
- [ ] Add API key validation (test call)
- [ ] Encrypt credentials before saving
- [ ] Publish PaymentGatewayConfigUpdated event

**Files**:
- `src/Backend/CRM.Application/Payments/Commands/CreatePaymentGatewayConfigCommand.cs`
- `src/Backend/CRM.Application/Payments/Commands/Handlers/CreatePaymentGatewayConfigCommandHandler.cs`
- `src/Backend/CRM.Application/Payments/Commands/UpdatePaymentGatewayConfigCommand.cs`
- `src/Backend/CRM.Application/Payments/Commands/Handlers/UpdatePaymentGatewayConfigCommandHandler.cs`
- `src/Backend/CRM.Application/Payments/Commands/DeletePaymentGatewayConfigCommand.cs`
- `src/Backend/CRM.Application/Payments/Commands/Handlers/DeletePaymentGatewayConfigCommandHandler.cs`
- `src/Backend/CRM.Application/Payments/Validators/CreatePaymentGatewayConfigRequestValidator.cs`
- `src/Backend/CRM.Application/Payments/Validators/UpdatePaymentGatewayConfigRequestValidator.cs`

**Priority**: 🟡 MEDIUM

---

## Phase 5: Queries & Handlers ⏳ PENDING

### ⏳ Task 5.1: GetPaymentByQuotationQuery
- [ ] Create GetPaymentByQuotationQuery
- [ ] Create GetPaymentByQuotationQueryHandler
- [ ] Create GetPaymentByQuotationQueryValidator
- [ ] Add authorization (user can only see own quotations)
- [ ] Return payment history for quotation
- [ ] Include payment status and amounts

**Files**:
- `src/Backend/CRM.Application/Payments/Queries/GetPaymentByQuotationQuery.cs`
- `src/Backend/CRM.Application/Payments/Queries/Handlers/GetPaymentByQuotationQueryHandler.cs`
- `src/Backend/CRM.Application/Payments/Validators/GetPaymentByQuotationQueryValidator.cs`

**Priority**: 🟡 MEDIUM

### ⏳ Task 5.2: GetPaymentByIdQuery
- [ ] Create GetPaymentByIdQuery
- [ ] Create GetPaymentByIdQueryHandler
- [ ] Create GetPaymentByIdQueryValidator
- [ ] Add authorization
- [ ] Return full payment details

**Files**:
- `src/Backend/CRM.Application/Payments/Queries/GetPaymentByIdQuery.cs`
- `src/Backend/CRM.Application/Payments/Queries/Handlers/GetPaymentByIdQueryHandler.cs`
- `src/Backend/CRM.Application/Payments/Validators/GetPaymentByIdQueryValidator.cs`

**Priority**: 🟡 MEDIUM

### ⏳ Task 5.3: GetPaymentsByUserQuery
- [ ] Create GetPaymentsByUserQuery
- [ ] Create GetPaymentsByUserQueryHandler
- [ ] Create GetPaymentsByUserQueryValidator
- [ ] Add filters (status, date range, quotation)
- [ ] Add pagination support
- [ ] Add sorting options
- [ ] Add authorization (SalesRep sees own, Admin sees all)

**Files**:
- `src/Backend/CRM.Application/Payments/Queries/GetPaymentsByUserQuery.cs`
- `src/Backend/CRM.Application/Payments/Queries/Handlers/GetPaymentsByUserQueryHandler.cs`
- `src/Backend/CRM.Application/Payments/Validators/GetPaymentsByUserQueryValidator.cs`

**Priority**: 🟡 MEDIUM

### ⏳ Task 5.4: GetPaymentsDashboardQuery
- [ ] Create GetPaymentsDashboardQuery
- [ ] Create GetPaymentsDashboardQueryHandler
- [ ] Create GetPaymentsDashboardQueryValidator
- [ ] Calculate summary statistics (pending, paid, refunded, failed)
- [ ] Support date range filtering
- [ ] Add authorization (role-based)
- [ ] Return PaymentDashboardDto

**Files**:
- `src/Backend/CRM.Application/Payments/Queries/GetPaymentsDashboardQuery.cs`
- `src/Backend/CRM.Application/Payments/Queries/Handlers/GetPaymentsDashboardQueryHandler.cs`
- `src/Backend/CRM.Application/Payments/Validators/GetPaymentsDashboardQueryValidator.cs`

**Priority**: 🟡 MEDIUM

### ⏳ Task 5.5: GetPaymentGatewayConfigQuery
- [ ] Create GetPaymentGatewayConfigQuery
- [ ] Create GetPaymentGatewayConfigQueryHandler
- [ ] Create GetPaymentGatewayConfigQueryValidator
- [ ] Mask sensitive data (API keys, secrets)
- [ ] Add admin-only authorization
- [ ] Return list of gateway configs

**Files**:
- `src/Backend/CRM.Application/Payments/Queries/GetPaymentGatewayConfigQuery.cs`
- `src/Backend/CRM.Application/Payments/Queries/Handlers/GetPaymentGatewayConfigQueryHandler.cs`
- `src/Backend/CRM.Application/Payments/Validators/GetPaymentGatewayConfigQueryValidator.cs`

**Priority**: 🟢 LOW

---

## Phase 6: API Endpoints & Controllers 🟡 IN PROGRESS (10%)

### ✅ Task 6.1: PaymentsController - Initiate
- [x] Create PaymentsController
- [x] Add POST /api/v1/payments/initiate endpoint
- [x] Add authorization
- [x] Add validation
- [x] Add error handling

**Files**:
- `src/Backend/CRM.Api/Controllers/PaymentsController.cs` (partial)

### ⏳ Task 6.2: PaymentsController - Get Endpoints
- [ ] Add GET /api/v1/payments/{paymentId} endpoint
- [ ] Add GET /api/v1/quotations/{quotationId}/payments endpoint
- [ ] Add authorization checks
- [ ] Add error handling
- [ ] Return appropriate status codes

**Files**:
- `src/Backend/CRM.Api/Controllers/PaymentsController.cs` (update)

**Priority**: 🟡 MEDIUM

### ⏳ Task 6.3: PaymentsController - Actions
- [ ] Add POST /api/v1/payments/{paymentId}/refund endpoint
- [ ] Add POST /api/v1/payments/{paymentId}/cancel endpoint
- [ ] Add authorization checks
- [ ] Add validation
- [ ] Add error handling

**Files**:
- `src/Backend/CRM.Api/Controllers/PaymentsController.cs` (update)

**Priority**: 🟡 MEDIUM

### ⏳ Task 6.4: PaymentsController - Dashboard
- [ ] Add GET /api/v1/payments/dashboard endpoint
- [ ] Add query parameters (date range, filters)
- [ ] Add authorization (role-based)
- [ ] Return PaymentDashboardDto

**Files**:
- `src/Backend/CRM.Api/Controllers/PaymentsController.cs` (update)

**Priority**: 🟡 MEDIUM

### ⏳ Task 6.5: PaymentGatewaysController
- [ ] Create PaymentGatewaysController
- [ ] Add POST /api/v1/payment-gateways/config endpoint
- [ ] Add GET /api/v1/payment-gateways/config/{companyId} endpoint
- [ ] Add admin-only authorization
- [ ] Add validation
- [ ] Add API key test functionality

**Files**:
- `src/Backend/CRM.Api/Controllers/PaymentGatewaysController.cs`

**Priority**: 🟡 MEDIUM

### ⏳ Task 6.6: PaymentWebhookController (CRITICAL)
- [ ] Create PaymentWebhookController
- [ ] Add POST /api/v1/payment-webhook/{gateway} endpoint
- [ ] Add webhook signature verification
- [ ] Add idempotency handling
- [ ] Parse gateway-specific webhook payloads
- [ ] Call UpdatePaymentStatusCommand
- [ ] Return appropriate HTTP status codes
- [ ] Add logging

**Files**:
- `src/Backend/CRM.Api/Controllers/PaymentWebhookController.cs`

**Priority**: 🔴 HIGH - Required for payment status updates

---

## Phase 7: AutoMapper & Service Registration ⏳ PENDING

### ⏳ Task 7.1: AutoMapper Profile
- [ ] Create PaymentProfile
- [ ] Map Payment → PaymentDto
- [ ] Map PaymentGatewayConfig → PaymentGatewayConfigDto
- [ ] Map request DTOs to commands/queries
- [ ] Add computed properties mapping

**Files**:
- `src/Backend/CRM.Application/Mapping/PaymentProfile.cs`

**Priority**: 🔴 HIGH - Required for DTO mapping

### ⏳ Task 7.2: Service Registration
- [ ] Register StripePaymentGatewayService
- [ ] Register RazorpayPaymentGatewayService
- [ ] Register PaymentGatewayFactory
- [ ] Register PaymentGatewayEncryptionService
- [ ] Register all command handlers
- [ ] Register all query handlers
- [ ] Register all validators
- [ ] Configure encryption key from appsettings

**Files**:
- `src/Backend/CRM.Api/Program.cs` (update)

**Priority**: 🔴 HIGH - Required for DI to work

---

## Phase 8: Frontend API Integration ⏳ PENDING

### ⏳ Task 8.1: TypeScript Types
- [ ] Create Payment type
- [ ] Create PaymentStatus type
- [ ] Create PaymentGateway type
- [ ] Create PaymentDto type
- [ ] Create PaymentDashboardDto type
- [ ] Create PaymentSummaryDto type
- [ ] Create PaymentGatewayConfigDto type
- [ ] Create request/response types for all endpoints

**Files**:
- `src/Frontend/web/src/types/payments.ts`

**Priority**: 🟡 MEDIUM

### ⏳ Task 8.2: API Client
- [ ] Add PaymentsApi object to api.ts
- [ ] Add initiatePayment method
- [ ] Add getPayment method
- [ ] Add getPaymentsByQuotation method
- [ ] Add refundPayment method
- [ ] Add cancelPayment method
- [ ] Add getPaymentsDashboard method
- [ ] Add error handling
- [ ] Add TypeScript types

**Files**:
- `src/Frontend/web/src/lib/api.ts` (update)

**Priority**: 🟡 MEDIUM

---

## Phase 9: Frontend Components ⏳ PENDING

### ⏳ Task 9.1: Payments Dashboard Page
- [ ] Create /payments page route
- [ ] Create PaymentsDashboard component
- [ ] Create PaymentSummaryCards component
- [ ] Create PaymentsTable component
- [ ] Add filters (status, date range, quotation)
- [ ] Add pagination
- [ ] Add search functionality
- [ ] Add retry/cancel buttons
- [ ] Integrate with PaymentsApi

**Files**:
- `src/Frontend/web/src/app/(protected)/payments/page.tsx`
- `src/Frontend/web/src/components/payments/PaymentsDashboard.tsx`
- `src/Frontend/web/src/components/payments/PaymentSummaryCards.tsx`
- `src/Frontend/web/src/components/payments/PaymentsTable.tsx`

**Priority**: 🟡 MEDIUM

### ⏳ Task 9.2: Payment Modal
- [ ] Create PaymentModal component
- [ ] Create PaymentMethodSelector component
- [ ] Create PaymentForm component
- [ ] Add payment method selection
- [ ] Add amount breakdown display
- [ ] Add form validation
- [ ] Add loading states
- [ ] Handle success/failure
- [ ] Integrate with notification system

**Files**:
- `src/Frontend/web/src/components/payments/PaymentModal.tsx`
- `src/Frontend/web/src/components/payments/PaymentMethodSelector.tsx`
- `src/Frontend/web/src/components/payments/PaymentForm.tsx`

**Priority**: 🟡 MEDIUM

### ⏳ Task 9.3: Quotation Payment Section
- [ ] Create QuotationPaymentSection component
- [ ] Create PaymentStatusBadge component
- [ ] Create PaymentHistory component
- [ ] Add to quotation detail page
- [ ] Show payment status
- [ ] Show payment history
- [ ] Add initiate payment button
- [ ] Add refund options (if applicable)

**Files**:
- `src/Frontend/web/src/components/payments/QuotationPaymentSection.tsx`
- `src/Frontend/web/src/components/payments/PaymentStatusBadge.tsx`
- `src/Frontend/web/src/components/payments/PaymentHistory.tsx`
- `src/Frontend/web/src/app/(protected)/quotations/[id]/page.tsx` (update)

**Priority**: 🟡 MEDIUM

### ⏳ Task 9.4: Admin Gateway Configuration
- [ ] Create /admin/payment-gateways page route
- [ ] Create GatewayConfigForm component
- [ ] Create GatewayConfigList component
- [ ] Add CRUD interface
- [ ] Add API key test functionality
- [ ] Add enable/disable toggle
- [ ] Add test mode toggle
- [ ] Add validation

**Files**:
- `src/Frontend/web/src/app/(protected)/admin/payment-gateways/page.tsx`
- `src/Frontend/web/src/components/payments/GatewayConfigForm.tsx`
- `src/Frontend/web/src/components/payments/GatewayConfigList.tsx`

**Priority**: 🟢 LOW

### ⏳ Task 9.5: Client Portal Payment Page
- [ ] Create /quotation/[token]/payment page route
- [ ] Create ClientPaymentPage component
- [ ] Show quotation details
- [ ] Show payment amount breakdown
- [ ] Integrate payment gateway UI
- [ ] Show payment status
- [ ] Add receipt download option
- [ ] Make responsive and accessible

**Files**:
- `src/Frontend/web/src/app/(public)/quotation/[token]/payment/page.tsx`
- `src/Frontend/web/src/components/payments/ClientPaymentPage.tsx`

**Priority**: 🟡 MEDIUM

---

## Phase 10: Event Handlers & Notifications ⏳ PENDING

### ⏳ Task 10.1: Payment Event Handlers
- [ ] Create PaymentSuccessEventHandler
- [ ] Create PaymentFailedEventHandler
- [ ] Create PaymentRefundedEventHandler
- [ ] Create PaymentCancelledEventHandler
- [ ] Integrate with Spec-013 notification system
- [ ] Send notifications to relevant users (client, sales rep, admin)
- [ ] Include payment details in notifications

**Files**:
- `src/Backend/CRM.Application/Payments/EventHandlers/PaymentSuccessEventHandler.cs`
- `src/Backend/CRM.Application/Payments/EventHandlers/PaymentFailedEventHandler.cs`
- `src/Backend/CRM.Application/Payments/EventHandlers/PaymentRefundedEventHandler.cs`
- `src/Backend/CRM.Application/Payments/EventHandlers/PaymentCancelledEventHandler.cs`

**Priority**: 🟡 MEDIUM

---

## Phase 11: Testing ⏳ PENDING

### ⏳ Task 11.1: Backend Unit Tests
- [ ] Test InitiatePaymentCommandHandler
- [ ] Test UpdatePaymentStatusCommandHandler
- [ ] Test RefundPaymentCommandHandler
- [ ] Test CancelPaymentCommandHandler
- [ ] Test GetPaymentByQuotationQueryHandler
- [ ] Test GetPaymentsDashboardQueryHandler
- [ ] Test Payment entity domain methods
- [ ] Test PaymentGatewayConfig entity domain methods
- [ ] Test payment gateway services (mocked)

**Files**:
- `tests/CRM.Tests/Payments/InitiatePaymentCommandHandlerTests.cs`
- `tests/CRM.Tests/Payments/UpdatePaymentStatusCommandHandlerTests.cs`
- `tests/CRM.Tests/Payments/RefundPaymentCommandHandlerTests.cs`
- `tests/CRM.Tests/Payments/CancelPaymentCommandHandlerTests.cs`
- `tests/CRM.Tests/Payments/GetPaymentByQuotationQueryHandlerTests.cs`
- `tests/CRM.Tests/Payments/GetPaymentsDashboardQueryHandlerTests.cs`
- `tests/CRM.Tests/Payments/PaymentEntityTests.cs`
- `tests/CRM.Tests/Payments/PaymentGatewayServiceTests.cs`

**Priority**: 🟡 MEDIUM

### ⏳ Task 11.2: Backend Integration Tests
- [ ] Test PaymentsController endpoints
- [ ] Test PaymentGatewaysController endpoints
- [ ] Test PaymentWebhookController endpoints
- [ ] Test authorization
- [ ] Test error scenarios
- [ ] Test webhook signature verification

**Files**:
- `tests/CRM.Tests.Integration/Payments/PaymentsControllerTests.cs`
- `tests/CRM.Tests.Integration/Payments/PaymentGatewaysControllerTests.cs`
- `tests/CRM.Tests.Integration/Payments/PaymentWebhookControllerTests.cs`

**Priority**: 🟡 MEDIUM

### ⏳ Task 11.3: Frontend Tests
- [ ] Test PaymentModal component
- [ ] Test PaymentsDashboard component
- [ ] Test PaymentSummaryCards component
- [ ] Test QuotationPaymentSection component
- [ ] Test GatewayConfigForm component
- [ ] Test ClientPaymentPage component
- [ ] E2E test for payment flow

**Files**:
- `src/Frontend/web/src/components/payments/__tests__/PaymentModal.test.tsx`
- `src/Frontend/web/src/components/payments/__tests__/PaymentsDashboard.test.tsx`
- `src/Frontend/web/src/components/payments/__tests__/PaymentSummaryCards.test.tsx`
- `src/Frontend/web/src/components/payments/__tests__/QuotationPaymentSection.test.tsx`
- `src/Frontend/web/src/components/payments/__tests__/GatewayConfigForm.test.tsx`
- `src/Frontend/web/src/components/payments/__tests__/ClientPaymentPage.test.tsx`
- `src/Frontend/web/src/components/payments/__tests__/payment-flow.e2e.test.tsx`

**Priority**: 🟢 LOW

---

## Phase 12: Documentation & Contracts ⏳ PENDING

### ⏳ Task 12.1: OpenAPI Contract
- [ ] Create payments.openapi.yaml
- [ ] Define all payment endpoints
- [ ] Define request/response schemas
- [ ] Add security definitions
- [ ] Add examples

**Files**:
- `specs/014-payment-processing/contracts/payments.openapi.yaml`

**Priority**: 🟢 LOW

### ⏳ Task 12.2: Quickstart Guide
- [ ] Create quickstart.md
- [ ] Add setup instructions
- [ ] Add gateway configuration steps
- [ ] Add testing instructions
- [ ] Add troubleshooting section

**Files**:
- `specs/014-payment-processing/quickstart.md`

**Priority**: 🟢 LOW

---

## Additional Tasks

### ⏳ Task A.1: SDK Integration
- [ ] Install Stripe.NET package
- [ ] Implement StripePaymentGatewayService with actual Stripe API calls
- [ ] Install Razorpay package
- [ ] Implement RazorpayPaymentGatewayService with actual Razorpay API calls
- [ ] Test gateway integrations

**Priority**: 🟡 MEDIUM

### ⏳ Task A.2: Configuration
- [ ] Add PaymentGateway:EncryptionKey to appsettings.json
- [ ] Add PaymentGateway settings section
- [ ] Document configuration requirements

**Files**:
- `src/Backend/CRM.Api/appsettings.json` (update)

**Priority**: 🟡 MEDIUM

### ⏳ Task A.3: Database Migration
- [ ] Apply database migrations
- [ ] Verify tables created correctly
- [ ] Test foreign key constraints

**Command**:
```bash
dotnet ef database update --startup-project src/Backend/CRM.Api
```

**Priority**: 🟡 MEDIUM

---

## Task Summary

| Phase | Total Tasks | Completed | Remaining | Status |
|-------|------------|-----------|-----------|--------|
| Phase 1 | 12 | 12 | 0 | ✅ Complete |
| Phase 2 | 6 | 6 | 0 | ✅ Complete |
| Phase 3 | 9 | 9 | 0 | ✅ Complete |
| Phase 4 | 20 | 5 | 15 | 🟡 25% |
| Phase 5 | 15 | 0 | 15 | ⏳ Pending |
| Phase 6 | 18 | 2 | 16 | 🟡 10% |
| Phase 7 | 2 | 0 | 2 | ⏳ Pending |
| Phase 8 | 10 | 0 | 10 | ⏳ Pending |
| Phase 9 | 25 | 0 | 25 | ⏳ Pending |
| Phase 10 | 4 | 0 | 4 | ⏳ Pending |
| Phase 11 | 15 | 0 | 15 | ⏳ Pending |
| Phase 12 | 2 | 0 | 2 | ⏳ Pending |
| Additional | 3 | 0 | 3 | ⏳ Pending |
| **TOTAL** | **141** | **34** | **107** | **24%** |

---

## Priority Legend

- 🔴 **HIGH**: Critical path items, must be completed for system to function
- 🟡 **MEDIUM**: Important features, should be completed soon
- 🟢 **LOW**: Nice to have, can be deferred

---

## Next Steps (Recommended Order)

1. **Complete Phase 4** - Finish remaining commands (UpdatePaymentStatus is critical)
2. **Complete Phase 7** - AutoMapper and service registration (required for DI)
3. **Complete Phase 6** - All API endpoints (especially webhook)
4. **Complete Phase 5** - All queries
5. **Complete Phase 8** - Frontend API integration
6. **Complete Phase 9** - Frontend components (start with payment modal)
7. **Complete Phase 10** - Event handlers
8. **Complete Phase 11** - Testing
9. **Complete Phase 12** - Documentation

---

**Last Updated**: 2025-11-16

