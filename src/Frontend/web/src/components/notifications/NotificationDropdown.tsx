'use client';

import { useState, useEffect, useRef } from 'react';
import Link from 'next/link';
import { NotificationBadge } from './NotificationBadge';
import { NotificationItem } from './NotificationItem';
import { useNotifications } from '../../hooks/useNotifications';
import type { Notification } from '../../lib/types/notification.types';

interface NotificationDropdownProps {
  className?: string;
}

export function NotificationDropdown({ className = '' }: NotificationDropdownProps) {
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);
  
  const { 
    notifications, 
    unreadCount, 
    isLoading, 
    loadNotifications,
    markAsRead 
  } = useNotifications();

  // Load recent notifications when dropdown opens
  useEffect(() => {
    if (isOpen) {
      loadNotifications({ pageSize: 10, pageNumber: 1 });
    }
  }, [isOpen, loadNotifications]);

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const handleNotificationClick = async (notification: Notification) => {
    if (!notification.isRead) {
      await markAsRead(notification.notificationId);
    }
    setIsOpen(false);
  };

  const recentNotifications = notifications.slice(0, 5);

  return (
    <div className={`relative ${className}`} ref={dropdownRef}>
      {/* Notification Bell Button */}
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="relative p-2 text-gray-600 hover:text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 rounded-md"
        aria-label="Notifications"
      >
        <svg
          className="w-6 h-6"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
          xmlns="http://www.w3.org/2000/svg"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"
          />
        </svg>
        
        {/* Notification Badge */}
        {unreadCount > 0 && (
          <NotificationBadge className="absolute -top-1 -right-1" />
        )}
      </button>

      {/* Dropdown Menu */}
      {isOpen && (
        <div className="absolute right-0 mt-2 w-96 bg-white rounded-lg shadow-lg border border-gray-200 z-50">
          {/* Header */}
          <div className="px-4 py-3 border-b border-gray-200">
            <div className="flex items-center justify-between">
              <h3 className="text-lg font-medium text-gray-900">Notifications</h3>
              <Link
                href="/notifications"
                onClick={() => setIsOpen(false)}
                className="text-sm text-blue-600 hover:text-blue-800"
              >
                View All
              </Link>
            </div>
          </div>

          {/* Notifications List */}
          <div className="max-h-96 overflow-y-auto">
            {isLoading ? (
              <div className="p-4 text-center">
                <div className="inline-flex items-center space-x-2 text-gray-600">
                  <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-blue-600"></div>
                  <span>Loading...</span>
                </div>
              </div>
            ) : recentNotifications.length > 0 ? (
              <>
                {recentNotifications.map((notification) => (
                  <div key={notification.notificationId} className="border-b border-gray-100 last:border-b-0">
                    <NotificationItem
                      notification={notification}
                      onClick={handleNotificationClick}
                    />
                  </div>
                ))}
                
                {notifications.length > 5 && (
                  <div className="p-3 text-center border-t border-gray-200">
                    <Link
                      href="/notifications"
                      onClick={() => setIsOpen(false)}
                      className="text-sm text-blue-600 hover:text-blue-800"
                    >
                      View {notifications.length - 5} more notifications
                    </Link>
                  </div>
                )}
              </>
            ) : (
              <div className="p-6 text-center">
                <div className="text-gray-400 mb-2">📭</div>
                <p className="text-gray-600">No notifications</p>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
