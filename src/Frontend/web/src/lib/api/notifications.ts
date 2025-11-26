import { apiFetch } from '../api';
import type {
  NotificationType,
  CreateNotificationRequest,
  NotificationFilters,
  NotificationListResponse,
  UnreadCountResponse,
  NotificationResponse,
} from '../types/notification.types';

export const notificationApi = {
  // Get user notifications with filtering and pagination
  async getUserNotifications(filters: NotificationFilters = {}): Promise<NotificationListResponse> {
    const params = new URLSearchParams();
    
    if (filters.pageNumber) params.append('pageNumber', filters.pageNumber.toString());
    if (filters.pageSize) params.append('pageSize', filters.pageSize.toString());
    if (filters.isRead !== undefined) params.append('isRead', filters.isRead.toString());
    if (filters.notificationTypeId) params.append('notificationTypeId', filters.notificationTypeId);
    if (filters.fromDate) params.append('fromDate', filters.fromDate);
    if (filters.toDate) params.append('toDate', filters.toDate);
    if (filters.targetUserId) params.append('targetUserId', filters.targetUserId);

    const queryString = params.toString();
    const url = queryString ? `/api/v1/notifications?${queryString}` : '/api/v1/notifications';
    
    return apiFetch<NotificationListResponse>(url);
  },

  // Get unread notification count
  async getUnreadCount(): Promise<UnreadCountResponse> {
    return apiFetch<UnreadCountResponse>('/api/v1/notifications/unread-count');
  },

  // Mark notification as read
  async markAsRead(notificationId: string): Promise<NotificationResponse> {
    return apiFetch<NotificationResponse>(`/api/v1/notifications/${notificationId}/read`, { method: 'PUT' });
  },

  // Create notification (Admin/System use)
  async createNotification(request: CreateNotificationRequest): Promise<NotificationResponse> {
    return apiFetch<NotificationResponse>('/api/v1/notifications', { 
      method: 'POST', 
      body: JSON.stringify(request) 
    });
  },

  // Get notification types (for filtering)
  async getNotificationTypes(): Promise<{ success: boolean; data: NotificationType[] }> {
    return apiFetch<{ success: boolean; data: NotificationType[] }>('/api/v1/notification-types');
  },

  // Get user notification preferences
  async getNotificationPreferences(): Promise<{ success: boolean; data: any }> {
    return apiFetch<{ success: boolean; data: any }>('/api/v1/notifications/preferences');
  },

  // Update user notification preferences
  async updateNotificationPreferences(preferences: any): Promise<{ success: boolean; data: any }> {
    return apiFetch<{ success: boolean; data: any }>('/api/v1/notifications/preferences', {
      method: 'PUT',
      body: JSON.stringify(preferences)
    });
  },
};