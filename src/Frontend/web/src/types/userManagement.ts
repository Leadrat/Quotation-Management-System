// TypeScript types for User Management matching backend DTOs

// Team Management
export interface Team {
  teamId: string;
  name: string;
  description?: string;
  teamLeadUserId: string;
  teamLeadName: string;
  parentTeamId?: string;
  parentTeamName?: string;
  companyId: string;
  isActive: boolean;
  memberCount: number;
  childTeams?: Team[];
  createdAt: string;
  updatedAt: string;
}

export interface TeamMember {
  teamMemberId: string;
  teamId: string;
  teamName: string;
  userId: string;
  userName: string;
  userEmail: string;
  role: string;
  joinedAt: string;
}

export interface CreateTeamRequest {
  name: string;
  description?: string;
  teamLeadUserId: string;
  parentTeamId?: string;
  companyId: string;
}

export interface UpdateTeamRequest {
  name?: string;
  description?: string;
  teamLeadUserId?: string;
  parentTeamId?: string;
  isActive?: boolean;
}

export interface AddTeamMemberRequest {
  userId: string;
  role: string;
}

// User Groups
export interface UserGroup {
  groupId: string;
  name: string;
  description?: string;
  permissions: string[];
  createdByUserId: string;
  createdByUserName: string;
  memberCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface UserGroupMember {
  groupMemberId: string;
  groupId: string;
  groupName: string;
  userId: string;
  userName: string;
  userEmail: string;
  addedAt: string;
}

export interface CreateUserGroupRequest {
  name: string;
  description?: string;
  permissions: string[];
}

export interface UpdateUserGroupRequest {
  name?: string;
  description?: string;
  permissions?: string[];
}

// Task Assignment
export interface TaskAssignment {
  assignmentId: string;
  entityType: string;
  entityId: string;
  assignedToUserId: string;
  assignedToUserName: string;
  assignedByUserId: string;
  assignedByUserName: string;
  dueDate?: string;
  status: string;
  isOverdue: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface AssignTaskRequest {
  entityType: string;
  entityId: string;
  assignedToUserId: string;
  dueDate?: string;
}

export interface UpdateTaskStatusRequest {
  status: string;
}

// Activity Feed
export interface UserActivity {
  activityId: string;
  userId: string;
  userName: string;
  actionType: string;
  entityType?: string;
  entityId?: string;
  ipAddress: string;
  userAgent: string;
  timestamp: string;
}

export interface PagedActivityFeedResult {
  success: boolean;
  data: UserActivity[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
}

// Mentions
export interface Mention {
  mentionId: string;
  entityType: string;
  entityId: string;
  mentionedUserId: string;
  mentionedUserName: string;
  mentionedByUserId: string;
  mentionedByUserName: string;
  isRead: boolean;
  createdAt: string;
}

export interface CreateMentionRequest {
  entityType: string;
  entityId: string;
  mentionedUserId: string;
}

export interface PagedMentionsResult {
  success: boolean;
  data: Mention[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
}

// Enhanced User Profiles
export interface EnhancedUserProfile {
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  avatarUrl?: string;
  bio?: string;
  linkedInUrl?: string;
  twitterUrl?: string;
  skills: string[];
  outOfOfficeStatus: boolean;
  outOfOfficeMessage?: string;
  delegateUserId?: string;
  delegateUserName?: string;
  lastSeenAt?: string;
  presenceStatus: string;
  createdAt: string;
  updatedAt: string;
}

export interface UpdateUserProfileRequest {
  avatarUrl?: string;
  bio?: string;
  linkedInUrl?: string;
  twitterUrl?: string;
  skills?: string[];
}

export interface SetOutOfOfficeRequest {
  isOutOfOffice: boolean;
  message?: string;
  delegateUserId?: string;
}

// Bulk Operations
export interface BulkOperationResult {
  totalCount: number;
  successCount: number;
  failureCount: number;
  results: BulkOperationItemResult[];
}

export interface BulkOperationItemResult {
  userId: string;
  userEmail: string;
  success: boolean;
  errorMessage?: string;
}

export interface BulkInviteUserItem {
  email: string;
  firstName: string;
  lastName: string;
  mobile?: string;
}

export interface BulkInviteUsersRequest {
  users: BulkInviteUserItem[];
  roleId?: string;
  teamId?: string;
  sendEmailInvites?: boolean;
}

export interface BulkUpdateUsersRequest {
  userIds: string[];
  isActive?: boolean;
  roleId?: string;
  teamId?: string;
}

// Custom Roles & Permissions
export interface CustomRole {
  roleId: string;
  roleName: string;
  description?: string;
  permissions: string[];
  isBuiltIn: boolean;
  isActive: boolean;
  userCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface Permission {
  key: string;
  name: string;
  category: string;
  description?: string;
}

export interface CreateCustomRoleRequest {
  roleName: string;
  description?: string;
  permissions: string[];
}

export interface UpdateRolePermissionsRequest {
  permissions: string[];
}

// Paged Results
export interface PagedResult<T> {
  success: boolean;
  data: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
}

// Presence Status
export type PresenceStatus = "Offline" | "Online" | "Busy" | "Away";

