# Implementation Plan: Advanced User Management & Team Collaboration Features (Spec-019)

**Branch**: `019-advanced-user-management-team-collaboration` | **Date**: 2025-11-18 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/019-advanced-user-management-team-collaboration/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

This plan implements advanced user management and team collaboration features including team/department hierarchies, user groups, task delegation, activity feeds, @mentions, shared workspaces, user activity monitoring, advanced permission controls, bulk user operations, CSV import/export, enhanced user profiles, out-of-office status, and real-time presence indicators. The implementation follows Clean Architecture with CQRS pattern, using .NET 8.0 backend and Next.js 15 frontend with TailAdmin theme. All team and user management actions are logged to activity feeds, and mentions trigger real-time notifications.

## Technical Context

**Language/Version**: C# 12+ (.NET 8.0), TypeScript 5.x, React 19, Next.js 15

**Primary Dependencies**: 
- Backend: MediatR, Entity Framework Core 8.0, FluentValidation, AutoMapper, Npgsql.EntityFrameworkCore.PostgreSQL, SignalR (for real-time presence)
- Frontend: Next.js 15, React 19, Tailwind CSS v4, TailAdmin Next.js template, React Query (TanStack Query), Axios, Zustand, @microsoft/signalr (for WebSocket)

**Storage**: PostgreSQL (Team, TeamMember, UserGroup, UserGroupMember, TaskAssignment, UserActivity, Mention tables), File storage for avatars (S3 or local filesystem)

**Testing**: 
- Backend: xUnit, Moq, FluentAssertions, Microsoft.AspNetCore.Mvc.Testing
- Frontend: Jest, React Testing Library, Playwright (E2E)

**Target Platform**: 
- Backend: Linux/Windows server (.NET 8.0)
- Frontend: Web browsers (Chrome, Firefox, Safari, Edge - latest 2 versions)

**Project Type**: Web application (backend API + frontend SPA)

**Performance Goals**: 
- API p90 <200ms for all team/user management endpoints
- Activity feed queries with pagination <500ms
- Presence updates via WebSocket <100ms latency
- Bulk operations (100+ users) <30 seconds
- CSV import/export <10 seconds for 1000 users
- Frontend LCP <2s

**Constraints**: 
- Team names unique per company
- Only team leads/admins can add/remove members
- Task assignments only by managers/team leads
- Mentions trigger real-time notifications (Spec 13 integration)
- Activity logging for all sensitive actions
- RBAC enforced on all endpoints
- Avatar uploads: max 5MB, PNG/JPG/SVG only
- CSV imports validated for malicious content

**Scale/Scope**: 
- Support 1-100 teams per organization
- 10-1000 users per organization
- 1-50 user groups per organization
- 100-10,000 task assignments per month
- 1M+ activity log entries (with archival strategy)
- Real-time presence for 100-500 concurrent users

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### ✅ Spec-Driven Delivery
- Spec-019 defines complete scope, entities, APIs, and acceptance criteria
- All deliverables trace back to spec requirements

### ✅ Clean Architecture & RBAC Enforcement
- Follows existing .NET Clean Architecture pattern (Domain, Application, Infrastructure, API layers)
- RBAC enforced at API endpoints using `[Authorize(Roles = "Admin", "Manager", "TeamLead")]` attributes
- CQRS pattern with Commands/Queries and Handlers

### ✅ Security, Compliance, and Data Integrity
- JWT authentication required for all endpoints
- Input validation with FluentValidation
- Activity trail for all team/user management actions
- UUID PKs, FK constraints, indexes per schema
- File upload validation (CSV, avatars) for security
- HTML sanitization for user-provided content (bio, OOO message)

### ✅ Testing & Quality Gates
- Unit tests ≥85% backend coverage
- Integration tests for all API endpoints
- E2E tests for team/user management workflows
- Frontend tests ≥80% coverage

### ✅ Observability, Auditability, and Change Control
- Complete activity trail (UserActivity entity)
- Structured logging via Serilog
- All team/user management actions logged with user, IP, timestamp, changes
- Real-time presence tracking

### ✅ Frontend Framework & UI Theme Integration
- Uses TailAdmin Next.js template
- Follows TailAdmin folder structure and conventions
- Tailwind CSS v4 utility classes
- React Query for data fetching and cache invalidation
- SignalR for real-time presence updates

**Gates Status**: ✅ **PASS** - All constitution principles satisfied

## Project Structure

### Documentation (this feature)

```text
specs/019-advanced-user-management-team-collaboration/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
│   └── user-management.openapi.yaml
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/Backend/
├── CRM.Domain/
│   └── UserManagement/
│       ├── Team.cs
│       ├── TeamMember.cs
│       ├── UserGroup.cs
│       ├── UserGroupMember.cs
│       ├── TaskAssignment.cs
│       ├── UserActivity.cs
│       ├── Mention.cs
│       └── Events/
│           ├── TeamCreated.cs
│           ├── TeamMemberAdded.cs
│           ├── TeamMemberRemoved.cs
│           ├── TaskAssigned.cs
│           ├── TaskCompleted.cs
│           ├── UserMentioned.cs
│           ├── UserActivityLogged.cs
│           ├── UserProfileUpdated.cs
│           └── OutOfOfficeStatusChanged.cs
├── CRM.Application/
│   └── UserManagement/
│       ├── Commands/
│       │   ├── CreateTeamCommand.cs
│       │   ├── UpdateTeamCommand.cs
│       │   ├── DeleteTeamCommand.cs
│       │   ├── AddTeamMemberCommand.cs
│       │   ├── RemoveTeamMemberCommand.cs
│       │   ├── CreateUserGroupCommand.cs
│       │   ├── UpdateUserGroupCommand.cs
│       │   ├── AddUserGroupMemberCommand.cs
│       │   ├── RemoveUserGroupMemberCommand.cs
│       │   ├── AssignTaskCommand.cs
│       │   ├── UpdateTaskStatusCommand.cs
│       │   ├── CreateMentionCommand.cs
│       │   ├── MarkMentionReadCommand.cs
│       │   ├── BulkInviteUsersCommand.cs
│       │   ├── BulkDeactivateUsersCommand.cs
│       │   ├── BulkChangeUserRolesCommand.cs
│       │   ├── UpdateUserProfileCommand.cs
│       │   ├── SetOutOfOfficeCommand.cs
│       │   ├── UpdatePresenceStatusCommand.cs
│       │   ├── CreateCustomRoleCommand.cs
│       │   ├── UpdateRolePermissionsCommand.cs
│       │   └── Handlers/
│       ├── Queries/
│       │   ├── GetTeamsQuery.cs
│       │   ├── GetTeamByIdQuery.cs
│       │   ├── GetTeamMembersQuery.cs
│       │   ├── GetUserGroupsQuery.cs
│       │   ├── GetUserGroupByIdQuery.cs
│       │   ├── GetUserTasksQuery.cs
│       │   ├── GetActivityFeedQuery.cs
│       │   ├── GetUserActivityQuery.cs
│       │   ├── GetMentionsQuery.cs
│       │   ├── GetUnreadMentionsCountQuery.cs
│       │   ├── GetAvailablePermissionsQuery.cs
│       │   ├── GetCustomRolesQuery.cs
│       │   └── Handlers/
│       ├── DTOs/
│       │   ├── TeamDto.cs
│       │   ├── TeamMemberDto.cs
│       │   ├── UserGroupDto.cs
│       │   ├── TaskAssignmentDto.cs
│       │   ├── UserActivityDto.cs
│       │   ├── MentionDto.cs
│       │   ├── UserProfileDto.cs
│       │   ├── CustomRoleDto.cs
│       │   ├── PermissionDto.cs
│       │   └── BulkOperationResultDto.cs
│       ├── Requests/
│       │   ├── CreateTeamRequest.cs
│       │   ├── UpdateTeamRequest.cs
│       │   ├── AddTeamMemberRequest.cs
│       │   ├── CreateUserGroupRequest.cs
│       │   ├── UpdateUserGroupRequest.cs
│       │   ├── AssignTaskRequest.cs
│       │   ├── UpdateTaskStatusRequest.cs
│       │   ├── CreateMentionRequest.cs
│       │   ├── BulkInviteUsersRequest.cs
│       │   ├── BulkDeactivateUsersRequest.cs
│       │   ├── BulkChangeUserRolesRequest.cs
│       │   ├── UpdateUserProfileRequest.cs
│       │   ├── SetOutOfOfficeRequest.cs
│       │   ├── UpdatePresenceStatusRequest.cs
│       │   ├── CreateCustomRoleRequest.cs
│       │   └── UpdateRolePermissionsRequest.cs
│       ├── Validators/
│       │   ├── CreateTeamRequestValidator.cs
│       │   ├── UpdateTeamRequestValidator.cs
│       │   ├── AssignTaskRequestValidator.cs
│       │   ├── BulkInviteUsersRequestValidator.cs
│       │   └── ...
│       ├── Services/
│       │   ├── ITeamService.cs
│       │   ├── TeamService.cs
│       │   ├── IUserGroupService.cs
│       │   ├── UserGroupService.cs
│       │   ├── ITaskAssignmentService.cs
│       │   ├── TaskAssignmentService.cs
│       │   ├── IActivityService.cs
│       │   ├── ActivityService.cs
│       │   ├── IMentionService.cs
│       │   ├── MentionService.cs
│       │   ├── IUserBulkOperationService.cs
│       │   ├── UserBulkOperationService.cs
│       │   ├── IUserProfileService.cs
│       │   ├── UserProfileService.cs
│       │   ├── IPresenceService.cs
│       │   ├── PresenceService.cs
│       │   ├── ICustomRoleService.cs
│       │   └── CustomRoleService.cs
│       └── Mapping/
│           └── UserManagementProfile.cs (AutoMapper)
├── CRM.Infrastructure/
│   └── UserManagement/
│       ├── FileStorage/
│       │   ├── IFileStorageService.cs
│       │   └── LocalFileStorageService.cs (or S3FileStorageService.cs)
│       ├── CsvImport/
│       │   ├── ICsvImportService.cs
│       │   └── CsvImportService.cs
│       └── HtmlSanitization/
│           ├── IHtmlSanitizer.cs
│           └── HtmlSanitizerService.cs
├── CRM.Api/
│   ├── Controllers/
│   │   ├── TeamsController.cs
│   │   ├── UserGroupsController.cs
│   │   ├── TaskAssignmentsController.cs
│   │   ├── ActivityFeedController.cs
│   │   ├── MentionsController.cs
│   │   ├── UserBulkOperationsController.cs
│   │   ├── UserProfileController.cs
│   │   ├── PresenceController.cs
│   │   └── CustomRolesController.cs
│   └── Hubs/
│       └── PresenceHub.cs (SignalR for real-time presence)
└── CRM.Migrator/
    └── Migrations/
        └── [timestamp]_AddUserManagementTables.cs

src/Frontend/web/
├── src/
│   ├── app/
│   │   ├── (protected)/
│   │   │   ├── admin/
│   │   │   │   ├── teams/
│   │   │   │   │   ├── page.tsx (Teams Management)
│   │   │   │   │   └── [teamId]/
│   │   │   │   │       └── page.tsx (Team Detail)
│   │   │   │   ├── user-groups/
│   │   │   │   │   └── page.tsx (User Groups Management)
│   │   │   │   ├── users/
│   │   │   │   │   └── bulk/
│   │   │   │   │       └── page.tsx (Bulk User Operations)
│   │   │   │   └── roles/
│   │   │   │       └── custom/
│   │   │   │           └── page.tsx (Custom Role Builder)
│   │   │   ├── teams/
│   │   │   │   └── [teamId]/
│   │   │   │       └── page.tsx (Team Dashboard)
│   │   │   ├── tasks/
│   │   │   │   └── page.tsx (Task Assignment UI)
│   │   │   ├── activity/
│   │   │   │   └── page.tsx (Activity Feed)
│   │   │   ├── mentions/
│   │   │   │   └── page.tsx (Mentions & Notifications)
│   │   │   └── profile/
│   │   │       └── page.tsx (Enhanced User Profile)
│   │   └── (public)/
│   ├── components/
│   │   └── user-management/
│   │       ├── TeamHierarchyTree.tsx
│   │       ├── UserAvatarWithPresence.tsx
│   │       ├── ActivityFeedItem.tsx
│       │       ├── ActivityFeedFilter.tsx
│       │       ├── MentionAutocomplete.tsx
│       │       ├── TaskCard.tsx
│       │       ├── UserGroupCard.tsx
│       │       ├── BulkActionModal.tsx
│       │       ├── SkillTagInput.tsx
│       │       ├── PresenceIndicator.tsx
│       │       ├── TeamMemberList.tsx
│       │       ├── TaskAssignmentForm.tsx
│       │       ├── MentionBadge.tsx
│       │       └── OutOfOfficeToggle.tsx
│   ├── lib/
│   │   └── api/
│   │       └── userManagement.ts (API client for user management endpoints)
│   └── hooks/
│       ├── useTeam.ts
│       ├── useActivity.ts
│       ├── useMentions.ts
│       ├── usePresence.ts
│       ├── useTaskAssignments.ts
│       └── useUserGroups.ts

tests/
├── CRM.Tests/
│   └── UserManagement/
│       ├── Commands/
│       ├── Queries/
│       └── Services/
└── CRM.Tests.Integration/
    └── UserManagement/
        └── Controllers/
```

**Structure Decision**: Web application structure (backend API + frontend SPA). Backend follows existing Clean Architecture pattern with Domain, Application, Infrastructure, and API layers. Frontend uses Next.js App Router with TailAdmin theme, organized by feature (user-management) with shared components. SignalR hub for real-time presence updates.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

No violations - all requirements align with constitution principles.

## Implementation Phases

### Phase 0: Research & Design (Days 1-3)

**Goal**: Research technical decisions, design data models, create API contracts.

#### Step 0.1: Technical Research
**File**: `specs/019-advanced-user-management-team-collaboration/research.md`

**Tasks**:
- Research SignalR for real-time presence updates
- Research CSV parsing libraries for bulk import
- Research file upload best practices (avatars)
- Research team hierarchy query patterns
- Research activity feed pagination strategies
- Document technical decisions

#### Step 0.2: Data Model Design
**File**: `specs/019-advanced-user-management-team-collaboration/data-model.md`

**Tasks**:
- Design all 7 new tables (Team, TeamMember, UserGroup, UserGroupMember, TaskAssignment, UserActivity, Mention)
- Design Users table extensions (avatar, bio, social links, skills, OOO, presence)
- Design indexes for performance
- Design foreign key relationships
- Document migration strategy

#### Step 0.3: API Contract Design
**File**: `specs/019-advanced-user-management-team-collaboration/contracts/user-management.openapi.yaml`

**Tasks**:
- Define all REST API endpoints (teams, groups, tasks, activity, mentions, bulk operations, profiles, presence, roles)
- Define request/response schemas
- Define error responses
- Define authentication requirements
- Define pagination parameters

#### Step 0.4: Quickstart Guide
**File**: `specs/019-advanced-user-management-team-collaboration/quickstart.md`

**Tasks**:
- Document setup instructions
- Document configuration requirements
- Document verification steps
- Document testing procedures

### Phase 1: Database & Domain Layer (Days 4-7)

**Goal**: Create database migrations, domain entities, and EF configurations.

#### Step 1.1: Database Migrations
**File**: `src/Backend/CRM.Migrator/Migrations/[timestamp]_AddUserManagementTables.cs`

**Tasks**:
- Create Team table (TeamId PK, Name, Description, TeamLeadUserId FK, ParentTeamId FK, CompanyId FK, IsActive, CreatedAt, UpdatedAt)
- Create TeamMember table (TeamMemberId PK, TeamId FK, UserId FK, JoinedAt, Role)
- Create UserGroup table (GroupId PK, Name, Description, Permissions JSONB, CreatedByUserId FK, CreatedAt, UpdatedAt)
- Create UserGroupMember table (GroupMemberId PK, GroupId FK, UserId FK, AddedAt)
- Create TaskAssignment table (AssignmentId PK, EntityType, EntityId, AssignedToUserId FK, AssignedByUserId FK, DueDate, Status, CreatedAt, UpdatedAt)
- Create UserActivity table (ActivityId PK, UserId FK, ActionType, EntityType, EntityId, IpAddress, UserAgent, Timestamp)
- Create Mention table (MentionId PK, EntityType, EntityId, MentionedUserId FK, MentionedByUserId FK, IsRead, CreatedAt)
- Alter Users table: Add AvatarUrl, Bio, LinkedInUrl, TwitterUrl, Skills JSONB, OutOfOfficeStatus, OutOfOfficeMessage, DelegateUserId FK, LastSeenAt, PresenceStatus
- Add all foreign keys and indexes
- Add unique constraint on Team.Name per CompanyId
- Add check constraints for enum values

#### Step 1.2: Domain Entities
**Files**:
- `src/Backend/CRM.Domain/UserManagement/Team.cs`
- `src/Backend/CRM.Domain/UserManagement/TeamMember.cs`
- `src/Backend/CRM.Domain/UserManagement/UserGroup.cs`
- `src/Backend/CRM.Domain/UserManagement/UserGroupMember.cs`
- `src/Backend/CRM.Domain/UserManagement/TaskAssignment.cs`
- `src/Backend/CRM.Domain/UserManagement/UserActivity.cs`
- `src/Backend/CRM.Domain/UserManagement/Mention.cs`

**Tasks**:
- Create all 7 domain entities with properties
- Add navigation properties (User, Team, etc.)
- Add domain methods (e.g., `MarkAsRead()` for Mention, `Complete()` for TaskAssignment)
- Add validation logic in domain entities
- Create enums: TaskAssignmentStatus, PresenceStatus

#### Step 1.3: Update Users Entity
**File**: `src/Backend/CRM.Domain/Entities/User.cs` (existing)

**Tasks**:
- Add new properties: AvatarUrl, Bio, LinkedInUrl, TwitterUrl, Skills, OutOfOfficeStatus, OutOfOfficeMessage, DelegateUserId, LastSeenAt, PresenceStatus
- Add navigation property to DelegateUser
- Add domain methods: `SetOutOfOffice()`, `UpdatePresence()`, `UpdateLastSeen()`

#### Step 1.4: Entity Framework Configurations
**Files**:
- `src/Backend/CRM.Infrastructure/EntityConfigurations/TeamEntityConfiguration.cs`
- `src/Backend/CRM.Infrastructure/EntityConfigurations/TeamMemberEntityConfiguration.cs`
- `src/Backend/CRM.Infrastructure/EntityConfigurations/UserGroupEntityConfiguration.cs`
- `src/Backend/CRM.Infrastructure/EntityConfigurations/UserGroupMemberEntityConfiguration.cs`
- `src/Backend/CRM.Infrastructure/EntityConfigurations/TaskAssignmentEntityConfiguration.cs`
- `src/Backend/CRM.Infrastructure/EntityConfigurations/UserActivityEntityConfiguration.cs`
- `src/Backend/CRM.Infrastructure/EntityConfigurations/MentionEntityConfiguration.cs`

**Tasks**:
- Configure table names, primary keys, property constraints
- Configure JSONB columns (Permissions, Skills)
- Configure enum to string conversions
- Configure relationships and foreign keys
- Configure all indexes (including composite and partial indexes)
- Configure cascade delete behavior

#### Step 1.5: Update DbContext
**Files**:
- `src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs`
- `src/Backend/CRM.Application/Common/Persistence/IAppDbContext.cs`

**Tasks**:
- Add `DbSet<Team> Teams`
- Add `DbSet<TeamMember> TeamMembers`
- Add `DbSet<UserGroup> UserGroups`
- Add `DbSet<UserGroupMember> UserGroupMembers`
- Add `DbSet<TaskAssignment> TaskAssignments`
- Add `DbSet<UserActivity> UserActivities`
- Add `DbSet<Mention> Mentions`
- Update interface with same properties

#### Step 1.6: Domain Events
**Files**:
- `src/Backend/CRM.Domain/UserManagement/Events/TeamCreated.cs`
- `src/Backend/CRM.Domain/UserManagement/Events/TeamMemberAdded.cs`
- `src/Backend/CRM.Domain/UserManagement/Events/TeamMemberRemoved.cs`
- `src/Backend/CRM.Domain/UserManagement/Events/TaskAssigned.cs`
- `src/Backend/CRM.Domain/UserManagement/Events/TaskCompleted.cs`
- `src/Backend/CRM.Domain/UserManagement/Events/UserMentioned.cs`
- `src/Backend/CRM.Domain/UserManagement/Events/UserActivityLogged.cs`
- `src/Backend/CRM.Domain/UserManagement/Events/UserProfileUpdated.cs`
- `src/Backend/CRM.Domain/UserManagement/Events/OutOfOfficeStatusChanged.cs`

**Tasks**:
- Create all 10 domain event classes
- Include relevant data (TeamId, UserId, TaskId, etc.)
- Add timestamps and metadata

### Phase 2: Application Layer - Services & DTOs (Days 8-12)

**Goal**: Create application services, DTOs, and request models.

#### Step 2.1: DTOs
**Files** (in `src/Backend/CRM.Application/UserManagement/DTOs/`):
- `TeamDto.cs`
- `TeamMemberDto.cs`
- `UserGroupDto.cs`
- `UserGroupMemberDto.cs`
- `TaskAssignmentDto.cs`
- `UserActivityDto.cs`
- `MentionDto.cs`
- `UserProfileDto.cs`
- `CustomRoleDto.cs`
- `PermissionDto.cs`
- `BulkOperationResultDto.cs`
- `PagedActivityFeedResult.cs`

**Tasks**:
- Create all 12 DTO classes with proper properties
- Add validation attributes where needed
- Include computed properties (e.g., formatted dates, entity links, user names)

#### Step 2.2: Request Models
**Files** (in `src/Backend/CRM.Application/UserManagement/Requests/`):
- `CreateTeamRequest.cs`
- `UpdateTeamRequest.cs`
- `AddTeamMemberRequest.cs`
- `CreateUserGroupRequest.cs`
- `UpdateUserGroupRequest.cs`
- `AssignTaskRequest.cs`
- `UpdateTaskStatusRequest.cs`
- `CreateMentionRequest.cs`
- `BulkInviteUsersRequest.cs`
- `BulkDeactivateUsersRequest.cs`
- `BulkChangeUserRolesRequest.cs`
- `UpdateUserProfileRequest.cs`
- `SetOutOfOfficeRequest.cs`
- `UpdatePresenceStatusRequest.cs`
- `CreateCustomRoleRequest.cs`
- `UpdateRolePermissionsRequest.cs`

**Tasks**:
- Create all 16 request classes
- Add validation attributes
- Include all required fields

#### Step 2.3: AutoMapper Profile
**File**: `src/Backend/CRM.Application/Mapping/UserManagementProfile.cs`

**Tasks**:
- Map Team → TeamDto
- Map TeamMember → TeamMemberDto
- Map UserGroup → UserGroupDto
- Map TaskAssignment → TaskAssignmentDto
- Map UserActivity → UserActivityDto
- Map Mention → MentionDto
- Map User → UserProfileDto (with new fields)
- Resolve User names from navigation properties
- Map JSONB fields (Permissions, Skills) to structured DTOs

#### Step 2.4: Application Services
**Files**:
- `src/Backend/CRM.Application/UserManagement/Services/ITeamService.cs`
- `src/Backend/CRM.Application/UserManagement/Services/TeamService.cs`
- `src/Backend/CRM.Application/UserManagement/Services/IUserGroupService.cs`
- `src/Backend/CRM.Application/UserManagement/Services/UserGroupService.cs`
- `src/Backend/CRM.Application/UserManagement/Services/ITaskAssignmentService.cs`
- `src/Backend/CRM.Application/UserManagement/Services/TaskAssignmentService.cs`
- `src/Backend/CRM.Application/UserManagement/Services/IActivityService.cs`
- `src/Backend/CRM.Application/UserManagement/Services/ActivityService.cs`
- `src/Backend/CRM.Application/UserManagement/Services/IMentionService.cs`
- `src/Backend/CRM.Application/UserManagement/Services/MentionService.cs`
- `src/Backend/CRM.Application/UserManagement/Services/IUserBulkOperationService.cs`
- `src/Backend/CRM.Application/UserManagement/Services/UserBulkOperationService.cs`
- `src/Backend/CRM.Application/UserManagement/Services/IUserProfileService.cs`
- `src/Backend/CRM.Application/UserManagement/Services/UserProfileService.cs`
- `src/Backend/CRM.Application/UserManagement/Services/IPresenceService.cs`
- `src/Backend/CRM.Application/UserManagement/Services/PresenceService.cs`
- `src/Backend/CRM.Application/UserManagement/Services/ICustomRoleService.cs`
- `src/Backend/CRM.Application/UserManagement/Services/CustomRoleService.cs`

**Tasks**:
- Create interfaces for all 9 services
- Implement business logic for team management
- Implement business logic for user groups
- Implement task assignment logic
- Implement activity logging
- Implement mention creation and notification
- Implement bulk operations (CSV import, bulk deactivate, bulk role change)
- Implement user profile updates
- Implement presence status updates
- Implement custom role management
- All services should log activities and publish domain events

#### Step 2.5: Infrastructure Services
**Files**:
- `src/Backend/CRM.Infrastructure/UserManagement/FileStorage/IFileStorageService.cs`
- `src/Backend/CRM.Infrastructure/UserManagement/FileStorage/LocalFileStorageService.cs`
- `src/Backend/CRM.Infrastructure/UserManagement/CsvImport/ICsvImportService.cs`
- `src/Backend/CRM.Infrastructure/UserManagement/CsvImport/CsvImportService.cs`
- `src/Backend/CRM.Infrastructure/UserManagement/HtmlSanitization/IHtmlSanitizer.cs`
- `src/Backend/CRM.Infrastructure/UserManagement/HtmlSanitization/HtmlSanitizerService.cs`

**Tasks**:
- Create file storage service for avatar uploads
- Create CSV import service with validation
- Create HTML sanitization service for user-provided content
- Implement file validation (type, size, content scanning)

### Phase 3: Application Layer - Commands (Days 13-18)

**Goal**: Implement CQRS commands for all write operations.

#### Step 3.1: Team Management Commands
**Files**:
- `src/Backend/CRM.Application/UserManagement/Commands/CreateTeamCommand.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/Handlers/CreateTeamCommandHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/UpdateTeamCommand.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/Handlers/UpdateTeamCommandHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/DeleteTeamCommand.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/Handlers/DeleteTeamCommandHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/AddTeamMemberCommand.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/Handlers/AddTeamMemberCommandHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/RemoveTeamMemberCommand.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/Handlers/RemoveTeamMemberCommandHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Validators/CreateTeamCommandValidator.cs`
- `src/Backend/CRM.Application/UserManagement/Validators/UpdateTeamCommandValidator.cs`
- `src/Backend/CRM.Application/UserManagement/Validators/AddTeamMemberCommandValidator.cs`

**Tasks**:
- Create all team management commands
- Implement handlers with RBAC checks (only team leads/admins can add/remove members)
- Validate team name uniqueness per company
- Publish TeamCreated, TeamMemberAdded, TeamMemberRemoved events
- Log activities

#### Step 3.2: User Group Commands
**Files**:
- `src/Backend/CRM.Application/UserManagement/Commands/CreateUserGroupCommand.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/Handlers/CreateUserGroupCommandHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/UpdateUserGroupCommand.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/Handlers/UpdateUserGroupCommandHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/AddUserGroupMemberCommand.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/Handlers/AddUserGroupMemberCommandHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/RemoveUserGroupMemberCommand.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/Handlers/RemoveUserGroupMemberCommandHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Validators/CreateUserGroupCommandValidator.cs`

**Tasks**:
- Create all user group commands
- Implement handlers with RBAC checks (admin only)
- Validate permissions array
- Log activities

#### Step 3.3: Task Assignment Commands
**Files**:
- `src/Backend/CRM.Application/UserManagement/Commands/AssignTaskCommand.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/Handlers/AssignTaskCommandHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/UpdateTaskStatusCommand.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/Handlers/UpdateTaskStatusCommandHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/DeleteTaskAssignmentCommand.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/Handlers/DeleteTaskAssignmentCommandHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Validators/AssignTaskCommandValidator.cs`

**Tasks**:
- Create task assignment commands
- Implement handlers with RBAC checks (managers/team leads only)
- Validate entity type and ID
- Publish TaskAssigned, TaskCompleted events
- Send notifications (Spec 13 integration)
- Log activities

#### Step 3.4: Mention Commands
**Files**:
- `src/Backend/CRM.Application/UserManagement/Commands/CreateMentionCommand.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/Handlers/CreateMentionCommandHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/MarkMentionReadCommand.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/Handlers/MarkMentionReadCommandHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Validators/CreateMentionCommandValidator.cs`

**Tasks**:
- Create mention commands
- Implement handlers
- Publish UserMentioned event
- Trigger real-time notifications (Spec 13 integration)
- Log activities

#### Step 3.5: Bulk Operation Commands
**Files**:
- `src/Backend/CRM.Application/UserManagement/Commands/BulkInviteUsersCommand.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/Handlers/BulkInviteUsersCommandHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/BulkDeactivateUsersCommand.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/Handlers/BulkDeactivateUsersCommandHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/BulkChangeUserRolesCommand.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/Handlers/BulkChangeUserRolesCommandHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Validators/BulkInviteUsersCommandValidator.cs`

**Tasks**:
- Create bulk operation commands
- Implement handlers with CSV parsing
- Validate CSV format and data
- Process in batches (show progress)
- Log activities
- Return BulkOperationResultDto with success/failure counts

#### Step 3.6: User Profile Commands
**Files**:
- `src/Backend/CRM.Application/UserManagement/Commands/UpdateUserProfileCommand.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/Handlers/UpdateUserProfileCommandHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/SetOutOfOfficeCommand.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/Handlers/SetOutOfOfficeCommandHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/UpdatePresenceStatusCommand.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/Handlers/UpdatePresenceStatusCommandHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Validators/UpdateUserProfileCommandValidator.cs`

**Tasks**:
- Create user profile commands
- Implement handlers
- Validate avatar uploads
- Sanitize HTML content (bio, OOO message)
- Publish UserProfileUpdated, OutOfOfficeStatusChanged events
- Update LastSeenAt on presence updates
- Log activities

#### Step 3.7: Custom Role Commands
**Files**:
- `src/Backend/CRM.Application/UserManagement/Commands/CreateCustomRoleCommand.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/Handlers/CreateCustomRoleCommandHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/UpdateRolePermissionsCommand.cs`
- `src/Backend/CRM.Application/UserManagement/Commands/Handlers/UpdateRolePermissionsCommandHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Validators/CreateCustomRoleCommandValidator.cs`

**Tasks**:
- Create custom role commands
- Implement handlers with RBAC checks (admin only)
- Validate permissions array
- Log activities

### Phase 4: Application Layer - Queries (Days 19-22)

**Goal**: Implement CQRS queries for all read operations.

#### Step 4.1: Team Queries
**Files**:
- `src/Backend/CRM.Application/UserManagement/Queries/GetTeamsQuery.cs`
- `src/Backend/CRM.Application/UserManagement/Queries/Handlers/GetTeamsQueryHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Queries/GetTeamByIdQuery.cs`
- `src/Backend/CRM.Application/UserManagement/Queries/Handlers/GetTeamByIdQueryHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Queries/GetTeamMembersQuery.cs`
- `src/Backend/CRM.Application/UserManagement/Queries/Handlers/GetTeamMembersQueryHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Validators/GetTeamsQueryValidator.cs`

**Tasks**:
- Create team queries
- Implement handlers with filtering and pagination
- Include navigation properties (TeamLead, Members)
- Support hierarchy queries (parent teams)

#### Step 4.2: User Group Queries
**Files**:
- `src/Backend/CRM.Application/UserManagement/Queries/GetUserGroupsQuery.cs`
- `src/Backend/CRM.Application/UserManagement/Queries/Handlers/GetUserGroupsQueryHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Queries/GetUserGroupByIdQuery.cs`
- `src/Backend/CRM.Application/UserManagement/Queries/Handlers/GetUserGroupByIdQueryHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Validators/GetUserGroupsQueryValidator.cs`

**Tasks**:
- Create user group queries
- Implement handlers
- Include navigation properties (Members, CreatedBy)

#### Step 4.3: Task Assignment Queries
**Files**:
- `src/Backend/CRM.Application/UserManagement/Queries/GetUserTasksQuery.cs`
- `src/Backend/CRM.Application/UserManagement/Queries/Handlers/GetUserTasksQueryHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Validators/GetUserTasksQueryValidator.cs`

**Tasks**:
- Create task queries
- Implement handlers with filtering (status, due date, entity type)
- Include navigation properties (AssignedTo, AssignedBy)

#### Step 4.4: Activity Feed Queries
**Files**:
- `src/Backend/CRM.Application/UserManagement/Queries/GetActivityFeedQuery.cs`
- `src/Backend/CRM.Application/UserManagement/Queries/Handlers/GetActivityFeedQueryHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Queries/GetUserActivityQuery.cs`
- `src/Backend/CRM.Application/UserManagement/Queries/Handlers/GetUserActivityQueryHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Validators/GetActivityFeedQueryValidator.cs`

**Tasks**:
- Create activity feed queries
- Implement handlers with filtering (user, action type, date range)
- Support pagination
- Optimize queries with proper indexes

#### Step 4.5: Mention Queries
**Files**:
- `src/Backend/CRM.Application/UserManagement/Queries/GetMentionsQuery.cs`
- `src/Backend/CRM.Application/UserManagement/Queries/Handlers/GetMentionsQueryHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Queries/GetUnreadMentionsCountQuery.cs`
- `src/Backend/CRM.Application/UserManagement/Queries/Handlers/GetUnreadMentionsCountQueryHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Validators/GetMentionsQueryValidator.cs`

**Tasks**:
- Create mention queries
- Implement handlers with filtering (read/unread)
- Support pagination
- Include navigation properties (MentionedUser, MentionedBy)

#### Step 4.6: Permission & Role Queries
**Files**:
- `src/Backend/CRM.Application/UserManagement/Queries/GetAvailablePermissionsQuery.cs`
- `src/Backend/CRM.Application/UserManagement/Queries/Handlers/GetAvailablePermissionsQueryHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Queries/GetCustomRolesQuery.cs`
- `src/Backend/CRM.Application/UserManagement/Queries/Handlers/GetCustomRolesQueryHandler.cs`
- `src/Backend/CRM.Application/UserManagement/Validators/GetCustomRolesQueryValidator.cs`

**Tasks**:
- Create permission and role queries
- Implement handlers
- Return list of all available permissions
- Return custom roles with permissions

### Phase 5: API Endpoints (Days 23-26)

**Goal**: Create REST API controllers for all endpoints.

#### Step 5.1: Teams Controller
**File**: `src/Backend/CRM.Api/Controllers/TeamsController.cs`

**Tasks**:
- Create controller with route `/api/v1/teams`
- Add `[Authorize]` attribute
- Implement POST `/teams` (CreateTeamCommand)
- Implement GET `/teams` (GetTeamsQuery)
- Implement GET `/teams/{teamId}` (GetTeamByIdQuery)
- Implement PUT `/teams/{teamId}` (UpdateTeamCommand)
- Implement DELETE `/teams/{teamId}` (DeleteTeamCommand)
- Implement POST `/teams/{teamId}/members` (AddTeamMemberCommand)
- Implement DELETE `/teams/{teamId}/members/{userId}` (RemoveTeamMemberCommand)
- Add proper error handling and validation
- Return consistent API response format

#### Step 5.2: User Groups Controller
**File**: `src/Backend/CRM.Api/Controllers/UserGroupsController.cs`

**Tasks**:
- Create controller with route `/api/v1/user-groups`
- Add `[Authorize(Roles = "Admin")]` attribute
- Implement POST `/user-groups` (CreateUserGroupCommand)
- Implement GET `/user-groups` (GetUserGroupsQuery)
- Implement GET `/user-groups/{groupId}` (GetUserGroupByIdQuery)
- Implement PUT `/user-groups/{groupId}` (UpdateUserGroupCommand)
- Implement POST `/user-groups/{groupId}/members` (AddUserGroupMemberCommand)
- Implement DELETE `/user-groups/{groupId}/members/{userId}` (RemoveUserGroupMemberCommand)
- Add proper error handling and validation

#### Step 5.3: Task Assignments Controller
**File**: `src/Backend/CRM.Api/Controllers/TaskAssignmentsController.cs`

**Tasks**:
- Create controller with route `/api/v1/task-assignments`
- Add `[Authorize]` attribute
- Implement POST `/task-assignments` (AssignTaskCommand)
- Implement GET `/task-assignments/user/{userId}` (GetUserTasksQuery)
- Implement PUT `/task-assignments/{assignmentId}/status` (UpdateTaskStatusCommand)
- Implement DELETE `/task-assignments/{assignmentId}` (DeleteTaskAssignmentCommand)
- Add proper error handling and validation

#### Step 5.4: Activity Feed Controller
**File**: `src/Backend/CRM.Api/Controllers/ActivityFeedController.cs`

**Tasks**:
- Create controller with route `/api/v1/activity-feed`
- Add `[Authorize]` attribute
- Implement GET `/activity-feed` (GetActivityFeedQuery)
- Implement GET `/users/{userId}/activity` (GetUserActivityQuery)
- Add proper error handling and validation

#### Step 5.5: Mentions Controller
**File**: `src/Backend/CRM.Api/Controllers/MentionsController.cs`

**Tasks**:
- Create controller with route `/api/v1/mentions`
- Add `[Authorize]` attribute
- Implement POST `/mentions` (CreateMentionCommand)
- Implement GET `/mentions/user/{userId}` (GetMentionsQuery)
- Implement PUT `/mentions/{mentionId}/mark-read` (MarkMentionReadCommand)
- Add proper error handling and validation

#### Step 5.6: User Bulk Operations Controller
**File**: `src/Backend/CRM.Api/Controllers/UserBulkOperationsController.cs`

**Tasks**:
- Create controller with route `/api/v1/users`
- Add `[Authorize(Roles = "Admin")]` attribute
- Implement POST `/users/bulk-invite` (BulkInviteUsersCommand)
- Implement POST `/users/bulk-deactivate` (BulkDeactivateUsersCommand)
- Implement POST `/users/bulk-role-change` (BulkChangeUserRolesCommand)
- Implement GET `/users/export` (export to CSV/Excel)
- Add proper error handling and validation

#### Step 5.7: User Profile Controller
**File**: `src/Backend/CRM.Api/Controllers/UserProfileController.cs`

**Tasks**:
- Create controller with route `/api/v1/users`
- Add `[Authorize]` attribute
- Implement PUT `/users/{userId}/profile` (UpdateUserProfileCommand)
- Implement PUT `/users/{userId}/out-of-office` (SetOutOfOfficeCommand)
- Implement PUT `/users/{userId}/presence` (UpdatePresenceStatusCommand)
- Add proper error handling and validation

#### Step 5.8: Custom Roles Controller
**File**: `src/Backend/CRM.Api/Controllers/CustomRolesController.cs`

**Tasks**:
- Create controller with route `/api/v1/roles`
- Add `[Authorize(Roles = "Admin")]` attribute
- Implement POST `/roles/custom` (CreateCustomRoleCommand)
- Implement GET `/roles/permissions` (GetAvailablePermissionsQuery)
- Implement PUT `/roles/{roleId}/permissions` (UpdateRolePermissionsCommand)
- Add proper error handling and validation

#### Step 5.9: Register Services in Program.cs
**File**: `src/Backend/CRM.Api/Program.cs`

**Tasks**:
- Register all services (TeamService, UserGroupService, etc.)
- Register all command handlers
- Register all query handlers
- Register all validators
- Register all event handlers
- Register SignalR services

### Phase 6: Real-Time Presence (Days 27-28)

**Goal**: Implement SignalR hub for real-time presence updates.

#### Step 6.1: Presence Hub
**File**: `src/Backend/CRM.Api/Hubs/PresenceHub.cs`

**Tasks**:
- Create SignalR hub for presence
- Implement `OnConnectedAsync()` - authenticate user, add to group, update presence to ONLINE
- Implement `OnDisconnectedAsync()` - remove from group, update presence to OFFLINE
- Create method `UpdatePresence()` - update user presence status
- Handle connection management (user groups, connection tracking)
- Update LastSeenAt on connection/disconnection

#### Step 6.2: Register SignalR in Program.cs
**File**: `src/Backend/CRM.Api/Program.cs`

**Tasks**:
- Add SignalR services: `builder.Services.AddSignalR()`
- Map hub: `app.MapHub<PresenceHub>("/ws/presence")`
- Configure CORS for WebSocket connections
- Add authentication for SignalR

### Phase 7: Frontend API Integration (Days 29-31)

**Goal**: Create TypeScript API client and types.

#### Step 7.1: TypeScript Types
**File**: `src/Frontend/web/src/types/userManagement.ts`

**Tasks**:
- Create interfaces: Team, TeamMember, UserGroup, TaskAssignment, UserActivity, Mention, UserProfile, CustomRole, Permission
- Create request types: CreateTeamRequest, UpdateTeamRequest, AssignTaskRequest, etc.
- Create response types: PagedActivityFeedResult, BulkOperationResult
- Match backend DTOs exactly

#### Step 7.2: API Client
**File**: `src/Frontend/web/src/lib/api/userManagement.ts`

**Tasks**:
- Add `UserManagementApi` object with methods for all endpoints:
  - Teams: `createTeam()`, `getTeams()`, `getTeamById()`, `updateTeam()`, `deleteTeam()`, `addTeamMember()`, `removeTeamMember()`
  - User Groups: `createUserGroup()`, `getUserGroups()`, `getUserGroupById()`, `updateUserGroup()`, `addUserGroupMember()`, `removeUserGroupMember()`
  - Tasks: `assignTask()`, `getUserTasks()`, `updateTaskStatus()`, `deleteTaskAssignment()`
  - Activity: `getActivityFeed()`, `getUserActivity()`
  - Mentions: `createMention()`, `getMentions()`, `markMentionRead()`
  - Bulk Operations: `bulkInviteUsers()`, `bulkDeactivateUsers()`, `bulkChangeUserRoles()`, `exportUsers()`
  - Profile: `updateUserProfile()`, `setOutOfOffice()`, `updatePresence()`
  - Roles: `createCustomRole()`, `getAvailablePermissions()`, `updateRolePermissions()`

#### Step 7.3: WebSocket Client Hook
**File**: `src/Frontend/web/src/hooks/usePresence.ts`

**Tasks**:
- Create React hook using SignalR client
- Connect to `/ws/presence` on mount
- Handle authentication token
- Listen for presence updates
- Return connection status and presence update callback
- Handle reconnection logic

### Phase 8: Frontend Core Components (Days 32-40)

**Goal**: Build reusable UI components.

#### Step 8.1: Team Components
**Files**:
- `src/Frontend/web/src/components/user-management/TeamHierarchyTree.tsx`
- `src/Frontend/web/src/components/user-management/TeamMemberList.tsx`

**Tasks**:
- Create team hierarchy tree component (nested teams with expand/collapse)
- Create team member list component (shows members with roles and presence)
- Add/remove member functionality
- Responsive design

#### Step 8.2: Presence Components
**Files**:
- `src/Frontend/web/src/components/user-management/UserAvatarWithPresence.tsx`
- `src/Frontend/web/src/components/user-management/PresenceIndicator.tsx`

**Tasks**:
- Create avatar component with presence dot (online/offline/busy)
- Create presence indicator component
- Real-time updates via WebSocket
- Accessible (ARIA labels)

#### Step 8.3: Activity Feed Components
**Files**:
- `src/Frontend/web/src/components/user-management/ActivityFeedItem.tsx`
- `src/Frontend/web/src/components/user-management/ActivityFeedFilter.tsx`

**Tasks**:
- Create activity feed item component (shows icon, user, action, timestamp)
- Create activity feed filter component (user, action type, date range)
- Click activity to view related entity
- Loading skeleton

#### Step 8.4: Mention Components
**Files**:
- `src/Frontend/web/src/components/user-management/MentionAutocomplete.tsx`
- `src/Frontend/web/src/components/user-management/MentionBadge.tsx`

**Tasks**:
- Create mention autocomplete component (typeahead for @mentions)
- Create mention badge component (unread mentions count)
- Integrate with comment/note editors
- Accessible

#### Step 8.5: Task Components
**Files**:
- `src/Frontend/web/src/components/user-management/TaskCard.tsx`
- `src/Frontend/web/src/components/user-management/TaskAssignmentForm.tsx`

**Tasks**:
- Create task card component (shows task, assignee, due date, status)
- Create task assignment form component
- Status update functionality
- Filter by status, due date, entity type

#### Step 8.6: User Group Components
**File**: `src/Frontend/web/src/components/user-management/UserGroupCard.tsx`

**Tasks**:
- Create user group card component (group name, member count, permissions summary)
- Add/remove users functionality
- Permission management UI

#### Step 8.7: Bulk Operation Components
**File**: `src/Frontend/web/src/components/user-management/BulkActionModal.tsx`

**Tasks**:
- Create bulk action modal component (select action, confirm)
- CSV import preview
- Progress indicator
- Error handling

#### Step 8.8: Profile Components
**Files**:
- `src/Frontend/web/src/components/user-management/SkillTagInput.tsx`
- `src/Frontend/web/src/components/user-management/OutOfOfficeToggle.tsx`

**Tasks**:
- Create skill tag input component (add/remove skills with autocomplete)
- Create out-of-office toggle component (OOO status with message and delegate selector)
- Avatar upload component (drag-drop or file picker)
- Bio editor (textarea with char count)
- Social links inputs (LinkedIn, Twitter)

### Phase 9: Frontend Pages (Days 41-48)

**Goal**: Create all user management pages.

#### Step 9.1: Teams Management Page
**File**: `src/Frontend/web/src/app/(protected)/admin/teams/page.tsx`

**Tasks**:
- Create teams management page
- List all teams (table with search, filter by active/inactive)
- Create/Edit team modal
- View team hierarchy
- Add/remove members
- Bulk invite to team via CSV

#### Step 9.2: Team Dashboard Page
**File**: `src/Frontend/web/src/app/(protected)/teams/[teamId]/page.tsx`

**Tasks**:
- Create team dashboard page
- Team overview (member count, active tasks, recent activity)
- Member list with avatars, roles, presence indicators
- Activity feed for team
- Shared workspace/dashboard (team-specific metrics)

#### Step 9.3: User Groups Management Page
**File**: `src/Frontend/web/src/app/(protected)/admin/user-groups/page.tsx`

**Tasks**:
- Create user groups management page
- List groups, create/edit/delete
- Assign permissions to group (checkboxes for all permissions)
- Add/remove users to group

#### Step 9.4: Task Assignment Page
**File**: `src/Frontend/web/src/app/(protected)/tasks/page.tsx`

**Tasks**:
- Create task assignment page
- My tasks page (assigned to me)
- Create task assignment modal
- Task status update
- Filter by status, due date, entity type

#### Step 9.5: Activity Feed Page
**File**: `src/Frontend/web/src/app/(protected)/activity/page.tsx`

**Tasks**:
- Create activity feed page
- Real-time activity stream (WebSocket or polling)
- Filters: by user, action type, date range
- Search and pagination
- Click activity to view related entity

#### Step 9.6: Enhanced User Profile Page
**File**: `src/Frontend/web/src/app/(protected)/profile/page.tsx`

**Tasks**:
- Create enhanced user profile page
- Avatar upload
- Bio editor
- Social links inputs
- Skills/tags
- Out-of-office toggle
- Presence status dropdown
- Activity history tab

#### Step 9.7: Mentions Page
**File**: `src/Frontend/web/src/app/(protected)/mentions/page.tsx`

**Tasks**:
- Create mentions page
- List all mentions with read/unread status
- Click mention to view context (comment/note)
- Unread mentions indicator (badge on bell icon)

#### Step 9.8: Bulk User Operations Page
**File**: `src/Frontend/web/src/app/(protected)/admin/users/bulk/page.tsx`

**Tasks**:
- Create bulk user operations page
- CSV import modal (upload file, preview, validate, import)
- Bulk deactivate (select users, confirm)
- Bulk role change (select users, choose role)
- Export users to CSV/Excel

#### Step 9.9: Custom Role Builder Page
**File**: `src/Frontend/web/src/app/(protected)/admin/roles/custom/page.tsx`

**Tasks**:
- Create custom role builder page
- Create custom role form (name, description)
- Permission checklist (grouped by module)
- Save and assign to users

### Phase 10: Testing & Polish (Days 49-55)

**Goal**: Write tests and polish the implementation.

#### Step 10.1: Backend Unit Tests
**Files** (in `tests/CRM.Tests/UserManagement/`):
- `TeamServiceTests.cs`
- `UserGroupServiceTests.cs`
- `TaskAssignmentServiceTests.cs`
- `ActivityServiceTests.cs`
- `MentionServiceTests.cs`
- `UserBulkOperationServiceTests.cs`
- `CreateTeamCommandHandlerTests.cs`
- `AssignTaskCommandHandlerTests.cs`
- `GetActivityFeedQueryHandlerTests.cs`

**Tasks**:
- Test all services
- Test command handlers (team creation, task assignment, etc.)
- Test query handlers (filtering, pagination)
- Mock dependencies appropriately
- Test RBAC enforcement
- Test validation logic

#### Step 10.2: Backend Integration Tests
**File**: `tests/CRM.Tests.Integration/UserManagement/Controllers/`

**Tasks**:
- Test all API endpoints
- Test authorization (RBAC enforcement)
- Test filtering and pagination
- Test bulk operations
- Test file uploads (avatars, CSV)
- Test WebSocket presence updates

#### Step 10.3: Frontend Component Tests
**Files** (in `src/Frontend/web/src/components/user-management/__tests__/`):
- `TeamHierarchyTree.test.tsx`
- `UserAvatarWithPresence.test.tsx`
- `ActivityFeedItem.test.tsx`
- `MentionAutocomplete.test.tsx`
- `TaskCard.test.tsx`

**Tasks**:
- Test component rendering
- Test user interactions (click, filter, mark read)
- Test WebSocket integration
- Mock API calls

#### Step 10.4: E2E Tests
**Files** (in `tests/e2e/user-management/`):
- `teams.spec.ts` - Create team → Add members → Assign task → Complete task
- `bulk-operations.spec.ts` - Bulk import users → Assign to groups → Verify permissions
- `mentions.spec.ts` - @mention in comment → Verify notification → Mark as read

**Tasks**:
- E2E test for team management workflow
- E2E test for bulk operations workflow
- E2E test for mentions workflow
- E2E test for activity feed
- E2E test for presence updates

#### Step 10.5: Error Boundaries & Loading States
**Files**:
- `src/Frontend/web/src/components/user-management/ErrorBoundary.tsx`
- `src/Frontend/web/src/components/user-management/LoadingSkeleton.tsx`

**Tasks**:
- Create error boundary for user management pages
- Create loading skeletons for all pages
- Integrate into pages

#### Step 10.6: Accessibility & Responsive Design
**Tasks**:
- Add ARIA labels to all interactive elements
- Test keyboard navigation
- Test screen reader compatibility
- Verify mobile responsiveness
- Test on different screen sizes

#### Step 10.7: Documentation
**Files**:
- `specs/019-advanced-user-management-team-collaboration/quickstart.md` (update)
- `specs/019-advanced-user-management-team-collaboration/checklists/requirements.md` (create)

**Tasks**:
- Update quickstart guide
- Create requirements checklist
- Document WebSocket connection setup
- Document bulk operations procedures

## Dependencies

- **Spec-003**: UserAuthentication & RBAC (for authentication and authorization)
- **Spec-013**: NotificationSystem (for mentions and task assignment notifications)
- **Spec-018**: SystemAdministration (for audit logs integration)
- **Existing**: Email infrastructure (FluentEmail), Authentication (JWT), File storage

## Key Technical Decisions

1. **Real-Time Presence**: SignalR for .NET backend, @microsoft/signalr for frontend
2. **Team Hierarchy**: Recursive CTE queries for efficient hierarchy traversal
3. **Activity Feed**: Pagination with cursor-based pagination for better performance
4. **CSV Import**: CsvHelper library for parsing, validation, and error reporting
5. **File Storage**: Local filesystem for development, S3 for production (configurable)
6. **Presence Updates**: WebSocket for real-time updates, polling fallback for disconnected clients
7. **Bulk Operations**: Background job processing for large batches (100+ users)

## Verification Checklist

- [ ] All database tables created with proper indexes
- [ ] All domain entities and enums created
- [ ] All services implemented with proper business logic
- [ ] All commands and queries implemented
- [ ] All API endpoints functional with proper RBAC
- [ ] Activity logging accurate for all actions
- [ ] Mentions trigger notifications correctly
- [ ] Bulk operations work without errors
- [ ] Real-time presence updates work via WebSocket
- [ ] Frontend pages display correctly
- [ ] All components functional and accessible
- [ ] Mobile responsive design works
- [ ] All tests pass (≥85% backend, ≥80% frontend)
- [ ] E2E workflows tested and verified

---

**Estimated Total Duration**: 55 days  
**Team Size**: 3-4 developers (2 backend, 1-2 frontend)  
**Critical Path**: Phases 1 → 2 → 3 → 5 → 6 → 8 → 9

