# Tasks: Advanced User Management & Team Collaboration Features (Spec-019)

**Input**: Design documents from `/specs/019-advanced-user-management-team-collaboration/`
**Prerequisites**: plan.md (required), spec.md (required for user stories)

**Tests**: Tests are OPTIONAL - only include them if explicitly requested in the feature specification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Backend**: `src/Backend/CRM.*/`
- **Frontend**: `src/Frontend/web/src/`
- **Tests**: `tests/CRM.Tests*/`
- **Migrations**: `src/Backend/CRM.Migrator/Migrations/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create UserManagement folder structure in CRM.Domain/UserManagement/
- [X] T002 Create UserManagement folder structure in CRM.Application/UserManagement/
- [X] T003 [P] Create UserManagement folder structure in CRM.Infrastructure/UserManagement/
- [X] T004 [P] Create UserManagement folder structure in CRM.Api/Controllers/ and CRM.Api/Hubs/
- [X] T005 [P] Create UserManagement folder structure in tests/CRM.Tests/UserManagement/
- [X] T006 [P] Create UserManagement folder structure in tests/CRM.Tests.Integration/UserManagement/

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T007 Create database migration for Team table in src/Backend/CRM.Infrastructure/Migrations/[timestamp]_AddUserManagementTables.cs
- [X] T008 Create database migration for TeamMember table in src/Backend/CRM.Infrastructure/Migrations/[timestamp]_AddUserManagementTables.cs
- [X] T009 Create database migration for UserGroup table in src/Backend/CRM.Infrastructure/Migrations/[timestamp]_AddUserManagementTables.cs
- [X] T010 Create database migration for UserGroupMember table in src/Backend/CRM.Infrastructure/Migrations/[timestamp]_AddUserManagementTables.cs
- [X] T011 Create database migration for TaskAssignment table in src/Backend/CRM.Infrastructure/Migrations/[timestamp]_AddUserManagementTables.cs
- [X] T012 Create database migration for UserActivity table in src/Backend/CRM.Infrastructure/Migrations/[timestamp]_AddUserManagementTables.cs
- [X] T013 Create database migration for Mention table in src/Backend/CRM.Infrastructure/Migrations/[timestamp]_AddUserManagementTables.cs
- [X] T014 Create database migration to alter Users table (add AvatarUrl, Bio, LinkedInUrl, TwitterUrl, Skills, OutOfOfficeStatus, OutOfOfficeMessage, DelegateUserId, LastSeenAt, PresenceStatus) in src/Backend/CRM.Infrastructure/Migrations/[timestamp]_AddUserManagementTables.cs
- [X] T015 Add all foreign keys and indexes to migration in src/Backend/CRM.Infrastructure/Migrations/[timestamp]_AddUserManagementTables.cs
- [X] T016 Create Team domain entity in src/Backend/CRM.Domain/UserManagement/Team.cs
- [X] T017 Create TeamMember domain entity in src/Backend/CRM.Domain/UserManagement/TeamMember.cs
- [X] T018 Create UserGroup domain entity in src/Backend/CRM.Domain/UserManagement/UserGroup.cs
- [X] T019 Create UserGroupMember domain entity in src/Backend/CRM.Domain/UserManagement/UserGroupMember.cs
- [X] T020 Create TaskAssignment domain entity in src/Backend/CRM.Domain/UserManagement/TaskAssignment.cs
- [X] T021 Create UserActivity domain entity in src/Backend/CRM.Domain/UserManagement/UserActivity.cs
- [X] T022 Create Mention domain entity in src/Backend/CRM.Domain/UserManagement/Mention.cs
- [X] T023 Create TaskAssignmentStatus enum in src/Backend/CRM.Domain/UserManagement/TaskAssignment.cs
- [X] T024 Create PresenceStatus enum in src/Backend/CRM.Domain/Enums/PresenceStatus.cs
- [X] T025 Update User domain entity with new profile properties in src/Backend/CRM.Domain/Entities/User.cs
- [X] T026 Create TeamEntityConfiguration in src/Backend/CRM.Infrastructure/EntityConfigurations/TeamEntityConfiguration.cs
- [X] T027 Create TeamMemberEntityConfiguration in src/Backend/CRM.Infrastructure/EntityConfigurations/TeamMemberEntityConfiguration.cs
- [X] T028 Create UserGroupEntityConfiguration in src/Backend/CRM.Infrastructure/EntityConfigurations/UserGroupEntityConfiguration.cs
- [X] T029 Create UserGroupMemberEntityConfiguration in src/Backend/CRM.Infrastructure/EntityConfigurations/UserGroupMemberEntityConfiguration.cs
- [X] T030 Create TaskAssignmentEntityConfiguration in src/Backend/CRM.Infrastructure/EntityConfigurations/TaskAssignmentEntityConfiguration.cs
- [X] T031 Create UserActivityEntityConfiguration in src/Backend/CRM.Infrastructure/EntityConfigurations/UserActivityEntityConfiguration.cs
- [X] T032 Create MentionEntityConfiguration in src/Backend/CRM.Infrastructure/EntityConfigurations/MentionEntityConfiguration.cs
- [X] T033 Add DbSet<Team> Teams to AppDbContext in src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs
- [X] T034 Add DbSet<TeamMember> TeamMembers to AppDbContext in src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs
- [X] T035 Add DbSet<UserGroup> UserGroups to AppDbContext in src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs
- [X] T036 Add DbSet<UserGroupMember> UserGroupMembers to AppDbContext in src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs
- [X] T037 Add DbSet<TaskAssignment> TaskAssignments to AppDbContext in src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs
- [X] T038 Add DbSet<UserActivity> UserActivities to AppDbContext in src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs
- [X] T039 Add DbSet<Mention> Mentions to AppDbContext in src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs
- [X] T040 Update IAppDbContext interface with new DbSets in src/Backend/CRM.Application/Common/Persistence/IAppDbContext.cs
- [X] T041 Create domain events: TeamCreated, TeamMemberAdded, TeamMemberRemoved, TaskAssigned, TaskCompleted, UserMentioned, UserActivityLogged, UserProfileUpdated, OutOfOfficeStatusChanged in src/Backend/CRM.Domain/UserManagement/Events/

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Team Management (Priority: P1) 🎯 MVP

**Goal**: Enable admins/team leads to create teams, assign members, and manage team hierarchies

**Independent Test**: Create a team, add members, view team hierarchy, and verify team lead can manage members independently

### Implementation for User Story 1

- [X] T042 [P] [US1] Create TeamDto in src/Backend/CRM.Application/UserManagement/DTOs/TeamDto.cs
- [X] T043 [P] [US1] Create TeamMemberDto in src/Backend/CRM.Application/UserManagement/DTOs/TeamMemberDto.cs
- [X] T044 [P] [US1] Create CreateTeamRequest in src/Backend/CRM.Application/UserManagement/Requests/CreateTeamRequest.cs
- [X] T045 [P] [US1] Create UpdateTeamRequest in src/Backend/CRM.Application/UserManagement/Requests/UpdateTeamRequest.cs
- [X] T046 [P] [US1] Create AddTeamMemberRequest in src/Backend/CRM.Application/UserManagement/Requests/AddTeamMemberRequest.cs
- [ ] T047 [US1] Create ITeamService interface in src/Backend/CRM.Application/UserManagement/Services/ITeamService.cs
- [ ] T048 [US1] Implement TeamService with business logic in src/Backend/CRM.Application/UserManagement/Services/TeamService.cs
- [X] T049 [US1] Create CreateTeamCommand in src/Backend/CRM.Application/UserManagement/Commands/CreateTeamCommand.cs
- [X] T050 [US1] Create CreateTeamCommandHandler in src/Backend/CRM.Application/UserManagement/Commands/Handlers/CreateTeamCommandHandler.cs
- [X] T051 [US1] Create UpdateTeamCommand in src/Backend/CRM.Application/UserManagement/Commands/UpdateTeamCommand.cs
- [X] T052 [US1] Create UpdateTeamCommandHandler in src/Backend/CRM.Application/UserManagement/Commands/Handlers/UpdateTeamCommandHandler.cs
- [X] T053 [US1] Create DeleteTeamCommand in src/Backend/CRM.Application/UserManagement/Commands/DeleteTeamCommand.cs
- [X] T054 [US1] Create DeleteTeamCommandHandler in src/Backend/CRM.Application/UserManagement/Commands/Handlers/DeleteTeamCommandHandler.cs
- [X] T055 [US1] Create AddTeamMemberCommand in src/Backend/CRM.Application/UserManagement/Commands/AddTeamMemberCommand.cs
- [X] T056 [US1] Create AddTeamMemberCommandHandler in src/Backend/CRM.Application/UserManagement/Commands/Handlers/AddTeamMemberCommandHandler.cs
- [X] T057 [US1] Create RemoveTeamMemberCommand in src/Backend/CRM.Application/UserManagement/Commands/RemoveTeamMemberCommand.cs
- [X] T058 [US1] Create RemoveTeamMemberCommandHandler in src/Backend/CRM.Application/UserManagement/Commands/Handlers/RemoveTeamMemberCommandHandler.cs
- [X] T059 [US1] Create GetTeamsQuery in src/Backend/CRM.Application/UserManagement/Queries/GetTeamsQuery.cs
- [X] T060 [US1] Create GetTeamsQueryHandler in src/Backend/CRM.Application/UserManagement/Queries/Handlers/GetTeamsQueryHandler.cs
- [X] T061 [US1] Create GetTeamByIdQuery in src/Backend/CRM.Application/UserManagement/Queries/GetTeamByIdQuery.cs
- [X] T062 [US1] Create GetTeamByIdQueryHandler in src/Backend/CRM.Application/UserManagement/Queries/Handlers/GetTeamByIdQueryHandler.cs
- [X] T063 [US1] Create GetTeamMembersQuery in src/Backend/CRM.Application/UserManagement/Queries/GetTeamMembersQuery.cs
- [X] T064 [US1] Create GetTeamMembersQueryHandler in src/Backend/CRM.Application/UserManagement/Queries/Handlers/GetTeamMembersQueryHandler.cs
- [X] T065 [US1] Create CreateTeamRequestValidator in src/Backend/CRM.Application/UserManagement/Validators/CreateTeamRequestValidator.cs
- [X] T066 [US1] Create UpdateTeamRequestValidator in src/Backend/CRM.Application/UserManagement/Validators/UpdateTeamRequestValidator.cs
- [X] T067 [US1] Create AddTeamMemberRequestValidator in src/Backend/CRM.Application/UserManagement/Validators/AddTeamMemberRequestValidator.cs
- [X] T068 [US1] Create TeamsController with all endpoints in src/Backend/CRM.Api/Controllers/TeamsController.cs
- [X] T069 [US1] Add AutoMapper mappings for Team and TeamMember in src/Backend/CRM.Application/Mapping/UserManagementProfile.cs
- [X] T070 [P] [US1] Create TypeScript types for Team and TeamMember in src/Frontend/web/src/types/userManagement.ts
- [X] T071 [P] [US1] Create API client methods for teams in src/Frontend/web/src/lib/api.ts
- [X] T072 [P] [US1] Create TeamHierarchyTree component in src/Frontend/web/src/components/user-management/TeamHierarchyTree.tsx
- [X] T073 [P] [US1] Create TeamMemberList component in src/Frontend/web/src/components/user-management/TeamMemberList.tsx
- [X] T074 [US1] Create Teams Management page in src/Frontend/web/src/app/(protected)/admin/teams/page.tsx
- [X] T075 [US1] Create Team Detail page in src/Frontend/web/src/app/(protected)/admin/teams/[teamId]/page.tsx
- [X] T076 [US1] Create Team Dashboard page in src/Frontend/web/src/app/(protected)/teams/[teamId]/page.tsx
- [X] T077 [US1] Register command/query handlers in src/Backend/CRM.Api/Program.cs

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - User Groups (Priority: P2)

**Goal**: Enable admins to create user groups with custom permissions and manage group membership

**Independent Test**: Create a user group, assign permissions, add users to group, and verify permissions are applied

### Implementation for User Story 2

- [X] T078 [P] [US2] Create UserGroupDto in src/Backend/CRM.Application/UserManagement/DTOs/UserGroupDto.cs
- [X] T079 [P] [US2] Create UserGroupMemberDto in src/Backend/CRM.Application/UserManagement/DTOs/UserGroupMemberDto.cs
- [X] T080 [P] [US2] Create CreateUserGroupRequest in src/Backend/CRM.Application/UserManagement/Requests/CreateUserGroupRequest.cs
- [X] T081 [P] [US2] Create UpdateUserGroupRequest in src/Backend/CRM.Application/UserManagement/Requests/UpdateUserGroupRequest.cs
- [ ] T082 [US2] Create IUserGroupService interface in src/Backend/CRM.Application/UserManagement/Services/IUserGroupService.cs
- [ ] T083 [US2] Implement UserGroupService with business logic in src/Backend/CRM.Application/UserManagement/Services/UserGroupService.cs
- [ ] T084 [US2] Create CreateUserGroupCommand in src/Backend/CRM.Application/UserManagement/Commands/CreateUserGroupCommand.cs
- [ ] T085 [US2] Create CreateUserGroupCommandHandler in src/Backend/CRM.Application/UserManagement/Commands/Handlers/CreateUserGroupCommandHandler.cs
- [ ] T086 [US2] Create UpdateUserGroupCommand in src/Backend/CRM.Application/UserManagement/Commands/UpdateUserGroupCommand.cs
- [ ] T087 [US2] Create UpdateUserGroupCommandHandler in src/Backend/CRM.Application/UserManagement/Commands/Handlers/UpdateUserGroupCommandHandler.cs
- [ ] T088 [US2] Create AddUserGroupMemberCommand in src/Backend/CRM.Application/UserManagement/Commands/AddUserGroupMemberCommand.cs
- [ ] T089 [US2] Create AddUserGroupMemberCommandHandler in src/Backend/CRM.Application/UserManagement/Commands/Handlers/AddUserGroupMemberCommandHandler.cs
- [ ] T090 [US2] Create RemoveUserGroupMemberCommand in src/Backend/CRM.Application/UserManagement/Commands/RemoveUserGroupMemberCommand.cs
- [ ] T091 [US2] Create RemoveUserGroupMemberCommandHandler in src/Backend/CRM.Application/UserManagement/Commands/Handlers/RemoveUserGroupMemberCommandHandler.cs
- [ ] T092 [US2] Create GetUserGroupsQuery in src/Backend/CRM.Application/UserManagement/Queries/GetUserGroupsQuery.cs
- [ ] T093 [US2] Create GetUserGroupsQueryHandler in src/Backend/CRM.Application/UserManagement/Queries/Handlers/GetUserGroupsQueryHandler.cs
- [ ] T094 [US2] Create GetUserGroupByIdQuery in src/Backend/CRM.Application/UserManagement/Queries/GetUserGroupByIdQuery.cs
- [ ] T095 [US2] Create GetUserGroupByIdQueryHandler in src/Backend/CRM.Application/UserManagement/Queries/Handlers/GetUserGroupByIdQueryHandler.cs
- [ ] T096 [US2] Create CreateUserGroupRequestValidator in src/Backend/CRM.Application/UserManagement/Validators/CreateUserGroupRequestValidator.cs
- [ ] T097 [US2] Create UserGroupsController with all endpoints in src/Backend/CRM.Api/Controllers/UserGroupsController.cs
- [ ] T098 [US2] Add AutoMapper mappings for UserGroup in src/Backend/CRM.Application/Mapping/UserManagementProfile.cs
- [ ] T099 [P] [US2] Create TypeScript types for UserGroup in src/Frontend/web/src/types/userManagement.ts
- [ ] T100 [P] [US2] Create API client methods for user groups in src/Frontend/web/src/lib/api/userManagement.ts
- [ ] T101 [P] [US2] Create UserGroupCard component in src/Frontend/web/src/components/user-management/UserGroupCard.tsx
- [ ] T102 [US2] Create User Groups Management page in src/Frontend/web/src/app/(protected)/admin/user-groups/page.tsx
- [ ] T103 [US2] Register UserGroupService and command/query handlers in src/Backend/CRM.Api/Program.cs

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 - Task Assignment (Priority: P2)

**Goal**: Enable managers/team leads to assign tasks to users and track task status

**Independent Test**: Assign a task to a user, update task status, view user's tasks, and verify notifications are sent

### Implementation for User Story 3

- [ ] T104 [P] [US3] Create TaskAssignmentDto in src/Backend/CRM.Application/UserManagement/DTOs/TaskAssignmentDto.cs
- [ ] T105 [P] [US3] Create AssignTaskRequest in src/Backend/CRM.Application/UserManagement/Requests/AssignTaskRequest.cs
- [ ] T106 [P] [US3] Create UpdateTaskStatusRequest in src/Backend/CRM.Application/UserManagement/Requests/UpdateTaskStatusRequest.cs
- [ ] T107 [US3] Create ITaskAssignmentService interface in src/Backend/CRM.Application/UserManagement/Services/ITaskAssignmentService.cs
- [ ] T108 [US3] Implement TaskAssignmentService with business logic in src/Backend/CRM.Application/UserManagement/Services/TaskAssignmentService.cs
- [ ] T109 [US3] Create AssignTaskCommand in src/Backend/CRM.Application/UserManagement/Commands/AssignTaskCommand.cs
- [ ] T110 [US3] Create AssignTaskCommandHandler in src/Backend/CRM.Application/UserManagement/Commands/Handlers/AssignTaskCommandHandler.cs
- [ ] T111 [US3] Create UpdateTaskStatusCommand in src/Backend/CRM.Application/UserManagement/Commands/UpdateTaskStatusCommand.cs
- [ ] T112 [US3] Create UpdateTaskStatusCommandHandler in src/Backend/CRM.Application/UserManagement/Commands/Handlers/UpdateTaskStatusCommandHandler.cs
- [ ] T113 [US3] Create DeleteTaskAssignmentCommand in src/Backend/CRM.Application/UserManagement/Commands/DeleteTaskAssignmentCommand.cs
- [ ] T114 [US3] Create DeleteTaskAssignmentCommandHandler in src/Backend/CRM.Application/UserManagement/Commands/Handlers/DeleteTaskAssignmentCommandHandler.cs
- [ ] T115 [US3] Create GetUserTasksQuery in src/Backend/CRM.Application/UserManagement/Queries/GetUserTasksQuery.cs
- [ ] T116 [US3] Create GetUserTasksQueryHandler in src/Backend/CRM.Application/UserManagement/Queries/Handlers/GetUserTasksQueryHandler.cs
- [ ] T117 [US3] Create AssignTaskRequestValidator in src/Backend/CRM.Application/UserManagement/Validators/AssignTaskRequestValidator.cs
- [ ] T118 [US3] Create TaskAssignmentsController with all endpoints in src/Backend/CRM.Api/Controllers/TaskAssignmentsController.cs
- [ ] T119 [US3] Add AutoMapper mappings for TaskAssignment in src/Backend/CRM.Application/Mapping/UserManagementProfile.cs
- [ ] T120 [P] [US3] Create TypeScript types for TaskAssignment in src/Frontend/web/src/types/userManagement.ts
- [ ] T121 [P] [US3] Create API client methods for task assignments in src/Frontend/web/src/lib/api/userManagement.ts
- [ ] T122 [P] [US3] Create TaskCard component in src/Frontend/web/src/components/user-management/TaskCard.tsx
- [ ] T123 [P] [US3] Create TaskAssignmentForm component in src/Frontend/web/src/components/user-management/TaskAssignmentForm.tsx
- [ ] T124 [US3] Create Task Assignment page in src/Frontend/web/src/app/(protected)/tasks/page.tsx
- [ ] T125 [US3] Register TaskAssignmentService and command/query handlers in src/Backend/CRM.Api/Program.cs

**Checkpoint**: At this point, User Stories 1, 2, AND 3 should all work independently

---

## Phase 6: User Story 4 - Activity Feed (Priority: P2)

**Goal**: Provide team-wide activity feed showing all team member actions with filtering and pagination

**Independent Test**: View activity feed, filter by user/action type/date range, paginate results, and click activity to view related entity

### Implementation for User Story 4

- [ ] T126 [P] [US4] Create UserActivityDto in src/Backend/CRM.Application/UserManagement/DTOs/UserActivityDto.cs
- [ ] T127 [P] [US4] Create PagedActivityFeedResult in src/Backend/CRM.Application/UserManagement/DTOs/PagedActivityFeedResult.cs
- [ ] T128 [US4] Create IActivityService interface in src/Backend/CRM.Application/UserManagement/Services/IActivityService.cs
- [ ] T129 [US4] Implement ActivityService with business logic in src/Backend/CRM.Application/UserManagement/Services/ActivityService.cs
- [ ] T130 [US4] Create GetActivityFeedQuery in src/Backend/CRM.Application/UserManagement/Queries/GetActivityFeedQuery.cs
- [ ] T131 [US4] Create GetActivityFeedQueryHandler in src/Backend/CRM.Application/UserManagement/Queries/Handlers/GetActivityFeedQueryHandler.cs
- [ ] T132 [US4] Create GetUserActivityQuery in src/Backend/CRM.Application/UserManagement/Queries/GetUserActivityQuery.cs
- [ ] T133 [US4] Create GetUserActivityQueryHandler in src/Backend/CRM.Application/UserManagement/Queries/Handlers/GetUserActivityQueryHandler.cs
- [ ] T134 [US4] Create GetActivityFeedQueryValidator in src/Backend/CRM.Application/UserManagement/Validators/GetActivityFeedQueryValidator.cs
- [ ] T135 [US4] Create ActivityFeedController with all endpoints in src/Backend/CRM.Api/Controllers/ActivityFeedController.cs
- [ ] T136 [US4] Add AutoMapper mappings for UserActivity in src/Backend/CRM.Application/Mapping/UserManagementProfile.cs
- [ ] T137 [P] [US4] Create TypeScript types for UserActivity in src/Frontend/web/src/types/userManagement.ts
- [ ] T138 [P] [US4] Create API client methods for activity feed in src/Frontend/web/src/lib/api/userManagement.ts
- [ ] T139 [P] [US4] Create ActivityFeedItem component in src/Frontend/web/src/components/user-management/ActivityFeedItem.tsx
- [ ] T140 [P] [US4] Create ActivityFeedFilter component in src/Frontend/web/src/components/user-management/ActivityFeedFilter.tsx
- [ ] T141 [US4] Create Activity Feed page in src/Frontend/web/src/app/(protected)/activity/page.tsx
- [ ] T142 [US4] Register ActivityService and query handlers in src/Backend/CRM.Api/Program.cs

**Checkpoint**: At this point, User Stories 1-4 should all work independently

---

## Phase 7: User Story 5 - Mentions (Priority: P2)

**Goal**: Enable @mentions in comments/notes with real-time notifications and read/unread tracking

**Independent Test**: Create mention in comment, verify notification is sent, view mentions page, mark as read

### Implementation for User Story 5

- [ ] T143 [P] [US5] Create MentionDto in src/Backend/CRM.Application/UserManagement/DTOs/MentionDto.cs
- [ ] T144 [P] [US5] Create CreateMentionRequest in src/Backend/CRM.Application/UserManagement/Requests/CreateMentionRequest.cs
- [ ] T145 [US5] Create IMentionService interface in src/Backend/CRM.Application/UserManagement/Services/IMentionService.cs
- [ ] T146 [US5] Implement MentionService with business logic in src/Backend/CRM.Application/UserManagement/Services/MentionService.cs
- [ ] T147 [US5] Create CreateMentionCommand in src/Backend/CRM.Application/UserManagement/Commands/CreateMentionCommand.cs
- [ ] T148 [US5] Create CreateMentionCommandHandler in src/Backend/CRM.Application/UserManagement/Commands/Handlers/CreateMentionCommandHandler.cs
- [ ] T149 [US5] Create MarkMentionReadCommand in src/Backend/CRM.Application/UserManagement/Commands/MarkMentionReadCommand.cs
- [ ] T150 [US5] Create MarkMentionReadCommandHandler in src/Backend/CRM.Application/UserManagement/Commands/Handlers/MarkMentionReadCommandHandler.cs
- [ ] T151 [US5] Create GetMentionsQuery in src/Backend/CRM.Application/UserManagement/Queries/GetMentionsQuery.cs
- [ ] T152 [US5] Create GetMentionsQueryHandler in src/Backend/CRM.Application/UserManagement/Queries/Handlers/GetMentionsQueryHandler.cs
- [ ] T153 [US5] Create GetUnreadMentionsCountQuery in src/Backend/CRM.Application/UserManagement/Queries/GetUnreadMentionsCountQuery.cs
- [ ] T154 [US5] Create GetUnreadMentionsCountQueryHandler in src/Backend/CRM.Application/UserManagement/Queries/Handlers/GetUnreadMentionsCountQueryHandler.cs
- [ ] T155 [US5] Create CreateMentionRequestValidator in src/Backend/CRM.Application/UserManagement/Validators/CreateMentionRequestValidator.cs
- [ ] T156 [US5] Create MentionsController with all endpoints in src/Backend/CRM.Api/Controllers/MentionsController.cs
- [ ] T157 [US5] Add AutoMapper mappings for Mention in src/Backend/CRM.Application/Mapping/UserManagementProfile.cs
- [ ] T158 [P] [US5] Create TypeScript types for Mention in src/Frontend/web/src/types/userManagement.ts
- [ ] T159 [P] [US5] Create API client methods for mentions in src/Frontend/web/src/lib/api/userManagement.ts
- [ ] T160 [P] [US5] Create MentionAutocomplete component in src/Frontend/web/src/components/user-management/MentionAutocomplete.tsx
- [ ] T161 [P] [US5] Create MentionBadge component in src/Frontend/web/src/components/user-management/MentionBadge.tsx
- [ ] T162 [US5] Create Mentions page in src/Frontend/web/src/app/(protected)/mentions/page.tsx
- [ ] T163 [US5] Register MentionService and command/query handlers in src/Backend/CRM.Api/Program.cs

**Checkpoint**: At this point, User Stories 1-5 should all work independently

---

## Phase 8: User Story 6 - Enhanced User Profiles (Priority: P3)

**Goal**: Enable users to upload avatars, add bio, social links, skills, and manage out-of-office status

**Independent Test**: Update user profile with avatar, bio, social links, skills, set OOO status, and verify profile displays correctly

### Implementation for User Story 6

- [ ] T164 [P] [US6] Create UserProfileDto in src/Backend/CRM.Application/UserManagement/DTOs/UserProfileDto.cs
- [ ] T165 [P] [US6] Create UpdateUserProfileRequest in src/Backend/CRM.Application/UserManagement/Requests/UpdateUserProfileRequest.cs
- [ ] T166 [P] [US6] Create SetOutOfOfficeRequest in src/Backend/CRM.Application/UserManagement/Requests/SetOutOfOfficeRequest.cs
- [ ] T167 [US6] Create IUserProfileService interface in src/Backend/CRM.Application/UserManagement/Services/IUserProfileService.cs
- [ ] T168 [US6] Implement UserProfileService with business logic in src/Backend/CRM.Application/UserManagement/Services/UserProfileService.cs
- [ ] T169 [US6] Create IFileStorageService interface in src/Backend/CRM.Infrastructure/UserManagement/FileStorage/IFileStorageService.cs
- [ ] T170 [US6] Implement LocalFileStorageService for avatar uploads in src/Backend/CRM.Infrastructure/UserManagement/FileStorage/LocalFileStorageService.cs
- [ ] T171 [US6] Create IHtmlSanitizer interface in src/Backend/CRM.Infrastructure/UserManagement/HtmlSanitization/IHtmlSanitizer.cs
- [ ] T172 [US6] Implement HtmlSanitizerService for bio and OOO message in src/Backend/CRM.Infrastructure/UserManagement/HtmlSanitization/HtmlSanitizerService.cs
- [ ] T173 [US6] Create UpdateUserProfileCommand in src/Backend/CRM.Application/UserManagement/Commands/UpdateUserProfileCommand.cs
- [ ] T174 [US6] Create UpdateUserProfileCommandHandler in src/Backend/CRM.Application/UserManagement/Commands/Handlers/UpdateUserProfileCommandHandler.cs
- [ ] T175 [US6] Create SetOutOfOfficeCommand in src/Backend/CRM.Application/UserManagement/Commands/SetOutOfOfficeCommand.cs
- [ ] T176 [US6] Create SetOutOfOfficeCommandHandler in src/Backend/CRM.Application/UserManagement/Commands/Handlers/SetOutOfOfficeCommandHandler.cs
- [ ] T177 [US6] Create UpdateUserProfileRequestValidator in src/Backend/CRM.Application/UserManagement/Validators/UpdateUserProfileRequestValidator.cs
- [ ] T178 [US6] Create UserProfileController with all endpoints in src/Backend/CRM.Api/Controllers/UserProfileController.cs
- [ ] T179 [US6] Add AutoMapper mappings for UserProfile in src/Backend/CRM.Application/Mapping/UserManagementProfile.cs
- [ ] T180 [P] [US6] Create TypeScript types for UserProfile in src/Frontend/web/src/types/userManagement.ts
- [ ] T181 [P] [US6] Create API client methods for user profile in src/Frontend/web/src/lib/api/userManagement.ts
- [ ] T182 [P] [US6] Create SkillTagInput component in src/Frontend/web/src/components/user-management/SkillTagInput.tsx
- [ ] T183 [P] [US6] Create OutOfOfficeToggle component in src/Frontend/web/src/components/user-management/OutOfOfficeToggle.tsx
- [ ] T184 [US6] Create Enhanced User Profile page in src/Frontend/web/src/app/(protected)/profile/page.tsx
- [ ] T185 [US6] Register UserProfileService, FileStorageService, HtmlSanitizerService and command handlers in src/Backend/CRM.Api/Program.cs

**Checkpoint**: At this point, User Stories 1-6 should all work independently

---

## Phase 9: User Story 7 - Real-Time Presence (Priority: P3)

**Goal**: Provide real-time presence indicators (online/offline/busy/away) via WebSocket

**Independent Test**: Connect to WebSocket, update presence status, verify other users see status updates in real-time

### Implementation for User Story 7

- [ ] T186 [P] [US7] Create UpdatePresenceStatusRequest in src/Backend/CRM.Application/UserManagement/Requests/UpdatePresenceStatusRequest.cs
- [ ] T187 [US7] Create IPresenceService interface in src/Backend/CRM.Application/UserManagement/Services/IPresenceService.cs
- [ ] T188 [US7] Implement PresenceService with business logic in src/Backend/CRM.Application/UserManagement/Services/PresenceService.cs
- [ ] T189 [US7] Create UpdatePresenceStatusCommand in src/Backend/CRM.Application/UserManagement/Commands/UpdatePresenceStatusCommand.cs
- [ ] T190 [US7] Create UpdatePresenceStatusCommandHandler in src/Backend/CRM.Application/UserManagement/Commands/Handlers/UpdatePresenceStatusCommandHandler.cs
- [ ] T191 [US7] Create PresenceHub SignalR hub in src/Backend/CRM.Api/Hubs/PresenceHub.cs
- [ ] T192 [US7] Register SignalR services and map PresenceHub in src/Backend/CRM.Api/Program.cs
- [ ] T193 [P] [US7] Create usePresence hook in src/Frontend/web/src/hooks/usePresence.ts
- [ ] T194 [P] [US7] Create UserAvatarWithPresence component in src/Frontend/web/src/components/user-management/UserAvatarWithPresence.tsx
- [ ] T195 [P] [US7] Create PresenceIndicator component in src/Frontend/web/src/components/user-management/PresenceIndicator.tsx
- [ ] T196 [US7] Integrate presence updates into UserProfileController in src/Backend/CRM.Api/Controllers/UserProfileController.cs
- [ ] T197 [US7] Register PresenceService and command handlers in src/Backend/CRM.Api/Program.cs

**Checkpoint**: At this point, User Stories 1-7 should all work independently

---

## Phase 10: User Story 8 - Bulk User Operations (Priority: P3)

**Goal**: Enable admins to bulk invite users, deactivate users, change roles, and export user data

**Independent Test**: Import users via CSV, bulk deactivate users, bulk change roles, export users to CSV

### Implementation for User Story 8

- [ ] T198 [P] [US8] Create BulkOperationResultDto in src/Backend/CRM.Application/UserManagement/DTOs/BulkOperationResultDto.cs
- [ ] T199 [P] [US8] Create BulkInviteUsersRequest in src/Backend/CRM.Application/UserManagement/Requests/BulkInviteUsersRequest.cs
- [ ] T200 [P] [US8] Create BulkDeactivateUsersRequest in src/Backend/CRM.Application/UserManagement/Requests/BulkDeactivateUsersRequest.cs
- [ ] T201 [P] [US8] Create BulkChangeUserRolesRequest in src/Backend/CRM.Application/UserManagement/Requests/BulkChangeUserRolesRequest.cs
- [ ] T202 [US8] Create IUserBulkOperationService interface in src/Backend/CRM.Application/UserManagement/Services/IUserBulkOperationService.cs
- [ ] T203 [US8] Implement UserBulkOperationService with business logic in src/Backend/CRM.Application/UserManagement/Services/UserBulkOperationService.cs
- [ ] T204 [US8] Create ICsvImportService interface in src/Backend/CRM.Infrastructure/UserManagement/CsvImport/ICsvImportService.cs
- [ ] T205 [US8] Implement CsvImportService with validation in src/Backend/CRM.Infrastructure/UserManagement/CsvImport/CsvImportService.cs
- [ ] T206 [US8] Create BulkInviteUsersCommand in src/Backend/CRM.Application/UserManagement/Commands/BulkInviteUsersCommand.cs
- [ ] T207 [US8] Create BulkInviteUsersCommandHandler in src/Backend/CRM.Application/UserManagement/Commands/Handlers/BulkInviteUsersCommandHandler.cs
- [ ] T208 [US8] Create BulkDeactivateUsersCommand in src/Backend/CRM.Application/UserManagement/Commands/BulkDeactivateUsersCommand.cs
- [ ] T209 [US8] Create BulkDeactivateUsersCommandHandler in src/Backend/CRM.Application/UserManagement/Commands/Handlers/BulkDeactivateUsersCommandHandler.cs
- [ ] T210 [US8] Create BulkChangeUserRolesCommand in src/Backend/CRM.Application/UserManagement/Commands/BulkChangeUserRolesCommand.cs
- [ ] T211 [US8] Create BulkChangeUserRolesCommandHandler in src/Backend/CRM.Application/UserManagement/Commands/Handlers/BulkChangeUserRolesCommandHandler.cs
- [ ] T212 [US8] Create BulkInviteUsersRequestValidator in src/Backend/CRM.Application/UserManagement/Validators/BulkInviteUsersRequestValidator.cs
- [ ] T213 [US8] Create UserBulkOperationsController with all endpoints in src/Backend/CRM.Api/Controllers/UserBulkOperationsController.cs
- [ ] T214 [US8] Add AutoMapper mappings for BulkOperationResult in src/Backend/CRM.Application/Mapping/UserManagementProfile.cs
- [ ] T215 [P] [US8] Create TypeScript types for bulk operations in src/Frontend/web/src/types/userManagement.ts
- [ ] T216 [P] [US8] Create API client methods for bulk operations in src/Frontend/web/src/lib/api/userManagement.ts
- [ ] T217 [P] [US8] Create BulkActionModal component in src/Frontend/web/src/components/user-management/BulkActionModal.tsx
- [ ] T218 [US8] Create Bulk User Operations page in src/Frontend/web/src/app/(protected)/admin/users/bulk/page.tsx
- [ ] T219 [US8] Register UserBulkOperationService, CsvImportService and command handlers in src/Backend/CRM.Api/Program.cs

**Checkpoint**: At this point, User Stories 1-8 should all work independently

---

## Phase 11: User Story 9 - Advanced Permissions & Custom Roles (Priority: P3)

**Goal**: Enable admins to create custom roles with granular permissions

**Independent Test**: Create custom role, assign permissions, assign role to user, verify permissions are enforced

### Implementation for User Story 9

- [ ] T220 [P] [US9] Create CustomRoleDto in src/Backend/CRM.Application/UserManagement/DTOs/CustomRoleDto.cs
- [ ] T221 [P] [US9] Create PermissionDto in src/Backend/CRM.Application/UserManagement/DTOs/PermissionDto.cs
- [ ] T222 [P] [US9] Create CreateCustomRoleRequest in src/Backend/CRM.Application/UserManagement/Requests/CreateCustomRoleRequest.cs
- [ ] T223 [P] [US9] Create UpdateRolePermissionsRequest in src/Backend/CRM.Application/UserManagement/Requests/UpdateRolePermissionsRequest.cs
- [ ] T224 [US9] Create ICustomRoleService interface in src/Backend/CRM.Application/UserManagement/Services/ICustomRoleService.cs
- [ ] T225 [US9] Implement CustomRoleService with business logic in src/Backend/CRM.Application/UserManagement/Services/CustomRoleService.cs
- [ ] T226 [US9] Create CreateCustomRoleCommand in src/Backend/CRM.Application/UserManagement/Commands/CreateCustomRoleCommand.cs
- [ ] T227 [US9] Create CreateCustomRoleCommandHandler in src/Backend/CRM.Application/UserManagement/Commands/Handlers/CreateCustomRoleCommandHandler.cs
- [ ] T228 [US9] Create UpdateRolePermissionsCommand in src/Backend/CRM.Application/UserManagement/Commands/UpdateRolePermissionsCommand.cs
- [ ] T229 [US9] Create UpdateRolePermissionsCommandHandler in src/Backend/CRM.Application/UserManagement/Commands/Handlers/UpdateRolePermissionsCommandHandler.cs
- [ ] T230 [US9] Create GetAvailablePermissionsQuery in src/Backend/CRM.Application/UserManagement/Queries/GetAvailablePermissionsQuery.cs
- [ ] T231 [US9] Create GetAvailablePermissionsQueryHandler in src/Backend/CRM.Application/UserManagement/Queries/Handlers/GetAvailablePermissionsQueryHandler.cs
- [ ] T232 [US9] Create GetCustomRolesQuery in src/Backend/CRM.Application/UserManagement/Queries/GetCustomRolesQuery.cs
- [ ] T233 [US9] Create GetCustomRolesQueryHandler in src/Backend/CRM.Application/UserManagement/Queries/Handlers/GetCustomRolesQueryHandler.cs
- [ ] T234 [US9] Create CreateCustomRoleRequestValidator in src/Backend/CRM.Application/UserManagement/Validators/CreateCustomRoleRequestValidator.cs
- [ ] T235 [US9] Create CustomRolesController with all endpoints in src/Backend/CRM.Api/Controllers/CustomRolesController.cs
- [ ] T236 [US9] Add AutoMapper mappings for CustomRole and Permission in src/Backend/CRM.Application/Mapping/UserManagementProfile.cs
- [ ] T237 [P] [US9] Create TypeScript types for CustomRole and Permission in src/Frontend/web/src/types/userManagement.ts
- [ ] T238 [P] [US9] Create API client methods for custom roles in src/Frontend/web/src/lib/api/userManagement.ts
- [ ] T239 [US9] Create Custom Role Builder page in src/Frontend/web/src/app/(protected)/admin/roles/custom/page.tsx
- [ ] T240 [US9] Register CustomRoleService and command/query handlers in src/Backend/CRM.Api/Program.cs

**Checkpoint**: At this point, all user stories should be independently functional

---

## Phase 12: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T241 [P] Update quickstart.md with setup instructions in specs/019-advanced-user-management-team-collaboration/quickstart.md
- [ ] T242 [P] Create requirements checklist in specs/019-advanced-user-management-team-collaboration/checklists/requirements.md
- [ ] T243 [P] Add error boundaries for user management pages in src/Frontend/web/src/components/user-management/ErrorBoundary.tsx
- [ ] T244 [P] Add loading skeletons for all pages in src/Frontend/web/src/components/user-management/LoadingSkeleton.tsx
- [ ] T245 Code cleanup and refactoring across all user management components
- [ ] T246 Performance optimization for activity feed queries
- [ ] T247 Performance optimization for team hierarchy queries
- [ ] T248 Security hardening: Validate all file uploads (CSV, avatars)
- [ ] T249 Security hardening: Rate limit presence status updates
- [ ] T250 Accessibility: Add ARIA labels to all interactive elements
- [ ] T251 Accessibility: Test keyboard navigation and screen reader compatibility
- [ ] T252 Mobile responsiveness: Test and fix all pages on mobile devices
- [ ] T253 Integration: Verify all endpoints work with existing RBAC system (Spec-003)
- [ ] T254 Integration: Verify mentions trigger notifications (Spec-013)
- [ ] T255 Integration: Verify activity logging integrates with audit system (Spec-018)
- [ ] T256 Run quickstart.md validation and update if needed

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-11)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Polish (Phase 12)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1) - Team Management**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2) - User Groups**: Can start after Foundational (Phase 2) - Independent
- **User Story 3 (P2) - Task Assignment**: Can start after Foundational (Phase 2) - May integrate with US1 but independently testable
- **User Story 4 (P2) - Activity Feed**: Can start after Foundational (Phase 2) - Independent
- **User Story 5 (P2) - Mentions**: Can start after Foundational (Phase 2) - Requires Spec-013 (NotificationSystem) integration
- **User Story 6 (P3) - Enhanced User Profiles**: Can start after Foundational (Phase 2) - Independent
- **User Story 7 (P3) - Real-Time Presence**: Can start after Foundational (Phase 2) - Independent
- **User Story 8 (P3) - Bulk User Operations**: Can start after Foundational (Phase 2) - Independent
- **User Story 9 (P3) - Advanced Permissions**: Can start after Foundational (Phase 2) - Requires Spec-003 (RBAC) integration

### Within Each User Story

- DTOs and Requests before Services
- Services before Commands/Queries
- Commands/Queries before Controllers
- Backend before Frontend
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational phase completes, all user stories can start in parallel (if team capacity allows)
- DTOs and Requests within a story marked [P] can run in parallel
- Frontend TypeScript types and API clients marked [P] can run in parallel
- Frontend components marked [P] can run in parallel
- Different user stories can be worked on in parallel by different team members

---

## Parallel Example: User Story 1

```bash
# Launch all DTOs and Requests for User Story 1 together:
Task: "Create TeamDto in src/Backend/CRM.Application/UserManagement/DTOs/TeamDto.cs"
Task: "Create TeamMemberDto in src/Backend/CRM.Application/UserManagement/DTOs/TeamMemberDto.cs"
Task: "Create CreateTeamRequest in src/Backend/CRM.Application/UserManagement/Requests/CreateTeamRequest.cs"
Task: "Create UpdateTeamRequest in src/Backend/CRM.Application/UserManagement/Requests/UpdateTeamRequest.cs"
Task: "Create AddTeamMemberRequest in src/Backend/CRM.Application/UserManagement/Requests/AddTeamMemberRequest.cs"

# Launch all Frontend components for User Story 1 together:
Task: "Create TypeScript types for Team and TeamMember in src/Frontend/web/src/types/userManagement.ts"
Task: "Create API client methods for teams in src/Frontend/web/src/lib/api/userManagement.ts"
Task: "Create TeamHierarchyTree component in src/Frontend/web/src/components/user-management/TeamHierarchyTree.tsx"
Task: "Create TeamMemberList component in src/Frontend/web/src/components/user-management/TeamMemberList.tsx"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1 (Team Management)
4. **STOP and VALIDATE**: Test User Story 1 independently
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 (Team Management) → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 (User Groups) → Test independently → Deploy/Demo
4. Add User Story 3 (Task Assignment) → Test independently → Deploy/Demo
5. Add User Story 4 (Activity Feed) → Test independently → Deploy/Demo
6. Add User Story 5 (Mentions) → Test independently → Deploy/Demo
7. Add User Stories 6-9 → Test independently → Deploy/Demo
8. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (Team Management)
   - Developer B: User Story 2 (User Groups)
   - Developer C: User Story 3 (Task Assignment)
   - Developer D: User Story 4 (Activity Feed)
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify all endpoints enforce RBAC properly
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
- All file uploads must be validated for security
- All user-provided HTML content must be sanitized
- Activity logging must be accurate for all sensitive actions
- Real-time features require WebSocket connection management

---

## Summary

**Total Task Count**: 256 tasks

**Task Count per User Story**:
- Phase 1 (Setup): 6 tasks
- Phase 2 (Foundational): 35 tasks
- Phase 3 (US1 - Team Management): 36 tasks
- Phase 4 (US2 - User Groups): 26 tasks
- Phase 5 (US3 - Task Assignment): 22 tasks
- Phase 6 (US4 - Activity Feed): 17 tasks
- Phase 7 (US5 - Mentions): 21 tasks
- Phase 8 (US6 - Enhanced User Profiles): 22 tasks
- Phase 9 (US7 - Real-Time Presence): 12 tasks
- Phase 10 (US8 - Bulk User Operations): 22 tasks
- Phase 11 (US9 - Advanced Permissions): 21 tasks
- Phase 12 (Polish): 16 tasks

**Parallel Opportunities Identified**: 
- 41 tasks marked [P] can run in parallel
- All user stories can be worked on in parallel after Foundational phase
- Frontend and backend work can proceed in parallel within each story

**Independent Test Criteria for Each Story**:
- US1: Create team, add members, view hierarchy independently
- US2: Create group, assign permissions, add users independently
- US3: Assign task, update status, view tasks independently
- US4: View activity feed, filter, paginate independently
- US5: Create mention, receive notification, mark read independently
- US6: Update profile, upload avatar, set OOO independently
- US7: Connect WebSocket, update presence, see real-time updates independently
- US8: Import CSV, bulk operations, export independently
- US9: Create custom role, assign permissions, verify enforcement independently

**Suggested MVP Scope**: User Story 1 (Team Management) - 36 tasks + Setup (6) + Foundational (35) = 77 tasks for MVP

**Format Validation**: ✅ All tasks follow the checklist format (checkbox, ID, labels, file paths)

