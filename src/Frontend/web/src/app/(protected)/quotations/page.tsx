"use client";
import { useEffect, useState } from "react";
import Link from "next/link";
import { QuotationsApi, DiscountApprovalsApi } from "@/lib/api";
import { getAccessToken } from "@/lib/session";
import { formatCurrency, formatDate, getStatusColor, getStatusLabel } from "@/utils/quotationFormatter";
import { QuotationListSkeleton } from "@/components/quotations/LoadingSkeleton";
import { QuotationErrorBoundary } from "@/components/quotations/ErrorBoundary";
import { useToast, ToastContainer } from "@/components/quotations/Toast";
import { ApprovalStatusBadge } from "@/components/approvals";
import { DiscountApproval } from "@/types/discount-approvals";

export default function QuotationsListPage() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [items, setItems] = useState<any[]>([]);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [total, setTotal] = useState(0);
  const [approvalMap, setApprovalMap] = useState<Record<string, DiscountApproval>>({});
  const toast = useToast();

  // Filters
  const [statusFilter, setStatusFilter] = useState("");
  const [clientIdFilter, setClientIdFilter] = useState("");
  const [dateFrom, setDateFrom] = useState("");
  const [dateTo, setDateTo] = useState("");

  const loadData = async () => {
    // Only load if authenticated
    if (!getAccessToken()) {
      setLoading(false);
      setItems([]);
      setTotal(0);
      return;
    }
    try {
      setLoading(true);
      setError(null);
      const params: any = {
        pageNumber,
        pageSize,
      };
      if (statusFilter) params.status = statusFilter;
      if (clientIdFilter) params.clientId = clientIdFilter;
      if (dateFrom) params.dateFrom = dateFrom;
      if (dateTo) params.dateTo = dateTo;

      const result = await QuotationsApi.list(params);
      const quotations = result.data || [];
      setItems(quotations);
      setTotal(result.totalCount || 0);
      setError(null);

      // Load approval status for each quotation
      const approvalPromises = quotations.map(async (q: any) => {
        try {
          const approvalsRes = await DiscountApprovalsApi.getQuotationApprovals(q.quotationId);
          const approvals = approvalsRes.data || [];
          const pending = approvals.find((a: DiscountApproval) => a.status === "Pending");
          return { quotationId: q.quotationId, approval: pending || null };
        } catch {
          return { quotationId: q.quotationId, approval: null };
        }
      });

      const approvalResults = await Promise.all(approvalPromises);
      const newApprovalMap: Record<string, DiscountApproval> = {};
      approvalResults.forEach(({ quotationId, approval }) => {
        if (approval) {
          newApprovalMap[quotationId] = approval;
        }
      });
      setApprovalMap(newApprovalMap);
    } catch (err: any) {
      // Silently ignore 401 errors
      if (err?.message?.includes("401")) {
        setItems([]);
        setTotal(0);
        setApprovalMap({});
        return;
      }
      const errorMsg = err.message || "Failed to load quotations";
      setError(errorMsg);
      toast.error(errorMsg);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, [pageNumber, pageSize, statusFilter, clientIdFilter, dateFrom, dateTo]);

  const handleDelete = async (id: string) => {
    if (!confirm("Are you sure you want to delete this quotation?")) return;
    try {
      toast.info("Deleting quotation...");
      const result = await QuotationsApi.delete(id);
      if (result.success) {
        toast.success("Quotation deleted successfully");
        await loadData();
      } else {
        toast.error(result.message || "Failed to delete quotation");
      }
    } catch (err: any) {
      toast.error(err.message || "Failed to delete quotation");
    }
  };

  return (
    <QuotationErrorBoundary>
      <div className="rounded-sm border border-stroke bg-white px-5 pb-2.5 pt-6 shadow-default dark:border-strokedark dark:bg-boxdark sm:px-7.5 xl:pb-1">
        <ToastContainer toasts={toast.toasts} onRemove={toast.removeToast} />
      <div className="mb-6 flex items-center justify-between">
        <h4 className="text-title-md2 font-bold text-black dark:text-white">Quotations</h4>
        <Link
          href="/quotations/new"
          className="inline-flex items-center justify-center rounded-md bg-primary px-6 py-2.5 text-center font-medium text-white hover:bg-opacity-90"
        >
          Create Quotation
        </Link>
      </div>

      {/* Filters */}
      <div className="mb-4 grid grid-cols-1 gap-4 md:grid-cols-4">
        <div>
          <label className="mb-2.5 block text-black dark:text-white">Status</label>
          <select
            value={statusFilter}
            onChange={(e) => {
              setStatusFilter(e.target.value);
              setPageNumber(1);
            }}
            className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 font-medium outline-none transition focus:border-primary active:border-primary disabled:cursor-default disabled:bg-whiter dark:border-form-strokedark dark:bg-form-input dark:text-white dark:focus:border-primary"
          >
            <option value="">All Statuses</option>
            <option value="DRAFT">Draft</option>
            <option value="SENT">Sent</option>
            <option value="VIEWED">Viewed</option>
            <option value="ACCEPTED">Accepted</option>
            <option value="REJECTED">Rejected</option>
            <option value="EXPIRED">Expired</option>
            <option value="CANCELLED">Cancelled</option>
          </select>
        </div>
        <div>
          <label className="mb-2.5 block text-black dark:text-white">Date From</label>
          <input
            type="date"
            value={dateFrom}
            onChange={(e) => {
              setDateFrom(e.target.value);
              setPageNumber(1);
            }}
            className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 font-medium outline-none transition focus:border-primary active:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
          />
        </div>
        <div>
          <label className="mb-2.5 block text-black dark:text-white">Date To</label>
          <input
            type="date"
            value={dateTo}
            onChange={(e) => {
              setDateTo(e.target.value);
              setPageNumber(1);
            }}
            className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 font-medium outline-none transition focus:border-primary active:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
          />
        </div>
        <div>
          <label className="mb-2.5 block text-black dark:text-white">Client ID</label>
          <input
            type="text"
            value={clientIdFilter}
            onChange={(e) => {
              setClientIdFilter(e.target.value);
              setPageNumber(1);
            }}
            placeholder="Filter by Client ID"
            className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 font-medium outline-none transition focus:border-primary active:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
          />
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
        <>
          <div className="max-w-full overflow-x-auto">
            <table className="w-full table-auto">
              <thead>
                <tr className="bg-gray-2 text-left dark:bg-meta-4">
                  <th className="min-w-[120px] px-4 py-4 font-medium text-black dark:text-white">Quotation #</th>
                  <th className="min-w-[150px] px-4 py-4 font-medium text-black dark:text-white">Client</th>
                  <th className="min-w-[100px] px-4 py-4 font-medium text-black dark:text-white">Status</th>
                  <th className="min-w-[120px] px-4 py-4 font-medium text-black dark:text-white">Approval</th>
                  <th className="min-w-[120px] px-4 py-4 font-medium text-black dark:text-white">Date</th>
                  <th className="min-w-[120px] px-4 py-4 font-medium text-black dark:text-white">Total Amount</th>
                  <th className="min-w-[150px] px-4 py-4 font-medium text-black dark:text-white">Actions</th>
                </tr>
              </thead>
              <tbody>
                {items.map((item) => (
                  <tr key={item.quotationId} className="border-b border-[#eee] dark:border-strokedark">
                    <td className="px-4 py-5 dark:border-strokedark">
                      <Link href={`/quotations/${item.quotationId}`} className="text-primary hover:underline">
                        {item.quotationNumber}
                      </Link>
                    </td>
                    <td className="px-4 py-5 dark:border-strokedark">
                      <p className="text-black dark:text-white">{item.clientName}</p>
                      <p className="text-sm text-gray-500">{item.clientEmail}</p>
                    </td>
                    <td className="px-4 py-5 dark:border-strokedark">
                      <span className={`inline-flex rounded-full px-3 py-1 text-xs font-medium ${getStatusColor(item.status)}`}>
                        {getStatusLabel(item.status)}
                      </span>
                    </td>
                    <td className="px-4 py-5 dark:border-strokedark">
                      {approvalMap[item.quotationId] ? (
                        <ApprovalStatusBadge status={approvalMap[item.quotationId].status} />
                      ) : (
                        <span className="text-xs text-gray-400">-</span>
                      )}
                    </td>
                    <td className="px-4 py-5 dark:border-strokedark">
                      <p className="text-black dark:text-white">{formatDate(item.quotationDate)}</p>
                    </td>
                    <td className="px-4 py-5 dark:border-strokedark">
                      <p className="font-medium text-black dark:text-white">{formatCurrency(item.totalAmount)}</p>
                    </td>
                    <td className="px-4 py-5 dark:border-strokedark">
                      <div className="flex items-center gap-2">
                        <Link
                          href={`/quotations/${item.quotationId}`}
                          className="rounded bg-primary px-3 py-1 text-xs text-white hover:bg-opacity-90"
                        >
                          View
                        </Link>
                        {item.status === "DRAFT" && (
                          <>
                            <Link
                              href={`/quotations/${item.quotationId}/edit`}
                              className="rounded bg-yellow-500 px-3 py-1 text-xs text-white hover:bg-opacity-90"
                            >
                              Edit
                            </Link>
                            <button
                              onClick={() => handleDelete(item.quotationId)}
                              className="rounded bg-red-500 px-3 py-1 text-xs text-white hover:bg-opacity-90"
                            >
                              Delete
                            </button>
                          </>
                        )}
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
              Showing {(pageNumber - 1) * pageSize + 1} to {Math.min(pageNumber * pageSize, total)} of {total} quotations
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
    </QuotationErrorBoundary>
  );
}

