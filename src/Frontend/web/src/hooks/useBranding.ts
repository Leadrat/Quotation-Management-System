import { useState, useEffect } from "react";
import { AdminApi } from "@/lib/api";
import { CustomBrandingDto, UpdateBrandingRequest } from "@/types/admin";

export function useBranding() {
  const [branding, setBranding] = useState<CustomBrandingDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const [uploading, setUploading] = useState(false);

  useEffect(() => {
    loadBranding();
  }, []);

  const loadBranding = async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await AdminApi.getBranding();
      setBranding(response.data || null);
    } catch (err: any) {
      const errorMsg = err.message || "Failed to load branding settings";
      setError(errorMsg);
      setBranding(null);
      // Don't show error for 400 - may be validation issue or not set up yet
      if (err.message?.includes("400")) {
        setError("Branding settings are not configured yet. You can set them up below.");
      }
    } finally {
      setLoading(false);
    }
  };

  const updateBranding = async (payload: UpdateBrandingRequest) => {
    setSaving(true);
    setError(null);
    try {
      const response = await AdminApi.updateBranding(payload);
      setBranding(response.data);
      return { success: true, message: response.message };
    } catch (err: any) {
      const errorMessage = err.message || "Failed to update branding";
      setError(errorMessage);
      return { success: false, message: errorMessage };
    } finally {
      setSaving(false);
    }
  };

  const uploadLogo = async (file: File) => {
    setUploading(true);
    setError(null);
    try {
      const response = await AdminApi.uploadLogo(file);
      setBranding(response.data);
      return { success: true, message: response.message };
    } catch (err: any) {
      const errorMessage = err.message || "Failed to upload logo";
      setError(errorMessage);
      return { success: false, message: errorMessage };
    } finally {
      setUploading(false);
    }
  };

  return {
    branding,
    loading,
    error,
    saving,
    uploading,
    updateBranding,
    uploadLogo,
    refetch: loadBranding,
  };
}

