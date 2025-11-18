"use client";
import { useState } from "react";
import { NotificationInbox, NotificationPreferencesModal } from "@/components/notifications";
import Button from "@/components/tailadmin/ui/button/Button";

export default function NotificationsPage() {
  const [preferencesModalOpen, setPreferencesModalOpen] = useState(false);

  return (
    <div className="mx-auto max-w-7xl">
      <div className="mb-4 flex items-center justify-end">
        <Button size="sm" variant="outline" onClick={() => setPreferencesModalOpen(true)}>
          Preferences
        </Button>
      </div>
      <NotificationInbox />
      <NotificationPreferencesModal
        isOpen={preferencesModalOpen}
        onClose={() => setPreferencesModalOpen(false)}
      />
    </div>
  );
}

