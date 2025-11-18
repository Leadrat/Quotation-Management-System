"use client";

import { AdjustmentDto, AdjustmentStatus } from "@/types/refunds";

interface AdjustmentPreviewProps {
  adjustment: AdjustmentDto;
  onApprove?: () => void;
  onApply?: () => void;
}

export default function AdjustmentPreview({
  adjustment,
  onApprove,
  onApply,
}: AdjustmentPreviewProps) {
  const getAdjustmentTypeLabel = (type: string) => {
    return type.replace("_", " ").replace(/\b\w/g, (l) => l.toUpperCase());
  };

  return (
    <div className="rounded-lg border border-gray-200 bg-white p-6">
      <h3 className="mb-4 text-lg font-semibold">Adjustment Preview</h3>
      
      <dl className="space-y-3">
        <div>
          <dt className="text-sm font-medium text-gray-500">Type</dt>
          <dd className="mt-1 text-sm text-gray-900">
            {getAdjustmentTypeLabel(adjustment.adjustmentType)}
          </dd>
        </div>
        <div>
          <dt className="text-sm font-medium text-gray-500">Original Amount</dt>
          <dd className="mt-1 text-sm text-gray-900">
            ₹{adjustment.originalAmount.toLocaleString()}
          </dd>
        </div>
        <div>
          <dt className="text-sm font-medium text-gray-500">Adjusted Amount</dt>
          <dd className="mt-1 text-sm text-gray-900">
            ₹{adjustment.adjustedAmount.toLocaleString()}
          </dd>
        </div>
        <div>
          <dt className="text-sm font-medium text-gray-500">Difference</dt>
          <dd className={`mt-1 text-sm font-semibold ${
            adjustment.adjustmentDifference >= 0 ? "text-green-600" : "text-red-600"
          }`}>
            {adjustment.adjustmentDifference >= 0 ? "+" : ""}
            ₹{adjustment.adjustmentDifference.toLocaleString()}
          </dd>
        </div>
        <div>
          <dt className="text-sm font-medium text-gray-500">Reason</dt>
          <dd className="mt-1 text-sm text-gray-900">{adjustment.reason}</dd>
        </div>
        <div>
          <dt className="text-sm font-medium text-gray-500">Status</dt>
          <dd className="mt-1">
            <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${
              adjustment.status === AdjustmentStatus.APPROVED
                ? "bg-green-100 text-green-800"
                : adjustment.status === AdjustmentStatus.PENDING
                ? "bg-yellow-100 text-yellow-800"
                : adjustment.status === AdjustmentStatus.REJECTED
                ? "bg-red-100 text-red-800"
                : "bg-blue-100 text-blue-800"
            }`}>
              {adjustment.status}
            </span>
          </dd>
        </div>
      </dl>

      {adjustment.status === AdjustmentStatus.APPROVED && onApply && (
        <div className="mt-4">
          <button
            onClick={onApply}
            className="w-full rounded-md bg-green-600 px-4 py-2 text-white hover:bg-green-700"
          >
            Apply Adjustment
          </button>
        </div>
      )}
    </div>
  );
}

