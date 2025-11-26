import { useState, useEffect, useCallback } from "react";
import { ReportsApi } from "@/lib/api";
import type { ForecastingData } from "@/types/reports";

interface UseForecastOptions {
  days?: number;
  confidenceLevel?: number;
  autoLoad?: boolean;
}

export function useForecast(options: UseForecastOptions = {}) {
  const { days = 30, confidenceLevel = 0.95, autoLoad = false } = options;
  const [data, setData] = useState<ForecastingData | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadForecast = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await ReportsApi.getForecasting(days, confidenceLevel);
      if (response.success && response.data) {
        setData(response.data);
      } else {
        setError("Failed to load forecast data");
      }
    } catch (err: any) {
      console.error("Error loading forecast:", err);
      setError(err.message || "Failed to load forecast data");
    } finally {
      setLoading(false);
    }
  }, [days, confidenceLevel]);

  useEffect(() => {
    if (autoLoad) {
      loadForecast();
    }
  }, [autoLoad, loadForecast]);

  const refetch = useCallback(() => {
    loadForecast();
  }, [loadForecast]);

  return {
    data,
    loading,
    error,
    loadForecast,
    refetch,
  };
}

