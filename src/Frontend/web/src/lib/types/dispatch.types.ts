export interface DispatchAttempt {
  id: number;
  notificationId: number;
  userId: number;
  channel: NotificationChannel;
  status: DispatchStatus;
  attemptedAt: string;
  deliveredAt?: string;
  nextRetryAt?: string;
  attemptNumber: number;
  externalId?: string;
  errorMessage?: string;
}

export interface DispatchHistoryItem {
  id: number;
  notificationId: number;
  notificationTitle: string;
  userId: number;
  userEmail: string;
  channel: NotificationChannel;
  status: DispatchStatus;
  attemptedAt: string;
  deliveredAt?: string;
  nextRetryAt?: string;
  attemptNumber: number;
  externalId?: string;
  errorMessage?: string;
  priority: NotificationPriority;
}

export interface DispatchStatistics {
  totalAttempts: number;
  successfulDeliveries: number;
  failedAttempts: number;
  pendingAttempts: number;
  permanentFailures: number;
  successRate: number;
  attemptsByChannel: Record<string, number>;
  attemptsByStatus: Record<string, number>;
  averageDeliveryTime: number;
}

export interface DispatchHistoryResponse {
  items: DispatchHistoryItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  statistics: DispatchStatistics;
}

export interface GetDispatchHistoryParams {
  notificationId?: number;
  userId?: number;
  channel?: NotificationChannel;
  status?: DispatchStatus;
  fromDate?: string;
  toDate?: string;
  page?: number;
  pageSize?: number;
}

export interface GetDispatchStatisticsParams {
  fromDate?: string;
  toDate?: string;
  channel?: NotificationChannel;
}

export enum NotificationChannel {
  IN_APP = 'inapp',
  EMAIL = 'email',
  SMS = 'sms'
}

export enum DispatchStatus {
  PENDING = 'pending',
  DELIVERED = 'delivered',
  FAILED = 'failed',
  PERMANENTLY_FAILED = 'permanently_failed',
  CANCELLED = 'cancelled'
}

export enum NotificationPriority {
  LOW = 'low',
  NORMAL = 'normal',
  HIGH = 'high',
  CRITICAL = 'critical'
}