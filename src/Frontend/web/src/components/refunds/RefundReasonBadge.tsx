"use client";

import { RefundReasonCode } from "@/types/refunds";

interface RefundReasonBadgeProps {
  reasonCode: RefundReasonCode;
}

export default function RefundReasonBadge({ reasonCode }: RefundReasonBadgeProps) {
  const getReasonLabel = (code: RefundReasonCode) => {
    switch (code) {
      case RefundReasonCode.CLIENT_REQUEST:
        return "Client Request";
      case RefundReasonCode.ERROR:
        return "Error";
      case RefundReasonCode.DISCOUNT_ADJUSTMENT:
        return "Discount Adjustment";
      case RefundReasonCode.CANCELLATION:
        return "Cancellation";
      case RefundReasonCode.DUPLICATE_PAYMENT:
        return "Duplicate Payment";
      case RefundReasonCode.OTHER:
        return "Other";
      default:
        return code;
    }
  };

  return (
    <span className="inline-flex items-center rounded-full bg-gray-100 px-2.5 py-0.5 text-xs font-medium text-gray-800">
      {getReasonLabel(reasonCode)}
    </span>
  );
}

