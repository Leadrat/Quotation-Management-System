"use client";
import { useState, useEffect } from "react";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import { ReportsApi } from "@/lib/api";
import { DateRangePicker } from "@/components/reports/ui";
import { ExportButton } from "@/components/reports/ui";
import { LineChart, PieChart } from "@/components/reports/charts";
import type { PaymentAnalytics } from "@/types/reports";

export default function PaymentStatusReportPage() {
  const [data, setData] = useState<PaymentAnalytics | null>(null);
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
      const response = await ReportsApi.getPaymentAnalytics(
        fromDate || undefined,
        toDate || undefined
      );
      if (response.success && response.data) {
        setData(response.data);
      }
    } catch (err: any) {
      console.error("Error loading payment status report:", err);
      setError(err.message || "Failed to load report");
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <PageBreadcrumb pageTitle="Payment Status Report" />
      
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
        <ComponentCard title="Payment Status Report">
          <div className="text-center py-12">
            <p className="text-gray-500 dark:text-gray-400">Loading report...</p>
          </div>
        </ComponentCard>
      ) : data ? (
        <div className="space-y-6">
          <ComponentCard title="Payment Analytics Overview">
            <div className="flex justify-end mb-4">
              <ExportButton reportId="payment-status" reportName="Payment Status Report" />
            </div>
            <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
              <div className="p-4 bg-gray-50 rounded-lg dark:bg-gray-800">
                <p className="text-sm text-gray-500 dark:text-gray-400">Collection Rate</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white mt-1">
                  {data.collectionRate.toFixed(1)}%
                </p>
              </div>
              <div className="p-4 bg-gray-50 rounded-lg dark:bg-gray-800">
                <p className="text-sm text-gray-500 dark:text-gray-400">Failed Payments</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white mt-1">
                  {data.failedPaymentsCount}
                </p>
              </div>
              <div className="p-4 bg-gray-50 rounded-lg dark:bg-gray-800">
                <p className="text-sm text-gray-500 dark:text-gray-400">Total Refunds</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white mt-1">
                  ₹{data.totalRefunds.toLocaleString()}
                </p>
              </div>
              <div className="p-4 bg-gray-50 rounded-lg dark:bg-gray-800">
                <p className="text-sm text-gray-500 dark:text-gray-400">Payment Methods</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white mt-1">
                  {data.paymentMethodDistribution.length}
                </p>
              </div>
            </div>

            {data.paymentMethodDistribution && data.paymentMethodDistribution.length > 0 && (
              <PieChart
                data={{
                  chartType: "pie",
                  title: "Payment Method Distribution",
                  series: [
                    {
                      name: "Distribution",
                      data: data.paymentMethodDistribution.map((m) => m.amount),
                    },
                  ],
                  categories: data.paymentMethodDistribution.map((m) => m.paymentMethod),
                }}
                title="Payment Method Distribution"
              />
            )}

            {data.paymentStatusBreakdown && data.paymentStatusBreakdown.length > 0 && (
              <div className="mt-6">
                <PieChart
                  data={{
                    chartType: "pie",
                    title: "Payment Status Breakdown",
                    series: [
                      {
                        name: "Count",
                        data: data.paymentStatusBreakdown.map((s) => s.count),
                      },
                    ],
                    categories: data.paymentStatusBreakdown.map((s) => s.status),
                  }}
                  title="Payment Status Breakdown"
                />
              </div>
            )}
          </ComponentCard>
        </div>
      ) : (
        <ComponentCard title="Payment Status Report">
          <div className="text-center py-12">
            <p className="text-gray-500 dark:text-gray-400">No data available</p>
          </div>
        </ComponentCard>
      )}
    </>
  );
}

