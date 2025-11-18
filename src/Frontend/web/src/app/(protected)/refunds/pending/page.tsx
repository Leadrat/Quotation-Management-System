"use client";

import { useState, useEffect } from "react";
import { RefundDto } from "@/types/refunds";
import { RefundsApi } from "@/lib/api";
import RefundStatusBadge from "@/components/refunds/RefundStatusBadge";
import RefundReasonBadge from "@/components/refunds/RefundReasonBadge";
import RefundAmountDisplay from "@/components/refunds/RefundAmountDisplay";
import ApprovalDialog from "@/components/refunds/ApprovalDialog";
import Link from "next/link";

export default function PendingRefundsPage() {
  const [refunds, setRefunds] = useState<RefundDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedRefund, setSelectedRefund] = useState<RefundDto | null>(null);
  const [approvalLevel, setApprovalLevel] = useState<string>("");

  useEffect(() => {
    loadPendingRefunds();
  }, [approvalLevel]);

  const loadPendingRefunds = async () => {
    setLoading(true);
    try {
      const response = await RefundsApi.getPending(approvalLevel || undefined);
      setRefunds(response.data || []);
    } catch (error) {
      console.error("Error loading pending refunds:", error);
    } finally {
      setLoading(false);
    }
  };

  const handleApprove = async (comments?: string) => {
    if (!selectedRefund) return;
    try {
      await RefundsApi.approve(selectedRefund.refundId, { comments });
      await loadPendingRefunds();
      setSelectedRefund(null);
    } catch (error) {
      throw error;
    }
  };

  const handleReject = async (reason: string, comments?: string) => {
    if (!selectedRefund) return;
    try {
      await RefundsApi.reject(selectedRefund.refundId, { rejectionReason: reason, comments });
      await loadPendingRefunds();
      setSelectedRefund(null);
    } catch (error) {
      throw error;
    }
  };

  return (
    <div className="p-6">
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Pending Refund Approvals</h1>
        <Link
          href="/refunds"
          className="text-blue-600 hover:text-blue-800"
        >
          View All Refunds
        </Link>
      </div>

      <div className="mb-4">
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Filter by Approval Level
        </label>
        <select
          value={approvalLevel}
          onChange={(e) => setApprovalLevel(e.target.value)}
          className="rounded-md border border-gray-300 px-3 py-2"
        >
          <option value="">All Levels</option>
          <option value="Auto">Auto</option>
          <option value="Manager">Manager</option>
          <option value="Admin">Admin</option>
        </select>
      </div>

      {loading ? (
        <div className="text-center py-8">Loading pending refunds...</div>
      ) : refunds.length === 0 ? (
        <div className="text-center py-8 text-gray-500">No pending refunds</div>
      ) : (
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Refund ID
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Amount
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Reason
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Status
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Approval Level
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Requested By
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 bg-white">
              {refunds.map((refund) => (
                <tr key={refund.refundId}>
                  <td className="whitespace-nowrap px-6 py-4 text-sm font-medium text-gray-900">
                    <Link
                      href={`/refunds/${refund.refundId}`}
                      className="text-blue-600 hover:text-blue-800"
                    >
                      {refund.refundId.substring(0, 8)}...
                    </Link>
                  </td>
                  <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                    <RefundAmountDisplay amount={refund.refundAmount} />
                  </td>
                  <td className="px-6 py-4 text-sm text-gray-500">
                    <div className="space-y-1">
                      <RefundReasonBadge reasonCode={refund.refundReasonCode} />
                      <p className="text-xs text-gray-400">{refund.refundReason}</p>
                    </div>
                  </td>
                  <td className="whitespace-nowrap px-6 py-4 text-sm">
                    <RefundStatusBadge status={refund.refundStatus} />
                  </td>
                  <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                    {refund.approvalLevel || "N/A"}
                  </td>
                  <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                    {refund.requestedByUserName}
                  </td>
                  <td className="whitespace-nowrap px-6 py-4 text-sm">
                    <button
                      onClick={() => setSelectedRefund(refund)}
                      className="text-blue-600 hover:text-blue-800"
                    >
                      Review
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {selectedRefund && (
        <ApprovalDialog
          refund={selectedRefund}
          onApprove={handleApprove}
          onReject={handleReject}
          onClose={() => setSelectedRefund(null)}
        />
      )}
    </div>
  );
}

