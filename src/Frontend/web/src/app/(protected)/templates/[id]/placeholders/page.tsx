"use client";
import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { getAccessToken, getRoleFromToken } from "@/lib/session";
import { DocumentTemplatesApi, TemplatesApi } from "@/lib/api";
import DocumentTemplatePreview from "@/components/templates/DocumentTemplatePreview";

interface PlaceholderDto {
  placeholderName: string;
  placeholderType: string;
  defaultValue?: string;
}

export default function ConfigurePlaceholdersPage() {
  const router = useRouter();
  const params = useParams();
  const templateId = String(params?.id || "");
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [placeholders, setPlaceholders] = useState<PlaceholderDto[]>([]);
  const [templateName, setTemplateName] = useState<string>("");

  useEffect(() => {
    const token = getAccessToken();
    const role = getRoleFromToken(token);
    // Only Admin & SalesRep can configure placeholders
    if (role !== "Admin" && role !== "SalesRep") {
      router.replace("/dashboard");
      return;
    }
    if (templateId) {
      load();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [templateId]);

  const load = async () => {
    try {
      setLoading(true);
      setError(null);
      // Fetch template basic info for header
      const t = await TemplatesApi.get(templateId);
      setTemplateName(t.data.name);
      // Fetch detected placeholders
      const res = await DocumentTemplatesApi.getPlaceholders(templateId);
      setPlaceholders(res.data || []);
    } catch (e: any) {
      setError(e?.message || "Failed to load placeholders");
    } finally {
      setLoading(false);
    }
  };

  const handleSave = async (edited: PlaceholderDto[]) => {
    try {
      setSaving(true);
      setError(null);
      await DocumentTemplatesApi.savePlaceholders(templateId, edited);
      router.push(`/templates/${templateId}`);
    } catch (e: any) {
      setError(e?.message || "Failed to save placeholders");
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <div className="rounded-sm border border-stroke bg-white px-5 pb-2.5 pt-6 shadow-default dark:border-strokedark dark:bg-boxdark sm:px-7.5 xl:pb-1">
        <div className="py-8 text-center">Loading placeholders...</div>
      </div>
    );
  }

  return (
    <div className="rounded-sm border border-stroke bg-white px-5 pb-2.5 pt-6 shadow-default dark:border-strokedark dark:bg-boxdark sm:px-7.5 xl:pb-1">
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h4 className="text-title-md2 font-bold text-black dark:text-white">Configure Placeholders</h4>
          <p className="mt-1 text-sm text-gray-600 dark:text-gray-400">Template: {templateName}</p>
        </div>
        <button
          onClick={() => router.push(`/templates/${templateId}`)}
          className="rounded border border-stroke px-4 py-2 text-sm font-medium hover:bg-gray-50 dark:border-strokedark dark:hover:bg-meta-4"
        >
          Back
        </button>
      </div>

      {error && (
        <div className="mb-4 rounded border-l-4 border-red-500 bg-red-50 p-4 dark:bg-red-900/20">
          <p className="text-red-700 dark:text-red-400">{error}</p>
        </div>
      )}

      <DocumentTemplatePreview
        placeholders={placeholders.map(p => ({
          placeholderName: p.placeholderName,
          placeholderType: p.placeholderType,
          defaultValue: p.defaultValue || "",
        }))}
        onSave={async (edited) => {
          if (saving) return;
          await handleSave(edited as any);
        }}
        onCancel={() => router.back()}
      />
    </div>
  );
}
