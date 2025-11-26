# Data Model: Notification Entity Model (Spec-025)

Created: 2025-11-23

## Entities

### Notification
- NotificationId: UUID (PK)
- UserId: UUID NOT NULL (FK → Users.UserId)
- NotificationTypeId: UUID NOT NULL (FK → NotificationTypes.NotificationTypeId)
- Title: varchar(255) NOT NULL
- Message: text NOT NULL (max 10,000 chars)
- RelatedEntityId: UUID NULL (references related system entities)
- RelatedEntityType: varchar(100) NULL (entity type: "Quotation", "ApprovalRequest", etc.)
- IsRead: boolean NOT NULL (default false)
- ReadAt: timestamptz NULL (when notification was read)
- SentVia: varchar(100) NOT NULL (delivery channels: "InApp", "Email", "SMS", or combinations)
- CreatedAt: timestamptz NOT NULL (default now)
- UpdatedAt: timestamptz NOT NULL (default now; update on change)

#### Relationships
- User (many-to-one) → Users(UserId)
- NotificationType (many-to-one) → NotificationTypes(NotificationTypeId)

#### Constraints & Validation
- Title: 1..255 chars, required
- Message: 1..10,000 chars, required
- SentVia: Must be valid channel combination (InApp, Email, SMS)
- RelatedEntityType: Required if RelatedEntityId is provided
- IsRead: Default false, only updatable field along with ReadAt
- ReadAt: Set only when IsRead becomes true

#### Indexes
- PK(NotificationId)
- INDEX(UserId) - for user notification queries
- INDEX(NotificationTypeId) - for type filtering
- INDEX(CreatedAt) - for chronological sorting
- INDEX(IsRead) - for read status filtering
- COMPOSITE INDEX(UserId, IsRead) - for unread count queries
- COMPOSITE INDEX(UserId, CreatedAt) - for user notification lists
- INDEX(RelatedEntityId) - for entity context queries

### NotificationType
- NotificationTypeId: UUID (PK)
- TypeName: varchar(100) NOT NULL UNIQUE
- Description: text NULL
- CreatedAt: timestamptz NOT NULL (default now)
- UpdatedAt: timestamptz NOT NULL (default now; update on change)

#### Relationships
- Notifications (one-to-many) → Notifications(NotificationTypeId)

#### Constraints & Validation
- TypeName: 1..100 chars, unique, required
- Description: Optional descriptive text

#### Indexes
- PK(NotificationTypeId)
- UNIQUE(TypeName)

## State Transitions

### Notification Lifecycle
- Create → sets CreatedAt/UpdatedAt; IsRead = false, ReadAt = null
- Mark as Read → sets IsRead = true, ReadAt = current timestamp, UpdatedAt = current timestamp
- No other updates allowed (immutable except for read status)

### NotificationType Lifecycle
- Create → sets CreatedAt/UpdatedAt
- Update → sets UpdatedAt (Admin only)

## Seed Data

### Default Notification Types
```sql
INSERT INTO NotificationTypes (NotificationTypeId, TypeName, Description) VALUES
(gen_random_uuid(), 'QuotationApproved', 'Quotation has been approved'),
(gen_random_uuid(), 'QuotationRejected', 'Quotation has been rejected'),
(gen_random_uuid(), 'PaymentRequest', 'Payment is requested for approved quotation'),
(gen_random_uuid(), 'PaymentReceived', 'Payment has been received'),
(gen_random_uuid(), 'QuotationExpiring', 'Quotation is expiring soon'),
(gen_random_uuid(), 'SystemMaintenance', 'System maintenance notification'),
(gen_random_uuid(), 'UserWelcome', 'Welcome message for new users'),
(gen_random_uuid(), 'PasswordChanged', 'Password has been changed'),
(gen_random_uuid(), 'ProfileUpdated', 'User profile has been updated');
```

## Business Rules

### Immutability Rules
- Core notification content (Title, Message, UserId, NotificationTypeId, RelatedEntityId, RelatedEntityType, CreatedAt) cannot be modified after creation
- Only IsRead and ReadAt fields can be updated
- UpdatedAt timestamp is automatically maintained on any update

### Authorization Rules
- Users can only access notifications where UserId matches their user ID
- Admin users can access all notifications regardless of ownership
- Users can only mark their own notifications as read
- Admin users can mark any notification as read

### Validation Rules
- NotificationTypeId must reference existing NotificationType
- RelatedEntityType is required if RelatedEntityId is provided
- SentVia must contain valid channel names (InApp, Email, SMS)
- Title and Message cannot be empty
- ReadAt can only be set when IsRead is true

## Notes

### Delivery Channel Format
- SentVia stores comma-separated channel names: "InApp", "Email", "SMS", "InApp,Email", etc.
- Validation ensures only valid channel combinations are stored
- Background services handle actual email/SMS delivery based on SentVia field

### Entity Reference Pattern
- RelatedEntityId + RelatedEntityType provide flexible linking to any system entity
- Common RelatedEntityType values: "Quotation", "ApprovalRequest", "Payment", "User"
- Frontend can use this information to provide contextual links and actions

### Performance Considerations
- Composite indexes optimize common query patterns (user notifications, unread counts)
- Pagination prevents large result sets from impacting performance
- Consider archiving old notifications if volume becomes significant

### Real-time Integration
- Domain events (NotificationCreated, NotificationRead) enable real-time updates
- SignalR integration uses these events to push notifications to connected clients
- Background services subscribe to events for email/SMS delivery