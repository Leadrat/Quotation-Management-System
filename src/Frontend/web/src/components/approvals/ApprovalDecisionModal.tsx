"use client";
import { useState } from "react";
import { Modal } from "@/components/tailadmin/ui/modal";
import Button from "@/components/tailadmin/ui/button/Button";
import Label from "@/components/tailadmin/form/Label";
import TextArea from "@/components/tailadmin/form/input/TextArea";
import { DiscountApproval } from "@/types/discount-approvals";
import { formatCurrency } from "@/utils/quotationFormatter";

interface ApprovalDecisionModalProps {
  isOpen: boolean;
  onClose: () => void;
  approval: DiscountApproval;
  action: "approve" | "reject";
  onSubmit: (reason: string, comments?: string) => Promise<void>;
}

export function ApprovalDecisionModal({
  isOpen,
  onClose,
  approval,
  action,
  onSubmit,
}: ApprovalDecisionModalProps) {
  const [reason, setReason] = useState("");
  const [comments, setComments] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e?: React.FormEvent | React.MouseEvent) => {
    e?.preventDefault();
    if (reason.trim().length < 10) {
      setError("Reason must be at least 10 characters.");
      return;
    }

    setSubmitting(true);
    setError(null);

    try {
      await onSubmit(reason.trim(), comments.trim() || undefined);
      setReason("");
      setComments("");
    } catch (err: any) {
      setError(err.message || `Failed to ${action} approval`);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} className="max-w-[600px] p-5 lg:p-10">
      <h4 className="font-semibold text-gray-800 mb-4 text-title-sm dark:text-white/90">
        {action === "approve" ? "Approve" : "Reject"} Discount Approval
      </h4>

      {/* Approval Summary */}
      <div className="mb-6 rounded-lg border border-gray-200 bg-gray-50 p-4 dark:border-gray-700 dark:bg-gray-800">
        <div className="space-y-2 text-sm">
          <div className="flex justify-between">
            <span className="text-gray-600 dark:text-gray-400">Quotation:</span>
            <span className="font-medium text-gray-900 dark:text-white">{approval.quotationNumber}</span>
          </div>
          <div className="flex justify-between">
            <span className="text-gray-600 dark:text-gray-400">Client:</span>
            <span className="font-medium text-gray-900 dark:text-white">{approval.clientName}</span>
          </div>
          <div className="flex justify-between">
            <span className="text-gray-600 dark:text-gray-400">Discount:</span>
            <span className="font-medium text-gray-900 dark:text-white">
              {approval.currentDiscountPercentage}%
            </span>
          </div>
          <div className="flex justify-between">
            <span className="text-gray-600 dark:text-gray-400">Requested by:</span>
            <span className="font-medium text-gray-900 dark:text-white">{approval.requestedByUserName}</span>
          </div>
        </div>
      </div>

      <form onSubmit={handleSubmit}>
        <div className="mb-4">
          <Label htmlFor="reason">
            Reason for {action === "approve" ? "Approval" : "Rejection"} <span className="text-red-500">*</span>
          </Label>
          <TextArea
            value={reason}
            onChange={setReason}
            placeholder={`Please provide a detailed reason for ${action === "approve" ? "approving" : "rejecting"} this discount...`}
            rows={4}
            className="mt-1"
            error={reason.length > 0 && reason.length < 10}
          />
          <p className="mt-1 text-xs text-gray-500">
            Minimum 10 characters. {reason.length}/2000
          </p>
        </div>

        <div className="mb-6">
          <Label htmlFor="comments">Additional Comments (Optional)</Label>
          <TextArea
            value={comments}
            onChange={setComments}
            placeholder="Any additional information..."
            rows={3}
            className="mt-1"
          />
          <p className="mt-1 text-xs text-gray-500">{comments.length}/5000</p>
        </div>

        {error && (
          <div className="mb-4 rounded-md bg-red-50 p-3 text-sm text-red-800 dark:bg-red-900/20 dark:text-red-300">
            {error}
          </div>
        )}

        <div className="flex items-center justify-end gap-3">
          <Button size="sm" variant="outline" onClick={onClose} disabled={submitting}>
            Cancel
          </Button>
          <Button
            size="sm"
            variant={action === "approve" ? "primary" : "outline"}
            className={action === "reject" ? "bg-red-500 text-white hover:bg-red-600" : ""}
            disabled={submitting || reason.trim().length < 10}
            onClick={handleSubmit}
          >
            {submitting ? `${action === "approve" ? "Approving" : "Rejecting"}...` : action === "approve" ? "Approve" : "Reject"}
          </Button>
        </div>
      </form>
    </Modal>
  );
}

