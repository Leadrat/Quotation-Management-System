"use client";
import type { QuotationTemplateVersion } from "@/types/templates";

interface VersionHistoryTimelineProps {
  versions: QuotationTemplateVersion[];
  onRestore?: (version: QuotationTemplateVersion) => void;
}

export default function VersionHistoryTimeline({ versions, onRestore }: VersionHistoryTimelineProps) {
  const formatDate = (date: string) => {
    return new Date(date).toLocaleString();
  };

  return (
    <div className="space-y-4">
      <h5 className="text-lg font-semibold text-black dark:text-white">Version History</h5>
      <div className="relative">
        {versions.map((version, index) => (
          <div key={version.templateId} className="relative flex gap-4 pb-8">
            {index < versions.length - 1 && (
              <div className="absolute left-3 top-8 h-full w-0.5 bg-gray-300 dark:bg-gray-600" />
            )}
            <div className="relative z-10">
              <div
                className={`flex h-6 w-6 items-center justify-center rounded-full ${
                  version.isCurrentVersion
                    ? "bg-primary text-white"
                    : "bg-gray-300 text-gray-700 dark:bg-gray-600 dark:text-gray-300"
                }`}
              >
                <span className="text-xs font-bold">{version.version}</span>
              </div>
            </div>
            <div className="flex-1 rounded border border-stroke bg-white p-4 dark:border-strokedark dark:bg-boxdark">
              <div className="flex items-center justify-between">
                <div>
                  <h6 className="font-semibold text-black dark:text-white">
                    Version {version.version}
                    {version.isCurrentVersion && (
                      <span className="ml-2 rounded bg-green-100 px-2 py-1 text-xs text-green-800 dark:bg-green-900 dark:text-green-300">
                        Current
                      </span>
                    )}
                  </h6>
                  <p className="mt-1 text-sm text-gray-600 dark:text-gray-400">{version.name}</p>
                  {version.description && (
                    <p className="mt-1 text-sm text-gray-500 dark:text-gray-500">{version.description}</p>
                  )}
                  <p className="mt-2 text-xs text-gray-500">
                    Updated by {version.updatedByUserName} on {formatDate(version.updatedAt)}
                  </p>
                </div>
                {onRestore && !version.isCurrentVersion && (
                  <button
                    onClick={() => onRestore(version)}
                    className="rounded bg-yellow-500 px-3 py-1 text-xs text-white hover:bg-opacity-90"
                  >
                    Restore
                  </button>
                )}
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

