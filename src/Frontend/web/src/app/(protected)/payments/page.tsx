"use client";

import { useEffect, useState } from "react";
import { PaymentsApi } from "@/lib/api";
import type { PaymentDashboardDto, PaymentDto } from "@/types/payments";
import { PaymentSummaryCards } from "@/components/payments/PaymentSummaryCards";
import { PaymentsTable } from "@/components/payments/PaymentsTable";

export default function PaymentsPage() {
  const [dashboard, setDashboard] = useState<PaymentDashboardDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadDashboard();
  }, []);

  const loadDashboard = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await PaymentsApi.getDashboard();
      setDashboard(data);
    } catch (err: any) {
      setError(err.message || "Failed to load payments dashboard");
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

  return (
    <div className="p-6">
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Payments Dashboard</h1>
        <p className="text-gray-600 mt-1">View and manage all payment transactions</p>
      </div>

      {dashboard && (
        <>
          <PaymentSummaryCards summary={dashboard.summary} />
          <div className="mt-6">
            <PaymentsTable payments={dashboard.recentPayments} onRefresh={loadDashboard} />
          </div>
        </>
      )}
    </div>
  );
}

