"use client";

import { useState, useEffect } from "react";
import { RefundDto } from "@/types/refunds";
import { RefundsApi } from "@/lib/api";
import RefundStatusBadge from "@/components/refunds/RefundStatusBadge";
import RefundReasonBadge from "@/components/refunds/RefundReasonBadge";
import RefundAmountDisplay from "@/components/refunds/RefundAmountDisplay";
import Link from "next/link";

export default function RefundsPage() {
  const [refunds, setRefunds] = useState<RefundDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState<"all" | "pending" | "approved" | "completed">("all");

  useEffect(() => {
    loadRefunds();
  }, [filter]);

  const loadRefunds = async () => {
    setLoading(true);
    try {
      let response;
      if (filter === "pending") {
        response = await RefundsApi.getPending();
      } else {
        // For now, get all refunds - in production, you'd have a getAll endpoint
        response = await RefundsApi.getPending();
      }
      setRefunds(response.data || []);
    } catch (error) {
      console.error("Error loading refunds:", error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="p-6">
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Refunds</h1>
        <Link
          href="/refunds/pending"
          className="rounded-md bg-blue-600 px-4 py-2 text-white hover:bg-blue-700"
        >
          View Pending Approvals
        </Link>
      </div>

      <div className="mb-4 flex space-x-2">
        <button
          onClick={() => setFilter("all")}
          className={`rounded-md px-4 py-2 ${
            filter === "all"
              ? "bg-blue-600 text-white"
              : "bg-gray-100 text-gray-700"
          }`}
        >
          All
        </button>
        <button
          onClick={() => setFilter("pending")}
          className={`rounded-md px-4 py-2 ${
            filter === "pending"
              ? "bg-blue-600 text-white"
              : "bg-gray-100 text-gray-700"
          }`}
        >
          Pending
        </button>
        <button
          onClick={() => setFilter("approved")}
          className={`rounded-md px-4 py-2 ${
            filter === "approved"
              ? "bg-blue-600 text-white"
              : "bg-gray-100 text-gray-700"
          }`}
        >
          Approved
        </button>
        <button
          onClick={() => setFilter("completed")}
          className={`rounded-md px-4 py-2 ${
            filter === "completed"
              ? "bg-blue-600 text-white"
              : "bg-gray-100 text-gray-700"
          }`}
        >
          Completed
        </button>
      </div>

      {loading ? (
        <div className="text-center py-8">Loading refunds...</div>
      ) : refunds.length === 0 ? (
        <div className="text-center py-8 text-gray-500">No refunds found</div>
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
                  Requested By
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Request Date
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
                    {refund.refundId.substring(0, 8)}...
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
                    {refund.requestedByUserName}
                  </td>
                  <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                    {new Date(refund.requestDate).toLocaleDateString()}
                  </td>
                  <td className="whitespace-nowrap px-6 py-4 text-sm">
                    <Link
                      href={`/refunds/${refund.refundId}`}
                      className="text-blue-600 hover:text-blue-800"
                    >
                      View Details
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

