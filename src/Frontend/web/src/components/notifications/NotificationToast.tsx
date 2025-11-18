"use client";
import { useEffect, useState } from "react";
import { Notification } from "@/types/notifications";
import { formatDateTime } from "@/utils/quotationFormatter";
import Link from "next/link";

interface NotificationToastProps {
  notification: Notification;
  onClose: () => void;
  duration?: number;
}

export function NotificationToast({
  notification,
  onClose,
  duration = 5000,
}: NotificationToastProps) {
  const [isVisible, setIsVisible] = useState(true);

  useEffect(() => {
    const timer = setTimeout(() => {
      setIsVisible(false);
      setTimeout(onClose, 300); // Wait for fade-out animation
    }, duration);

    return () => clearTimeout(timer);
  }, [duration, onClose]);

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
      return "border-green-500 bg-green-50 dark:bg-green-900/20";
    }
    if (eventType.includes("Rejected") || eventType.includes("Expired")) {
      return "border-red-500 bg-red-50 dark:bg-red-900/20";
    }
    if (eventType.includes("Requested")) {
      return "border-yellow-500 bg-yellow-50 dark:bg-yellow-900/20";
    }
    return "border-blue-500 bg-blue-50 dark:bg-blue-900/20";
  };

  if (!isVisible) return null;

  return (
    <div
      className={`fixed top-4 right-4 z-50 min-w-[320px] max-w-md rounded-lg border-l-4 p-4 shadow-lg transition-all ${
        isVisible ? "translate-x-0 opacity-100" : "translate-x-full opacity-0"
      } ${getEventColor(notification.eventType)}`}
    >
      <div className="flex items-start gap-3">
        <div className="text-2xl">{getEventIcon(notification.eventType)}</div>
        <div className="flex-1">
          <p className="text-sm font-medium text-gray-900 dark:text-white">
            {notification.message}
          </p>
          <p className="mt-1 text-xs text-gray-600 dark:text-gray-400">
            {formatDateTime(notification.createdAt)}
          </p>
          {notification.relatedEntityType && notification.relatedEntityId && (
            <Link
              href={notification.entityLinkUrl}
              className="mt-2 inline-block text-xs text-primary hover:underline"
              onClick={onClose}
            >
              View {notification.relatedEntityType} →
            </Link>
          )}
        </div>
        <button
          onClick={onClose}
          className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-200"
        >
          <svg
            className="h-4 w-4"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M6 18L18 6M6 6l12 12"
            />
          </svg>
        </button>
      </div>
    </div>
  );
}

