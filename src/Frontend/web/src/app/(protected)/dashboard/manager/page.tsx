"use client";
import { useEffect, useState } from "react";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import { ReportsApi } from "@/lib/api";
import type { ManagerDashboardMetrics } from "@/types/reports";
import dynamic from "next/dynamic";

// Dynamically import chart components to avoid SSR issues
const QuotationTrendChart = dynamic(() => import("@/components/reports/QuotationTrendChart"), { ssr: false });

export default function ManagerDashboardPage() {
  const [metrics, setMetrics] = useState<ManagerDashboardMetrics | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [fromDate, setFromDate] = useState<string>("");
  const [toDate, setToDate] = useState<string>("");

  useEffect(() => {
    loadMetrics();
  }, [fromDate, toDate]);

  const loadMetrics = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await ReportsApi.getManagerDashboard(
        undefined,
        fromDate || undefined,
        toDate || undefined
      );
      if (response.success && response.data) {
        setMetrics(response.data);
      } else {
        setError("Failed to load dashboard metrics");
      }
    } catch (err: any) {
      console.error("Error loading manager dashboard:", err);
      setError(err.message || "Failed to load dashboard metrics");
    } finally {
      setLoading(false);
    }
  };

  const formatCurrency = (amount: number) => {
    return `₹${amount.toLocaleString("en-IN", { maximumFractionDigits: 0 })}`;
  };

  const formatPercent = (value: number) => {
    return `${value.toFixed(1)}%`;
  };

  const getStatusColor = (status: "green" | "yellow" | "red") => {
    switch (status) {
      case "green":
        return "bg-green-100 text-green-800 dark:bg-green-900/20 dark:text-green-400";
      case "yellow":
        return "bg-yellow-100 text-yellow-800 dark:bg-yellow-900/20 dark:text-yellow-400";
      case "red":
        return "bg-red-100 text-red-800 dark:bg-red-900/20 dark:text-red-400";
    }
  };

  return (
    <>
      <PageBreadcrumb pageTitle="Manager Dashboard" />
      
      {/* Date Range Filter */}
      <div className="mb-6 flex gap-4 items-end">
        <div className="flex-1">
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            From Date
          </label>
          <input
            type="date"
            value={fromDate}
            onChange={(e) => setFromDate(e.target.value)}
            className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-gray-700 focus:border-blue-500 focus:outline-none dark:border-gray-600 dark:bg-gray-800 dark:text-gray-300"
          />
        </div>
        <div className="flex-1">
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            To Date
          </label>
          <input
            type="date"
            value={toDate}
            onChange={(e) => setToDate(e.target.value)}
            className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-gray-700 focus:border-blue-500 focus:outline-none dark:border-gray-600 dark:bg-gray-800 dark:text-gray-300"
          />
        </div>
        <button
          onClick={() => {
            setFromDate("");
            setToDate("");
          }}
          className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 dark:bg-gray-800 dark:text-gray-300 dark:border-gray-600 dark:hover:bg-gray-700"
        >
          Clear
        </button>
      </div>

      {error && (
        <div className="mb-6 rounded-lg bg-red-50 border border-red-200 p-4 dark:bg-red-900/20 dark:border-red-800">
          <p className="text-sm text-red-800 dark:text-red-400">{error}</p>
        </div>
      )}

      {loading ? (
        <div className="text-center py-12">
          <p className="text-gray-500 dark:text-gray-400">Loading dashboard metrics...</p>
        </div>
      ) : metrics ? (
        <>
          {/* KPI Cards */}
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-5 mb-6">
            <div className="rounded-2xl border border-gray-200 bg-white p-5 dark:border-gray-800 dark:bg-white/[0.03]">
              <div className="text-sm text-gray-500 dark:text-gray-400">Team Quotations</div>
              <div className="mt-2 text-2xl font-bold text-gray-800 dark:text-white/90">
                {metrics.teamQuotationsThisMonth}
              </div>
              <div className="text-xs text-gray-500 dark:text-gray-400 mt-1">This month</div>
            </div>

            <div className="rounded-2xl border border-gray-200 bg-white p-5 dark:border-gray-800 dark:bg-white/[0.03]">
              <div className="text-sm text-gray-500 dark:text-gray-400">Conversion Rate</div>
              <div className="mt-2 text-2xl font-bold text-gray-800 dark:text-white/90">
                {formatPercent(metrics.teamConversionRate)}
              </div>
              <div className="text-xs text-gray-500 dark:text-gray-400 mt-1">Team average</div>
            </div>

            <div className="rounded-2xl border border-gray-200 bg-white p-5 dark:border-gray-800 dark:bg-white/[0.03]">
              <div className="text-sm text-gray-500 dark:text-gray-400">Avg Discount</div>
              <div className="mt-2 text-2xl font-bold text-gray-800 dark:text-white/90">
                {formatPercent(metrics.averageDiscountPercent)}
              </div>
              <div className="text-xs text-gray-500 dark:text-gray-400 mt-1">Team average</div>
            </div>

            <div className="rounded-2xl border border-gray-200 bg-white p-5 dark:border-gray-800 dark:bg-white/[0.03]">
              <div className="text-sm text-gray-500 dark:text-gray-400">Pending Approvals</div>
              <div className="mt-2 text-2xl font-bold text-gray-800 dark:text-white/90">
                {metrics.pendingApprovals}
              </div>
              <div className="text-xs text-gray-500 dark:text-gray-400 mt-1">In your queue</div>
            </div>

            <div className="rounded-2xl border border-gray-200 bg-white p-5 dark:border-gray-800 dark:bg-white/[0.03]">
              <div className="text-sm text-gray-500 dark:text-gray-400">Value at Risk</div>
              <div className="mt-2 text-2xl font-bold text-gray-800 dark:text-white/90">
                {formatCurrency(metrics.totalValueAtRisk)}
              </div>
              <div className="text-xs text-gray-500 dark:text-gray-400 mt-1">Pending approvals</div>
            </div>
          </div>

          {/* Charts Row */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">
            {/* Team Quota vs Actual */}
            <ComponentCard title="Team Quota vs Actual">
              {metrics.teamQuotaVsActual && metrics.teamQuotaVsActual.length > 0 ? (
                <div className="h-64">
                  <QuotationTrendChart
                    data={metrics.teamQuotaVsActual.map((item) => ({
                      date: item.period,
                      created: item.actual,
                      sent: item.quota,
                    }))}
                  />
                </div>
              ) : (
                <p className="text-gray-500 dark:text-gray-400 text-center py-8">No data available</p>
              )}
            </ComponentCard>

            {/* Rep Performance */}
            <ComponentCard title="Rep Performance">
              {metrics.repPerformance && metrics.repPerformance.length > 0 ? (
                <div className="space-y-4">
                  {metrics.repPerformance.slice(0, 5).map((rep, idx) => (
                    <div key={rep.userId} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg dark:bg-gray-800">
                      <div className="flex items-center gap-3">
                        <div className="w-8 h-8 rounded-full bg-blue-100 dark:bg-blue-900/20 flex items-center justify-center text-sm font-semibold text-blue-600 dark:text-blue-400">
                          {idx + 1}
                        </div>
                        <div>
                          <div className="font-medium text-gray-900 dark:text-white">{rep.userName}</div>
                          <div className="text-xs text-gray-500 dark:text-gray-400">
                            {rep.quotationsCreated} quotations
                          </div>
                        </div>
                      </div>
                      <div className="text-right">
                        <div className="font-semibold text-gray-900 dark:text-white">
                          {formatPercent(rep.conversionRate)}
                        </div>
                        <div className="text-xs text-gray-500 dark:text-gray-400">Conversion</div>
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <p className="text-gray-500 dark:text-gray-400 text-center py-8">No data available</p>
              )}
            </ComponentCard>
          </div>

          {/* Pipeline Stages */}
          <ComponentCard title="Pipeline Stages" className="mb-6">
            {metrics.pipelineStages && metrics.pipelineStages.length > 0 ? (
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                {metrics.pipelineStages.map((stage) => (
                  <div key={stage.stage} className="text-center p-4 bg-gray-50 rounded-lg dark:bg-gray-800">
                    <div className="text-2xl font-bold text-gray-900 dark:text-white">{stage.count}</div>
                    <div className="text-sm text-gray-500 dark:text-gray-400 mt-1">{stage.stage}</div>
                    <div className="text-xs text-gray-600 dark:text-gray-300 mt-1">
                      {formatCurrency(stage.value)}
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-gray-500 dark:text-gray-400 text-center py-8">No data available</p>
            )}
          </ComponentCard>

          {/* Team Members */}
          <ComponentCard title="Team Members" className="mb-6">
            {metrics.teamMembers && metrics.teamMembers.length > 0 ? (
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead>
                    <tr className="border-b border-gray-200 dark:border-gray-700">
                      <th className="text-left py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                        Name
                      </th>
                      <th className="text-right py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                        Quotations
                      </th>
                      <th className="text-right py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                        Pipeline Value
                      </th>
                      <th className="text-right py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                        Conversion
                      </th>
                      <th className="text-right py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                        Pending Approvals
                      </th>
                      <th className="text-center py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                        Status
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {metrics.teamMembers.map((member) => (
                      <tr key={member.userId} className="border-b border-gray-100 dark:border-gray-800">
                        <td className="py-3 px-4 text-sm text-gray-900 dark:text-white">{member.userName}</td>
                        <td className="py-3 px-4 text-sm text-right text-gray-700 dark:text-gray-300">
                          {member.quotationsCreated}
                        </td>
                        <td className="py-3 px-4 text-sm text-right text-gray-700 dark:text-gray-300">
                          {formatCurrency(member.pipelineValue)}
                        </td>
                        <td className="py-3 px-4 text-sm text-right text-gray-700 dark:text-gray-300">
                          {formatPercent(member.conversionRate)}
                        </td>
                        <td className="py-3 px-4 text-sm text-right text-gray-700 dark:text-gray-300">
                          {member.pendingApprovals}
                        </td>
                        <td className="py-3 px-4 text-center">
                          <span
                            className={`inline-block px-2 py-1 text-xs font-medium rounded-full ${getStatusColor(
                              member.status
                            )}`}
                          >
                            {member.status}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : (
              <p className="text-gray-500 dark:text-gray-400 text-center py-8">No team members data available</p>
            )}
          </ComponentCard>

          {/* Pending Approvals */}
          <ComponentCard title="Pending Approvals Queue">
            {metrics.pendingApprovalsList && metrics.pendingApprovalsList.length > 0 ? (
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead>
                    <tr className="border-b border-gray-200 dark:border-gray-700">
                      <th className="text-left py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                        Quotation
                      </th>
                      <th className="text-left py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                        Client
                      </th>
                      <th className="text-right py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                        Discount Amount
                      </th>
                      <th className="text-right py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                        Discount %
                      </th>
                      <th className="text-left py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                        Requested At
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {metrics.pendingApprovalsList.map((approval) => (
                      <tr key={approval.approvalId} className="border-b border-gray-100 dark:border-gray-800">
                        <td className="py-3 px-4 text-sm text-gray-900 dark:text-white">
                          {approval.quotationNumber}
                        </td>
                        <td className="py-3 px-4 text-sm text-gray-700 dark:text-gray-300">{approval.clientName}</td>
                        <td className="py-3 px-4 text-sm text-right text-gray-700 dark:text-gray-300">
                          {formatCurrency(approval.discountAmount)}
                        </td>
                        <td className="py-3 px-4 text-sm text-right text-gray-700 dark:text-gray-300">
                          {formatPercent(approval.discountPercent)}
                        </td>
                        <td className="py-3 px-4 text-sm text-gray-700 dark:text-gray-300">
                          {new Date(approval.requestedAt).toLocaleDateString()}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : (
              <p className="text-gray-500 dark:text-gray-400 text-center py-8">No pending approvals</p>
            )}
          </ComponentCard>
        </>
      ) : (
        <div className="text-center py-12">
          <p className="text-gray-500 dark:text-gray-400">No data available</p>
        </div>
      )}
    </>
  );
}

