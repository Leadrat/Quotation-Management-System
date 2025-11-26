'use client';

import Link from 'next/link';
import { NotificationDropdown } from '../notifications/NotificationDropdown';

export function Header() {
  return (
    <header className="bg-white shadow-sm border-b border-gray-200">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-16">
          {/* Logo */}
          <div className="flex items-center">
            <Link href="/" className="text-xl font-bold text-gray-900">
              CRM System
            </Link>
          </div>

          {/* Navigation */}
          <nav className="hidden md:flex space-x-8">
            <Link
              href="/dashboard"
              className="text-gray-600 hover:text-gray-900 px-3 py-2 rounded-md text-sm font-medium"
            >
              Dashboard
            </Link>
            <Link
              href="/clients"
              className="text-gray-600 hover:text-gray-900 px-3 py-2 rounded-md text-sm font-medium"
            >
              Clients
            </Link>
            <Link
              href="/quotations"
              className="text-gray-600 hover:text-gray-900 px-3 py-2 rounded-md text-sm font-medium"
            >
              Quotations
            </Link>
            <Link
              href="/notifications"
              className="text-gray-600 hover:text-gray-900 px-3 py-2 rounded-md text-sm font-medium"
            >
              Notifications
            </Link>
          </nav>

          {/* Right side - Notifications and User Menu */}
          <div className="flex items-center space-x-4">
            {/* Notification Dropdown */}
            <NotificationDropdown />

            {/* User Menu */}
            <div className="relative">
              <button className="flex items-center space-x-2 text-gray-600 hover:text-gray-900">
                <div className="w-8 h-8 bg-gray-300 rounded-full flex items-center justify-center">
                  <span className="text-sm font-medium">U</span>
                </div>
                <span className="text-sm font-medium">User</span>
              </button>
            </div>
          </div>
        </div>
      </div>
    </header>
  );
}
