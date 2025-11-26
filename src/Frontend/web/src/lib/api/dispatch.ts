import { apiClient } from './client';
import { 
  DispatchHistoryResponse, 
  DispatchStatistics, 
  GetDispatchHistoryParams,
  GetDispatchStatisticsParams
} from '@/lib/types/dispatch.types';

export const dispatchApi = {
  /**
   * Get dispatch history with optional filtering
   */
  async getDispatchHistory(params: GetDispatchHistoryParams): Promise<DispatchHistoryResponse> {
    const searchParams = new URLSearchParams();
    
    if (params.notificationId) searchParams.set('notificationId', params.notificationId.toString());
    if (params.userId) searchParams.set('userId', params.userId.toString());
    if (params.channel) searchParams.set('channel', params.channel);
    if (params.status) searchParams.set('status', params.status);
    if (params.fromDate) searchParams.set('fromDate', params.fromDate);
    if (params.toDate) searchParams.set('toDate', params.toDate);
    if (params.page) searchParams.set('page', params.page.toString());
    if (params.pageSize) searchParams.set('pageSize', params.pageSize.toString());

    const response = await apiClient.get(`/notification-dispatch/history?${searchParams.toString()}`);
    return response.data;
  },

  /**
   * Get dispatch statistics for a specific time period
   */
  async getDispatchStatistics(params: GetDispatchStatisticsParams): Promise<DispatchStatistics> {
    const searchParams = new URLSearchParams();
    
    if (params.fromDate) searchParams.set('fromDate', params.fromDate);
    if (params.toDate) searchParams.set('toDate', params.toDate);
    if (params.channel) searchParams.set('channel', params.channel);

    const response = await apiClient.get(`/notification-dispatch/statistics?${searchParams.toString()}`);
    return response.data;
  },

  /**
   * Get dispatch history for a specific notification
   */
  async getNotificationDispatchHistory(
    notificationId: number,
    params?: { page?: number; pageSize?: number }
  ): Promise<DispatchHistoryResponse> {
    const searchParams = new URLSearchParams();
    if (params?.page) searchParams.set('page', params.page.toString());
    if (params?.pageSize) searchParams.set('pageSize', params.pageSize.toString());

    const response = await apiClient.get(
      `/notification-dispatch/notification/${notificationId}?${searchParams.toString()}`
    );
    return response.data;
  },

  /**
   * Get dispatch history for a specific user
   */
  async getUserDispatchHistory(
    userId: number,
    params: Partial<GetDispatchHistoryParams>
  ): Promise<DispatchHistoryResponse> {
    const searchParams = new URLSearchParams();
    
    if (params.channel) searchParams.set('channel', params.channel);
    if (params.status) searchParams.set('status', params.status);
    if (params.fromDate) searchParams.set('fromDate', params.fromDate);
    if (params.toDate) searchParams.set('toDate', params.toDate);
    if (params.page) searchParams.set('page', params.page.toString());
    if (params.pageSize) searchParams.set('pageSize', params.pageSize.toString());

    const response = await apiClient.get(
      `/notification-dispatch/user/${userId}?${searchParams.toString()}`
    );
    return response.data;
  },

  /**
   * Get failed dispatches that may need attention
   */
  async getFailedDispatches(params?: {
    channel?: string;
    fromDate?: string;
    toDate?: string;
    page?: number;
    pageSize?: number;
  }): Promise<DispatchHistoryResponse> {
    const searchParams = new URLSearchParams();
    
    if (params?.channel) searchParams.set('channel', params.channel);
    if (params?.fromDate) searchParams.set('fromDate', params.fromDate);
    if (params?.toDate) searchParams.set('toDate', params.toDate);
    if (params?.page) searchParams.set('page', params.page.toString());
    if (params?.pageSize) searchParams.set('pageSize', params.pageSize.toString());

    const response = await apiClient.get(`/notification-dispatch/failed?${searchParams.toString()}`);
    return response.data;
  },

  /**
   * Get permanently failed dispatches
   */
  async getPermanentlyFailedDispatches(params?: {
    channel?: string;
    fromDate?: string;
    toDate?: string;
    page?: number;
    pageSize?: number;
  }): Promise<DispatchHistoryResponse> {
    const searchParams = new URLSearchParams();
    
    if (params?.channel) searchParams.set('channel', params.channel);
    if (params?.fromDate) searchParams.set('fromDate', params.fromDate);
    if (params?.toDate) searchParams.set('toDate', params.toDate);
    if (params?.page) searchParams.set('page', params.page.toString());
    if (params?.pageSize) searchParams.set('pageSize', params.pageSize.toString());

    const response = await apiClient.get(`/notification-dispatch/permanently-failed?${searchParams.toString()}`);
    return response.data;
  },

  /**
   * Retry a failed dispatch attempt
   */
  async retryFailedDispatch(dispatchAttemptId: number): Promise<void> {
    await apiClient.post(`/notification-dispatch/retry/${dispatchAttemptId}`);
  },

  /**
   * Cancel pending dispatches for a notification
   */
  async cancelPendingDispatches(notificationId: number): Promise<void> {
    await apiClient.post(`/notification-dispatch/cancel/${notificationId}`);
  }
};