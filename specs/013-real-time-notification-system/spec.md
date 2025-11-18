# Spec-013: Real-Time Quotation Notification System (Sales Rep, Manager, Client)

## Overview

This spec delivers a real-time notification and alerting system across all main quotation workflows. It provides multi-channel notifications (in-app, email, configurable push/WebSocket) for all important status changes and user actions: new quotations, approval decisions, client responses, expiring/expired quotations, and reminders. It includes notification preferences, delivery guarantees, unread tracking, and alert UI. Managers and users see a unified inbox and banners, and clients get instant feedback after any critical action.

## Project Information

- **PROJECT_NAME**: CRM Quotation Management System
- **SPEC_NUMBER**: Spec-013
- **SPEC_NAME**: Real-Time Quotation Notification System (Sales Rep, Manager, Client)
- **GROUP**: Notification & Communication (Group 4 of 11)
- **PRIORITY**: CRITICAL (Phase 1, after Spec 12)
- **DEPENDENCIES**: Spec-009 (QuotationEntity), Spec-010 (QuotationManagement), Spec-012 (DiscountApprovalWorkflow)
- **RELATED_SPECS**: Spec-014 (PaymentProcessing), Spec-015 (Reporting & Analytics)

## Key Features

- In-app real-time notifications for all quotation lifecycle events (created, sent, viewed, accepted, rejected, pending approval, expired, reminders, comments)
- Configurable email alerts for all major events (user preferences)
- Optional push notifications (WebSocket-based for MVP; FCM/OneSignal for v2+)
- Unified Notification Inbox (read/unread, archive, mark as read)
- Real-time banners/toasts (success, warnings, actions needed) in UI
- Per-user notification preference management (which events, mute/unmute)
- Alert badges on sidebar, dashboard, approval pages
- Notification audit/history logged for compliance
- Delivery tracking (sent, delivered, read)
- Client portal: Immediate email/onscreen confirmation for client responses/accept/reject
- Bulk notifications (e.g., expiring quotations, pending approvals)
- Retry logic for email delivery failures, duplicate prevention
- Mobile/web responsive designs

## JTBD Alignment

### Persona: Sales Rep, Manager, Admin, Client
**JTBD**: "I want to be instantly notified about every important update, response, or required action"  
**Success Metric**: "I never miss a critical quotation event or approval; client and manager always know next step"

## Business Value

- Ensures no quotation, approval, or client response is ever missed
- Dramatically faster turnaround for approvals, reminders, and closing deals
- Audit trail for compliance and user activity
- Drives fast user engagement and fewer delays in sales processes
- Immediate client satisfaction with rapid status feedback

## Database Schema

See `data-model.md` for detailed database schema.

### Core Tables

1. **Notifications**
   - NotificationId (UUID, PK)
   - RecipientUserId (UUID, FK → Users)
   - RelatedEntityType (VARCHAR, e.g., Quotation, Approval, ClientResponse)
   - RelatedEntityId (UUID)
   - EventType (VARCHAR, e.g., SENT, VIEWED, APPROVED, REJECTED, EXPIRED, RESPONSE)
   - Message (VARCHAR(500), required)
   - IsRead (BOOLEAN, default false)
   - IsArchived (BOOLEAN, default false)
   - DeliveredChannels (VARCHAR(255), e.g., in-app,email,push)
   - DeliveryStatus (VARCHAR, SENT, DELIVERED, FAILED)
   - CreatedAt (TIMESTAMPTZ, NOT NULL)
   - ReadAt (TIMESTAMPTZ, NULLABLE)
   - ArchivedAt (TIMESTAMPTZ, NULLABLE)
   - Meta (JSONB, for extra context e.g., old/new status, discount %)

2. **NotificationPreferences**
   - UserId (UUID, PK, FK → Users)
   - PreferenceData (JSONB: per event type, channel, enabled/disabled, mute/unmute)

3. **EmailNotificationLog**
   - LogId (UUID, PK)
   - NotificationId (FK, nullable)
   - RecipientEmail (VARCHAR)
   - EventType (VARCHAR)
   - SentAt (TIMESTAMPTZ)
   - DeliveredAt (TIMESTAMPTZ, nullable)
   - Status (VARCHAR: SENT, DELIVERED, BOUNCED, FAILED, etc.)
   - ErrorMsg (TEXT, nullable)

## Domain Events

- `NotificationPublished` - When a notification is created and queued
- `NotificationRead` - When a user marks a notification as read
- `NotificationArchived` - When a user archives a notification
- `EmailNotificationSent` - When an email notification is sent
- `EmailNotificationDelivered` - When an email is confirmed delivered
- `EmailNotificationFailed` - When email delivery fails
- `UserNotificationPreferenceUpdated` - When user updates preferences

## API Endpoints

1. `GET /api/v1/notifications` - List inbox, with filters: unread, archived, date, entity
2. `POST /api/v1/notifications/mark-read` - Mark notification(s) as read ([id] or all)
3. `POST /api/v1/notifications/archive` - Archive notification(s) ([id] or all)
4. `POST /api/v1/notifications/unarchive` - Unarchive notification(s) ([id])
5. `GET /api/v1/notifications/preferences` - Get user notification preferences
6. `POST /api/v1/notifications/preferences` - Update user notification preferences
7. `GET /api/v1/notifications/unread-count` - Get unread count for dashboard badge
8. `WebSocket /ws/notifications` - Real-time push for in-app alerts
9. `GET /api/v1/notifications/entity/{entityType}/{entityId}` - Get notification history for one quotation/approval
10. `GET /api/v1/notifications/logs` - Admin/audit access; search by user/email/event/status/date

All endpoints require JWT unless event is public (e.g., client confirmation).

## Frontend UI Components

### SalesRep/Manager/Admin UI

**NF-P01: Unified Notification Inbox**
- Sidebar icon with real-time red badge ("unread count"); mobile badge shows count
- Inbox page: Table/list/grid of all notifications (message, icon, date, status, related entity)
- Filters: unread, archived, event type, date range, entity type, search text
- Actions: Mark as read, archive, mark all read, un-archive
- Live update: Animates when new notification arrives (WebSocket push)
- Click notification: Opens related quotation/approval page, marks as read

**NF-P02: Real-Time Toasts & Banners**
- Global in-app toast component (shows new notification instantly at top/bottom)
- Success (green), Info (blue), Warning (yellow), Error (red) styles
- Clicking toast leads to related entity or inbox
- Temporary banners under navbar for urgent/expiring/approval-needed (auto-hide or dismiss)
- Accessible, auto-fades or until clicked/timeout

**NF-P03: Notification Preferences Page** (`/profile/notifications`)
- Card with toggles for each event type (Quotation Sent, Viewed, Response, Approval, etc.)
- Channel per toggle: In-app, email, push (if supported)
- "Mute" option per event/channel
- Save preferences (API call)
- Summary: "How you'll be notified" preview

**NF-P04: Entity Context Badges & Timeline**
- On quotation/approval/client timeline: Show notification badges next to status/events
- Hover: See full message, sender, timestamp
- "See all related alerts" link → opens filtered inbox modal

**NF-P05: Expiration/Reminder Banner**
- Banners on expiring quotations, approval due, pending reminders
- Action button: "Send Reminder", "Acknowledge", "Dismiss"

### Client Portal UI

**CP-P03: Immediate Client Feedback**
- On accept/reject/submit: "Success" banner ("Your response was received")
- For errors (expired link, duplicate response): Clear, friendly error banner
- Responsive toast: "Our team has been notified"
- Mobile: Banners/overlays at top, push if FCM is present

**NF-P06: Public Event Confirmation Email Page**
- "Thank you for your response. Reference: Quotation #QT-XXXXX"
- Link/button: "Back to home" or "Contact Sales Rep"

### Shared/Reusable Components

- `NotificationInbox` - Inbox list/table
- `NotificationToast` / `GlobalBanner` - Toast/banner components
- `NotificationBadge` - Red dot/count badge
- `NotificationPreferenceCard` - Preferences UI
- `ContextBadge` - Badge in entity timelines
- `NotificationModal` - See details, batch actions
- `ExpiringAlertBanner` - Expiration alerts
- `ToastProvider` / context - Handles real-time push
- `useNotification` hook - Polls + WebSocket integration

## Email/Push Templates

All major events have corresponding subject/body templates (reusable across channels):

- **Quotation Created** - "New Quotation Created: {QuotationNumber}"
- **Quotation Sent** - "Quotation {QuotationNumber} Sent to {ClientName}"
- **Quotation Viewed** - "Client Viewed Quotation {QuotationNumber}"
- **Quotation Accepted** - "Quotation {QuotationNumber} Accepted by {ClientName}"
- **Quotation Rejected** - "Quotation {QuotationNumber} Rejected by {ClientName}"
- **Approval Needed** - "Discount Approval Required for Quotation {QuotationNumber}"
- **Approval Approved** - "Discount Approval Approved for Quotation {QuotationNumber}"
- **Approval Rejected** - "Discount Approval Rejected for Quotation {QuotationNumber}"
- **Quotation Expiring** - "Quotation {QuotationNumber} Expiring Soon"
- **Quotation Expired** - "Quotation {QuotationNumber} Has Expired"
- **Client Response** - "Client Response Received for Quotation {QuotationNumber}"

Placeholders: `{QuotationNumber}`, `{ClientName}`, `{UserName}`, `{Status}`, `{DiscountPercentage}`, `{ActionLink}`, `{Decision}`, `{Message}`

## Test Cases

### Backend

1. Publish notification on every major event (sent/viewed/approved/expired/etc.)
2. Logs record delivery and read/archiving
3. Notification not sent if user mutes event in preferences
4. Delivery retry after email failure; errors logged
5. WebSocket push delivers new events instantly to connected user
6. Filtering, pagination, unread, and archiving APIs work as expected
7. Client receives instant email on response (accept/reject)
8. No duplication of notifications per event per user

### Frontend

1. Toast appears instantly for new notification while on any page
2. Sidebar badge updates instantly; clears on read/mark all read
3. Inbox table filters/work; unread shows bold, archived grayed out
4. Click notification leads to related entity/view
5. Notification preferences toggle updates server; mute disables toasts
6. Expiry/reminder banner visible on expiring quotations, triggers reminder
7. Client portal shows success/error banners accordingly
8. Mobile browser renders badges/inbox/toasts correctly
9. E2E: View email, receive in-app toast, mark as read, archive, restore, see audit in admin logs

## Acceptance Criteria

✅ Real-time, reliable notifications for all major events (in-app, email, client portal)  
✅ UI badges, inbox, toasts, preferences functional & responsive  
✅ Users can manage/mark/read/archive & filter their notifications  
✅ Audit for all notification delivery/consumption steps  
✅ No ignored or duplicate notifications; all frontend workflows in sync with backend events  
✅ Accessible, fast, mobile-compatible notification UI

## Deliverables

### Backend (30+ files)
- Entities, DTOs, event publishers, emailer, WebSocket push, controllers, queries/commands, migration scripts, notification templates, audit handlers
- Tests: unit, integration, delivery/audit/e2e

### Frontend (35+ files)
- All inbox, badge, toast, banner, prefs, context, hook, and notification timeline UI
- TypeScript + TailAdmin + real-time WebSocket (for MVP)
- Test files for all major UI flows/components + E2E (Cypress or Playwright)

