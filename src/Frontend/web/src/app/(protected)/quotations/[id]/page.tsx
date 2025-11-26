 "use client";
import { useEffect, useMemo, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import { QuotationsApi, PaymentsApi } from "@/lib/api";
import { getAccessToken, getRoleFromToken } from "@/lib/session";
import { formatCurrency, formatDate, formatDateTime, getStatusColor, getStatusLabel } from "@/utils/quotationFormatter";
import { ClientResponseCard, QuotationStatusTimeline, SendQuotationModal } from "@/components/quotations";
import { ApprovalTimeline, ApprovalStatusBadge, ApprovalSubmissionModal } from "@/components/approvals";
import { DiscountApprovalsApi, AdjustmentsApi } from "@/lib/api";
import ManualPaymentModal from "@/components/payments/ManualPaymentModal";
import { ApprovalTimeline as ApprovalTimelineType, DiscountApproval } from "@/types/discount-approvals";
import { AdjustmentDto } from "@/types/refunds";
import { AdjustmentRequestForm, AdjustmentTimeline, AdjustmentPreview } from "@/components/adjustments";

export default function ViewQuotationPage() {
  const params = useParams();
  const router = useRouter();
  const quotationId = params.id as string;
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [quotation, setQuotation] = useState<any>(null);
  const [statusHistory, setStatusHistory] = useState<any[]>([]);
  const [historyLoading, setHistoryLoading] = useState(false);
  const [responseData, setResponseData] = useState<any | null>(null);
  const [accessLink, setAccessLink] = useState<any | null>(null);
  const [sendModalOpen, setSendModalOpen] = useState(false);
  const [resendModalOpen, setResendModalOpen] = useState(false);
  const [approvalTimeline, setApprovalTimeline] = useState<ApprovalTimelineType[]>([]);
  const [pendingApproval, setPendingApproval] = useState<DiscountApproval | null>(null);
  const [approvalSubmissionModalOpen, setApprovalSubmissionModalOpen] = useState(false);
  const [approvalLoading, setApprovalLoading] = useState(false);
  const [adjustments, setAdjustments] = useState<AdjustmentDto[]>([]);
  const [showAdjustmentForm, setShowAdjustmentForm] = useState(false);
  const [selectedAdjustment, setSelectedAdjustment] = useState<AdjustmentDto | null>(null);
  const [role, setRole] = useState<string | null>(null);
  const [templatePreview, setTemplatePreview] = useState<{ hasTemplate: boolean; templateName?: string; content?: string } | null>(null);
  const [manualPaymentOpen, setManualPaymentOpen] = useState(false);
  const [paymentHistory, setPaymentHistory] = useState<import("@/types/payments").PaymentDto[]>([]);
  const [templateLoading, setTemplateLoading] = useState(false);

  useEffect(() => {
    const token = getAccessToken();
    const userRole = getRoleFromToken(token);
    setRole(userRole);
  }, []);

  useEffect(() => {
    if (quotationId) {
      loadQuotation();
    }
  }, [quotationId]);

  const loadQuotation = async () => {
    try {
      setLoading(true);
      setError(null);
      const result = await QuotationsApi.get(quotationId);
      setQuotation(result.data);
      await loadSupplemental();
      
      // Load template preview if template exists
      if (result.data.templateId) {
        await loadTemplatePreview();
      }
    } catch (err: any) {
      setError(err.message || "Failed to load quotation");
    } finally {
      setLoading(false);
    }
  };

  const loadTemplatePreview = async () => {
    try {
      setTemplateLoading(true);
      const result = await QuotationsApi.getTemplatePreview(quotationId);
      if (result.data.hasTemplate) {
        setTemplatePreview({
          hasTemplate: true,
          templateName: result.data.templateName,
          content: result.data.content
        });
      } else {
        setTemplatePreview({ hasTemplate: false });
      }
    } catch (err: any) {
      console.error("Failed to load template preview:", err);
      setTemplatePreview({ hasTemplate: false });
    } finally {
      setTemplateLoading(false);
    }
  };

  const loadSupplemental = async () => {
    try {
      setHistoryLoading(true);
      const [historyRes, responseRes, accessLinkRes, approvalsRes, adjustmentsRes, paymentsRes] = await Promise.all([
        QuotationsApi.statusHistory(quotationId),
        QuotationsApi.response(quotationId).catch(() => undefined),
        QuotationsApi.accessLink(quotationId).catch(() => undefined),
        DiscountApprovalsApi.getQuotationApprovals(quotationId).catch(() => undefined),
        AdjustmentsApi.getByQuotation(quotationId).catch(() => undefined),
        PaymentsApi.getQuotationHistory(quotationId).catch(() => undefined),
      ]);
      setStatusHistory(historyRes?.data || []);
      setResponseData(responseRes && "data" in (responseRes || {}) ? responseRes.data : null);
      setAccessLink(accessLinkRes && "data" in (accessLinkRes || {}) ? accessLinkRes.data : null);
      setAdjustments(adjustmentsRes?.data || []);
      setPaymentHistory(paymentsRes?.data || []);
      
      // Load approval timeline
      if (approvalsRes?.data) {
        const approvals = approvalsRes.data;
        const pending = approvals.find((a: DiscountApproval) => a.status === "Pending");
        setPendingApproval(pending || null);
        
        // Load timeline
        try {
          const timelineRes = await DiscountApprovalsApi.getTimeline({ quotationId });
          setApprovalTimeline(timelineRes.data || []);
        } catch (err) {
          console.warn("Failed to load approval timeline", err);
        }
      }
    } catch (err) {
      console.warn("Failed to load supplemental quotation info", err);
    } finally {
      setHistoryLoading(false);
    }
  };

  const handleAdjustmentSuccess = async () => {
    setShowAdjustmentForm(false);
    await loadSupplemental();
  };

  const handleApproveAdjustment = async (adjustmentId: string) => {
    try {
      await AdjustmentsApi.approve(adjustmentId, {});
      await loadSupplemental();
    } catch (error) {
      console.error("Error approving adjustment:", error);
      alert("Failed to approve adjustment");
    }
  };

  const handleApplyAdjustment = async (adjustmentId: string) => {
    try {
      await AdjustmentsApi.apply(adjustmentId);
      await loadSupplemental();
      await loadQuotation();
    } catch (error) {
      console.error("Error applying adjustment:", error);
      alert("Failed to apply adjustment");
    }
  };

  const handleDelete = async () => {
    if (!confirm("Are you sure you want to delete this quotation?")) return;
    try {
      await QuotationsApi.delete(quotationId);
      router.push("/quotations");
    } catch (err: any) {
      alert(err.message || "Failed to delete quotation");
    }
  };

  const handleDownloadPdf = () => {
    QuotationsApi.downloadPdf(quotationId);
  };

  const handleDownloadDocx = () => {
    QuotationsApi.downloadDocx(quotationId);
  };

  const recipientFallback = quotation?.clientEmail;
  const isAdmin = role === "Admin";
  const isManager = role === "Manager";
  const isSalesRep = role === "SalesRep";
  const canAddPayment = isAdmin || isManager || isSalesRep;
  // Only SalesRep can send/resend quotations - Admin and Manager cannot
  const canSend = quotation?.status === "DRAFT" && isSalesRep;
  const canResend = ["SENT", "VIEWED", "ACCEPTED", "REJECTED", "EXPIRED"].includes(quotation?.status) && isSalesRep;
  const canEdit = quotation?.status === "DRAFT" && isSalesRep;
  const canDelete = quotation?.status === "DRAFT" && isSalesRep;

  if (loading) {
    return (
      <div className="rounded-sm border border-gray-200 bg-white px-5 pb-2.5 pt-6 shadow-default dark:border-gray-800 dark:bg-gray-900 sm:px-7.5 xl:pb-1">
        <div className="py-8 text-center">Loading...</div>
      </div>
    );
  }

  if (error || !quotation) {
    return (
      <div className="rounded-sm border border-gray-200 bg-white px-5 pb-2.5 pt-6 shadow-default dark:border-gray-800 dark:bg-gray-900 sm:px-7.5 xl:pb-1">
        <div className="py-8 text-center text-red-500">{error || "Quotation not found"}</div>
        <div className="text-center">
          <Link href="/quotations" className="text-primary hover:underline">
            Back to Quotations
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="rounded-sm border border-gray-200 bg-white px-5 pb-2.5 pt-6 shadow-default dark:border-gray-800 dark:bg-gray-900 sm:px-7.5 xl:pb-1">
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h4 className="text-title-md2 font-bold text-black dark:text-white">Quotation {quotation.quotationNumber}</h4>
          <p className="text-sm text-gray-500">Status: <span className={`inline-flex rounded-full px-3 py-1 text-xs font-medium ${getStatusColor(quotation.status)}`}>
            {getStatusLabel(quotation.status)}
          </span></p>
        </div>
        <div className="flex gap-2">
          {canSend && (
            <>
              {canEdit && (
                <Link
                  href={`/quotations/${quotationId}/edit`}
                  className="rounded bg-yellow-500 px-4 py-2 text-sm text-white hover:bg-opacity-90"
                >
                  Edit
                </Link>
              )}
              {canDelete && (
                <button
                  onClick={handleDelete}
                  className="rounded bg-red-500 px-4 py-2 text-sm text-white hover:bg-opacity-90"
                >
                  Delete
                </button>
              )}
              <button
                onClick={() => setSendModalOpen(true)}
                className="rounded border border-blue-500 bg-primary px-4 py-2 text-sm text-black dark:text-white hover:bg-opacity-90"
              >
                Send
              </button>
            </>
          )}
          {canResend && (
            <>
              <button
                onClick={() => setResendModalOpen(true)}
                className="rounded bg-primary px-4 py-2 text-sm text-black hover:bg-opacity-90"
              >
                Resend
              </button>
            </>
          )}

          {/* Download Actions - always available to viewers */}
          <button
            onClick={handleDownloadPdf}
            className="rounded border border-stroke px-4 py-2 text-sm text-black hover:bg-gray-50 dark:border-strokedark dark:text-white dark:hover:bg-meta-4"
          >
            Download PDF
          </button>
          <button
            onClick={handleDownloadDocx}
            className="rounded border border-stroke px-4 py-2 text-sm text-black hover:bg-gray-50 dark:border-strokedark dark:text-white dark:hover:bg-meta-4"
          >
            Download Word
          </button>

          {canAddPayment && (
            <button
              onClick={() => setManualPaymentOpen(true)}
              className="rounded bg-emerald-600 px-4 py-2 text-sm text-white hover:bg-emerald-700"
            >
              Add Payment
            </button>
          )}

          <Link
            href="/quotations"
            className="rounded border border-stroke px-4 py-2 text-sm text-black hover:bg-gray-50 dark:border-strokedark dark:text-white dark:hover:bg-meta-4"
          >
            Back to List
          </Link>
        </div>
      </div>

      {/* Quick Actions */}
      {canSend && (
        <div className="mb-6 rounded border border-dashed border-primary px-4 py-3 text-sm text-primary">
          This quotation is still a draft. Send it to the client when you are ready.
        </div>
      )}

      {/* Quotation Details */}
      <div className="mb-6 grid grid-cols-1 gap-6 md:grid-cols-2">
        <div className="rounded border border-stroke p-4 dark:border-strokedark">
          <h5 className="mb-3 font-semibold text-black dark:text-white">Quotation Information</h5>
          <div className="space-y-2 text-sm">
            <div>
              <span className="text-gray-600 dark:text-gray-400">Quotation Number:</span>
              <span className="ml-2 font-medium text-black dark:text-white">{quotation.quotationNumber}</span>
            </div>
            <div>
              <span className="text-gray-600 dark:text-gray-400">Quotation Date:</span>
              <span className="ml-2 font-medium text-black dark:text-white">{formatDate(quotation.quotationDate)}</span>
            </div>
            <div>
              <span className="text-gray-600 dark:text-gray-400">Valid Until:</span>
              <span className="ml-2 font-medium text-black dark:text-white">{formatDate(quotation.validUntil)}</span>
            </div>
            <div>
              <span className="text-gray-600 dark:text-gray-400">Created By:</span>
              <span className="ml-2 font-medium text-black dark:text-white">{quotation.createdByUserName}</span>
            </div>
          </div>
        </div>

        <div className="rounded border border-stroke p-4 dark:border-strokedark">
          <h5 className="mb-3 font-semibold text-black dark:text-white">Client Information</h5>
          <div className="space-y-2 text-sm">
            <div>
              <span className="text-gray-600 dark:text-gray-400">Client Name:</span>
              <span className="ml-2 font-medium text-black dark:text-white">{quotation.clientName}</span>
            </div>
            <div>
              <span className="text-gray-600 dark:text-gray-400">Email:</span>
              <span className="ml-2 font-medium text-black dark:text-white">{quotation.clientEmail}</span>
            </div>
          </div>
        </div>
      </div>

      {/* Line Items */}
      <div className="mb-6">
        <h5 className="mb-3 font-semibold text-black dark:text-white">Line Items</h5>
        <div className="max-w-full overflow-x-auto">
          <table className="w-full table-auto">
            <thead>
              <tr className="text-left bg-gray-50 dark:bg-gray-800">
                <th className="px-4 py-3 font-medium text-black dark:text-white">#</th>
                <th className="px-4 py-3 font-medium text-black dark:text-white">Item Name</th>
                <th className="px-4 py-3 font-medium text-black dark:text-white">Description</th>
                <th className="px-4 py-3 font-medium text-black dark:text-white">Quantity</th>
                <th className="px-4 py-3 font-medium text-black dark:text-white">Unit Rate</th>
                <th className="px-4 py-3 font-medium text-black dark:text-white">Amount</th>
              </tr>
            </thead>
            <tbody>
              {quotation.lineItems?.map((item: any, index: number) => (
                <tr key={item.lineItemId || index} className="border-b border-[#eee] dark:border-strokedark">
                  <td className="px-4 py-3 text-black dark:text-white">{item.sequenceNumber}</td>
                  <td className="px-4 py-3 text-black dark:text-white">{item.itemName}</td>
                  <td className="px-4 py-3 text-gray-600 dark:text-gray-400">{item.description || "-"}</td>
                  <td className="px-4 py-3 text-black dark:text-white">{item.quantity}</td>
                  <td className="px-4 py-3 text-black dark:text-white">{formatCurrency(item.unitRate)}</td>
                  <td className="px-4 py-3 font-medium text-black dark:text-white">{formatCurrency(item.amount)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Summary */}
      <div className="mb-6 rounded border border-gray-200 bg-gray-50 p-4 dark:border-gray-800 dark:bg-gray-800">
        <h5 className="mb-3 font-semibold text-black dark:text-white">Summary</h5>
        <div className="space-y-2 text-sm">
          <div className="flex justify-between">
            <span className="text-gray-600 dark:text-gray-400">Subtotal:</span>
            <span className="font-medium text-black dark:text-white">{formatCurrency(quotation.subTotal)}</span>
          </div>
          {quotation.discountAmount > 0 && (
            <div className="flex justify-between">
              <span className="text-gray-600 dark:text-gray-400">Discount ({quotation.discountPercentage}%):</span>
              <span className="font-medium text-black dark:text-white">-{formatCurrency(quotation.discountAmount)}</span>
            </div>
          )}

          {/* New Framework-Based Tax Breakdown */}
          {quotation.taxBreakdown && (() => {
            try {
              const breakdown = typeof quotation.taxBreakdown === 'string' 
                ? JSON.parse(quotation.taxBreakdown) 
                : quotation.taxBreakdown;
              if (Array.isArray(breakdown) && breakdown.length > 0) {
                return (
                  <div className="mt-3 pt-2 border-t border-stroke dark:border-strokedark">
                    <div className="mb-2 text-xs font-medium text-gray-700 dark:text-gray-300">Tax Breakdown:</div>
                    {breakdown.map((component: any, idx: number) => (
                      <div key={idx} className="flex justify-between text-xs">
                        <span className="text-gray-600 dark:text-gray-400">
                          {component.component} ({component.rate}%):
                        </span>
                        <span className="font-medium text-black dark:text-white">
                          {formatCurrency(component.amount)}
                        </span>
                      </div>
                    ))}
                  </div>
                );
              }
            } catch (e) {
              // Fall through to legacy display
            }
            return null;
          })()}

          {/* Legacy Tax Display (Fallback) */}
          {(!quotation.taxBreakdown || (() => {
            try {
              const breakdown = typeof quotation.taxBreakdown === 'string' 
                ? JSON.parse(quotation.taxBreakdown) 
                : quotation.taxBreakdown;
              return !Array.isArray(breakdown) || breakdown.length === 0;
            } catch {
              return true;
            }
          })()) && (
            <>
              {quotation.cgstAmount > 0 && (
                <>
                  <div className="flex justify-between">
                    <span className="text-gray-600 dark:text-gray-400">CGST (9%):</span>
                    <span className="font-medium text-black dark:text-white">{formatCurrency(quotation.cgstAmount)}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-600 dark:text-gray-400">SGST (9%):</span>
                    <span className="font-medium text-black dark:text-white">{formatCurrency(quotation.sgstAmount)}</span>
                  </div>
                </>
              )}
              {quotation.igstAmount > 0 && (
                <div className="flex justify-between">
                  <span className="text-gray-600 dark:text-gray-400">IGST (18%):</span>
                  <span className="font-medium text-black dark:text-white">{formatCurrency(quotation.igstAmount)}</span>
                </div>
              )}
            </>
          )}

          <div className="mt-3 flex justify-between border-t border-stroke pt-2 dark:border-strokedark">
            <span className="font-semibold text-black dark:text-white">Total Amount:</span>
            <span className="text-lg font-bold text-primary">{formatCurrency(quotation.totalAmount)}</span>
          </div>
        </div>
      </div>

      {/* Template Preview - Intro & Terms */}
      {templatePreview?.hasTemplate && templatePreview.content && (
        <div className="mb-6 space-y-6">
          {templateLoading ? (
            <div className="rounded border border-gray-200 bg-white p-6 dark:border-gray-800 dark:bg-gray-900">
              <div className="py-8 text-center text-gray-500">Loading template content...</div>
            </div>
          ) : (
            <div 
              className="template-sections"
              dangerouslySetInnerHTML={{ __html: templatePreview.content }}
            />
          )}
        </div>
      )}

      {/* Notes */}
      {quotation.notes && (
        <div className="mb-6 rounded border border-gray-200 p-4 dark:border-gray-800">
          <h5 className="mb-2 font-semibold text-black dark:text-white">Notes</h5>
          <p className="text-sm text-gray-600 dark:text-gray-400 whitespace-pre-wrap">{quotation.notes}</p>
        </div>
      )}
      {/* Approval Status Banner */}
      {pendingApproval && (
        <div className="mb-6 rounded-lg border border-yellow-200 bg-yellow-50 p-4 dark:border-yellow-800 dark:bg-yellow-900/20">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <ApprovalStatusBadge status={pendingApproval.status} />
              <div>
                <p className="text-sm font-medium text-gray-900 dark:text-white">
                  Pending {pendingApproval.approvalLevel} Approval
                </p>
                <p className="text-xs text-gray-600 dark:text-gray-400">
                  Requested on {formatDateTime(pendingApproval.requestDate)}
                </p>
              </div>
            </div>
            {pendingApproval.approverUserName && (
              <p className="text-sm text-gray-600 dark:text-gray-400">
                Approver: {pendingApproval.approverUserName}
              </p>
            )}
          </div>
        </div>
      )}

      {/* Approval Timeline */}
      {approvalTimeline.length > 0 && (
        <div className="mb-6 rounded border border-stroke p-4 dark:border-strokedark">
          <h5 className="mb-3 font-semibold text-black dark:text-white">Discount Approval Timeline</h5>
          <ApprovalTimeline timeline={approvalTimeline} />
        </div>
      )}

      {/* Status Timeline & Client Response */}
      <div className="mb-6 grid grid-cols-1 gap-6 lg:grid-cols-3">
        <div className="rounded border border-stroke p-4 dark:border-strokedark">
          <div className="mb-3 flex items-center justify-between">
            <h5 className="font-semibold text-black dark:text-white">Status Timeline</h5>
            <button className="text-xs text-primary hover:underline" onClick={loadSupplemental}>
              Refresh
            </button>
          </div>
          <QuotationStatusTimeline history={statusHistory} loading={historyLoading} />
        </div>
        <div className="rounded border border-stroke p-4 dark:border-strokedark">
          <h5 className="mb-3 font-semibold text-black dark:text-white">Client Response</h5>
          {responseData ? (
            <ClientResponseCard response={responseData} />
          ) : (
            <p className="text-sm text-gray-500">No client response yet.</p>
          )}
        </div>
        <div className="rounded border border-stroke p-4 dark:border-strokedark">
          <h5 className="mb-3 font-semibold text-black dark:text-white">Payment History</h5>
          {paymentHistory.length === 0 ? (
            <p className="text-sm text-gray-500">No payments recorded yet.</p>
          ) : (
            <div className="max-h-72 overflow-y-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-stroke text-left text-xs uppercase text-gray-500 dark:border-strokedark">
                    <th className="px-2 py-2">Date</th>
                    <th className="px-2 py-2">Gateway</th>
                    <th className="px-2 py-2">Amount</th>
                    <th className="px-2 py-2">Status</th>
                  </tr>
                </thead>
                <tbody>
                  {paymentHistory.map((p) => (
                    <tr key={p.paymentId} className="border-b border-gray-100 text-xs dark:border-strokedark">
                      <td className="px-2 py-2 text-gray-700 dark:text-gray-200">{p.paymentDate ? new Date(p.paymentDate).toLocaleDateString() : new Date(p.createdAt).toLocaleDateString()}</td>
                      <td className="px-2 py-2 text-gray-600 dark:text-gray-300">{p.paymentGateway}</td>
                      <td className="px-2 py-2 font-medium text-gray-900 dark:text-white">{formatCurrency(p.amountPaid)}</td>
                      <td className="px-2 py-2 text-gray-600 dark:text-gray-300">{p.statusLabel}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>

      {/* Adjustments Section */}
      <div className="mb-6 rounded border border-stroke p-4 dark:border-strokedark">
        <div className="mb-4 flex items-center justify-between">
          <h5 className="font-semibold text-black dark:text-white">Adjustments</h5>
          <button
            onClick={() => setShowAdjustmentForm(true)}
            className="rounded bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700"
          >
            Request Adjustment
          </button>
        </div>

        {adjustments.length === 0 ? (
          <p className="text-sm text-gray-500">No adjustments for this quotation</p>
        ) : (
          <div className="space-y-4">
            {adjustments.map((adjustment) => (
              <div
                key={adjustment.adjustmentId}
                className="rounded-md border border-gray-200 p-4"
              >
                <div className="mb-2 flex items-center justify-between">
                  <span className="text-sm font-medium">
                    {adjustment.adjustmentType.replace("_", " ")}
                  </span>
                  <span
                    className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${
                      adjustment.status === "APPROVED"
                        ? "bg-green-100 text-green-800"
                        : adjustment.status === "PENDING"
                        ? "bg-yellow-100 text-yellow-800"
                        : adjustment.status === "REJECTED"
                        ? "bg-red-100 text-red-800"
                        : "bg-blue-100 text-blue-800"
                    }`}
                  >
                    {adjustment.status}
                  </span>
                </div>
                <p className="text-sm text-gray-600">
                  ₹{adjustment.originalAmount.toLocaleString()} → ₹
                  {adjustment.adjustedAmount.toLocaleString()} (
                  {adjustment.adjustmentDifference > 0 ? "+" : ""}
                  ₹{adjustment.adjustmentDifference.toLocaleString()})
                </p>
                <p className="mt-1 text-xs text-gray-500">{adjustment.reason}</p>
                {adjustment.status === "PENDING" && (
                  <div className="mt-2 flex space-x-2">
                    <button
                      onClick={() => handleApproveAdjustment(adjustment.adjustmentId)}
                      className="rounded-md bg-green-600 px-3 py-1 text-xs text-white hover:bg-green-700"
                    >
                      Approve
                    </button>
                  </div>
                )}
                {adjustment.status === "APPROVED" && (
                  <div className="mt-2">
                    <button
                      onClick={() => handleApplyAdjustment(adjustment.adjustmentId)}
                      className="rounded-md bg-blue-600 px-3 py-1 text-xs text-white hover:bg-blue-700"
                    >
                      Apply Adjustment
                    </button>
                  </div>
                )}
              </div>
            ))}
          </div>
        )}

        {adjustments.length > 0 && (
          <div className="mt-4">
            <h3 className="mb-2 text-sm font-medium">Adjustment Timeline</h3>
            <AdjustmentTimeline adjustments={adjustments} />
          </div>
        )}
      </div>

      {/* Access Link */}
      <div className="rounded border border-gray-200 p-4 text-sm dark:border-gray-800">
        <h5 className="mb-3 font-semibold text-black dark:text-white">Access Link</h5>
        {accessLink ? (
          <div className="space-y-2">
            <p>
              <span className="font-medium text-black dark:text-white">Recipient:</span> {accessLink.clientEmail}
            </p>
            <div className="flex flex-wrap items-center gap-2">
              <span className="font-medium text-black dark:text-white">Link:</span>
              <span className="rounded bg-gray-100 px-2 py-1 text-xs text-gray-700 dark:bg-gray-800 dark:text-gray-200">
                {accessLink.viewUrl}
              </span>
              <button
                onClick={() => navigator.clipboard?.writeText(accessLink.viewUrl)}
                className="text-xs font-medium text-primary hover:underline"
              >
                Copy
              </button>
            </div>
            <p className="text-gray-500">
              Sent {accessLink.sentAt ? formatDateTime(accessLink.sentAt) : "n/a"} • Viewed {accessLink.viewCount || 0}{" "}
              times
            </p>
          </div>
        ) : (
          <p className="text-gray-500">No active access link. Send or resend the quotation to generate one.</p>
        )}
      </div>

      {/* Modals */}
      <ApprovalSubmissionModal
        isOpen={approvalSubmissionModalOpen}
        onClose={() => setApprovalSubmissionModalOpen(false)}
        quotationId={quotationId}
        discountPercentage={quotation.discountPercentage}
        threshold={quotation.discountPercentage >= 20 ? 20 : 10}
        onSuccess={() => {
          loadQuotation();
          setApprovalSubmissionModalOpen(false);
        }}
      />
      <SendQuotationModal
        open={sendModalOpen}
        onClose={() => setSendModalOpen(false)}
        quotationId={quotationId}
        defaultRecipient={recipientFallback}
        onSuccess={async () => {
          await loadQuotation();
          setSendModalOpen(false);
        }}
      />
      <SendQuotationModal
        open={resendModalOpen}
        onClose={() => setResendModalOpen(false)}
        quotationId={quotationId}
        defaultRecipient={recipientFallback}
        mode="resend"
        onSuccess={async () => {
          await loadSupplemental();
          setResendModalOpen(false);
        }}
      />

      {manualPaymentOpen && (
        <ManualPaymentModal
          quotationId={quotationId}
          defaultCurrency={quotation?.currency || "INR"}
          onSuccess={async () => {
            await loadQuotation();
            setManualPaymentOpen(false);
          }}
          onClose={() => setManualPaymentOpen(false)}
        />
      )}

      {/* Adjustment Request Modal */}
      {showAdjustmentForm && quotation && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4 backdrop-blur-sm">
          <div className="w-full max-w-2xl rounded-2xl bg-white shadow-2xl dark:bg-gray-900">
            {/* Modal Header */}
            <div className="border-b border-gray-200 px-6 py-4 dark:border-gray-800">
              <div className="flex items-center justify-between">
                <div>
                  <h3 className="text-xl font-bold text-gray-900">Request Adjustment</h3>
                  <p className="mt-1 text-sm text-gray-500">
                    Submit a request to adjust the quotation amount
                  </p>
                </div>
                <button
                  onClick={() => setShowAdjustmentForm(false)}
                  className="rounded-lg p-2 text-gray-400 transition-colors hover:bg-gray-100 hover:text-gray-600 dark:hover:bg-gray-800 dark:hover:text-gray-300"
                >
                  <svg className="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>
              </div>
            </div>
            
            {/* Modal Body */}
            <div className="px-6 py-6">
              <AdjustmentRequestForm
                quotationId={quotation.quotationId}
                onSuccess={handleAdjustmentSuccess}
                onCancel={() => setShowAdjustmentForm(false)}
              />
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

