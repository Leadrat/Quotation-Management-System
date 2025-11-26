import { create } from 'zustand';
import { devtools } from 'zustand/middleware';
import type { Notification, NotificationFilters } from '../lib/types/notification.types';
import { notificationApi } from '../lib/api/notifications';

interface NotificationState {
  // State
  notifications: Notification[];
  unreadCount: number;
  totalCount: number;
  currentPage: number;
  pageSize: number;
  hasMore: boolean;
  isLoading: boolean;
  error: string | null;
  filters: NotificationFilters;

  // Actions
  loadNotifications: (filters?: NotificationFilters) => Promise<void>;
  loadMoreNotifications: () => Promise<void>;
  markAsRead: (notificationId: string) => Promise<void>;
  refreshUnreadCount: () => Promise<void>;
  setFilters: (filters: Partial<NotificationFilters>) => void;
  clearFilters: () => void;
  addNotification: (notification: Notification) => void; // For real-time updates
  updateNotification: (notificationId: string, updates: Partial<Notification>) => void;
  setUnreadCount: (count: number) => void;
  reset: () => void;
}

const initialFilters: NotificationFilters = {
  pageNumber: 1,
  pageSize: 20,
};

export const useNotificationStore = create<NotificationState>()(
  devtools(
    (set, get) => ({
      // Initial state
      notifications: [],
      unreadCount: 0,
      totalCount: 0,
      currentPage: 1,
      pageSize: 20,
      hasMore: false,
      isLoading: false,
      error: null,
      filters: initialFilters,

      // Load notifications with filters
      loadNotifications: async (filters) => {
        set({ isLoading: true, error: null });
        
        try {
          const newFilters = filters || get().filters;
          const response = await notificationApi.getUserNotifications(newFilters);
          
          set({
            notifications: response.data,
            totalCount: response.totalCount,
            currentPage: response.pageNumber,
            pageSize: response.pageSize,
            hasMore: response.hasMore,
            filters: newFilters,
            isLoading: false,
          });
        } catch (error) {
          set({
            error: error instanceof Error ? error.message : 'Failed to load notifications',
            isLoading: false,
          });
        }
      },

      // Load more notifications (pagination)
      loadMoreNotifications: async () => {
        const { filters, currentPage, isLoading, hasMore } = get();
        
        if (isLoading || !hasMore) return;
        
        set({ isLoading: true });
        
        try {
          const nextPageFilters = {
            ...filters,
            pageNumber: currentPage + 1,
          };
          
          const response = await notificationApi.getUserNotifications(nextPageFilters);
          
          set((state) => ({
            notifications: [...state.notifications, ...response.data],
            currentPage: response.pageNumber,
            hasMore: response.hasMore,
            isLoading: false,
          }));
        } catch (error) {
          set({
            error: error instanceof Error ? error.message : 'Failed to load more notifications',
            isLoading: false,
          });
        }
      },

      // Mark notification as read
      markAsRead: async (notificationId) => {
        try {
          await notificationApi.markAsRead(notificationId);
          
          set((state) => ({
            notifications: state.notifications.map((notification) =>
              notification.notificationId === notificationId
                ? { ...notification, isRead: true, readAt: new Date().toISOString() }
                : notification
            ),
            unreadCount: Math.max(0, state.unreadCount - 1),
          }));
        } catch (error) {
          set({
            error: error instanceof Error ? error.message : 'Failed to mark notification as read',
          });
        }
      },

      // Refresh unread count
      refreshUnreadCount: async () => {
        try {
          const response = await notificationApi.getUnreadCount();
          set({ unreadCount: response.data.unreadCount });
        } catch (error) {
          console.error('Failed to refresh unread count:', error);
        }
      },

      // Set filters
      setFilters: (newFilters) => {
        set((state) => ({
          filters: { ...state.filters, ...newFilters, pageNumber: 1 },
        }));
      },

      // Clear filters
      clearFilters: () => {
        set({ filters: initialFilters });
      },

      // Add notification (for real-time updates)
      addNotification: (notification) => {
        set((state) => ({
          notifications: [notification, ...state.notifications],
          totalCount: state.totalCount + 1,
          unreadCount: notification.isRead ? state.unreadCount : state.unreadCount + 1,
        }));
      },

      // Update notification
      updateNotification: (notificationId, updates) => {
        set((state) => ({
          notifications: state.notifications.map((notification) =>
            notification.notificationId === notificationId
              ? { ...notification, ...updates }
              : notification
          ),
        }));
      },

      // Set unread count (for real-time updates)
      setUnreadCount: (count) => {
        set({ unreadCount: count });
      },

      // Reset store
      reset: () => {
        set({
          notifications: [],
          unreadCount: 0,
          totalCount: 0,
          currentPage: 1,
          pageSize: 20,
          hasMore: false,
          isLoading: false,
          error: null,
          filters: initialFilters,
        });
      },
    }),
    {
      name: 'notification-store',
    }
  )
);