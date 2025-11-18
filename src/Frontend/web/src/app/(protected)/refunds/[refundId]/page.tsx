"use client";

import { useState, useEffect } from "react";
import { useParams, useRouter } from "next/navigation";
import { RefundDto, RefundTimelineDto } from "@/types/refunds";
import { RefundsApi } from "@/lib/api";
import RefundStatusBadge from "@/components/refunds/RefundStatusBadge";
import RefundReasonBadge from "@/components/refunds/RefundReasonBadge";
import RefundAmountDisplay from "@/components/refunds/RefundAmountDisplay";
import RefundTimeline from "@/components/refunds/RefundTimeline";
import ApprovalDialog from "@/components/refunds/ApprovalDialog";

export default function RefundDetailPage() {
  const params = useParams();
  const router = useRouter();
  const refundId = params.refundId as string;

  const [refund, setRefund] = useState<RefundDto | null>(null);
  const [timeline, setTimeline] = useState<RefundTimelineDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [showApprovalDialog, setShowApprovalDialog] = useState(false);

  useEffect(() => {
    if (refundId) {
      loadRefund();
      loadTimeline();
    }
  }, [refundId]);

  const loadRefund = async () => {
    try {
      const response = await RefundsApi.getById(refundId);
      setRefund(response.data);
    } catch (error) {
      console.error("Error loading refund:", error);
    } finally {
      setLoading(false);
    }
  };

  const loadTimeline = async () => {
    try {
      const response = await RefundsApi.getTimeline(refundId);
      setTimeline(response.data || []);
    } catch (error) {
      console.error("Error loading timeline:", error);
    }
  };

  const handleApprove = async (comments?: string) => {
    if (!refund) return;
    try {
      await RefundsApi.approve(refund.refundId, { comments });
      await loadRefund();
      await loadTimeline();
    } catch (error) {
      throw error;
    }
  };

  const handleReject = async (reason: string, comments?: string) => {
    if (!refund) return;
    try {
      await RefundsApi.reject(refund.refundId, { rejectionReason: reason, comments });
      await loadRefund();
      await loadTimeline();
    } catch (error) {
      throw error;
    }
  };

  const handleProcess = async () => {
    if (!refund) return;
    try {
      await RefundsApi.process(refund.refundId);
      await loadRefund();
      await loadTimeline();
    } catch (error) {
      console.error("Error processing refund:", error);
      alert("Failed to process refund");
    }
  };

  if (loading) {
    return <div className="p-6">Loading refund details...</div>;
  }

  if (!refund) {
    return <div className="p-6">Refund not found</div>;
  }

  return (
    <div className="p-6">
      <div className="mb-6">
        <button
          onClick={() => router.back()}
          className="mb-4 text-blue-600 hover:text-blue-800"
        >
          ← Back
        </button>
        <h1 className="text-2xl font-bold text-gray-900">Refund Details</h1>
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        <div className="space-y-6">
          <div className="rounded-lg border border-gray-200 bg-white p-6">
            <h2 className="mb-4 text-lg font-semibold">Refund Information</h2>
            <dl className="space-y-3">
              <div>
                <dt className="text-sm font-medium text-gray-500">Refund ID</dt>
                <dd className="mt-1 text-sm text-gray-900">{refund.refundId}</dd>
              </div>
              <div>
                <dt className="text-sm font-medium text-gray-500">Amount</dt>
                <dd className="mt-1">
                  <RefundAmountDisplay amount={refund.refundAmount} />
                </dd>
              </div>
              <div>
                <dt className="text-sm font-medium text-gray-500">Status</dt>
                <dd className="mt-1">
                  <RefundStatusBadge status={refund.refundStatus} />
                </dd>
              </div>
              <div>
                <dt className="text-sm font-medium text-gray-500">Reason</dt>
                <dd className="mt-1">
                  <RefundReasonBadge reasonCode={refund.refundReasonCode} />
                  <p className="mt-1 text-sm text-gray-600">{refund.refundReason}</p>
                </dd>
              </div>
              <div>
                <dt className="text-sm font-medium text-gray-500">Requested By</dt>
                <dd className="mt-1 text-sm text-gray-900">{refund.requestedByUserName}</dd>
              </div>
              <div>
                <dt className="text-sm font-medium text-gray-500">Request Date</dt>
                <dd className="mt-1 text-sm text-gray-900">
                  {new Date(refund.requestDate).toLocaleString()}
                </dd>
              </div>
              {refund.approvedByUserName && (
                <div>
                  <dt className="text-sm font-medium text-gray-500">Approved By</dt>
                  <dd className="mt-1 text-sm text-gray-900">{refund.approvedByUserName}</dd>
                </div>
              )}
              {refund.approvalDate && (
                <div>
                  <dt className="text-sm font-medium text-gray-500">Approval Date</dt>
                  <dd className="mt-1 text-sm text-gray-900">
                    {new Date(refund.approvalDate).toLocaleString()}
                  </dd>
                </div>
              )}
              {refund.comments && (
                <div>
                  <dt className="text-sm font-medium text-gray-500">Comments</dt>
                  <dd className="mt-1 text-sm text-gray-900">{refund.comments}</dd>
                </div>
              )}
            </dl>
          </div>

          <div className="rounded-lg border border-gray-200 bg-white p-6">
            <h2 className="mb-4 text-lg font-semibold">Actions</h2>
            <div className="space-y-2">
              {refund.refundStatus === "Pending" && (
                <button
                  onClick={() => setShowApprovalDialog(true)}
                  className="w-full rounded-md bg-blue-600 px-4 py-2 text-white hover:bg-blue-700"
                >
                  Approve/Reject
                </button>
              )}
              {refund.refundStatus === "Approved" && (
                <button
                  onClick={handleProcess}
                  className="w-full rounded-md bg-green-600 px-4 py-2 text-white hover:bg-green-700"
                >
                  Process Refund
                </button>
              )}
            </div>
          </div>
        </div>

        <div className="rounded-lg border border-gray-200 bg-white p-6">
          <h2 className="mb-4 text-lg font-semibold">Timeline</h2>
          <RefundTimeline timeline={timeline} />
        </div>
      </div>

      {showApprovalDialog && refund && (
        <ApprovalDialog
          refund={refund}
          onApprove={handleApprove}
          onReject={handleReject}
          onClose={() => setShowApprovalDialog(false)}
        />
      )}
    </div>
  );
}

