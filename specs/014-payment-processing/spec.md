# Spec-014: Payment Processing & Integration

## Overview

This spec covers integration of payment processing within the CRM system to allow clients to pay accepted quotations online securely. It supports multiple payment gateways (e.g., Stripe, Razorpay, PayPal), handles payment status updates, reconciliations, refunds, and provides sales reps and admins a dashboard to track payment status, pending payments, and failed transactions. The payment workflow is tightly coupled with quotations and must update statuses and trigger notifications accordingly.

## Project Information

- **PROJECT_NAME**: CRM Quotation Management System
- **SPEC_NUMBER**: Spec-014
- **SPEC_NAME**: Payment Processing & Integration
- **GROUP**: Payment & Finance (Group 5 of 11)
- **PRIORITY**: HIGH (Phase 2, after Quotation Lifecycle Management)
- **DEPENDENCIES**: Spec-009 (QuotationEntity), Spec-010 (QuotationManagement), Spec-013 (NotificationSystem)
- **RELATED_SPECS**: Spec-015 (Reporting & Analytics), Spec-016 (Refund & Adjustment Workflow)

## Key Features

- Initiate payments linked to accepted quotations
- Multiple payment gateway support configurable per company
- Secure client payment pages integrated from quotation
- Real-time payment status tracking (pending, success, failure)
- Automatic update of quotation/payment status on payment events
- Refund and partial refund support with reasons
- Payment retry and cancellation mechanisms
- Dashboard views for sales reps and admins to see payment statuses
- Detailed payment history with timestamps and amounts
- PCI-compliance and security considerations
- Notifications on payment success/failure/refund (Spec 13 integration)
- Support for taxes, transaction fees, discount offsets
- Transaction reconciliation and manual adjustments by finance admins

## JTBD Alignment

**Persona**: Client, Sales Rep, Finance/Admin  
**JTBD**: "I want to securely pay my accepted quotations online, and sales/finance to track payments easily"  
**Success Metric**: "Payments complete without friction; Finance reconciles accurately; Sales follow up on pending"

## Business Value

- Enables faster collections, reducing sales cycle time
- Improves cash flow visibility for business
- Enhances client experience with online payments
- Integrates payment and quotation status for operational efficiency
- Reduces manual payment tracking errors
- Enables refunds and adjustments without manual intervention
- Supports multiple payment options to maximize client convenience

## Database Schema

See `data-model.md` for detailed database schema.

## Backend Services & Commands

- `InitiatePaymentCommand` (create new payment, call gateway API)
- `UpdatePaymentStatusCommand` (update status based on webhook/response)
- `RefundPaymentCommand` (trigger partial/full refund)
- `CancelPaymentCommand`
- `GetPaymentByQuotationQuery`
- `GetPaymentsByUserQuery` (for dashboard)
- `PaymentGatewayService` (abstract interface, implement stripe/razorpay etc.)
- `WebhookHandler` (for asynchronous gateway callbacks)

All commands publish relevant domain events like `PaymentSuccess`, `PaymentFailed`, `RefundProcessed`.

## API Endpoints

1. `POST   /api/v1/payments/initiate`
2. `GET    /api/v1/payments/{paymentId}`
3. `GET    /api/v1/quotations/{quotationId}/payments`
4. `POST   /api/v1/payments/{paymentId}/refund`
5. `POST   /api/v1/payments/{paymentId}/cancel`
6. `GET    /api/v1/payments/dashboard` (dashboard data for reps/admins)
7. `POST   /api/v1/payment-gateways/config` (create/update gateway configs, admin-only)
8. `GET    /api/v1/payment-gateways/config/{companyId}`
9. `POST   /api/v1/payment-webhook/{gateway}` (public webhook for payment gateway callbacks)

## Frontend UI Components (TailAdmin Next.js Theme)

### CRUCIAL: Frontend development MANDATORY alongside backend

**SalesRep Pages:**

**SP-1: Payments Dashboard**
- Summary cards: Total Pending, Total Paid, Refunds, Failed Payments
- Search/filter payments by quotation, status, date
- List/paginated table showing payment status/details
- Retry/cancel buttons for pending/failed payments
- Detailed payment history, refund status

**SP-2: Quotation Payment Section (in quotation detail)**
- Initiate payment button (opens Payment Modal)
- Display payment status (pending, success, failed)
- Show payment history and refund options if applicable
- Link to payment portal

**SP-3: Payment Modal (Client & Sales Rep)**
- Select payment method (dynamic based on enabled gateways)
- Enter payment details or redirect to gateway-hosted page
- Real-time validation of fields
- Show estimated fees, taxes, total payable
- Confirm/Cancel buttons
- Loading spinner, success/error messages
- On success: update payment status, close modal, notify user

**Admin Pages:**

**AP-1: Payment Gateway Configuration**
- CRUD interface for payment gateways (API keys, enable/disable)
- Validation of credentials via test API call
- List of configured gateways with status
- Role-based access (Admins only)

**AP-2: Payment Reconciliation Dashboard**
- View all payments, status, gateway information
- Export payments for finance, filter by date, client, status
- Alerts for failed or disputed payments

**Client Portal Pages:**

**CP-1: Client Payment Page (embedded in quotation portal)**
- Secure payment initiation with accessible UI
- Show quotation amount and breakdown
- Redirect or iframe for payment gateway processing
- Show real-time payment status and receipt download

## UX & Design Considerations

- PCI compliance: Sensitive fields never stored on own servers
- Clear status indicators and confirmations
- Accessible forms with validation and error messages
- Mobile friendly and responsive components
- Seamless integration with existing quotation workflow
- Push real-time notifications on payment events (Spec 13)

## Test Cases

**Backend:**
- Initiate payment creates correct records and calls gateway
- Webhook updates payment status correctly
- Refund partial/full processed correctly
- Cancel payment logic works and prevents duplicate payments
- Security validations on gateway config and payment data
- Dashboard queries return accurate summaries and filters

**Frontend:**
- Payments dashboard loads and filters correctly
- Payment modal validates inputs and handles success/failure flows
- Admin config UI validates API keys and enables/disables gateways
- Client portal payment UI smooth and responsive
- Real-time notification integration functional
- E2E tests cover payment initiation, callback, status update, refund

## Deliverables

**BACKEND (30+ files):**
- Payment entities: Payment, PaymentGatewayConfig, Refund
- Commands, queries, event handlers, webhook processors
- Controllers, validators, migration scripts
- Services for integrating Stripe, Razorpay, PayPal, etc.
- Notification triggers (Spec 13 integration)
- Background job support (reconciliation)

**FRONTEND (40+ files):**
- Payments dashboard, payment modals, client portal payment UI
- Admin gateway config pages
- Custom hooks, API services, React Query support
- Validation, notifications integration
- TailAdmin component usage and styling
- Responsive/mobile-friendly designs
- Complete test coverage

## Acceptance Criteria

- Backend and frontend functionalities must be built in tandem
- Payments must be secure, reliable, and timely
- Multiple payment gateways supported and configurable
- Payment statuses updated real-time and reflected in UI
- Refunds and cancellations processed through UI and backend APIs
- Real-time notifications integrated for all payment events
- UI accessible, responsive, and intuitive
- Full unit, integration, and E2E tests with 85%+ coverage backend, 80%+ coverage frontend
- PCI compliance adhered to for sensitive data

