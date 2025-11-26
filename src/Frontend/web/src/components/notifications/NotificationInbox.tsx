"use client";
import { useState, useEffect } from "react";
import { NotificationsApi } from "@/lib/api";
import { Notification } from "@/types/notifications";
import { NotificationItem } from "./NotificationItem";
import Button from "@/components/tailadmin/ui/button/Button";

type TabType = "all" | "unread" | "archived";

interface NotificationInboxProps {
  onUnreadCountChange?: (count: number) => void;
}

export function NotificationInbox({ onUnreadCountChange }: NotificationInboxProps) {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(20);
  const [total, setTotal] = useState(0);
  const [activeTab, setActiveTab] = useState<TabType>("all");
  const [selectedNotifications, setSelectedNotifications] = useState<Set<string>>(new Set());

  useEffect(() => {
    loadData();
    loadUnreadCount();
  }, [pageNumber, activeTab]);

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);

      const params: any = {
        pageNumber,
        pageSize,
        unread: activeTab === "unread" ? true : undefined,
        archived: activeTab === "archived" ? true : false,
      };

      const result = await NotificationsApi.get(params);
      setNotifications(result.data || []);
      setTotal(result.totalCount || 0);
    } catch (err: any) {
      setError(err.message || "Failed to load notifications");
    } finally {
      setLoading(false);
    }
  };

  const loadUnreadCount = async () => {
    try {
      const result = await NotificationsApi.getUnreadCount();
      const count = result.data?.count || 0;
      onUnreadCountChange?.(count);
    } catch (err) {
      console.error("Failed to load unread count", err);
    }
  };

  const handleMarkRead = async (notificationId: string) => {
    try {
      await NotificationsApi.markRead({ notificationIds: [notificationId] });
      await loadData();
      await loadUnreadCount();
    } catch (err: any) {
      alert(err.message || "Failed to mark as read");
    }
  };

  const handleArchive = async (notificationId: string) => {
    try {
      await NotificationsApi.archive({ notificationIds: [notificationId] });
      await loadData();
      await loadUnreadCount();
    } catch (err: any) {
      alert(err.message || "Failed to archive");
    }
  };

  const handleUnarchive = async (notificationId: string) => {
    try {
      await NotificationsApi.unarchive({ notificationIds: [notificationId] });
      await loadData();
      await loadUnreadCount();
    } catch (err: any) {
      alert(err.message || "Failed to unarchive");
    }
  };

  const handleMarkAllRead = async () => {
    try {
      await NotificationsApi.markRead({ notificationIds: undefined });
      await loadData();
      await loadUnreadCount();
    } catch (err: any) {
      alert(err.message || "Failed to mark all as read");
    }
  };

  const handleArchiveSelected = async () => {
    if (selectedNotifications.size === 0) return;
    try {
      await NotificationsApi.archive({
        notificationIds: Array.from(selectedNotifications),
      });
      setSelectedNotifications(new Set());
      await loadData();
      await loadUnreadCount();
    } catch (err: any) {
      alert(err.message || "Failed to archive selected");
    }
  };

  const toggleSelection = (notificationId: string) => {
    const newSelection = new Set(selectedNotifications);
    if (newSelection.has(notificationId)) {
      newSelection.delete(notificationId);
    } else {
      newSelection.add(notificationId);
    }
    setSelectedNotifications(newSelection);
  };

  return (
    <div className="rounded-sm border border-gray-200 bg-white px-5 pb-2.5 pt-6 shadow-default dark:border-gray-800 dark:bg-gray-900 sm:px-7.5 xl:pb-1">
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h4 className="text-title-md2 font-bold text-black dark:text-white">Notifications</h4>
          <p className="mt-1 text-sm text-gray-600 dark:text-gray-400">
            Manage your notifications and preferences
          </p>
        </div>
        <div className="flex gap-2">
          {activeTab === "all" && (
            <Button size="sm" variant="outline" onClick={handleMarkAllRead}>
              Mark All Read
            </Button>
          )}
          {selectedNotifications.size > 0 && (
            <Button size="sm" variant="outline" onClick={handleArchiveSelected}>
              Archive Selected ({selectedNotifications.size})
            </Button>
          )}
        </div>
      </div>

      {/* Tabs */}
      <div className="mb-6 border-b border-stroke dark:border-strokedark">
        <div className="flex gap-4">
          {(["all", "unread", "archived"] as TabType[]).map((tab) => (
            <button
              key={tab}
              onClick={() => {
                setActiveTab(tab);
                setPageNumber(1);
                setSelectedNotifications(new Set());
              }}
              className={`px-4 py-2 font-medium text-sm border-b-2 transition-colors ${
                activeTab === tab
                  ? "border-primary text-primary"
                  : "border-transparent text-gray-600 hover:text-gray-900 dark:text-gray-400 dark:hover:text-white"
              }`}
            >
              {tab.charAt(0).toUpperCase() + tab.slice(1)}
            </button>
          ))}
        </div>
      </div>

      {error && (
        <div className="mb-4 rounded-md bg-red-50 p-3 text-sm text-red-800 dark:bg-red-900/20 dark:text-red-300">
          {error}
        </div>
      )}

      {/* Notifications List */}
      {loading ? (
        <div className="py-8 text-center text-gray-500">Loading notifications...</div>
      ) : notifications.length === 0 ? (
        <div className="py-8 text-center text-gray-500">
          No {activeTab === "unread" ? "unread" : activeTab === "archived" ? "archived" : ""}{" "}
          notifications found.
        </div>
      ) : (
        <div className="space-y-3">
          {notifications.map((notification) => (
            <div key={notification.notificationId} className="flex items-start gap-3">
              {activeTab === "all" && (
                <input
                  type="checkbox"
                  checked={selectedNotifications.has(notification.notificationId)}
                  onChange={() => toggleSelection(notification.notificationId)}
                  className="mt-4 rounded border-gray-300"
                />
              )}
              <div className="flex-1">
                <NotificationItem
                  notification={notification}
                  onMarkRead={handleMarkRead}
                  onArchive={handleArchive}
                  onUnarchive={handleUnarchive}
                />
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Pagination */}
      {total > 0 && (
        <div className="mt-4 flex items-center justify-between">
          <div className="text-sm text-gray-600 dark:text-gray-400">
            Showing {(pageNumber - 1) * pageSize + 1} to {Math.min(pageNumber * pageSize, total)}{" "}
            of {total} notifications
          </div>
          <div className="flex gap-2">
            <button
              onClick={() => setPageNumber((p) => Math.max(1, p - 1))}
              disabled={pageNumber === 1}
              className="rounded border border-stroke px-3 py-1 text-sm disabled:opacity-50 dark:border-strokedark"
            >
              Previous
            </button>
            <button
              onClick={() => setPageNumber((p) => p + 1)}
              disabled={pageNumber * pageSize >= total}
              className="rounded border border-stroke px-3 py-1 text-sm disabled:opacity-50 dark:border-strokedark"
            >
              Next
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

