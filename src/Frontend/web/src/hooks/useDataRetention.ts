import { useState, useEffect } from "react";
import { AdminApi } from "@/lib/api";
import {
  DataRetentionPolicyDto,
  UpdateDataRetentionPolicyRequest,
} from "@/types/admin";

export function useDataRetention() {
  const [policies, setPolicies] = useState<DataRetentionPolicyDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    loadPolicies();
  }, []);

  const loadPolicies = async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await AdminApi.getDataRetentionPolicies();
      setPolicies(response.data);
    } catch (err: any) {
      setError(err.message || "Failed to load data retention policies");
    } finally {
      setLoading(false);
    }
  };

  const updatePolicy = async (payload: UpdateDataRetentionPolicyRequest) => {
    setSaving(true);
    setError(null);
    try {
      const response = await AdminApi.updateDataRetentionPolicy(payload);
      await loadPolicies(); // Reload list
      return { success: true, message: response.message, data: response.data };
    } catch (err: any) {
      const errorMessage = err.message || "Failed to update data retention policy";
      setError(errorMessage);
      return { success: false, message: errorMessage };
    } finally {
      setSaving(false);
    }
  };

  return {
    policies,
    loading,
    error,
    saving,
    updatePolicy,
    refetch: loadPolicies,
  };
}

