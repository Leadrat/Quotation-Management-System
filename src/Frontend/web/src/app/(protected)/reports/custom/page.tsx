"use client";
import { useState } from "react";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import { useReport } from "@/hooks/useReport";
import { DateRangePicker } from "@/components/reports/ui";
import { ExportButton } from "@/components/reports/ui";
import { LineChart, BarChart, PieChart } from "@/components/reports/charts";
import type { ReportGenerationRequest } from "@/types/reports";

export default function CustomReportBuilderPage() {
  const [reportType, setReportType] = useState("quotations");
  const [fromDate, setFromDate] = useState("");
  const [toDate, setToDate] = useState("");
  const [selectedMetrics, setSelectedMetrics] = useState<string[]>([]);
  const [groupBy, setGroupBy] = useState("date");
  const [sortBy, setSortBy] = useState("date");
  const [reportId, setReportId] = useState<string | null>(null);

  const { data, loading, error, loadReport } = useReport();

  const availableMetrics = [
    { id: "quotationsCreated", label: "Quotations Created" },
    { id: "quotationsSent", label: "Quotations Sent" },
    { id: "quotationsAccepted", label: "Quotations Accepted" },
    { id: "totalValue", label: "Total Value" },
    { id: "conversionRate", label: "Conversion Rate" },
    { id: "averageDiscount", label: "Average Discount" },
  ];

  const handleRunReport = async () => {
    const request: ReportGenerationRequest = {
      reportType: "custom",
      filters: {
        fromDate,
        toDate,
        metrics: selectedMetrics,
      },
      groupBy,
      sortBy,
      format: "json",
    };

    const result = await loadReport(request);
    if (result) {
      setReportId(result.reportType + "-" + Date.now());
    }
  };

  return (
    <>
      <PageBreadcrumb pageTitle="Custom Report Builder" />
      
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Filters Panel */}
        <div className="lg:col-span-1">
          <ComponentCard title="Report Configuration">
            <div className="space-y-6">
              {/* Report Type */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Report Type
                </label>
                <select
                  value={reportType}
                  onChange={(e) => setReportType(e.target.value)}
                  className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-gray-700 focus:border-blue-500 focus:outline-none dark:border-gray-600 dark:bg-gray-800 dark:text-gray-300"
                >
                  <option value="quotations">Quotations</option>
                  <option value="payments">Payments</option>
                  <option value="approvals">Approvals</option>
                  <option value="clients">Clients</option>
                </select>
              </div>

              {/* Date Range */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Date Range
                </label>
                <DateRangePicker
                  fromDate={fromDate}
                  toDate={toDate}
                  onChange={(from, to) => {
                    setFromDate(from);
                    setToDate(to);
                  }}
                />
              </div>

              {/* Metrics Selection */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Select Metrics
                </label>
                <div className="space-y-2">
                  {availableMetrics.map((metric) => (
                    <label key={metric.id} className="flex items-center">
                      <input
                        type="checkbox"
                        checked={selectedMetrics.includes(metric.id)}
                        onChange={(e) => {
                          if (e.target.checked) {
                            setSelectedMetrics([...selectedMetrics, metric.id]);
                          } else {
                            setSelectedMetrics(selectedMetrics.filter((m) => m !== metric.id));
                          }
                        }}
                        className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                      />
                      <span className="ml-2 text-sm text-gray-700 dark:text-gray-300">{metric.label}</span>
                    </label>
                  ))}
                </div>
              </div>

              {/* Group By */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Group By
                </label>
                <select
                  value={groupBy}
                  onChange={(e) => setGroupBy(e.target.value)}
                  className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-gray-700 focus:border-blue-500 focus:outline-none dark:border-gray-600 dark:bg-gray-800 dark:text-gray-300"
                >
                  <option value="date">Date</option>
                  <option value="user">User</option>
                  <option value="team">Team</option>
                  <option value="client">Client</option>
                  <option value="status">Status</option>
                </select>
              </div>

              {/* Sort By */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Sort By
                </label>
                <select
                  value={sortBy}
                  onChange={(e) => setSortBy(e.target.value)}
                  className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-gray-700 focus:border-blue-500 focus:outline-none dark:border-gray-600 dark:bg-gray-800 dark:text-gray-300"
                >
                  <option value="date">Date</option>
                  <option value="value">Value</option>
                  <option value="count">Count</option>
                </select>
              </div>

              {/* Run Report Button */}
              <button
                onClick={handleRunReport}
                disabled={loading || selectedMetrics.length === 0}
                className="w-full px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed dark:bg-blue-500 dark:hover:bg-blue-600"
              >
                {loading ? "Generating..." : "Run Report"}
              </button>
            </div>
          </ComponentCard>
        </div>

        {/* Report Preview */}
        <div className="lg:col-span-2">
          {error && (
            <div className="mb-6 rounded-lg bg-red-50 border border-red-200 p-4 dark:bg-red-900/20 dark:border-red-800">
              <p className="text-sm text-red-800 dark:text-red-400">{error}</p>
            </div>
          )}

          {loading ? (
            <ComponentCard title="Report Preview">
              <div className="text-center py-12">
                <p className="text-gray-500 dark:text-gray-400">Generating report...</p>
              </div>
            </ComponentCard>
          ) : data ? (
            <div className="space-y-6">
              <ComponentCard title={data.title || "Report Results"}>
                <div className="flex justify-between items-center mb-4">
                  <p className="text-sm text-gray-600 dark:text-gray-400">{data.summary}</p>
                  {reportId && <ExportButton reportId={reportId} reportName={data.title} />}
                </div>

                {/* KPI Cards */}
                {data.metrics && data.metrics.length > 0 && (
                  <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
                    {data.metrics.map((metric, idx) => (
                      <div key={idx} className="p-4 bg-gray-50 rounded-lg dark:bg-gray-800">
                        <p className="text-sm text-gray-500 dark:text-gray-400">{metric.name}</p>
                        <p className="text-2xl font-bold text-gray-900 dark:text-white mt-1">
                          {metric.value} {metric.unit}
                        </p>
                      </div>
                    ))}
                  </div>
                )}

                {/* Charts */}
                {data.charts && data.charts.length > 0 && (
                  <div className="space-y-6">
                    {data.charts.map((chart, idx) => {
                      if (chart.chartType === "line") {
                        return <LineChart key={idx} data={chart} title={chart.title} />;
                      } else if (chart.chartType === "bar") {
                        return <BarChart key={idx} data={chart} title={chart.title} />;
                      } else if (chart.chartType === "pie") {
                        return <PieChart key={idx} data={chart} title={chart.title} />;
                      }
                      return null;
                    })}
                  </div>
                )}

                {/* Data Table */}
                {data.details && data.details.length > 0 && (
                  <div className="mt-6 overflow-x-auto">
                    <table className="w-full">
                      <thead>
                        <tr className="border-b border-gray-200 dark:border-gray-700">
                          {Object.keys(data.details[0]).map((key) => (
                            <th
                              key={key}
                              className="text-left py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300"
                            >
                              {key}
                            </th>
                          ))}
                        </tr>
                      </thead>
                      <tbody>
                        {data.details.slice(0, 20).map((row, idx) => (
                          <tr key={idx} className="border-b border-gray-100 dark:border-gray-800">
                            {Object.values(row).map((value, vIdx) => (
                              <td key={vIdx} className="py-3 px-4 text-sm text-gray-700 dark:text-gray-300">
                                {String(value)}
                              </td>
                            ))}
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                )}
              </ComponentCard>
            </div>
          ) : (
            <ComponentCard title="Report Preview">
              <div className="text-center py-12">
                <p className="text-gray-500 dark:text-gray-400">
                  Configure your report settings and click "Run Report" to generate
                </p>
              </div>
            </ComponentCard>
          )}
        </div>
      </div>
    </>
  );
}

