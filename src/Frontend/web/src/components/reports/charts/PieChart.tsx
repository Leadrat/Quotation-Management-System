"use client";

import dynamic from "next/dynamic";
import type { ChartData } from "@/types/reports";

const Chart = dynamic(() => import("react-apexcharts"), { ssr: false });

interface PieChartProps {
  data: ChartData;
  title?: string;
  height?: number;
}

export function PieChart({ data, title, height = 300 }: PieChartProps) {
  if (!data || !data.series || data.series.length === 0) {
    return (
      <div className="bg-white rounded-lg shadow p-6 dark:bg-gray-800">
        {title && <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">{title}</h3>}
        <p className="text-gray-500 dark:text-gray-400 text-center py-8">No data available</p>
      </div>
    );
  }

  const options = {
    chart: {
      type: "pie",
      toolbar: { show: false },
    },
    labels: data.categories,
    legend: {
      position: "bottom",
    },
    dataLabels: {
      enabled: true,
      formatter: (val: number) => `${val.toFixed(1)}%`,
    },
    tooltip: {
      y: {
        formatter: (val: number) => val.toLocaleString(),
      },
    },
    colors: ["#3b82f6", "#10b981", "#f59e0b", "#ef4444", "#8b5cf6", "#ec4899"],
  };

  const series = data.series[0]?.data || [];

  return (
    <div className="bg-white rounded-lg shadow p-6 dark:bg-gray-800">
      {title && <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">{title}</h3>}
      <Chart options={options} series={series} type="pie" height={height} />
    </div>
  );
}

