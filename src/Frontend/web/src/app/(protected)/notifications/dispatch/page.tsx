'use client';

import { DispatchStatusDashboard } from '@/components/notifications/DispatchStatusDashboard';
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";

export default function NotificationDispatchPage() {
  return (
    <>
      <PageBreadcrumb pageTitle="Notification Dispatch" />
      <div className="mb-6">
        <h2 className="text-xl font-semibold text-gray-800 dark:text-white/90">Notification Dispatch</h2>
        <p className="text-gray-600 dark:text-gray-400">
          Monitor and manage notification delivery across all channels
        </p>
      </div>
      <DispatchStatusDashboard />
    </>
  );
}
