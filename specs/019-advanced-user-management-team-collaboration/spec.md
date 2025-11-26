# Spec-019: Advanced User Management & Team Collaboration Features

## Overview

This specification extends user management with advanced team collaboration features including team/department structures, user groups, delegation of tasks, activity feeds, @mentions in comments, shared workspaces, user activity monitoring, advanced permission controls, bulk user operations, user import/export, and enhanced profile management. The goal is to enable better team coordination, transparency, and scalability for growing organizations.

## Project Information

- **PROJECT_NAME**: CRM Quotation Management System
- **SPEC_NUMBER**: Spec-019
- **SPEC_NAME**: Advanced User Management & Team Collaboration Features
- **GROUP**: User Management & Collaboration (Group 9 of 11)
- **PRIORITY**: HIGH (Phase 2, after System Administration)
- **DEPENDENCIES**: Spec-003 (UserAuthentication & RBAC), Spec-018 (SystemAdministration), Spec-013 (NotificationSystem)
- **RELATED_SPECS**: Spec-020 (Advanced Integrations), Spec-021 (Mobile App Support)

---

## Key Features

- Team/Department hierarchy (create teams, assign members, set team leads)
- User Groups for bulk permissions and access control
- Task delegation and assignment (quotations, approvals)
- Activity feed showing team member actions (quotations created, approved, sent, etc.)
- @mentions in comments/notes (notify specific users)
- Shared workspaces/dashboards for teams
- User activity monitoring and audit (login history, active sessions, last seen)
- Advanced permission controls (custom roles, granular permissions)
- Bulk user operations (bulk invite, deactivate, role change)
- CSV import/export of users
- Enhanced user profiles (avatar upload, bio, social links, skills/tags)
- Out-of-office status and delegation settings
- Real-time presence indicators (online/offline/busy)

---

## JTBD Alignment

**Persona**: Manager, Admin, Team Lead

**JTBD**: "I want to organize my sales team, assign tasks, monitor activity, and ensure proper access controls"

**Success Metric**: "Team structure clear, tasks assigned efficiently, full visibility into who's doing what"

---

## Business Value

- Improved team coordination and accountability
- Scalable user management for growing organizations
- Better security with granular permissions
- Enhanced collaboration with mentions and shared spaces
- Audit and compliance through activity tracking
- Reduced admin overhead with bulk operations

---

## Requirements

### Functional Requirements

#### Team Management
- **FR-001**: System MUST allow admins/team leads to create teams with name, description, and team lead assignment
- **FR-002**: System MUST support hierarchical team structures (parent-child relationships)
- **FR-003**: System MUST allow team leads/admins to add/remove team members
- **FR-004**: System MUST enforce unique team names per company
- **FR-005**: System MUST support soft deletion of teams (mark as inactive)
- **FR-006**: System MUST track team membership history (joined date, role)

#### User Groups
- **FR-007**: System MUST allow admins to create user groups with custom permissions
- **FR-008**: System MUST allow bulk assignment of users to groups
- **FR-009**: System MUST apply group permissions to all group members
- **FR-010**: System MUST allow admins to manage group membership (add/remove users)
- **FR-011**: System MUST support permission inheritance from groups

#### Task Assignment
- **FR-012**: System MUST allow managers/team leads to assign tasks (quotations, approvals, clients) to users
- **FR-013**: System MUST track task status (pending, in progress, completed, cancelled)
- **FR-014**: System MUST support due dates for task assignments
- **FR-015**: System MUST notify users when tasks are assigned to them
- **FR-016**: System MUST allow task assignees to update task status
- **FR-017**: System MUST track task assignment history (who assigned, when, to whom)

#### Activity Feed
- **FR-018**: System MUST log all user actions (login, logout, quotation created, approval given, etc.)
- **FR-019**: System MUST provide a team-wide activity feed showing all team member actions
- **FR-020**: System MUST support filtering activity feed by user, action type, date range
- **FR-021**: System MUST support pagination for activity feed
- **FR-022**: System MUST allow clicking activity items to view related entities
- **FR-023**: System MUST track IP address and user agent for security auditing

#### Mentions
- **FR-024**: System MUST support @mentions in comments and notes
- **FR-025**: System MUST provide autocomplete for @mentions (typeahead)
- **FR-026**: System MUST notify mentioned users in real-time
- **FR-027**: System MUST track read/unread status for mentions
- **FR-028**: System MUST allow users to view all their mentions in a dedicated page

#### User Activity Monitoring
- **FR-029**: System MUST track user login/logout history
- **FR-030**: System MUST track active user sessions
- **FR-031**: System MUST update last seen timestamp for users
- **FR-032**: System MUST provide presence status (online, offline, busy, away)
- **FR-033**: System MUST update presence status in real-time via WebSocket

#### Enhanced User Profiles
- **FR-034**: System MUST allow users to upload avatar images
- **FR-035**: System MUST allow users to add bio (max 500 characters)
- **FR-036**: System MUST allow users to add social links (LinkedIn, Twitter)
- **FR-037**: System MUST allow users to add skills/tags (array of strings)
- **FR-038**: System MUST support out-of-office status with custom message
- **FR-039**: System MUST allow users to delegate tasks to another user when OOO
- **FR-040**: System MUST validate avatar file type and size

#### Bulk User Operations
- **FR-041**: System MUST support CSV import of users (bulk invite)
- **FR-042**: System MUST validate CSV format and data before import
- **FR-043**: System MUST support bulk deactivation of users
- **FR-044**: System MUST support bulk role changes for users
- **FR-045**: System MUST provide CSV/Excel export of user data
- **FR-046**: System MUST show preview of bulk operations before execution
- **FR-047**: System MUST provide progress feedback for bulk operations

#### Advanced Permissions & Roles
- **FR-048**: System MUST allow admins to create custom roles with granular permissions
- **FR-049**: System MUST provide a list of all available permissions
- **FR-050**: System MUST allow admins to assign custom permissions to roles
- **FR-051**: System MUST enforce custom role permissions in RBAC checks

#### Shared Workspaces
- **FR-052**: System MUST provide team-specific dashboards/workspaces
- **FR-053**: System MUST show team metrics and statistics in workspace
- **FR-054**: System MUST allow team leads to customize workspace layout

#### Security & Access Control
- **FR-055**: System MUST enforce RBAC on all team/group management endpoints
- **FR-056**: System MUST only allow team leads/admins to add/remove team members
- **FR-057**: System MUST only allow managers/team leads to assign tasks
- **FR-058**: System MUST log all sensitive actions to audit log
- **FR-059**: System MUST validate file uploads (CSV, avatars) for security

---

## Technical Requirements

### Backend Requirements

#### Entities & Data Models

**Team**
- TeamId (UUID, PK)
- Name (string, required, unique per company)
- Description (string, nullable)
- TeamLeadUserId (UUID, FK → Users)
- ParentTeamId (UUID, FK → Team, nullable, for hierarchy)
- CompanyId (UUID, FK)
- IsActive (bool)
- CreatedAt, UpdatedAt

**TeamMember**
- TeamMemberId (UUID, PK)
- TeamId (UUID, FK → Team)
- UserId (UUID, FK → Users)
- JoinedAt (TIMESTAMPTZ)
- Role (string: "Member", "Lead", "Admin")

**UserGroup**
- GroupId (UUID, PK)
- Name (string, required)
- Description (string, nullable)
- Permissions (JSONB, array of permission strings)
- CreatedByUserId (UUID, FK)
- CreatedAt, UpdatedAt

**UserGroupMember**
- GroupMemberId (UUID, PK)
- GroupId (UUID, FK → UserGroup)
- UserId (UUID, FK → Users)
- AddedAt (TIMESTAMPTZ)

**TaskAssignment**
- AssignmentId (UUID, PK)
- EntityType (string: "Quotation", "Approval", "Client")
- EntityId (UUID)
- AssignedToUserId (UUID, FK → Users)
- AssignedByUserId (UUID, FK → Users)
- DueDate (DATE, nullable)
- Status (enum: "PENDING", "IN_PROGRESS", "COMPLETED", "CANCELLED")
- CreatedAt, UpdatedAt

**UserActivity**
- ActivityId (UUID, PK)
- UserId (UUID, FK → Users)
- ActionType (string: "LOGIN", "LOGOUT", "QUOTATION_CREATED", "APPROVAL_GIVEN", etc.)
- EntityType (string, nullable)
- EntityId (UUID, nullable)
- IpAddress (string)
- UserAgent (TEXT)
- Timestamp (TIMESTAMPTZ)

**UserProfile (Extended)**
- Add columns to existing Users table:
  - AvatarUrl (string, nullable)
  - Bio (TEXT, nullable, max 500 chars)
  - LinkedInUrl, TwitterUrl (strings, nullable)
  - Skills (JSONB, array of strings)
  - OutOfOfficeStatus (bool, default false)
  - OutOfOfficeMessage (string, nullable)
  - DelegateUserId (UUID, FK → Users, nullable)
  - LastSeenAt (TIMESTAMPTZ)
  - PresenceStatus (enum: "ONLINE", "OFFLINE", "BUSY", "AWAY")

**Mention**
- MentionId (UUID, PK)
- EntityType (string: "Comment", "Note")
- EntityId (UUID)
- MentionedUserId (UUID, FK → Users)
- MentionedByUserId (UUID, FK → Users)
- IsRead (bool, default false)
- CreatedAt (TIMESTAMPTZ)

#### APIs

**Team Management:**
- `POST   /api/v1/teams` - Create team
- `GET    /api/v1/teams` - List all teams
- `GET    /api/v1/teams/{teamId}` - Get team details
- `PUT    /api/v1/teams/{teamId}` - Update team
- `DELETE /api/v1/teams/{teamId}` - Soft delete team
- `POST   /api/v1/teams/{teamId}/members` - Add member to team
- `DELETE /api/v1/teams/{teamId}/members/{userId}` - Remove member from team

**User Groups:**
- `POST   /api/v1/user-groups` - Create user group
- `GET    /api/v1/user-groups` - List all user groups
- `GET    /api/v1/user-groups/{groupId}` - Get group details
- `PUT    /api/v1/user-groups/{groupId}` - Update group
- `POST   /api/v1/user-groups/{groupId}/members` - Add users to group
- `DELETE /api/v1/user-groups/{groupId}/members/{userId}` - Remove user from group

**Task Assignment:**
- `POST   /api/v1/task-assignments` - Assign task
- `GET    /api/v1/task-assignments/user/{userId}` - Get user's tasks
- `PUT    /api/v1/task-assignments/{assignmentId}/status` - Update task status
- `DELETE /api/v1/task-assignments/{assignmentId}` - Cancel task assignment

**Activity Feed:**
- `GET    /api/v1/activity-feed` - Get team activity (paginated)
- `GET    /api/v1/users/{userId}/activity` - Get specific user activity

**Mentions:**
- `POST   /api/v1/mentions` - Create mention in comment/note
- `GET    /api/v1/mentions/user/{userId}` - Get user's mentions (with unread count)
- `PUT    /api/v1/mentions/{mentionId}/mark-read` - Mark mention as read

**User Management (Enhanced):**
- `POST   /api/v1/users/bulk-invite` - Bulk invite users (CSV import or bulk create)
- `POST   /api/v1/users/bulk-deactivate` - Bulk deactivate users
- `POST   /api/v1/users/bulk-role-change` - Bulk change user roles
- `GET    /api/v1/users/export` - Export users to CSV/Excel
- `PUT    /api/v1/users/{userId}/profile` - Update user profile (avatar, bio, social links)
- `PUT    /api/v1/users/{userId}/out-of-office` - Set out-of-office status
- `PUT    /api/v1/users/{userId}/presence` - Update presence status

**Permissions & Roles (Enhanced):**
- `POST   /api/v1/roles/custom` - Create custom role with granular permissions
- `GET    /api/v1/roles/permissions` - List all available permissions
- `PUT    /api/v1/roles/{roleId}/permissions` - Update role permissions

#### Validation & Security
- Team names unique per company
- Only team leads/admins can add/remove members
- Task assignments only by managers or team leads
- Mentions trigger real-time notifications (Spec 13 integration)
- Activity logging for all sensitive actions
- RBAC enforced on all endpoints
- File upload validation (CSV, avatar images)
- HTML sanitization for user-provided content (bio, OOO message)

#### Events
- `TeamCreated` - Published when a team is created
- `TeamMemberAdded` - Published when a member is added to a team
- `TeamMemberRemoved` - Published when a member is removed from a team
- `TaskAssigned` - Published when a task is assigned
- `TaskCompleted` - Published when a task is completed
- `UserMentioned` - Published when a user is mentioned
- `UserActivityLogged` - Published when user activity is logged
- `UserProfileUpdated` - Published when user profile is updated
- `OutOfOfficeStatusChanged` - Published when OOO status changes

#### Unit/Integration Tests
- CRUD for teams, groups, assignments, mentions
- Permission validation and RBAC enforcement
- Activity logging accuracy
- Notification triggers on mentions
- Bulk operations validation
- File upload validation
- Presence status updates
- Task assignment workflows

### Database & Migrations

#### New Tables

**Team**
- TeamId (UUID, PK)
- Name (string, required, unique per company)
- Description (string, nullable)
- TeamLeadUserId (UUID, FK → Users)
- ParentTeamId (UUID, FK → Team, nullable)
- CompanyId (UUID, FK)
- IsActive (bool)
- CreatedAt (TIMESTAMPTZ)
- UpdatedAt (TIMESTAMPTZ)

**TeamMember**
- TeamMemberId (UUID, PK)
- TeamId (UUID, FK → Team)
- UserId (UUID, FK → Users)
- JoinedAt (TIMESTAMPTZ)
- Role (string: "Member", "Lead", "Admin")

**UserGroup**
- GroupId (UUID, PK)
- Name (string, required)
- Description (string, nullable)
- Permissions (JSONB, array of permission strings)
- CreatedByUserId (UUID, FK → Users)
- CreatedAt (TIMESTAMPTZ)
- UpdatedAt (TIMESTAMPTZ)

**UserGroupMember**
- GroupMemberId (UUID, PK)
- GroupId (UUID, FK → UserGroup)
- UserId (UUID, FK → Users)
- AddedAt (TIMESTAMPTZ)

**TaskAssignment**
- AssignmentId (UUID, PK)
- EntityType (string: "Quotation", "Approval", "Client")
- EntityId (UUID)
- AssignedToUserId (UUID, FK → Users)
- AssignedByUserId (UUID, FK → Users)
- DueDate (DATE, nullable)
- Status (enum: "PENDING", "IN_PROGRESS", "COMPLETED", "CANCELLED")
- CreatedAt (TIMESTAMPTZ)
- UpdatedAt (TIMESTAMPTZ)

**UserActivity**
- ActivityId (UUID, PK)
- UserId (UUID, FK → Users)
- ActionType (string: "LOGIN", "LOGOUT", "QUOTATION_CREATED", "APPROVAL_GIVEN", etc.)
- EntityType (string, nullable)
- EntityId (UUID, nullable)
- IpAddress (string)
- UserAgent (TEXT)
- Timestamp (TIMESTAMPTZ)

**Mention**
- MentionId (UUID, PK)
- EntityType (string: "Comment", "Note")
- EntityId (UUID)
- MentionedUserId (UUID, FK → Users)
- MentionedByUserId (UUID, FK → Users)
- IsRead (bool, default false)
- CreatedAt (TIMESTAMPTZ)

#### Alter Existing Users Table

Add columns to Users table:
- AvatarUrl (string, nullable)
- Bio (TEXT, nullable, max 500 chars)
- LinkedInUrl (string, nullable)
- TwitterUrl (string, nullable)
- Skills (JSONB, array of strings)
- OutOfOfficeStatus (bool, default false)
- OutOfOfficeMessage (string, nullable)
- DelegateUserId (UUID, FK → Users, nullable)
- LastSeenAt (TIMESTAMPTZ)
- PresenceStatus (enum: "ONLINE", "OFFLINE", "BUSY", "AWAY")

#### Indexes
- INDEX on TeamId, UserId in TeamMember
- INDEX on GroupId, UserId in UserGroupMember
- INDEX on AssignedToUserId, Status in TaskAssignment
- INDEX on UserId, Timestamp in UserActivity
- INDEX on MentionedUserId, IsRead in Mention
- INDEX on ParentTeamId in Team (for hierarchy queries)
- INDEX on CompanyId, Name in Team (for unique constraint)
- INDEX on EntityType, EntityId in TaskAssignment
- INDEX on ActionType, Timestamp in UserActivity

#### Migrations
- Create all new tables with proper foreign keys
- Alter Users table to add new profile columns
- Seed default permissions list for custom roles
- Create indexes for performance
- Add cascade deletes where appropriate (e.g., TeamMember on Team delete)
- Add check constraints for enum values
- Add unique constraints (team name per company)

### Frontend Requirements (TailAdmin Next.js Theme - MANDATORY)

#### Pages & Components

**UM-P01: Teams Management (`/admin/teams`)**
- List all teams (table with search, filter by active/inactive)
- Create/Edit team modal (name, description, team lead selector, parent team)
- View team hierarchy (tree view or nested cards)
- Team detail page showing members, assigned tasks, activity
- Add/remove members from team
- Bulk invite to team via CSV

**UM-P02: Team Dashboard (`/teams/{teamId}`)**
- Team overview (member count, active tasks, recent activity)
- Member list with avatars, roles, presence indicators
- Activity feed for team (all actions by team members)
- Shared workspace/dashboard (team-specific metrics)

**UM-P03: User Groups Management (`/admin/user-groups`)**
- List groups, create/edit/delete
- Assign permissions to group (checkboxes for all permissions)
- Add/remove users to group
- View group members

**UM-P04: Task Assignment UI (`/tasks`)**
- My tasks page (assigned to me)
- Create task assignment modal (entity selector, assignee, due date)
- Task status update (mark in progress, completed)
- Filter by status, due date, entity type

**UM-P05: Activity Feed (`/activity`)**
- Real-time activity stream (WebSocket or polling)
- Filters: by user, action type, date range
- Search and pagination
- Click activity to view related entity

**UM-P06: Enhanced User Profile (`/profile`, `/users/{userId}`)**
- Avatar upload (drag-drop or file picker)
- Bio editor (textarea with char count)
- Social links inputs (LinkedIn, Twitter)
- Skills/tags (tag input, autocomplete)
- Out-of-office toggle with message and delegate selector
- Presence status dropdown (online/busy/away)
- Activity history tab

**UM-P07: Mentions & Notifications**
- @mention autocomplete in comment boxes
- Unread mentions indicator (badge on bell icon)
- Mentions page listing all mentions with read/unread status
- Click mention to view context (comment/note)

**UM-P08: Bulk User Operations (`/admin/users/bulk`)**
- CSV import modal (upload file, preview, validate, import)
- Bulk deactivate (select users, confirm)
- Bulk role change (select users, choose role)
- Export users to CSV/Excel

**UM-P09: Custom Role Builder (`/admin/roles/custom`)**
- Create custom role form (name, description)
- Permission checklist (grouped by module: quotations, clients, payments, etc.)
- Save and assign to users

#### Components

- `TeamHierarchyTree` - Nested teams with expand/collapse
- `UserAvatarWithPresence` - Shows online/offline/busy dot
- `ActivityFeedItem` - Single activity with icon, user, action, timestamp
- `MentionAutocomplete` - Typeahead for @mentions
- `TaskCard` - Shows task, assignee, due date, status
- `UserGroupCard` - Group name, member count, permissions summary
- `BulkActionModal` - Select action, confirm
- `SkillTagInput` - Add/remove skills with autocomplete
- `PresenceIndicator` - Real-time presence status display
- `TeamMemberList` - List of team members with roles and presence
- `TaskAssignmentForm` - Form for creating task assignments
- `ActivityFeedFilter` - Filter component for activity feed
- `MentionBadge` - Unread mentions count badge
- `OutOfOfficeToggle` - OOO status toggle with message editor

#### UX Requirements
- Real-time presence updates via WebSocket
- Toast notifications for task assignments and mentions
- Drag-drop to assign tasks or add team members (optional)
- Mobile responsive for all pages
- Accessible forms with validation and error messages
- Loading states for all async operations
- Error handling with user-friendly messages
- Success toasts for all save operations
- Confirmation dialogs for destructive actions
- Progress indicators for bulk operations

#### Tests
- Component tests for all new UI elements
- Integration tests for team/group CRUD flows
- E2E: Create team → Add members → Assign task → Complete task
- E2E: Bulk import users → Assign to groups → Verify permissions
- E2E: @mention in comment → Verify notification → Mark as read
- Accessibility and mobile responsiveness tests
- File upload validation tests

---

## Security & Compliance

- All team/group management endpoints enforce RBAC
- Only team leads/admins can modify team membership
- Only managers/team leads can assign tasks
- All sensitive actions logged to audit trail
- File uploads validated for type, size, and malicious content
- CSV imports validated and sanitized
- Presence status updates rate-limited to prevent abuse
- Activity logs immutable (append-only)
- User data encrypted at rest where applicable

---

## Performance Considerations

- Activity feed: Paginate, cache recent activities
- Presence: Use WebSocket for real-time updates, fallback to polling
- Bulk operations: Queue for processing, show progress
- Team hierarchy queries optimized with proper indexes
- Activity log partitioned by date for performance (if volume is high)
- Mentions indexed for fast unread queries
- Avatar images served via CDN with compression

---

## Scalability

- Team hierarchy supports unlimited nesting
- Activity log partitioned by date for performance
- Mentions indexed for fast unread queries
- WebSocket connections managed efficiently for presence updates
- Bulk operations processed asynchronously via background jobs
- Support for large teams (100+ members)
- Efficient queries for team member lookups

---

## Success Criteria

### Backend
- ✅ All entities, DTOs, commands, queries implemented
- ✅ All APIs functional with proper RBAC
- ✅ Activity logging accurate for all actions
- ✅ Mentions trigger notifications correctly
- ✅ Bulk operations work without errors
- ✅ Events published for all key actions
- ✅ Unit and integration tests pass with ≥85% coverage

### Database
- ✅ All tables created with correct relationships and indexes
- ✅ Migrations run successfully
- ✅ Users table extended with new profile fields
- ✅ Seed data for permissions and default roles

### Frontend
- ✅ All pages and components built using TailAdmin
- ✅ Real-time activity feed and presence indicators work
- ✅ Mentions autocomplete functional
- ✅ Bulk operations UI intuitive and error-free
- ✅ Mobile responsive and accessible
- ✅ Component and E2E tests pass with ≥80% coverage

### Integration
- ✅ Backend and frontend built in parallel
- ✅ All UI connects to real APIs (no dummy data)
- ✅ Notifications integrated (Spec 13)
- ✅ No existing functionality broken
- ✅ Full E2E workflows tested and verified

---

## Deliverables

### Backend (~40 files)
- All entities, DTOs, commands, queries, event handlers
- Controllers for teams, groups, tasks, activity, mentions
- Validators and business logic
- Migration scripts
- Unit and integration tests

### Database
- 7 new tables with migrations
- Alter Users table migration
- Index creation scripts
- Seed data scripts

### Frontend (~45 files)
- All pages listed above using TailAdmin
- All reusable components
- Custom hooks (useTeam, useActivity, useMentions, usePresence)
- API services (teamService.ts, activityService.ts, etc.)
- TypeScript types
- Component and E2E tests

### Integration
- Full E2E flow: Create team → Add members → Assign task → Complete task → View activity
- Integration with notification system (Spec-013) for mentions and task assignments
- Integration with RBAC system (Spec-003) for permissions
- Integration with system administration (Spec-018) for audit logs

---

## Implementation Notes

### Performance
- Activity feed: Paginate, cache recent activities
- Presence: Use WebSocket for real-time updates, fallback to polling
- Bulk operations: Queue for processing, show progress

### Security
- Encrypt sensitive user data (social links optional)
- Audit all role/permission changes
- Validate file uploads (CSV) for malicious content
- Sanitize user-provided HTML content

### Scalability
- Team hierarchy supports unlimited nesting
- Activity log partitioned by date for performance
- Mentions indexed for fast unread queries
- WebSocket connection pooling for presence updates

---

**End of Spec-019: Advanced User Management & Team Collaboration Features**

