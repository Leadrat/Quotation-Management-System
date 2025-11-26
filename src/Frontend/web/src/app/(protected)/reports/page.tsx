"use client";

import { useEffect, useState } from "react";
import { ReportsApi } from "@/lib/api";
import type { SalesDashboardMetrics } from "@/types/reports";
import { SalesDashboardCards } from "@/components/reports/SalesDashboardCards";
import QuotationTrendChart from "@/components/reports/QuotationTrendChart";
import { StatusBreakdownChart } from "@/components/reports/StatusBreakdownChart";
import { TopClientsTable } from "@/components/reports/TopClientsTable";
import { RecentQuotationsTable } from "@/components/reports/RecentQuotationsTable";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import Alert from "@/components/tailadmin/ui/alert/Alert";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";

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
      let errorMessage = err.message || "Failed to load dashboard";
      if (err.errors && Array.isArray(err.errors) && err.errors.length > 0) {
        errorMessage += `: ${err.errors.join(", ")}`;
      }
      if (err.details) {
        errorMessage += ` (${err.details})`;
      }
      setError(errorMessage);
      console.error("Dashboard load error:", err);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <>
        <PageBreadcrumb pageTitle="Reports" />
        <ComponentCard title="Loading">
          <div className="flex items-center justify-center py-12">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-brand-500"></div>
          </div>
        </ComponentCard>
      </>
    );
  }

  if (error) {
    return (
      <>
        <PageBreadcrumb pageTitle="Reports" />
        <Alert variant="error" title="Error" message={error || "An error occurred"} />
      </>
    );
  }

  if (!metrics) {
    return null;
  }

  return (
    <>
      <PageBreadcrumb pageTitle="Sales Dashboard" />

      <div className="mb-6">
        <h2 className="text-xl font-semibold text-gray-800 dark:text-white/90">Sales Dashboard</h2>
        <p className="text-gray-500 dark:text-gray-400 mt-1">Overview of your sales performance</p>
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
    </>
  );
}

