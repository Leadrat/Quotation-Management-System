"use client";
import { useState } from "react";
import { Modal } from "@/components/tailadmin/ui/modal";
import Button from "@/components/tailadmin/ui/button/Button";
import Label from "@/components/tailadmin/form/Label";
import TextArea from "@/components/tailadmin/form/input/TextArea";
import { DiscountApprovalsApi } from "@/lib/api";
import { CreateDiscountApprovalRequest } from "@/types/discount-approvals";

interface ApprovalSubmissionModalProps {
  isOpen: boolean;
  onClose: () => void;
  quotationId: string;
  discountPercentage: number;
  threshold: number;
  onSuccess?: () => void;
}

export function ApprovalSubmissionModal({
  isOpen,
  onClose,
  quotationId,
  discountPercentage,
  threshold,
  onSuccess,
}: ApprovalSubmissionModalProps) {
  const [reason, setReason] = useState("");
  const [comments, setComments] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (reason.trim().length < 10) {
      setError("Reason must be at least 10 characters.");
      return;
    }

    setSubmitting(true);
    setError(null);

    try {
      const request: CreateDiscountApprovalRequest = {
        quotationId,
        discountPercentage,
        reason: reason.trim(),
        comments: comments.trim() || undefined,
      };

      await DiscountApprovalsApi.request(request);
      onSuccess?.();
      onClose();
      setReason("");
      setComments("");
    } catch (err: any) {
      setError(err.message || "Failed to submit approval request");
    } finally {
      setSubmitting(false);
    }
  };

  const approvalLevel = discountPercentage >= 20 ? "Admin" : "Manager";

  return (
    <Modal isOpen={isOpen} onClose={onClose} className="max-w-[600px] p-5 lg:p-10">
      <h4 className="font-semibold text-gray-800 mb-4 text-title-sm dark:text-white/90">
        Request Discount Approval
      </h4>
      <p className="text-sm text-gray-600 dark:text-gray-400 mb-6">
        This quotation has a discount of <strong>{discountPercentage}%</strong>, which requires{" "}
        <strong>{approvalLevel}</strong> approval (threshold: {threshold}%).
      </p>

      <form onSubmit={handleSubmit}>
        <div className="mb-4">
          <Label htmlFor="reason" required>
            Reason for Discount
          </Label>
          <TextArea
            value={reason}
            onChange={setReason}
            placeholder="Please provide a detailed reason for this discount..."
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
          <Button type="button" size="sm" variant="outline" onClick={onClose} disabled={submitting}>
            Cancel
          </Button>
          <Button type="submit" size="sm" disabled={submitting || reason.trim().length < 10}>
            {submitting ? "Submitting..." : "Submit for Approval"}
          </Button>
        </div>
      </form>
    </Modal>
  );
}

