"use client";
import { DiscountApproval } from "@/types/discount-approvals";
import { ApprovalStatusBadge } from "./ApprovalStatusBadge";
import { formatDateTime } from "@/utils/quotationFormatter";

interface LockedFormOverlayProps {
  approval: DiscountApproval;
  className?: string;
}

export function LockedFormOverlay({ approval, className = "" }: LockedFormOverlayProps) {
  return (
    <div className={`relative ${className}`}>
      <div className="absolute inset-0 z-50 flex items-center justify-center rounded-lg bg-white/90 backdrop-blur-sm dark:bg-gray-900/90">
        <div className="mx-4 max-w-md rounded-lg border border-gray-200 bg-white p-6 shadow-lg dark:border-gray-700 dark:bg-gray-800">
          <div className="text-center">
            <div className="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-yellow-100 dark:bg-yellow-900">
              <svg
                className="h-6 w-6 text-yellow-600 dark:text-yellow-300"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"
                />
              </svg>
            </div>
            <h3 className="mb-2 text-lg font-semibold text-gray-900 dark:text-white">
              Quotation Locked for Approval
            </h3>
            <p className="mb-4 text-sm text-gray-600 dark:text-gray-400">
              This quotation is pending approval and cannot be edited until a decision is made.
            </p>

            <div className="space-y-3 text-left">
              <div className="flex items-center justify-between">
                <span className="text-sm text-gray-600 dark:text-gray-400">Status:</span>
                <ApprovalStatusBadge status={approval.status} />
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-gray-600 dark:text-gray-400">Approval Level:</span>
                <span className="text-sm font-medium text-gray-900 dark:text-white">
                  {approval.approvalLevel}
                </span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-gray-600 dark:text-gray-400">Requested:</span>
                <span className="text-sm text-gray-900 dark:text-white">
                  {formatDateTime(approval.requestDate)}
                </span>
              </div>
              {approval.approverUserName && (
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600 dark:text-gray-400">Approver:</span>
                  <span className="text-sm text-gray-900 dark:text-white">
                    {approval.approverUserName}
                  </span>
                </div>
              )}
              <div className="mt-3 rounded-md bg-gray-50 p-3 dark:bg-gray-700">
                <p className="text-xs font-medium text-gray-700 dark:text-gray-300">Reason:</p>
                <p className="mt-1 text-sm text-gray-600 dark:text-gray-400">{approval.reason}</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

