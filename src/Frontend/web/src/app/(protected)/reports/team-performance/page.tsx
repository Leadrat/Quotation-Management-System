"use client";
import { useState, useEffect } from "react";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import { ReportsApi } from "@/lib/api";
import { DateRangePicker } from "@/components/reports/ui";
import { ExportButton } from "@/components/reports/ui";
import { BarChart } from "@/components/reports/charts";
import type { TeamPerformance } from "@/types/reports";

export default function TeamPerformanceReportPage() {
  const [data, setData] = useState<TeamPerformance[]>([]);
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
      const response = await ReportsApi.getTeamPerformance(
        undefined,
        fromDate || undefined,
        toDate || undefined
      );
      if (response.success && response.data) {
        setData(response.data);
      }
    } catch (err: any) {
      console.error("Error loading team performance report:", err);
      setError(err.message || "Failed to load report");
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <PageBreadcrumb pageTitle="Team Performance Report" />
      
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
        <ComponentCard title="Team Performance Report">
          <div className="text-center py-12">
            <p className="text-gray-500 dark:text-gray-400">Loading report...</p>
          </div>
        </ComponentCard>
      ) : data.length > 0 ? (
        <div className="space-y-6">
          <ComponentCard title="Team Performance Overview">
            <div className="flex justify-end mb-4">
              <ExportButton reportId="team-performance" reportName="Team Performance Report" />
            </div>

            <BarChart
              data={{
                chartType: "bar",
                title: "Quotations Created by Team Member",
                series: [
                  {
                    name: "Quotations Created",
                    data: data.map((member) => member.quotationsCreated),
                  },
                ],
                categories: data.map((member) => member.userName),
              }}
              title="Quotations Created by Team Member"
            />

            <div className="mt-6 overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="border-b border-gray-200 dark:border-gray-700">
                    <th className="text-left py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                      Rank
                    </th>
                    <th className="text-left py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                      Name
                    </th>
                    <th className="text-right py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                      Quotations Created
                    </th>
                    <th className="text-right py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                      Quotations Sent
                    </th>
                    <th className="text-right py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                      Quotations Accepted
                    </th>
                    <th className="text-right py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                      Conversion Rate
                    </th>
                    <th className="text-right py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                      Pipeline Value
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {data.map((member) => (
                    <tr key={member.userId} className="border-b border-gray-100 dark:border-gray-800">
                      <td className="py-3 px-4 text-sm text-gray-700 dark:text-gray-300">#{member.rank}</td>
                      <td className="py-3 px-4 text-sm font-medium text-gray-900 dark:text-white">
                        {member.userName}
                      </td>
                      <td className="py-3 px-4 text-sm text-right text-gray-700 dark:text-gray-300">
                        {member.quotationsCreated}
                      </td>
                      <td className="py-3 px-4 text-sm text-right text-gray-700 dark:text-gray-300">
                        {member.quotationsSent}
                      </td>
                      <td className="py-3 px-4 text-sm text-right text-gray-700 dark:text-gray-300">
                        {member.quotationsAccepted}
                      </td>
                      <td className="py-3 px-4 text-sm text-right text-gray-700 dark:text-gray-300">
                        {member.conversionRate.toFixed(1)}%
                      </td>
                      <td className="py-3 px-4 text-sm text-right text-gray-700 dark:text-gray-300">
                        ₹{member.pipelineValue.toLocaleString()}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </ComponentCard>
        </div>
      ) : (
        <ComponentCard title="Team Performance Report">
          <div className="text-center py-12">
            <p className="text-gray-500 dark:text-gray-400">No data available</p>
          </div>
        </ComponentCard>
      )}
    </>
  );
}

