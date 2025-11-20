import { useState, useEffect, useCallback } from "react";
import { ReportsApi } from "@/lib/api";
import type { ReportData, ReportGenerationRequest } from "@/types/reports";

interface UseReportOptions {
  reportType?: string;
  filters?: Record<string, any>;
  autoLoad?: boolean;
}

export function useReport(options: UseReportOptions = {}) {
  const { reportType, filters, autoLoad = false } = options;
  const [data, setData] = useState<ReportData | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadReport = useCallback(
    async (request?: ReportGenerationRequest) => {
      try {
        setLoading(true);
        setError(null);

        const reportRequest: ReportGenerationRequest = request || {
          reportType: reportType || "custom",
          filters: filters || {},
          format: "json",
        };

        const response = await ReportsApi.generateCustomReport(reportRequest);
        if (response.success && response.data) {
          setData(response.data);
        } else {
          setError("Failed to generate report");
        }
      } catch (err: any) {
        console.error("Error loading report:", err);
        setError(err.message || "Failed to load report");
      } finally {
        setLoading(false);
      }
    },
    [reportType, filters]
  );

  useEffect(() => {
    if (autoLoad && reportType) {
      loadReport();
    }
  }, [autoLoad, reportType, loadReport]);

  const refetch = useCallback(() => {
    loadReport();
  }, [loadReport]);

  return {
    data,
    loading,
    error,
    loadReport,
    refetch,
  };
}

