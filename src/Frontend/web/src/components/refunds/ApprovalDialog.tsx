"use client";

import { useState } from "react";
import { RefundDto } from "@/types/refunds";

interface ApprovalDialogProps {
  refund: RefundDto;
  onApprove: (comments?: string) => Promise<void>;
  onReject: (reason: string, comments?: string) => Promise<void>;
  onClose: () => void;
}

export default function ApprovalDialog({
  refund,
  onApprove,
  onReject,
  onClose,
}: ApprovalDialogProps) {
  const [action, setAction] = useState<"approve" | "reject" | null>(null);
  const [comments, setComments] = useState("");
  const [rejectionReason, setRejectionReason] = useState("");
  const [loading, setLoading] = useState(false);

  const handleApprove = async () => {
    setLoading(true);
    try {
      await onApprove(comments || undefined);
      onClose();
    } catch (error) {
      console.error("Error approving refund:", error);
    } finally {
      setLoading(false);
    }
  };

  const handleReject = async () => {
    if (!rejectionReason.trim()) {
      alert("Please provide a rejection reason");
      return;
    }
    setLoading(true);
    try {
      await onReject(rejectionReason, comments || undefined);
      onClose();
    } catch (error) {
      console.error("Error rejecting refund:", error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
      <div className="w-full max-w-md rounded-lg bg-white p-6 shadow-xl">
        <h3 className="mb-4 text-lg font-semibold">Refund Approval</h3>
        
        <div className="mb-4 space-y-2">
          <p className="text-sm text-gray-600">
            <span className="font-medium">Amount:</span> ₹{refund.refundAmount.toLocaleString()}
          </p>
          <p className="text-sm text-gray-600">
            <span className="font-medium">Reason:</span> {refund.refundReason}
          </p>
        </div>

        {!action ? (
          <div className="space-y-3">
            <button
              onClick={() => setAction("approve")}
              className="w-full rounded-md bg-green-600 px-4 py-2 text-white hover:bg-green-700"
            >
              Approve
            </button>
            <button
              onClick={() => setAction("reject")}
              className="w-full rounded-md bg-red-600 px-4 py-2 text-white hover:bg-red-700"
            >
              Reject
            </button>
          </div>
        ) : action === "approve" ? (
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700">
                Comments (optional)
              </label>
              <textarea
                value={comments}
                onChange={(e) => setComments(e.target.value)}
                className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2"
                rows={3}
                placeholder="Add any comments..."
              />
            </div>
            <div className="flex space-x-3">
              <button
                onClick={handleApprove}
                disabled={loading}
                className="flex-1 rounded-md bg-green-600 px-4 py-2 text-white hover:bg-green-700 disabled:opacity-50"
              >
                {loading ? "Processing..." : "Confirm Approval"}
              </button>
              <button
                onClick={() => setAction(null)}
                className="rounded-md border border-gray-300 px-4 py-2 text-gray-700 hover:bg-gray-50"
              >
                Back
              </button>
            </div>
          </div>
        ) : (
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700">
                Rejection Reason <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                value={rejectionReason}
                onChange={(e) => setRejectionReason(e.target.value)}
                className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2"
                placeholder="Enter rejection reason..."
                required
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">
                Comments (optional)
              </label>
              <textarea
                value={comments}
                onChange={(e) => setComments(e.target.value)}
                className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2"
                rows={3}
                placeholder="Add any comments..."
              />
            </div>
            <div className="flex space-x-3">
              <button
                onClick={handleReject}
                disabled={loading || !rejectionReason.trim()}
                className="flex-1 rounded-md bg-red-600 px-4 py-2 text-white hover:bg-red-700 disabled:opacity-50"
              >
                {loading ? "Processing..." : "Confirm Rejection"}
              </button>
              <button
                onClick={() => setAction(null)}
                className="rounded-md border border-gray-300 px-4 py-2 text-gray-700 hover:bg-gray-50"
              >
                Back
              </button>
            </div>
          </div>
        )}

        <button
          onClick={onClose}
          className="mt-4 w-full rounded-md border border-gray-300 px-4 py-2 text-gray-700 hover:bg-gray-50"
        >
          Cancel
        </button>
      </div>
    </div>
  );
}

