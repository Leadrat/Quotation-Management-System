"use client";
import { useEffect, useState } from "react";
import { DiscountApprovalsApi } from "@/lib/api";
import { DiscountApproval, ApprovalStatus } from "@/types/discount-approvals";
import { ApprovalStatusBadge, ApprovalErrorBoundary, ApprovalListSkeleton, ApprovalDetailModal } from "@/components/approvals";
import { formatCurrency, formatDateTime } from "@/utils/quotationFormatter";
import Button from "@/components/tailadmin/ui/button/Button";
import { ApprovalDecisionModal } from "@/components/approvals/ApprovalDecisionModal";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import Input from "@/components/tailadmin/form/input/InputField";
import Label from "@/components/tailadmin/form/Label";
import { Table, TableBody, TableCell, TableHeader, TableRow } from "@/components/tailadmin/ui/table";
import Pagination from "@/components/tailadmin/tables/Pagination";
import Alert from "@/components/tailadmin/ui/alert/Alert";
import Checkbox from "@/components/tailadmin/form/input/Checkbox";

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
  const [detailModalOpen, setDetailModalOpen] = useState(false);
  const [selectedApprovalIdForDetail, setSelectedApprovalIdForDetail] = useState<string | null>(null);

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

      console.log("Loading approvals with params:", params);
      const result = await DiscountApprovalsApi.getPending(params);
      console.log("Approvals API response:", result);
      console.log("Approvals data:", result.data?.data);
      console.log("Total count:", result.data?.totalCount);
      
      setApprovals(result.data?.data || []);
      setTotal(result.data?.totalCount || 0);
    } catch (err: any) {
      console.error("Error loading approvals:", err);
      setError(err.message || "Failed to load approvals");
    } finally {
      setLoading(false);
    }
  };

  const handleApprove = (approval: DiscountApproval) => {
    console.log("Approve button clicked for approval:", approval.approvalId);
    setSelectedApprovalForDecision(approval);
    setDecisionType("approve");
    setDecisionModalOpen(true);
  };

  const handleReject = (approval: DiscountApproval) => {
    console.log("Reject button clicked for approval:", approval.approvalId);
    setSelectedApprovalForDecision(approval);
    setDecisionType("reject");
    setDecisionModalOpen(true);
  };

  const handleView = (approval: DiscountApproval) => {
    console.log("View button clicked for approval:", approval.approvalId);
    setSelectedApprovalIdForDetail(approval.approvalId);
    setDetailModalOpen(true);
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

  const totalPages = Math.max(1, Math.ceil(total / pageSize));

  return (
    <ApprovalErrorBoundary>
      <PageBreadcrumb pageTitle="Discount Approvals" />
      
      <div className="mb-6">
        <h2 className="text-xl font-semibold text-gray-800 dark:text-white/90">Discount Approval Dashboard</h2>
        <p className="text-gray-500 dark:text-gray-400 mt-1">
          Review and manage discount approval requests
        </p>
      </div>

      {/* Tabs */}
      <div className="mb-6 border-b border-gray-200 dark:border-gray-800">
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
                  ? "border-brand-500 text-brand-500"
                  : "border-transparent text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-white/90"
              }`}
            >
              {tab.charAt(0).toUpperCase() + tab.slice(1)}
            </button>
          ))}
        </div>
      </div>

      {/* Filters */}
      <div className="mb-6 grid grid-cols-1 gap-4 md:grid-cols-5">
        <div>
          <Label>Discount % Min</Label>
          <Input
            type="number"
            value={discountMin}
            onChange={(e) => {
              setDiscountMin(e.target.value);
              setPageNumber(1);
            }}
            placeholder="0"
          />
        </div>
        <div>
          <Label>Discount % Max</Label>
          <Input
            type="number"
            value={discountMax}
            onChange={(e) => {
              setDiscountMax(e.target.value);
              setPageNumber(1);
            }}
            placeholder="100"
          />
        </div>
        <div>
          <Label>Date From</Label>
          <Input
            type="date"
            value={dateFrom}
            onChange={(e) => {
              setDateFrom(e.target.value);
              setPageNumber(1);
            }}
          />
        </div>
        <div>
          <Label>Date To</Label>
          <Input
            type="date"
            value={dateTo}
            onChange={(e) => {
              setDateTo(e.target.value);
              setPageNumber(1);
            }}
          />
        </div>
        <div>
          <Label>Status (All tab)</Label>
          <select
            value={statusFilter}
            onChange={(e) => {
              setStatusFilter(e.target.value as ApprovalStatus | "");
              setPageNumber(1);
            }}
            className="h-11 w-full appearance-none rounded-lg border border-gray-300 px-4 py-2.5 pr-11 text-sm shadow-theme-xs focus:border-brand-300 focus:outline-hidden focus:ring-3 focus:ring-brand-500/10 dark:border-gray-700 dark:bg-gray-900 dark:text-white/90"
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
        <div className="mb-4 rounded-xl border border-blue-light-500 bg-blue-light-50 dark:border-blue-light-500/30 dark:bg-blue-light-500/15 p-4">
          <div className="flex items-center justify-between">
            <span className="text-sm text-blue-light-800 dark:text-blue-light-300">
              {selectedApprovals.size} approval(s) selected
            </span>
            <Button size="sm" onClick={handleBulkApprove}>
              Bulk Approve Selected
            </Button>
          </div>
        </div>
      )}

      {error && <Alert className="mb-4" variant="error" title="Error" message={error} />}

      {/* Table */}
      {loading ? (
        <ApprovalListSkeleton />
      ) : approvals.length === 0 ? (
        <div className="rounded-xl border border-gray-200 bg-white dark:border-white/[0.05] dark:bg-white/[0.03] p-8 text-center">
          <div className="text-gray-500 dark:text-gray-400">No approvals found.</div>
        </div>
      ) : (
        <div className="overflow-hidden rounded-xl border border-gray-200 bg-white dark:border-white/[0.05] dark:bg-white/[0.03]">
          <div className="max-w-full overflow-x-auto">
            <Table>
              <TableHeader className="border-b border-gray-100 dark:border-white/[0.05]">
                <TableRow>
                  {activeTab === "pending" && (
                    <TableCell isHeader className="px-5 py-3">
                      <Checkbox
                        checked={selectedApprovals.size === approvals.length && approvals.length > 0}
                        onChange={toggleSelectAll}
                      />
                    </TableCell>
                  )}
                  <TableCell isHeader className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400">Quotation #</TableCell>
                  <TableCell isHeader className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400">Client</TableCell>
                  <TableCell isHeader className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400">Discount %</TableCell>
                  <TableCell isHeader className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400">Sales Rep</TableCell>
                  <TableCell isHeader className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400">Level</TableCell>
                  <TableCell isHeader className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400">Status</TableCell>
                  <TableCell isHeader className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400">Requested</TableCell>
                  <TableCell isHeader className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400">Actions</TableCell>
                </TableRow>
              </TableHeader>
              <TableBody className="divide-y divide-gray-100 dark:divide-white/[0.05]">
                {approvals.map((approval) => (
                  <TableRow key={approval.approvalId}>
                    {activeTab === "pending" && (
                      <TableCell className="px-5 py-4">
                        <Checkbox
                          checked={selectedApprovals.has(approval.approvalId)}
                          onChange={() => toggleSelection(approval.approvalId)}
                        />
                      </TableCell>
                    )}
                    <TableCell className="px-5 py-4 text-gray-800 text-theme-sm dark:text-white/90">{approval.quotationNumber}</TableCell>
                    <TableCell className="px-5 py-4 text-gray-800 text-theme-sm dark:text-white/90">{approval.clientName}</TableCell>
                    <TableCell className="px-5 py-4 text-gray-800 text-theme-sm dark:text-white/90 font-medium">
                      {approval.currentDiscountPercentage}%
                    </TableCell>
                    <TableCell className="px-5 py-4 text-gray-500 text-theme-sm dark:text-gray-400">{approval.requestedByUserName}</TableCell>
                    <TableCell className="px-5 py-4 text-gray-500 text-theme-sm dark:text-gray-400">{approval.approvalLevel}</TableCell>
                    <TableCell className="px-5 py-4">
                      <ApprovalStatusBadge status={approval.status} />
                    </TableCell>
                    <TableCell className="px-5 py-4 text-gray-500 text-theme-sm dark:text-gray-400">
                      {formatDateTime(approval.requestDate)}
                    </TableCell>
                    <TableCell className="px-5 py-4">
                      <div className="flex gap-2 flex-wrap items-center">
                        <Button 
                          size="sm" 
                          variant="outline" 
                          className="text-xs px-2 py-1 text-blue-600 hover:text-blue-700 hover:bg-blue-50 dark:hover:bg-blue-900/20 cursor-pointer !pointer-events-auto" 
                          onClick={() => handleView(approval)}
                        >
                          View
                        </Button>
                        {approval.status === "Pending" && (
                          <>
                            <Button 
                              size="sm" 
                              variant="outline" 
                              className="text-xs px-2 py-1 text-success-600 hover:text-success-700 hover:bg-success-50 dark:hover:bg-success-900/20 cursor-pointer !pointer-events-auto" 
                              onClick={() => handleApprove(approval)}
                            >
                              Approve
                            </Button>
                            <Button 
                              size="sm" 
                              variant="outline" 
                              className="text-xs px-2 py-1 text-error-500 hover:text-error-600 hover:bg-error-50 dark:hover:bg-error-900/20 cursor-pointer !pointer-events-auto" 
                              onClick={() => handleReject(approval)}
                            >
                              Reject
                            </Button>
                          </>
                        )}
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        </div>
      )}

      {/* Pagination */}
      {total > 0 && (
        <div className="mt-4 flex items-center justify-between">
          <div className="text-sm text-gray-500 dark:text-gray-400">
            Showing {(pageNumber - 1) * pageSize + 1} to {Math.min(pageNumber * pageSize, total)} of {total} approvals
          </div>
          <Pagination 
            currentPage={pageNumber} 
            totalPages={totalPages} 
            onPageChange={(p) => setPageNumber(p)} 
          />
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

      {/* Detail Modal */}
      {detailModalOpen && selectedApprovalIdForDetail && (
        <ApprovalDetailModal
          isOpen={detailModalOpen}
          onClose={() => {
            setDetailModalOpen(false);
            setSelectedApprovalIdForDetail(null);
          }}
          approvalId={selectedApprovalIdForDetail}
        />
      )}
    </ApprovalErrorBoundary>
  );
}

