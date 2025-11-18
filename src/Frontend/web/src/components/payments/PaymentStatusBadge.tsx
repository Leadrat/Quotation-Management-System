"use client";

import { PaymentStatus } from "@/types/payments";

interface PaymentStatusBadgeProps {
  status: PaymentStatus;
}

export function PaymentStatusBadge({ status }: PaymentStatusBadgeProps) {
  const getStatusConfig = (status: PaymentStatus) => {
    switch (status) {
      case PaymentStatus.Success:
        return { label: "Success", className: "bg-green-100 text-green-800" };
      case PaymentStatus.Pending:
        return { label: "Pending", className: "bg-yellow-100 text-yellow-800" };
      case PaymentStatus.Processing:
        return { label: "Processing", className: "bg-blue-100 text-blue-800" };
      case PaymentStatus.Failed:
        return { label: "Failed", className: "bg-red-100 text-red-800" };
      case PaymentStatus.Refunded:
        return { label: "Refunded", className: "bg-gray-100 text-gray-800" };
      case PaymentStatus.PartiallyRefunded:
        return { label: "Partially Refunded", className: "bg-orange-100 text-orange-800" };
      case PaymentStatus.Cancelled:
        return { label: "Cancelled", className: "bg-gray-100 text-gray-800" };
      default:
        return { label: "Unknown", className: "bg-gray-100 text-gray-800" };
    }
  };

  const config = getStatusConfig(status);

  return (
    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${config.className}`}>
      {config.label}
    </span>
  );
}

