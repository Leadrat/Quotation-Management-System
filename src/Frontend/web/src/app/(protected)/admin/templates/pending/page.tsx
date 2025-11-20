"use client";
import { useEffect, useState } from "react";
import Link from "next/link";
import { TemplatesApi } from "@/lib/api";
import { AdminApprovalActions } from "@/components/templates/admin";
import type { QuotationTemplate } from "@/types/templates";

export default function TemplateApprovalQueuePage() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [templates, setTemplates] = useState<QuotationTemplate[]>([]);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [total, setTotal] = useState(0);
  const [searchTerm, setSearchTerm] = useState("");
  const [visibilityFilter, setVisibilityFilter] = useState("");

  useEffect(() => {
    loadData();
  }, [pageNumber, pageSize, searchTerm, visibilityFilter]);

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);
      const params: any = {
        pageNumber,
        pageSize,
        isApproved: false, // Only pending templates
      };
      if (searchTerm) params.search = searchTerm;
      if (visibilityFilter) params.visibility = visibilityFilter;

      const result = await TemplatesApi.list(params);
      setTemplates(Array.isArray(result.data?.data) ? result.data.data : []);
      setTotal(result.data?.totalCount || 0);
    } catch (err: any) {
      const errorMsg = err.message || "Failed to load pending templates";
      setError(errorMsg);
      setTemplates([]);
      setTotal(0);
      // Don't show error for 500 - endpoint may have issues
      if (err.message?.includes("500")) {
        setError("Template approval queue is temporarily unavailable. Please try again later.");
      }
    } finally {
      setLoading(false);
    }
  };

  const handleApproval = async (templateId: string) => {
    try {
      await TemplatesApi.approve(templateId);
      await loadData(); // Reload list
    } catch (err: any) {
      alert(err.message || "Failed to approve template");
    }
  };

  const formatDate = (date: string) => {
    return new Date(date).toLocaleDateString();
  };

  const getVisibilityBadge = (visibility: string) => {
    const colors: Record<string, string> = {
      Public: "bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300",
      Team: "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300",
      Private: "bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-300",
    };
    return (
      <span className={`inline-flex rounded-full px-3 py-1 text-xs font-medium ${colors[visibility] || colors.Private}`}>
        {visibility}
      </span>
    );
  };

  return (
    <div className="rounded-sm border border-stroke bg-white px-5 pb-2.5 pt-6 shadow-default dark:border-strokedark dark:bg-boxdark sm:px-7.5 xl:pb-1">
      <div className="mb-6">
        <h4 className="text-title-md2 font-bold text-black dark:text-white">Template Approval Queue</h4>
        <p className="mt-1 text-sm text-gray-600 dark:text-gray-400">
          Review and approve templates submitted for public use
        </p>
      </div>

      {/* Filters */}
      <div className="mb-4 grid grid-cols-1 gap-4 md:grid-cols-3">
        <div>
          <label className="mb-2.5 block text-black dark:text-white">Search</label>
          <input
            type="text"
            value={searchTerm}
            onChange={(e) => {
              setSearchTerm(e.target.value);
              setPageNumber(1);
            }}
            placeholder="Search by name or description..."
            className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 font-medium outline-none transition focus:border-primary active:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
          />
        </div>
        <div>
          <label className="mb-2.5 block text-black dark:text-white">Visibility</label>
          <select
            value={visibilityFilter}
            onChange={(e) => {
              setVisibilityFilter(e.target.value);
              setPageNumber(1);
            }}
            className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 font-medium outline-none transition focus:border-primary active:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
          >
            <option value="">All</option>
            <option value="Public">Public</option>
            <option value="Team">Team</option>
            <option value="Private">Private</option>
          </select>
        </div>
        <div className="flex items-end">
          <button
            onClick={() => {
              setSearchTerm("");
              setVisibilityFilter("");
              setPageNumber(1);
            }}
            className="w-full rounded border border-stroke px-4 py-3 text-sm font-medium hover:bg-gray-50 dark:border-strokedark dark:hover:bg-meta-4"
          >
            Clear Filters
          </button>
        </div>
      </div>

      {error && (
        <div className="mb-4 rounded border-l-4 border-red-500 bg-red-50 p-4 dark:bg-red-900/20">
          <p className="text-red-700 dark:text-red-400">{error}</p>
        </div>
      )}

      {loading ? (
        <div className="py-8 text-center">Loading pending templates...</div>
      ) : templates.length === 0 ? (
        <div className="py-8 text-center text-gray-500">No pending templates for approval</div>
      ) : (
        <>
          <div className="max-w-full overflow-x-auto">
            <table className="w-full table-auto">
              <thead>
                <tr className="bg-gray-2 text-left dark:bg-meta-4">
                  <th className="min-w-[200px] px-4 py-4 font-medium text-black dark:text-white">Template Name</th>
                  <th className="min-w-[120px] px-4 py-4 font-medium text-black dark:text-white">Owner</th>
                  <th className="min-w-[100px] px-4 py-4 font-medium text-black dark:text-white">Visibility</th>
                  <th className="min-w-[80px] px-4 py-4 font-medium text-black dark:text-white">Items</th>
                  <th className="min-w-[120px] px-4 py-4 font-medium text-black dark:text-white">Created</th>
                  <th className="min-w-[200px] px-4 py-4 font-medium text-black dark:text-white">Actions</th>
                </tr>
              </thead>
              <tbody>
                {!templates || templates.length === 0 ? (
                  <tr>
                    <td colSpan={6} className="px-4 py-12 text-center text-gray-500 dark:text-gray-400">
                      No pending templates found
                    </td>
                  </tr>
                ) : (
                  templates.map((template) => (
                  <tr key={template.templateId} className="border-b border-[#eee] dark:border-strokedark">
                    <td className="px-4 py-5 dark:border-strokedark">
                      <Link href={`/templates/${template.templateId}`} className="text-primary hover:underline font-medium">
                        {template.name}
                      </Link>
                      {template.description && (
                        <p className="text-sm text-gray-500 mt-1">{template.description}</p>
                      )}
                    </td>
                    <td className="px-4 py-5 dark:border-strokedark">
                      <p className="text-black dark:text-white">{template.ownerUserName}</p>
                      <p className="text-xs text-gray-500">{template.ownerRole}</p>
                    </td>
                    <td className="px-4 py-5 dark:border-strokedark">
                      {getVisibilityBadge(template.visibility)}
                    </td>
                    <td className="px-4 py-5 dark:border-strokedark">
                      <p className="text-black dark:text-white">{template.lineItems.length}</p>
                    </td>
                    <td className="px-4 py-5 dark:border-strokedark">
                      <p className="text-black dark:text-white">{formatDate(template.createdAt)}</p>
                    </td>
                    <td className="px-4 py-5 dark:border-strokedark">
                      <AdminApprovalActions
                        template={template}
                        onApprove={() => handleApproval(template.templateId)}
                      />
                    </td>
                  </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          <div className="mt-4 flex items-center justify-between">
            <div className="text-sm text-gray-500">
              Showing {(pageNumber - 1) * pageSize + 1} to {Math.min(pageNumber * pageSize, total)} of {total} templates
            </div>
            <div className="flex gap-2">
              <button
                onClick={() => setPageNumber((p) => Math.max(1, p - 1))}
                disabled={pageNumber === 1}
                className="rounded border border-stroke px-4 py-2 text-sm disabled:opacity-50 dark:border-strokedark"
              >
                Previous
              </button>
              <button
                onClick={() => setPageNumber((p) => p + 1)}
                disabled={pageNumber * pageSize >= total}
                className="rounded border border-stroke px-4 py-2 text-sm disabled:opacity-50 dark:border-strokedark"
              >
                Next
              </button>
            </div>
          </div>
        </>
      )}
    </div>
  );
}

