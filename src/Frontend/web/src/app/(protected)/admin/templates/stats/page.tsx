"use client";
import { useEffect, useState } from "react";
import { TemplatesApi } from "@/lib/api";
import { UsageStatsWidgets, TemplateUsageChart } from "@/components/templates/admin";
import type { TemplateUsageStats } from "@/types/templates";

export default function TemplateUsageStatsPage() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [stats, setStats] = useState<TemplateUsageStats | null>(null);
  const [dateFrom, setDateFrom] = useState("");
  const [dateTo, setDateTo] = useState("");

  useEffect(() => {
    loadStats();
  }, [dateFrom, dateTo]);

  const loadStats = async () => {
    try {
      setLoading(true);
      setError(null);
      const params: any = {};
      if (dateFrom) params.startDate = dateFrom;
      if (dateTo) params.endDate = dateTo;

      const result = await TemplatesApi.getUsageStats(params);
      setStats(result.data || null);
    } catch (err: any) {
      const errorMsg = err.message || "Failed to load usage statistics";
      setError(errorMsg);
      setStats(null);
      // Don't show error for 500 - endpoint may have issues
      if (err.message?.includes("500")) {
        setError("Template usage statistics are temporarily unavailable. Please try again later.");
      }
    } finally {
      setLoading(false);
    }
  };

  const handleExport = () => {
    // Create CSV export
    if (!stats) return;

    const csvRows: string[] = [];
    csvRows.push("Template Usage Statistics");
    csvRows.push(`Generated: ${new Date().toISOString()}`);
    csvRows.push("");
    csvRows.push("Metric,Value");
    csvRows.push(`Total Templates,${stats.totalTemplates}`);
    csvRows.push(`Total Usage,${stats.totalUsage}`);
    csvRows.push(`Approved Templates,${stats.approvedTemplates}`);
    csvRows.push(`Pending Approval,${stats.pendingApprovalTemplates}`);
    csvRows.push("");
    csvRows.push("Most Used Templates");
    csvRows.push("Template Name,Usage Count,Last Used");
    stats.mostUsedTemplates.forEach((t) => {
      csvRows.push(`"${t.name}",${t.usageCount},"${t.lastUsedAt || "Never"}"`);
    });

    const csvContent = csvRows.join("\n");
    const blob = new Blob([csvContent], { type: "text/csv" });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = `template-usage-stats-${new Date().toISOString().split("T")[0]}.csv`;
    a.click();
    window.URL.revokeObjectURL(url);
  };

  const formatDate = (date: string) => {
    return new Date(date).toLocaleDateString();
  };

  return (
    <div className="rounded-sm border border-stroke bg-white px-5 pb-2.5 pt-6 shadow-default dark:border-strokedark dark:bg-boxdark sm:px-7.5 xl:pb-1">
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h4 className="text-title-md2 font-bold text-black dark:text-white">Template Usage Statistics</h4>
          <p className="mt-1 text-sm text-gray-600 dark:text-gray-400">
            Analyze template usage patterns and performance metrics
          </p>
        </div>
        <button
          onClick={handleExport}
          disabled={!stats}
          className="rounded bg-primary px-4 py-2 text-sm text-white hover:bg-opacity-90 disabled:opacity-50"
        >
          Export CSV
        </button>
      </div>

      {/* Date Filters */}
      <div className="mb-6 grid grid-cols-1 gap-4 md:grid-cols-3">
        <div>
          <label className="mb-2.5 block text-black dark:text-white">Start Date</label>
          <input
            type="date"
            value={dateFrom}
            onChange={(e) => setDateFrom(e.target.value)}
            className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 font-medium outline-none transition focus:border-primary active:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
          />
        </div>
        <div>
          <label className="mb-2.5 block text-black dark:text-white">End Date</label>
          <input
            type="date"
            value={dateTo}
            onChange={(e) => setDateTo(e.target.value)}
            className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 font-medium outline-none transition focus:border-primary active:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
          />
        </div>
        <div className="flex items-end">
          <button
            onClick={() => {
              setDateFrom("");
              setDateTo("");
            }}
            className="w-full rounded border border-stroke px-4 py-3 text-sm font-medium hover:bg-gray-50 dark:border-strokedark dark:hover:bg-meta-4"
          >
            Clear Dates
          </button>
        </div>
      </div>

      {error && (
        <div className="mb-4 rounded border-l-4 border-red-500 bg-red-50 p-4 dark:bg-red-900/20">
          <p className="text-red-700 dark:text-red-400">{error}</p>
        </div>
      )}

      {loading ? (
        <div className="py-8 text-center">Loading statistics...</div>
      ) : !stats ? (
        <div className="py-8 text-center text-gray-500">No statistics available</div>
      ) : (
        <>
          {/* Stats Widgets */}
          <UsageStatsWidgets stats={stats} />

          {/* Usage Chart */}
          <div className="mb-6">
            <TemplateUsageChart stats={stats} />
          </div>

          {/* Usage by Visibility */}
          <div className="mb-6 rounded border border-stroke bg-white p-6 dark:border-strokedark dark:bg-boxdark">
            <h5 className="mb-4 text-lg font-semibold text-black dark:text-white">Usage by Visibility</h5>
            <div className="space-y-3">
              {Object.entries(stats.usageByVisibility).map(([visibility, count]) => (
                <div key={visibility} className="flex items-center justify-between">
                  <span className="text-black dark:text-white">{visibility}</span>
                  <span className="font-medium text-black dark:text-white">{count}</span>
                </div>
              ))}
            </div>
          </div>

          {/* Usage by Role */}
          <div className="mb-6 rounded border border-stroke bg-white p-6 dark:border-strokedark dark:bg-boxdark">
            <h5 className="mb-4 text-lg font-semibold text-black dark:text-white">Usage by Role</h5>
            <div className="space-y-3">
              {Object.entries(stats.usageByRole).map(([role, count]) => (
                <div key={role} className="flex items-center justify-between">
                  <span className="text-black dark:text-white">{role}</span>
                  <span className="font-medium text-black dark:text-white">{count}</span>
                </div>
              ))}
            </div>
          </div>

          {/* Most Used Templates Table */}
          <div className="rounded border border-stroke bg-white p-6 dark:border-strokedark dark:bg-boxdark">
            <h5 className="mb-4 text-lg font-semibold text-black dark:text-white">Most Used Templates</h5>
            {stats.mostUsedTemplates.length === 0 ? (
              <p className="text-center text-gray-500 py-4">No usage data available</p>
            ) : (
              <div className="max-w-full overflow-x-auto">
                <table className="w-full table-auto">
                  <thead>
                    <tr className="bg-gray-2 text-left dark:bg-meta-4">
                      <th className="px-4 py-3 font-medium text-black dark:text-white">Template Name</th>
                      <th className="px-4 py-3 font-medium text-black dark:text-white">Usage Count</th>
                      <th className="px-4 py-3 font-medium text-black dark:text-white">Last Used</th>
                    </tr>
                  </thead>
                  <tbody>
                    {stats.mostUsedTemplates.map((template) => (
                      <tr key={template.templateId} className="border-b border-[#eee] dark:border-strokedark">
                        <td className="px-4 py-3 text-black dark:text-white">{template.name}</td>
                        <td className="px-4 py-3 text-black dark:text-white">{template.usageCount}</td>
                        <td className="px-4 py-3 text-black dark:text-white">
                          {template.lastUsedAt ? formatDate(template.lastUsedAt) : "Never"}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </>
      )}
    </div>
  );
}

