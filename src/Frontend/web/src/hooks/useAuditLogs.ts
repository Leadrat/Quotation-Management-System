import { useState, useEffect } from "react";
import { AdminApi } from "@/lib/api";
import { AuditLogDto } from "@/types/admin";

export interface AuditLogFilters {
  actionType?: string;
  entity?: string;
  performedBy?: string;
  startDate?: string;
  endDate?: string;
}

export function useAuditLogs() {
  const [logs, setLogs] = useState<AuditLogDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [totalCount, setTotalCount] = useState(0);
  const [filters, setFilters] = useState<AuditLogFilters>({});

  useEffect(() => {
    loadLogs();
  }, [pageNumber, pageSize, filters]);

  const loadLogs = async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await AdminApi.getAuditLogs({
        ...filters,
        pageNumber,
        pageSize,
      });
      // Ensure logs is always an array
      setLogs(Array.isArray(response.data) ? response.data : []);
      setPageNumber(response.pageNumber || pageNumber);
      setPageSize(response.pageSize || pageSize);
      setTotalCount(response.totalCount || 0);
    } catch (err: any) {
      setError(err.message || "Failed to load audit logs");
      setLogs([]); // Set empty array on error
    } finally {
      setLoading(false);
    }
  };

  const applyFilters = (newFilters: AuditLogFilters) => {
    setFilters(newFilters);
    setPageNumber(1); // Reset to first page when filters change
  };

  const clearFilters = () => {
    setFilters({});
    setPageNumber(1);
  };

  const exportLogs = async () => {
    try {
      await AdminApi.exportAuditLogs(filters);
    } catch (err: any) {
      setError(err.message || "Failed to export audit logs");
    }
  };

  const getLogById = async (id: string) => {
    try {
      const response = await AdminApi.getAuditLogById(id);
      return { success: true, data: response.data };
    } catch (err: any) {
      return { success: false, message: err.message || "Failed to get audit log" };
    }
  };

  return {
    logs,
    loading,
    error,
    pageNumber,
    pageSize,
    totalCount,
    filters,
    setPageNumber,
    setPageSize,
    applyFilters,
    clearFilters,
    exportLogs,
    getLogById,
    refetch: loadLogs,
  };
}

