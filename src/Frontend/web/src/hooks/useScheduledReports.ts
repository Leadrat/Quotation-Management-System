import { useState, useEffect, useCallback } from "react";
import { ScheduledReportsApi } from "@/lib/api";
import type { ScheduledReport, ScheduleReportRequest } from "@/types/reports";

export function useScheduledReports() {
  const [reports, setReports] = useState<ScheduledReport[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadReports = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await ScheduledReportsApi.list();
      if (response.success && response.data) {
        setReports(response.data);
      }
    } catch (err: any) {
      console.error("Error loading scheduled reports:", err);
      setError(err.message || "Failed to load scheduled reports");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadReports();
  }, [loadReports]);

  const createReport = useCallback(
    async (request: ScheduleReportRequest) => {
      try {
        setLoading(true);
        setError(null);
        const response = await ScheduledReportsApi.create(request);
        if (response.success) {
          await loadReports();
          return response.data;
        }
        return null;
      } catch (err: any) {
        console.error("Error creating scheduled report:", err);
        setError(err.message || "Failed to create scheduled report");
        return null;
      } finally {
        setLoading(false);
      }
    },
    [loadReports]
  );

  const deleteReport = useCallback(
    async (reportId: string) => {
      try {
        setLoading(true);
        setError(null);
        await ScheduledReportsApi.delete(reportId);
        await loadReports();
      } catch (err: any) {
        console.error("Error deleting scheduled report:", err);
        setError(err.message || "Failed to delete scheduled report");
      } finally {
        setLoading(false);
      }
    },
    [loadReports]
  );

  const sendTestEmail = useCallback(
    async (reportId: string, emailRecipients: string) => {
      try {
        setLoading(true);
        setError(null);
        const response = await ScheduledReportsApi.sendTest(reportId, emailRecipients);
        if (response.success) {
          return true;
        }
        return false;
      } catch (err: any) {
        console.error("Error sending test email:", err);
        setError(err.message || "Failed to send test email");
        return false;
      } finally {
        setLoading(false);
      }
    },
    []
  );

  return {
    reports,
    loading,
    error,
    createReport,
    deleteReport,
    sendTestEmail,
    refetch: loadReports,
  };
}

