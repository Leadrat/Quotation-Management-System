"use client";
import { useState, useEffect } from "react";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import { ReportsApi } from "@/lib/api";
import { DateRangePicker } from "@/components/reports/ui";
import { ExportButton } from "@/components/reports/ui";
import { BarChart, PieChart } from "@/components/reports/charts";
import type { ApprovalMetrics } from "@/types/reports";

export default function ApprovalMetricsReportPage() {
  const [data, setData] = useState<ApprovalMetrics | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [fromDate, setFromDate] = useState("");
  const [toDate, setToDate] = useState("");

  useEffect(() => {
    loadData();
  }, [fromDate, toDate]);

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await ReportsApi.getApprovalMetrics(
        undefined,
        fromDate || undefined,
        toDate || undefined
      );
      if (response.success && response.data) {
        setData(response.data);
      }
    } catch (err: any) {
      console.error("Error loading approval metrics report:", err);
      setError(err.message || "Failed to load report");
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <PageBreadcrumb pageTitle="Approval Metrics Report" />
      
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
        <ComponentCard title="Approval Metrics Report">
          <div className="text-center py-12">
            <p className="text-gray-500 dark:text-gray-400">Loading report...</p>
          </div>
        </ComponentCard>
      ) : data ? (
        <div className="space-y-6">
          <ComponentCard title="Approval Metrics Overview">
            <div className="flex justify-end mb-4">
              <ExportButton reportId="approval-metrics" reportName="Approval Metrics Report" />
            </div>
            <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
              <div className="p-4 bg-gray-50 rounded-lg dark:bg-gray-800">
                <p className="text-sm text-gray-500 dark:text-gray-400">Avg Approval TAT</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white mt-1">
                  {data.averageApprovalTAT.toFixed(1)} hrs
                </p>
              </div>
              <div className="p-4 bg-gray-50 rounded-lg dark:bg-gray-800">
                <p className="text-sm text-gray-500 dark:text-gray-400">Rejection Rate</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white mt-1">
                  {data.rejectionRate.toFixed(1)}%
                </p>
              </div>
              <div className="p-4 bg-gray-50 rounded-lg dark:bg-gray-800">
                <p className="text-sm text-gray-500 dark:text-gray-400">Escalation %</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white mt-1">
                  {data.escalationPercent.toFixed(1)}%
                </p>
              </div>
              <div className="p-4 bg-gray-50 rounded-lg dark:bg-gray-800">
                <p className="text-sm text-gray-500 dark:text-gray-400">Pending Approvals</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white mt-1">
                  {data.pendingApprovals}
                </p>
              </div>
            </div>

            {data.approvalStatusBreakdown && data.approvalStatusBreakdown.length > 0 && (
              <PieChart
                data={{
                  chartType: "pie",
                  title: "Approval Status Breakdown",
                  series: [
                    {
                      name: "Count",
                      data: data.approvalStatusBreakdown.map((s) => s.count),
                    },
                  ],
                  categories: data.approvalStatusBreakdown.map((s) => s.status),
                }}
                title="Approval Status Breakdown"
              />
            )}
          </ComponentCard>
        </div>
      ) : (
        <ComponentCard title="Approval Metrics Report">
          <div className="text-center py-12">
            <p className="text-gray-500 dark:text-gray-400">No data available</p>
          </div>
        </ComponentCard>
      )}
    </>
  );
}

