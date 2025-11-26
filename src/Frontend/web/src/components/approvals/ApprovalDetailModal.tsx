"use client";
import { useEffect, useState } from "react";
import { Modal } from "@/components/tailadmin/ui/modal";
import Button from "@/components/tailadmin/ui/button/Button";
import { DiscountApproval } from "@/types/discount-approvals";
import { DiscountApprovalsApi } from "@/lib/api";
import { formatDateTime } from "@/utils/quotationFormatter";
import { ApprovalStatusBadge } from "./ApprovalStatusBadge";
import Label from "@/components/tailadmin/form/Label";

interface ApprovalDetailModalProps {
  isOpen: boolean;
  onClose: () => void;
  approvalId: string;
}

export function ApprovalDetailModal({
  isOpen,
  onClose,
  approvalId,
}: ApprovalDetailModalProps) {
  const [loading, setLoading] = useState(true);
  const [approval, setApproval] = useState<DiscountApproval | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen && approvalId) {
      loadApprovalDetails();
    } else {
      setApproval(null);
      setError(null);
      setLoading(true);
    }
  }, [isOpen, approvalId]);

  const loadApprovalDetails = async () => {
    try {
      setLoading(true);
      setError(null);
      const result = await DiscountApprovalsApi.getById(approvalId);
      setApproval(result.data);
    } catch (err: any) {
      setError(err.message || "Failed to load approval details");
    } finally {
      setLoading(false);
    }
  };

  if (!isOpen) return null;

  return (
    <Modal isOpen={isOpen} onClose={onClose} className="max-w-[700px] p-5 lg:p-10">
      <h4 className="font-semibold text-gray-800 mb-4 text-title-sm dark:text-white/90">
        Approval Request Details
      </h4>

      {loading ? (
        <div className="py-8 text-center">
          <div className="inline-block h-8 w-8 animate-spin rounded-full border-4 border-solid border-current border-r-transparent align-[-0.125em] motion-reduce:animate-[spin_1.5s_linear_infinite]"></div>
          <p className="mt-4 text-sm text-gray-500 dark:text-gray-400">Loading approval details...</p>
        </div>
      ) : error ? (
        <div className="rounded-md bg-red-50 p-4 text-sm text-red-800 dark:bg-red-900/20 dark:text-red-300">
          {error}
        </div>
      ) : approval ? (
        <div className="space-y-6">
          {/* Approval Summary */}
          <div className="rounded-lg border border-gray-200 bg-gray-50 p-4 dark:border-gray-700 dark:bg-gray-800">
            <div className="space-y-3 text-sm">
              <div className="flex justify-between items-center">
                <span className="text-gray-600 dark:text-gray-400">Quotation:</span>
                <span className="font-medium text-gray-900 dark:text-white">{approval.quotationNumber}</span>
              </div>
              <div className="flex justify-between items-center">
                <span className="text-gray-600 dark:text-gray-400">Client:</span>
                <span className="font-medium text-gray-900 dark:text-white">{approval.clientName}</span>
              </div>
              <div className="flex justify-between items-center">
                <span className="text-gray-600 dark:text-gray-400">Discount:</span>
                <span className="font-medium text-gray-900 dark:text-white">
                  {approval.currentDiscountPercentage}%
                </span>
              </div>
              <div className="flex justify-between items-center">
                <span className="text-gray-600 dark:text-gray-400">Requested by:</span>
                <span className="font-medium text-gray-900 dark:text-white">{approval.requestedByUserName}</span>
              </div>
              <div className="flex justify-between items-center">
                <span className="text-gray-600 dark:text-gray-400">Level:</span>
                <span className="font-medium text-gray-900 dark:text-white">{approval.approvalLevel}</span>
              </div>
              <div className="flex justify-between items-center">
                <span className="text-gray-600 dark:text-gray-400">Status:</span>
                <ApprovalStatusBadge status={approval.status} />
              </div>
              <div className="flex justify-between items-center">
                <span className="text-gray-600 dark:text-gray-400">Requested Date:</span>
                <span className="font-medium text-gray-900 dark:text-white">{formatDateTime(approval.requestDate)}</span>
              </div>
              {approval.approvalDate && (
                <div className="flex justify-between items-center">
                  <span className="text-gray-600 dark:text-gray-400">Approved Date:</span>
                  <span className="font-medium text-gray-900 dark:text-white">{formatDateTime(approval.approvalDate)}</span>
                </div>
              )}
              {approval.rejectionDate && (
                <div className="flex justify-between items-center">
                  <span className="text-gray-600 dark:text-gray-400">Rejected Date:</span>
                  <span className="font-medium text-gray-900 dark:text-white">{formatDateTime(approval.rejectionDate)}</span>
                </div>
              )}
              {approval.approverUserName && (
                <div className="flex justify-between items-center">
                  <span className="text-gray-600 dark:text-gray-400">Approved/Rejected by:</span>
                  <span className="font-medium text-gray-900 dark:text-white">{approval.approverUserName}</span>
                </div>
              )}
            </div>
          </div>

          {/* Sales Rep's Request Reason */}
          <div>
            <Label className="mb-2 block font-medium text-gray-700 dark:text-gray-300">
              Request Reason (from Sales Rep)
            </Label>
            <div className="rounded-lg border border-gray-200 bg-white p-4 dark:border-gray-700 dark:bg-gray-800">
              <p className="text-sm text-gray-800 dark:text-white whitespace-pre-wrap">
                {approval.reason || "No reason provided"}
              </p>
            </div>
          </div>

          {/* Sales Rep's Comments */}
          {approval.comments && (
            <div>
              <Label className="mb-2 block font-medium text-gray-700 dark:text-gray-300">
                Additional Comments (from Sales Rep)
              </Label>
              <div className="rounded-lg border border-gray-200 bg-white p-4 dark:border-gray-700 dark:bg-gray-800">
                <p className="text-sm text-gray-800 dark:text-white whitespace-pre-wrap">
                  {approval.comments}
                </p>
              </div>
            </div>
          )}

          <div className="flex items-center justify-end gap-3 pt-4 border-t border-gray-200 dark:border-gray-700">
            <Button size="sm" variant="outline" onClick={onClose}>
              Close
            </Button>
          </div>
        </div>
      ) : null}
    </Modal>
  );
}

