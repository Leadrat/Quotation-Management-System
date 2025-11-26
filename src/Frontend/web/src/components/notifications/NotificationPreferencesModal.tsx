"use client";
import { useState, useEffect } from "react";
import { Modal } from "@/components/tailadmin/ui/modal";
import Button from "@/components/tailadmin/ui/button/Button";
import Label from "@/components/tailadmin/form/Label";
import { NotificationsApi } from "@/lib/api";
import { NotificationPreferences } from "@/types/notifications";

interface NotificationPreferencesModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess?: () => void;
}

const EVENT_TYPES = [
  "QuotationSent",
  "QuotationViewed",
  "QuotationAccepted",
  "QuotationRejected",
  "QuotationExpired",
  "DiscountApprovalRequested",
  "DiscountApprovalApproved",
  "DiscountApprovalRejected",
];

const CHANNELS = ["inApp", "email"];

export function NotificationPreferencesModal({
  isOpen,
  onClose,
  onSuccess,
}: NotificationPreferencesModalProps) {
  const [loading, setLoading] = useState(false);
  const [loadingPrefs, setLoadingPrefs] = useState(true);
  const [preferences, setPreferences] = useState<Record<string, Record<string, boolean>>>({});
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen) {
      loadPreferences();
    }
  }, [isOpen]);

  const loadPreferences = async () => {
    try {
      setLoadingPrefs(true);
      const result = await NotificationsApi.getPreferences();
      setPreferences(result.data?.preferences || {});
    } catch (err: any) {
      setError(err.message || "Failed to load preferences");
    } finally {
      setLoadingPrefs(false);
    }
  };

  const handleToggle = (eventType: string, channel: string) => {
    setPreferences((prev) => {
      const newPrefs = { ...prev };
      if (!newPrefs[eventType]) {
        newPrefs[eventType] = {};
      }
      newPrefs[eventType] = {
        ...newPrefs[eventType],
        [channel]: !newPrefs[eventType][channel],
      };
      return newPrefs;
    });
  };

  const handleSave = async () => {
    try {
      setLoading(true);
      setError(null);
      await NotificationsApi.updatePreferences({ preferences });
      onSuccess?.();
      onClose();
    } catch (err: any) {
      setError(err.message || "Failed to save preferences");
    } finally {
      setLoading(false);
    }
  };

  const getDefaultValue = (eventType: string, channel: string): boolean => {
    return preferences[eventType]?.[channel] ?? true; // Default to enabled
  };

  if (!isOpen) return null;

  return (
    <Modal isOpen={isOpen} onClose={onClose} className="max-w-[800px] p-5 lg:p-10">
      <h4 className="mb-4 text-title-sm font-semibold text-gray-800 dark:text-white/90">
        Notification Preferences
      </h4>
      <p className="mb-6 text-sm text-gray-600 dark:text-gray-400">
        Configure how you receive notifications for different events.
      </p>

      {loadingPrefs ? (
        <div className="py-8 text-center text-gray-500">Loading preferences...</div>
      ) : (
        <div className="space-y-6">
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-stroke dark:border-strokedark">
                  <th className="px-4 py-3 text-left text-sm font-medium text-black dark:text-white">
                    Event Type
                  </th>
                  {CHANNELS.map((channel) => (
                    <th
                      key={channel}
                      className="px-4 py-3 text-center text-sm font-medium text-black dark:text-white"
                    >
                      {channel === "inApp" ? "In-App" : "Email"}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {EVENT_TYPES.map((eventType) => (
                  <tr
                    key={eventType}
                    className="border-b border-stroke dark:border-strokedark"
                  >
                    <td className="px-4 py-3 text-sm text-black dark:text-white">
                      {eventType.replace(/([A-Z])/g, " $1").trim()}
                    </td>
                    {CHANNELS.map((channel) => (
                      <td key={channel} className="px-4 py-3 text-center">
                        <input
                          type="checkbox"
                          checked={getDefaultValue(eventType, channel)}
                          onChange={() => handleToggle(eventType, channel)}
                          className="h-4 w-4 rounded border-gray-300"
                        />
                      </td>
                    ))}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {error && (
            <div className="rounded-md bg-red-50 p-3 text-sm text-red-800 dark:bg-red-900/20 dark:text-red-300">
              {error}
            </div>
          )}

          <div className="flex items-center justify-end gap-3">
            <Button size="sm" variant="outline" onClick={onClose} disabled={loading}>
              Cancel
            </Button>
            <Button size="sm" onClick={handleSave} disabled={loading}>
              {loading ? "Saving..." : "Save Preferences"}
            </Button>
          </div>
        </div>
      )}
    </Modal>
  );
}

