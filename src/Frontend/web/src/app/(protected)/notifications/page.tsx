"use client";
import { useState } from "react";
import Link from "next/link";
import { NotificationInbox, NotificationPreferencesModal } from "@/components/notifications";
import Button from "@/components/tailadmin/ui/button/Button";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import { Settings, BarChart3 } from "lucide-react";

export default function NotificationsPage() {
  const [preferencesModalOpen, setPreferencesModalOpen] = useState(false);

  return (
    <>
      <PageBreadcrumb pageTitle="Notifications" />
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h2 className="text-xl font-semibold text-gray-800 dark:text-white/90">Notifications</h2>
          <p className="text-gray-600 dark:text-gray-400">
            Stay updated with important system events and activities
          </p>
        </div>
        <div className="flex items-center gap-2">
          <Button size="sm" variant="outline" asChild>
            <Link href="/notifications/dispatch">
              <BarChart3 className="h-4 w-4 mr-2" />
              Dispatch Status
            </Link>
          </Button>
          <Button size="sm" variant="outline" asChild>
            <Link href="/notifications/config">
              <Settings className="h-4 w-4 mr-2" />
              Configuration
            </Link>
          </Button>
          <Button size="sm" variant="outline" onClick={() => setPreferencesModalOpen(true)}>
            Preferences
          </Button>
        </div>
      </div>
      <NotificationInbox />
      <NotificationPreferencesModal
        isOpen={preferencesModalOpen}
        onClose={() => setPreferencesModalOpen(false)}
      />
    </>
  );
}

