"use client";

import { RefundStatus } from "@/types/refunds";

interface RefundStatusBadgeProps {
  status: RefundStatus;
}

export default function RefundStatusBadge({ status }: RefundStatusBadgeProps) {
  const getStatusColor = (status: RefundStatus) => {
    switch (status) {
      case RefundStatus.Pending:
        return "bg-yellow-100 text-yellow-800";
      case RefundStatus.Approved:
        return "bg-blue-100 text-blue-800";
      case RefundStatus.Processing:
        return "bg-purple-100 text-purple-800";
      case RefundStatus.Completed:
        return "bg-green-100 text-green-800";
      case RefundStatus.Failed:
        return "bg-red-100 text-red-800";
      case RefundStatus.Reversed:
        return "bg-gray-100 text-gray-800";
      default:
        return "bg-gray-100 text-gray-800";
    }
  };

  return (
    <span
      className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${getStatusColor(status)}`}
    >
      {status}
    </span>
  );
}

