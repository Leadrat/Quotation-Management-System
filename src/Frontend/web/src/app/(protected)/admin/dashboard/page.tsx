"use client";
import { useEffect, useState } from "react";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import { ReportsApi } from "@/lib/api";
import type { AdminDashboardMetrics } from "@/types/reports";
import dynamic from "next/dynamic";

// Dynamically import chart components to avoid SSR issues
const QuotationTrendChart = dynamic(() => import("@/components/reports/QuotationTrendChart"), { ssr: false });

export default function AdminDashboardPage() {
  const [metrics, setMetrics] = useState<AdminDashboardMetrics | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadMetrics();
  }, []);

  const loadMetrics = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await ReportsApi.getAdminDashboard();
      if (response.success && response.data) {
        setMetrics(response.data);
      } else {
        setError("Failed to load dashboard metrics");
      }
    } catch (err: any) {
      console.error("Error loading admin dashboard:", err);
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

  const getHealthStatus = (uptime: number) => {
    if (uptime >= 99.9) return { color: "text-green-600 dark:text-green-400", label: "Excellent" };
    if (uptime >= 99.0) return { color: "text-yellow-600 dark:text-yellow-400", label: "Good" };
    return { color: "text-red-600 dark:text-red-400", label: "Needs Attention" };
  };

  return (
    <>
      <PageBreadcrumb pageTitle="Admin Dashboard" />
      
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
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4 mb-6">
            <div className="rounded-2xl border border-gray-200 bg-white p-5 dark:border-gray-800 dark:bg-white/[0.03]">
              <div className="text-sm text-gray-500 dark:text-gray-400">Active Users</div>
              <div className="mt-2 text-2xl font-bold text-gray-800 dark:text-white/90">
                {metrics.activeUsers}
              </div>
              <div className="text-xs text-gray-500 dark:text-gray-400 mt-1">Total active</div>
            </div>

            <div className="rounded-2xl border border-gray-200 bg-white p-5 dark:border-gray-800 dark:bg-white/[0.03]">
              <div className="text-sm text-gray-500 dark:text-gray-400">Sales Reps</div>
              <div className="mt-2 text-2xl font-bold text-gray-800 dark:text-white/90">
                {metrics.activeSalesReps}
              </div>
              <div className="text-xs text-gray-500 dark:text-gray-400 mt-1">Active sales reps</div>
            </div>

            <div className="rounded-2xl border border-gray-200 bg-white p-5 dark:border-gray-800 dark:bg-white/[0.03]">
              <div className="text-sm text-gray-500 dark:text-gray-400">Managers</div>
              <div className="mt-2 text-2xl font-bold text-gray-800 dark:text-white/90">
                {metrics.activeManagers}
              </div>
              <div className="text-xs text-gray-500 dark:text-gray-400 mt-1">Active managers</div>
            </div>

            <div className="rounded-2xl border border-gray-200 bg-white p-5 dark:border-gray-800 dark:bg-white/[0.03]">
              <div className="text-sm text-gray-500 dark:text-gray-400">Total Clients</div>
              <div className="mt-2 text-2xl font-bold text-gray-800 dark:text-white/90">
                {metrics.totalClientsLifetime?.toLocaleString() || 0}
              </div>
              <div className="text-xs text-gray-500 dark:text-gray-400 mt-1">Till date</div>
            </div>

            <div className="rounded-2xl border border-gray-200 bg-white p-5 dark:border-gray-800 dark:bg-white/[0.03]">
              <div className="text-sm text-gray-500 dark:text-gray-400">Total Quotations</div>
              <div className="mt-2 text-2xl font-bold text-gray-800 dark:text-white/90">
                {metrics.totalQuotationsLifetime.toLocaleString()}
              </div>
              <div className="text-xs text-gray-500 dark:text-gray-400 mt-1">Till date</div>
            </div>
          </div>

          {/* Revenue & System Health */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">
            <div className="rounded-2xl border border-gray-200 bg-white p-5 dark:border-gray-800 dark:bg-white/[0.03]">
              <div className="text-sm text-gray-500 dark:text-gray-400 mb-2">Total Revenue</div>
              <div className="text-3xl font-bold text-gray-800 dark:text-white/90">
                {formatCurrency(metrics.totalRevenue)}
              </div>
              <div className="text-xs text-gray-500 dark:text-gray-400 mt-1">All accepted & paid</div>
            </div>

            <div className="rounded-2xl border border-gray-200 bg-white p-5 dark:border-gray-800 dark:bg-white/[0.03]">
              <div className="text-sm text-gray-500 dark:text-gray-400 mb-2">System Health</div>
              <div className="flex items-center gap-4 mt-2">
                <div className="flex-1">
                  <div className="text-lg font-semibold text-gray-800 dark:text-white/90">
                    {formatPercent(metrics.systemHealth.apiUptime)} Uptime
                  </div>
                  <div className="text-xs text-gray-500 dark:text-gray-400 mt-1">API availability</div>
                </div>
                <div className={`text-sm font-medium ${getHealthStatus(metrics.systemHealth.apiUptime).color}`}>
                  {getHealthStatus(metrics.systemHealth.apiUptime).label}
                </div>
              </div>
            </div>
          </div>

          {/* System Metrics */}
          <ComponentCard title="System Metrics" className="mb-6">
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
              <div className="p-4 bg-gray-50 rounded-lg dark:bg-gray-800">
                <div className="text-sm text-gray-500 dark:text-gray-400">Error Count</div>
                <div className="text-2xl font-bold text-gray-900 dark:text-white mt-1">
                  {metrics.systemHealth.errorCount}
                </div>
                <div className="text-xs text-gray-500 dark:text-gray-400 mt-1">Recent errors</div>
              </div>
              <div className="p-4 bg-gray-50 rounded-lg dark:bg-gray-800">
                <div className="text-sm text-gray-500 dark:text-gray-400">API Uptime</div>
                <div className="text-2xl font-bold text-gray-900 dark:text-white mt-1">
                  {formatPercent(metrics.systemHealth.apiUptime)}
                </div>
                <div className="text-xs text-gray-500 dark:text-gray-400 mt-1">Availability</div>
              </div>
              <div className="p-4 bg-gray-50 rounded-lg dark:bg-gray-800">
                <div className="text-sm text-gray-500 dark:text-gray-400">Database Size</div>
                <div className="text-2xl font-bold text-gray-900 dark:text-white mt-1">
                  {metrics.systemHealth.databaseSizeMB.toFixed(0)} MB
                </div>
                <div className="text-xs text-gray-500 dark:text-gray-400 mt-1">Total size</div>
              </div>
              <div className="p-4 bg-gray-50 rounded-lg dark:bg-gray-800">
                <div className="text-sm text-gray-500 dark:text-gray-400">Avg Response Time</div>
                <div className="text-2xl font-bold text-gray-900 dark:text-white mt-1">
                  {metrics.systemHealth.averageResponseTimeMs.toFixed(0)} ms
                </div>
                <div className="text-xs text-gray-500 dark:text-gray-400 mt-1">API response</div>
              </div>
            </div>
          </ComponentCard>

          {/* Charts Row */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">
            {/* Growth Chart */}
            <ComponentCard title="Growth Chart">
              {metrics.growthChart && metrics.growthChart.length > 0 ? (
                <div className="h-64">
                  <QuotationTrendChart
                    data={metrics.growthChart.map((item) => ({
                      date: item.period,
                      created: item.quotations,
                      sent: item.revenue / 1000, // Scale revenue for visibility
                    }))}
                  />
                </div>
              ) : (
                <p className="text-gray-500 dark:text-gray-400 text-center py-8">No data available</p>
              )}
            </ComponentCard>

            {/* Usage Chart */}
            <ComponentCard title="Daily Active Users">
              {metrics.usageChart && metrics.usageChart.length > 0 ? (
                <div className="h-64">
                  <QuotationTrendChart
                    data={metrics.usageChart.map((item) => ({
                      date: item.date,
                      created: item.dailyActiveUsers,
                      sent: 0,
                    }))}
                  />
                </div>
              ) : (
                <p className="text-gray-500 dark:text-gray-400 text-center py-8">No data available</p>
              )}
            </ComponentCard>
          </div>

          {/* Summary */}
          <ComponentCard title="System Summary">
            <div className="space-y-3">
              <div className="flex justify-between items-center p-3 bg-gray-50 rounded-lg dark:bg-gray-800">
                <span className="text-sm font-medium text-gray-700 dark:text-gray-300">Total Active Users</span>
                <span className="text-lg font-bold text-gray-900 dark:text-white">{metrics.activeUsers}</span>
              </div>
              <div className="flex justify-between items-center p-3 bg-gray-50 rounded-lg dark:bg-gray-800">
                <span className="text-sm font-medium text-gray-700 dark:text-gray-300">Total Quotations (Lifetime)</span>
                <span className="text-lg font-bold text-gray-900 dark:text-white">
                  {metrics.totalQuotationsLifetime.toLocaleString()}
                </span>
              </div>
              <div className="flex justify-between items-center p-3 bg-gray-50 rounded-lg dark:bg-gray-800">
                <span className="text-sm font-medium text-gray-700 dark:text-gray-300">Total Revenue</span>
                <span className="text-lg font-bold text-gray-900 dark:text-white">
                  {formatCurrency(metrics.totalRevenue)}
                </span>
              </div>
              <div className="flex justify-between items-center p-3 bg-gray-50 rounded-lg dark:bg-gray-800">
                <span className="text-sm font-medium text-gray-700 dark:text-gray-300">System Health</span>
                <span className={`text-lg font-bold ${getHealthStatus(metrics.systemHealth.apiUptime).color}`}>
                  {getHealthStatus(metrics.systemHealth.apiUptime).label}
                </span>
              </div>
            </div>
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

