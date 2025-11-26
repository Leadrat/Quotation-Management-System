'use client';

import { useEffect } from 'react';
import { useNotificationStore } from '../../store/notificationStore';

interface NotificationBadgeProps {
  className?: string;
  showZero?: boolean;
}

export function NotificationBadge({ className = '', showZero = false }: NotificationBadgeProps) {
  const { unreadCount, refreshUnreadCount } = useNotificationStore();

  useEffect(() => {
    // Load initial unread count
    refreshUnreadCount();
    
    // Refresh every 30 seconds
    const interval = setInterval(refreshUnreadCount, 30000);
    
    return () => clearInterval(interval);
  }, [refreshUnreadCount]);

  if (!showZero && unreadCount === 0) {
    return null;
  }

  return (
    <span
      className={`inline-flex items-center justify-center px-2 py-1 text-xs font-bold leading-none text-white bg-red-600 rounded-full ${className}`}
    >
      {unreadCount > 99 ? '99+' : unreadCount}
    </span>
  );
}
