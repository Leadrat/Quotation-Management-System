# Implementation Plan: Spec-013 Real-Time Notification System

**Spec**: Spec-013  
**Last Updated**: 2025-01-XX

## Overview

This plan outlines the phased implementation of the Real-Time Quotation Notification System, building on Spec-009 (Quotation Entity), Spec-010 (Quotation Management), and Spec-012 (Discount Approval Workflow). The system provides multi-channel notifications (in-app, email, WebSocket) for all quotation lifecycle events.

---

## Implementation Phases

### Phase 1: Setup & Foundational (Days 1-3)

**Goal**: Establish database schema, entities, enums, and basic infrastructure.

#### Step 1.1: Database Migrations
**Files**: 
- `src/Backend/CRM.Infrastructure/Migrations/YYYYMMDDHHMMSS_CreateNotificationsTable.cs`
- `src/Backend/CRM.Infrastructure/Migrations/YYYYMMDDHHMMSS_CreateNotificationPreferencesTable.cs`
- `src/Backend/CRM.Infrastructure/Migrations/YYYYMMDDHHMMSS_CreateEmailNotificationLogTable.cs`

**Tasks**:
- Create `Notifications` table with 13 columns (NotificationId, RecipientUserId, RelatedEntityType, RelatedEntityId, EventType, Message, IsRead, IsArchived, DeliveredChannels, DeliveryStatus, CreatedAt, ReadAt, ArchivedAt, Meta)
- Create `NotificationPreferences` table with 4 columns (UserId, PreferenceData, CreatedAt, UpdatedAt)
- Create `EmailNotificationLog` table with 10 columns (LogId, NotificationId, RecipientEmail, EventType, Subject, SentAt, DeliveredAt, Status, ErrorMsg, RetryCount, LastRetryAt)
- Add foreign keys (RecipientUserId → Users, NotificationId → Notifications)
- Add all indexes (RecipientUserId, IsRead, IsArchived, RelatedEntityType+RelatedEntityId, DeliveryStatus, CreatedAt, Unread composite index)
- Add JSONB indexes for PreferenceData queries

**Verification**:
```sql
SELECT table_name FROM information_schema.tables 
WHERE table_name IN ('Notifications', 'NotificationPreferences', 'EmailNotificationLog');
```

#### Step 1.2: Domain Entities
**Files**:
- `src/Backend/CRM.Domain/Entities/Notification.cs`
- `src/Backend/CRM.Domain/Entities/NotificationPreference.cs`
- `src/Backend/CRM.Domain/Entities/EmailNotificationLog.cs`
- `src/Backend/CRM.Domain/Enums/NotificationEventType.cs`
- `src/Backend/CRM.Domain/Enums/NotificationDeliveryStatus.cs`
- `src/Backend/CRM.Domain/Enums/EmailNotificationStatus.cs`
- `src/Backend/CRM.Domain/Enums/NotificationChannel.cs`

**Tasks**:
- Create `Notification` entity with all 13 properties
- Add navigation property `RecipientUser`
- Add domain methods: `MarkAsRead()`, `Archive()`, `Unarchive()`, `IsUnread()`
- Create `NotificationPreference` entity with JSONB PreferenceData
- Add navigation property `User`
- Create `EmailNotificationLog` entity with all 10 properties
- Add navigation property `Notification`
- Create all 4 enums with appropriate values

#### Step 1.3: Entity Framework Configuration
**Files**:
- `src/Backend/CRM.Infrastructure/EntityConfigurations/NotificationEntityConfiguration.cs`
- `src/Backend/CRM.Infrastructure/EntityConfigurations/NotificationPreferenceEntityConfiguration.cs`
- `src/Backend/CRM.Infrastructure/EntityConfigurations/EmailNotificationLogEntityConfiguration.cs`

**Tasks**:
- Configure table names, primary keys, property constraints
- Configure JSONB column for PreferenceData and Meta
- Configure enum to string conversions
- Configure relationships and foreign keys
- Configure all indexes (including composite and partial indexes)
- Configure cascade delete behavior

#### Step 1.4: Update DbContext
**Files**:
- `src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs`
- `src/Backend/CRM.Application/Common/Persistence/IAppDbContext.cs`

**Tasks**:
- Add `DbSet<Notification> Notifications`
- Add `DbSet<NotificationPreference> NotificationPreferences`
- Add `DbSet<EmailNotificationLog> EmailNotificationLogs`
- Update interface with same properties

#### Step 1.5: DTOs
**Files** (in `src/Backend/CRM.Application/Notifications/Dtos/`):
- `NotificationDto.cs`
- `CreateNotificationRequest.cs`
- `MarkNotificationsReadRequest.cs`
- `ArchiveNotificationsRequest.cs`
- `UnarchiveNotificationsRequest.cs`
- `NotificationPreferencesDto.cs`
- `UpdateNotificationPreferencesRequest.cs`
- `UnreadCountDto.cs`
- `EmailNotificationLogDto.cs`
- `PagedNotificationsResult.cs`

**Tasks**:
- Create all 10 DTO classes with proper properties
- Add validation attributes where needed
- Include computed properties (e.g., formatted dates, entity links)

#### Step 1.6: AutoMapper Profile
**File**: `src/Backend/CRM.Application/Mapping/NotificationProfile.cs`

**Tasks**:
- Map Notification → NotificationDto
- Map NotificationPreference → NotificationPreferencesDto
- Map EmailNotificationLog → EmailNotificationLogDto
- Resolve User names from navigation properties
- Map JSONB PreferenceData to structured DTO

---

### Phase 2: Notification Publishing Infrastructure (Days 4-6)

**Goal**: Create services and event handlers for publishing notifications.

#### Step 2.1: Notification Service
**File**: `src/Backend/CRM.Application/Notifications/Services/INotificationService.cs`
**File**: `src/Backend/CRM.Application/Notifications/Services/NotificationService.cs`

**Tasks**:
- Create interface with methods: `PublishNotificationAsync()`, `PublishBulkNotificationsAsync()`, `GetUserPreferencesAsync()`, `ShouldSendNotificationAsync()`
- Implement notification creation logic
- Check user preferences before sending
- Support multiple channels (in-app, email, push)
- Handle duplicate prevention
- Return notification ID for tracking

#### Step 2.2: Email Notification Service
**File**: `src/Backend/CRM.Application/Notifications/Services/IEmailNotificationService.cs`
**File**: `src/Backend/CRM.Application/Notifications/Services/EmailNotificationService.cs`

**Tasks**:
- Create interface with methods: `SendEmailNotificationAsync()`, `RetryFailedEmailsAsync()`, `LogEmailDeliveryAsync()`
- Integrate with existing FluentEmail infrastructure
- Load email templates for each event type
- Replace placeholders (QuotationNumber, ClientName, UserName, etc.)
- Handle delivery status tracking
- Implement retry logic for failed emails
- Log all email attempts to EmailNotificationLog

#### Step 2.3: Notification Templates
**Files** (in `src/Backend/CRM.Application/Notifications/Templates/`):
- `QuotationCreatedTemplate.cs`
- `QuotationSentTemplate.cs`
- `QuotationViewedTemplate.cs`
- `QuotationAcceptedTemplate.cs`
- `QuotationRejectedTemplate.cs`
- `ApprovalNeededTemplate.cs`
- `ApprovalApprovedTemplate.cs`
- `ApprovalRejectedTemplate.cs`
- `QuotationExpiringTemplate.cs`
- `QuotationExpiredTemplate.cs`
- `ClientResponseTemplate.cs`

**Tasks**:
- Create template classes for each event type
- Define subject and body templates with placeholders
- Support HTML and plain text formats
- Include action links (e.g., "View Quotation", "Approve Request")

#### Step 2.4: Domain Events
**Files**:
- `src/Backend/CRM.Domain/Events/NotificationPublished.cs`
- `src/Backend/CRM.Domain/Events/NotificationRead.cs`
- `src/Backend/CRM.Domain/Events/NotificationArchived.cs`
- `src/Backend/CRM.Domain/Events/EmailNotificationSent.cs`
- `src/Backend/CRM.Domain/Events/EmailNotificationDelivered.cs`
- `src/Backend/CRM.Domain/Events/EmailNotificationFailed.cs`
- `src/Backend/CRM.Domain/Events/UserNotificationPreferenceUpdated.cs`

**Tasks**:
- Create all 8 domain event classes
- Include relevant data (NotificationId, UserId, EventType, etc.)
- Add timestamps and metadata

#### Step 2.5: Event Handlers for Publishing
**Files** (in `src/Backend/CRM.Application/Notifications/EventHandlers/`):
- `QuotationSentEventHandler.cs` (publish notification when quotation sent)
- `QuotationViewedEventHandler.cs` (publish notification when quotation viewed)
- `QuotationAcceptedEventHandler.cs` (publish notification when quotation accepted)
- `QuotationRejectedEventHandler.cs` (publish notification when quotation rejected)
- `DiscountApprovalRequestedEventHandler.cs` (update to publish notification)
- `DiscountApprovalApprovedEventHandler.cs` (update to publish notification)
- `DiscountApprovalRejectedEventHandler.cs` (update to publish notification)
- `QuotationExpiringEventHandler.cs` (publish reminder notifications)

**Tasks**:
- Subscribe to existing domain events (from Spec-010, Spec-012)
- Create new event handlers for notification publishing
- Determine recipients based on event type
- Call NotificationService to publish notifications
- Handle errors gracefully (log, don't fail main workflow)

#### Step 2.6: Background Job for Expiring Quotations
**File**: `src/Backend/CRM.Infrastructure/Jobs/QuotationExpirationNotificationJob.cs`

**Tasks**:
- Create cron job (runs daily, e.g., at 9 AM)
- Find quotations expiring in next 24-48 hours
- Publish notifications to sales rep and manager
- Handle bulk notification publishing
- Log job execution

---

### Phase 3: Backend Commands (Days 7-9)

**Goal**: Implement commands for notification management.

#### Step 3.1: Mark Notifications Read Command
**Files**:
- `src/Backend/CRM.Application/Notifications/Commands/MarkNotificationsReadCommand.cs`
- `src/Backend/CRM.Application/Notifications/Commands/Handlers/MarkNotificationsReadCommandHandler.cs`
- `src/Backend/CRM.Application/Notifications/Validators/MarkNotificationsReadCommandValidator.cs`

**Tasks**:
- Create command with NotificationIds array (empty = mark all)
- Validate user owns notifications
- Update IsRead = true, ReadAt = now
- Publish NotificationRead event
- Return success result

#### Step 3.2: Archive Notifications Command
**Files**:
- `src/Backend/CRM.Application/Notifications/Commands/ArchiveNotificationsCommand.cs`
- `src/Backend/CRM.Application/Notifications/Commands/Handlers/ArchiveNotificationsCommandHandler.cs`
- `src/Backend/CRM.Application/Notifications/Validators/ArchiveNotificationsCommandValidator.cs`

**Tasks**:
- Create command with NotificationIds array (empty = archive all)
- Validate user owns notifications
- Update IsArchived = true, ArchivedAt = now
- Publish NotificationArchived event
- Return success result

#### Step 3.3: Unarchive Notifications Command
**Files**:
- `src/Backend/CRM.Application/Notifications/Commands/UnarchiveNotificationsCommand.cs`
- `src/Backend/CRM.Application/Notifications/Commands/Handlers/UnarchiveNotificationsCommandHandler.cs`
- `src/Backend/CRM.Application/Notifications/Validators/UnarchiveNotificationsCommandValidator.cs`

**Tasks**:
- Create command with NotificationIds array
- Validate user owns notifications
- Update IsArchived = false, ArchivedAt = null
- Return success result

#### Step 3.4: Update Notification Preferences Command
**Files**:
- `src/Backend/CRM.Application/Notifications/Commands/UpdateNotificationPreferencesCommand.cs`
- `src/Backend/CRM.Application/Notifications/Commands/Handlers/UpdateNotificationPreferencesCommandHandler.cs`
- `src/Backend/CRM.Application/Notifications/Validators/UpdateNotificationPreferencesCommandValidator.cs`

**Tasks**:
- Create command with Preferences JSONB data
- Validate preference structure (event types, channels, boolean values)
- Create or update NotificationPreference record
- Publish UserNotificationPreferenceUpdated event
- Return updated preferences DTO

---

### Phase 4: Backend Queries (Days 10-12)

**Goal**: Implement queries for retrieving notifications.

#### Step 4.1: Get Notifications Query
**Files**:
- `src/Backend/CRM.Application/Notifications/Queries/GetNotificationsQuery.cs`
- `src/Backend/CRM.Application/Notifications/Queries/Handlers/GetNotificationsQueryHandler.cs`
- `src/Backend/CRM.Application/Notifications/Validators/GetNotificationsQueryValidator.cs`

**Tasks**:
- Create query with filters: unread, archived, eventType, entityType, entityId, dateFrom, dateTo, pageNumber, pageSize
- Filter by RecipientUserId (current user)
- Apply all filters with proper SQL
- Include navigation properties (RecipientUser)
- Return paginated result with NotificationDto array

#### Step 4.2: Get Unread Count Query
**Files**:
- `src/Backend/CRM.Application/Notifications/Queries/GetUnreadCountQuery.cs`
- `src/Backend/CRM.Application/Notifications/Queries/Handlers/GetUnreadCountQueryHandler.cs`
- `src/Backend/CRM.Application/Notifications/Validators/GetUnreadCountQueryValidator.cs`

**Tasks**:
- Create query (no parameters, uses current user)
- Count unread notifications (IsRead = false, IsArchived = false)
- Return count as integer

#### Step 4.3: Get Notification Preferences Query
**Files**:
- `src/Backend/CRM.Application/Notifications/Queries/GetNotificationPreferencesQuery.cs`
- `src/Backend/CRM.Application/Notifications/Queries/Handlers/GetNotificationPreferencesQueryHandler.cs`
- `src/Backend/CRM.Application/Notifications/Validators/GetNotificationPreferencesQueryValidator.cs`

**Tasks**:
- Create query (no parameters, uses current user)
- Load NotificationPreference or return defaults
- Parse JSONB PreferenceData to DTO
- Return NotificationPreferencesDto

#### Step 4.4: Get Entity Notifications Query
**Files**:
- `src/Backend/CRM.Application/Notifications/Queries/GetEntityNotificationsQuery.cs`
- `src/Backend/CRM.Application/Notifications/Queries/Handlers/GetEntityNotificationsQueryHandler.cs`
- `src/Backend/CRM.Application/Notifications/Validators/GetEntityNotificationsQueryValidator.cs`

**Tasks**:
- Create query with EntityType and EntityId
- Filter by RelatedEntityType and RelatedEntityId
- Filter by RecipientUserId (current user)
- Return NotificationDto array (chronological order)

#### Step 4.5: Get Email Notification Logs Query (Admin)
**Files**:
- `src/Backend/CRM.Application/Notifications/Queries/GetEmailNotificationLogsQuery.cs`
- `src/Backend/CRM.Application/Notifications/Queries/Handlers/GetEmailNotificationLogsQueryHandler.cs`
- `src/Backend/CRM.Application/Notifications/Validators/GetEmailNotificationLogsQueryValidator.cs`

**Tasks**:
- Create query with filters: userId, recipientEmail, eventType, status, dateFrom, dateTo, pageNumber, pageSize
- Require Admin role
- Apply all filters
- Return paginated EmailNotificationLogDto array

---

### Phase 5: API Endpoints (Days 13-15)

**Goal**: Create REST API endpoints for notification management.

#### Step 5.1: Notifications Controller
**File**: `src/Backend/CRM.Api/Controllers/NotificationsController.cs`

**Tasks**:
- Create controller with route `/api/v1/notifications`
- Add `[Authorize]` attribute
- Implement GET `/notifications` (calls GetNotificationsQuery)
- Implement POST `/notifications/mark-read` (calls MarkNotificationsReadCommand)
- Implement POST `/notifications/archive` (calls ArchiveNotificationsCommand)
- Implement POST `/notifications/unarchive` (calls UnarchiveNotificationsCommand)
- Implement GET `/notifications/preferences` (calls GetNotificationPreferencesQuery)
- Implement POST `/notifications/preferences` (calls UpdateNotificationPreferencesCommand)
- Implement GET `/notifications/unread-count` (calls GetUnreadCountQuery)
- Implement GET `/notifications/entity/{entityType}/{entityId}` (calls GetEntityNotificationsQuery)
- Implement GET `/notifications/logs` (calls GetEmailNotificationLogsQuery, Admin only)
- Add proper error handling and validation
- Return consistent API response format

#### Step 5.2: Register Services in Program.cs
**File**: `src/Backend/CRM.Api/Program.cs`

**Tasks**:
- Register NotificationService, EmailNotificationService
- Register all command handlers
- Register all query handlers
- Register all validators
- Register all event handlers
- Register QuotationExpirationNotificationJob

---

### Phase 6: WebSocket/Real-Time Infrastructure (Days 16-18)

**Goal**: Implement real-time push notifications via WebSocket.

#### Step 6.1: WebSocket Hub
**File**: `src/Backend/CRM.Api/Hubs/NotificationHub.cs`

**Tasks**:
- Create SignalR hub for notifications
- Implement `OnConnectedAsync()` - authenticate user, add to group
- Implement `OnDisconnectedAsync()` - remove from group
- Create method `SendNotificationToUser()` - send notification to specific user
- Create method `SendNotificationToGroup()` - send to role-based group
- Handle connection management (user groups, connection tracking)

#### Step 6.2: WebSocket Service Integration
**File**: `src/Backend/CRM.Application/Notifications/Services/IRealTimeNotificationService.cs`
**File**: `src/Backend/CRM.Application/Notifications/Services/RealTimeNotificationService.cs`

**Tasks**:
- Create interface with method `SendToUserAsync()`
- Implement SignalR hub context injection
- Send notification DTO to connected clients
- Handle disconnected users gracefully
- Log WebSocket delivery attempts

#### Step 6.3: Update Notification Service
**File**: `src/Backend/CRM.Application/Notifications/Services/NotificationService.cs`

**Tasks**:
- Inject IRealTimeNotificationService
- Call real-time service after creating in-app notification
- Handle WebSocket failures (log, don't fail main flow)

#### Step 6.4: Register SignalR in Program.cs
**File**: `src/Backend/CRM.Api/Program.cs`

**Tasks**:
- Add SignalR services: `builder.Services.AddSignalR()`
- Map hub: `app.MapHub<NotificationHub>("/ws/notifications")`
- Configure CORS for WebSocket connections
- Add authentication for SignalR

---

### Phase 7: Frontend API Integration (Days 19-20)

**Goal**: Create TypeScript API client and types.

#### Step 7.1: TypeScript Types
**File**: `src/Frontend/web/src/types/notifications.ts`

**Tasks**:
- Create interfaces: Notification, NotificationPreferences, EmailNotificationLog
- Create request types: MarkNotificationsReadRequest, ArchiveNotificationsRequest, UpdateNotificationPreferencesRequest
- Create response types: PagedNotificationsResult, UnreadCountResponse
- Match backend DTOs exactly

#### Step 7.2: API Client
**File**: `src/Frontend/web/src/lib/api.ts`

**Tasks**:
- Add `NotificationsApi` object with methods:
  - `getAll(params)` - GET /notifications
  - `markRead(request)` - POST /notifications/mark-read
  - `archive(request)` - POST /notifications/archive
  - `unarchive(request)` - POST /notifications/unarchive
  - `getPreferences()` - GET /notifications/preferences
  - `updatePreferences(request)` - POST /notifications/preferences
  - `getUnreadCount()` - GET /notifications/unread-count
  - `getEntityNotifications(entityType, entityId)` - GET /notifications/entity/{type}/{id}
  - `getLogs(params)` - GET /notifications/logs (admin)

#### Step 7.3: WebSocket Client Hook
**File**: `src/Frontend/web/src/hooks/useNotificationWebSocket.ts`

**Tasks**:
- Create React hook using SignalR client
- Connect to `/ws/notifications` on mount
- Handle authentication token
- Listen for `ReceiveNotification` event
- Return connection status and notification callback
- Handle reconnection logic

---

### Phase 8: Frontend Core Components (Days 21-25)

**Goal**: Build reusable notification UI components.

#### Step 8.1: Notification Badge Component
**File**: `src/Frontend/web/src/components/notifications/NotificationBadge.tsx`

**Tasks**:
- Create badge component showing unread count
- Red dot with count (hide if 0)
- Animate on count change
- Accessible (ARIA labels)
- Responsive (mobile/desktop)

#### Step 8.2: Notification Toast Component
**File**: `src/Frontend/web/src/components/notifications/NotificationToast.tsx`

**Tasks**:
- Create toast component (success, info, warning, error styles)
- Show notification message, icon, timestamp
- Click handler to navigate to related entity
- Auto-dismiss after timeout (configurable)
- Manual dismiss button
- Stack multiple toasts
- Accessible (keyboard navigation, screen reader)

#### Step 8.3: Global Toast Provider
**File**: `src/Frontend/web/src/components/notifications/ToastProvider.tsx`
**File**: `src/Frontend/web/src/components/notifications/useToast.ts`

**Tasks**:
- Create context provider for global toast state
- Create hook `useToast()` for showing toasts
- Integrate with WebSocket hook
- Auto-show toast when notification received
- Manage toast queue (max 5 visible)

#### Step 8.4: Notification Inbox Component
**File**: `src/Frontend/web/src/components/notifications/NotificationInbox.tsx`

**Tasks**:
- Create table/list view of notifications
- Show message, icon, date, status, related entity link
- Unread styling (bold, different background)
- Archived styling (grayed out)
- Click notification → navigate to entity, mark as read
- Loading skeleton
- Empty state

#### Step 8.5: Notification Filters Component
**File**: `src/Frontend/web/src/components/notifications/NotificationFilters.tsx`

**Tasks**:
- Create filter UI: unread toggle, archived toggle, event type dropdown, entity type dropdown, date range picker, search text
- Apply filters to inbox
- Clear filters button
- Persist filters in URL query params

#### Step 8.6: Notification Actions Component
**File**: `src/Frontend/web/src/components/notifications/NotificationActions.tsx`

**Tasks**:
- Create action buttons: Mark as read, Archive, Mark all read, Unarchive
- Batch selection (checkboxes)
- Bulk actions
- Confirmation dialogs for bulk actions

#### Step 8.7: Notification Preferences Component
**File**: `src/Frontend/web/src/components/notifications/NotificationPreferences.tsx`

**Tasks**:
- Create card with toggles for each event type
- Per-event: In-app toggle, Email toggle, Push toggle, Mute toggle
- Save button (calls API)
- Loading state
- Success/error feedback
- Default preferences shown

#### Step 8.8: Expiring Alert Banner Component
**File**: `src/Frontend/web/src/components/notifications/ExpiringAlertBanner.tsx`

**Tasks**:
- Create banner component for expiring quotations
- Show warning message, quotation number, days until expiry
- Action buttons: "Send Reminder", "Acknowledge", "Dismiss"
- Auto-hide after acknowledge/dismiss
- Accessible

#### Step 8.9: Context Badge Component
**File**: `src/Frontend/web/src/components/notifications/ContextBadge.tsx`

**Tasks**:
- Create badge for entity timelines (quotations, approvals)
- Show notification icon next to status/events
- Hover tooltip: full message, sender, timestamp
- "See all related alerts" link → opens filtered inbox

---

### Phase 9: Frontend Pages (Days 26-28)

**Goal**: Create notification management pages.

#### Step 9.1: Notification Inbox Page
**File**: `src/Frontend/web/src/app/(protected)/notifications/page.tsx`

**Tasks**:
- Create main inbox page
- Integrate NotificationInbox, NotificationFilters, NotificationActions
- Load notifications on mount (with filters)
- Real-time updates via WebSocket
- Pagination
- Error boundary
- Loading skeleton

#### Step 9.2: Notification Preferences Page
**File**: `src/Frontend/web/src/app/(protected)/profile/notifications/page.tsx`

**Tasks**:
- Create preferences page
- Integrate NotificationPreferences component
- Load preferences on mount
- Save on submit
- Success message after save

#### Step 9.3: Update Sidebar Navigation
**File**: `src/Frontend/web/src/components/layout/Sidebar.tsx` (or similar)

**Tasks**:
- Add notification icon with NotificationBadge
- Link to `/notifications`
- Real-time badge update via WebSocket
- Mobile responsive

#### Step 9.4: Integrate Toasts in Layout
**File**: `src/Frontend/web/src/app/layout.tsx` (or root layout)

**Tasks**:
- Wrap app with ToastProvider
- Add toast container (top-right or bottom-right)
- Initialize WebSocket connection
- Handle global notification events

#### Step 9.5: Update Quotation Pages
**Files**:
- `src/Frontend/web/src/app/(protected)/quotations/[id]/page.tsx`
- `src/Frontend/web/src/app/(protected)/approvals/page.tsx`

**Tasks**:
- Add ContextBadge components to timelines
- Show related notifications
- Link to filtered inbox

#### Step 9.6: Update Client Portal
**File**: `src/Frontend/web/src/app/(public)/client-portal/quotations/[quotationId]/[token]/page.tsx`

**Tasks**:
- Add success banner after accept/reject/submit
- Show error banner for expired/duplicate responses
- Toast: "Our team has been notified"
- Responsive design

---

### Phase 10: Testing & Polish (Days 29-30)

**Goal**: Write tests and polish the implementation.

#### Step 10.1: Backend Unit Tests
**Files** (in `tests/CRM.Tests/Notifications/`):
- `NotificationServiceTests.cs`
- `EmailNotificationServiceTests.cs`
- `MarkNotificationsReadCommandHandlerTests.cs`
- `ArchiveNotificationsCommandHandlerTests.cs`
- `GetNotificationsQueryHandlerTests.cs`

**Tasks**:
- Test notification creation and preference checking
- Test email sending and retry logic
- Test command handlers (mark read, archive, unarchive)
- Test query handlers (filtering, pagination)
- Mock dependencies appropriately

#### Step 10.2: Backend Integration Tests
**File**: `tests/CRM.Tests.Integration/Notifications/NotificationsControllerTests.cs`

**Tasks**:
- Test all API endpoints
- Test authorization (user can only see own notifications)
- Test filtering and pagination
- Test bulk actions
- Test preferences update

#### Step 10.3: Frontend Component Tests
**Files** (in `src/Frontend/web/src/components/notifications/__tests__/`):
- `NotificationBadge.test.tsx`
- `NotificationToast.test.tsx`
- `NotificationInbox.test.tsx`

**Tasks**:
- Test component rendering
- Test user interactions (click, filter, mark read)
- Test WebSocket integration
- Mock API calls

#### Step 10.4: Error Boundaries & Loading States
**Files**:
- `src/Frontend/web/src/components/notifications/ErrorBoundary.tsx`
- `src/Frontend/web/src/components/notifications/LoadingSkeleton.tsx`

**Tasks**:
- Create error boundary for notification pages
- Create loading skeletons for inbox and preferences
- Integrate into pages

#### Step 10.5: Accessibility & Responsive Design
**Tasks**:
- Add ARIA labels to all interactive elements
- Test keyboard navigation
- Test screen reader compatibility
- Verify mobile responsiveness (badges, toasts, inbox)
- Test on different screen sizes

#### Step 10.6: Documentation
**Files**:
- `specs/013-real-time-notification-system/quickstart.md`
- `specs/013-real-time-notification-system/checklists/requirements.md`

**Tasks**:
- Create quickstart guide (setup, configuration, verification)
- Create requirements checklist
- Document WebSocket connection setup
- Document notification event types and templates

---

## Dependencies

- **Spec-009**: Quotation Entity (for RelatedEntityType/Id)
- **Spec-010**: Quotation Management (for event triggers)
- **Spec-012**: Discount Approval Workflow (for approval events)
- **Existing**: Email infrastructure (FluentEmail), Authentication (JWT)

## Key Technical Decisions

1. **WebSocket Library**: SignalR for .NET backend, @microsoft/signalr for frontend
2. **Notification Storage**: PostgreSQL JSONB for flexible PreferenceData and Meta
3. **Email Retry**: Exponential backoff, max 3 retries, log failures
4. **Real-time Updates**: WebSocket for instant delivery, polling fallback for disconnected clients
5. **Duplicate Prevention**: Check for existing notification (RecipientUserId + RelatedEntityType + RelatedEntityId + EventType) within last 5 minutes

## Verification Checklist

- [ ] All database tables created with proper indexes
- [ ] All domain entities and enums created
- [ ] NotificationService publishes notifications correctly
- [ ] Email notifications sent and logged
- [ ] WebSocket delivers real-time notifications
- [ ] API endpoints return correct data
- [ ] Frontend inbox displays notifications
- [ ] Badges update in real-time
- [ ] Toasts appear for new notifications
- [ ] Preferences save and apply correctly
- [ ] All tests pass
- [ ] Mobile responsive design works
- [ ] Accessibility requirements met

---

**Estimated Total Duration**: 30 days  
**Team Size**: 2-3 developers (backend + frontend)  
**Critical Path**: Phases 1 → 2 → 5 → 6 → 8 → 9

