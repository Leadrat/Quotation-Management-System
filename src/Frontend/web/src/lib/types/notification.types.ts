export interface Notification {
  notificationId: string;
  title: string;
  message: string;
  relatedEntityId?: string;
  relatedEntityType?: string;
  isRead: boolean;
  readAt?: string;
  sentVia: string;
  createdAt: string;
  notificationType: NotificationType;
  dispatchStatus?: NotificationDispatchStatus[];
}

export interface NotificationDispatchStatus {
  id: number;
  channel: string;
  status: string;
  attemptedAt: string;
  deliveredAt?: string;
  nextRetryAt?: string;
  attemptNumber: number;
  externalId?: string;
  errorMessage?: string;
}

export interface NotificationType {
  notificationTypeId: string;
  typeName: string;
  description?: string;
}

export interface CreateNotificationRequest {
  userId: string;
  notificationTypeId: string;
  title: string;
  message: string;
  relatedEntityId?: string;
  relatedEntityType?: string;
  sentVia: string;
}

export interface NotificationFilters {
  pageNumber?: number;
  pageSize?: number;
  isRead?: boolean;
  notificationTypeId?: string;
  fromDate?: string;
  toDate?: string;
  targetUserId?: string; // Admin only
}

export interface NotificationListResponse {
  success: boolean;
  data: Notification[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  hasMore: boolean;
}

export interface UnreadCountResponse {
  success: boolean;
  data: {
    unreadCount: number;
  };
}

export interface NotificationResponse {
  success: boolean;
  data: Notification;
  message?: string;
}