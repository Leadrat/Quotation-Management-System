"use client";

import type { KPIMetric } from "@/types/reports";

interface KPICardProps {
  metric: KPIMetric;
  icon?: React.ReactNode;
}

export function KPICard({ metric, icon }: KPICardProps) {
  const getTrendIcon = () => {
    switch (metric.trend) {
      case "up":
        return (
          <svg className="w-4 h-4 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 7h8m0 0v8m0-8l-8 8-4-4-6 6" />
          </svg>
        );
      case "down":
        return (
          <svg className="w-4 h-4 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 17h8m0 0V9m0 8l-8-8-4 4-6-6" />
          </svg>
        );
      default:
        return null;
    }
  };

  const getColorClass = () => {
    switch (metric.color) {
      case "green":
        return "bg-green-100 text-green-800 dark:bg-green-900/20 dark:text-green-400";
      case "red":
        return "bg-red-100 text-red-800 dark:bg-red-900/20 dark:text-red-400";
      case "yellow":
        return "bg-yellow-100 text-yellow-800 dark:bg-yellow-900/20 dark:text-yellow-400";
      default:
        return "bg-gray-100 text-gray-800 dark:bg-gray-900/20 dark:text-gray-400";
    }
  };

  return (
    <div className="rounded-2xl border border-gray-200 bg-white p-5 dark:border-gray-800 dark:bg-gray-900">
      <div className="flex items-start justify-between mb-2">
        <h4 className="font-bold text-gray-800 text-title-sm dark:text-white/90 flex items-center gap-2">
          {metric.value} {metric.unit && <span className="text-sm font-normal">{metric.unit}</span>}
          {metric.trend && <span className="mt-0.5">{getTrendIcon()}</span>}
        </h4>
        {icon && (
          <div className="flex items-center justify-center w-12 h-12 bg-gray-100 rounded-xl dark:bg-gray-800">
            {icon}
          </div>
        )}
      </div>
      <div>
        <p className="text-sm text-gray-500 dark:text-gray-400">{metric.name}</p>
        {metric.color && (
          <span className={`inline-block mt-2 px-2 py-1 text-xs font-medium rounded-full ${getColorClass()}`}>
            {metric.color}
          </span>
        )}
      </div>
    </div>
  );
}

