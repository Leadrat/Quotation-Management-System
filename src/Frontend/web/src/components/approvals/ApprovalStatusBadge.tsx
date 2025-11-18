"use client";
import { ApprovalStatus } from "@/types/discount-approvals";

interface ApprovalStatusBadgeProps {
  status: ApprovalStatus;
  className?: string;
}

export function ApprovalStatusBadge({ status, className = "" }: ApprovalStatusBadgeProps) {
  const getStatusColor = (status: ApprovalStatus) => {
    switch (status) {
      case "Pending":
        return "bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-300";
      case "Approved":
        return "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300";
      case "Rejected":
        return "bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300";
      default:
        return "bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-300";
    }
  };

  const getStatusLabel = (status: ApprovalStatus) => {
    switch (status) {
      case "Pending":
        return "Pending";
      case "Approved":
        return "Approved";
      case "Rejected":
        return "Rejected";
      default:
        return status;
    }
  };

  return (
    <span
      className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${getStatusColor(status)} ${className}`}
    >
      {getStatusLabel(status)}
    </span>
  );
}

