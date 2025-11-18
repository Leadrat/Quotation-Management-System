"use client";

import type { StatusBreakdownData } from "@/types/reports";

interface StatusBreakdownChartProps {
  data: StatusBreakdownData[];
}

export function StatusBreakdownChart({ data }: StatusBreakdownChartProps) {
  if (!data || data.length === 0) {
    return (
      <div className="bg-white rounded-lg shadow p-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Status Breakdown</h3>
        <p className="text-gray-500 text-center py-8">No data available</p>
      </div>
    );
  }

  const colors = [
    "bg-blue-500",
    "bg-green-500",
    "bg-yellow-500",
    "bg-red-500",
    "bg-purple-500",
    "bg-indigo-500",
  ];

  const total = data.reduce((sum, item) => sum + item.count, 0);

  return (
    <div className="bg-white rounded-lg shadow p-6">
      <h3 className="text-lg font-semibold text-gray-900 mb-4">Status Breakdown</h3>
      <div className="space-y-3">
        {data.map((item, index) => (
          <div key={index} className="flex items-center gap-4">
            <div className="flex-1">
              <div className="flex justify-between items-center mb-1">
                <span className="text-sm font-medium text-gray-700">{item.status}</span>
                <span className="text-sm text-gray-600">
                  {item.count} ({item.percentage.toFixed(1)}%)
                </span>
              </div>
              <div className="w-full bg-gray-200 rounded-full h-2">
                <div
                  className={`${colors[index % colors.length]} h-2 rounded-full transition-all`}
                  style={{ width: `${item.percentage}%` }}
                />
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

