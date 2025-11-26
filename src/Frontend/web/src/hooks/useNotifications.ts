'use client';

import { useCallback } from 'react';
import { useNotificationStore } from '../store/notificationStore';
import type { NotificationFilters } from '../lib/types/notification.types';

export function useNotifications() {
  const store = useNotificationStore();

  const loadNotifications = useCallback((filters?: NotificationFilters) => {
    return store.loadNotifications(filters);
  }, [store]);

  const markAsRead = useCallback((notificationId: string) => {
    return store.markAsRead(notificationId);
  }, [store]);

  const markAllAsRead = useCallback(async () => {
    const unreadNotifications = store.notifications.filter(n => !n.isRead);
    
    // Mark all unread notifications as read
    await Promise.all(
      unreadNotifications.map(notification => 
        store.markAsRead(notification.notificationId)
      )
    );
  }, [store]);

  const refreshUnreadCount = useCallback(() => {
    return store.refreshUnreadCount();
  }, [store]);

  const setFilters = useCallback((filters: Partial<NotificationFilters>) => {
    store.setFilters(filters);
    return store.loadNotifications();
  }, [store]);

  return {
    // State
    notifications: store.notifications,
    unreadCount: store.unreadCount,
    totalCount: store.totalCount,
    isLoading: store.isLoading,
    error: store.error,
    hasMore: store.hasMore,
    filters: store.filters,

    // Actions
    loadNotifications,
    loadMore: store.loadMoreNotifications,
    markAsRead,
    markAllAsRead,
    refreshUnreadCount,
    setFilters,
    clearFilters: store.clearFilters,
    reset: store.reset,
  };
}