"use client";

import { useState } from "react";
import { CreateRefundRequest, RefundReasonCode } from "@/types/refunds";
import { RefundsApi } from "@/lib/api";

interface RefundRequestFormProps {
  paymentId: string;
  quotationId?: string;
  maxRefundAmount: number;
  onSuccess: () => void;
  onCancel: () => void;
}

export default function RefundRequestForm({
  paymentId,
  quotationId,
  maxRefundAmount,
  onSuccess,
  onCancel,
}: RefundRequestFormProps) {
  const [formData, setFormData] = useState<CreateRefundRequest>({
    paymentId,
    quotationId,
    refundAmount: undefined,
    refundReason: "",
    refundReasonCode: RefundReasonCode.CLIENT_REQUEST,
    comments: "",
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);

    try {
      await RefundsApi.create(formData);
      onSuccess();
    } catch (err: any) {
      setError(err.message || "Failed to create refund request");
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      {error && (
        <div className="rounded-md bg-red-50 p-3 text-sm text-red-800">
          {error}
        </div>
      )}

      <div>
        <label className="block text-sm font-medium text-gray-700">
          Refund Amount
        </label>
        <div className="mt-1">
          <input
            type="number"
            step="0.01"
            min="0.01"
            max={maxRefundAmount}
            value={formData.refundAmount || ""}
            onChange={(e) =>
              setFormData({
                ...formData,
                refundAmount: e.target.value ? parseFloat(e.target.value) : undefined,
              })
            }
            className="w-full rounded-md border border-gray-300 px-3 py-2"
            placeholder={`Max: ₹${maxRefundAmount.toLocaleString()}`}
          />
          <p className="mt-1 text-xs text-gray-500">
            Leave empty for full refund. Maximum: ₹{maxRefundAmount.toLocaleString()}
          </p>
        </div>
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700">
          Refund Reason <span className="text-red-500">*</span>
        </label>
        <input
          type="text"
          value={formData.refundReason}
          onChange={(e) =>
            setFormData({ ...formData, refundReason: e.target.value })
          }
          className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2"
          required
          maxLength={500}
        />
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700">
          Reason Code <span className="text-red-500">*</span>
        </label>
        <select
          value={formData.refundReasonCode}
          onChange={(e) =>
            setFormData({
              ...formData,
              refundReasonCode: e.target.value as RefundReasonCode,
            })
          }
          className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2"
          required
        >
          <option value={RefundReasonCode.CLIENT_REQUEST}>Client Request</option>
          <option value={RefundReasonCode.ERROR}>Error</option>
          <option value={RefundReasonCode.DISCOUNT_ADJUSTMENT}>
            Discount Adjustment
          </option>
          <option value={RefundReasonCode.CANCELLATION}>Cancellation</option>
          <option value={RefundReasonCode.DUPLICATE_PAYMENT}>
            Duplicate Payment
          </option>
          <option value={RefundReasonCode.OTHER}>Other</option>
        </select>
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700">
          Comments (optional)
        </label>
        <textarea
          value={formData.comments || ""}
          onChange={(e) =>
            setFormData({ ...formData, comments: e.target.value })
          }
          className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2"
          rows={3}
          maxLength={1000}
        />
      </div>

      <div className="flex space-x-3">
        <button
          type="submit"
          disabled={loading}
          className="flex-1 rounded-md bg-blue-600 px-4 py-2 text-white hover:bg-blue-700 disabled:opacity-50"
        >
          {loading ? "Submitting..." : "Submit Refund Request"}
        </button>
        <button
          type="button"
          onClick={onCancel}
          className="rounded-md border border-gray-300 px-4 py-2 text-gray-700 hover:bg-gray-50"
        >
          Cancel
        </button>
      </div>
    </form>
  );
}

