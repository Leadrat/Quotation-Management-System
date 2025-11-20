"use client";

import { useState } from "react";
import { useReportExport } from "@/hooks/useReportExport";
import type { ExportFormat } from "@/types/reports";

interface ExportButtonProps {
  reportId: string;
  reportName?: string;
}

export function ExportButton({ reportId, reportName = "Report" }: ExportButtonProps) {
  const { exporting, error, progress, exportReport } = useReportExport();
  const [showMenu, setShowMenu] = useState(false);

  const handleExport = async (format: ExportFormat) => {
    setShowMenu(false);
    await exportReport(reportId, format);
  };

  return (
    <div className="relative">
      <button
        onClick={() => setShowMenu(!showMenu)}
        disabled={exporting}
        className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed dark:bg-blue-500 dark:hover:bg-blue-600"
      >
        {exporting ? `Exporting... ${progress}%` : "Export"}
      </button>

      {showMenu && (
        <>
          <div className="fixed inset-0 z-10" onClick={() => setShowMenu(false)}></div>
          <div className="absolute right-0 mt-2 w-48 bg-white rounded-lg shadow-lg z-20 dark:bg-gray-800 border border-gray-200 dark:border-gray-700">
            <div className="py-1">
              <button
                onClick={() => handleExport("pdf")}
                className="block w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 dark:text-gray-300 dark:hover:bg-gray-700"
              >
                Export as PDF
              </button>
              <button
                onClick={() => handleExport("excel")}
                className="block w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 dark:text-gray-300 dark:hover:bg-gray-700"
              >
                Export as Excel
              </button>
              <button
                onClick={() => handleExport("csv")}
                className="block w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 dark:text-gray-300 dark:hover:bg-gray-700"
              >
                Export as CSV
              </button>
            </div>
          </div>
        </>
      )}

      {error && (
        <div className="mt-2 text-sm text-red-600 dark:text-red-400">{error}</div>
      )}

      {exporting && progress > 0 && (
        <div className="mt-2 w-full bg-gray-200 rounded-full h-2 dark:bg-gray-700">
          <div
            className="bg-blue-600 h-2 rounded-full transition-all duration-300 dark:bg-blue-500"
            style={{ width: `${progress}%` }}
          ></div>
        </div>
      )}
    </div>
  );
}

