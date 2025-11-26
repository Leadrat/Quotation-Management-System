'use client';

import { useState, useEffect } from 'react';
import type { NotificationFilters, NotificationType } from '../../lib/types/notification.types';
import { useNotificationStore } from '../../store/notificationStore';
import { notificationApi } from '../../lib/api/notifications';

interface NotificationFiltersProps {
  onFiltersChange: (filters: NotificationFilters) => void;
  currentFilters: NotificationFilters;
}

export function NotificationFilters({ onFiltersChange, currentFilters }: NotificationFiltersProps) {
  const [notificationTypes, setNotificationTypes] = useState<NotificationType[]>([]);
  const [localFilters, setLocalFilters] = useState<NotificationFilters>(currentFilters);
  const { clearFilters } = useNotificationStore();

  useEffect(() => {
    // Load notification types for filtering
    const loadNotificationTypes = async () => {
      try {
        const response = await notificationApi.getNotificationTypes();
        setNotificationTypes(response.data);
      } catch (error) {
        console.error('Failed to load notification types:', error);
      }
    };

    loadNotificationTypes();
  }, []);

  useEffect(() => {
    setLocalFilters(currentFilters);
  }, [currentFilters]);

  const handleFilterChange = (key: keyof NotificationFilters, value: any) => {
    const newFilters = { ...localFilters, [key]: value };
    setLocalFilters(newFilters);
    onFiltersChange(newFilters);
  };

  const handleClearFilters = () => {
    clearFilters();
    setLocalFilters({
      pageNumber: 1,
      pageSize: 20,
    });
  };

  const hasActiveFilters = localFilters.isRead !== undefined || 
                          localFilters.notificationTypeId || 
                          localFilters.fromDate || 
                          localFilters.toDate;

  return (
    <div className="bg-white p-4 border-b border-gray-200">
      <div className="flex flex-wrap items-center gap-4">
        {/* Read Status Filter */}
        <div className="flex items-center space-x-2">
          <label htmlFor="readStatus" className="text-sm font-medium text-gray-700">
            Status:
          </label>
          <select
            id="readStatus"
            value={localFilters.isRead === undefined ? 'all' : localFilters.isRead.toString()}
            onChange={(e) => {
              const value = e.target.value === 'all' ? undefined : e.target.value === 'true';
              handleFilterChange('isRead', value);
            }}
            className="block w-32 px-3 py-1 text-sm border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="all">All</option>
            <option value="false">Unread</option>
            <option value="true">Read</option>
          </select>
        </div>

        {/* Notification Type Filter */}
        <div className="flex items-center space-x-2">
          <label htmlFor="notificationType" className="text-sm font-medium text-gray-700">
            Type:
          </label>
          <select
            id="notificationType"
            value={localFilters.notificationTypeId || ''}
            onChange={(e) => handleFilterChange('notificationTypeId', e.target.value || undefined)}
            className="block w-48 px-3 py-1 text-sm border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="">All Types</option>
            {notificationTypes.map((type) => (
              <option key={type.notificationTypeId} value={type.notificationTypeId}>
                {type.typeName}
              </option>
            ))}
          </select>
        </div>

        {/* Date Range Filters */}
        <div className="flex items-center space-x-2">
          <label htmlFor="fromDate" className="text-sm font-medium text-gray-700">
            From:
          </label>
          <input
            type="date"
            id="fromDate"
            value={localFilters.fromDate ? localFilters.fromDate.split('T')[0] : ''}
            onChange={(e) => {
              const value = e.target.value ? `${e.target.value}T00:00:00.000Z` : undefined;
              handleFilterChange('fromDate', value);
            }}
            className="block px-3 py-1 text-sm border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          />
        </div>

        <div className="flex items-center space-x-2">
          <label htmlFor="toDate" className="text-sm font-medium text-gray-700">
            To:
          </label>
          <input
            type="date"
            id="toDate"
            value={localFilters.toDate ? localFilters.toDate.split('T')[0] : ''}
            onChange={(e) => {
              const value = e.target.value ? `${e.target.value}T23:59:59.999Z` : undefined;
              handleFilterChange('toDate', value);
            }}
            className="block px-3 py-1 text-sm border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          />
        </div>

        {/* Clear Filters Button */}
        {hasActiveFilters && (
          <button
            onClick={handleClearFilters}
            className="px-3 py-1 text-sm text-gray-600 hover:text-gray-800 border border-gray-300 rounded-md hover:bg-gray-50 transition-colors"
          >
            Clear Filters
          </button>
        )}
      </div>

      {/* Active Filters Summary */}
      {hasActiveFilters && (
        <div className="mt-3 flex flex-wrap items-center gap-2">
          <span className="text-sm text-gray-500">Active filters:</span>
          {localFilters.isRead !== undefined && (
            <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
              {localFilters.isRead ? 'Read' : 'Unread'}
            </span>
          )}
          {localFilters.notificationTypeId && (
            <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-green-100 text-green-800">
              {notificationTypes.find(t => t.notificationTypeId === localFilters.notificationTypeId)?.typeName || 'Type Filter'}
            </span>
          )}
          {localFilters.fromDate && (
            <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-purple-100 text-purple-800">
              From: {new Date(localFilters.fromDate).toLocaleDateString()}
            </span>
          )}
          {localFilters.toDate && (
            <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-purple-100 text-purple-800">
              To: {new Date(localFilters.toDate).toLocaleDateString()}
            </span>
          )}
        </div>
      )}
    </div>
  );
}
