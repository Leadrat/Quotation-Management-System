# Spec-010: Quotation Management (Send, Track, Status Updates)

**Project**: CRM Quotation Management System  
**Spec Number**: Spec-010  
**Spec Name**: Quotation Management (Send, Track, Status Updates)  
**Group**: Quotation Management (Group 3 of 11)  
**Priority**: CRITICAL (Phase 1, after Spec-009)  
**Status**: Draft  
**Date**: 2025-11-15

## Dependencies

- **Spec-009**: Quotation Entity & CRUD Operations (REQUIRED)
- **Spec-003**: User Authentication & JWT (REQUIRED)
- **Related Specs**: Spec-011 (Template Management), Spec-012 (Approval Workflow), Spec-013 (Payment Processing)

## Overview

This specification defines the Quotation Management workflow including sending quotations to clients via email, tracking quotation status and client interactions (viewed, accepted, rejected), generating PDF files, managing quotation access links for clients, and updating quotation status as client responds. This enables the complete quotation lifecycle from draft to final outcome.

## Business Value

- **Increases quotation conversion rate** through timely follow-ups and client engagement tracking
- **Provides visibility into client engagement** (viewed vs not viewed, response rates)
- **Enables client self-service** (no need to call for quotation details)
- **Creates audit trail** of client responses for compliance
- **Allows bulk sending** (send to multiple clients at once)
- **Professional communication** improves brand image

## User Stories

### US-1: Sales Rep Sends Quotation to Client
**As a** Sales Representative  
**I want to** send a professional quotation via email to my client  
**So that** they receive it immediately and can review it at their convenience

**Acceptance Criteria:**
- Can send quotation only if status is DRAFT
- Email includes PDF attachment and secure access link
- Can CC/BCC team members
- Can add custom message to email
- Quotation status automatically changes to SENT
- Access link is generated and included in email
- Sales rep receives confirmation when email is sent

### US-2: Track Quotation Views
**As a** Sales Representative  
**I want to** know when my client views the quotation  
**So that** I can follow up at the right time

**Acceptance Criteria:**
- System tracks when client opens access link
- View count is incremented on each view
- First view and last view timestamps are recorded
- IP address is captured for security
- Quotation status changes from SENT to VIEWED on first view
- Sales rep can see view history in analytics

### US-3: Client Views Quotation
**As a** Client  
**I want to** view my quotation online without logging in  
**So that** I can review it quickly and easily

**Acceptance Criteria:**
- Can access quotation via secure link in email
- No authentication required
- Professional quotation display matching PDF
- Can download PDF version
- Page loads quickly (<2 seconds)
- Mobile responsive design

### US-4: Client Responds to Quotation
**As a** Client  
**I want to** accept or reject the quotation online  
**So that** I can provide quick feedback without emailing back

**Acceptance Criteria:**
- Can accept, reject, or request modification
- Can add optional message with response
- Response is immediately recorded
- Sales rep receives notification email
- Quotation status updates automatically
- Cannot respond twice to same quotation

### US-5: Sales Rep Views Quotation Status & History
**As a** Sales Representative  
**I want to** see the complete status history of my quotation  
**So that** I understand the client's engagement level

**Acceptance Criteria:**
- Can see timeline of all status changes
- Can see view count and last viewed time
- Can see client response (if any)
- Can see when email was sent
- Can see access link details
- Can download PDF at any time

### US-6: Automatic Expiration Management
**As a** System  
**I want to** automatically mark expired quotations  
**So that** sales reps know which quotations are no longer valid

**Acceptance Criteria:**
- Background job runs daily to check expiration
- Quotations past ValidUntil date are marked EXPIRED
- Status history records expiration
- Sales rep can be notified of expiration
- Expired quotations cannot be accepted

### US-7: Resend Quotation
**As a** Sales Representative  
**I want to** resend a quotation to the same or different email  
**So that** I can follow up or correct email address

**Acceptance Criteria:**
- Can resend to original recipient or new email
- New access link is generated
- Status history records resend action
- Email is sent with same PDF and format

## Key Entities

### QuotationAccessLink
Stores secure access tokens for clients to view quotations without authentication.

**Properties:**
- `AccessLinkId` (UUID, PK)
- `QuotationId` (UUID, FK → Quotations)
- `ClientEmail` (VARCHAR(255))
- `AccessToken` (VARCHAR(500), UNIQUE) - Secure random token
- `IsActive` (BOOLEAN, default: true)
- `CreatedAt` (TIMESTAMPTZ)
- `ExpiresAt` (TIMESTAMPTZ, nullable)
- `SentAt` (TIMESTAMPTZ, nullable)
- `FirstViewedAt` (TIMESTAMPTZ, nullable)
- `LastViewedAt` (TIMESTAMPTZ, nullable)
- `ViewCount` (INT, default: 0)
- `IpAddress` (VARCHAR(50), nullable)

### QuotationStatusHistory
Immutable log of all quotation status transitions.

**Properties:**
- `HistoryId` (UUID, PK)
- `QuotationId` (UUID, FK → Quotations)
- `PreviousStatus` (VARCHAR(50), nullable)
- `NewStatus` (VARCHAR(50))
- `ChangedByUserId` (UUID, FK → Users, nullable)
- `Reason` (VARCHAR(500), nullable)
- `ChangedAt` (TIMESTAMPTZ)
- `IpAddress` (VARCHAR(50), nullable)

### QuotationResponse
Stores client's response to quotation.

**Properties:**
- `ResponseId` (UUID, PK)
- `QuotationId` (UUID, FK → Quotations, UNIQUE)
- `ResponseType` (VARCHAR(50)) - ACCEPTED, REJECTED, NEEDS_MODIFICATION
- `ClientEmail` (VARCHAR(255))
- `ClientName` (VARCHAR(255), nullable)
- `ResponseMessage` (TEXT, nullable, max: 2000)
- `ResponseDate` (TIMESTAMPTZ)
- `IpAddress` (VARCHAR(50), nullable)
- `UserAgent` (TEXT, nullable)
- `NotifiedAdminAt` (TIMESTAMPTZ, nullable)

## Commands

### SendQuotationCommand
Sends quotation via email to client.

**Input:**
- `QuotationId` (Guid, required)
- `RecipientEmail` (string, required)
- `CcEmails` (List<string>, optional)
- `BccEmails` (List<string>, optional)
- `CustomMessage` (string, optional)
- `SentByUserId` (Guid) - from JWT

**Validation:**
- Quotation exists and Status = DRAFT
- RecipientEmail is valid format
- Quotation has line items
- CcEmails/BccEmails are valid formats

**Implementation:**
1. Verify quotation status is DRAFT
2. Generate secure access link (QuotationAccessLink)
3. Generate PDF of quotation
4. Send email with PDF attachment and access link
5. Update Quotation.Status = SENT
6. Create QuotationStatusHistory record
7. Emit QuotationSent event

### MarkQuotationAsViewedCommand
Tracks when client opens access link.

**Input:**
- `QuotationId` (Guid)
- `AccessToken` (string)
- `IpAddress` (string)

**Implementation:**
1. Verify QuotationAccessLink exists with matching token
2. If Status = SENT, change to VIEWED
3. Update QuotationAccessLink (FirstViewedAt, LastViewedAt, ViewCount, IpAddress)
4. Create QuotationStatusHistory record
5. Emit QuotationViewed event

### SubmitQuotationResponseCommand
Records client's accept/reject response.

**Input:**
- `QuotationId` (Guid)
- `AccessToken` (string)
- `ResponseType` (string: ACCEPTED, REJECTED, NEEDS_MODIFICATION)
- `ClientName` (string, optional)
- `ResponseMessage` (string, optional)
- `IpAddress` (string)

**Implementation:**
1. Verify QuotationAccessLink exists
2. Verify quotation not already responded
3. Create QuotationResponse record
4. Update Quotation status based on response
5. Create QuotationStatusHistory record
6. Send notification email to sales rep
7. Emit QuotationResponseReceived event

### MarkQuotationAsExpiredCommand
Marks quotation as expired (automatic or manual).

**Input:**
- `QuotationId` (Guid)
- `ExpiredByUserId` (Guid, optional) - null if system

**Implementation:**
1. Load quotation
2. Verify Status is not ACCEPTED/REJECTED
3. If ValidUntil < today, set Status = EXPIRED
4. Create QuotationStatusHistory record
5. Emit QuotationExpired event

### ResendQuotationCommand
Resends quotation to same or different email.

**Input:**
- `QuotationId` (Guid)
- `NewRecipientEmail` (string, optional)
- `ResendByUserId` (Guid)

**Implementation:**
1. Load quotation and verify Status is SENT/VIEWED/REJECTED
2. Generate new access link (new token or new email)
3. Send email (same as SendQuotationCommand)
4. Create QuotationStatusHistory: "Resent on {date}"
5. Emit QuotationResent event

## Queries

### GetQuotationStatusHistoryQuery
Returns timeline of status changes for a quotation.

**Input:** `QuotationId` (Guid)  
**Output:** `List<QuotationStatusHistoryDto>` (ordered by date DESC)

### GetQuotationResponseQuery
Returns client response (if exists).

**Input:** `QuotationId` (Guid)  
**Output:** `QuotationResponseDto` or null

### GetQuotationAccessLinkQuery
Returns access link info for sales rep to share.

**Input:** `QuotationId` (Guid)  
**Output:** `QuotationAccessLinkDto`

### GetQuotationByAccessTokenQuery
Retrieves quotation for unauthenticated client portal.

**Input:** `AccessToken` (string)  
**Output:** `QuotationDto` (public, minimal fields)

## Services

### QuotationPdfGenerationService
Generates professional PDF of quotation.

**Method:** `GenerateQuotationPdf(Quotation quotation) → byte[]`

**Implementation:**
- Use library: QuestPDF, iTextSharp, or SelectPdf
- PDF layout: Header (logo, company info), Quotation details, Client section, Line items table, Summary (subtotal, discount, tax, total), Footer (terms, contact info)
- Styling: Professional, brand colors (white + forest green)
- Optional: QR code linking to quotation view link
- Font: Professional sans-serif (Arial, Helvetica)
- Page size: A4, margins: 1 inch all sides

### EmailService
Sends quotation emails with templates.

**Templates:**
1. **QuotationSentEmail.html** - To client with PDF and access link
2. **QuotationAcceptedNotification.html** - To sales rep when client accepts
3. **QuotationRejectedNotification.html** - To sales rep when client rejects
4. **QuotationExpirationReminder.html** - To sales rep for unviewed quotations

## API Endpoints

### Sales Rep Endpoints (Authenticated)

1. **POST** `/api/v1/quotations/{quotationId}/send` - Send quotation via email
2. **POST** `/api/v1/quotations/{quotationId}/resend` - Resend quotation
3. **GET** `/api/v1/quotations/{quotationId}/status-history` - Get status history
4. **GET** `/api/v1/quotations/{quotationId}/response` - Get client response
5. **GET** `/api/v1/quotations/{quotationId}/download-pdf` - Download PDF

### Client Portal Endpoints (Public, No Auth)

6. **GET** `/api/v1/client-portal/quotations/{quotationId}/{accessToken}` - View quotation
7. **POST** `/api/v1/client-portal/quotations/{quotationId}/{accessToken}/respond` - Submit response

## Frontend UI Components

### Sales Rep Side (TailAdmin Next.js)

- **SR-P17**: Quotation Detail Page Extended (with send functionality)
- **SR-P18**: Send Quotation Modal (reusable component)
- **SR-P19**: Resend Quotation Modal
- **SR-P20**: Quotation Analytics/Tracking Page

### Client Portal (Public, No Auth)

- **C-P05**: Client Quotation View Page (public portal)
- **C-P06**: Client Response Form Modal

### Admin Pages

- **A-P13**: Quotation Tracking Dashboard

## Background Jobs

1. **QuotationExpirationCheckJob** - Daily at midnight, marks expired quotations
2. **UnviewedQuotationReminderJob** - Daily at 9 AM, reminds sales rep of unviewed quotations
3. **PendingResponseFollowUpJob** - Daily at 3 PM, reminds sales rep of pending responses

## Domain Events

1. **QuotationSent** - When quotation is sent via email
2. **QuotationViewed** - When client opens access link
3. **QuotationResponseReceived** - When client accepts/rejects
4. **QuotationExpired** - When quotation expires
5. **QuotationResent** - When quotation is resent

## Success Criteria

1. Sales rep can send quotation in <1 minute
2. Client can view quotation in <30 seconds
3. Client can respond in <2 minutes
4. PDF generation completes in <5 seconds
5. Email delivery within 30 seconds
6. Status updates are real-time
7. Access links are secure and cannot be guessed
8. Client portal works without authentication
9. All status transitions are logged
10. Background jobs run reliably

## Assumptions

1. Email service (SMTP/SendGrid/SES) is configured
2. PDF generation library is available
3. Base URL for client portal is configured
4. Company branding assets (logo) are available
5. Email templates can be customized
6. Background job scheduler (Quartz/Hangfire) is configured

## Technical Constraints

1. Access tokens must be cryptographically secure (32+ characters)
2. Access links expire based on ExpiresAt field
3. Only one response per quotation allowed
4. PDF must be regenerated if quotation is updated
5. Email sending must be async to avoid blocking
6. Client portal must be public (no authentication)
7. Status transitions must be logged immutably

## Security Considerations

1. Access tokens must be unique and unguessable
2. Client portal must validate token before showing data
3. No internal notes or sensitive data in client portal
4. IP address tracking for fraud detection
5. Rate limiting on client portal endpoints
6. Expired links must be rejected
7. Revoked links (IsActive=false) must be rejected

## Performance Goals

- Send quotation: <1 minute
- PDF generation: <5 seconds
- Email delivery: <30 seconds
- Client portal load: <2 seconds
- Status update: Real-time (<1 second)

## Testing Requirements

- Unit tests for all commands and queries
- Integration tests for API endpoints
- E2E tests for complete send → view → respond workflow
- Security tests for access token validation
- Performance tests for PDF generation
- Email delivery tests (mock email service)

