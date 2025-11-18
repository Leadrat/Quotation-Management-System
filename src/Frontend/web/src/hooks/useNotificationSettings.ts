import { useState, useEffect } from "react";
import { AdminApi } from "@/lib/api";
import {
  NotificationSettingsDto,
  UpdateNotificationSettingsRequest,
} from "@/types/admin";

export function useNotificationSettings() {
  const [settings, setSettings] = useState<NotificationSettingsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    loadSettings();
  }, []);

  const loadSettings = async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await AdminApi.getNotificationSettings();
      setSettings(response.data);
    } catch (err: any) {
      setError(err.message || "Failed to load notification settings");
    } finally {
      setLoading(false);
    }
  };

  const updateSettings = async (payload: UpdateNotificationSettingsRequest) => {
    setSaving(true);
    setError(null);
    try {
      const response = await AdminApi.updateNotificationSettings(payload);
      setSettings(response.data);
      return { success: true, message: response.message };
    } catch (err: any) {
      const errorMessage = err.message || "Failed to update notification settings";
      setError(errorMessage);
      return { success: false, message: errorMessage };
    } finally {
      setSaving(false);
    }
  };

  return {
    settings,
    loading,
    error,
    saving,
    updateSettings,
    refetch: loadSettings,
  };
}

