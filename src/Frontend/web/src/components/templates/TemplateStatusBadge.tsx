"use client";
import type { TemplateVisibility } from "@/types/templates";

interface TemplateStatusBadgeProps {
  visibility: TemplateVisibility;
  isApproved: boolean;
}

export default function TemplateStatusBadge({ visibility, isApproved }: TemplateStatusBadgeProps) {
  const visibilityColors: Record<TemplateVisibility, string> = {
    Public: "bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300",
    Team: "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300",
    Private: "bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-300",
  };

  return (
    <div className="flex items-center gap-2">
      <span className={`inline-flex rounded-full px-3 py-1 text-xs font-medium ${visibilityColors[visibility]}`}>
        {visibility}
      </span>
      {isApproved ? (
        <span className="inline-flex rounded-full bg-green-100 px-3 py-1 text-xs font-medium text-green-800 dark:bg-green-900 dark:text-green-300">
          Approved
        </span>
      ) : (
        <span className="inline-flex rounded-full bg-yellow-100 px-3 py-1 text-xs font-medium text-yellow-800 dark:bg-yellow-900 dark:text-yellow-300">
          Pending
        </span>
      )}
    </div>
  );
}

