"use client";
import { useState, useEffect } from "react";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import { ReportsApi } from "@/lib/api";
import { DateRangePicker } from "@/components/reports/ui";
import { ExportButton } from "@/components/reports/ui";
import { LineChart, BarChart } from "@/components/reports/charts";
import type { SalesDashboardMetrics } from "@/types/reports";

export default function SalesPipelineReportPage() {
  const [metrics, setMetrics] = useState<SalesDashboardMetrics | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [fromDate, setFromDate] = useState("");
  const [toDate, setToDate] = useState("");

  useEffect(() => {
    loadMetrics();
  }, [fromDate, toDate]);

  const loadMetrics = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await ReportsApi.getSalesDashboard(
        fromDate || undefined,
        toDate || undefined
      );
      if (response.success && response.data) {
        setMetrics(response.data);
      }
    } catch (err: any) {
      console.error("Error loading sales pipeline report:", err);
      setError(err.message || "Failed to load report");
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <PageBreadcrumb pageTitle="Sales Pipeline Report" />
      
      <div className="mb-6">
        <DateRangePicker
          fromDate={fromDate}
          toDate={toDate}
          onChange={(from, to) => {
            setFromDate(from);
            setToDate(to);
          }}
        />
      </div>

      {error && (
        <div className="mb-6 rounded-lg bg-red-50 border border-red-200 p-4 dark:bg-red-900/20 dark:border-red-800">
          <p className="text-sm text-red-800 dark:text-red-400">{error}</p>
        </div>
      )}

      {loading ? (
        <ComponentCard title="Sales Pipeline Report">
          <div className="text-center py-12">
            <p className="text-gray-500 dark:text-gray-400">Loading report...</p>
          </div>
        </ComponentCard>
      ) : metrics ? (
        <div className="space-y-6">
          <ComponentCard title="Sales Pipeline Overview">
            <div className="flex justify-end mb-4">
              <ExportButton reportId="sales-pipeline" reportName="Sales Pipeline Report" />
            </div>
            <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
              <div className="p-4 bg-gray-50 rounded-lg dark:bg-gray-800">
                <p className="text-sm text-gray-500 dark:text-gray-400">Quotations Created</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white mt-1">
                  {metrics.quotationsCreatedThisMonth}
                </p>
              </div>
              <div className="p-4 bg-gray-50 rounded-lg dark:bg-gray-800">
                <p className="text-sm text-gray-500 dark:text-gray-400">Pipeline Value</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white mt-1">
                  ₹{metrics.totalPipelineValue.toLocaleString()}
                </p>
              </div>
              <div className="p-4 bg-gray-50 rounded-lg dark:bg-gray-800">
                <p className="text-sm text-gray-500 dark:text-gray-400">Conversion Rate</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white mt-1">
                  {metrics.conversionRate.toFixed(1)}%
                </p>
              </div>
              <div className="p-4 bg-gray-50 rounded-lg dark:bg-gray-800">
                <p className="text-sm text-gray-500 dark:text-gray-400">Pending Approvals</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white mt-1">
                  {metrics.pendingApprovals}
                </p>
              </div>
            </div>

            {metrics.quotationTrend && metrics.quotationTrend.length > 0 && (
              <LineChart
                data={{
                  chartType: "line",
                  title: "Quotation Trend",
                  series: [
                    { name: "Created", data: metrics.quotationTrend.map((t) => t.created) },
                    { name: "Sent", data: metrics.quotationTrend.map((t) => t.sent) },
                  ],
                  categories: metrics.quotationTrend.map((t) => t.date),
                }}
                title="Quotation Trend"
              />
            )}

            {metrics.statusBreakdown && metrics.statusBreakdown.length > 0 && (
              <div className="mt-6">
                <BarChart
                  data={{
                    chartType: "bar",
                    title: "Status Breakdown",
                    series: [
                      {
                        name: "Count",
                        data: metrics.statusBreakdown.map((s) => s.count),
                      },
                    ],
                    categories: metrics.statusBreakdown.map((s) => s.status),
                  }}
                  title="Status Breakdown"
                />
              </div>
            )}
          </ComponentCard>
        </div>
      ) : (
        <ComponentCard title="Sales Pipeline Report">
          <div className="text-center py-12">
            <p className="text-gray-500 dark:text-gray-400">No data available</p>
          </div>
        </ComponentCard>
      )}
    </>
  );
}

