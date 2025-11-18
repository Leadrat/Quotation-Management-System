"use client";
import type { TemplateUsageStats } from "@/types/templates";

interface TemplateUsageChartProps {
  stats: TemplateUsageStats;
}

export default function TemplateUsageChart({ stats }: TemplateUsageChartProps) {
  const maxUsage = Math.max(...stats.mostUsedTemplates.map((t) => t.usageCount), 1);

  return (
    <div className="rounded border border-stroke bg-white p-6 dark:border-strokedark dark:bg-boxdark">
      <h5 className="mb-4 text-lg font-semibold text-black dark:text-white">Most Used Templates</h5>
      <div className="space-y-4">
        {stats.mostUsedTemplates.length === 0 ? (
          <p className="text-center text-gray-500 py-4">No usage data available</p>
        ) : (
          stats.mostUsedTemplates.map((template) => {
            const percentage = (template.usageCount / maxUsage) * 100;
            return (
              <div key={template.templateId} className="space-y-2">
                <div className="flex items-center justify-between text-sm">
                  <span className="font-medium text-black dark:text-white">{template.name}</span>
                  <span className="text-gray-600 dark:text-gray-400">{template.usageCount} uses</span>
                </div>
                <div className="w-full bg-gray-200 rounded-full h-2.5 dark:bg-gray-700">
                  <div
                    className="bg-primary h-2.5 rounded-full transition-all"
                    style={{ width: `${percentage}%` }}
                  />
                </div>
              </div>
            );
          })
        )}
      </div>
    </div>
  );
}

