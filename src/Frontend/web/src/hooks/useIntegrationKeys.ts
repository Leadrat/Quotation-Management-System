import { useState, useEffect } from "react";
import { AdminApi } from "@/lib/api";
import {
  IntegrationKeyDto,
  CreateIntegrationKeyRequest,
  UpdateIntegrationKeyRequest,
} from "@/types/admin";

export function useIntegrationKeys() {
  const [keys, setKeys] = useState<IntegrationKeyDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    loadKeys();
  }, []);

  const loadKeys = async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await AdminApi.getIntegrationKeys();
      setKeys(response.data);
    } catch (err: any) {
      setError(err.message || "Failed to load integration keys");
    } finally {
      setLoading(false);
    }
  };

  const createKey = async (payload: CreateIntegrationKeyRequest) => {
    setSaving(true);
    setError(null);
    try {
      const response = await AdminApi.createIntegrationKey(payload);
      await loadKeys(); // Reload list
      return { success: true, message: response.message, data: response.data };
    } catch (err: any) {
      const errorMessage = err.message || "Failed to create integration key";
      setError(errorMessage);
      return { success: false, message: errorMessage };
    } finally {
      setSaving(false);
    }
  };

  const updateKey = async (id: string, payload: UpdateIntegrationKeyRequest) => {
    setSaving(true);
    setError(null);
    try {
      const response = await AdminApi.updateIntegrationKey(id, payload);
      await loadKeys(); // Reload list
      return { success: true, message: response.message, data: response.data };
    } catch (err: any) {
      const errorMessage = err.message || "Failed to update integration key";
      setError(errorMessage);
      return { success: false, message: errorMessage };
    } finally {
      setSaving(false);
    }
  };

  const deleteKey = async (id: string) => {
    setSaving(true);
    setError(null);
    try {
      const response = await AdminApi.deleteIntegrationKey(id);
      await loadKeys(); // Reload list
      return { success: true, message: response.message };
    } catch (err: any) {
      const errorMessage = err.message || "Failed to delete integration key";
      setError(errorMessage);
      return { success: false, message: errorMessage };
    } finally {
      setSaving(false);
    }
  };

  const getKeyWithValue = async (id: string) => {
    try {
      const response = await AdminApi.getIntegrationKeyWithValue(id);
      return { success: true, data: response.data };
    } catch (err: any) {
      return { success: false, message: err.message || "Failed to get key value" };
    }
  };

  return {
    keys,
    loading,
    error,
    saving,
    createKey,
    updateKey,
    deleteKey,
    getKeyWithValue,
    refetch: loadKeys,
  };
}

