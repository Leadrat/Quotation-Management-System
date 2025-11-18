// TypeScript types for Notifications matching backend DTOs

export interface Notification {
  notificationId: string;
  recipientUserId: string;
  relatedEntityType: string;
  relatedEntityId: string;
  eventType: string;
  message: string;
  isRead: boolean;
  isArchived: boolean;
  deliveredChannels?: string;
  deliveryStatus: string;
  createdAt: string;
  readAt?: string;
  archivedAt?: string;
  meta?: string;
  // Computed properties
  isUnread: boolean;
  isDelivered: boolean;
  formattedDate: string;
  entityLinkUrl: string;
}

export interface PagedNotificationsResult {
  success: boolean;
  data: Notification[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
}

export interface UnreadCount {
  count: number;
}

export interface NotificationPreferences {
  userId: string;
  preferences: Record<string, Record<string, boolean>>;
}

export interface MarkNotificationsReadRequest {
  notificationIds?: string[]; // null or empty = mark all
}

export interface ArchiveNotificationsRequest {
  notificationIds?: string[]; // null or empty = archive all
}

export interface UnarchiveNotificationsRequest {
  notificationIds: string[]; // Required - must specify IDs
}

export interface UpdateNotificationPreferencesRequest {
  preferences: Record<string, Record<string, boolean>>;
}

export interface EmailNotificationLog {
  logId: string;
  notificationId?: string;
  recipientEmail: string;
  eventType: string;
  subject: string;
  sentAt: string;
  deliveredAt?: string;
  status: string;
  errorMsg?: string;
  retryCount: number;
  lastRetryAt?: string;
  // Computed properties
  formattedSentAt: string;
  formattedDeliveredAt?: string;
}

export interface PagedEmailLogsResult {
  success: boolean;
  data: EmailNotificationLog[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
}

