"use client";

import dynamic from "next/dynamic";
import type { ChartData } from "@/types/reports";

const Chart = dynamic(() => import("react-apexcharts"), { ssr: false });

interface LineChartProps {
  data: ChartData;
  title?: string;
  height?: number;
}

export function LineChart({ data, title, height = 300 }: LineChartProps) {
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
      type: "line",
      toolbar: { show: false },
      zoom: { enabled: false },
    },
    stroke: {
      curve: "smooth",
      width: 2,
    },
    markers: {
      size: 4,
    },
    xaxis: {
      categories: data.categories,
    },
    yaxis: {
      title: {
        text: "Value",
      },
    },
    tooltip: {
      y: {
        formatter: (val: number) => val.toLocaleString(),
      },
    },
    colors: ["#3b82f6", "#10b981", "#f59e0b", "#ef4444"],
    legend: {
      position: "top",
    },
  };

  const series = data.series.map((s) => ({
    name: s.name,
    data: s.data,
  }));

  return (
    <div className="bg-white rounded-lg shadow p-6 dark:bg-gray-800">
      {title && <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">{title}</h3>}
      <Chart options={options} series={series} type="line" height={height} />
    </div>
  );
}

