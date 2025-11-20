"use client";
import { useState, useEffect } from "react";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import { ReportsApi } from "@/lib/api";
import { DateRangePicker } from "@/components/reports/ui";
import { ExportButton } from "@/components/reports/ui";
import { LineChart, BarChart } from "@/components/reports/charts";
import type { ClientEngagement } from "@/types/reports";

export default function ClientEngagementReportPage() {
  const [data, setData] = useState<ClientEngagement | null>(null);
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
      const response = await ReportsApi.getClientEngagement(
        undefined,
        fromDate || undefined,
        toDate || undefined
      );
      if (response.success && response.data) {
        setData(response.data);
      }
    } catch (err: any) {
      console.error("Error loading client engagement report:", err);
      setError(err.message || "Failed to load report");
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <PageBreadcrumb pageTitle="Client Engagement Report" />
      
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
        <ComponentCard title="Client Engagement Report">
          <div className="text-center py-12">
            <p className="text-gray-500 dark:text-gray-400">Loading report...</p>
          </div>
        </ComponentCard>
      ) : data ? (
        <div className="space-y-6">
          <ComponentCard title="Client Engagement Overview">
            <div className="flex justify-end mb-4">
              <ExportButton reportId="client-engagement" reportName="Client Engagement Report" />
            </div>
            <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
              <div className="p-4 bg-gray-50 rounded-lg dark:bg-gray-800">
                <p className="text-sm text-gray-500 dark:text-gray-400">View Rate</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white mt-1">
                  {data.viewRate.toFixed(1)}%
                </p>
              </div>
              <div className="p-4 bg-gray-50 rounded-lg dark:bg-gray-800">
                <p className="text-sm text-gray-500 dark:text-gray-400">Response Rate</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white mt-1">
                  {data.responseRate.toFixed(1)}%
                </p>
              </div>
              <div className="p-4 bg-gray-50 rounded-lg dark:bg-gray-800">
                <p className="text-sm text-gray-500 dark:text-gray-400">Conversion Rate</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white mt-1">
                  {data.conversionRate.toFixed(1)}%
                </p>
              </div>
              <div className="p-4 bg-gray-50 rounded-lg dark:bg-gray-800">
                <p className="text-sm text-gray-500 dark:text-gray-400">Avg Response Time</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white mt-1">
                  {data.averageResponseTimeHours.toFixed(1)} hrs
                </p>
              </div>
            </div>

            {data.clientEngagement && data.clientEngagement.length > 0 && (
              <div className="mt-6 overflow-x-auto">
                <table className="w-full">
                  <thead>
                    <tr className="border-b border-gray-200 dark:border-gray-700">
                      <th className="text-left py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                        Client
                      </th>
                      <th className="text-right py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                        Sent
                      </th>
                      <th className="text-right py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                        Viewed
                      </th>
                      <th className="text-right py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                        Responded
                      </th>
                      <th className="text-right py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                        Accepted
                      </th>
                      <th className="text-right py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                        View Rate
                      </th>
                      <th className="text-right py-3 px-4 text-sm font-medium text-gray-700 dark:text-gray-300">
                        Response Rate
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {data.clientEngagement.slice(0, 20).map((client) => (
                      <tr key={client.clientId} className="border-b border-gray-100 dark:border-gray-800">
                        <td className="py-3 px-4 text-sm font-medium text-gray-900 dark:text-white">
                          {client.clientName}
                        </td>
                        <td className="py-3 px-4 text-sm text-right text-gray-700 dark:text-gray-300">
                          {client.quotationsSent}
                        </td>
                        <td className="py-3 px-4 text-sm text-right text-gray-700 dark:text-gray-300">
                          {client.quotationsViewed}
                        </td>
                        <td className="py-3 px-4 text-sm text-right text-gray-700 dark:text-gray-300">
                          {client.quotationsResponded}
                        </td>
                        <td className="py-3 px-4 text-sm text-right text-gray-700 dark:text-gray-300">
                          {client.quotationsAccepted}
                        </td>
                        <td className="py-3 px-4 text-sm text-right text-gray-700 dark:text-gray-300">
                          {client.viewRate.toFixed(1)}%
                        </td>
                        <td className="py-3 px-4 text-sm text-right text-gray-700 dark:text-gray-300">
                          {client.responseRate.toFixed(1)}%
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </ComponentCard>
        </div>
      ) : (
        <ComponentCard title="Client Engagement Report">
          <div className="text-center py-12">
            <p className="text-gray-500 dark:text-gray-400">No data available</p>
          </div>
        </ComponentCard>
      )}
    </>
  );
}

