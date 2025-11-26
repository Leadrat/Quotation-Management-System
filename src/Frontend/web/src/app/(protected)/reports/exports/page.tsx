"use client";
import { useState, useEffect } from "react";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import { ReportsApi } from "@/lib/api";
import type { ExportedReport } from "@/types/reports";

export default function ExportHistoryPage() {
  const [exports, setExports] = useState<ExportedReport[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(20);
  const [totalCount, setTotalCount] = useState(0);

  useEffect(() => {
    loadExports();
  }, [pageNumber]);

  const loadExports = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await ReportsApi.getExportHistory(pageNumber, pageSize);
      if (response.success && response.data) {
        setExports(response.data.items || response.data);
        setTotalCount(response.data.totalCount || response.data.length);
      }
    } catch (err: any) {
      console.error("Error loading export history:", err);
      setError(err.message || "Failed to load export history");
    } finally {
      setLoading(false);
    }
  };

  const formatFileSize = (bytes: number) => {
    if (bytes < 1024) return bytes + " B";
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(2) + " KB";
    return (bytes / (1024 * 1024)).toFixed(2) + " MB";
  };

  const handleDownload = (exportId: string) => {
    const downloadUrl = `/api/v1/reports/exports/${exportId}/download`;
    window.open(downloadUrl, "_blank");
  };

  const handleDelete = async (exportId: string) => {
    if (!confirm("Are you sure you want to delete this export?")) return;
    // TODO: Implement delete API call
    console.log("Delete export:", exportId);
  };

  return (
    <>
      <PageBreadcrumb pageTitle="Export History" />
      
      {error && (
        <div className="mb-6 rounded-lg bg-red-50 border border-red-200 p-4 dark:bg-red-900/20 dark:border-red-800">
          <p className="text-sm text-red-800 dark:text-red-400">{error}</p>
        </div>
      )}

      <ComponentCard title="Exported Reports">
        {loading ? (
          <div className="text-center py-12">
            <p className="text-gray-500 dark:text-gray-400">Loading export history...</p>
          </div>
        ) : exports.length === 0 ? (
          <div className="text-center py-12">
            <p className="text-gray-500 dark:text-gray-400">No exports found</p>
          </div>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="border-b border-gray-200 dark:border-gray-700">
                    <th className="text-left py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                      Report Type
                    </th>
                    <th className="text-left py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                      Format
                    </th>
                    <th className="text-left py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                      File Size
                    </th>
                    <th className="text-left py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                      Created At
                    </th>
                    <th className="text-center py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                      Actions
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {exports.map((exportItem) => (
                    <tr key={exportItem.exportId} className="border-b border-gray-100 dark:border-gray-800">
                      <td className="py-3 px-4 text-sm text-gray-900 dark:text-white">{exportItem.reportType}</td>
                      <td className="py-3 px-4 text-sm text-gray-700 dark:text-gray-300 uppercase">
                        {exportItem.exportFormat}
                      </td>
                      <td className="py-3 px-4 text-sm text-gray-700 dark:text-gray-300">
                        {formatFileSize(exportItem.fileSize)}
                      </td>
                      <td className="py-3 px-4 text-sm text-gray-700 dark:text-gray-300">
                        {new Date(exportItem.createdAt).toLocaleString()}
                      </td>
                      <td className="py-3 px-4 text-center">
                        <div className="flex items-center justify-center gap-2">
                          <button
                            onClick={() => handleDownload(exportItem.exportId)}
                            className="px-3 py-1 text-sm font-medium text-blue-600 hover:text-blue-700 dark:text-blue-400"
                          >
                            Download
                          </button>
                          <button
                            onClick={() => handleDelete(exportItem.exportId)}
                            className="px-3 py-1 text-sm font-medium text-red-600 hover:text-red-700 dark:text-red-400"
                          >
                            Delete
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {/* Pagination */}
            {totalCount > pageSize && (
              <div className="mt-6 flex items-center justify-between">
                <div className="text-sm text-gray-700 dark:text-gray-300">
                  Showing {(pageNumber - 1) * pageSize + 1} to {Math.min(pageNumber * pageSize, totalCount)} of{" "}
                  {totalCount} exports
                </div>
                <div className="flex gap-2">
                  <button
                    onClick={() => setPageNumber((p) => Math.max(1, p - 1))}
                    disabled={pageNumber === 1}
                    className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed dark:bg-gray-800 dark:text-gray-300 dark:border-gray-600"
                  >
                    Previous
                  </button>
                  <button
                    onClick={() => setPageNumber((p) => p + 1)}
                    disabled={pageNumber * pageSize >= totalCount}
                    className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed dark:bg-gray-800 dark:text-gray-300 dark:border-gray-600"
                  >
                    Next
                  </button>
                </div>
              </div>
            )}
          </>
        )}
      </ComponentCard>
    </>
  );
}

