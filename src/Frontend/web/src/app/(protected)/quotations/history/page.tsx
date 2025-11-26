"use client";
import { useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import { QuotationsApi, TemplatesApi } from "@/lib/api";
import { getAccessToken, getRoleFromToken } from "@/lib/session";

export default function QuotationsHistoryPage() {
  const router = useRouter();
  const [role, setRole] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [items, setItems] = useState<any[]>([]);
  const [templates, setTemplates] = useState<any[]>([]);

  // Filters
  const [templateId, setTemplateId] = useState<string>("");
  const [dateFrom, setDateFrom] = useState<string>("");
  const [dateTo, setDateTo] = useState<string>("");
  const [search, setSearch] = useState<string>("");

  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [totalCount, setTotalCount] = useState(0);

  useEffect(() => {
    const token = getAccessToken();
    const r = getRoleFromToken(token);
    setRole(r);
    if (!r) {
      router.replace("/login");
      return;
    }
    // Managers can view history; Admin/SalesRep too
    loadTemplates();
  }, [router]);

  useEffect(() => {
    loadData();
  }, [pageNumber, pageSize, templateId, dateFrom, dateTo, search]);

  const loadTemplates = async () => {
    try {
      const res = await TemplatesApi.list({ pageNumber: 1, pageSize: 100 });
      setTemplates(res.data.data || []);
    } catch {}
  };

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);
      // Reuse QuotationsApi.list with basic filters; templateId filter not exposed in API, so filter client-side for now
      const q = await QuotationsApi.list({ pageNumber, pageSize, dateFrom, dateTo });
      let data = q.data || [];
      // Client-side filter by templateId / search
      if (templateId) data = data.filter((x: any) => x.templateId === templateId);
      if (search) {
        const s = search.toLowerCase();
        data = data.filter((x: any) =>
          (x.quotationNumber || "").toLowerCase().includes(s) ||
          (x.clientName || "").toLowerCase().includes(s)
        );
      }
      setItems(data);
      setTotalCount(q.totalCount || data.length || 0);
    } catch (e: any) {
      setError(e?.message || "Failed to load history");
    } finally {
      setLoading(false);
    }
  };

  const templateOptions = useMemo(() => templates.map((t: any) => ({ id: t.templateId, name: t.name })), [templates]);

  return (
    <div className="rounded-sm border border-stroke bg-white px-5 pb-2.5 pt-6 shadow-default dark:border-strokedark dark:bg-boxdark sm:px-7.5 xl:pb-1">
      <div className="mb-6 flex items-center justify-between">
        <h4 className="text-title-md2 font-bold text-black dark:text-white">Quotations History</h4>
      </div>

      {/* Filters */}
      <div className="mb-4 grid grid-cols-1 gap-4 md:grid-cols-5">
        <div>
          <label className="mb-2.5 block text-black dark:text-white">Search</label>
          <input
            type="text"
            value={search}
            onChange={(e) => { setSearch(e.target.value); setPageNumber(1); }}
            placeholder="Quotation # or Client"
            className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 font-medium outline-none transition focus:border-primary active:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
          />
        </div>
        <div>
          <label className="mb-2.5 block text-black dark:text-white">Template</label>
          <select
            value={templateId}
            onChange={(e) => { setTemplateId(e.target.value); setPageNumber(1); }}
            className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 font-medium outline-none transition focus:border-primary active:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
          >
            <option value="">All</option>
            {templateOptions.map((t) => (
              <option key={t.id} value={t.id}>{t.name}</option>
            ))}
          </select>
        </div>
        <div>
          <label className="mb-2.5 block text-black dark:text-white">Date From</label>
          <input
            type="date"
            value={dateFrom}
            onChange={(e) => { setDateFrom(e.target.value); setPageNumber(1); }}
            className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 font-medium outline-none transition focus:border-primary active:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
          />
        </div>
        <div>
          <label className="mb-2.5 block text-black dark:text-white">Date To</label>
          <input
            type="date"
            value={dateTo}
            onChange={(e) => { setDateTo(e.target.value); setPageNumber(1); }}
            className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 font-medium outline-none transition focus:border-primary active:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
          />
        </div>
        <div className="flex items-end">
          <button
            onClick={() => { setTemplateId(""); setDateFrom(""); setDateTo(""); setSearch(""); setPageNumber(1); }}
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
        <div className="py-8 text-center">Loading...</div>
      ) : items.length === 0 ? (
        <div className="py-8 text-center text-gray-500">No quotations found</div>
      ) : (
        <div className="max-w-full overflow-x-auto">
          <table className="w-full table-auto">
            <thead>
              <tr className="bg-gray-2 text-left dark:bg-meta-4">
                <th className="px-4 py-3 font-medium text-black dark:text-white">Quotation #</th>
                <th className="px-4 py-3 font-medium text-black dark:text-white">Client</th>
                <th className="px-4 py-3 font-medium text-black dark:text-white">Template</th>
                <th className="px-4 py-3 font-medium text-black dark:text-white">Date</th>
                <th className="px-4 py-3 font-medium text-black dark:text-white">Status</th>
                <th className="px-4 py-3 font-medium text-black dark:text-white">Actions</th>
              </tr>
            </thead>
            <tbody>
              {items.map((q: any) => (
                <tr key={q.quotationId} className="border-b border-[#eee] dark:border-strokedark">
                  <td className="px-4 py-3 text-black dark:text-white">{q.quotationNumber}</td>
                  <td className="px-4 py-3 text-black dark:text-white">{q.clientName}</td>
                  <td className="px-4 py-3 text-black dark:text-white">{q.templateName || "-"}</td>
                  <td className="px-4 py-3 text-black dark:text-white">{q.quotationDate ? new Date(q.quotationDate).toLocaleDateString() : "-"}</td>
                  <td className="px-4 py-3 text-black dark:text-white">{q.status || "-"}</td>
                  <td className="px-4 py-3">
                    <button
                      onClick={() => router.push(`/quotations/${q.quotationId}`)}
                      className="rounded bg-primary px-3 py-1 text-xs text-white hover:bg-opacity-90"
                    >
                      View
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          <div className="mt-4 flex items-center justify-between">
            <div className="text-sm text-gray-500">
              Showing {(pageNumber - 1) * pageSize + 1} to {Math.min(pageNumber * pageSize, totalCount)} of {totalCount}
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
                disabled={pageNumber * pageSize >= totalCount}
                className="rounded border border-stroke px-4 py-2 text-sm disabled:opacity-50 dark:border-strokedark"
              >
                Next
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
