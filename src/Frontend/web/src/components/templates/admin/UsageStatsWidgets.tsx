"use client";
import type { TemplateUsageStats } from "@/types/templates";

interface UsageStatsWidgetsProps {
  stats: TemplateUsageStats;
}

export default function UsageStatsWidgets({ stats }: UsageStatsWidgetsProps) {
  return (
    <div className="grid grid-cols-1 gap-4 md:grid-cols-4 mb-6">
      <div className="rounded border border-stroke bg-white p-6 dark:border-strokedark dark:bg-boxdark">
        <div className="flex items-center justify-between">
          <div>
            <p className="text-sm text-gray-600 dark:text-gray-400">Total Templates</p>
            <p className="mt-2 text-2xl font-bold text-black dark:text-white">{stats.totalTemplates}</p>
          </div>
          <div className="rounded-full bg-blue-100 p-3 dark:bg-blue-900">
            <svg className="w-6 h-6 text-blue-600 dark:text-blue-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
          </div>
        </div>
      </div>

      <div className="rounded border border-stroke bg-white p-6 dark:border-strokedark dark:bg-boxdark">
        <div className="flex items-center justify-between">
          <div>
            <p className="text-sm text-gray-600 dark:text-gray-400">Total Usage</p>
            <p className="mt-2 text-2xl font-bold text-black dark:text-white">{stats.totalUsage}</p>
          </div>
          <div className="rounded-full bg-green-100 p-3 dark:bg-green-900">
            <svg className="w-6 h-6 text-green-600 dark:text-green-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 7h8m0 0v8m0-8l-8 8-4-4-6 6" />
            </svg>
          </div>
        </div>
      </div>

      <div className="rounded border border-stroke bg-white p-6 dark:border-strokedark dark:bg-boxdark">
        <div className="flex items-center justify-between">
          <div>
            <p className="text-sm text-gray-600 dark:text-gray-400">Approved</p>
            <p className="mt-2 text-2xl font-bold text-black dark:text-white">{stats.approvedTemplates}</p>
          </div>
          <div className="rounded-full bg-purple-100 p-3 dark:bg-purple-900">
            <svg className="w-6 h-6 text-purple-600 dark:text-purple-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
        </div>
      </div>

      <div className="rounded border border-stroke bg-white p-6 dark:border-strokedark dark:bg-boxdark">
        <div className="flex items-center justify-between">
          <div>
            <p className="text-sm text-gray-600 dark:text-gray-400">Pending Approval</p>
            <p className="mt-2 text-2xl font-bold text-black dark:text-white">{stats.pendingApprovalTemplates}</p>
          </div>
          <div className="rounded-full bg-yellow-100 p-3 dark:bg-yellow-900">
            <svg className="w-6 h-6 text-yellow-600 dark:text-yellow-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
        </div>
      </div>
    </div>
  );
}

