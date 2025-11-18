"use client";
import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import { TemplatesApi } from "@/lib/api";
import { VersionHistoryTimeline } from "@/components/templates";
import type { QuotationTemplateVersion } from "@/types/templates";

export default function TemplateVersionsPage() {
  const params = useParams();
  const router = useRouter();
  const templateId = String(params?.id || "");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [versions, setVersions] = useState<QuotationTemplateVersion[]>([]);
  const [currentTemplate, setCurrentTemplate] = useState<any>(null);

  useEffect(() => {
    if (templateId) {
      loadData();
    }
  }, [templateId]);

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);
      const [versionsResult, templateResult] = await Promise.all([
        TemplatesApi.getVersions(templateId),
        TemplatesApi.get(templateId),
      ]);
      setVersions(versionsResult.data);
      setCurrentTemplate(templateResult.data);
    } catch (err: any) {
      setError(err.message || "Failed to load version history");
    } finally {
      setLoading(false);
    }
  };

  const handleRestore = async (version: QuotationTemplateVersion) => {
    if (
      !confirm(
        `Are you sure you want to restore version ${version.version}? This will create a new version based on the selected version.`
      )
    )
      return;

    try {
      // Navigate to edit page with a flag to restore
      // For now, we'll just show a message - actual restore would require backend support
      alert(
        "Restore functionality: This would create a new version based on the selected version. Please use the Edit page to manually restore."
      );
    } catch (err: any) {
      alert(err.message || "Failed to restore version");
    }
  };

  if (loading) {
    return (
      <div className="rounded-sm border border-stroke bg-white px-5 pb-2.5 pt-6 shadow-default dark:border-strokedark dark:bg-boxdark sm:px-7.5 xl:pb-1">
        <div className="py-8 text-center">Loading version history...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="rounded-sm border border-stroke bg-white px-5 pb-2.5 pt-6 shadow-default dark:border-strokedark dark:bg-boxdark sm:px-7.5 xl:pb-1">
        <div className="rounded border-l-4 border-red-500 bg-red-50 p-4 dark:bg-red-900/20">
          <p className="text-red-700 dark:text-red-400">{error}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="rounded-sm border border-stroke bg-white px-5 pb-2.5 pt-6 shadow-default dark:border-strokedark dark:bg-boxdark sm:px-7.5 xl:pb-1">
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h4 className="text-title-md2 font-bold text-black dark:text-white">
            Version History
            {currentTemplate && (
              <span className="ml-2 text-lg font-normal text-gray-600 dark:text-gray-400">
                - {currentTemplate.name}
              </span>
            )}
          </h4>
          <p className="mt-1 text-sm text-gray-600 dark:text-gray-400">
            View and restore previous versions of this template
          </p>
        </div>
        <Link
          href={`/templates/${templateId}`}
          className="rounded border border-stroke px-4 py-2 text-sm hover:bg-gray-50 dark:border-strokedark dark:hover:bg-meta-4"
        >
          Back to Template
        </Link>
      </div>

      {versions.length === 0 ? (
        <div className="py-8 text-center text-gray-500">No version history available</div>
      ) : (
        <VersionHistoryTimeline versions={versions} onRestore={handleRestore} />
      )}
    </div>
  );
}

