# Implementation Plan: Spec-014 Payment Processing & Integration

**Spec**: Spec-014  
**Last Updated**: 2025-11-15

## Overview

This plan outlines the phased implementation of the Payment Processing & Integration system, building on Spec-009 (Quotation Entity), Spec-010 (Quotation Management), and Spec-013 (Notification System). The system provides secure payment processing with multiple gateway support, real-time status updates, refunds, and comprehensive dashboards.

---

## Implementation Phases

### Phase 1: Setup & Foundational (Days 1-2)

**Goal**: Establish database schema, entities, enums, and basic infrastructure.

#### Step 1.1: Database Migrations
**Files**: 
- `src/Backend/CRM.Infrastructure/Migrations/YYYYMMDDHHMMSS_CreatePaymentsTable.cs`
- `src/Backend/CRM.Infrastructure/Migrations/YYYYMMDDHHMMSS_CreatePaymentGatewayConfigsTable.cs`

**Tasks**:
- Create `Payments` table with all columns (PaymentId, QuotationId, PaymentGateway, PaymentReference, AmountPaid, Currency, PaymentStatus, PaymentDate, CreatedAt, UpdatedAt, FailureReason, IsRefundable, RefundAmount, RefundReason, RefundDate, Metadata)
- Create `PaymentGatewayConfigs` table with all columns (ConfigId, CompanyId, GatewayName, ApiKey, ApiSecret, WebhookSecret, Enabled, IsTestMode, CreatedAt, UpdatedAt, CreatedByUserId, Metadata)
- Add foreign keys (QuotationId → Quotations, CompanyId → Companies, CreatedByUserId → Users)
- Add unique constraint on PaymentReference
- Add unique constraint on CompanyId + GatewayName
- Add all indexes (QuotationId, PaymentReference, PaymentStatus, PaymentDate, CompanyId, GatewayName, Enabled)

#### Step 1.2: Domain Entities
**Files**:
- `src/Backend/CRM.Domain/Entities/Payment.cs`
- `src/Backend/CRM.Domain/Entities/PaymentGatewayConfig.cs`
- `src/Backend/CRM.Domain/Enums/PaymentStatus.cs`
- `src/Backend/CRM.Domain/Enums/PaymentGateway.cs`

**Tasks**:
- Create `Payment` entity with all properties
- Add navigation property `Quotation`
- Add domain methods: `MarkAsSuccess()`, `MarkAsFailed()`, `ProcessRefund()`, `Cancel()`
- Create `PaymentGatewayConfig` entity with all properties
- Add navigation property `CreatedByUser`
- Add domain methods: `Enable()`, `Disable()`, `UpdateCredentials()`
- Create `PaymentStatus` enum (Pending, Processing, Success, Failed, Refunded, PartiallyRefunded, Cancelled)
- Create `PaymentGateway` enum (Stripe, Razorpay, PayPal, Custom)

#### Step 1.3: Entity Framework Configuration
**Files**:
- `src/Backend/CRM.Infrastructure/EntityConfigurations/PaymentEntityConfiguration.cs`
- `src/Backend/CRM.Infrastructure/EntityConfigurations/PaymentGatewayConfigEntityConfiguration.cs`

**Tasks**:
- Configure table names, primary keys, property constraints
- Configure JSONB column for Metadata
- Configure enum to integer conversions
- Configure relationships and foreign keys
- Configure all indexes
- Configure cascade delete behavior

#### Step 1.4: Update DbContext
**Files**:
- `src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs`
- `src/Backend/CRM.Application/Common/Persistence/IAppDbContext.cs`

**Tasks**:
- Add `DbSet<Payment> Payments`
- Add `DbSet<PaymentGatewayConfig> PaymentGatewayConfigs`
- Update interface with same properties

#### Step 1.5: DTOs
**Files** (in `src/Backend/CRM.Application/Payments/Dtos/`):
- `PaymentDto.cs`
- `InitiatePaymentRequest.cs`
- `RefundPaymentRequest.cs`
- `PaymentGatewayConfigDto.cs`
- `CreatePaymentGatewayConfigRequest.cs`
- `UpdatePaymentGatewayConfigRequest.cs`
- `PaymentDashboardDto.cs`
- `PaymentSummaryDto.cs`

**Tasks**:
- Create all DTO classes with proper properties
- Add validation attributes where needed
- Include computed properties (e.g., formatted dates, status labels)

---

### Phase 2: Domain Events (Day 2)

**Goal**: Create domain events for payment lifecycle.

#### Step 2.1: Domain Events
**Files** (in `src/Backend/CRM.Domain/Events/`):
- `PaymentInitiated.cs`
- `PaymentSuccess.cs`
- `PaymentFailed.cs`
- `PaymentRefunded.cs`
- `PaymentCancelled.cs`
- `PaymentGatewayConfigUpdated.cs`

**Tasks**:
- Create all domain event classes
- Include relevant properties (PaymentId, QuotationId, Amount, etc.)
- Add timestamp and user information

---

### Phase 3: Payment Gateway Services (Days 3-4)

**Goal**: Create payment gateway abstraction and implementations.

#### Step 3.1: Payment Gateway Interface
**Files**:
- `src/Backend/CRM.Application/Payments/Services/IPaymentGatewayService.cs`
- `src/Backend/CRM.Application/Payments/Services/PaymentGatewayRequest.cs`
- `src/Backend/CRM.Application/Payments/Services/PaymentGatewayResponse.cs`

**Tasks**:
- Define interface with methods: `InitiatePaymentAsync()`, `VerifyPaymentAsync()`, `RefundPaymentAsync()`, `CancelPaymentAsync()`, `VerifyWebhookAsync()`
- Create request/response DTOs for gateway communication
- Include error handling and retry logic

#### Step 3.2: Stripe Implementation
**Files**:
- `src/Backend/CRM.Application/Payments/Services/StripePaymentGatewayService.cs`

**Tasks**:
- Implement `IPaymentGatewayService` for Stripe
- Use Stripe.NET SDK
- Handle payment intents, webhooks, refunds
- Implement webhook signature verification

#### Step 3.3: Razorpay Implementation
**Files**:
- `src/Backend/CRM.Application/Payments/Services/RazorpayPaymentGatewayService.cs`

**Tasks**:
- Implement `IPaymentGatewayService` for Razorpay
- Use Razorpay SDK
- Handle orders, payments, refunds
- Implement webhook signature verification

#### Step 3.4: Payment Gateway Factory
**Files**:
- `src/Backend/CRM.Application/Payments/Services/IPaymentGatewayFactory.cs`
- `src/Backend/CRM.Application/Payments/Services/PaymentGatewayFactory.cs`

**Tasks**:
- Create factory to resolve gateway service based on gateway name
- Load configuration from database
- Handle encryption/decryption of API keys

#### Step 3.5: Encryption Service
**Files**:
- `src/Backend/CRM.Infrastructure/Services/IPaymentGatewayEncryptionService.cs`
- `src/Backend/CRM.Infrastructure/Services/PaymentGatewayEncryptionService.cs`

**Tasks**:
- Encrypt/decrypt API keys and secrets
- Use secure key management
- Store encryption keys in configuration

---

### Phase 4: Commands & Handlers (Days 4-5)

**Goal**: Implement payment commands and handlers.

#### Step 4.1: InitiatePaymentCommand
**Files**:
- `src/Backend/CRM.Application/Payments/Commands/InitiatePaymentCommand.cs`
- `src/Backend/CRM.Application/Payments/Commands/Handlers/InitiatePaymentCommandHandler.cs`
- `src/Backend/CRM.Application/Payments/Validators/InitiatePaymentCommandValidator.cs`

**Tasks**:
- Create command with QuotationId, Gateway, Amount
- Validate quotation exists and is accepted
- Check if payment already exists
- Call payment gateway service
- Create Payment entity
- Publish `PaymentInitiated` event
- Trigger notification (Spec-013)

#### Step 4.2: UpdatePaymentStatusCommand
**Files**:
- `src/Backend/CRM.Application/Payments/Commands/UpdatePaymentStatusCommand.cs`
- `src/Backend/CRM.Application/Payments/Commands/Handlers/UpdatePaymentStatusCommandHandler.cs`
- `src/Backend/CRM.Application/Payments/Validators/UpdatePaymentStatusCommandValidator.cs`

**Tasks**:
- Create command with PaymentId, Status, PaymentReference
- Update payment status
- Update quotation status if needed
- Publish appropriate domain event
- Trigger notification

#### Step 4.3: RefundPaymentCommand
**Files**:
- `src/Backend/CRM.Application/Payments/Commands/RefundPaymentCommand.cs`
- `src/Backend/CRM.Application/Payments/Commands/Handlers/RefundPaymentCommandHandler.cs`
- `src/Backend/CRM.Application/Payments/Validators/RefundPaymentCommandValidator.cs`

**Tasks**:
- Create command with PaymentId, Amount, Reason
- Validate payment is refundable
- Call gateway refund service
- Update payment entity
- Publish `PaymentRefunded` event
- Trigger notification

#### Step 4.4: CancelPaymentCommand
**Files**:
- `src/Backend/CRM.Application/Payments/Commands/CancelPaymentCommand.cs`
- `src/Backend/CRM.Application/Payments/Commands/Handlers/CancelPaymentCommandHandler.cs`
- `src/Backend/CRM.Application/Payments/Validators/CancelPaymentCommandValidator.cs`

**Tasks**:
- Create command with PaymentId
- Validate payment can be cancelled
- Call gateway cancel service if needed
- Update payment entity
- Publish `PaymentCancelled` event
- Trigger notification

#### Step 4.5: Payment Gateway Config Commands
**Files**:
- `src/Backend/CRM.Application/Payments/Commands/CreatePaymentGatewayConfigCommand.cs`
- `src/Backend/CRM.Application/Payments/Commands/UpdatePaymentGatewayConfigCommand.cs`
- `src/Backend/CRM.Application/Payments/Commands/DeletePaymentGatewayConfigCommand.cs`
- Handlers and validators for each

**Tasks**:
- Create CRUD commands for gateway configs
- Validate API keys with test calls
- Encrypt credentials before saving
- Publish `PaymentGatewayConfigUpdated` event

---

### Phase 5: Queries & Handlers (Day 5)

**Goal**: Implement payment queries.

#### Step 5.1: GetPaymentByQuotationQuery
**Files**:
- `src/Backend/CRM.Application/Payments/Queries/GetPaymentByQuotationQuery.cs`
- `src/Backend/CRM.Application/Payments/Queries/Handlers/GetPaymentByQuotationQueryHandler.cs`
- `src/Backend/CRM.Application/Payments/Validators/GetPaymentByQuotationQueryValidator.cs`

**Tasks**:
- Query payments by QuotationId
- Return payment history
- Include authorization

#### Step 5.2: GetPaymentsByUserQuery
**Files**:
- `src/Backend/CRM.Application/Payments/Queries/GetPaymentsByUserQuery.cs`
- `src/Backend/CRM.Application/Payments/Queries/Handlers/GetPaymentsByUserQueryHandler.cs`
- `src/Backend/CRM.Application/Payments/Validators/GetPaymentsByUserQueryValidator.cs`

**Tasks**:
- Query payments for user's quotations
- Support filtering by status, date range
- Include pagination

#### Step 5.3: GetPaymentsDashboardQuery
**Files**:
- `src/Backend/CRM.Application/Payments/Queries/GetPaymentsDashboardQuery.cs`
- `src/Backend/CRM.Application/Payments/Queries/Handlers/GetPaymentsDashboardQueryHandler.cs`
- `src/Backend/CRM.Application/Payments/Validators/GetPaymentsDashboardQueryValidator.cs`

**Tasks**:
- Aggregate payment statistics
- Return summary cards (Total Pending, Total Paid, Refunds, Failed)
- Support date range filtering
- Role-based access (SalesRep sees own, Admin sees all)

#### Step 5.4: GetPaymentGatewayConfigQuery
**Files**:
- `src/Backend/CRM.Application/Payments/Queries/GetPaymentGatewayConfigQuery.cs`
- `src/Backend/CRM.Application/Payments/Queries/Handlers/GetPaymentGatewayConfigQueryHandler.cs`
- `src/Backend/CRM.Application/Payments/Validators/GetPaymentGatewayConfigQueryValidator.cs`

**Tasks**:
- Query gateway configs by CompanyId
- Mask sensitive data in response
- Admin-only access

---

### Phase 6: API Endpoints & Controllers (Day 6)

**Goal**: Create REST API endpoints.

#### Step 6.1: PaymentsController
**Files**:
- `src/Backend/CRM.Api/Controllers/PaymentsController.cs`

**Tasks**:
- `POST /api/v1/payments/initiate` - Initiate payment
- `GET /api/v1/payments/{paymentId}` - Get payment details
- `GET /api/v1/quotations/{quotationId}/payments` - Get payments for quotation
- `POST /api/v1/payments/{paymentId}/refund` - Refund payment
- `POST /api/v1/payments/{paymentId}/cancel` - Cancel payment
- `GET /api/v1/payments/dashboard` - Get dashboard data
- Add authorization attributes
- Add validation
- Add error handling

#### Step 6.2: PaymentGatewaysController
**Files**:
- `src/Backend/CRM.Api/Controllers/PaymentGatewaysController.cs`

**Tasks**:
- `POST /api/v1/payment-gateways/config` - Create/update gateway config
- `GET /api/v1/payment-gateways/config/{companyId}` - Get gateway configs
- Admin-only authorization
- Validate API keys with test calls

#### Step 6.3: PaymentWebhookController
**Files**:
- `src/Backend/CRM.Api/Controllers/PaymentWebhookController.cs`

**Tasks**:
- `POST /api/v1/payment-webhook/{gateway}` - Handle webhook callbacks
- Verify webhook signatures
- Update payment status
- Handle idempotency
- Return appropriate HTTP status codes

#### Step 6.4: AutoMapper Profile
**Files**:
- `src/Backend/CRM.Application/Mapping/PaymentProfile.cs`

**Tasks**:
- Map Payment → PaymentDto
- Map PaymentGatewayConfig → PaymentGatewayConfigDto
- Map request DTOs to commands/queries

#### Step 6.5: Register Services
**Files**:
- `src/Backend/CRM.Api/Program.cs`

**Tasks**:
- Register all payment services
- Register gateway implementations
- Register handlers and validators
- Register encryption service

---

### Phase 7: Frontend API Integration (Day 7)

**Goal**: Create frontend API client and types.

#### Step 7.1: TypeScript Types
**Files**:
- `src/Frontend/web/src/types/payments.ts`

**Tasks**:
- Define Payment, PaymentStatus, PaymentGateway types
- Define PaymentDto, PaymentDashboardDto types
- Define request/response types for all endpoints

#### Step 7.2: API Client
**Files**:
- `src/Frontend/web/src/lib/api.ts` (update)

**Tasks**:
- Add `PaymentsApi` object with all methods
- Implement error handling
- Add TypeScript types

---

### Phase 8: Frontend Components (Days 8-10)

**Goal**: Create frontend UI components.

#### Step 8.1: Payments Dashboard
**Files**:
- `src/Frontend/web/src/app/(protected)/payments/page.tsx`
- `src/Frontend/web/src/components/payments/PaymentsDashboard.tsx`
- `src/Frontend/web/src/components/payments/PaymentSummaryCards.tsx`
- `src/Frontend/web/src/components/payments/PaymentsTable.tsx`

**Tasks**:
- Create dashboard page with summary cards
- Implement filters (status, date range, quotation)
- Implement pagination
- Add retry/cancel buttons
- Show payment history

#### Step 8.2: Payment Modal
**Files**:
- `src/Frontend/web/src/components/payments/PaymentModal.tsx`
- `src/Frontend/web/src/components/payments/PaymentMethodSelector.tsx`
- `src/Frontend/web/src/components/payments/PaymentForm.tsx`

**Tasks**:
- Create modal component
- Show payment method selection
- Show amount breakdown (subtotal, tax, fees, total)
- Handle payment initiation
- Show loading states
- Handle success/failure
- Integrate with notification system

#### Step 8.3: Quotation Payment Section
**Files**:
- `src/Frontend/web/src/components/payments/QuotationPaymentSection.tsx`
- `src/Frontend/web/src/components/payments/PaymentStatusBadge.tsx`
- `src/Frontend/web/src/components/payments/PaymentHistory.tsx`

**Tasks**:
- Add payment section to quotation detail page
- Show payment status
- Show payment history
- Add initiate payment button
- Add refund options if applicable

#### Step 8.4: Admin Gateway Configuration
**Files**:
- `src/Frontend/web/src/app/(protected)/admin/payment-gateways/page.tsx`
- `src/Frontend/web/src/components/payments/GatewayConfigForm.tsx`
- `src/Frontend/web/src/components/payments/GatewayConfigList.tsx`

**Tasks**:
- Create admin page for gateway configuration
- CRUD interface for gateway configs
- Test API key functionality
- Enable/disable gateways
- Show gateway status

#### Step 8.5: Client Portal Payment Page
**Files**:
- `src/Frontend/web/src/app/(public)/quotation/[token]/payment/page.tsx`
- `src/Frontend/web/src/components/payments/ClientPaymentPage.tsx`

**Tasks**:
- Create client-facing payment page
- Show quotation details
- Show payment amount breakdown
- Integrate payment gateway UI
- Show payment status
- Download receipt option

---

### Phase 9: Event Handlers & Notifications (Day 10)

**Goal**: Integrate with notification system.

#### Step 9.1: Payment Event Handlers
**Files**:
- `src/Backend/CRM.Application/Payments/EventHandlers/PaymentSuccessEventHandler.cs`
- `src/Backend/CRM.Application/Payments/EventHandlers/PaymentFailedEventHandler.cs`
- `src/Backend/CRM.Application/Payments/EventHandlers/PaymentRefundedEventHandler.cs`

**Tasks**:
- Subscribe to payment domain events
- Create notifications using Spec-013 system
- Send to relevant users (client, sales rep, admin)
- Include payment details in notification

---

### Phase 10: Testing (Days 11-12)

**Goal**: Create comprehensive tests.

#### Step 10.1: Unit Tests
**Files** (in `tests/CRM.Tests/Payments/`):
- Command handler tests
- Query handler tests
- Gateway service tests
- Entity domain method tests

**Tasks**:
- Test all command handlers
- Test all query handlers
- Test payment gateway services (mocked)
- Test domain methods
- Achieve 85%+ coverage

#### Step 10.2: Integration Tests
**Files** (in `tests/CRM.Tests.Integration/Payments/`):
- `PaymentsControllerTests.cs`
- `PaymentGatewaysControllerTests.cs`
- `PaymentWebhookControllerTests.cs`

**Tasks**:
- Test all API endpoints
- Test webhook handling
- Test authorization
- Test error scenarios

#### Step 10.3: Frontend Tests
**Files** (in `src/Frontend/web/src/components/payments/__tests__/`):
- Component unit tests
- Integration tests
- E2E tests

**Tasks**:
- Test payment modal
- Test dashboard
- Test admin config
- Test client payment page
- Achieve 80%+ coverage

---

## Verification Checklist

- [ ] All database migrations created and applied
- [ ] All entities and enums created
- [ ] All commands and queries implemented
- [ ] All API endpoints functional
- [ ] Payment gateway services implemented (Stripe, Razorpay)
- [ ] Webhook handling works correctly
- [ ] Frontend components created and integrated
- [ ] Notifications integrated (Spec-013)
- [ ] All tests passing
- [ ] Documentation updated

---

## Dependencies

- Spec-009: Quotation Entity
- Spec-010: Quotation Management
- Spec-013: Notification System
- Stripe.NET SDK (NuGet)
- Razorpay SDK (NuGet)
- Encryption libraries

---

## Notes

- PCI compliance: Never store sensitive payment data (card numbers, CVV)
- Use encryption for API keys and secrets
- Implement idempotency for webhooks
- Handle gateway timeouts and retries
- Support test mode for development
- Log all payment operations for audit

