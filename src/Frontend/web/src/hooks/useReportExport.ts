import { useState, useCallback } from "react";
import { ReportsApi } from "@/lib/api";
import type { ExportFormat, ExportReportRequest } from "@/types/reports";

export function useReportExport() {
  const [exporting, setExporting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [progress, setProgress] = useState(0);

  const exportReport = useCallback(
    async (reportId: string, format: ExportFormat) => {
      try {
        setExporting(true);
        setError(null);
        setProgress(0);

        const request: ExportReportRequest = {
          reportId,
          format,
        };

        setProgress(25);
        const response = await ReportsApi.exportReport(request);
        setProgress(50);

        if (response.success && response.data) {
          setProgress(75);
          // Download the file
          const downloadUrl = response.data.downloadUrl || `/api/v1/reports/exports/${response.data.exportId}/download`;
          window.open(downloadUrl, "_blank");
          setProgress(100);
          return response.data;
        } else {
          setError("Failed to export report");
          return null;
        }
      } catch (err: any) {
        console.error("Error exporting report:", err);
        setError(err.message || "Failed to export report");
        return null;
      } finally {
        setExporting(false);
        setTimeout(() => setProgress(0), 1000);
      }
    },
    []
  );

  return {
    exporting,
    error,
    progress,
    exportReport,
  };
}

