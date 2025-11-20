"use client";
import { useEffect, useState } from "react";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import { ReportsApi } from "@/lib/api";
import type { FinanceDashboardMetrics } from "@/types/reports";
import dynamic from "next/dynamic";

// Dynamically import chart components to avoid SSR issues
const QuotationTrendChart = dynamic(() => import("@/components/reports/QuotationTrendChart"), { ssr: false });

export default function FinanceDashboardPage() {
  const [metrics, setMetrics] = useState<FinanceDashboardMetrics | null>(null);
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
      const response = await ReportsApi.getFinanceDashboard(
        fromDate || undefined,
        toDate || undefined
      );
      if (response.success && response.data) {
        setMetrics(response.data);
      } else {
        setError("Failed to load dashboard metrics");
      }
    } catch (err: any) {
      console.error("Error loading finance dashboard:", err);
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

  const getStatusBadge = (status: string) => {
    const statusColors: Record<string, string> = {
      Success: "bg-green-100 text-green-800 dark:bg-green-900/20 dark:text-green-400",
      Failed: "bg-red-100 text-red-800 dark:bg-red-900/20 dark:text-red-400",
      Pending: "bg-yellow-100 text-yellow-800 dark:bg-yellow-900/20 dark:text-yellow-400",
      Processing: "bg-blue-100 text-blue-800 dark:bg-blue-900/20 dark:text-blue-400",
      Refunded: "bg-gray-100 text-gray-800 dark:bg-gray-900/20 dark:text-gray-400",
    };
    return statusColors[status] || "bg-gray-100 text-gray-800 dark:bg-gray-900/20 dark:text-gray-400";
  };

  return (
    <>
      <PageBreadcrumb pageTitle="Finance Dashboard" />
      
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
              <div className="text-sm text-gray-500 dark:text-gray-400">Payments Received</div>
              <div className="mt-2 text-2xl font-bold text-gray-800 dark:text-white/90">
                {formatCurrency(metrics.totalPaymentsReceivedThisMonth)}
              </div>
              <div className="text-xs text-gray-500 dark:text-gray-400 mt-1">This month</div>
            </div>

            <div className="rounded-2xl border border-gray-200 bg-white p-5 dark:border-gray-800 dark:bg-white/[0.03]">
              <div className="text-sm text-gray-500 dark:text-gray-400">Success Rate</div>
              <div className="mt-2 text-2xl font-bold text-gray-800 dark:text-white/90">
                {formatPercent(metrics.paymentSuccessRate)}
              </div>
              <div className="text-xs text-gray-500 dark:text-gray-400 mt-1">Payment success</div>
            </div>

            <div className="rounded-2xl border border-gray-200 bg-white p-5 dark:border-gray-800 dark:bg-white/[0.03]">
              <div className="text-sm text-gray-500 dark:text-gray-400">Failed Payments</div>
              <div className={`mt-2 text-2xl font-bold ${metrics.failedPaymentsCount > 0 ? "text-red-600 dark:text-red-400" : "text-gray-800 dark:text-white/90"}`}>
                {metrics.failedPaymentsCount}
              </div>
              <div className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                {metrics.failedPaymentsCount > 0 && metrics.failedPaymentsCount > 5 && (
                  <span className="text-red-600 dark:text-red-400">⚠️ Alert</span>
                )}
                {metrics.failedPaymentsCount <= 5 && "Count"}
              </div>
            </div>

            <div className="rounded-2xl border border-gray-200 bg-white p-5 dark:border-gray-800 dark:bg-white/[0.03]">
              <div className="text-sm text-gray-500 dark:text-gray-400">Total Refunds</div>
              <div className="mt-2 text-2xl font-bold text-gray-800 dark:text-white/90">
                {formatCurrency(metrics.totalRefunds)}
              </div>
              <div className="text-xs text-gray-500 dark:text-gray-400 mt-1">All time</div>
            </div>

            <div className="rounded-2xl border border-gray-200 bg-white p-5 dark:border-gray-800 dark:bg-white/[0.03]">
              <div className="text-sm text-gray-500 dark:text-gray-400">Collection %</div>
              <div className="mt-2 text-2xl font-bold text-gray-800 dark:text-white/90">
                {formatPercent(metrics.collectionPercent)}
              </div>
              <div className="text-xs text-gray-500 dark:text-gray-400 mt-1">Collection rate</div>
            </div>
          </div>

          {/* Charts Row */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">
            {/* Payment Trend */}
            <ComponentCard title="Payment Collection Trend">
              {metrics.paymentTrend && metrics.paymentTrend.length > 0 ? (
                <div className="h-64">
                  <QuotationTrendChart
                    data={metrics.paymentTrend.map((item) => ({
                      date: item.date,
                      created: item.amount,
                      sent: item.count,
                    }))}
                  />
                </div>
              ) : (
                <p className="text-gray-500 dark:text-gray-400 text-center py-8">No data available</p>
              )}
            </ComponentCard>

            {/* Payment Method Distribution */}
            <ComponentCard title="Payment Method Distribution">
              {metrics.paymentMethodDistribution && metrics.paymentMethodDistribution.length > 0 ? (
                <div className="space-y-4">
                  {metrics.paymentMethodDistribution.map((method) => (
                    <div key={method.paymentMethod} className="p-4 bg-gray-50 rounded-lg dark:bg-gray-800">
                      <div className="flex items-center justify-between mb-2">
                        <span className="font-medium text-gray-900 dark:text-white">{method.paymentMethod}</span>
                        <span className="text-sm text-gray-600 dark:text-gray-300">
                          {formatPercent(method.percentage)}
                        </span>
                      </div>
                      <div className="w-full bg-gray-200 rounded-full h-2 dark:bg-gray-700">
                        <div
                          className="bg-blue-600 h-2 rounded-full dark:bg-blue-500"
                          style={{ width: `${method.percentage}%` }}
                        ></div>
                      </div>
                      <div className="mt-2 flex justify-between text-xs text-gray-500 dark:text-gray-400">
                        <span>{method.count} payments</span>
                        <span>{formatCurrency(method.amount)}</span>
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <p className="text-gray-500 dark:text-gray-400 text-center py-8">No data available</p>
              )}
            </ComponentCard>
          </div>

          {/* Payment Funnel */}
          <ComponentCard title="Payment Funnel" className="mb-6">
            {metrics.paymentFunnel && metrics.paymentFunnel.length > 0 ? (
              <div className="space-y-3">
                {metrics.paymentFunnel.map((stage, idx) => {
                  const maxValue = Math.max(...metrics.paymentFunnel.map((s) => s.value));
                  const widthPercent = maxValue > 0 ? (stage.value / maxValue) * 100 : 0;
                  return (
                    <div key={stage.stage} className="flex items-center gap-4">
                      <div className="w-32 text-sm font-medium text-gray-700 dark:text-gray-300">{stage.stage}</div>
                      <div className="flex-1">
                        <div className="w-full bg-gray-200 rounded-full h-6 dark:bg-gray-700 relative">
                          <div
                            className="bg-blue-600 h-6 rounded-full dark:bg-blue-500 flex items-center justify-end pr-2"
                            style={{ width: `${widthPercent}%` }}
                          >
                            {widthPercent > 10 && (
                              <span className="text-xs text-white font-medium">
                                {formatCurrency(stage.value)}
                              </span>
                            )}
                          </div>
                        </div>
                      </div>
                      <div className="w-24 text-right text-sm text-gray-700 dark:text-gray-300">
                        {stage.count} items
                      </div>
                    </div>
                  );
                })}
              </div>
            ) : (
              <p className="text-gray-500 dark:text-gray-400 text-center py-8">No data available</p>
            )}
          </ComponentCard>

          {/* Payments List */}
          <ComponentCard title="Recent Payments">
            {metrics.payments && metrics.payments.length > 0 ? (
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
                      <th className="text-left py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                        Gateway
                      </th>
                      <th className="text-right py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                        Amount
                      </th>
                      <th className="text-center py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                        Status
                      </th>
                      <th className="text-left py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                        Date
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {metrics.payments.slice(0, 20).map((payment) => (
                      <tr key={payment.paymentId} className="border-b border-gray-100 dark:border-gray-800">
                        <td className="py-3 px-4 text-sm text-gray-900 dark:text-white">
                          {payment.quotationNumber}
                        </td>
                        <td className="py-3 px-4 text-sm text-gray-700 dark:text-gray-300">{payment.clientName}</td>
                        <td className="py-3 px-4 text-sm text-gray-700 dark:text-gray-300">
                          {payment.paymentGateway}
                        </td>
                        <td className="py-3 px-4 text-sm text-right text-gray-700 dark:text-gray-300">
                          {formatCurrency(payment.amount)} {payment.currency}
                        </td>
                        <td className="py-3 px-4 text-center">
                          <span
                            className={`inline-block px-2 py-1 text-xs font-medium rounded-full ${getStatusBadge(
                              payment.status
                            )}`}
                          >
                            {payment.status}
                          </span>
                        </td>
                        <td className="py-3 px-4 text-sm text-gray-700 dark:text-gray-300">
                          {payment.paymentDate
                            ? new Date(payment.paymentDate).toLocaleDateString()
                            : new Date(payment.createdAt).toLocaleDateString()}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : (
              <p className="text-gray-500 dark:text-gray-400 text-center py-8">No payments data available</p>
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

