# Feature Specification: Notification Entity Model (Spec-025)

**Feature Branch**: `025-notification-entity-model`  
**Created**: 2025-11-23  
**Status**: Draft  
**Input**: User description captured in request for Spec-025 (Notification Entity Model)

## Clarifications

### Session 2025-11-23

- Q: Notification delivery channel combinations and storage format → A: Store as comma-separated values (e.g., "InApp,Email,SMS") in SentVia field; support any combination of InApp, Email, SMS.
- Q: Notification type management and seeding strategy → A: Seed common types (QuotationApproved, PaymentRequest, etc.) via migration; allow runtime creation by Admin users.
- Q: Real-time notification delivery mechanism → A: Use SignalR for in-app notifications; email/SMS handled by background services.
- Q: Notification retention and cleanup policy → A: Keep all notifications indefinitely for audit trail; implement soft archiving for old notifications if needed.
- Q: Authorization model for notification access → A: Users can only access their own notifications; Admin can access all notifications for support purposes.

## User Scenarios & Testing *(mandatory)*

<!--
  IMPORTANT: User stories should be PRIORITIZED as user journeys ordered by importance.
  Each user story/journey must be INDEPENDENTLY TESTABLE - meaning if you implement just ONE of them,
  you should still have a viable MVP (Minimum Viable Product) that delivers value.
  
  Assign priorities (P1, P2, P3, etc.) to each story, where P1 is the most critical.
  Think of each story as a standalone slice of functionality that can be:
  - Developed independently
  - Tested independently
  - Deployed independently
  - Demonstrated to users independently
-->

### User Story 1 - Create system notification (Priority: P1)

As a system component, I want to create notifications for users about important events so users can stay informed about activities relevant to their work.

**Why this priority**: Core notification functionality; enables all downstream notification features.

**Independent Test**: POST /notifications with valid data creates notification and returns 201 with NotificationDto.

**Acceptance Scenarios**:

1. **Given** a valid system event (quotation approval), **When** I create a notification with UserId, NotificationTypeId, Title, Message, and SentVia, **Then** the system creates a notification with unique ID, timestamps, and IsRead=false.
2. **Given** a notification with related entity (QuotationId), **When** I include RelatedEntityId and RelatedEntityType, **Then** the system stores the entity reference for context.

---

### User Story 2 - View my notifications (Priority: P1)

As a user, I want to see my notifications with pagination so I can stay informed about important system events.

**Why this priority**: Primary user interaction; essential for notification consumption.

**Independent Test**: GET /notifications returns only my notifications with pagination metadata.

**Acceptance Scenarios**:

1. **Given** a user token, **When** I call GET /notifications, **Then** I see only notifications where UserId equals my user ID, sorted by CreatedAt DESC.
2. **Given** many notifications, **When** I supply pageNumber and pageSize, **Then** results are paginated with total count and hasMore indicators.

---

### User Story 3 - Mark notification as read (Priority: P1)

As a user, I want to mark notifications as read so I can track which notifications I have already reviewed.

**Why this priority**: Essential for notification state management and user experience.

**Independent Test**: PUT /notifications/{id}/read updates IsRead=true and sets ReadAt timestamp.

**Acceptance Scenarios**:

1. **Given** an unread notification I own, **When** I mark it as read, **Then** IsRead becomes true and ReadAt is set to current timestamp.
2. **Given** a notification I don't own, **When** I attempt to mark as read, **Then** 403 Forbidden is returned.

---

### User Story 4 - Filter notifications (Priority: P2)

As a user, I want to filter notifications by read status and type so I can find specific notifications efficiently.

**Independent Test**: GET /notifications with query parameters returns filtered results.

**Acceptance Scenarios**:

1. **Given** mixed read/unread notifications, **When** I filter by IsRead=false, **Then** only unread notifications are returned.
2. **Given** notifications of different types, **When** I filter by NotificationTypeId, **Then** only notifications of that type are returned.

---

### User Story 5 - Get unread count (Priority: P2)

As a user, I want to see my unread notification count so I know when I have new notifications without viewing the full list.

**Independent Test**: GET /notifications/unread-count returns accurate count of unread notifications.

**Acceptance Scenarios**:

1. **Given** 5 unread and 3 read notifications, **When** I request unread count, **Then** the system returns 5.
2. **Given** I mark 2 notifications as read, **When** I request unread count again, **Then** the system returns 3.

---

### User Story 6 - Admin notification oversight (Priority: P3)

As an Admin, I want to view any user's notifications and create system-wide notifications so I can support users and manage system communications.

**Independent Test**: Admin token can access all notifications and create notifications for any user.

**Acceptance Scenarios**:

1. **Given** an Admin token, **When** I query notifications for any user, **Then** I can see their notifications.
2. **Given** an Admin token, **When** I create a notification for any user, **Then** the notification is created successfully.

### Edge Cases

- Creating notification with invalid NotificationTypeId → 400 validation error
- Creating notification with empty Title or Message → 400 validation error
- Accessing soft-deleted notification → 404 Not Found (if soft delete implemented)
- Marking already-read notification as read → 200 with no change to ReadAt timestamp
- Pagination parameters out of bounds: clamp pageNumber<1 to 1 and pageSize>100 to 100
- SentVia with invalid channel combination → 400 validation error
- RelatedEntityId provided without RelatedEntityType → 400 validation error
- Very long notification messages → truncation or validation based on business rules

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow creation of notifications with required metadata (UserId, NotificationTypeId, Title, Message, SentVia).
- **FR-002**: System MUST assign unique NotificationId and CreatedAt timestamp to each notification.
- **FR-003**: System MUST set initial read status to false (IsRead=false, ReadAt=null) for new notifications.
- **FR-004**: System MUST allow users to mark their own notifications as read, updating IsRead and ReadAt fields.
- **FR-005**: System MUST enforce notification ownership: users can only access their own notifications; Admin can access all.
- **FR-006**: System MUST provide paginated notification lists sorted by CreatedAt DESC with total count.
- **FR-007**: System MUST support filtering by read status (IsRead) and notification type (NotificationTypeId).
- **FR-008**: System MUST validate NotificationTypeId references existing notification types.
- **FR-009**: System MUST support optional entity references (RelatedEntityId, RelatedEntityType) for context.
- **FR-010**: System MUST record delivery channels in SentVia field (InApp, Email, SMS combinations).
- **FR-011**: System MUST maintain data immutability: only IsRead and ReadAt can be updated after creation.
- **FR-012**: System MUST provide accurate unread notification counts per user.
- **FR-013**: System MUST validate Title (max 255 chars) and Message (max 10,000 chars) lengths.
- **FR-014**: System MUST return 403 for unauthorized notification access and 404 for missing notifications.
- **FR-015**: System MUST publish domain events on notification create and read operations.
- **FR-016**: System MUST support Admin override for all notification operations regardless of ownership.

### Key Entities *(include if feature involves data)*

- **Notification**: Represents a system notification sent to a user. Attributes include identity (NotificationId), recipient (UserId), categorization (NotificationTypeId), content (Title, Message), context (RelatedEntityId, RelatedEntityType), status (IsRead, ReadAt), delivery (SentVia), and timestamps (CreatedAt, UpdatedAt).
- **NotificationType**: Represents notification categories/types. Attributes include identity (NotificationTypeId), classification (TypeName, Description), and timestamps (CreatedAt, UpdatedAt).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: System can create notifications with all required fields in ≤ 100ms average response time.
- **SC-002**: Users can view only their own notifications with correct pagination and sorting.
- **SC-003**: Notification read status updates are reflected immediately with 100% accuracy.
- **SC-004**: Unread count API returns accurate counts with ≤ 50ms average response time.
- **SC-005**: Authorization rules prevent cross-user access with 0 known security violations.
- **SC-006**: 100% of notification operations emit appropriate domain events for audit trail.