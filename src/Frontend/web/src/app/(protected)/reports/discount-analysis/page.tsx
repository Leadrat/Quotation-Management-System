"use client";
import { useState, useEffect } from "react";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import { ReportsApi } from "@/lib/api";
import { DateRangePicker } from "@/components/reports/ui";
import { ExportButton } from "@/components/reports/ui";
import { BarChart, PieChart } from "@/components/reports/charts";
import type { DiscountAnalytics } from "@/types/reports";

export default function DiscountAnalysisReportPage() {
  const [data, setData] = useState<DiscountAnalytics | null>(null);
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
      const response = await ReportsApi.getDiscountAnalytics(
        fromDate || undefined,
        toDate || undefined
      );
      if (response.success && response.data) {
        setData(response.data);
      }
    } catch (err: any) {
      console.error("Error loading discount analysis report:", err);
      setError(err.message || "Failed to load report");
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <PageBreadcrumb pageTitle="Discount Analysis Report" />
      
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
        <ComponentCard title="Discount Analysis Report">
          <div className="text-center py-12">
            <p className="text-gray-500 dark:text-gray-400">Loading report...</p>
          </div>
        </ComponentCard>
      ) : data ? (
        <div className="space-y-6">
          <ComponentCard title="Discount Analytics Overview">
            <div className="flex justify-end mb-4">
              <ExportButton reportId="discount-analysis" reportName="Discount Analysis Report" />
            </div>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
              <div className="p-4 bg-gray-50 rounded-lg dark:bg-gray-800">
                <p className="text-sm text-gray-500 dark:text-gray-400">Average Discount %</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white mt-1">
                  {data.averageDiscountPercent.toFixed(1)}%
                </p>
              </div>
              <div className="p-4 bg-gray-50 rounded-lg dark:bg-gray-800">
                <p className="text-sm text-gray-500 dark:text-gray-400">Approval Rate</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white mt-1">
                  {data.approvalRate.toFixed(1)}%
                </p>
              </div>
              <div className="p-4 bg-gray-50 rounded-lg dark:bg-gray-800">
                <p className="text-sm text-gray-500 dark:text-gray-400">Margin Impact</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white mt-1">
                  ₹{data.marginImpact.toLocaleString()}
                </p>
              </div>
            </div>

            {data.discountByRep && data.discountByRep.length > 0 && (
              <BarChart
                data={{
                  chartType: "bar",
                  title: "Discount % by Sales Rep",
                  series: [
                    {
                      name: "Average Discount",
                      data: data.discountByRep.map((r) => r.averageDiscount),
                    },
                  ],
                  categories: data.discountByRep.map((r) => r.userName),
                }}
                title="Discount % by Sales Rep"
              />
            )}
          </ComponentCard>
        </div>
      ) : (
        <ComponentCard title="Discount Analysis Report">
          <div className="text-center py-12">
            <p className="text-gray-500 dark:text-gray-400">No data available</p>
          </div>
        </ComponentCard>
      )}
    </>
  );
}

