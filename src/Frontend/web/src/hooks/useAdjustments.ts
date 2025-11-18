import { useState, useEffect } from "react";
import { AdjustmentDto } from "@/types/refunds";
import { AdjustmentsApi } from "@/lib/api";

export function useAdjustments(quotationId: string) {
  const [adjustments, setAdjustments] = useState<AdjustmentDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (quotationId) {
      loadAdjustments();
    }
  }, [quotationId]);

  const loadAdjustments = async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await AdjustmentsApi.getByQuotation(quotationId);
      setAdjustments(response.data || []);
    } catch (err: any) {
      setError(err.message || "Failed to load adjustments");
    } finally {
      setLoading(false);
    }
  };

  return { adjustments, loading, error, refetch: loadAdjustments };
}

