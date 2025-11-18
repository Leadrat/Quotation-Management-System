"use client";

import { useEffect, useState } from "react";
import { ReportsApi } from "@/lib/api";
import type { SalesDashboardMetrics } from "@/types/reports";
import { SalesDashboardCards } from "@/components/reports/SalesDashboardCards";
import { QuotationTrendChart } from "@/components/reports/QuotationTrendChart";
import { StatusBreakdownChart } from "@/components/reports/StatusBreakdownChart";
import { TopClientsTable } from "@/components/reports/TopClientsTable";
import { RecentQuotationsTable } from "@/components/reports/RecentQuotationsTable";

export default function ReportsPage() {
  const [metrics, setMetrics] = useState<SalesDashboardMetrics | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadDashboard();
  }, []);

  const loadDashboard = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await ReportsApi.getSalesDashboard();
      if (response.success) {
        setMetrics(response.data);
      }
    } catch (err: any) {
      setError(err.message || "Failed to load dashboard");
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary"></div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="p-6">
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
          {error}
        </div>
      </div>
    );
  }

  if (!metrics) {
    return null;
  }

  return (
    <div className="p-6">
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Sales Dashboard</h1>
        <p className="text-gray-600 mt-1">Overview of your sales performance</p>
      </div>

      <SalesDashboardCards metrics={metrics} />

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mt-6">
        <QuotationTrendChart data={metrics.quotationTrend} />
        <StatusBreakdownChart data={metrics.statusBreakdown} />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mt-6">
        <TopClientsTable clients={metrics.topClients} />
        <RecentQuotationsTable quotations={metrics.recentQuotations} />
      </div>
    </div>
  );
}

