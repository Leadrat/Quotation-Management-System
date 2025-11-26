# Tasks: Notification Entity Model (Spec-025)

Created: 2025-11-23
Branch: 025-notification-entity-model
Spec: specs/025-notification-entity-model/spec.md

## Phase 1: Setup

- [x] T001 Ensure feature branch is active `025-notification-entity-model`











































- [ ] T002 Add specs/025-notification-entity-model/contracts/notifications.openapi.yaml to API Swagger in src/Backend/CRM.Api/Program.cs
- [ ] T003 Add constitution gating checklist to PR template (reference Spec-025) in .github/PULL_REQUEST_TEMPLATE.md

## Phase 2: Foundational











- [ ] T004 Create Notification entity class in src/Backend/CRM.Domain/Entities/Notification.cs
- [ ] T005 Create NotificationType entity class in src/Backend/CRM.Domain/Entities/NotificationType.cs
- [ ] T006 Create EF configuration in src/Backend/CRM.Infrastructure/EntityConfigurations/NotificationEntityConfiguration.cs
- [ ] T007 Create EF configuration in src/Backend/CRM.Infrastructure/EntityConfigurations/NotificationTypeEntityConfiguration.cs
- [ ] T008 Add migration CreateNotifications table in src/Backend/CRM.Infrastructure/Migrations/<timestamp>_CreateNotifications.cs
- [x] T009 Apply indexes (UserId, NotificationTypeId, CreatedAt, IsRead, composite indexes) in migration file path T008



- [ ] T010 Seed default notification types in migration file path T008
- [ ] T011 Wire DbSet<Notification> and DbSet<NotificationType> in src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs
- [ ] T012 [P] Add AutoMapper profile for Notification→NotificationDto in src/Backend/CRM.Application/Mapping/NotificationProfile.cs

## Phase 3: US1 Create notification (P1)
Goal: System components can create notifications with required metadata; ownership and validation enforced.

- [ ] T013 [US1] Define CreateNotificationCommand in src/Backend/CRM.Application/Notifications/Commands/CreateNotificationCommand.cs
- [ ] T014 [US1] Implement CreateNotificationCommandHandler in src/Backend/CRM.Application/Notifications/Commands/Handlers/CreateNotificationCommandHandler.cs
- [ ] T015 [US1] Implement CreateNotificationCommandValidator in src/Backend/CRM.Application/Notifications/Validators/CreateNotificationCommandValidator.cs
- [ ] T016 [US1] Add NotificationNotFoundException in src/Backend/CRM.Application/Notifications/Exceptions/NotificationNotFoundException.cs
- [x] T017 [US1] Add UnauthorizedNotificationAccessException in src/Backend/CRM.Application/Notifications/Exceptions/UnauthorizedNotificationAccessException.cs









- [ ] T018 [US1] Add domain event NotificationCreated in src/Backend/CRM.Domain/Events/NotificationCreated.cs
- [ ] T019 [US1] Add POST /api/v1/notifications to src/Backend/CRM.Api/Controllers/NotificationsController.cs
- [ ]* T020 [P] [US1] Unit tests: Create handler/validator in tests/CRM.Tests/Notifications/CreateNotificationTests.cs
- [ ]* T021 [P] [US1] Integration test: POST /notifications success, validation errors in tests/CRM.Tests.Integration/Notifications/CreateNotificationEndpointTests.cs
- [ ]* T022 [P] [US1] Property test for notification creation completeness
  - **Property 1: Notification creation completeness**
  - **Validates: Requirements FR-001, FR-002, FR-003**

## Phase 4: US2 List user notifications (P1)
Goal: Users list only their own notifications; paginated and sorted by CreatedAt DESC; Admin sees all.

- [ ] T023 [US2] Define GetUserNotificationsQuery in src/Backend/CRM.Application/Notifications/Queries/GetUserNotificationsQuery.cs
- [ ] T024 [US2] Implement GetUserNotificationsQueryHandler in src/Backend/CRM.Application/Notifications/Queries/Handlers/GetUserNotificationsQueryHandler.cs
- [ ] T025 [US2] Add GET /api/v1/notifications to NotificationsController in src/Backend/CRM.Api/Controllers/NotificationsController.cs
- [ ]* T026 [P] [US2] Unit tests: query handler pagination/ownership in tests/CRM.Tests/Notifications/GetUserNotificationsQueryTests.cs
- [ ]* T027 [P] [US2] Integration test: GET /notifications owner vs admin, pagination clamp in tests/CRM.Tests.Integration/Notifications/ListNotificationsEndpointTests.cs
- [ ]* T028 [P] [US2] Property test for user notification isolation
  - **Property 14: User notification isolation**
  - **Validates: Requirements FR-005, FR-006**

## Phase 5: US3 Mark notification as read (P1)
Goal: Users can mark their own notifications as read; IsRead and ReadAt updated; ownership enforced.

- [ ] T029 [US3] Define MarkNotificationAsReadCommand in src/Backend/CRM.Application/Notifications/Commands/MarkNotificationAsReadCommand.cs
- [ ] T030 [US3] Implement MarkNotificationAsReadCommandHandler in src/Backend/CRM.Application/Notifications/Commands/Handlers/MarkNotificationAsReadCommandHandler.cs
- [ ] T031 [US3] Implement MarkNotificationAsReadCommandValidator in src/Backend/CRM.Application/Notifications/Validators/MarkNotificationAsReadCommandValidator.cs
- [ ] T032 [US3] Add domain event NotificationRead in src/Backend/CRM.Domain/Events/NotificationRead.cs
- [x] T033 [US3] Add PUT /api/v1/notifications/{notificationId}/read to NotificationsController in src/Backend/CRM.Api/Controllers/NotificationsController.cs




- [ ]* T034 [P] [US3] Unit tests: mark as read handler (owner/admin, already read) in tests/CRM.Tests/Notifications/MarkNotificationAsReadTests.cs
- [x]* T035 [P] [US3] Integration test: PUT mark as read (200 owner/admin, 403 non-owner) in tests/CRM.Tests.Integration/Notifications/MarkAsReadEndpointTests.cs


- [x]* T036 [P] [US3] Property test for read status transition correctness


  - **Property 6: Read status transition correctness**

  - **Validates: Requirements FR-004**
- [x]* T037 [P] [US3] Property test for read operation immutability


  - **Property 8: Read operation immutability**
  - **Validates: Requirements FR-011**

## Phase 6: US4 Filter notifications (P2)
Goal: Users can filter notifications by read status and type; query parameters validated.

- [ ] T038 [US4] Enhance GetUserNotificationsQuery with filtering parameters in src/Backend/CRM.Application/Notifications/Queries/GetUserNotificationsQuery.cs
- [ ] T039 [US4] Update GetUserNotificationsQueryHandler to support filtering in src/Backend/CRM.Application/Notifications/Queries/Handlers/GetUserNotificationsQueryHandler.cs
- [ ] T040 [US4] Update GET /api/v1/notifications to support query parameters in src/Backend/CRM.Api/Controllers/NotificationsController.cs
- [ ]* T041 [P] [US4] Unit tests: filtering logic (read status, type, date range) in tests/CRM.Tests/Notifications/FilterNotificationsTests.cs
- [ ]* T042 [P] [US4] Integration test: GET /notifications with filters in tests/CRM.Tests.Integration/Notifications/FilterNotificationsEndpointTests.cs
- [ ]* T043 [P] [US4] Property test for filtering capability correctness
  - **Property 17: Filtering capability correctness**
  - **Validates: Requirements FR-007**

## Phase 7: US5 Get unread count (P2)
Goal: Users can get accurate unread notification count; optimized query performance.

- [ ] T044 [US5] Define GetUnreadNotificationCountQuery in src/Backend/CRM.Application/Notifications/Queries/GetUnreadNotificationCountQuery.cs
- [ ] T045 [US5] Implement GetUnreadNotificationCountQueryHandler in src/Backend/CRM.Application/Notifications/Queries/Handlers/GetUnreadNotificationCountQueryHandler.cs
- [ ] T046 [US5] Add GET /api/v1/notifications/unread-count to NotificationsController in src/Backend/CRM.Api/Controllers/NotificationsController.cs
- [ ]* T047 [P] [US5] Unit tests: unread count accuracy in tests/CRM.Tests/Notifications/GetUnreadCountTests.cs
- [ ]* T048 [P] [US5] Integration test: GET /notifications/unread-count in tests/CRM.Tests.Integration/Notifications/UnreadCountEndpointTests.cs

## Phase 8: US6 Admin oversight (P3)
Goal: Admin can perform all notification operations regardless of ownership with RBAC enforced.

- [ ] T049 [US6] Ensure [Authorize(Roles="Admin,SalesRep")] is applied to all endpoints in src/Backend/CRM.Api/Controllers/NotificationsController.cs
- [ ] T050 [US6] Add admin override logic in handlers for cross-user access in notification handlers
- [ ]* T051 [P] [US6] Integration test: Admin CRUD flows across endpoints in tests/CRM.Tests.Integration/Notifications/AdminOversightEndpointTests.cs

## Phase 9: Additional Properties & Validation
Goal: Implement remaining correctness properties and comprehensive validation.

- [ ]* T052 [P] Property test for unique identifier assignment
  - **Property 2: Unique identifier assignment**
  - **Validates: Requirements FR-002**
- [ ]* T053 [P] Property test for notification type assignment consistency
  - **Property 9: Notification type assignment consistency**
  - **Validates: Requirements FR-008**
- [ ]* T054 [P] Property test for content completeness requirement
  - **Property 10: Content completeness requirement**
  - **Validates: Requirements FR-013**
- [ ]* T055 [P] Property test for delivery channel recording
  - **Property 12: Delivery channel recording**
  - **Validates: Requirements FR-010**
- [ ]* T056 [P] Property test for entity reference storage consistency
  - **Property 4: Entity reference storage consistency**
  - **Validates: Requirements FR-009**
- [ ]* T057 [P] Property test for optional entity reference handling
  - **Property 21: Optional entity reference handling**
  - **Validates: Requirements FR-009**

## Final Phase: Polish & Cross-Cutting

- [ ] T058 [P] Add Serilog structured logs for notification CRUD and authorization denials across handlers and controller
- [ ] T059 [P] Swagger: ensure tags, summaries, response codes align to contracts in src/Backend/CRM.Api/Program.cs and XML comments
- [ ] T060 [P] Update quickstart with auth/token instructions in specs/025-notification-entity-model/quickstart.md
- [ ] T061 Add audit logging event handlers for NotificationCreated/Read in src/Backend/CRM.Application/Notifications/Events/*.cs
- [ ] T062 [P] Add pagination clamp tests to list endpoint integration tests path T027
- [ ] T063 Review DTO mapping for computed fields in src/Backend/CRM.Application/Notifications/Dtos/NotificationDto.cs
- [ ] T064 [P] Add README snippet linking notifications.openapi.yaml in specs/025-notification-entity-model/contracts/notifications.openapi.yaml
- [ ] T065 Add SignalR hub for real-time notifications in src/Backend/CRM.Api/Hubs/NotificationHub.cs
- [ ] T066 [P] Add background service for email/SMS delivery in src/Backend/CRM.Infrastructure/Services/NotificationDeliveryService.cs

## Checkpoint Tasks

- [ ] T067 Checkpoint 1 - Ensure all tests pass after Phase 3 (Create notifications)
  - Ensure all tests pass, ask the user if questions arise.

- [ ] T068 Checkpoint 2 - Ensure all tests pass after Phase 5 (Core CRUD complete)
  - Ensure all tests pass, ask the user if questions arise.

- [ ] T069 Final Checkpoint - Ensure all tests pass after all phases
  - Ensure all tests pass, ask the user if questions arise.

## Dependencies & Order

- Phase 1 → Phase 2 → US1 (P1) → US2 (P1) → US3 (P1) → US4/US5 (P2, parallel) → US6 (P3) → Additional Properties → Final
- Parallel examples: T041/T042, T047/T048, T058/T059/T060/T062/T064
- Property tests can be implemented in parallel with corresponding functionality

## Implementation Strategy (MVP-first)

- MVP = US1 (Create) + US2 (List) + US3 (Mark as Read) + basic validation + CRUD endpoints
- Next = US4 (Filter) + US5 (Unread Count) + enhanced querying
- Then = US6 (Admin) + property tests + real-time features + polish
- Final = Background services + comprehensive logging + performance optimization