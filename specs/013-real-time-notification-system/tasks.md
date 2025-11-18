# Task Breakdown: Spec-013 Real-Time Notification System

**Spec**: Spec-013  
**Last Updated**: 2025-01-XX

## Overview

This document provides a detailed task breakdown for implementing Spec-013: Real-Time Quotation Notification System. Tasks are organized by phase and include priority markers `[P]` for parallelizable work.

---

## Phase 1: Setup & Foundational

**Purpose**: Establish database schema, entities, DTOs, and basic infrastructure.

### Database & Migrations

- [ ] T1 [P] Create migration `CreateNotificationsTable`:
  - Create `Notifications` table with all 13 columns
  - Add foreign key (RecipientUserId → Users, CASCADE delete)
  - Add all indexes (RecipientUserId, IsRead, IsArchived, RelatedEntityType+RelatedEntityId, DeliveryStatus, CreatedAt, Unread composite)
  - Add default values (IsRead = false, IsArchived = false, DeliveryStatus = 'SENT', CreatedAt = CURRENT_TIMESTAMP)
  - Add JSONB column for Meta

- [ ] T2 [P] Create migration `CreateNotificationPreferencesTable`:
  - Create `NotificationPreferences` table with 4 columns
  - Add foreign key (UserId → Users, CASCADE delete, PRIMARY KEY)
  - Add JSONB column for PreferenceData
  - Add default values (CreatedAt/UpdatedAt = CURRENT_TIMESTAMP)

- [ ] T3 [P] Create migration `CreateEmailNotificationLogTable`:
  - Create `EmailNotificationLog` table with 10 columns
  - Add foreign key (NotificationId → Notifications, SET NULL on delete)
  - Add all indexes (NotificationId, RecipientEmail, EventType, Status, SentAt, Failed composite)
  - Add default values (RetryCount = 0, SentAt = CURRENT_TIMESTAMP)

**Checkpoint**: Migrations run successfully, all 3 tables created with proper indexes.

---

### Domain Entities

- [ ] T4 [P] Create `src/Backend/CRM.Domain/Enums/NotificationEventType.cs`:
  - QuotationCreated, QuotationSent, QuotationViewed, QuotationAccepted, QuotationRejected
  - ApprovalNeeded, ApprovalApproved, ApprovalRejected
  - QuotationExpiring, QuotationExpired, ClientResponse, CommentMention

- [ ] T5 [P] Create `src/Backend/CRM.Domain/Enums/NotificationDeliveryStatus.cs`:
  - Sent, Delivered, Failed

- [ ] T6 [P] Create `src/Backend/CRM.Domain/Enums/EmailNotificationStatus.cs`:
  - Sent, Delivered, Bounced, Failed, Opened, Clicked

- [ ] T7 [P] Create `src/Backend/CRM.Domain/Enums/NotificationChannel.cs`:
  - InApp, Email, Push

- [ ] T8 [P] Create `src/Backend/CRM.Domain/Entities/Notification.cs`:
  - All 13 properties with correct types
  - Navigation property: RecipientUser
  - Domain methods: `MarkAsRead()`, `Archive()`, `Unarchive()`, `IsUnread()`, `IsDelivered()`

- [ ] T9 [P] Create `src/Backend/CRM.Domain/Entities/NotificationPreference.cs`:
  - All 4 properties with correct types
  - Navigation property: User
  - Domain methods: `GetPreferenceForEvent()`, `UpdatePreference()`, `IsMuted()`, `IsChannelEnabled()`

- [ ] T10 [P] Create `src/Backend/CRM.Domain/Entities/EmailNotificationLog.cs`:
  - All 10 properties with correct types
  - Navigation property: Notification (optional)
  - Domain methods: `MarkAsDelivered()`, `MarkAsFailed()`, `IncrementRetry()`

**Checkpoint**: Entities compile and pass basic validation.

---

### Entity Framework Configuration

- [ ] T11 [P] Create `src/Backend/CRM.Infrastructure/EntityConfigurations/NotificationEntityConfiguration.cs`:
  - Table name mapping
  - Primary key configuration
  - Property constraints (max lengths, required, defaults)
  - JSONB column configuration for Meta
  - Relationship (RecipientUser)
  - Indexes configuration (all 7 indexes)

- [ ] T12 [P] Create `src/Backend/CRM.Infrastructure/EntityConfigurations/NotificationPreferenceEntityConfiguration.cs`:
  - Table name mapping
  - Primary key configuration
  - JSONB column configuration for PreferenceData
  - Relationship (User)
  - Default values

- [ ] T13 [P] Create `src/Backend/CRM.Infrastructure/EntityConfigurations/EmailNotificationLogEntityConfiguration.cs`:
  - Table name mapping
  - Primary key configuration
  - Property constraints
  - Relationship (Notification, optional)
  - Indexes configuration (all 6 indexes)

- [ ] T14 [P] Update `src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs`:
  - Add `DbSet<Notification> Notifications`
  - Add `DbSet<NotificationPreference> NotificationPreferences`
  - Add `DbSet<EmailNotificationLog> EmailNotificationLogs`

- [ ] T15 [P] Update `src/Backend/CRM.Application/Common/Persistence/IAppDbContext.cs`:
  - Add `DbSet<Notification> Notifications`
  - Add `DbSet<NotificationPreference> NotificationPreferences`
  - Add `DbSet<EmailNotificationLog> EmailNotificationLogs`

**Checkpoint**: EF Core configuration complete, migrations generate correctly.

---

### DTOs

- [ ] T16 [P] Create `src/Backend/CRM.Application/Notifications/Dtos/NotificationDto.cs`:
  - All notification properties
  - Computed properties: `IsUnread`, `IsArchived`, `FormattedDate`
  - Entity link URL (computed from RelatedEntityType/Id)

- [ ] T17 [P] Create `src/Backend/CRM.Application/Notifications/Dtos/MarkNotificationsReadRequest.cs`:
  - NotificationIds array (nullable, empty = all)

- [ ] T18 [P] Create `src/Backend/CRM.Application/Notifications/Dtos/ArchiveNotificationsRequest.cs`:
  - NotificationIds array (nullable, empty = all)

- [ ] T19 [P] Create `src/Backend/CRM.Application/Notifications/Dtos/UnarchiveNotificationsRequest.cs`:
  - NotificationIds array (required, non-empty)

- [ ] T20 [P] Create `src/Backend/CRM.Application/Notifications/Dtos/NotificationPreferencesDto.cs`:
  - UserId
  - Preferences object (structured from JSONB)

- [ ] T21 [P] Create `src/Backend/CRM.Application/Notifications/Dtos/UpdateNotificationPreferencesRequest.cs`:
  - Preferences object (event type → channel settings)

- [ ] T22 [P] Create `src/Backend/CRM.Application/Notifications/Dtos/UnreadCountDto.cs`:
  - Count (integer)

- [ ] T23 [P] Create `src/Backend/CRM.Application/Notifications/Dtos/EmailNotificationLogDto.cs`:
  - All log properties
  - Formatted dates

- [ ] T24 [P] Create `src/Backend/CRM.Application/Notifications/Dtos/PagedNotificationsResult.cs`:
  - Data array (NotificationDto[])
  - PageNumber, PageSize, TotalCount

**Checkpoint**: All DTOs created with proper validation attributes.

---

### AutoMapper Profile

- [ ] T25 [P] Create `src/Backend/CRM.Application/Mapping/NotificationProfile.cs`:
  - Map Notification → NotificationDto
  - Map NotificationPreference → NotificationPreferencesDto
  - Map EmailNotificationLog → EmailNotificationLogDto
  - Resolve User names from navigation properties
  - Map JSONB PreferenceData to structured DTO
  - Map JSONB Meta to object

**Checkpoint**: AutoMapper profile compiles and maps correctly.

---

## Phase 2: Notification Publishing Infrastructure

**Purpose**: Create services and event handlers for publishing notifications.

### Notification Service

- [ ] T26 [P] Create `src/Backend/CRM.Application/Notifications/Services/INotificationService.cs`:
  - `PublishNotificationAsync(NotificationEventType, RelatedEntityType, RelatedEntityId, RecipientUserId, Message, Meta)`
  - `PublishBulkNotificationsAsync(NotificationEventType, RelatedEntityType, RelatedEntityId, RecipientUserIds, Message, Meta)`
  - `GetUserPreferencesAsync(Guid userId)`
  - `ShouldSendNotificationAsync(Guid userId, NotificationEventType, NotificationChannel)`

- [ ] T27 [P] Create `src/Backend/CRM.Application/Notifications/Services/NotificationService.cs`:
  - Implement interface
  - Check user preferences before sending
  - Create Notification entity
  - Support multiple channels (in-app, email, push)
  - Handle duplicate prevention (check last 5 minutes)
  - Call EmailNotificationService for email channel
  - Call RealTimeNotificationService for in-app channel
  - Return notification ID

**Checkpoint**: NotificationService publishes notifications correctly.

---

### Email Notification Service

- [ ] T28 [P] Create `src/Backend/CRM.Application/Notifications/Services/IEmailNotificationService.cs`:
  - `SendEmailNotificationAsync(Notification, User)`
  - `RetryFailedEmailsAsync()`
  - `LogEmailDeliveryAsync(EmailNotificationLog)`

- [ ] T29 [P] Create `src/Backend/CRM.Application/Notifications/Services/EmailNotificationService.cs`:
  - Implement interface
  - Integrate with FluentEmail
  - Load email template for event type
  - Replace placeholders (QuotationNumber, ClientName, UserName, etc.)
  - Send email
  - Log to EmailNotificationLog
  - Handle delivery status tracking
  - Implement retry logic (exponential backoff, max 3 retries)

**Checkpoint**: Email notifications sent and logged correctly.

---

### Notification Templates

- [ ] T30 [P] Create `src/Backend/CRM.Application/Notifications/Templates/QuotationCreatedTemplate.cs`:
  - Subject template
  - Body template (HTML and plain text)
  - Placeholder replacement logic

- [ ] T31 [P] Create `src/Backend/CRM.Application/Notifications/Templates/QuotationSentTemplate.cs`

- [ ] T32 [P] Create `src/Backend/CRM.Application/Notifications/Templates/QuotationViewedTemplate.cs`

- [ ] T33 [P] Create `src/Backend/CRM.Application/Notifications/Templates/QuotationAcceptedTemplate.cs`

- [ ] T34 [P] Create `src/Backend/CRM.Application/Notifications/Templates/QuotationRejectedTemplate.cs`

- [ ] T35 [P] Create `src/Backend/CRM.Application/Notifications/Templates/ApprovalNeededTemplate.cs`

- [ ] T36 [P] Create `src/Backend/CRM.Application/Notifications/Templates/ApprovalApprovedTemplate.cs`

- [ ] T37 [P] Create `src/Backend/CRM.Application/Notifications/Templates/ApprovalRejectedTemplate.cs`

- [ ] T38 [P] Create `src/Backend/CRM.Application/Notifications/Templates/QuotationExpiringTemplate.cs`

- [ ] T39 [P] Create `src/Backend/CRM.Application/Notifications/Templates/QuotationExpiredTemplate.cs`

- [ ] T40 [P] Create `src/Backend/CRM.Application/Notifications/Templates/ClientResponseTemplate.cs`

- [ ] T41 [P] Create `src/Backend/CRM.Application/Notifications/Templates/INotificationTemplate.cs`:
  - Interface with `GetSubject()`, `GetBody()`, `ReplacePlaceholders()` methods

**Checkpoint**: All templates created with proper placeholders.

---

### Domain Events

- [ ] T42 [P] Create `src/Backend/CRM.Domain/Events/NotificationPublished.cs`:
  - NotificationId, RecipientUserId, EventType, RelatedEntityType, RelatedEntityId, Timestamp

- [ ] T43 [P] Create `src/Backend/CRM.Domain/Events/NotificationRead.cs`:
  - NotificationId, UserId, ReadAt

- [ ] T44 [P] Create `src/Backend/CRM.Domain/Events/NotificationArchived.cs`:
  - NotificationId, UserId, ArchivedAt

- [ ] T45 [P] Create `src/Backend/CRM.Domain/Events/EmailNotificationSent.cs`:
  - LogId, NotificationId, RecipientEmail, EventType, SentAt

- [ ] T46 [P] Create `src/Backend/CRM.Domain/Events/EmailNotificationDelivered.cs`:
  - LogId, DeliveredAt

- [ ] T47 [P] Create `src/Backend/CRM.Domain/Events/EmailNotificationFailed.cs`:
  - LogId, ErrorMsg, RetryCount

- [ ] T48 [P] Create `src/Backend/CRM.Domain/Events/UserNotificationPreferenceUpdated.cs`:
  - UserId, UpdatedAt

**Checkpoint**: All domain events created.

---

### Event Handlers for Publishing

- [ ] T49 [P] Create `src/Backend/CRM.Application/Notifications/EventHandlers/QuotationSentEventHandler.cs`:
  - Subscribe to QuotationSent event (from Spec-010)
  - Determine recipients (sales rep, manager if applicable)
  - Call NotificationService.PublishNotificationAsync
  - Handle errors gracefully

- [ ] T50 [P] Create `src/Backend/CRM.Application/Notifications/EventHandlers/QuotationViewedEventHandler.cs`:
  - Subscribe to QuotationViewed event
  - Notify sales rep
  - Call NotificationService

- [ ] T51 [P] Create `src/Backend/CRM.Application/Notifications/EventHandlers/QuotationAcceptedEventHandler.cs`:
  - Subscribe to QuotationAccepted event
  - Notify sales rep and manager
  - Call NotificationService

- [ ] T52 [P] Create `src/Backend/CRM.Application/Notifications/EventHandlers/QuotationRejectedEventHandler.cs`:
  - Subscribe to QuotationRejected event
  - Notify sales rep
  - Call NotificationService

- [ ] T53 [P] Update `src/Backend/CRM.Application/DiscountApprovals/EventHandlers/DiscountApprovalRequestedEventHandler.cs`:
  - Add notification publishing (notify manager/admin)
  - Call NotificationService

- [ ] T54 [P] Update `src/Backend/CRM.Application/DiscountApprovals/EventHandlers/DiscountApprovalApprovedEventHandler.cs`:
  - Add notification publishing (notify sales rep)
  - Call NotificationService

- [ ] T55 [P] Update `src/Backend/CRM.Application/DiscountApprovals/EventHandlers/DiscountApprovalRejectedEventHandler.cs`:
  - Add notification publishing (notify sales rep)
  - Call NotificationService

- [ ] T56 [P] Create `src/Backend/CRM.Application/Notifications/EventHandlers/QuotationExpiringEventHandler.cs`:
  - Subscribe to QuotationExpiring event (new event or background job trigger)
  - Notify sales rep and manager
  - Call NotificationService

**Checkpoint**: Event handlers publish notifications for all major events.

---

### Background Job

- [ ] T57 [P] Create `src/Backend/CRM.Infrastructure/Jobs/QuotationExpirationNotificationJob.cs`:
  - Extend CronBackgroundService
  - Run daily at 9 AM
  - Find quotations expiring in next 24-48 hours
  - Publish bulk notifications
  - Log job execution

**Checkpoint**: Background job runs and publishes notifications.

---

## Phase 3: Backend Commands

**Purpose**: Implement commands for notification management.

### Mark Notifications Read Command

- [ ] T58 [P] Create `src/Backend/CRM.Application/Notifications/Commands/MarkNotificationsReadCommand.cs`:
  - NotificationIds array (nullable, empty = all)
  - RequestedByUserId

- [ ] T59 [P] Create `src/Backend/CRM.Application/Notifications/Commands/Handlers/MarkNotificationsReadCommandHandler.cs`:
  - Validate user owns notifications
  - Update IsRead = true, ReadAt = now
  - Publish NotificationRead event
  - Return success result

- [ ] T60 [P] Create `src/Backend/CRM.Application/Notifications/Validators/MarkNotificationsReadCommandValidator.cs`:
  - Validate NotificationIds (if provided, must be valid GUIDs)
  - Validate user context

**Checkpoint**: Mark read command works correctly.

---

### Archive Notifications Command

- [ ] T61 [P] Create `src/Backend/CRM.Application/Notifications/Commands/ArchiveNotificationsCommand.cs`:
  - NotificationIds array (nullable, empty = all)
  - RequestedByUserId

- [ ] T62 [P] Create `src/Backend/CRM.Application/Notifications/Commands/Handlers/ArchiveNotificationsCommandHandler.cs`:
  - Validate user owns notifications
  - Update IsArchived = true, ArchivedAt = now
  - Publish NotificationArchived event
  - Return success result

- [ ] T63 [P] Create `src/Backend/CRM.Application/Notifications/Validators/ArchiveNotificationsCommandValidator.cs`:
  - Validate NotificationIds (if provided, must be valid GUIDs)
  - Validate user context

**Checkpoint**: Archive command works correctly.

---

### Unarchive Notifications Command

- [ ] T64 [P] Create `src/Backend/CRM.Application/Notifications/Commands/UnarchiveNotificationsCommand.cs`:
  - NotificationIds array (required, non-empty)
  - RequestedByUserId

- [ ] T65 [P] Create `src/Backend/CRM.Application/Notifications/Commands/Handlers/UnarchiveNotificationsCommandHandler.cs`:
  - Validate user owns notifications
  - Update IsArchived = false, ArchivedAt = null
  - Return success result

- [ ] T66 [P] Create `src/Backend/CRM.Application/Notifications/Validators/UnarchiveNotificationsCommandValidator.cs`:
  - Validate NotificationIds (required, non-empty, valid GUIDs)
  - Validate user context

**Checkpoint**: Unarchive command works correctly.

---

### Update Notification Preferences Command

- [ ] T67 [P] Create `src/Backend/CRM.Application/Notifications/Commands/UpdateNotificationPreferencesCommand.cs`:
  - UserId
  - Preferences JSONB data

- [ ] T68 [P] Create `src/Backend/CRM.Application/Notifications/Commands/Handlers/UpdateNotificationPreferencesCommandHandler.cs`:
  - Validate preference structure
  - Create or update NotificationPreference record
  - Publish UserNotificationPreferenceUpdated event
  - Return updated preferences DTO

- [ ] T69 [P] Create `src/Backend/CRM.Application/Notifications/Validators/UpdateNotificationPreferencesCommandValidator.cs`:
  - Validate preferences structure (event types, channels, boolean values)
  - Validate user context

**Checkpoint**: Update preferences command works correctly.

---

## Phase 4: Backend Queries

**Purpose**: Implement queries for retrieving notifications.

### Get Notifications Query

- [ ] T70 [P] Create `src/Backend/CRM.Application/Notifications/Queries/GetNotificationsQuery.cs`:
  - Filters: unread, archived, eventType, entityType, entityId, dateFrom, dateTo
  - Pagination: pageNumber, pageSize
  - RequestorUserId

- [ ] T71 [P] Create `src/Backend/CRM.Application/Notifications/Queries/Handlers/GetNotificationsQueryHandler.cs`:
  - Filter by RecipientUserId (current user)
  - Apply all filters with proper SQL
  - Include navigation properties
  - Return paginated result

- [ ] T72 [P] Create `src/Backend/CRM.Application/Notifications/Validators/GetNotificationsQueryValidator.cs`:
  - Validate pagination parameters
  - Validate date ranges
  - Validate user context

**Checkpoint**: Get notifications query returns filtered, paginated results.

---

### Get Unread Count Query

- [ ] T73 [P] Create `src/Backend/CRM.Application/Notifications/Queries/GetUnreadCountQuery.cs`:
  - RequestorUserId

- [ ] T74 [P] Create `src/Backend/CRM.Application/Notifications/Queries/Handlers/GetUnreadCountQueryHandler.cs`:
  - Count unread notifications (IsRead = false, IsArchived = false)
  - Return count as integer

- [ ] T75 [P] Create `src/Backend/CRM.Application/Notifications/Validators/GetUnreadCountQueryValidator.cs`:
  - Validate user context

**Checkpoint**: Unread count query returns correct count.

---

### Get Notification Preferences Query

- [ ] T76 [P] Create `src/Backend/CRM.Application/Notifications/Queries/GetNotificationPreferencesQuery.cs`:
  - RequestorUserId

- [ ] T77 [P] Create `src/Backend/CRM.Application/Notifications/Queries/Handlers/GetNotificationPreferencesQueryHandler.cs`:
  - Load NotificationPreference or return defaults
  - Parse JSONB PreferenceData to DTO
  - Return NotificationPreferencesDto

- [ ] T78 [P] Create `src/Backend/CRM.Application/Notifications/Validators/GetNotificationPreferencesQueryValidator.cs`:
  - Validate user context

**Checkpoint**: Get preferences query returns user preferences or defaults.

---

### Get Entity Notifications Query

- [ ] T79 [P] Create `src/Backend/CRM.Application/Notifications/Queries/GetEntityNotificationsQuery.cs`:
  - EntityType, EntityId
  - RequestorUserId

- [ ] T80 [P] Create `src/Backend/CRM.Application/Notifications/Queries/Handlers/GetEntityNotificationsQueryHandler.cs`:
  - Filter by RelatedEntityType and RelatedEntityId
  - Filter by RecipientUserId (current user)
  - Return NotificationDto array (chronological order)

- [ ] T81 [P] Create `src/Backend/CRM.Application/Notifications/Validators/GetEntityNotificationsQueryValidator.cs`:
  - Validate EntityType and EntityId
  - Validate user context

**Checkpoint**: Get entity notifications query returns related notifications.

---

### Get Email Notification Logs Query (Admin)

- [ ] T82 [P] Create `src/Backend/CRM.Application/Notifications/Queries/GetEmailNotificationLogsQuery.cs`:
  - Filters: userId, recipientEmail, eventType, status, dateFrom, dateTo
  - Pagination: pageNumber, pageSize
  - RequestorUserId, RequestorRole

- [ ] T83 [P] Create `src/Backend/CRM.Application/Notifications/Queries/Handlers/GetEmailNotificationLogsQueryHandler.cs`:
  - Require Admin role
  - Apply all filters
  - Return paginated EmailNotificationLogDto array

- [ ] T84 [P] Create `src/Backend/CRM.Application/Notifications/Validators/GetEmailNotificationLogsQueryValidator.cs`:
  - Validate Admin role
  - Validate pagination parameters
  - Validate date ranges

**Checkpoint**: Admin email logs query returns filtered, paginated results.

---

## Phase 5: API Endpoints

**Purpose**: Create REST API endpoints for notification management.

### Notifications Controller

- [ ] T85 [P] Create `src/Backend/CRM.Api/Controllers/NotificationsController.cs`:
  - Route: `/api/v1/notifications`
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

**Checkpoint**: All API endpoints return correct responses.

---

### Register Services

- [ ] T86 [P] Update `src/Backend/CRM.Api/Program.cs`:
  - Register NotificationService, EmailNotificationService
  - Register all command handlers
  - Register all query handlers
  - Register all validators
  - Register all event handlers
  - Register QuotationExpirationNotificationJob

**Checkpoint**: All services registered and dependency injection works.

---

## Phase 6: WebSocket/Real-Time Infrastructure

**Purpose**: Implement real-time push notifications via WebSocket.

### WebSocket Hub

- [ ] T87 [P] Create `src/Backend/CRM.Api/Hubs/NotificationHub.cs`:
  - Extend Hub
  - Implement `OnConnectedAsync()` - authenticate user, add to group
  - Implement `OnDisconnectedAsync()` - remove from group
  - Create method `SendNotificationToUser()` - send notification to specific user
  - Create method `SendNotificationToGroup()` - send to role-based group
  - Handle connection management (user groups, connection tracking)

**Checkpoint**: WebSocket hub connects and authenticates users.

---

### WebSocket Service Integration

- [ ] T88 [P] Create `src/Backend/CRM.Application/Notifications/Services/IRealTimeNotificationService.cs`:
  - `SendToUserAsync(Guid userId, NotificationDto)`
  - `SendToGroupAsync(string groupName, NotificationDto)`

- [ ] T89 [P] Create `src/Backend/CRM.Application/Notifications/Services/RealTimeNotificationService.cs`:
  - Implement interface
  - Inject IHubContext<NotificationHub>
  - Send notification DTO to connected clients
  - Handle disconnected users gracefully
  - Log WebSocket delivery attempts

- [ ] T90 [P] Update `src/Backend/CRM.Application/Notifications/Services/NotificationService.cs`:
  - Inject IRealTimeNotificationService
  - Call real-time service after creating in-app notification
  - Handle WebSocket failures (log, don't fail main flow)

**Checkpoint**: Real-time notifications delivered via WebSocket.

---

### Register SignalR

- [ ] T91 [P] Update `src/Backend/CRM.Api/Program.cs`:
  - Add SignalR services: `builder.Services.AddSignalR()`
  - Map hub: `app.MapHub<NotificationHub>("/ws/notifications")`
  - Configure CORS for WebSocket connections
  - Add authentication for SignalR

**Checkpoint**: SignalR configured and WebSocket endpoint accessible.

---

## Phase 7: Frontend API Integration

**Purpose**: Create TypeScript API client and types.

### TypeScript Types

- [ ] T92 [P] Create `src/Frontend/web/src/types/notifications.ts`:
  - Interface: Notification (matches NotificationDto)
  - Interface: NotificationPreferences (matches DTO)
  - Interface: EmailNotificationLog (matches DTO)
  - Type: MarkNotificationsReadRequest
  - Type: ArchiveNotificationsRequest
  - Type: UnarchiveNotificationsRequest
  - Type: UpdateNotificationPreferencesRequest
  - Type: PagedNotificationsResult
  - Type: UnreadCountResponse

**Checkpoint**: TypeScript types match backend DTOs.

---

### API Client

- [ ] T93 [P] Update `src/Frontend/web/src/lib/api.ts`:
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

**Checkpoint**: API client methods work correctly.

---

### WebSocket Client Hook

- [ ] T94 [P] Install SignalR client package:
  - `npm install @microsoft/signalr`

- [ ] T95 [P] Create `src/Frontend/web/src/hooks/useNotificationWebSocket.ts`:
  - Create React hook using SignalR client
  - Connect to `/ws/notifications` on mount
  - Handle authentication token
  - Listen for `ReceiveNotification` event
  - Return connection status and notification callback
  - Handle reconnection logic
  - Cleanup on unmount

**Checkpoint**: WebSocket hook connects and receives notifications.

---

## Phase 8: Frontend Core Components

**Purpose**: Build reusable notification UI components.

### Notification Badge Component

- [ ] T96 [P] Create `src/Frontend/web/src/components/notifications/NotificationBadge.tsx`:
  - Badge component showing unread count
  - Red dot with count (hide if 0)
  - Animate on count change
  - Accessible (ARIA labels)
  - Responsive (mobile/desktop)

**Checkpoint**: Badge displays unread count correctly.

---

### Notification Toast Component

- [ ] T97 [P] Create `src/Frontend/web/src/components/notifications/NotificationToast.tsx`:
  - Toast component (success, info, warning, error styles)
  - Show notification message, icon, timestamp
  - Click handler to navigate to related entity
  - Auto-dismiss after timeout (configurable)
  - Manual dismiss button
  - Stack multiple toasts
  - Accessible (keyboard navigation, screen reader)

**Checkpoint**: Toast displays and dismisses correctly.

---

### Global Toast Provider

- [ ] T98 [P] Create `src/Frontend/web/src/components/notifications/ToastProvider.tsx`:
  - Context provider for global toast state
  - Manage toast queue (max 5 visible)
  - Provide toast methods (show, dismiss, clear)

- [ ] T99 [P] Create `src/Frontend/web/src/components/notifications/useToast.ts`:
  - Hook `useToast()` for showing toasts
  - Integrate with WebSocket hook
  - Auto-show toast when notification received
  - Return methods: show, dismiss, clear

**Checkpoint**: Toast provider works globally.

---

### Notification Inbox Component

- [ ] T100 [P] Create `src/Frontend/web/src/components/notifications/NotificationInbox.tsx`:
  - Table/list view of notifications
  - Show message, icon, date, status, related entity link
  - Unread styling (bold, different background)
  - Archived styling (grayed out)
  - Click notification → navigate to entity, mark as read
  - Loading skeleton
  - Empty state

**Checkpoint**: Inbox displays notifications correctly.

---

### Notification Filters Component

- [ ] T101 [P] Create `src/Frontend/web/src/components/notifications/NotificationFilters.tsx`:
  - Filter UI: unread toggle, archived toggle, event type dropdown, entity type dropdown, date range picker, search text
  - Apply filters to inbox
  - Clear filters button
  - Persist filters in URL query params

**Checkpoint**: Filters work correctly.

---

### Notification Actions Component

- [ ] T102 [P] Create `src/Frontend/web/src/components/notifications/NotificationActions.tsx`:
  - Action buttons: Mark as read, Archive, Mark all read, Unarchive
  - Batch selection (checkboxes)
  - Bulk actions
  - Confirmation dialogs for bulk actions

**Checkpoint**: Actions work correctly.

---

### Notification Preferences Component

- [ ] T103 [P] Create `src/Frontend/web/src/components/notifications/NotificationPreferences.tsx`:
  - Card with toggles for each event type
  - Per-event: In-app toggle, Email toggle, Push toggle, Mute toggle
  - Save button (calls API)
  - Loading state
  - Success/error feedback
  - Default preferences shown

**Checkpoint**: Preferences component saves correctly.

---

### Expiring Alert Banner Component

- [ ] T104 [P] Create `src/Frontend/web/src/components/notifications/ExpiringAlertBanner.tsx`:
  - Banner component for expiring quotations
  - Show warning message, quotation number, days until expiry
  - Action buttons: "Send Reminder", "Acknowledge", "Dismiss"
  - Auto-hide after acknowledge/dismiss
  - Accessible

**Checkpoint**: Banner displays and actions work.

---

### Context Badge Component

- [ ] T105 [P] Create `src/Frontend/web/src/components/notifications/ContextBadge.tsx`:
  - Badge for entity timelines (quotations, approvals)
  - Show notification icon next to status/events
  - Hover tooltip: full message, sender, timestamp
  - "See all related alerts" link → opens filtered inbox

**Checkpoint**: Context badge displays correctly.

---

### Component Index

- [ ] T106 [P] Create `src/Frontend/web/src/components/notifications/index.ts`:
  - Export all notification components

**Checkpoint**: All components exported correctly.

---

## Phase 9: Frontend Pages

**Purpose**: Create notification management pages.

### Notification Inbox Page

- [ ] T107 [P] Create `src/Frontend/web/src/app/(protected)/notifications/page.tsx`:
  - Main inbox page
  - Integrate NotificationInbox, NotificationFilters, NotificationActions
  - Load notifications on mount (with filters)
  - Real-time updates via WebSocket
  - Pagination
  - Error boundary
  - Loading skeleton

**Checkpoint**: Inbox page works correctly.

---

### Notification Preferences Page

- [ ] T108 [P] Create `src/Frontend/web/src/app/(protected)/profile/notifications/page.tsx`:
  - Preferences page
  - Integrate NotificationPreferences component
  - Load preferences on mount
  - Save on submit
  - Success message after save

**Checkpoint**: Preferences page works correctly.

---

### Update Sidebar Navigation

- [ ] T109 [P] Update `src/Frontend/web/src/components/layout/Sidebar.tsx` (or similar):
  - Add notification icon with NotificationBadge
  - Link to `/notifications`
  - Real-time badge update via WebSocket
  - Mobile responsive

**Checkpoint**: Sidebar badge updates in real-time.

---

### Integrate Toasts in Layout

- [ ] T110 [P] Update `src/Frontend/web/src/app/layout.tsx` (or root layout):
  - Wrap app with ToastProvider
  - Add toast container (top-right or bottom-right)
  - Initialize WebSocket connection
  - Handle global notification events

**Checkpoint**: Toasts appear globally.

---

### Update Quotation Pages

- [ ] T111 [P] Update `src/Frontend/web/src/app/(protected)/quotations/[id]/page.tsx`:
  - Add ContextBadge components to timelines
  - Show related notifications
  - Link to filtered inbox

- [ ] T112 [P] Update `src/Frontend/web/src/app/(protected)/approvals/page.tsx`:
  - Add ContextBadge components
  - Show related notifications
  - Link to filtered inbox

**Checkpoint**: Quotation and approval pages show notifications.

---

### Update Client Portal

- [ ] T113 [P] Update `src/Frontend/web/src/app/(public)/client-portal/quotations/[quotationId]/[token]/page.tsx`:
  - Add success banner after accept/reject/submit
  - Show error banner for expired/duplicate responses
  - Toast: "Our team has been notified"
  - Responsive design

**Checkpoint**: Client portal shows feedback correctly.

---

## Phase 10: Testing & Polish

**Purpose**: Write tests and polish the implementation.

### Backend Unit Tests

- [ ] T114 [P] Create `tests/CRM.Tests/Notifications/NotificationServiceTests.cs`:
  - Test notification creation
  - Test preference checking
  - Test duplicate prevention
  - Test bulk publishing

- [ ] T115 [P] Create `tests/CRM.Tests/Notifications/EmailNotificationServiceTests.cs`:
  - Test email sending
  - Test retry logic
  - Test template replacement
  - Mock FluentEmail

- [ ] T116 [P] Create `tests/CRM.Tests/Notifications/MarkNotificationsReadCommandHandlerTests.cs`:
  - Test marking single notification as read
  - Test marking all as read
  - Test authorization (user owns notifications)

- [ ] T117 [P] Create `tests/CRM.Tests/Notifications/ArchiveNotificationsCommandHandlerTests.cs`:
  - Test archiving notifications
  - Test bulk archive
  - Test authorization

- [ ] T118 [P] Create `tests/CRM.Tests/Notifications/GetNotificationsQueryHandlerTests.cs`:
  - Test filtering
  - Test pagination
  - Test user isolation

**Checkpoint**: All unit tests pass.

---

### Backend Integration Tests

- [ ] T119 [P] Create `tests/CRM.Tests.Integration/Notifications/NotificationsControllerTests.cs`:
  - Test all API endpoints
  - Test authorization (user can only see own notifications)
  - Test filtering and pagination
  - Test bulk actions
  - Test preferences update
  - Test admin email logs endpoint

**Checkpoint**: All integration tests pass.

---

### Frontend Component Tests

- [ ] T120 [P] Create `src/Frontend/web/src/components/notifications/__tests__/NotificationBadge.test.tsx`:
  - Test rendering with count
  - Test hiding when count is 0
  - Test animation

- [ ] T121 [P] Create `src/Frontend/web/src/components/notifications/__tests__/NotificationToast.test.tsx`:
  - Test rendering
  - Test auto-dismiss
  - Test click navigation

- [ ] T122 [P] Create `src/Frontend/web/src/components/notifications/__tests__/NotificationInbox.test.tsx`:
  - Test rendering notifications
  - Test filtering
  - Test mark as read
  - Test navigation

**Checkpoint**: All component tests pass.

---

### Error Boundaries & Loading States

- [ ] T123 [P] Create `src/Frontend/web/src/components/notifications/ErrorBoundary.tsx`:
  - Error boundary for notification pages
  - Show error message
  - Reload button

- [ ] T124 [P] Create `src/Frontend/web/src/components/notifications/LoadingSkeleton.tsx`:
  - Loading skeleton for inbox
  - Loading skeleton for preferences
  - Integrate into pages

**Checkpoint**: Error boundaries and loading states work.

---

### Accessibility & Responsive Design

- [ ] T125 [P] Accessibility audit:
  - Add ARIA labels to all interactive elements
  - Test keyboard navigation
  - Test screen reader compatibility
  - Verify focus management

- [ ] T126 [P] Responsive design audit:
  - Verify mobile responsiveness (badges, toasts, inbox)
  - Test on different screen sizes (mobile, tablet, desktop)
  - Verify touch interactions work

**Checkpoint**: Accessibility and responsive design verified.

---

### Documentation

- [ ] T127 [P] Create `specs/013-real-time-notification-system/quickstart.md`:
  - Setup instructions
  - Configuration (WebSocket, email)
  - Verification steps
  - Testing guide

- [ ] T128 [P] Create `specs/013-real-time-notification-system/checklists/requirements.md`:
  - Requirements checklist
  - Acceptance criteria verification
  - Manual testing checklist

**Checkpoint**: Documentation complete.

---

## Summary

**Total Tasks**: 128  
**Estimated Duration**: 30 days  
**Critical Path**: Phases 1 → 2 → 5 → 6 → 8 → 9

**Key Deliverables**:
- 3 database tables with proper indexes
- 3 domain entities and 4 enums
- 10 DTOs
- Notification publishing infrastructure
- 10 API endpoints
- WebSocket real-time push
- 10+ frontend components
- 3 frontend pages
- Comprehensive test coverage

