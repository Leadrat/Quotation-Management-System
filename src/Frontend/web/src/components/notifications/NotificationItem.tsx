"use client";
import { Notification } from "@/types/notifications";
import Link from "next/link";
import { formatDateTime } from "@/utils/quotationFormatter";

interface NotificationItemProps {
  notification: Notification;
  onMarkRead?: (notificationId: string) => void;
  onArchive?: (notificationId: string) => void;
  onUnarchive?: (notificationId: string) => void;
}

export function NotificationItem({
  notification,
  onMarkRead,
  onArchive,
  onUnarchive,
}: NotificationItemProps) {
  const getEventIcon = (eventType: string) => {
    switch (eventType) {
      case "QuotationSent":
        return "📧";
      case "QuotationViewed":
        return "👁️";
      case "QuotationAccepted":
        return "✅";
      case "QuotationRejected":
        return "❌";
      case "QuotationExpired":
        return "⏰";
      case "DiscountApprovalRequested":
        return "🔔";
      case "DiscountApprovalApproved":
        return "✓";
      case "DiscountApprovalRejected":
        return "✗";
      default:
        return "🔔";
    }
  };

  const getEventColor = (eventType: string) => {
    if (eventType.includes("Accepted") || eventType.includes("Approved")) {
      return "text-green-600 dark:text-green-400";
    }
    if (eventType.includes("Rejected") || eventType.includes("Expired")) {
      return "text-red-600 dark:text-red-400";
    }
    if (eventType.includes("Requested")) {
      return "text-yellow-600 dark:text-yellow-400";
    }
    return "text-blue-600 dark:text-blue-400";
  };

  return (
    <div
      className={`rounded-lg border p-4 transition-colors ${
        notification.isUnread
          ? "border-primary bg-primary/5 dark:bg-primary/10"
          : "border-stroke bg-white dark:border-strokedark dark:bg-boxdark"
      }`}
    >
      <div className="flex items-start gap-3">
        <div className={`text-2xl ${getEventColor(notification.eventType)}`}>
          {getEventIcon(notification.eventType)}
        </div>
        <div className="flex-1">
          <div className="flex items-start justify-between gap-2">
            <div className="flex-1">
              <p
                className={`text-sm ${
                  notification.isUnread
                    ? "font-semibold text-black dark:text-white"
                    : "text-gray-700 dark:text-gray-300"
                }`}
              >
                {notification.message}
              </p>
              <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">
                {formatDateTime(notification.createdAt)}
              </p>
              {notification.relatedEntityType && notification.relatedEntityId && (
                <Link
                  href={notification.entityLinkUrl}
                  className="mt-2 inline-block text-xs text-primary hover:underline"
                >
                  View {notification.relatedEntityType} →
                </Link>
              )}
            </div>
            {notification.isUnread && (
              <span className="h-2 w-2 rounded-full bg-primary"></span>
            )}
          </div>
          <div className="mt-3 flex items-center gap-2">
            {notification.isUnread && onMarkRead && (
              <button
                onClick={() => onMarkRead(notification.notificationId)}
                className="text-xs text-primary hover:underline"
              >
                Mark as read
              </button>
            )}
            {!notification.isArchived && onArchive && (
              <button
                onClick={() => onArchive(notification.notificationId)}
                className="text-xs text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
              >
                Archive
              </button>
            )}
            {notification.isArchived && onUnarchive && (
              <button
                onClick={() => onUnarchive(notification.notificationId)}
                className="text-xs text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
              >
                Unarchive
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

