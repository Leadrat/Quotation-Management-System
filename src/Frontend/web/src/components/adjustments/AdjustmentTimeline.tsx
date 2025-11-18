"use client";

import { AdjustmentDto } from "@/types/refunds";

interface AdjustmentTimelineProps {
  adjustments: AdjustmentDto[];
}

export default function AdjustmentTimeline({ adjustments }: AdjustmentTimelineProps) {
  const getStatusIcon = (status: string) => {
    switch (status) {
      case "PENDING":
        return "⏳";
      case "APPROVED":
        return "✅";
      case "REJECTED":
        return "❌";
      case "APPLIED":
        return "✓";
      default:
        return "•";
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString("en-IN", {
      year: "numeric",
      month: "short",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  return (
    <div className="space-y-4">
      {adjustments.map((adjustment, index) => (
        <div key={adjustment.adjustmentId} className="flex items-start space-x-4">
          <div className="flex-shrink-0">
            <div className="flex h-8 w-8 items-center justify-center rounded-full bg-gray-100">
              <span className="text-sm">{getStatusIcon(adjustment.status)}</span>
            </div>
            {index < adjustments.length - 1 && (
              <div className="mx-auto mt-2 h-8 w-0.5 bg-gray-200" />
            )}
          </div>
          <div className="flex-1 space-y-1">
            <div className="flex items-center justify-between">
              <p className="text-sm font-medium text-gray-900">
                {adjustment.adjustmentType.replace("_", " ")}
              </p>
              <p className="text-xs text-gray-500">
                {formatDate(adjustment.requestDate)}
              </p>
            </div>
            <p className="text-sm text-gray-600">
              ₹{adjustment.originalAmount.toLocaleString()} → ₹
              {adjustment.adjustedAmount.toLocaleString()} (
              {adjustment.adjustmentDifference > 0 ? "+" : ""}
              ₹{adjustment.adjustmentDifference.toLocaleString()})
            </p>
            <p className="text-sm text-gray-500">{adjustment.reason}</p>
            <p className="text-xs text-gray-400">
              Status: {adjustment.status} | Requested by: {adjustment.requestedByUserName}
            </p>
          </div>
        </div>
      ))}
    </div>
  );
}

