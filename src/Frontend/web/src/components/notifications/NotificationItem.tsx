'use client';

import { formatDistanceToNow } from 'date-fns';
import type { Notification } from '../../lib/types/notification.types';
import { useNotificationStore } from '../../store/notificationStore';
import { NotificationDispatchStatus } from './NotificationDispatchStatus';

interface NotificationItemProps {
  notification: Notification;
  onClick?: (notification: Notification) => void;
}

export function NotificationItem({ notification, onClick }: NotificationItemProps) {
  const { markAsRead } = useNotificationStore();

  const handleClick = async () => {
    if (!notification.isRead) {
      await markAsRead(notification.notificationId);
    }
    onClick?.(notification);
  };

  const getNotificationIcon = (typeName: string) => {
    switch (typeName) {
      case 'QuotationApproved':
        return '✅';
      case 'QuotationRejected':
        return '❌';
      case 'PaymentRequest':
        return '💰';
      case 'PaymentReceived':
        return '✅';
      case 'QuotationExpiring':
        return '⏰';
      case 'SystemMaintenance':
        return '🔧';
      case 'UserWelcome':
        return '👋';
      case 'PasswordChanged':
        return '🔒';
      case 'ProfileUpdated':
        return '👤';
      default:
        return '📢';
    }
  };

  const getChannelBadges = (sentVia: string) => {
    const channels = sentVia.split(',').map(c => c.trim());
    return channels.map(channel => (
      <span
        key={channel}
        className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-blue-100 text-blue-800"
      >
        {channel}
      </span>
    ));
  };

  return (
    <div
      className={`p-4 border-b border-gray-200 hover:bg-gray-50 cursor-pointer transition-colors ${
        !notification.isRead ? 'bg-blue-50 border-l-4 border-l-blue-500' : ''
      }`}
      onClick={handleClick}
    >
      <div className="flex items-start space-x-3">
        {/* Icon */}
        <div className="flex-shrink-0">
          <span className="text-2xl">
            {getNotificationIcon(notification.notificationType.typeName)}
          </span>
        </div>

        {/* Content */}
        <div className="flex-1 min-w-0">
          <div className="flex items-center justify-between">
            <h4 className={`text-sm font-medium ${!notification.isRead ? 'text-gray-900' : 'text-gray-700'}`}>
              {notification.title}
            </h4>
            {!notification.isRead && (
              <div className="flex-shrink-0">
                <div className="w-2 h-2 bg-blue-600 rounded-full"></div>
              </div>
            )}
          </div>

          <p className="mt-1 text-sm text-gray-600 line-clamp-2">
            {notification.message}
          </p>

          {/* Metadata */}
          <div className="mt-2 flex items-center justify-between">
            <div className="flex items-center space-x-2">
              <span className="text-xs text-gray-500">
                {formatDistanceToNow(new Date(notification.createdAt), { addSuffix: true })}
              </span>
              {notification.readAt && (
                <span className="text-xs text-gray-400">
                  • Read {formatDistanceToNow(new Date(notification.readAt), { addSuffix: true })}
                </span>
              )}
            </div>

            {/* Channel badges */}
            <div className="flex items-center space-x-1">
              {getChannelBadges(notification.sentVia)}
            </div>
          </div>

          {/* Dispatch Status */}
          {notification.dispatchStatus && notification.dispatchStatus.length > 0 && (
            <div className="mt-3">
              <NotificationDispatchStatus 
                dispatchStatuses={notification.dispatchStatus}
                showDetails={false}
              />
            </div>
          )}

          {/* Related entity info */}
          {notification.relatedEntityId && notification.relatedEntityType && (
            <div className="mt-2">
              <span className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-gray-100 text-gray-800">
                {notification.relatedEntityType}: {notification.relatedEntityId.slice(0, 8)}...
              </span>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
