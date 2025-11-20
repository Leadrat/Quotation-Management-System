"use client";

import type { ChartData } from "@/types/reports";

interface FunnelChartProps {
  data: ChartData;
  title?: string;
}

export function FunnelChart({ data, title }: FunnelChartProps) {
  if (!data || !data.series || data.series.length === 0) {
    return (
      <div className="bg-white rounded-lg shadow p-6 dark:bg-gray-800">
        {title && <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">{title}</h3>}
        <p className="text-gray-500 dark:text-gray-400 text-center py-8">No data available</p>
      </div>
    );
  }

  const maxValue = Math.max(...data.series[0].data);
  const colors = ["#3b82f6", "#10b981", "#f59e0b", "#ef4444", "#8b5cf6"];

  return (
    <div className="bg-white rounded-lg shadow p-6 dark:bg-gray-800">
      {title && <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">{title}</h3>}
      <div className="space-y-2">
        {data.categories.map((category, index) => {
          const value = data.series[0].data[index] || 0;
          const widthPercent = maxValue > 0 ? (value / maxValue) * 100 : 0;
          const color = colors[index % colors.length];

          return (
            <div key={category} className="flex items-center gap-4">
              <div className="w-32 text-sm font-medium text-gray-700 dark:text-gray-300">{category}</div>
              <div className="flex-1">
                <div className="w-full bg-gray-200 rounded-full h-8 dark:bg-gray-700 relative">
                  <div
                    className="h-8 rounded-full flex items-center justify-end pr-3 text-white text-sm font-medium"
                    style={{ width: `${widthPercent}%`, backgroundColor: color }}
                  >
                    {widthPercent > 15 && <span>{value.toLocaleString()}</span>}
                  </div>
                </div>
              </div>
              <div className="w-24 text-right text-sm font-medium text-gray-700 dark:text-gray-300">
                {value.toLocaleString()}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}

