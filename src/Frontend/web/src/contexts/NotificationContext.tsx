"use client";
import React, { createContext, useContext, useEffect, useState, useCallback } from "react";
import { NotificationsApi } from "@/lib/api";
import { Notification } from "@/types/notifications";
import { getAccessToken } from "@/lib/session";

interface NotificationContextType {
  unreadCount: number;
  notifications: Notification[];
  loading: boolean;
  refreshNotifications: () => Promise<void>;
  refreshUnreadCount: () => Promise<void>;
  markAsRead: (notificationId: string) => Promise<void>;
  addNotification: (notification: Notification) => void;
}

const NotificationContext = createContext<NotificationContextType | undefined>(undefined);

export function NotificationProvider({ children }: { children: React.ReactNode }) {
  const [unreadCount, setUnreadCount] = useState(0);
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [loading, setLoading] = useState(true);

  const refreshUnreadCount = useCallback(async () => {
    // Only fetch if user is authenticated
    if (!getAccessToken()) {
      setUnreadCount(0);
      return;
    }
    try {
      const result = await NotificationsApi.getUnreadCount();
      setUnreadCount(result.data?.count || 0);
    } catch (err: any) {
      // Silently ignore 401 errors (user not logged in)
      if (err?.message?.includes("401")) {
        setUnreadCount(0);
        return;
      }
      console.error("Failed to refresh unread count", err);
    }
  }, []);

  const refreshNotifications = useCallback(async () => {
    // Only fetch if user is authenticated
    if (!getAccessToken()) {
      setNotifications([]);
      setLoading(false);
      return;
    }
    try {
      setLoading(true);
      const result = await NotificationsApi.get({ pageNumber: 1, pageSize: 10, unread: true });
      setNotifications(result.data || []);
    } catch (err: any) {
      // Silently ignore 401 errors (user not logged in)
      if (err?.message?.includes("401")) {
        setNotifications([]);
        return;
      }
      console.error("Failed to refresh notifications", err);
    } finally {
      setLoading(false);
    }
  }, []);

  const markAsRead = useCallback(async (notificationId: string) => {
    if (!getAccessToken()) return;
    try {
      await NotificationsApi.markRead({ notificationIds: [notificationId] });
      await refreshUnreadCount();
      await refreshNotifications();
    } catch (err: any) {
      // Silently ignore 401 errors
      if (!err?.message?.includes("401")) {
        console.error("Failed to mark as read", err);
      }
    }
  }, [refreshUnreadCount, refreshNotifications]);

  const addNotification = useCallback((notification: Notification) => {
    setNotifications((prev) => [notification, ...prev].slice(0, 10));
    if (!notification.isRead) {
      setUnreadCount((prev) => prev + 1);
    }
  }, []);

  useEffect(() => {
    // Only start polling if user is authenticated
    if (!getAccessToken()) {
      setLoading(false);
      return;
    }

    refreshUnreadCount();
    refreshNotifications();

    // Poll for new notifications every 30 seconds (only if authenticated)
    const interval = setInterval(() => {
      if (getAccessToken()) {
        refreshUnreadCount();
        refreshNotifications();
      }
    }, 30000);

    return () => clearInterval(interval);
  }, [refreshUnreadCount, refreshNotifications]);

  return (
    <NotificationContext.Provider
      value={{
        unreadCount,
        notifications,
        loading,
        refreshNotifications,
        refreshUnreadCount,
        markAsRead,
        addNotification,
      }}
    >
      {children}
    </NotificationContext.Provider>
  );
}

export function useNotifications() {
  const context = useContext(NotificationContext);
  if (context === undefined) {
    throw new Error("useNotifications must be used within a NotificationProvider");
  }
  return context;
}

