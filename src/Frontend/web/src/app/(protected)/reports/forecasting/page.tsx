"use client";
import { useState } from "react";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import { useForecast } from "@/hooks/useForecast";
import { ExportButton } from "@/components/reports/ui";
import { LineChart } from "@/components/reports/charts";

export default function ForecastingReportPage() {
  const [days, setDays] = useState(30);
  const [confidenceLevel, setConfidenceLevel] = useState(0.95);
  const { data, loading, error, loadForecast } = useForecast({ days, confidenceLevel, autoLoad: true });

  return (
    <>
      <PageBreadcrumb pageTitle="Forecasting Report" />
      
      <div className="mb-6 grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Forecast Days
          </label>
          <input
            type="number"
            value={days}
            onChange={(e) => setDays(Number(e.target.value))}
            min={7}
            max={365}
            className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-gray-700 focus:border-blue-500 focus:outline-none dark:border-gray-600 dark:bg-gray-800 dark:text-gray-300"
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Confidence Level
          </label>
          <input
            type="number"
            value={confidenceLevel}
            onChange={(e) => setConfidenceLevel(Number(e.target.value))}
            min={0.5}
            max={0.99}
            step={0.01}
            className="w-full rounded-lg border border-gray-300 bg-white px-4 py-2 text-gray-700 focus:border-blue-500 focus:outline-none dark:border-gray-600 dark:bg-gray-800 dark:text-gray-300"
          />
        </div>
      </div>

      {error && (
        <div className="mb-6 rounded-lg bg-red-50 border border-red-200 p-4 dark:bg-red-900/20 dark:border-red-800">
          <p className="text-sm text-red-800 dark:text-red-400">{error}</p>
        </div>
      )}

      {loading ? (
        <ComponentCard title="Forecasting Report">
          <div className="text-center py-12">
            <p className="text-gray-500 dark:text-gray-400">Generating forecast...</p>
          </div>
        </ComponentCard>
      ) : data ? (
        <div className="space-y-6">
          <ComponentCard title="Revenue Forecast">
            <div className="flex justify-end mb-4">
              <ExportButton reportId="forecasting" reportName="Forecasting Report" />
            </div>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
              <div className="p-4 bg-gray-50 rounded-lg dark:bg-gray-800">
                <p className="text-sm text-gray-500 dark:text-gray-400">Predicted Revenue</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white mt-1">
                  ₹{data.predictedRevenue.toLocaleString()}
                </p>
              </div>
              <div className="p-4 bg-gray-50 rounded-lg dark:bg-gray-800">
                <p className="text-sm text-gray-500 dark:text-gray-400">Confidence Level</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white mt-1">
                  {(data.confidenceLevel * 100).toFixed(0)}%
                </p>
              </div>
              <div className="p-4 bg-gray-50 rounded-lg dark:bg-gray-800">
                <p className="text-sm text-gray-500 dark:text-gray-400">Success Rate</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white mt-1">
                  {data.predictedSuccessRate.toFixed(1)}%
                </p>
              </div>
            </div>

            {data.revenueForecast && data.revenueForecast.length > 0 && (
              <LineChart
                data={{
                  chartType: "line",
                  title: "Revenue Forecast",
                  series: [
                    {
                      name: "Predicted Revenue",
                      data: data.revenueForecast.map((f) => f.predictedRevenue),
                    },
                  ],
                  categories: data.revenueForecast.map((f) => f.date),
                }}
                title="Revenue Forecast"
              />
            )}

            {data.trend && data.trend.length > 0 && (
              <div className="mt-6">
                <LineChart
                  data={{
                    chartType: "line",
                    title: "Trend Analysis",
                    series: [
                      {
                        name: "Actual",
                        data: data.trend.map((t) => t.actualValue),
                      },
                      {
                        name: "Predicted",
                        data: data.trend.map((t) => t.predictedValue || 0),
                      },
                    ],
                    categories: data.trend.map((t) => t.date),
                  }}
                  title="Trend Analysis (Actual vs Predicted)"
                />
              </div>
            )}
          </ComponentCard>
        </div>
      ) : (
        <ComponentCard title="Forecasting Report">
          <div className="text-center py-12">
            <p className="text-gray-500 dark:text-gray-400">No forecast data available</p>
          </div>
        </ComponentCard>
      )}
    </>
  );
}

