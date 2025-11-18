import { useState, useEffect } from "react";
import { AdminApi } from "@/lib/api";
import { SystemSettingsDto, UpdateSystemSettingsRequest } from "@/types/admin";

export function useAdminSettings() {
  const [settings, setSettings] = useState<SystemSettingsDto | null>(null);
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
      const response = await AdminApi.getSystemSettings();
      setSettings(response.data);
    } catch (err: any) {
      setError(err.message || "Failed to load system settings");
    } finally {
      setLoading(false);
    }
  };

  const updateSettings = async (payload: UpdateSystemSettingsRequest) => {
    setSaving(true);
    setError(null);
    try {
      const response = await AdminApi.updateSystemSettings(payload);
      setSettings(response.data);
      return { success: true, message: response.message };
    } catch (err: any) {
      const errorMessage = err.message || "Failed to update system settings";
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

