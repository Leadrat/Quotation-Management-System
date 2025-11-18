"use client";
import { useEffect, useState } from "react";
import { DiscountApprovalsApi } from "@/lib/api";
import { DiscountApproval, ApprovalStatus } from "@/types/discount-approvals";
import { ApprovalStatusBadge, ApprovalErrorBoundary, ApprovalListSkeleton } from "@/components/approvals";
import { formatCurrency, formatDateTime } from "@/utils/quotationFormatter";
import Button from "@/components/tailadmin/ui/button/Button";
import { ApprovalDecisionModal } from "@/components/approvals/ApprovalDecisionModal";

type TabType = "pending" | "approved" | "rejected" | "all";

export default function DiscountApprovalsDashboardPage() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [approvals, setApprovals] = useState<DiscountApproval[]>([]);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [total, setTotal] = useState(0);
  const [activeTab, setActiveTab] = useState<TabType>("pending");
  const [selectedApprovals, setSelectedApprovals] = useState<Set<string>>(new Set());
  
  // Filters
  const [statusFilter, setStatusFilter] = useState<ApprovalStatus | "">("");
  const [discountMin, setDiscountMin] = useState("");
  const [discountMax, setDiscountMax] = useState("");
  const [dateFrom, setDateFrom] = useState("");
  const [dateTo, setDateTo] = useState("");
  const [requestedByFilter, setRequestedByFilter] = useState("");

  // Modal states
  const [decisionModalOpen, setDecisionModalOpen] = useState(false);
  const [selectedApprovalForDecision, setSelectedApprovalForDecision] = useState<DiscountApproval | null>(null);
  const [decisionType, setDecisionType] = useState<"approve" | "reject" | null>(null);

  useEffect(() => {
    loadData();
  }, [pageNumber, pageSize, activeTab, statusFilter, discountMin, discountMax, dateFrom, dateTo, requestedByFilter]);

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);
      
      const status = activeTab === "pending" ? "Pending" : activeTab === "approved" ? "Approved" : activeTab === "rejected" ? "Rejected" : statusFilter || undefined;
      
      const params: any = {
        pageNumber,
        pageSize,
        status: status || undefined,
        discountPercentageMin: discountMin ? parseFloat(discountMin) : undefined,
        discountPercentageMax: discountMax ? parseFloat(discountMax) : undefined,
        dateFrom: dateFrom || undefined,
        dateTo: dateTo || undefined,
        requestedByUserId: requestedByFilter || undefined,
      };

      const result = await DiscountApprovalsApi.getPending(params);
      setApprovals(result.data.data || []);
      setTotal(result.data.totalCount || 0);
    } catch (err: any) {
      setError(err.message || "Failed to load approvals");
    } finally {
      setLoading(false);
    }
  };

  const handleApprove = (approval: DiscountApproval) => {
    setSelectedApprovalForDecision(approval);
    setDecisionType("approve");
    setDecisionModalOpen(true);
  };

  const handleReject = (approval: DiscountApproval) => {
    setSelectedApprovalForDecision(approval);
    setDecisionType("reject");
    setDecisionModalOpen(true);
  };

  const handleDecisionSubmit = async (reason: string, comments?: string) => {
    if (!selectedApprovalForDecision || !decisionType) return;

    try {
      if (decisionType === "approve") {
        await DiscountApprovalsApi.approve(selectedApprovalForDecision.approvalId, { reason, comments });
      } else {
        await DiscountApprovalsApi.reject(selectedApprovalForDecision.approvalId, { reason, comments });
      }
      setDecisionModalOpen(false);
      setSelectedApprovalForDecision(null);
      setDecisionType(null);
      await loadData();
    } catch (err: any) {
      alert(err.message || `Failed to ${decisionType} approval`);
    }
  };

  const handleBulkApprove = async () => {
    if (selectedApprovals.size === 0) return;
    const reason = prompt("Enter reason for bulk approval:");
    if (!reason || reason.trim().length < 10) {
      alert("Reason must be at least 10 characters.");
      return;
    }

    try {
      await DiscountApprovalsApi.bulkApprove({
        approvalIds: Array.from(selectedApprovals),
        reason: reason.trim(),
      });
      setSelectedApprovals(new Set());
      await loadData();
    } catch (err: any) {
      alert(err.message || "Failed to bulk approve");
    }
  };

  const toggleSelection = (approvalId: string) => {
    const newSelection = new Set(selectedApprovals);
    if (newSelection.has(approvalId)) {
      newSelection.delete(approvalId);
    } else {
      newSelection.add(approvalId);
    }
    setSelectedApprovals(newSelection);
  };

  const toggleSelectAll = () => {
    if (selectedApprovals.size === approvals.length) {
      setSelectedApprovals(new Set());
    } else {
      setSelectedApprovals(new Set(approvals.map(a => a.approvalId)));
    }
  };

  return (
    <ApprovalErrorBoundary>
      <div className="rounded-sm border border-stroke bg-white px-5 pb-2.5 pt-6 shadow-default dark:border-strokedark dark:bg-boxdark sm:px-7.5 xl:pb-1">
        <div className="mb-6">
          <h4 className="text-title-md2 font-bold text-black dark:text-white">Discount Approval Dashboard</h4>
          <p className="mt-1 text-sm text-gray-600 dark:text-gray-400">
            Review and manage discount approval requests
          </p>
        </div>

      {/* Tabs */}
      <div className="mb-6 border-b border-stroke dark:border-strokedark">
        <div className="flex gap-4">
          {(["pending", "approved", "rejected", "all"] as TabType[]).map((tab) => (
            <button
              key={tab}
              onClick={() => {
                setActiveTab(tab);
                setPageNumber(1);
                setSelectedApprovals(new Set());
              }}
              className={`px-4 py-2 font-medium text-sm border-b-2 transition-colors ${
                activeTab === tab
                  ? "border-primary text-primary"
                  : "border-transparent text-gray-600 hover:text-gray-900 dark:text-gray-400 dark:hover:text-white"
              }`}
            >
              {tab.charAt(0).toUpperCase() + tab.slice(1)}
            </button>
          ))}
        </div>
      </div>

      {/* Filters */}
      <div className="mb-4 grid grid-cols-1 gap-4 md:grid-cols-5">
        <div>
          <label className="mb-2.5 block text-sm text-black dark:text-white">Discount % Min</label>
          <input
            type="number"
            value={discountMin}
            onChange={(e) => {
              setDiscountMin(e.target.value);
              setPageNumber(1);
            }}
            placeholder="0"
            className="w-full rounded border-[1.5px] border-stroke bg-transparent px-3 py-2 text-sm outline-none transition focus:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
          />
        </div>
        <div>
          <label className="mb-2.5 block text-sm text-black dark:text-white">Discount % Max</label>
          <input
            type="number"
            value={discountMax}
            onChange={(e) => {
              setDiscountMax(e.target.value);
              setPageNumber(1);
            }}
            placeholder="100"
            className="w-full rounded border-[1.5px] border-stroke bg-transparent px-3 py-2 text-sm outline-none transition focus:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
          />
        </div>
        <div>
          <label className="mb-2.5 block text-sm text-black dark:text-white">Date From</label>
          <input
            type="date"
            value={dateFrom}
            onChange={(e) => {
              setDateFrom(e.target.value);
              setPageNumber(1);
            }}
            className="w-full rounded border-[1.5px] border-stroke bg-transparent px-3 py-2 text-sm outline-none transition focus:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
          />
        </div>
        <div>
          <label className="mb-2.5 block text-sm text-black dark:text-white">Date To</label>
          <input
            type="date"
            value={dateTo}
            onChange={(e) => {
              setDateTo(e.target.value);
              setPageNumber(1);
            }}
            className="w-full rounded border-[1.5px] border-stroke bg-transparent px-3 py-2 text-sm outline-none transition focus:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
          />
        </div>
        <div>
          <label className="mb-2.5 block text-sm text-black dark:text-white">Status (All tab)</label>
          <select
            value={statusFilter}
            onChange={(e) => {
              setStatusFilter(e.target.value as ApprovalStatus | "");
              setPageNumber(1);
            }}
            className="w-full rounded border-[1.5px] border-stroke bg-transparent px-3 py-2 text-sm outline-none transition focus:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
          >
            <option value="">All</option>
            <option value="Pending">Pending</option>
            <option value="Approved">Approved</option>
            <option value="Rejected">Rejected</option>
          </select>
        </div>
      </div>

      {/* Bulk Actions */}
      {activeTab === "pending" && selectedApprovals.size > 0 && (
        <div className="mb-4 flex items-center justify-between rounded-lg bg-blue-50 p-3 dark:bg-blue-900/20">
          <span className="text-sm text-blue-800 dark:text-blue-300">
            {selectedApprovals.size} approval(s) selected
          </span>
          <Button size="sm" onClick={handleBulkApprove}>
            Bulk Approve Selected
          </Button>
        </div>
      )}

      {error && (
        <div className="mb-4 rounded-md bg-red-50 p-3 text-sm text-red-800 dark:bg-red-900/20 dark:text-red-300">
          {error}
        </div>
      )}

      {/* Table */}
      {loading ? (
        <ApprovalListSkeleton />
      ) : approvals.length === 0 ? (
        <div className="py-8 text-center text-gray-500">No approvals found.</div>
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full table-auto">
            <thead>
              <tr className="bg-gray-2 text-left dark:bg-meta-4">
                {activeTab === "pending" && (
                  <th className="px-4 py-3">
                    <input
                      type="checkbox"
                      checked={selectedApprovals.size === approvals.length && approvals.length > 0}
                      onChange={toggleSelectAll}
                      className="rounded border-gray-300"
                    />
                  </th>
                )}
                <th className="px-4 py-3 font-medium text-black dark:text-white">Quotation #</th>
                <th className="px-4 py-3 font-medium text-black dark:text-white">Client</th>
                <th className="px-4 py-3 font-medium text-black dark:text-white">Discount %</th>
                <th className="px-4 py-3 font-medium text-black dark:text-white">Sales Rep</th>
                <th className="px-4 py-3 font-medium text-black dark:text-white">Level</th>
                <th className="px-4 py-3 font-medium text-black dark:text-white">Status</th>
                <th className="px-4 py-3 font-medium text-black dark:text-white">Requested</th>
                <th className="px-4 py-3 font-medium text-black dark:text-white">Actions</th>
              </tr>
            </thead>
            <tbody>
              {approvals.map((approval) => (
                <tr key={approval.approvalId} className="border-b border-[#eee] dark:border-strokedark">
                  {activeTab === "pending" && (
                    <td className="px-4 py-3">
                      <input
                        type="checkbox"
                        checked={selectedApprovals.has(approval.approvalId)}
                        onChange={() => toggleSelection(approval.approvalId)}
                        className="rounded border-gray-300"
                      />
                    </td>
                  )}
                  <td className="px-4 py-3 text-black dark:text-white">{approval.quotationNumber}</td>
                  <td className="px-4 py-3 text-black dark:text-white">{approval.clientName}</td>
                  <td className="px-4 py-3 text-black dark:text-white font-medium">
                    {approval.currentDiscountPercentage}%
                  </td>
                  <td className="px-4 py-3 text-gray-600 dark:text-gray-400">{approval.requestedByUserName}</td>
                  <td className="px-4 py-3 text-gray-600 dark:text-gray-400">{approval.approvalLevel}</td>
                  <td className="px-4 py-3">
                    <ApprovalStatusBadge status={approval.status} />
                  </td>
                  <td className="px-4 py-3 text-sm text-gray-600 dark:text-gray-400">
                    {formatDateTime(approval.requestDate)}
                  </td>
                  <td className="px-4 py-3">
                    {approval.status === "Pending" && (
                      <div className="flex gap-2">
                        <button
                          onClick={() => handleApprove(approval)}
                          className="rounded bg-green-500 px-3 py-1 text-xs text-white hover:bg-opacity-90"
                        >
                          Approve
                        </button>
                        <button
                          onClick={() => handleReject(approval)}
                          className="rounded bg-red-500 px-3 py-1 text-xs text-white hover:bg-opacity-90"
                        >
                          Reject
                        </button>
                      </div>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Pagination */}
      {total > 0 && (
        <div className="mt-4 flex items-center justify-between">
          <div className="text-sm text-gray-600 dark:text-gray-400">
            Showing {(pageNumber - 1) * pageSize + 1} to {Math.min(pageNumber * pageSize, total)} of {total} approvals
          </div>
          <div className="flex gap-2">
            <button
              onClick={() => setPageNumber(p => Math.max(1, p - 1))}
              disabled={pageNumber === 1}
              className="rounded border border-stroke px-3 py-1 text-sm disabled:opacity-50 dark:border-strokedark"
            >
              Previous
            </button>
            <button
              onClick={() => setPageNumber(p => p + 1)}
              disabled={pageNumber * pageSize >= total}
              className="rounded border border-stroke px-3 py-1 text-sm disabled:opacity-50 dark:border-strokedark"
            >
              Next
            </button>
          </div>
        </div>
      )}

      {/* Decision Modal */}
      {decisionModalOpen && selectedApprovalForDecision && decisionType && (
        <ApprovalDecisionModal
          isOpen={decisionModalOpen}
          onClose={() => {
            setDecisionModalOpen(false);
            setSelectedApprovalForDecision(null);
            setDecisionType(null);
          }}
          approval={selectedApprovalForDecision}
          action={decisionType}
          onSubmit={handleDecisionSubmit}
        />
      )}
      </div>
    </ApprovalErrorBoundary>
  );
}

