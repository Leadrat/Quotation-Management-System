"use client";
import { useEffect, useState } from "react";
import Link from "next/link";
import { QuotationsApi, DiscountApprovalsApi } from "@/lib/api";
import { getAccessToken, getRoleFromToken } from "@/lib/session";
import { formatCurrency, formatDate, getStatusColor, getStatusLabel } from "@/utils/quotationFormatter";
import { QuotationListSkeleton } from "@/components/quotations/LoadingSkeleton";
import { QuotationErrorBoundary } from "@/components/quotations/ErrorBoundary";
import { useToast, ToastContainer } from "@/components/quotations/Toast";
import { ApprovalStatusBadge } from "@/components/approvals";
import { DiscountApproval } from "@/types/discount-approvals";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import Input from "@/components/tailadmin/form/input/InputField";
import Label from "@/components/tailadmin/form/Label";
import Button from "@/components/tailadmin/ui/button/Button";
import { Table, TableBody, TableCell, TableHeader, TableRow } from "@/components/tailadmin/ui/table";
import Pagination from "@/components/tailadmin/tables/Pagination";
import Alert from "@/components/tailadmin/ui/alert/Alert";
import Badge from "@/components/tailadmin/ui/badge/Badge";

export default function QuotationsListPage() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [items, setItems] = useState<any[]>([]);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [total, setTotal] = useState(0);
  const [approvalMap, setApprovalMap] = useState<Record<string, DiscountApproval>>({});
  const [role, setRole] = useState<string | null>(null);
  const toast = useToast();

  useEffect(() => {
    const token = getAccessToken();
    const userRole = getRoleFromToken(token);
    setRole(userRole);
  }, []);

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

  const totalPages = Math.max(1, Math.ceil(total / pageSize));

  const getStatusBadgeColor = (status: string) => {
    switch (status) {
      case "ACCEPTED": return "success";
      case "REJECTED": return "error";
      case "DRAFT": return "warning";
      case "SENT": return "primary";
      case "VIEWED": return "primary";
      case "EXPIRED": return "error";
      case "CANCELLED": return "error";
      default: return "primary";
    }
  };

  return (
    <QuotationErrorBoundary>
      <ToastContainer toasts={toast.toasts} onRemove={toast.removeToast} />
      <PageBreadcrumb pageTitle="Quotations" />
      
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-xl font-semibold text-gray-800 dark:text-white/90">Quotations</h2>
        {role !== "Admin" && (
          <Link href="/quotations/new">
            <Button size="sm" variant="outline" className="!text-black dark:!text-white">Create Quotation</Button>
          </Link>
        )}
      </div>

      {/* Filters */}
      <div className="mb-6 grid grid-cols-1 gap-4 md:grid-cols-4">
        <div>
          <Label>Status</Label>
          <select
            value={statusFilter}
            onChange={(e) => {
              setStatusFilter(e.target.value);
              setPageNumber(1);
            }}
            className="h-11 w-full appearance-none rounded-lg border border-gray-300 px-4 py-2.5 pr-11 text-sm shadow-theme-xs focus:border-brand-300 focus:outline-hidden focus:ring-3 focus:ring-brand-500/10 dark:border-gray-700 dark:bg-gray-900 dark:text-white/90"
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
          <Label>Client ID</Label>
          <Input
            type="text"
            value={clientIdFilter}
            onChange={(e) => {
              setClientIdFilter(e.target.value);
              setPageNumber(1);
            }}
            placeholder="Filter by Client ID"
          />
        </div>
      </div>

      {error && <Alert className="mb-4" variant="error" title="Error" message={error} />}

      {loading ? (
        <div className="rounded-xl border border-gray-200 bg-white dark:border-white/[0.05] dark:bg-white/[0.03] p-8 text-center">
          <div className="text-gray-500 dark:text-gray-400">Loading...</div>
        </div>
      ) : items.length === 0 ? (
        <div className="rounded-xl border border-gray-200 bg-white dark:border-white/[0.05] dark:bg-white/[0.03] p-8 text-center">
          <div className="text-gray-500 dark:text-gray-400">No quotations found</div>
        </div>
      ) : (
        <>
          <div className="overflow-hidden rounded-xl border border-gray-200 bg-white dark:border-white/[0.05] dark:bg-white/[0.03]">
            <div className="max-w-full overflow-x-auto">
              <Table>
                <TableHeader className="border-b border-gray-100 dark:border-white/[0.05]">
                  <TableRow>
                    <TableCell isHeader className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400">Quotation #</TableCell>
                    <TableCell isHeader className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400">Client</TableCell>
                    <TableCell isHeader className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400">Status</TableCell>
                    <TableCell isHeader className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400">Approval</TableCell>
                    <TableCell isHeader className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400">Date</TableCell>
                    <TableCell isHeader className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400">Total Amount</TableCell>
                    <TableCell isHeader className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400">Actions</TableCell>
                  </TableRow>
                </TableHeader>
                <TableBody className="divide-y divide-gray-100 dark:divide-white/[0.05]">
                  {items.map((item) => (
                    <TableRow key={item.quotationId}>
                      <TableCell className="px-5 py-4">
                        <Link href={`/quotations/${item.quotationId}`} className="text-brand-500 hover:text-brand-600 text-theme-sm">
                          {item.quotationNumber}
                        </Link>
                      </TableCell>
                      <TableCell className="px-5 py-4">
                        <p className="text-gray-800 text-theme-sm dark:text-white/90">{item.clientName}</p>
                        <p className="text-gray-500 text-theme-xs dark:text-gray-400">{item.clientEmail}</p>
                      </TableCell>
                      <TableCell className="px-5 py-4">
                        <Badge size="sm" color={getStatusBadgeColor(item.status)}>
                          {getStatusLabel(item.status)}
                        </Badge>
                      </TableCell>
                      <TableCell className="px-5 py-4">
                        {approvalMap[item.quotationId] ? (
                          <ApprovalStatusBadge status={approvalMap[item.quotationId].status} />
                        ) : (
                          <span className="text-xs text-gray-400">-</span>
                        )}
                      </TableCell>
                      <TableCell className="px-5 py-4 text-gray-500 text-theme-sm dark:text-gray-400">
                        {formatDate(item.quotationDate)}
                      </TableCell>
                      <TableCell className="px-5 py-4 text-gray-800 text-theme-sm dark:text-white/90 font-medium">
                        {formatCurrency(item.totalAmount)}
                      </TableCell>
                      <TableCell className="px-5 py-4">
                        <div className="flex items-center gap-2">
                          <Link href={`/quotations/${item.quotationId}`}>
                            <Button size="sm" variant="outline" className="text-xs px-2 py-1">View</Button>
                          </Link>
                          {item.status === "DRAFT" && role !== "Admin" && (
                            <>
                              <Link href={`/quotations/${item.quotationId}/edit`}>
                                <Button size="sm" variant="outline" className="text-xs px-2 py-1">Edit</Button>
                              </Link>
                              <Button 
                                size="sm" 
                                variant="outline" 
                                className="text-xs px-2 py-1 text-error-500 hover:text-error-600"
                                onClick={() => handleDelete(item.quotationId)}
                              >
                                Delete
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

          {/* Pagination */}
          <div className="mt-4 flex items-center justify-between">
            <div className="text-sm text-gray-500 dark:text-gray-400">
              Showing {(pageNumber - 1) * pageSize + 1} to {Math.min(pageNumber * pageSize, total)} of {total} quotations
            </div>
            <Pagination 
              currentPage={pageNumber} 
              totalPages={totalPages} 
              onPageChange={(p) => setPageNumber(p)} 
            />
          </div>
        </>
      )}
    </QuotationErrorBoundary>
  );
}

