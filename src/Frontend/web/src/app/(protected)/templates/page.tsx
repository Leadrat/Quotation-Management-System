"use client";
import { useEffect, useState } from "react";
import Link from "next/link";
import { TemplatesApi } from "@/lib/api";
import { TemplateErrorBoundary, TemplateListSkeleton } from "@/components/templates";
import { useToast, ToastContainer } from "@/components/quotations/Toast";
import type { QuotationTemplate } from "@/types/templates";

export default function TemplatesListPage() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [items, setItems] = useState<QuotationTemplate[]>([]);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [total, setTotal] = useState(0);
  const toast = useToast();

  // Filters & search
  const [searchTerm, setSearchTerm] = useState("");
  const [visibilityFilter, setVisibilityFilter] = useState("");
  const [approvalFilter, setApprovalFilter] = useState("");

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);
      const params: any = {
        pageNumber,
        pageSize,
      };
      if (searchTerm) params.search = searchTerm;
      if (visibilityFilter) params.visibility = visibilityFilter;
      if (approvalFilter !== "") params.isApproved = approvalFilter === "approved";

      const result = await TemplatesApi.list(params);
      setItems(result.data.data || []);
      setTotal(result.data.totalCount || 0);
      setError(null);
    } catch (err: any) {
      const errorMsg = err.message || "Failed to load templates";
      setError(errorMsg);
      toast.error(errorMsg);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, [pageNumber, pageSize, searchTerm, visibilityFilter, approvalFilter]);

  const handleDelete = async (id: string) => {
    if (!confirm("Are you sure you want to delete this template? It will be soft-deleted and can be restored later.")) return;
    try {
      toast.info("Deleting template...");
      await TemplatesApi.delete(id);
      toast.success("Template deleted successfully");
      await loadData();
    } catch (err: any) {
      const errorMsg = err.message || "Failed to delete template";
      toast.error(errorMsg);
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
    <TemplateErrorBoundary>
      <div className="rounded-sm border border-stroke bg-white px-5 pb-2.5 pt-6 shadow-default dark:border-strokedark dark:bg-boxdark sm:px-7.5 xl:pb-1">
      <ToastContainer toasts={toast.toasts} onRemove={toast.removeToast} />
      <div className="mb-6 flex items-center justify-between">
        <h4 className="text-title-md2 font-bold text-black dark:text-white">Quotation Templates</h4>
        <Link
          href="/templates/upload"
          className="inline-flex items-center justify-center rounded-md border-2 border-blue-500 bg-white px-6 py-2.5 text-center font-medium text-black hover:bg-blue-50 dark:bg-gray-800 dark:text-black dark:hover:bg-gray-700"
        >
          Upload Template
        </Link>
      </div>

      {/* Search and Filters */}
      <div className="mb-4 grid grid-cols-1 gap-4 md:grid-cols-4">
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
        <div>
          <label className="mb-2.5 block text-black dark:text-white">Approval Status</label>
          <select
            value={approvalFilter}
            onChange={(e) => {
              setApprovalFilter(e.target.value);
              setPageNumber(1);
            }}
            className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 font-medium outline-none transition focus:border-primary active:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
          >
            <option value="">All</option>
            <option value="approved">Approved</option>
            <option value="pending">Pending</option>
          </select>
        </div>
        <div className="flex items-end">
          <button
            onClick={() => {
              setSearchTerm("");
              setVisibilityFilter("");
              setApprovalFilter("");
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
        <TemplateListSkeleton />
      ) : items.length === 0 ? (
        <div className="py-8 text-center text-gray-500">No templates found</div>
      ) : (
        <>
          <div className="max-w-full overflow-x-auto">
            <table className="w-full table-auto">
              <thead>
                <tr className="bg-gray-2 text-left dark:bg-meta-4">
                  <th className="min-w-[200px] px-4 py-4 font-medium text-black dark:text-white">Name</th>
                  <th className="min-w-[120px] px-4 py-4 font-medium text-black dark:text-white">Owner</th>
                  <th className="min-w-[100px] px-4 py-4 font-medium text-black dark:text-white">Visibility</th>
                  <th className="min-w-[100px] px-4 py-4 font-medium text-black dark:text-white">Status</th>
                  <th className="min-w-[80px] px-4 py-4 font-medium text-black dark:text-white">Version</th>
                  <th className="min-w-[80px] px-4 py-4 font-medium text-black dark:text-white">Usage</th>
                  <th className="min-w-[120px] px-4 py-4 font-medium text-black dark:text-white">Last Updated</th>
                  <th className="min-w-[200px] px-4 py-4 font-medium text-black dark:text-white">Actions</th>
                </tr>
              </thead>
              <tbody>
                {items.map((item) => (
                  <tr key={item.templateId} className="border-b border-[#eee] dark:border-strokedark">
                    <td className="px-4 py-5 dark:border-strokedark">
                      <div className="flex items-center gap-2">
                        <Link href={`/templates/${item.templateId}`} className="text-primary hover:underline font-medium">
                          {item.name}
                        </Link>
                        {item.isFileBased && (
                          <span className="rounded bg-blue-100 px-2 py-0.5 text-xs text-blue-800 dark:bg-blue-900 dark:text-blue-300">
                            {item.templateType || "File"}
                          </span>
                        )}
                      </div>
                      {item.description && (
                        <p className="text-sm text-gray-500 mt-1">{item.description}</p>
                      )}
                      {item.fileName && (
                        <p className="text-xs text-gray-400 mt-1">📄 {item.fileName}</p>
                      )}
                    </td>
                    <td className="px-4 py-5 dark:border-strokedark">
                      <p className="text-black dark:text-white">{item.ownerUserName}</p>
                    </td>
                    <td className="px-4 py-5 dark:border-strokedark">
                      {getVisibilityBadge(item.visibility)}
                    </td>
                    <td className="px-4 py-5 dark:border-strokedark">
                      {item.isApproved ? (
                        <span className="inline-flex rounded-full bg-green-100 px-3 py-1 text-xs font-medium text-green-800 dark:bg-green-900 dark:text-green-300">
                          Approved
                        </span>
                      ) : (
                        <span className="inline-flex rounded-full bg-yellow-100 px-3 py-1 text-xs font-medium text-yellow-800 dark:bg-yellow-900 dark:text-yellow-300">
                          Pending
                        </span>
                      )}
                    </td>
                    <td className="px-4 py-5 dark:border-strokedark">
                      <p className="text-black dark:text-white">v{item.version}</p>
                    </td>
                    <td className="px-4 py-5 dark:border-strokedark">
                      <p className="text-black dark:text-white">{item.usageCount}</p>
                    </td>
                    <td className="px-4 py-5 dark:border-strokedark">
                      <p className="text-black dark:text-white">{formatDate(item.updatedAt)}</p>
                    </td>
                    <td className="px-4 py-5 dark:border-strokedark">
                      <div className="flex items-center gap-2 flex-wrap">
                        <Link
                          href={`/templates/${item.templateId}`}
                          className="rounded bg-primary px-3 py-1 text-xs text-white hover:bg-opacity-90"
                        >
                          View
                        </Link>
                        <Link
                          href={`/templates/${item.templateId}/edit`}
                          className="rounded bg-yellow-500 px-3 py-1 text-xs text-white hover:bg-opacity-90"
                        >
                          Edit
                        </Link>
                        <Link
                          href={`/templates/${item.templateId}/versions`}
                          className="rounded bg-blue-500 px-3 py-1 text-xs text-white hover:bg-opacity-90"
                        >
                          Versions
                        </Link>
                        <button
                          onClick={() => handleDelete(item.templateId)}
                          className="rounded bg-red-500 px-3 py-1 text-xs text-white hover:bg-opacity-90"
                        >
                          Delete
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
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
    </TemplateErrorBoundary>
  );
}

