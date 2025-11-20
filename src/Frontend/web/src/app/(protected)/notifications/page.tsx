"use client";
import { useState } from "react";
import { NotificationInbox, NotificationPreferencesModal } from "@/components/notifications";
import Button from "@/components/tailadmin/ui/button/Button";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";

export default function NotificationsPage() {
  const [preferencesModalOpen, setPreferencesModalOpen] = useState(false);

  return (
    <>
      <PageBreadcrumb pageTitle="Notifications" />
      <div className="mb-6 flex items-center justify-between">
        <h2 className="text-xl font-semibold text-gray-800 dark:text-white/90">Notifications</h2>
        <Button size="sm" variant="outline" onClick={() => setPreferencesModalOpen(true)}>
          Preferences
        </Button>
      </div>
      <NotificationInbox />
      <NotificationPreferencesModal
        isOpen={preferencesModalOpen}
        onClose={() => setPreferencesModalOpen(false)}
      />
    </>
  );
}

