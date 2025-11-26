'use client';

import { useEffect, useCallback } from 'react';
import { useNotificationStore } from '../../store/notificationStore';
import { NotificationItem } from './NotificationItem';
import { NotificationFilters } from './NotificationFilters';
import type { Notification, NotificationFilters as FilterType } from '../../lib/types/notification.types';

interface NotificationListProps {
  onNotificationClick?: (notification: Notification) => void;
  showFilters?: boolean;
  className?: string;
}

export function NotificationList({ 
  onNotificationClick, 
  showFilters = true, 
  className = '' 
}: NotificationListProps) {
  const {
    notifications,
    totalCount,
    isLoading,
    error,
    hasMore,
    filters,
    loadNotifications,
    loadMoreNotifications,
    setFilters,
  } = useNotificationStore();

  // Load initial notifications
  useEffect(() => {
    loadNotifications();
  }, [loadNotifications]);

  const handleFiltersChange = useCallback((newFilters: FilterType) => {
    setFilters(newFilters);
    loadNotifications(newFilters);
  }, [setFilters, loadNotifications]);

  const handleLoadMore = useCallback(() => {
    if (!isLoading && hasMore) {
      loadMoreNotifications();
    }
  }, [isLoading, hasMore, loadMoreNotifications]);

  if (error) {
    return (
      <div className={`bg-white rounded-lg shadow ${className}`}>
        <div className="p-6 text-center">
          <div className="text-red-600 mb-2">⚠️</div>
          <h3 className="text-lg font-medium text-gray-900 mb-2">Error Loading Notifications</h3>
          <p className="text-gray-600 mb-4">{error}</p>
          <button
            onClick={() => loadNotifications()}
            className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
          >
            Try Again
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className={`bg-white rounded-lg shadow ${className}`}>
      {/* Header */}
      <div className="px-6 py-4 border-b border-gray-200">
        <div className="flex items-center justify-between">
          <h2 className="text-lg font-medium text-gray-900">
            Notifications
            {totalCount > 0 && (
              <span className="ml-2 text-sm text-gray-500">({totalCount})</span>
            )}
          </h2>
          <button
            onClick={() => loadNotifications()}
            disabled={isLoading}
            className="px-3 py-1 text-sm text-gray-600 hover:text-gray-800 border border-gray-300 rounded-md hover:bg-gray-50 transition-colors disabled:opacity-50"
          >
            {isLoading ? 'Refreshing...' : 'Refresh'}
          </button>
        </div>
      </div>

      {/* Filters */}
      {showFilters && (
        <NotificationFilters
          currentFilters={filters}
          onFiltersChange={handleFiltersChange}
        />
      )}

      {/* Notifications List */}
      <div className="divide-y divide-gray-200">
        {notifications.length === 0 && !isLoading ? (
          <div className="p-8 text-center">
            <div className="text-gray-400 mb-2">📭</div>
            <h3 className="text-lg font-medium text-gray-900 mb-2">No Notifications</h3>
            <p className="text-gray-600">
              {Object.keys(filters).length > 2 
                ? 'No notifications match your current filters.'
                : 'You have no notifications at this time.'
              }
            </p>
          </div>
        ) : (
          <>
            {notifications.map((notification) => (
              <NotificationItem
                key={notification.notificationId}
                notification={notification}
                onClick={onNotificationClick}
              />
            ))}

            {/* Loading indicator */}
            {isLoading && (
              <div className="p-4 text-center">
                <div className="inline-flex items-center space-x-2 text-gray-600">
                  <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-blue-600"></div>
                  <span>Loading notifications...</span>
                </div>
              </div>
            )}

            {/* Load More Button */}
            {hasMore && !isLoading && (
              <div className="p-4 text-center border-t border-gray-200">
                <button
                  onClick={handleLoadMore}
                  className="px-4 py-2 text-sm text-blue-600 hover:text-blue-800 border border-blue-300 rounded-md hover:bg-blue-50 transition-colors"
                >
                  Load More Notifications
                </button>
              </div>
            )}

            {/* End of list indicator */}
            {!hasMore && notifications.length > 0 && (
              <div className="p-4 text-center text-sm text-gray-500 border-t border-gray-200">
                You've reached the end of your notifications
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
}
