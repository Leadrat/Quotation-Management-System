'use client';

import { NotificationConfigurationPanel } from '@/components/notifications/NotificationConfigurationPanel';
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";

export default function NotificationConfigPage() {
  return (
    <>
      <PageBreadcrumb pageTitle="Notification Configuration" />
      <div className="mb-6">
        <h2 className="text-xl font-semibold text-gray-800 dark:text-white/90">Notification Configuration</h2>
        <p className="text-gray-600 dark:text-gray-400">
          Configure notification settings and preferences
        </p>
      </div>
      <NotificationConfigurationPanel />
    </>
  );
}
